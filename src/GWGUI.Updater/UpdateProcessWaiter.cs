using System.Diagnostics;

namespace GWGUI.Updater;

internal static class UpdateProcessWaiter
{
    internal static async Task<bool> WaitAsync(IReadOnlyList<int> processIds, TimeSpan timeout,
        int? updaterProcessId = null)
    {
        using var cancellation = new CancellationTokenSource(timeout);
        try
        {
            foreach (var processId in processIds.Distinct())
            {
                if (processId == (updaterProcessId ?? Environment.ProcessId)) continue;
                Process? process;
                try { process = Process.GetProcessById(processId); }
                catch (ArgumentException) { continue; }
                using (process) await process.WaitForExitAsync(cancellation.Token).ConfigureAwait(false);
            }
            return true;
        }
        catch (OperationCanceledException) { return false; }
    }
}
