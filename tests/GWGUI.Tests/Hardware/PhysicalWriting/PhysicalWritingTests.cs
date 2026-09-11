namespace GWGUI.Tests.Hardware.PhysicalWriting;
public sealed class PhysicalWritingTests
{
    [Fact] public Task DisconnectStopsWritingAndKeepsCompletedTrackCount() => WriteCancellationScenarios.Disconnected();
    [Fact] public Task OrderedTracksAndOptions() => WritePlanningScenarios.Order();
    [Theory] [InlineData(false)] [InlineData(true)]
    public Task InvalidOptionsHaveNoDeviceEffects(bool verify) => WritePlanningScenarios.Invalid(verify);
    [Fact]
    public Task UnsupportedVerificationHasNoDeviceEffects() => WriteVerificationScenarios.Unsupported();
    [Fact] public Task CancellationKeepsCompletedCountAndCloses() => WriteCancellationScenarios.Cancel();
    [Fact] public Task WriteProtectionIsReported() => WriteCancellationScenarios.Protected();
}
