namespace GWGUI.Emulation.Microsoft.Common.Interfaces;

internal interface IEmulatorCore : IDisposable, GWGUI.Emulation.Interfaces.IEmulationCoreStateCommands
{
    VideoFrame? LatestVideoFrame { get; }
    AudioChunk? LatestAudioChunk { get; }
    bool TryDequeueAudio(out AudioChunk? chunk);
    IReadOnlyList<CoreOption> Options { get; }
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
    void Initialize(MachineConfiguration configuration, string sessionDirectory, string? saveDirectory = null);
    void RunFrame();
    void SoftReset();
    void SetInput(EmulationInputSnapshot snapshot);
    void InsertMedia(string path);
    void EjectMedia();
}
