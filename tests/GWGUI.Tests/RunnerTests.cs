using GWGUI.Domain.Commands;
using GWGUI.Infrastructure.Processes;
using GWGUI.MediaEngine.Exploration.Scp;
using System.Diagnostics;
using System.IO;

namespace GWGUI.Tests;

public sealed class RunnerTests
{
    [Fact]
    public async Task CancellationTerminatesALongRunningProcessTree()
    {
        var powershell = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "WindowsPowerShell", "v1.0", "powershell.exe");
        var runner = new GreaseweazleRunner();
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(150));
        var watch = Stopwatch.StartNew();
        var result = await runner.RunAsync(new GwCommand(powershell, "-NoProfile", ["-Command", "Start-Sleep -Seconds 30"]), cancellationToken: cancellation.Token);
        Assert.True(result.WasCancelled);
        Assert.True(watch.Elapsed < TimeSpan.FromSeconds(6));
        Assert.False(runner.IsRunning);
    }
}
