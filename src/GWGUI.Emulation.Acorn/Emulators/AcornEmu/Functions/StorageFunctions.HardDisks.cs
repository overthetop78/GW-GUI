using System.Globalization;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.StorageConstants;
using WinchesterFormats = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Dictionaries.HardDiskFormats;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Functions;

/// <summary>Unités Winchester optionnelles et paramètres persistants transmis par les contrats de stockage communs.</summary>
internal static partial class StorageFunctions
{
    private static readonly string[] HardDiskOptions = ["protected", "redirect", "readLatency", "writeLatency", "speed"];
    internal static bool HardDiskConnected(MachineConfiguration configuration, int unit) => configuration.Options.TryGetValue(C.HardDiskKey(unit, "connected"), out var value) ? bool.Parse(value) : configuration.Media.Any(media => media.Slot == new EmulationMediaSlot(EmulationMediaCategory.HardDisk, unit));
    internal static bool HasWinchester(MachineConfiguration configuration) => Enumerable.Range(0, C.HardDiskCount).Any(unit => HardDiskConnected(configuration, unit));
    internal static bool NeedsDiskRom(MachineConfiguration configuration) => HasInterface(configuration) || HasWinchester(configuration);

    internal static (bool Protected, bool Redirect, int ReadLatency, int WriteLatency, int Speed) HardDiskSettings(MachineConfiguration configuration, int unit)
    {
        int Number(string name, int fallback) => int.TryParse(configuration.Options.GetValueOrDefault(C.HardDiskKey(unit, name), fallback.ToString(CultureInfo.InvariantCulture)), NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) ? number : throw new ArgumentException(ErrorMessages.OptionValueInvalidFormat, nameof(configuration));
        bool Flag(string name) => bool.TryParse(configuration.Options.GetValueOrDefault(C.HardDiskKey(unit, name), bool.FalseString), out var flag) ? flag : throw new ArgumentException(ErrorMessages.OptionValueInvalidFormat, nameof(configuration));
        return (Flag("protected"), Flag("redirect"), Number("readLatency", 2), Number("writeLatency", 2), Number("speed", 100));
    }

    internal static IReadOnlyList<EmulationMediaDevice> HardDiskDevices(MachineConfiguration configuration) => Enumerable.Range(0, C.HardDiskCount).Select(unit => new EmulationMediaDevice(new(EmulationMediaCategory.HardDisk, unit), EmulationMediaType.HardDisk, C.HardDiskExtensions,
        IsRemovable: false, InterfaceChoices: [new(C.ScsiInterface, string.Empty, "SCSI")], HardDiskFormats: WinchesterFormats.All,
        ConfigurationKind: EmulationStorageConfigurationKind.HardDiskDrive, ConfigurationFields: HardDiskFields(configuration, unit))).ToArray();

    private static IReadOnlyList<EmulationSettingsField> HardDiskFields(MachineConfiguration configuration, int unit)
    {
        var settings = HardDiskSettings(configuration, unit);
        EmulationSettingsField Flag(string id, string key, bool value) => new(id, EmulationMachineTab.Storage, "HardDisk", key, EmulationSettingsEditor.Toggle, value.ToString(), EnabledValue: bool.TrueString, DisabledValue: bool.FalseString, RequiresRestart: true);
        EmulationSettingsField Delay(string id, string key, int value) => new(id, EmulationMachineTab.Storage, "HardDisk", key, EmulationSettingsEditor.Slider, value.ToString(CultureInfo.InvariantCulture), RequiresRestart: true, NumericRange: new(0, 10000, 1, "ms"));
        return
        [
            new("speed", EmulationMachineTab.Storage, "HardDisk", "Emulation.Storage.Disk.TransferSpeed", EmulationSettingsEditor.Selection, settings.Speed.ToString(CultureInfo.InvariantCulture), C.Speeds.Select(speed => speed == "0" ? new EmulationSettingsChoice(speed, "Emulation.Value.Maximum") : new(speed, string.Empty, speed + " %")).ToArray(), RequiresRestart: true),
            Delay("readLatency", "Emulation.Storage.Disk.ReadLatency", settings.ReadLatency), Delay("writeLatency", "Emulation.Storage.Disk.WriteLatency", settings.WriteLatency),
            Flag("protected", "Emulation.Storage.Floppy.WriteProtection", settings.Protected), Flag("redirect", "Emulation.Storage.Floppy.WriteRedirect", settings.Redirect)
        ];
    }

    internal static IReadOnlyList<EmulationStorageDeviceSettings> DescribeHardDiskSettings(MachineConfiguration configuration) => Enumerable.Range(0, C.HardDiskCount).Where(unit => HardDiskConnected(configuration, unit)).Select(unit => new EmulationStorageDeviceSettings(new(EmulationMediaCategory.HardDisk, unit), InterfaceId: C.ScsiInterface,
        Options: HardDiskFields(configuration, unit).ToDictionary(field => field.Id, field => field.Value ?? string.Empty, StringComparer.Ordinal))).ToArray();

    private static MachineConfiguration SelectHardDiskSlots(MachineConfiguration configuration, IReadOnlyList<EmulationMediaSlot> slots)
    {
        var options = new Dictionary<string, string>(configuration.Options, StringComparer.Ordinal);
        for (var unit = 0; unit < C.HardDiskCount; unit++) options[C.HardDiskKey(unit, "connected")] = slots.Contains(new(EmulationMediaCategory.HardDisk, unit)).ToString();
        return configuration with { Options = options };
    }

    private static MachineConfiguration ApplyHardDiskSettings(MachineConfiguration configuration, IReadOnlyList<EmulationStorageDeviceSettings> settings)
    {
        var options = new Dictionary<string, string>(configuration.Options, StringComparer.Ordinal);
        foreach (var entry in settings.Where(entry => entry.Slot.Category == EmulationMediaCategory.HardDisk))
        {
            if (entry.Slot.Index is < 0 or >= C.HardDiskCount || entry.InterfaceId is not null && !entry.InterfaceId.Equals(C.ScsiInterface, StringComparison.OrdinalIgnoreCase) || entry.Options?.Keys.Any(key => !HardDiskOptions.Contains(key, StringComparer.Ordinal)) == true) throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(settings));
            options[C.HardDiskKey(entry.Slot.Index, "interface")] = C.ScsiInterface;
            foreach (var pair in entry.Options ?? new Dictionary<string, string>()) options[C.HardDiskKey(entry.Slot.Index, pair.Key)] = pair.Value;
        }
        var result = configuration with { Options = options }; ValidateHardDisks(result); return result;
    }

    private static void ValidateHardDisks(MachineConfiguration configuration)
    {
        for (var unit = 0; unit < C.HardDiskCount; unit++)
        {
            _ = HardDiskConnected(configuration, unit); var settings = HardDiskSettings(configuration, unit);
            if (settings.ReadLatency is < 0 or > 10000 || settings.WriteLatency is < 0 or > 10000 || !C.Speeds.Contains(settings.Speed.ToString(CultureInfo.InvariantCulture)) || !configuration.Options.GetValueOrDefault(C.HardDiskKey(unit, "interface"), C.ScsiInterface).Equals(C.ScsiInterface, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException(ErrorMessages.OptionValueInvalidFormat, nameof(configuration));
        }
    }
}
