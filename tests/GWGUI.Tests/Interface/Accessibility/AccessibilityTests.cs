using GWGUI.Tests.Application.TestInfrastructure;
namespace GWGUI.Tests.Interface.Accessibility;
[Collection("WPF")]
public sealed class AccessibilityTests(StaExecutionScenarios sta)
{
    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)] [InlineData(6)]
    public Task DeclaredControlsExposeNamesAndRoles(int tab) => sta.Run(() => AccessibleControlsScenarios.Controls(tab));
    [Theory]
    [InlineData(0, false)] [InlineData(0, true)]
    [InlineData(1, false)] [InlineData(1, true)]
    [InlineData(2, false)] [InlineData(2, true)]
    public Task CommandContractsSurviveOperationStates(int kind, bool failure) => sta.RunAsync(() => AccessibleControlsScenarios.OperationStates(kind, failure));
}
