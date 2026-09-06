using GWGUI.Tests.Application.TestInfrastructure;
namespace GWGUI.Tests.Interface.MaintenanceViews;
[Collection("WPF")]
public sealed class MaintenanceViewsTests(StaExecutionScenarios sta)
{
    [Fact] public Task SelectionKeepsToolValues() => sta.Run(ToolSelectionScenarios.Selection);
    [Theory]
    [InlineData(0, "0")] [InlineData(0, "invalid")]
    [InlineData(1, "0")] [InlineData(1, "2147483648")]
    [InlineData(2, "-1")] [InlineData(2, "invalid")]
    [InlineData(3, "-1")] [InlineData(3, "")]
    public Task InvalidEnabledOptionBlocksLaunchAndCanBeCorrected(int field, string value) => sta.RunAsync(() => ToolSelectionScenarios.Invalid(field, value));
    [Fact] public Task EditedValuesReachCommands() => sta.Run(ToolOperationScenarios.Commands);
    [Theory] [InlineData(false, 0)] [InlineData(false, 5)] [InlineData(false, -1)] [InlineData(true, 0)] [InlineData(true, 5)] [InlineData(true, -1)]
    public Task OutcomeRestoresControls(bool clean, int exit) => sta.RunAsync(() => ToolOperationScenarios.Outcome(clean, exit));
    [Theory] [InlineData(false)] [InlineData(true)] public Task StopRequiresAcceptedConfirmation(bool clean) => sta.RunAsync(() => ToolConfirmationScenarios.Stop(clean));
}
