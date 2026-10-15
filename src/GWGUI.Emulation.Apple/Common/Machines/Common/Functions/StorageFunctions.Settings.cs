using System.IO;

namespace GWGUI.Emulation.Apple.Common.Machines.Common.Functions;

internal static class StorageSettingsFunctions
{
    internal static EmulationStorageSettings Describe(MachineConfiguration configuration)
    {
        var devices = new List<EmulationMediaDevice>();
        if (ConfigurationFunctions.SupportsRuntime(configuration.Model))
        {
            var appleII = configuration.Family == MachineFamily.AppleII;
            var floppyExtensions = appleII ? StorageSettingsFunctionsConstants.AppleWinFloppyExtensions : StorageSettingsFunctionsConstants.MacintoshDiskExtensions;
            devices.Add(new(EmulationMediaSlot.Floppy0, EmulationMediaType.Floppy, floppyExtensions,
                RequiresMachineRecreation: !appleII, DisplayLabel: StorageSettingsFunctionsConstants.FirstDriveLabel,
                ImageDirectory: configuration.Folders.Floppies, IsPermanent: true));
            if (appleII)
                devices.Add(new(EmulationMediaSlot.Floppy1, EmulationMediaType.Floppy, floppyExtensions, DisplayLabel: StorageSettingsFunctionsConstants.SecondDriveLabel, ImageDirectory: configuration.Folders.Floppies, IsPermanent: true));
            devices.Add(new(EmulationMediaSlot.HardDisk0, EmulationMediaType.HardDisk,
                appleII ? StorageSettingsFunctionsConstants.AppleWinHardDiskExtensions : StorageSettingsFunctionsConstants.MacintoshDiskExtensions,
                IsRemovable: false, RequiresMachineRecreation: true,
                DisplayLabel: StorageSettingsFunctionsConstants.HardDiskLabel,
                ImageDirectory: configuration.Folders.HardDisks,
                HardDiskFormats: HardDiskFormats.For(configuration),
                ConfigurationKind: EmulationStorageConfigurationKind.HardDiskDrive));
        }
        return new(devices, devices.Where(device => device.IsPermanent || configuration.Options.ContainsKey(StorageSettingsFunctionsConstants.StorageDevice + device.Slot) || configuration.Media.Any(media => media.Slot == device.Slot)).Select(device => device.Slot).ToArray(),
            configuration.Media.Select(EmulationMediaConversionFunctions.ToCommon).ToArray());
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationStorageSettings settings)
    {
        var devices = Describe(configuration).AvailableDevices.ToDictionary(device => device.Slot);
        if (settings.ConfiguredSlots.Any(slot => !devices.ContainsKey(slot)) ||
            settings.MountedMedia.Any(media => !settings.ConfiguredSlots.Contains(media.Slot)
                || !devices.TryGetValue(media.Slot, out var device) || device.MediaType != media.Type
                || !device.AcceptedExtensions.Contains(Path.GetExtension(media.Path), StringComparer.OrdinalIgnoreCase)))
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(settings));
        var options = configuration.Options.Where(option => !option.Key.StartsWith(StorageSettingsFunctionsConstants.StorageDevice, StringComparison.Ordinal)) .ToDictionary(option => option.Key, option => option.Value, StringComparer.Ordinal);
        foreach (var slot in settings.ConfiguredSlots)
            if (!devices[slot].IsPermanent)
                options[StorageSettingsFunctionsConstants.StorageDevice + slot] = devices[slot].MediaType.ToString();
        var result = configuration with
        {
            Options = options,
            Media = settings.MountedMedia.Select(media => EmulationMediaConversionFunctions.ToApple(media, configuration.Media)).ToArray()
        };
        if (new Engine().Adapter(result) is IEmulatorStorageConfiguration adapter)
            result = adapter.ApplyStorageConfiguration(result, settings.ConfiguredSlots);
        ConfigurationFunctions.Validate(result);
        return result;
    }
}
