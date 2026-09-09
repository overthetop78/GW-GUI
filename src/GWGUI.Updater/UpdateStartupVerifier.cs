using System.Diagnostics;
using GWGUI.Updates.Contracts;

namespace GWGUI.Updater;

internal sealed class UpdateStartupVerifier(UpdateExecutionPlan plan)
{
    private int _launchStarted;

    internal async Task<bool> LaunchAndWaitAsync()
    {
        if (Interlocked.Exchange(ref _launchStarted, 1) != 0)
            throw new InvalidOperationException("GW GUI can only be launched once by an update transaction.");
        if (File.Exists(plan.StartupSignalPath)) File.Delete(plan.StartupSignalPath);
        using var process = Program.Launch(plan, includeSignal: true);
        var deadline = DateTime.UtcNow.AddSeconds(plan.StartupTimeoutSeconds);
        while (DateTime.UtcNow < deadline)
        {
            if (File.Exists(plan.StartupSignalPath)) return true;
            if (process.HasExited) return false;
            await Task.Delay(100).ConfigureAwait(false);
        }
        try
        {
            if (!process.HasExited)
            {
                process.CloseMainWindow();
                using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                try { await process.WaitForExitAsync(cancellation.Token).ConfigureAwait(false); }
                catch (OperationCanceledException) { process.Kill(entireProcessTree: true); }
            }
        }
        catch { }
        return false;
    }
}
