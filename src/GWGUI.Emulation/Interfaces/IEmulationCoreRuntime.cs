using GWGUI.Emulation;

namespace GWGUI.Emulation.Interfaces;

internal interface IEmulationCoreRuntime<in TConfiguration, out TOption> : IDisposable
{
    VideoFrame? LatestVideoFrame { get; }
    AudioChunk? LatestAudioChunk { get; }
    bool TryDequeueAudio(out AudioChunk? chunk);
    IReadOnlyList<TOption> Options { get; }
    IReadOnlyList<string> Diagnostics { get; }
    IReadOnlyDictionary<int, bool> LedStates { get; }
    string CoreName { get; }
    string CoreVersion { get; }
    IReadOnlySet<string> SupportedContentExtensions { get; }
    string CoreSha256 { get; }
    double FramesPerSecond { get; }
    int SampleRate { get; }
    void Initialize(TConfiguration configuration, string sessionDirectory, string? saveDirectory = null);
    void RunFrame();
    void SetInput(EmulationInputSnapshot snapshot);
}
