using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga;

public interface IAmigaMachine : IEmulatedMachine
{
    AmigaMachineConfiguration Configuration { get; }
    VideoFrame? LatestVideoFrame { get; }
    AudioChunk? LatestAudioChunk { get; }
    event EventHandler<VideoFrame>? VideoFrameReady;
    event EventHandler<AudioChunk>? AudioChunkReady;
    void SetInput(EmulationInputSnapshot snapshot);
    ValueTask InsertFloppyAsync(string path, CancellationToken cancellationToken = default);
    ValueTask EjectFloppyAsync(CancellationToken cancellationToken = default);
}
