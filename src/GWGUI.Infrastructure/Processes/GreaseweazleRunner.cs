using GWGUI.Domain.Commands;
using GWGUI.Domain.Commands.Execution;
using System.Diagnostics;
using System.Text;

namespace GWGUI.Infrastructure.Processes;

public sealed class GreaseweazleRunner : IGreaseweazleRunner
{
    private readonly IOperationLogWriter? logWriter;
    private readonly Func<ProcessStartInfo, ICommandProcess> createProcess;
    private readonly Func<Task> waitForStop;
    public GreaseweazleRunner(IOperationLogWriter? logWriter = null)
        : this(info => new CommandProcess(info), () => Task.Delay(TimeSpan.FromSeconds(2), CancellationToken.None), logWriter) { }
    public GreaseweazleRunner(Func<ProcessStartInfo, ICommandProcess> createProcess, Func<Task> waitForStop, IOperationLogWriter? logWriter = null)
    {
        this.createProcess = createProcess ?? throw new ArgumentNullException(nameof(createProcess));
        this.waitForStop = waitForStop ?? throw new ArgumentNullException(nameof(waitForStop));
        this.logWriter = logWriter;
    }
    private int _running;
    public bool IsRunning => Volatile.Read(ref _running) != 0;

    public async Task<GwExecutionResult> RunAsync(GwCommand command, IProgress<GwOutputLine>? output = null, CancellationToken cancellationToken = default)
    {
        if (Interlocked.CompareExchange(ref _running, 1, 0) != 0)
            throw new InvalidOperationException("A Greaseweazle command is already running.");

        var lines = new List<GwOutputLine>();
        var gate = new object();
        var started = Stopwatch.StartNew();

        void Publish(GwOutputStream stream, string? text)
        {
            if (text is null) return;
            var line = new GwOutputLine(DateTimeOffset.Now, stream, text);
            lock (gate) lines.Add(line);
            output?.Report(line);
        }

        try
        {
            using var process = createProcess(CreateStartInfo(command));
            process.StandardOutput += text => Publish(GwOutputStream.Standard, text);
            process.StandardError += text => Publish(GwOutputStream.Error, text);
            process.Start();
            process.BeginRead();
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
                    if (await Task.WhenAny(exited, waitForStop()).ConfigureAwait(false) != exited && !process.HasExited)
                        process.Kill();
                }
                await process.WaitForExitAsync(CancellationToken.None).ConfigureAwait(false);
            }

            started.Stop();
            GwExecutionResult result;
            lock (gate) result = new GwExecutionResult(process.ExitCode, cancelled, started.Elapsed, lines.ToArray());
            if (logWriter is not null)
            {
                try { await logWriter.WriteAsync(command, result, CancellationToken.None).ConfigureAwait(false); }
                catch (Exception exception) when (exception is IOException or UnauthorizedAccessException) { }
            }
            return result;
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
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
        foreach (var argument in command.AllArguments()) info.ArgumentList.Add(argument);
        return info;
    }
}
