namespace GWGUI.Tests.Emulation.Audio;
public class AudioTests
{
    [Fact] public void VolumeMutePauseAndSampleRateChangesUseOnlySimulatedOutput() => AudioBufferScenarios.ControlledAudio();
    [Theory] [InlineData(false)] [InlineData(true)] public void FailedOutputIsDisposedAndCanBeReplaced(bool available) => AudioBufferScenarios.OutputRecovery(available);
    [Fact] public void SamplesReachInjectedOutputAndFlushProducesSilence()=>AudioBufferScenarios.Samples();
    [Fact] public void LifecycleRejectsInvalidUseAndDisposesOutput()=>AudioBufferScenarios.Lifecycle();
}
