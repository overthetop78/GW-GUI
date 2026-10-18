using System.Diagnostics;

namespace GWGUI.Emulation.Watara.Common.Machines.Common.Functions;
internal static class RuntimeFunctions
{
    internal static int RegionValue(EmulationVideoStandard? region) => EmulationHostProtocolFunctions.RegionValue(region);
    internal static EmulationVideoStandard? ReadRegion(int value) => EmulationHostProtocolFunctions.ReadRegion(value, allowAllDefinedStandards: true);
    internal static HostProcessState ProcessState(Process? process, bool connectionFailed, bool disposed)
        =>
            EmulationHostProtocolFunctions.GetProcessState(process, connectionFailed, disposed,
                HostProcessState.Faulted, HostProcessState.NotStarted, HostProcessState.Exited, HostProcessState.Running);

    internal static RuntimeStatus Status(MachineConfiguration configuration, IEmulatorCore core)
    {
        var frame = core.LatestVideoFrame;
        var geometry = frame is null ? null : new RuntimeGeometry(frame.Width, frame.Height, frame.Pitch, frame.AspectRatio);
        return new RuntimeStatus(
            configuration.Model,
            core.Region,
            core.FramesPerSecond,
            core.SampleRate,
            geometry,
            core.CoreName,
            new Dictionary<EmulationMediaSlot, bool>(),
            new Dictionary<int, bool>(core.LedStates),
            core.BufferedAudioFrames,
            core.AudioOverrunCount,
            core.AudioUnderrunCount,
            core.HostProcessState,
            core.HostProcessId
        );
    }
}

internal static class MessageFunctions
{
    internal static Exception Translate(Exception error, MachineConfiguration configuration) => EmulationErrorService.Translate(error, EmulationMessageCategory.Machine, EmulationMessageCode.MachineStartFailed, new EmulationMachineMessageContext(configuration.MachineId));
}