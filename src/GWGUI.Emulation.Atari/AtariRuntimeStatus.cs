using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari;

public enum AtariRuntimeRegion { Ntsc, Pal }

public enum AtariHostProcessState { InProcess, NotStarted, Running, Exited, Faulted }

public sealed record AtariRuntimeGeometry(int Width, int Height, int Pitch, float AspectRatio);

public sealed record AtariRuntimeStatus(
    AtariMachineModel Model,
    AtariRuntimeRegion? Region,
    double FramesPerSecond,
    int SampleRate,
    AtariRuntimeGeometry? Geometry,
    string CoreName,
    IReadOnlyDictionary<EmulationMediaSlot, bool> MediaActivity,
    IReadOnlyDictionary<int, bool> LedStates,
    int BufferedAudioFrames,
    long AudioOverrunCount,
    long AudioUnderrunCount,
    Exception? LastError,
    AtariHostProcessState HostProcessState,
    int? HostProcessId);
