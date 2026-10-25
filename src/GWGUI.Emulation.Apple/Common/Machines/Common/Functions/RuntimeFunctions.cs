using CommonCoreHostFunctions = GWGUI.Emulation.Apple.Common.Functions.CoreHostFunctions;
using System.Diagnostics;

namespace GWGUI.Emulation.Apple.Common.Machines.Common.Functions;

internal static class RuntimeFunctions
{

    internal static EmulationVideoStandard? ReadRegion(int value) => CommonCoreHostFunctions.Protocol.ReadRegion(value, allowAllDefinedStandards: false);

/// <summary>Publie les mesures du cœur et l'activité des lecteurs par la facette média de la session.</summary>
    /// <param name="configuration">Configuration complète empruntée, conservée sans modification.</param>
    /// <param name="core">Cœur emprunté dont les sorties et compteurs doivent être consultés.</param>
    /// <param name="mediaCapabilities">Facette média empruntée de la session ; null conserve la projection de repli existante.</param>
    /// <returns>Instantané public des mesures et des activités, sans transfert de ressource native.</returns>
    internal static RuntimeStatus Status(MachineConfiguration configuration, IEmulatorCore core, IEmulatorMediaCapabilities? mediaCapabilities = null)
    {
        var frame = core.LatestVideoFrame;
        var geometry = frame is null ? null : new RuntimeGeometry(frame.Width, frame.Height, frame.Pitch, frame.AspectRatio);
        return new RuntimeStatus(configuration.Model, core.Region, core.FramesPerSecond, core.SampleRate,
            geometry, core.CoreName, mediaCapabilities?.MediaActivity(configuration, core.LedStates) ?? new Dictionary<EmulationMediaSlot, bool>(),
            new Dictionary<int, bool>(core.LedStates), core.BufferedAudioFrames, core.AudioOverrunCount,
            core.AudioUnderrunCount, core.HostProcessState, core.HostProcessId);
    }
}

internal static class MessageFunctions
{
    internal static Exception Translate(Exception error, MachineConfiguration configuration)
    {
        var context = new EmulationMachineMessageContext(configuration.MachineId);
        if (error is not EmulationException coreError)
            return CommonCoreHostFunctions.Messages.Translate(error, EmulationMessageCategory.Machine, EmulationMessageCode.MachineStartFailed, context);
        if (coreError.IsLocalized)
            return CommonCoreHostFunctions.Messages.TranslateLocalized(error, Category(coreError.Category), context);
        if (coreError.Code == ErrorCode.ContentRequired)
            return CommonCoreHostFunctions.Messages.RequiredMediaMissing(configuration.MachineId, [EmulationMediaCategory.FloppyDrive], error);
        return CommonCoreHostFunctions.Messages.Translate(error, Category(coreError.Category), MessageCode(coreError.Code), context);
    }

    private static EmulationMessageCategory Category(ErrorCategory category) => category switch
    {
        ErrorCategory.Core or ErrorCategory.Host => EmulationMessageCategory.Emulator,
        ErrorCategory.Firmware => EmulationMessageCategory.Firmware,
        ErrorCategory.Content => EmulationMessageCategory.Media,
        ErrorCategory.State => EmulationMessageCategory.SavedState,
        _ => EmulationMessageCategory.Machine
    };

    private static EmulationMessageCode MessageCode(ErrorCode code) => code switch
    {
        ErrorCode.CoreNotFound => EmulationMessageCode.EmulatorNotInstalled,
        ErrorCode.CoreRejected => EmulationMessageCode.EmulatorRejected,
        ErrorCode.FirmwareMissing => EmulationMessageCode.FirmwareMissing,
        ErrorCode.FirmwareInvalid => EmulationMessageCode.FirmwareIncompatible,
        ErrorCode.ContentNotFound => EmulationMessageCode.MediaNotFound,
        ErrorCode.ContentUnsupported => EmulationMessageCode.MediaUnsupported,
        ErrorCode.OptionInvalid => EmulationMessageCode.OptionInvalid,
        ErrorCode.HostProtocolFailure => EmulationMessageCode.HostCommunicationFailed,
        ErrorCode.StateInvalid => EmulationMessageCode.SavedStateInvalid,
        ErrorCode.StateIncompatible => EmulationMessageCode.SavedStateIncompatible,
        _ => EmulationMessageCode.MachineStartFailed
    };
}
