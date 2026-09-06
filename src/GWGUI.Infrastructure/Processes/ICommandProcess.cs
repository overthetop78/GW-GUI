using System.Diagnostics;

namespace GWGUI.Infrastructure.Processes;

/// <summary>Process boundary used by the command runner.</summary>
public interface ICommandProcess : IDisposable
{
    event Action<string?>? StandardOutput;
    event Action<string?>? StandardError;
    bool HasExited { get; }
    int ExitCode { get; }
    void Start();
    void BeginRead();
    Task WaitForExitAsync(CancellationToken cancellationToken);
    void CloseMainWindow();
    void Kill();
}

internal sealed class CommandProcess : ICommandProcess
{
    private readonly Process process;
    public CommandProcess(ProcessStartInfo startInfo)
    {
        process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
        process.OutputDataReceived += (_, e) => StandardOutput?.Invoke(e.Data);
        process.ErrorDataReceived += (_, e) => StandardError?.Invoke(e.Data);
    }
    public event Action<string?>? StandardOutput;
    public event Action<string?>? StandardError;
    public bool HasExited => process.HasExited;
    public int ExitCode => process.ExitCode;
    public void Start() => process.Start();
    public void BeginRead() { process.BeginOutputReadLine(); process.BeginErrorReadLine(); }
    public Task WaitForExitAsync(CancellationToken cancellationToken) => process.WaitForExitAsync(cancellationToken);
    public void CloseMainWindow() => process.CloseMainWindow();
    public void Kill() => process.Kill(entireProcessTree: true);
    public void Dispose() => process.Dispose();
}
