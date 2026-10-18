using GWGUI.Emulation;

namespace GWGUI.Emulation.Acorn.Common.Interfaces;

internal interface IEmulatorCore : IDisposable, GWGUI.Emulation.Interfaces.IEmulationCoreCommands<MediaConfiguration, PeripheralCategory, DiskStatus>
{
    Emulator Emulator { get; }
    VideoFrame? LatestVideoFrame { get; }
    AudioChunk? LatestAudioChunk { get; }
    bool TryDequeueAudio(out AudioChunk? chunk);
    IReadOnlyList<CoreOption> Options { get; }
    IReadOnlyList<string> Diagnostics { get; }
    IReadOnlyDictionary<int, bool> LedStates { get; }
    string CoreName { get; }
    string CoreVersion { get; }
    string CoreSha256 { get; }
    IReadOnlySet<string> SupportedContentExtensions { get; }
    bool SupportsSaveStates { get; }
    double FramesPerSecond { get; }
    int SampleRate { get; }
    EmulationVideoStandard? Region { get; }
    int BufferedAudioFrames { get; }
    long AudioOverrunCount { get; }
    long AudioUnderrunCount { get; }
    HostProcessState HostProcessState { get; }
    int? HostProcessId { get; }
    void Initialize(MachineConfiguration configuration, string sessionDirectory, string? saveDirectory = null);
    void RunFrame();
    void SetInput(EmulationInputSnapshot snapshot);
    void SaveMediaChanges(EmulationMediaSlot slot);
    bool HasUnsavedMediaChanges(EmulationMediaSlot slot);
}

