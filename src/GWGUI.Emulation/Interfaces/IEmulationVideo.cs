namespace GWGUI.Emulation;

public interface IEmulationVideo
{
    VideoFrame? LatestFrame { get; }
    double FramesPerSecond { get; }
    event EventHandler<VideoFrame>? FrameReady;
}
