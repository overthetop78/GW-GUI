namespace GWGUI.Tests.Hardware.PhysicalReading;

[Collection("WPF")]
public sealed class PhysicalReadingTests(GWGUI.Tests.Application.TestInfrastructure.StaExecutionScenarios sta)
{
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)] public Task InternalEngineRejectsMissingHardwareKnownFormatAndUnsupportedOptions(int variant)=>sta.RunAsync(()=>ReadPlanningScenarios.EngineValidation(variant));
    [Fact] public Task InvalidLaterCapturePreservesCompletedTrackProgressAndClosesDevice() => ReadFailureScenarios.PartialInvalidCapture();
    [Theory] [InlineData(false)] [InlineData(true)] public Task AcquisitionSelectsFakeOrHardSectorIndexMode(bool hard) => ReadAcquisitionScenarios.IndexModes(hard);
    [Fact] public Task TrackOrderAndFlux() => ReadAcquisitionScenarios.Acquire();
    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    [InlineData(4)] [InlineData(5)] [InlineData(6)] [InlineData(7)]
    public Task InvalidPlanDoesNotOpenDevice(int variant) => ReadPlanningScenarios.Invalid(variant);
    [Fact] public Task RetryRestoresTrackAndReportsAttempt() => ReadFailureScenarios.Retry();
    [Theory] [InlineData(false)] [InlineData(true)]
    public Task FailureAlwaysClosesDevice(bool cancel) => ReadFailureScenarios.Failure(cancel);
}
