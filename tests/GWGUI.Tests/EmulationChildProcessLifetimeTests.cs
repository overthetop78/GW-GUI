using GWGUI.Emulation.Functions;
using System.Diagnostics;
using System.IO;

namespace GWGUI.Tests;

public sealed class EmulationChildProcessLifetimeTests
{
    [Fact]
    public void ClosingJobTerminatesAttachedChildProcess()
    {
        using var child = Process.Start(new ProcessStartInfo(
            Path.Combine(Environment.SystemDirectory, "WindowsPowerShell", "v1.0", "powershell.exe"))
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            ArgumentList = { "-NoProfile", "-Command", "Start-Sleep -Seconds 60" }
        }) ?? throw new InvalidOperationException("The child test process could not be started.");

        using (var job = new EmulationChildProcessJob()) job.Attach(child);

        Assert.True(child.WaitForExit(TimeSpan.FromSeconds(5)));
    }
}
