using GWGUI.Emulation.Interop;

namespace GWGUI.Emulation.Interfaces;

internal interface IEmulationCoreCallbacks
{
    ExternalCoreApi.EnvironmentCallback Environment { get; }
    ExternalCoreApi.VideoCallback Video { get; }
    ExternalCoreApi.AudioSampleCallback AudioSample { get; }
    ExternalCoreApi.AudioBatchCallback AudioBatch { get; }
    ExternalCoreApi.InputPollCallback InputPoll { get; }
    ExternalCoreApi.InputStateCallback InputState { get; }
}
