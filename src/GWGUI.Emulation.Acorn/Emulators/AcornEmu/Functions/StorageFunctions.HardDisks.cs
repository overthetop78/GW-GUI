using S = GWGUI.Emulation.Constants.EmulationMechanicalSoundConstants;
using System.Globalization;
using CommonHost = GWGUI.Emulation.Acorn.Common.Functions.CoreHostFunctions;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.StorageConstants;
using WinchesterFormats = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Dictionaries.HardDiskFormats;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Functions;

/// <summary>Unités Winchester optionnelles et paramètres persistants transmis par les contrats de stockage communs.</summary>
internal static partial class StorageFunctions
{
    internal static bool HardDiskConnected(MachineConfiguration configuration, int unit) => configuration.Options.TryGetValue(C.HardDiskKey(unit, C.ConnectedOption), out var value) ? bool.Parse(value) : configuration.Media.Any(media => media.Slot == new EmulationMediaSlot(EmulationMediaCategory.HardDisk, unit));
    internal static bool HasWinchester(MachineConfiguration configuration) => Enumerable.Range(0, C.HardDiskCount).Any(unit => HardDiskConnected(configuration, unit));
    internal static bool NeedsDiskRom(MachineConfiguration configuration) => HasInterface(configuration) || HasWinchester(configuration);

    internal static (bool Protected, int ReadLatency, int WriteLatency, int Speed) HardDiskSettings(MachineConfiguration configuration, int unit)
    {
        int Number(string name, int fallback) => int.TryParse(configuration.Options.GetValueOrDefault(C.HardDiskKey(unit, name), fallback.ToString(CultureInfo.InvariantCulture)), NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) ? number : throw new ArgumentException(ErrorMessages.OptionValueInvalidFormat, nameof(configuration));
        bool Flag(string name) => bool.TryParse(configuration.Options.GetValueOrDefault(C.HardDiskKey(unit, name), bool.FalseString), out var flag) ? flag : throw new ArgumentException(ErrorMessages.OptionValueInvalidFormat, nameof(configuration));
        return (Flag(C.ProtectedOption), Number(C.ReadLatencyOption, C.DefaultHardDiskLatencyMilliseconds), Number(C.WriteLatencyOption, C.DefaultHardDiskLatencyMilliseconds), Number(C.SpeedOption, C.DefaultSpeedPercent));
    }

    internal static IReadOnlyList<EmulationMediaDevice> HardDiskDevices(MachineConfiguration configuration) => Enumerable.Range(0, C.HardDiskCount).Select(unit => new EmulationMediaDevice(new(EmulationMediaCategory.HardDisk, unit), EmulationMediaType.HardDisk, C.HardDiskExtensions,
        IsRemovable: false, InterfaceChoices: [new(C.ScsiInterface, string.Empty, C.ScsiDisplay)], HardDiskFormats: WinchesterFormats.All,
        ConfigurationKind: EmulationStorageConfigurationKind.HardDiskDrive, ConfigurationFields: HardDiskFields(configuration, unit))).ToArray();

    private static IReadOnlyList<EmulationSettingsField> HardDiskFields(MachineConfiguration configuration, int unit)
    {
        var settings = HardDiskSettings(configuration, unit);
        EmulationSettingsField Flag(string id, string key, bool value) => new(id, EmulationMachineTab.Storage, S.HardDisk, key, EmulationSettingsEditor.Toggle, value.ToString(), EnabledValue: bool.TrueString, DisabledValue: bool.FalseString, RequiresRestart: true);
        EmulationSettingsField Delay(string id, string key, int value) => new(id, EmulationMachineTab.Storage, S.HardDisk, key, EmulationSettingsEditor.Slider, value.ToString(CultureInfo.InvariantCulture), RequiresRestart: true, NumericRange: new(0, C.MaximumLatencyMilliseconds, 1, C.MillisecondsSuffix));
        return
        [
            new(C.SpeedOption, EmulationMachineTab.Storage, S.HardDisk, C.SpeedResource, EmulationSettingsEditor.Selection, settings.Speed.ToString(CultureInfo.InvariantCulture), C.Speeds.Select(speed => speed == C.MaximumSpeed ? new EmulationSettingsChoice(speed, C.MaximumSpeedResource) : new(speed, string.Empty, speed + C.PercentSuffix)).ToArray(), RequiresRestart: true),
            Delay(C.ReadLatencyOption, C.ReadLatencyResource, settings.ReadLatency), Delay(C.WriteLatencyOption, C.WriteLatencyResource, settings.WriteLatency),
            Flag(C.ProtectedOption, C.ProtectionResource, settings.Protected),
            Flag(C.MechanicalSoundOption, C.SoundResource, bool.Parse(configuration.Options.GetValueOrDefault(C.HardDiskKey(unit, C.MechanicalSoundOption), bool.TrueString))), SoundProfileField(configuration, unit, true), SoundVolumeField(configuration, unit, true)
        ];
    }

