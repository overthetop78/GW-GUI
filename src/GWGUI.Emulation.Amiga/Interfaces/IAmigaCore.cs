using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga.Interfaces;

internal interface IAmigaCore : IDisposable
{
    VideoFrame? LatestVideoFrame { get; }
    AudioChunk? LatestAudioChunk { get; }
    bool TryDequeueAudio(out AudioChunk? chunk);
    IReadOnlyList<AmigaCoreOption> Options { get; }
    IReadOnlyList<string> Diagnostics { get; }
    IReadOnlyDictionary<int, bool> LedStates { get; }
    string CoreName { get; }
    string CoreVersion { get; }
    IReadOnlySet<string> SupportedContentExtensions { get; }
    string CoreSha256 { get; }
    double FramesPerSecond { get; }
    int SampleRate { get; }
    int DiskCount { get; }
    int CurrentDiskIndex { get; }
    void Initialize(AmigaMachineConfiguration configuration, string sessionDirectory, string? saveDirectory = null);
    void RunFrame();
    void HardReset();
    void Stop();
    void SetInput(EmulationInputSnapshot snapshot);
    void InsertMedia(string path);
    void EjectMedia();
    void SelectDisk(int index);
    byte[] SaveState();
    void LoadState(ReadOnlySpan<byte> state);
    void SetOption(string key, string value);
}
