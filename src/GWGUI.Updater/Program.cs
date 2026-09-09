using System.Diagnostics;
using GWGUI.Updates.Contracts;

namespace GWGUI.Updater;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        UpdateExecutionPlan? plan = null;
        try
        {
            if (args is not ["--apply", var planPath]) return 2;
            plan = UpdatePlanReader.Read(planPath);
            if (!await UpdateProcessWaiter.WaitAsync(plan.ProcessIds,
                    TimeSpan.FromSeconds(plan.ShutdownTimeoutSeconds)))
            {
                UpdatePlanReader.WriteResult(plan, UpdateTransactionStatus.FailedBeforeReplacement,
                    "GW GUI processes did not close before the deadline.");
                return 3;
            }

            var transaction = new UpdateInstallationTransaction(plan, Path.GetDirectoryName(planPath)!);
            try { transaction.Apply(); }
            catch (Exception error)
            {
                UpdatePlanReader.WriteResult(plan, UpdateTransactionStatus.Restored, error.Message);
                Launch(plan, includeSignal: false);
                return 4;
            }

            var verifier = new UpdateStartupVerifier(plan);
            string? startupFailure = null;
            var started = false;
            try { started = await verifier.LaunchAndWaitAsync(); }
            catch (Exception error) { startupFailure = error.Message; }
            if (started)
            {
                transaction.Commit();
                UpdatePlanReader.WriteResult(plan, UpdateTransactionStatus.Succeeded);
                return 0;
            }

            transaction.Restore();
            UpdatePlanReader.WriteResult(plan, UpdateTransactionStatus.Restored,
                startupFailure ?? "The updated application did not report a successful startup.");
            Launch(plan, includeSignal: false);
            return 5;
        }
        catch (Exception error)
        {
            if (plan is not null)
            {
                try { UpdatePlanReader.WriteResult(plan, UpdateTransactionStatus.FailedBeforeReplacement, error.Message); }
                catch { }
            }
            return 1;
        }
    }

    internal static Process Launch(UpdateExecutionPlan plan, bool includeSignal)
    {
        var start = new ProcessStartInfo(plan.ApplicationExecutable) { UseShellExecute = false };
        if (includeSignal)
        {
            start.ArgumentList.Add("--update-startup-signal");
            start.ArgumentList.Add(plan.StartupSignalPath);
        }
        start.ArgumentList.Add("--update-result");
        start.ArgumentList.Add(plan.ResultPath);
        start.ArgumentList.Add("--update-work");
        start.ArgumentList.Add(Path.GetDirectoryName(plan.ResultPath)!);
        return Process.Start(start) ?? throw new InvalidOperationException("GW GUI could not be restarted.");
    }
}
