using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga.Cores;

internal interface IAmigaCore : IDisposable
{
    VideoFrame? LatestVideoFrame { get; }
    AudioChunk? LatestAudioChunk { get; }
    double FramesPerSecond { get; }
    int SampleRate { get; }
    void Initialize(AmigaMachineConfiguration configuration, string sessionDirectory);
    void RunFrame();
    void HardReset();
    void Stop();
    void SetInput(EmulationInputSnapshot snapshot);
    void InsertFloppy(string path);
    void EjectFloppy();
}
