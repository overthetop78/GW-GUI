using CommonCoreHostFunctions = GWGUI.Emulation.Panasonic.Common.Functions.CoreHostFunctions;
using System.Diagnostics;

namespace GWGUI.Emulation.Panasonic.Common.Machines.Common.Functions;
internal static class RuntimeFunctions
{
    internal static EmulationVideoStandard? ReadRegion(int value) => CommonCoreHostFunctions.Protocol.ReadRegion(value, allowAllDefinedStandards: true);
/// <summary>Publie les mesures du cœur et l'activité des lecteurs par la facette média de la session.</summary>
    /// <param name="configuration">Configuration complète empruntée, conservée sans modification.</param>
    /// <param name="core">Cœur emprunté dont les sorties et compteurs doivent être consultés.</param>
    /// <param name="mediaCapabilities">Facette média empruntée de la session ; null conserve la projection de repli existante.</param>
    /// <returns>Instantané public des mesures et des activités, sans transfert de ressource native.</returns>
    internal static RuntimeStatus Status(MachineConfiguration configuration, IEmulatorCore core, IEmulatorMediaCapabilities? mediaCapabilities = null)
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
            mediaCapabilities?.MediaActivity(configuration, core.LedStates) ?? new Dictionary<EmulationMediaSlot, bool>(),
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