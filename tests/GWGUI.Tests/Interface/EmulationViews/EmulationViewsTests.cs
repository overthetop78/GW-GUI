namespace GWGUI.Tests.Interface.EmulationViews;
[Collection("WPF")]
public class EmulationViewsTests(GWGUI.Tests.Application.TestInfrastructure.StaExecutionScenarios sta)
{
    [Fact] public Task DraftReloadDiscardAndSavedConfigurationAreIsolated() => sta.RunAsync(MachineConfigurationScenarios.DraftLifecycle);
    [Fact] public Task TabsSelectAndCloseOnlyTargetSession() => sta.RunAsync(MachineTabsScenarios.Tabs);
    [Theory] [InlineData(false)] [InlineData(true)]
    public Task ConfigurationDraftSaveAndRetry(bool failure) => sta.RunAsync(() => MachineConfigurationScenarios.ConfigurationEditing(failure));
    [Theory] [InlineData(false)] [InlineData(true)]
    public Task CommandsRespectPowerStateAndReportErrors(bool failure) => sta.Run(() => EmulationInteractionScenarios.Commands(failure));
}
