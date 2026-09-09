using System.Diagnostics;
using GWGUI.Updates.Contracts;

namespace GWGUI.Updater;

internal sealed class UpdateStartupVerifier(UpdateExecutionPlan plan)
{
    internal async Task<bool> LaunchAndWaitAsync()
    {
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
