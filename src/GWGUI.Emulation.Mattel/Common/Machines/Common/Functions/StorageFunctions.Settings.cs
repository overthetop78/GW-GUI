using System.IO;

namespace GWGUI.Emulation.Mattel.Common.Machines.Common.Functions;

internal static class StorageSettingsFunctions
{
    internal static EmulationStorageSettings Describe(MachineConfiguration configuration)
    {
        var devices = new Engine().Adapter(configuration) is IEmulatorMediaCapabilities storage ? storage.DescribeMediaDevices(configuration) : [];
        return new(devices, configuration.ConfiguredSlots, configuration.Media.Select(EmulationMediaConversionFunctions.ToCommon).ToArray());
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration,EmulationStorageSettings settings)
    {
        var device=Describe(configuration).AvailableDevices.Single();
        if(settings.ConfiguredSlots.Count!=1 || settings.ConfiguredSlots[0]!=device.Slot || settings.MountedMedia.Count>1
            || settings.DeviceSettings?.Count>0 || settings.MountedMedia.Any(media=>media.Slot!=device.Slot || media.Type!=device.MediaType
                || !device.AcceptedExtensions.Contains(Path.GetExtension(media.Path),StringComparer.OrdinalIgnoreCase)))
            throw new ArgumentException(ErrorMessages.IncompatibleMedia,nameof(settings));
        var result=configuration with {ConfiguredSlots=settings.ConfiguredSlots.ToArray(),Media=settings.MountedMedia.Select(media=>EmulationMediaConversionFunctions.ToModule(media,configuration.Media)).ToArray()};
        ConfigurationFunctions.Validate(result);
        return result;
    }
}
