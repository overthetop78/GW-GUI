using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga;

public interface IAmigaMachine : IEmulatedMachine
{
    AmigaMachineConfiguration Configuration { get; }
    Exception? Fault { get; }
    VideoFrame? LatestVideoFrame { get; }
    AudioChunk? LatestAudioChunk { get; }
    IReadOnlyList<AmigaCoreOption> AvailableOptions { get; }
    event EventHandler<VideoFrame>? VideoFrameReady;
    event EventHandler<AudioChunk>? AudioChunkReady;
    void SetInput(EmulationInputSnapshot snapshot);
    ValueTask InsertFloppyAsync(string path, CancellationToken cancellationToken = default);
    ValueTask EjectFloppyAsync(CancellationToken cancellationToken = default);
    ValueTask SaveStateAsync(string path, CancellationToken cancellationToken = default);
    ValueTask LoadStateAsync(string path, CancellationToken cancellationToken = default);
    ValueTask SetOptionAsync(string key, string value, CancellationToken cancellationToken = default);
}
