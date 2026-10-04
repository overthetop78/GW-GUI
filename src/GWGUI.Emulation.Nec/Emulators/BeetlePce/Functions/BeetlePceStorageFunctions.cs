using GWGUI.Emulation.Nec.Common.Machines.PcEngineDuo.Constants;
using GWGUI.Emulation.Nec.Emulators.BeetlePce.Constants;

namespace GWGUI.Emulation.Nec.Emulators.BeetlePce.Functions;

internal static class BeetlePceStorageFunctions
{
    internal static IReadOnlyList<EmulationMediaDevice> Devices(Model model)
    {
        var devices = new List<EmulationMediaDevice>();
        if (model.SupportsCartridgeSlot)
            devices.Add(new EmulationMediaDevice(EmulationMediaSlot.Cartridge0,
                EmulationMediaType.Cartridge, [BeetlePceStorageConstants.PceExtension,
                    BeetlePceStorageConstants.BinExtension],
                RequiresMachineRecreation: true,
                DisplayLabel: BeetlePceStorageConstants.CartridgeSlotLabel,
                IsPermanent: model.HasBuiltInCartridgeSlot,
                ModelResourceKey: BeetlePceStorageConstants.HuCardSlotResourceKey));
        if (model.SupportsCdDrive)
            devices.Add(new EmulationMediaDevice(EmulationMediaSlot.Cd0,
                EmulationMediaType.CompactDisc,
                [StorageSettingsFunctionsConstants.Cue, StorageSettingsFunctionsConstants.Ccd,
                    BeetlePceStorageConstants.IsoExtension, BeetlePceStorageConstants.ImgExtension,
                    BeetlePceStorageConstants.BinExtension, StorageSettingsFunctionsConstants.Chd],
                RequiresMachineRecreation: true,
                DisplayLabel: StorageSettingsFunctionsConstants.CdDriveLabel,
                IsPermanent: model.HasBuiltInCdDrive,
                ConfigurationKind: EmulationStorageConfigurationKind.CompactDiscDrive,
                CompactDiscSpeeds: BeetlePceStorageConstants.CdSpeeds,
                CompactDiscCacheResourceKey: StorageSettingsFunctionsConstants.ResourceCdImageCache,
                CompactDiscIgnoreErrorsResourceKey:
                    BeetlePceStorageConstants.CdIgnoreErrorsResourceKey,
                ModelResourceKey: model.Id == PcEngineDuoMachineConstants.Id
                    ? BeetlePceStorageConstants.DuoCdRomResourceKey
                    : BeetlePceStorageConstants.CdRom2ResourceKey));
        return devices;
    }

    internal static bool IsCompactDiscEnabled(Model model,
        IReadOnlyDictionary<string, string> options) =>
        model.HasBuiltInCdDrive || options.TryGetValue(
            BeetlePceStorageConstants.CdDriveEnabledOption, out var value)
            && bool.TryParse(value, out var enabled) && enabled;

    internal static EmulationStorageDeviceSettings CompactDiscSettings(
        IReadOnlyDictionary<string, string> options) =>
        new(EmulationMediaSlot.Cd0,
            CompactDisc: new EmulationCompactDiscDriveSettings(
                options.GetValueOrDefault(SettingsConstants.PceCdSpeed,
                    BeetlePceStorageConstants.DefaultCdSpeed),
                options.GetValueOrDefault(SettingsConstants.PceCdImageCache,
                    StorageSettingsFunctionsConstants.CacheDisabled)
                    == StorageSettingsFunctionsConstants.CacheEnabled,
                options.GetValueOrDefault(SettingsConstants.PceCdIgnoreErrors,
                    StorageSettingsFunctionsConstants.CacheDisabled)
                    == StorageSettingsFunctionsConstants.CacheEnabled));

    internal static void ApplyOptions(Dictionary<string, string> options,
        EmulationStorageSettings settings, IReadOnlyCollection<EmulationMediaSlot> configured)
    {
        options[BeetlePceStorageConstants.CdDriveEnabledOption] =
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
