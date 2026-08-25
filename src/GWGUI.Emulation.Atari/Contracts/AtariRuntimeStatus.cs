using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Contracts;

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
    AtariHostProcessState HostProcessState,
    int? HostProcessId);
