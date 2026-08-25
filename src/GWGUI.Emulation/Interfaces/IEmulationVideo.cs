namespace GWGUI.Emulation.Interfaces;

public interface IEmulationVideo
{
    VideoFrame? LatestFrame { get; }
    double FramesPerSecond { get; }
    event EventHandler<VideoFrame>? FrameReady;
}
