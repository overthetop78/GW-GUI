using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga.Cores;

internal interface IAmigaCore : IDisposable
{
    VideoFrame? LatestVideoFrame { get; }
    AudioChunk? LatestAudioChunk { get; }
    bool TryDequeueAudio(out AudioChunk? chunk);
    IReadOnlyList<AmigaCoreOption> Options { get; }
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
    void InsertFloppy(string path);
    void EjectFloppy();
    void SelectDisk(int index);
    byte[] SaveState();
    void LoadState(ReadOnlySpan<byte> state);
    void SetOption(string key, string value);
}
