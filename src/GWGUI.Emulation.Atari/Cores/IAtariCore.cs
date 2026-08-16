using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Cores;

internal interface IAtariCore : IDisposable
{
    AtariCoreKind Kind { get; }
    VideoFrame? LatestVideoFrame { get; }
    AudioChunk? LatestAudioChunk { get; }
    bool TryDequeueAudio(out AudioChunk? chunk);
    IReadOnlyList<AtariCoreOption> Options { get; }
    IReadOnlyList<string> Diagnostics { get; }
    IReadOnlyDictionary<int, bool> LedStates { get; }
    string CoreName { get; }
    string CoreVersion { get; }
    string CoreSha256 { get; }
    IReadOnlySet<string> SupportedContentExtensions { get; }
    double FramesPerSecond { get; }
    int SampleRate { get; }
    AtariRuntimeRegion? Region { get; }
    int BufferedAudioFrames { get; }
    long AudioOverrunCount { get; }
    long AudioUnderrunCount { get; }
    AtariHostProcessState HostProcessState { get; }
    int? HostProcessId { get; }
    void Initialize(AtariMachineConfiguration configuration, string sessionDirectory, string? saveDirectory = null);
    void RunFrame();
    void HardReset();
    void Stop();
    void SetInput(EmulationInputSnapshot snapshot);
    void InsertMedia(AtariMediaConfiguration media);
    void EjectMedia(EmulationMediaSlot slot);
    void SelectDisk(int index);
    void SaveMediaChanges(EmulationMediaSlot slot);
    AtariDiskStatus GetDiskStatus();
    bool HasUnsavedMediaChanges(EmulationMediaSlot slot);
    byte[] SaveState();
    void LoadState(ReadOnlySpan<byte> state);
    void SetOption(string key, string value);
}