    internal static IReadOnlyList<EmulationStorageDeviceSettings> DescribeHardDiskSettings(MachineConfiguration configuration) => Enumerable.Range(0, C.HardDiskCount).Where(unit => HardDiskConnected(configuration, unit)).Select(unit => new EmulationStorageDeviceSettings(new(EmulationMediaCategory.HardDisk, unit), InterfaceId: C.ScsiInterface,
        Options: HardDiskFields(configuration, unit).ToDictionary(field => field.Id, field => field.Value ?? string.Empty, StringComparer.Ordinal))).ToArray();

    private static MachineConfiguration SelectHardDiskSlots(MachineConfiguration configuration, IReadOnlyList<EmulationMediaSlot> slots)
    {
        var options = new Dictionary<string, string>(configuration.Options, StringComparer.Ordinal);
        for (var unit = 0; unit < C.HardDiskCount; unit++) options[C.HardDiskKey(unit, C.ConnectedOption)] = slots.Contains(new(EmulationMediaCategory.HardDisk, unit)).ToString();
        return configuration with { Options = options };
    }

    private static MachineConfiguration ApplyHardDiskSettings(MachineConfiguration configuration, IReadOnlyList<EmulationStorageDeviceSettings> settings)
    {
        var options = new Dictionary<string, string>(configuration.Options, StringComparer.Ordinal);
        foreach (var entry in settings.Where(entry => entry.Slot.Category == EmulationMediaCategory.HardDisk))
        {
            if (entry.Slot.Index is < 0 or >= C.HardDiskCount || entry.InterfaceId is not null && !entry.InterfaceId.Equals(C.ScsiInterface, StringComparison.OrdinalIgnoreCase) || entry.Options?.Keys.Any(key => !C.HardDiskOptions.Contains(key, StringComparer.Ordinal)) == true) throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(settings));
            options.Remove(C.HardDiskKey(entry.Slot.Index, C.RedirectOption));
            options[C.HardDiskKey(entry.Slot.Index, C.InterfaceOption)] = C.ScsiInterface;
            foreach (var pair in entry.Options ?? new Dictionary<string, string>()) options[C.HardDiskKey(entry.Slot.Index, pair.Key)] = pair.Value;
        }
        var result = configuration with { Options = options }; ValidateHardDisks(result); return result;
    }

    private static void ValidateHardDisks(MachineConfiguration configuration)
    {
        for (var unit = 0; unit < C.HardDiskCount; unit++)
        {
            _ = EmulationMechanicalSoundSettings.ReadVolume(configuration.Options, C.HardDiskKey(unit, C.SoundVolumeOption));
            _ = HardDiskConnected(configuration, unit); var settings = HardDiskSettings(configuration, unit);
            if (!bool.TryParse(configuration.Options.GetValueOrDefault(C.HardDiskKey(unit, C.MechanicalSoundOption), bool.TrueString), out _) || !CommonHost.MediaProcessing.MechanicalSoundProfiles(S.HardDisk).Contains(configuration.Options.GetValueOrDefault(C.HardDiskKey(unit, C.SoundProfileOption), S.Winchester))) throw new ArgumentException(ErrorMessages.OptionValueInvalidFormat, nameof(configuration));
            if (settings.ReadLatency is < 0 or > C.MaximumLatencyMilliseconds || settings.WriteLatency is < 0 or > C.MaximumLatencyMilliseconds || !C.Speeds.Contains(settings.Speed.ToString(CultureInfo.InvariantCulture)) || !configuration.Options.GetValueOrDefault(C.HardDiskKey(unit, C.InterfaceOption), C.ScsiInterface).Equals(C.ScsiInterface, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException(ErrorMessages.OptionValueInvalidFormat, nameof(configuration));
        }
    }
}
