using CommonCoreHostFunctions = GWGUI.Emulation.Atari.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation;
using System.Diagnostics;

namespace GWGUI.Emulation.Atari.Common.Machines.Common.Functions;

internal static class RuntimeFunctions
{
    internal static EmulationVideoStandard? Region(uint nativeRegion) => CommonCoreHostFunctions.Protocol.Region(nativeRegion);

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
            geometry, core.CoreName, mediaCapabilities?.MediaActivity(configuration, core.LedStates) ?? EmulationMediaActivityFunctions.FromLedStates(configuration.Core, core.LedStates),
            new Dictionary<int, bool>(core.LedStates), core.BufferedAudioFrames, core.AudioOverrunCount,
            core.AudioUnderrunCount, core.HostProcessState, core.HostProcessId);
    }
}

internal static class RuntimeOptionFunctions
{
    private static readonly IReadOnlySet<string> RestartRequired = new HashSet<string>(StringComparer.Ordinal)
    {
        EightBitSettingsConstants.SystemOptionKey,
        EightBitSettingsConstants.BasicEnabledOptionKey,
        EightBitSettingsConstants.Os400800OptionKey,
        EightBitSettingsConstants.XlOsOptionKey,
        EightBitSettingsConstants.ConsoleOsOptionKey,
        EightBitSettingsConstants.BasicVersionOptionKey,
        EightBitSettingsConstants.MosaicMemoryOptionKey,
        EightBitSettingsConstants.AxlonMemoryOptionKey,
        EightBitSettingsConstants.AxlonShadowOptionKey,
        EightBitSettingsConstants.MapRamOptionKey,
        EightBitSettingsConstants.Xep80OptionKey,
        EightBitSettingsConstants.RealTimeClockOptionKey,
        EightBitSettingsConstants.PrinterDeviceOptionKey,
        EightBitSettingsConstants.SerialDeviceOptionKey,
        EightBitSettingsConstants.CassetteBootOptionKey,
        EightBitSettingsConstants.PokeyStereoOptionKey
    };

    internal static bool RequiresRestart(Emulator emulator, string key) => emulator == Emulator.Atari800 && RestartRequired.Contains(key);
}

internal static class MessageFunctions
{
    internal static Exception Translate(Exception error, MachineConfiguration configuration)
    {
        if (error is not EmulationException atariError)
            return CommonCoreHostFunctions.Messages.Translate(error, EmulationMessageCategory.Machine, EmulationMessageCode.MachineStartFailed, new EmulationMachineMessageContext(configuration.MachineId));
        if (atariError.IsLocalized)
            return CommonCoreHostFunctions.Messages.TranslateLocalized(error, Category(atariError.Category), new EmulationMachineMessageContext(configuration.MachineId));
        if (atariError.Code != ErrorCode.ContentRequired)
            return CommonCoreHostFunctions.Messages.Translate(error, Category(atariError.Category), MessageCode(atariError.Code), new EmulationMachineMessageContext(configuration.MachineId));
        var required = CompatibilityCatalog.Get(configuration.Model).Media.Where(rule => rule.Availability == MediaAvailability.Available && rule.Category != MediaCategory.Directory)
            .Select(rule => rule.Category switch
            {
                MediaCategory.Floppy => EmulationMediaCategory.FloppyDrive,
                MediaCategory.HardDisk => EmulationMediaCategory.HardDisk,
                MediaCategory.CompactDisc => EmulationMediaCategory.CompactDiscDrive,
                MediaCategory.Cartridge => EmulationMediaCategory.CartridgeSlot,
                MediaCategory.Cassette => EmulationMediaCategory.CassetteDrive,
                _ => throw new ArgumentOutOfRangeException(nameof(rule))
            })
            .Distinct()
            .ToArray();
        return CommonCoreHostFunctions.Messages.RequiredMediaMissing(configuration.MachineId, required, error);
    }

    private static EmulationMessageCategory Category(ErrorCategory category) => category switch
    {
        ErrorCategory.Core or ErrorCategory.Host => EmulationMessageCategory.Emulator,
        ErrorCategory.Firmware => EmulationMessageCategory.Firmware,
        ErrorCategory.Content => EmulationMessageCategory.Media,
        ErrorCategory.Option => EmulationMessageCategory.Machine,
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
