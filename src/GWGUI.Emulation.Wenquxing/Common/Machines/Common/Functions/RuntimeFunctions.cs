using CommonCoreHostFunctions = GWGUI.Emulation.Wenquxing.Common.Functions.CoreHostFunctions;
using System.Diagnostics;

namespace GWGUI.Emulation.Wenquxing.Common.Machines.Common.Functions;
internal static class RuntimeFunctions
{
    internal static EmulationVideoStandard? ReadRegion(int value) => CommonCoreHostFunctions.Protocol.ReadRegion(value, allowAllDefinedStandards: true);
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
    internal static Exception Translate(Exception error, MachineConfiguration configuration) => CommonCoreHostFunctions.Messages.Translate(error, EmulationMessageCategory.Machine, EmulationMessageCode.MachineStartFailed, new EmulationMachineMessageContext(configuration.MachineId));
}