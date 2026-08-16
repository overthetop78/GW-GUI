using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari;

public interface IAtariMachine : IEmulatedMachine
{
    AtariMachineConfiguration Configuration { get; }
    Exception? Fault { get; }
    VideoFrame? LatestVideoFrame { get; }
    AudioChunk? LatestAudioChunk { get; }
    IReadOnlyList<AtariCoreOption> AvailableOptions { get; }
    IReadOnlyList<string> Diagnostics { get; }
    IReadOnlyDictionary<int, bool> LedStates { get; }
    string CoreName { get; }
    string CoreVersion { get; }
    IReadOnlySet<string> SupportedContentExtensions { get; }
    bool IsAudioMuted { get; }
    float AudioVolume { get; }
    event EventHandler<VideoFrame>? VideoFrameReady;
    event EventHandler<AudioChunk>? AudioChunkReady;
    void SetInput(EmulationInputSnapshot snapshot);
    void SetAudioMuted(bool muted);
    void SetAudioVolume(float volume);
    void SetAudioOutputFactory(Func<IAudioOutput?>? factory);
    ValueTask InsertMediaAsync(AtariMediaConfiguration media, CancellationToken cancellationToken = default);
    ValueTask EjectMediaAsync(EmulationMediaSlot slot, CancellationToken cancellationToken = default);
    ValueTask SelectDiskAsync(int index, CancellationToken cancellationToken = default);
    ValueTask SaveStateAsync(string path, CancellationToken cancellationToken = default);
    ValueTask LoadStateAsync(string path, CancellationToken cancellationToken = default);
    ValueTask SetOptionAsync(string key, string value, CancellationToken cancellationToken = default);
}
