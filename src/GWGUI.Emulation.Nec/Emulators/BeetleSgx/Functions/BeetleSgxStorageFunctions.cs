using GWGUI.Emulation.Nec.Emulators.BeetlePceFast.Constants;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Nec.Emulators.BeetleSgx.Constants;

namespace GWGUI.Emulation.Nec.Emulators.BeetleSgx.Functions;

internal static class BeetleSgxStorageFunctions
{
    internal static IReadOnlyList<EmulationMediaDevice> Devices(Model model)
    {
        var devices = new List<EmulationMediaDevice>();
        if (model.SupportsCartridgeSlot)
            devices.Add(new EmulationMediaDevice(EmulationMediaSlot.Cartridge0,
                EmulationMediaType.Cartridge,
                [BeetlePceFastStorageConstants.PceExtension, BeetlePceFastStorageConstants.BinExtension],
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
                CompactDiscSpeeds: BeetleSgxSettingsConstants.CdSpeeds,
                CompactDiscCacheResourceKey: StorageSettingsFunctionsConstants.ResourceCdImageCache,
                ModelResourceKey: BeetlePceFastStorageConstants.CdRom2ResourceKey));
        return devices;
    }

    internal static bool IsCompactDiscEnabled(Model model,
        IReadOnlyDictionary<string, string> options) =>
        model.HasBuiltInCdDrive || options.TryGetValue(
            BeetleSgxSettingsConstants.CdDriveEnabledOption, out var value)
            && bool.TryParse(value, out var enabled) && enabled;

    internal static EmulationStorageDeviceSettings CompactDiscSettings(
        IReadOnlyDictionary<string, string> options) =>
        new(EmulationMediaSlot.Cd0,
            CompactDisc: new EmulationCompactDiscDriveSettings(
                options.GetValueOrDefault(BeetleSgxSettingsConstants.CdSpeed,
                    BeetleSgxSettingsConstants.DefaultCdSpeed),
                options.GetValueOrDefault(BeetleSgxSettingsConstants.CdImageCache,
                    StorageSettingsFunctionsConstants.CacheDisabled)
                    == StorageSettingsFunctionsConstants.CacheEnabled,
                false));

    internal static void ApplyOptions(Dictionary<string, string> options,
        EmulationStorageSettings settings, IReadOnlyCollection<EmulationMediaSlot> configured)
    {
        options[BeetleSgxSettingsConstants.CdDriveEnabledOption] =
            configured.Contains(EmulationMediaSlot.Cd0).ToString();
        var cd = settings.DeviceSettings?.FirstOrDefault(item =>
            item.Slot == EmulationMediaSlot.Cd0)?.CompactDisc;
        if (cd is null) return;
        options[BeetleSgxSettingsConstants.CdImageCache] = cd.CacheImage
            ? StorageSettingsFunctionsConstants.CacheEnabled
            : StorageSettingsFunctionsConstants.CacheDisabled;
        options[BeetleSgxSettingsConstants.CdSpeed] = cd.Speed;
    }
}
