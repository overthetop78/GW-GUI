using GWGUI.Emulation.Bandai.Emulators.BeetleWonderSwan.Dictionaries;
using GWGUI.Emulation.Bandai.Emulators.PlaydiaEmu.Dictionaries;
using GWGUI.Emulation.Bandai.Emulators.TamaLibretro.Dictionaries;

namespace GWGUI.Emulation.Bandai.Common.Machines.Common.Functions;

internal static class ConfigurationFunctions
{
    internal static Emulator DefaultEmulator(MachineModel model) => model switch
    {
        MachineModel.WonderSwan or MachineModel.WonderSwanColor or MachineModel.SwanCrystal => Emulator.BeetleWonderSwan,
        MachineModel.Playdia => Emulator.PlaydiaEmu,
        MachineModel.TamagotchiP1 => Emulator.TamaLibretro,
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };

    internal static PeripheralCategory DefaultPeripheral(MachineModel model) => model switch
    {
        MachineModel.WonderSwan or MachineModel.WonderSwanColor or MachineModel.SwanCrystal => PeripheralCategory.WonderSwan,
        MachineModel.Playdia => PeripheralCategory.Playdia,
        MachineModel.TamagotchiP1 => PeripheralCategory.Tamagotchi,
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };

    internal static void Validate(MachineConfiguration configuration)
    {
        if (configuration.SchemaVersion != ConfigurationStoreConstants.CurrentSchemaVersion)
            throw new ArgumentOutOfRangeException(nameof(configuration.SchemaVersion));
        if (configuration.Id == Guid.Empty || !Enum.IsDefined(configuration.Model))
            throw new ArgumentException(nameof(configuration));
        if (configuration.Core != DefaultEmulator(configuration.Model))
            throw new ArgumentException(nameof(configuration.Core));
        var options = configuration.Core switch
        {
            Emulator.BeetleWonderSwan => BeetleWonderSwanOptionCatalog.All,
            Emulator.PlaydiaEmu => PlaydiaEmuOptionCatalog.All,
            Emulator.TamaLibretro => TamaLibretroOptionCatalog.All,
            _ => throw new ArgumentOutOfRangeException(nameof(configuration.Core))
        };
        foreach (var option in configuration.Options)
        {
            if (option.Key == SettingsConstants.AudioOutput)
            {
                if (string.IsNullOrWhiteSpace(option.Value))
                    throw new ArgumentException(nameof(configuration.Options));
                continue;
            }
            if (option.Key == SettingsConstants.AudioLatency)
            {
                if (!int.TryParse(option.Value, System.Globalization.NumberStyles.Integer,
                        System.Globalization.CultureInfo.InvariantCulture, out var latency)
                    || !SettingsConstants.Latencies.Contains(latency))
                    throw new ArgumentException(nameof(configuration.Options));
                continue;
            }
            if (option.Key == VideoAudioSettingsConstants.AudioVolumeOption)
            {
                if (!int.TryParse(option.Value, System.Globalization.NumberStyles.Integer,
                        System.Globalization.CultureInfo.InvariantCulture, out var volume)
                    || volume is < VideoAudioSettingsConstants.MinimumVolumePercent
                        or > VideoAudioSettingsConstants.MaximumVolumePercent)
                    throw new ArgumentException(nameof(configuration.Options));
                continue;
            }
            if (!options.Any(definition => string.Equals(definition.Key, option.Key, StringComparison.Ordinal)
                && definition.Values.Any(choice => string.Equals(choice.Value, option.Value, StringComparison.Ordinal))))
                throw new ArgumentException(nameof(configuration.Options));
        }
        var firmwareRoles = new HashSet<FirmwareCategory>();
        foreach (var firmware in configuration.Firmwares)
            if (configuration.Model != MachineModel.TamagotchiP1 || firmware.Category != FirmwareCategory.System
                || !firmwareRoles.Add(firmware.Category) || string.IsNullOrWhiteSpace(firmware.Path))
                throw new ArgumentException(nameof(configuration.Firmwares));
        var slots = new HashSet<EmulationMediaSlot>();
        foreach (var media in configuration.Media)
        {
            var compatible = configuration.Family switch
            {
                MachineFamily.WonderSwan => media.Category == MediaCategory.Cartridge && media.Slot == EmulationMediaSlot.Cartridge0,
                MachineFamily.Playdia => media.Category == MediaCategory.CompactDisc && media.Slot == EmulationMediaSlot.Cd0,
                _ => false
            };
            if (!compatible || !slots.Add(media.Slot) || string.IsNullOrWhiteSpace(media.Path))
                throw new ArgumentException(nameof(configuration.Media));
        }
        var ports = new HashSet<int>();
        foreach (var controller in configuration.Input.Controllers ?? [])
        {
            if (controller.Port != ControllerConstants.FirstPort || !ports.Add(controller.Port)
                || controller.DeadZonePercent is < ControllerConstants.MinimumDeadZonePercent
                    or > ControllerConstants.MaximumDeadZonePercent
                || controller.Peripheral != PeripheralCategory.None && controller.Peripheral != PeripheralCategory.Automatic
                    && controller.Peripheral != DefaultPeripheral(configuration.Model))
                throw new ArgumentException(nameof(configuration.Input));
            if (controller.Mappings?.Any(mapping =>
                    !InputSnapshotDictionary.ForModel(configuration.Model).ContainsKey(mapping.Key)
                    || string.IsNullOrWhiteSpace(mapping.Value)) == true)
                throw new ArgumentException(nameof(configuration.Input));
        }
    }
}
