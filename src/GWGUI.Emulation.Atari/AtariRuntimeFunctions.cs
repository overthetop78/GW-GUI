using System.Diagnostics;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari.Cores;

namespace GWGUI.Emulation.Atari;

internal static class AtariRuntimeFunctions
{
    internal static AtariRuntimeRegion? Region(uint nativeRegion) => nativeRegion switch
    {
        AtariRuntimeConstants.NativeNtscRegion => AtariRuntimeRegion.Ntsc,
        AtariRuntimeConstants.NativePalRegion => AtariRuntimeRegion.Pal,
        _ => null
    };

    internal static int RegionValue(AtariRuntimeRegion? region) =>
        region is null ? AtariRuntimeConstants.MissingRegionValue : (int)region.Value;

    internal static AtariRuntimeRegion? ReadRegion(int value) =>
        value == AtariRuntimeConstants.MissingRegionValue || !Enum.IsDefined(typeof(AtariRuntimeRegion), value)
            ? null
            : (AtariRuntimeRegion)value;

    internal static AtariHostProcessState ProcessState(Process? process, bool connectionFailed, bool disposed)
    {
        if (connectionFailed) return AtariHostProcessState.Faulted;
        if (process is null) return AtariHostProcessState.NotStarted;
        if (disposed) return AtariHostProcessState.Exited;
        try { return process.HasExited ? AtariHostProcessState.Exited : AtariHostProcessState.Running; }
        catch (InvalidOperationException) { return AtariHostProcessState.Exited; }
    }

    internal static AtariRuntimeStatus Status(AtariMachineConfiguration configuration, IAtariCore core)
    {
        var frame = core.LatestVideoFrame;
        var geometry = frame is null
            ? null
            : new AtariRuntimeGeometry(frame.Width, frame.Height, frame.Pitch, frame.AspectRatio);
        return new AtariRuntimeStatus(configuration.Model, core.Region, core.FramesPerSecond, core.SampleRate,
            geometry, core.CoreName, new Dictionary<EmulationMediaSlot, bool>(),
            new Dictionary<int, bool>(core.LedStates), core.BufferedAudioFrames, core.AudioOverrunCount,
            core.AudioUnderrunCount, core.HostProcessState, core.HostProcessId);
    }
}
