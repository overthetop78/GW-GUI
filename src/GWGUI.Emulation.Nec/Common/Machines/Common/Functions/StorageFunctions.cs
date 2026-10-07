using GWGUI.Emulation.Nec.Emulators.BeetlePceFast.Constants;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Nec.Emulators.BeetlePceFast.Functions;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Nec.Emulators.BeetlePcfx.Constants;
using GWGUI.Emulation.Nec.Emulators.BeetlePcfx.Functions;
using GWGUI.Emulation.Nec.Emulators.BeetleSgx.Constants;
using GWGUI.Emulation.Nec.Emulators.BeetleSgx.Functions;
using GWGUI.Emulation.Nec.Emulators.Geargrafx.Constants;
using GWGUI.Emulation.Nec.Emulators.Geargrafx.Functions;

namespace GWGUI.Emulation.Nec.Common.Machines.Common.Functions;

internal static class StorageSettingsFunctions
{
    internal static EmulationStorageSettings Describe(MachineConfiguration configuration)
    {
        var model = ModelCatalog.Get(configuration.Model);
        var options = configuration.Options ?? new Dictionary<string, string>();
        var devices = configuration.EmulatorId switch
        {
            BeetlePceFastConstants.Id => BeetlePceFastStorageFunctions.Devices(model),
            BeetlePcfxConstants.Id => BeetlePcfxStorageFunctions.Devices(model),
            BeetleSgxConstants.Id => BeetleSgxStorageFunctions.Devices(model),
            GeargrafxConstants.Id => GeargrafxStorageFunctions.Devices(model),
            Emulators.BeetlePce.Constants.CoreConstants.Id => Emulators.BeetlePce.Functions.StorageFunctions.Devices(model),
            Emulators.Quasi88.Constants.CoreConstants.Id => Emulators.Quasi88.Functions.StorageFunctions.Devices(model),
            Emulators.NekoProjectII.Constants.CoreConstants.Id => Emulators.NekoProjectII.Functions.StorageFunctions.Devices(model),
            Emulators.NP2Kai.Constants.CoreConstants.Id => Emulators.NP2Kai.Functions.StorageFunctions.Devices(model),
            _ => []
        };
        var configured = devices.Where(device => device.Slot.Category switch
        {
            EmulationMediaCategory.CartridgeSlot => device.IsPermanent,
            EmulationMediaCategory.CompactDiscDrive => device.IsPermanent
                || configuration.EmulatorId == BeetlePceFastConstants.Id
                    && BeetlePceFastStorageFunctions.IsCompactDiscEnabled(model, options)
                || configuration.EmulatorId == BeetleSgxConstants.Id
                    && BeetleSgxStorageFunctions.IsCompactDiscEnabled(model, options)
                || configuration.EmulatorId == GeargrafxConstants.Id
                    && GeargrafxStorageFunctions.IsCompactDiscEnabled(model, options),
            EmulationMediaCategory.MemoryCard => configuration.EmulatorId == BeetlePcfxConstants.Id
                && BeetlePcfxStorageFunctions.IsExternalCardEnabled(options),
            _ => device.IsPermanent
        }).Select(device => device.Slot).ToArray();
        var mounted = EmulationMediaConversionFunctions.ToCommon(configuration.Media ?? []);
        var deviceSettings = new List<EmulationStorageDeviceSettings>();
        if (!CoreSettingsFunctions.HasDefinitions(configuration.EmulatorId)
            && devices.Any(device => device.Slot == EmulationMediaSlot.Cd0))
            deviceSettings.Add(configuration.EmulatorId switch
            {
                BeetlePcfxConstants.Id => BeetlePcfxStorageFunctions.CompactDiscSettings(options),
                BeetleSgxConstants.Id => BeetleSgxStorageFunctions.CompactDiscSettings(options),
                _ => BeetlePceFastStorageFunctions.CompactDiscSettings(options)
            });
        if (devices.Any(device => device.Slot == EmulationMediaSlot.MemoryCard0))
            deviceSettings.Add(BeetlePcfxStorageFunctions.ExternalCardSettings(options));
        return new EmulationStorageSettings(devices, configured, mounted, deviceSettings);
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration,
        EmulationStorageSettings settings)
    {
        var model = ModelCatalog.Get(configuration.Model);
        var available = Describe(configuration).AvailableDevices;
        if (settings.ConfiguredSlots.Any(slot => available.All(device => device.Slot != slot))
            || settings.MountedMedia.Any(media => settings.ConfiguredSlots.All(slot => slot != media.Slot)
                || available.All(device => device.Slot != media.Slot
                    || device.MediaType != media.Type)))
            throw new ArgumentOutOfRangeException(nameof(settings));
        var configured = settings.ConfiguredSlots.Concat(available
            .Where(device => device.IsPermanent).Select(device => device.Slot)).Distinct().ToArray();
        var media = settings.MountedMedia.Select((item, index) => new MediaConfiguration(
            item.Path, item.Type switch
            {
                EmulationMediaType.CompactDisc => MediaCategory.CompactDisc,
                EmulationMediaType.Cartridge => MediaCategory.Cartridge,
                EmulationMediaType.Floppy => MediaCategory.Floppy,
                EmulationMediaType.HardDisk => MediaCategory.HardDisk,
                _ => throw new ArgumentOutOfRangeException(nameof(settings), item.Type, null)
            }, IsReadOnly: item.IsReadOnly, IsInserted: item.IsInserted, MountOrder: index, SlotIndex: item.Slot.Index)).ToArray();
        foreach (var item in media)
            if (!ConfigurationValidationFunctions.Supports(model, item.Category))
                throw new ArgumentOutOfRangeException(nameof(settings), item.Category, null);
        var options = new Dictionary<string, string>(configuration.Options
            ?? new Dictionary<string, string>(), StringComparer.Ordinal);
        if (configuration.EmulatorId == BeetlePcfxConstants.Id)
            BeetlePcfxStorageFunctions.ApplyOptions(options, settings, configured);
        else if (configuration.EmulatorId == BeetlePceFastConstants.Id)
            BeetlePceFastStorageFunctions.ApplyOptions(options, settings, configured);
        else if (configuration.EmulatorId == BeetleSgxConstants.Id)
            BeetleSgxStorageFunctions.ApplyOptions(options, settings, configured);
        else if (configuration.EmulatorId == GeargrafxConstants.Id)
            GeargrafxStorageFunctions.ApplyOptions(options, configured);
        return configuration with { Media = media, Options = options };
    }
}
