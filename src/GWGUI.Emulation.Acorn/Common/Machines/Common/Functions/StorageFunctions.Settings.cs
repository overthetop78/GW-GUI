using System.IO;

namespace GWGUI.Emulation.Acorn.Common.Machines.Common.Functions;

internal static class StorageSettingsFunctions
{
    internal static EmulationStorageSettings Describe(MachineConfiguration configuration)
    {
        IReadOnlyList<EmulationMediaDevice> devices = configuration.Core != Emulator.None && new Engine().Adapter(configuration) is IEmulatorStorageSettings storage ? storage.DescribeMediaDevices(configuration) : [];
        return new EmulationStorageSettings(devices, devices.Select(device => device.Slot).ToArray(), configuration.Media.Select(EmulationMediaConversionFunctions.ToCommon).ToArray());
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationStorageSettings settings)
    {
        var devices = Describe(configuration).AvailableDevices;
        var available = devices.Select(device => device.Slot).ToHashSet();
        if (settings.ConfiguredSlots.Any(slot => !available.Contains(slot))
           || settings.MountedMedia.Any(media => !settings.ConfiguredSlots.Contains(media.Slot)
               || !devices.Any(device => device.Slot == media.Slot && device.MediaType == media.Type
                    && device.AcceptedExtensions.Contains(Path.GetExtension(media.Path), StringComparer.OrdinalIgnoreCase))))
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(settings));
        var result = configuration with
        {
            Media = settings.MountedMedia.Select(media => EmulationMediaConversionFunctions.ToAcorn(media, configuration.Media)).ToArray()
        };
        ConfigurationFunctions.Validate(result);
        return result;
    }
}
