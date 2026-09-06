namespace GWGUI.Tests.Interface.DeviceSettingsViews;
[Collection("WPF")]
public class DeviceSettingsViewsTests(GWGUI.Tests.Application.TestInfrastructure.StaExecutionScenarios sta)
{
    [Fact] public Task DiscoveryPreservesSelectionAndHandlesRemoval() => sta.RunAsync(DeviceListScenarios.Selection);
    [Fact] public Task DetectionFailureAllowsRetry() => sta.RunAsync(DeviceListScenarios.Failure);
    [Fact] public Task InputRowsAndProfilesFollowSelectedDevice() => sta.RunAsync(DeviceInputPreviewScenarios.Values);
    [Fact] public Task InputFailuresAreDeduplicatedAndRecover() => sta.RunAsync(DeviceInputPreviewScenarios.Failure);
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public Task RumbleTargetsOneMotorThenStops(int bit) => sta.RunAsync(() => DeviceFeedbackScenarios.Pulse(bit));
    [Theory] [InlineData(false)] [InlineData(true)]
    public Task RumbleUnavailableOrFailedRestoresControls(bool throws) => sta.RunAsync(() => DeviceFeedbackScenarios.Failure(throws));
}
