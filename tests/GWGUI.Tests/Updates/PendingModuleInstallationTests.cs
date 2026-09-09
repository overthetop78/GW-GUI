namespace GWGUI.Tests.Updates;

public sealed class PendingModuleInstallationTests
{
    [Fact]
    public void Store_keeps_multiple_modules_and_rejects_duplicate_identifiers() =>
        PendingModuleInstallationScenarios.StoreKeepsDistinctModulesAndRejectsDuplicate();

    [Fact]
    public Task Finish_prepares_one_launch_plan_for_every_downloaded_module() =>
        PendingModuleInstallationScenarios.OneLaunchPlanContainsEveryPendingModule();
}
