using GWGUI.Domain.Commands;
using GWGUI.Domain.Commands.Execution;
using GWGUI.Infrastructure.Processes;
using System.Diagnostics;

namespace GWGUI.Tests.Application.ExternalProcesses;
internal static class ProcessBoundaryScenarios
{
    public static async Task LoggingFailure()
    {
        var files = new GWGUI.Tests.Application.TestInfrastructure.MemoryLogFiles { Fail = true };
        var process = new Process { ExitCode = 7 }; process.Exit.SetResult();
        var runner = new GreaseweazleRunner(_ => process,() => Task.CompletedTask,new RotatingOperationLogWriter("virtual-log",fileSystem:files));
        var result = await runner.RunAsync(Command);
        Assert.Equal(7,result.ExitCode); Assert.False(result.IsSuccess); Assert.Equal(3,result.Output.Count);
        Assert.True(process.Disposed); Assert.False(runner.IsRunning); Assert.NotEmpty(files.Calls);
    }
    public static async Task FactoryFailure()
    {
        var fail = true; var error = new IOException("synthetic creation failure");
        var process = new Process(); process.Exit.SetResult();
        var runner = new GreaseweazleRunner(_ => fail ? throw error : process,() => Task.CompletedTask);
        Assert.Same(error,await Assert.ThrowsAsync<IOException>(() => runner.RunAsync(Command)));
        Assert.False(runner.IsRunning); fail = false; Assert.True((await runner.RunAsync(Command)).IsSuccess);
    }
    private static GwCommand Command => new("virtual tool", "read", ["--tracks", "c=0:h=1", "virtual folder/disk.scp"]);
    private static GreaseweazleRunner Runner(Process process) => new(info => {
        Assert.Equal("virtual tool", info.FileName);
        Assert.Equal(new[] { "read", "--tracks", "c=0:h=1", "virtual folder/disk.scp" }, info.ArgumentList);
        Assert.False(info.UseShellExecute); Assert.True(info.CreateNoWindow);
        Assert.True(info.RedirectStandardOutput); Assert.True(info.RedirectStandardError);
        return process;
    }, () => Task.CompletedTask);
    public static async Task Completion(int code)
    {
        var process = new Process { ExitCode = code }; process.Exit.TrySetResult();
        var runner = Runner(process);
        var result = await runner.RunAsync(Command);
        Assert.Equal(code, result.ExitCode); Assert.Equal(code == 0, result.IsSuccess);
        Assert.Equal(new[] { "first", "warning", "last" }, result.Output.Select(line => line.Text));
        Assert.Equal(new[] { GwOutputStream.Standard, GwOutputStream.Error, GwOutputStream.Standard }, result.Output.Select(line => line.Stream));
        Assert.True(process.Disposed); Assert.False(runner.IsRunning);
    }
    public static async Task Cancellation()
    {
        var process = new Process(); var runner = Runner(process);
        using var source = new CancellationTokenSource();
        var running = runner.RunAsync(Command, cancellationToken: source.Token);
        Assert.True(runner.IsRunning);
        await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync(Command));
        source.Cancel();
        var result = await running;
        Assert.True(result.WasCancelled); Assert.False(result.IsSuccess);
        Assert.Equal(1, process.CloseRequests); Assert.Equal(1, process.Kills);
        Assert.True(process.Disposed); Assert.False(runner.IsRunning);
    }
    public static async Task StartFailure()
    {
        var error = new InvalidOperationException("synthetic start failure");
        var process = new Process { StartError = error }; var runner = Runner(process);
        Assert.Same(error, await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync(Command)));
        Assert.False(runner.IsRunning); Assert.True(process.Disposed);
        process.StartError = null; process.Exit.TrySetResult();
        Assert.True((await runner.RunAsync(Command)).IsSuccess);
    }
    private sealed class Process : ICommandProcess
    {
        public TaskCompletionSource Exit { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public event Action<string?>? StandardOutput;
        public event Action<string?>? StandardError;
        public Exception? StartError { get; set; }
        public bool HasExited => Exit.Task.IsCompleted;
        public int ExitCode { get; init; }
        public int CloseRequests { get; private set; }
        public int Kills { get; private set; }
        public bool Disposed { get; private set; }
        public void Start() { if (StartError is not null) throw StartError; }
        public void BeginRead() { StandardOutput?.Invoke("first"); StandardError?.Invoke("warning"); StandardOutput?.Invoke("last"); StandardOutput?.Invoke(null); StandardError?.Invoke(null); }
        public Task WaitForExitAsync(CancellationToken cancellationToken) => Exit.Task.WaitAsync(cancellationToken);
        public void CloseMainWindow() => CloseRequests++;
        public void Kill() { Kills++; Exit.TrySetResult(); }
        public void Dispose() => Disposed = true;
    }
}
