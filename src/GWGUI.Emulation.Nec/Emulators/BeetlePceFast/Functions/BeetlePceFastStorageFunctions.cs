using GWGUI.Emulation.Nec.Common.Machines.PcEngineDuo.Constants;
using GWGUI.Emulation.Nec.Emulators.BeetlePceFast.Constants;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Constants;

namespace GWGUI.Emulation.Nec.Emulators.BeetlePceFast.Functions;

internal static class BeetlePceFastStorageFunctions
{
    internal static IReadOnlyList<EmulationMediaDevice> Devices(Model model)
    {
        var devices = new List<EmulationMediaDevice>();
        if (model.SupportsCartridgeSlot)
            devices.Add(new EmulationMediaDevice(EmulationMediaSlot.Cartridge0,
                EmulationMediaType.Cartridge, [BeetlePceFastStorageConstants.PceExtension,
                    BeetlePceFastStorageConstants.BinExtension],
                RequiresMachineRecreation: true,
                DisplayLabel: BeetlePceFastStorageConstants.CartridgeSlotLabel,
                IsPermanent: model.HasBuiltInCartridgeSlot,
                ModelResourceKey: BeetlePceFastStorageConstants.HuCardSlotResourceKey));
        if (model.SupportsCdDrive)
            devices.Add(new EmulationMediaDevice(EmulationMediaSlot.Cd0,
                EmulationMediaType.CompactDisc,
                [StorageSettingsFunctionsConstants.Cue, StorageSettingsFunctionsConstants.Ccd,
                    BeetlePceFastStorageConstants.IsoExtension, BeetlePceFastStorageConstants.ImgExtension,
                    BeetlePceFastStorageConstants.BinExtension, StorageSettingsFunctionsConstants.Chd],
                RequiresMachineRecreation: true,
                DisplayLabel: StorageSettingsFunctionsConstants.CdDriveLabel,
                IsPermanent: model.HasBuiltInCdDrive,
                ConfigurationKind: EmulationStorageConfigurationKind.CompactDiscDrive,
                CompactDiscSpeeds: BeetlePceFastStorageConstants.CdSpeeds,
                CompactDiscCacheResourceKey: StorageSettingsFunctionsConstants.ResourceCdImageCache,
                CompactDiscIgnoreErrorsResourceKey:
                    BeetlePceFastStorageConstants.CdIgnoreErrorsResourceKey,
                ModelResourceKey: model.Id == PcEngineDuoMachineConstants.Id
                    ? BeetlePceFastStorageConstants.DuoCdRomResourceKey
                    : BeetlePceFastStorageConstants.CdRom2ResourceKey));
        return devices;
    }

    internal static bool IsCompactDiscEnabled(Model model,
        IReadOnlyDictionary<string, string> options) =>
        model.HasBuiltInCdDrive || options.TryGetValue(
            BeetlePceFastStorageConstants.CdDriveEnabledOption, out var value)
            && bool.TryParse(value, out var enabled) && enabled;

    internal static EmulationStorageDeviceSettings CompactDiscSettings(
        IReadOnlyDictionary<string, string> options) =>
        new(EmulationMediaSlot.Cd0,
            CompactDisc: new EmulationCompactDiscDriveSettings(
                options.GetValueOrDefault(SettingsConstants.PceCdSpeed,
                    BeetlePceFastStorageConstants.DefaultCdSpeed),
                options.GetValueOrDefault(SettingsConstants.PceCdImageCache,
                    StorageSettingsFunctionsConstants.CacheDisabled)
                    == StorageSettingsFunctionsConstants.CacheEnabled,
                options.GetValueOrDefault(SettingsConstants.PceCdIgnoreErrors,
                    StorageSettingsFunctionsConstants.CacheDisabled)
                    == StorageSettingsFunctionsConstants.CacheEnabled));

    internal static void ApplyOptions(Dictionary<string, string> options,
        EmulationStorageSettings settings, IReadOnlyCollection<EmulationMediaSlot> configured)
    {
        options[BeetlePceFastStorageConstants.CdDriveEnabledOption] =
            configured.Contains(EmulationMediaSlot.Cd0).ToString();
        var cd = settings.DeviceSettings?.FirstOrDefault(item =>
            item.Slot == EmulationMediaSlot.Cd0)?.CompactDisc;
        if (cd is null) return;
        options[SettingsConstants.PceCdImageCache] = cd.CacheImage
            ? StorageSettingsFunctionsConstants.CacheEnabled
            : StorageSettingsFunctionsConstants.CacheDisabled;
        options[SettingsConstants.PceCdSpeed] = cd.Speed;
        options[SettingsConstants.PceCdIgnoreErrors] = cd.IgnoreErrors
            ? StorageSettingsFunctionsConstants.CacheEnabled
            : StorageSettingsFunctionsConstants.CacheDisabled;
    }
}
