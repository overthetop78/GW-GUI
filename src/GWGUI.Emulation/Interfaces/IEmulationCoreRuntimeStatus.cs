namespace GWGUI.Emulation.Interfaces;

internal interface IEmulationCoreRuntimeStatus
{
    bool SupportsSaveStates { get; }
    EmulationVideoStandard? Region { get; }
    int BufferedAudioFrames { get; }
    long AudioOverrunCount { get; }
    long AudioUnderrunCount { get; }
}
