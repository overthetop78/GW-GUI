using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Interfaces;

internal interface IEmulatorCore : IDisposable
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
    RuntimeRegion? Region { get; }
    int BufferedAudioFrames { get; }
    long AudioOverrunCount { get; }
    long AudioUnderrunCount { get; }
    HostProcessState HostProcessState { get; }
    int? HostProcessId { get; }
    void Initialize(MachineConfiguration configuration, string sessionDirectory, string? saveDirectory = null);
    void RunFrame();
    void HardReset();
    void Stop();
    void SetInput(EmulationInputSnapshot snapshot);
    void SetControllerPortDevice(int port, PeripheralCategory peripheral);
    void InsertMedia(MediaConfiguration media);
    void EjectMedia(EmulationMediaSlot slot);
    void SelectDisk(int index);
    void SaveMediaChanges(EmulationMediaSlot slot);
    DiskStatus GetDiskStatus();
    bool HasUnsavedMediaChanges(EmulationMediaSlot slot);
    byte[] SaveState();
    void LoadState(ReadOnlySpan<byte> state);
    void SetOption(string key, string value);
}
