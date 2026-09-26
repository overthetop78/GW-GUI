using GWGUI.Emulation;
using System.Diagnostics;

namespace GWGUI.Emulation.Atari.Common.Machines.Common.Functions;

internal static class RuntimeFunctions
{
    internal static RuntimeRegion? Region(uint nativeRegion) => nativeRegion switch
    {
        RuntimeConstants.NativeNtscRegion => RuntimeRegion.Ntsc,
        RuntimeConstants.NativePalRegion => RuntimeRegion.Pal,
        _ => null
    };

    internal static int RegionValue(RuntimeRegion? region) =>
        region is null ? RuntimeConstants.MissingRegionValue : (int)region.Value;

    internal static RuntimeRegion? ReadRegion(int value) =>
        value == RuntimeConstants.MissingRegionValue || !Enum.IsDefined(typeof(RuntimeRegion), value)
            ? null
            : (RuntimeRegion)value;

    internal static HostProcessState ProcessState(Process? process, bool connectionFailed, bool disposed)
    {
        if (connectionFailed) return HostProcessState.Faulted;
        if (process is null) return HostProcessState.NotStarted;
        if (disposed) return HostProcessState.Exited;
        try { return process.HasExited ? HostProcessState.Exited : HostProcessState.Running; }
        catch (InvalidOperationException) { return HostProcessState.Exited; }
    }

    internal static RuntimeStatus Status(MachineConfiguration configuration, IEmulatorCore core)
    {
        var frame = core.LatestVideoFrame;
        var geometry = frame is null
            ? null
            : new RuntimeGeometry(frame.Width, frame.Height, frame.Pitch, frame.AspectRatio);
        return new RuntimeStatus(configuration.Model, core.Region, core.FramesPerSecond, core.SampleRate,
            geometry, core.CoreName, EmulationMediaActivityFunctions.FromLedStates(configuration.Core, core.LedStates),
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

    internal static bool RequiresRestart(Emulator emulator, string key) =>
        emulator == Emulator.Atari800 && RestartRequired.Contains(key);
}

internal static class MessageFunctions
{
    internal static Exception Translate(Exception error, MachineConfiguration configuration)
    {
        if (error is not EmulationException atariError)
            return EmulationErrorService.Translate(error,
                EmulationMessageCategory.Machine,
                EmulationMessageCode.MachineStartFailed,
                new EmulationMachineMessageContext(configuration.MachineId));
        if (atariError.IsLocalized)
            return EmulationErrorService.TranslateLocalized(error,
                Category(atariError.Category),
                new EmulationMachineMessageContext(configuration.MachineId));
        if (atariError.Code != ErrorCode.ContentRequired)
            return EmulationErrorService.Translate(error,
                Category(atariError.Category), MessageCode(atariError.Code),
                new EmulationMachineMessageContext(configuration.MachineId));
        var required = CompatibilityCatalog.Get(configuration.Model).Media
            .Where(rule => rule.Availability == MediaAvailability.Available
                && rule.Category != MediaCategory.Directory)
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
        return new EmulationMessageException(new EmulationMessage(
            EmulationMessageCategory.Media,
            EmulationMessageCode.RequiredMediaMissing,
            EmulationMessageSeverity.Error,
            EmulationMessageTarget.Dialog,
            new EmulationRequiredMachineMediaMessageContext(configuration.MachineId, required)), error);
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

public static class ShortcutFunctions
{
    public static IReadOnlyList<ShortcutRule> Rules(MachineConfiguration configuration,
        bool statesAvailable, bool quickStateExists)
    {
        var rules = ShortcutConstants.CommonActions.Select(Available).ToList();
        rules.Add(new ShortcutRule(EmulationShortcutActions.QuickSave,
            Availability(statesAvailable)));
        rules.Add(new ShortcutRule(EmulationShortcutActions.QuickLoad,
            Availability(statesAvailable && quickStateExists)));

        var removable = CompatibilityCatalog.Get(configuration.Model).Media
            .Any(rule => rule.Availability == MediaAvailability.Available && IsRemovable(rule.Category));
        rules.Add(new ShortcutRule(EmulationShortcutActions.InsertMedia, Availability(removable)));
        rules.Add(new ShortcutRule(EmulationShortcutActions.EjectMedia,
            Availability(configuration.Media.Any(media => media.IsInserted && IsEjectable(media.Category)))));
        rules.Add(new ShortcutRule(EmulationShortcutActions.NextMedia,
            Availability(configuration.Media.Count(media => IsDiskSelectable(media.Category)) >
                         ShortcutConstants.MinimumMediaForSelection)));
        return rules;
    }

    public static bool IsAvailable(IReadOnlyList<ShortcutRule> rules, string action) =>
        rules.FirstOrDefault(rule => string.Equals(rule.Action, action, StringComparison.Ordinal))?.Availability ==
        ShortcutAvailability.Available;

    private static ShortcutRule Available(string action) =>
        new(action, ShortcutAvailability.Available);

    private static ShortcutAvailability Availability(bool available) => available
        ? ShortcutAvailability.Available
        : ShortcutAvailability.Unavailable;

    private static bool IsRemovable(MediaCategory category) => category is MediaCategory.Floppy or
        MediaCategory.Cassette or MediaCategory.Cartridge or MediaCategory.CompactDisc;

    private static bool IsDiskSelectable(MediaCategory category) => category == MediaCategory.Floppy;

    private static bool IsEjectable(MediaCategory category) =>
        category is MediaCategory.Floppy or MediaCategory.Cassette;
}
