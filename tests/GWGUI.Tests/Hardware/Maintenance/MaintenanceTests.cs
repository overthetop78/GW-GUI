using GWGUI.Tests.Application.TestInfrastructure;
namespace GWGUI.Tests.Hardware.Maintenance;
[Collection("WPF")]
public class MaintenanceTests(StaExecutionScenarios sta)
{
    [Theory] [InlineData("select")] [InlineData("step")] [InlineData("settle")] [InlineData("motor")] [InlineData("watchdog")] [InlineData("pre-write")] [InlineData("post-write")] [InlineData("index-mask")]
    public void DelayOptionsValidateOnlyEnabledFields(string key) => MaintenanceRequestScenarios.Delay(key);
    [Fact] public void AlignmentOptionsPreserveTracksAndValidateReadCount() => MaintenanceRequestScenarios.Alignment();
    [Theory]
    [InlineData("info", new[] { "--device", "controller" })]
    [InlineData("bandwidth", new[] { "--device", "controller" })]
    [InlineData("reset", new[] { "--device", "controller" })]
    [InlineData("rpm", new[] { "--nr", "1", "--device", "controller", "--drive", "B" })]
    [InlineData("seek", new[] { "0", "--device", "controller", "--drive", "B" })]
    [InlineData("pin", new[] { "get", "26", "--device", "controller" })]
    [InlineData("delays", new[] { "--device", "controller" })]
    [InlineData("align", new[] { "--tracks", "c=40:h=0-1", "--revs", "3", "--reads", "10", "--device", "controller", "--drive", "B" })]
    public Task DefaultToolRequests(string verb, string[] expected) => sta.Run(() => MaintenanceRequestScenarios.Defaults(verb, expected));
    [Theory] [InlineData("rpm")] [InlineData("seek")] [InlineData("pin")] public Task InvalidValuesDisableExecution(string verb) => sta.Run(() => MaintenanceRequestScenarios.Invalid(verb));
    [Theory] [InlineData(false)] [InlineData(true)] public Task ControllerInformationAndEmptyResponse(bool empty) => sta.Run(() => DiagnosticResultScenarios.Info(empty));
    [Theory] [InlineData(0)] [InlineData(7)] [InlineData(-1)] public Task DiagnosticOutcomeAndDisconnection(int exit) => sta.Run(() => DiagnosticResultScenarios.Result(exit));
}
