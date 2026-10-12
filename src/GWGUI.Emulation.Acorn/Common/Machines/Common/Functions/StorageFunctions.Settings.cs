using System.IO;

namespace GWGUI.Emulation.Acorn.Common.Machines.Common.Functions;

internal static class StorageSettingsFunctions
{
    internal static EmulationStorageSettings Describe(MachineConfiguration configuration)
    {
        IReadOnlyList<EmulationMediaDevice> devices = ConfigurationFunctions.SupportsRuntime(configuration.Model)
            ? [new(EmulationMediaSlot.Floppy0, EmulationMediaType.Floppy,
                StorageSettingsFunctionsConstants.FloppyExtensions, RequiresMachineRecreation: true,
                DisplayLabel: StorageSettingsFunctionsConstants.DriveLabel,
                ImageDirectory: configuration.Folders.Floppies, IsPermanent: true)] : [];
        return new EmulationStorageSettings(devices, devices.Select(device => device.Slot).ToArray(), configuration.Media.Select(EmulationMediaConversionFunctions.ToCommon).ToArray());
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration,
        EmulationStorageSettings settings)
    {
        var available = Describe(configuration).AvailableDevices.Select(device => device.Slot).ToHashSet();
        if (settings.ConfiguredSlots.Any(slot => !available.Contains(slot))
            || settings.MountedMedia.Any(media => !settings.ConfiguredSlots.Contains(media.Slot)
                || !available.Contains(media.Slot) || media.Type != EmulationMediaType.Floppy
                || !StorageSettingsFunctionsConstants.FloppyExtensions.Contains(Path.GetExtension(media.Path), StringComparer.OrdinalIgnoreCase)))
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(settings));
        var result = configuration with
        {
            Media = settings.MountedMedia.Select(media => EmulationMediaConversionFunctions.ToAcorn(media, configuration.Media)).ToArray()
        };
        ConfigurationFunctions.Validate(result);
        return result;
    }
}
