using System.Diagnostics;
using GWGUI.Domain.Commands;

namespace GWGUI.Infrastructure.Processes;

public sealed class GreaseweazleRunner : IGreaseweazleRunner
{
    private int _running;
    public bool IsRunning => Volatile.Read(ref _running) != 0;

    public async Task<GwExecutionResult> RunAsync(GwCommand command, IProgress<GwOutputLine>? output = null, CancellationToken cancellationToken = default)
    {
        if (Interlocked.CompareExchange(ref _running, 1, 0) != 0)
            throw new InvalidOperationException("A Greaseweazle command is already running.");

        var lines = new List<GwOutputLine>();
        var gate = new object();
        var started = Stopwatch.StartNew();
        using var process = new Process { StartInfo = CreateStartInfo(command), EnableRaisingEvents = true };

        void Publish(GwOutputStream stream, string? text)
        {
            if (text is null) return;
            var line = new GwOutputLine(DateTimeOffset.Now, stream, text);
            lock (gate) lines.Add(line);
            output?.Report(line);
        }

        process.OutputDataReceived += (_, e) => Publish(GwOutputStream.Standard, e.Data);
        process.ErrorDataReceived += (_, e) => Publish(GwOutputStream.Error, e.Data);

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            var cancelled = false;
            try
            {
                await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                cancelled = true;
                if (!process.HasExited)
                {
                    // Some packaged host-tool builds can react to a normal close request.
                    // Give them a short grace period before terminating the process tree.
                    try { process.CloseMainWindow(); } catch (InvalidOperationException) { }
                    var exited = process.WaitForExitAsync(CancellationToken.None);
                    if (await Task.WhenAny(exited, Task.Delay(TimeSpan.FromSeconds(2), CancellationToken.None)).ConfigureAwait(false) != exited && !process.HasExited)
                        process.Kill(entireProcessTree: true);
                }
                await process.WaitForExitAsync(CancellationToken.None).ConfigureAwait(false);
            }

            started.Stop();
            lock (gate) return new GwExecutionResult(process.ExitCode, cancelled, started.Elapsed, lines.ToArray());
        }
        finally
        {
            Volatile.Write(ref _running, 0);
        }
    }

    private static ProcessStartInfo CreateStartInfo(GwCommand command)
    {
        var info = new ProcessStartInfo(command.ExecutablePath)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        foreach (var argument in command.AllArguments()) info.ArgumentList.Add(argument);
        return info;
    }
}
