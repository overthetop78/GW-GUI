using CommonCoreHostFunctions = GWGUI.Emulation.Sinclair.Common.Functions.CoreHostFunctions;
using System.IO;

namespace GWGUI.Emulation.Sinclair.Common.Machines.Common.Functions;
internal static class StorageSettingsFunctions
{
    internal static EmulationStorageSettings Describe(MachineConfiguration configuration)
    {
        if (configuration.Core == Emulator.None)
            return new(Array.Empty<EmulationMediaDevice>(), Array.Empty<EmulationMediaSlot>(), Array.Empty<EmulationMedia>());
        var devices = new Engine().Adapter(configuration) is IEmulatorMediaCapabilities storage ? storage.DescribeMediaDevices(configuration).ToArray() : [];
        return new(devices, configuration.ConfiguredSlots, configuration.Media.Select(EmulationMediaConversionFunctions.ToCommon).ToArray());
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationStorageSettings settings)
    {
        var devices = Describe(configuration).AvailableDevices;
        if (CommonCoreHostFunctions.MediaRules.HasInvalidStorageSlots(settings, devices)
            || settings.DeviceSettings?.Count 
            > 0)
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(settings));
        foreach (var media in settings.MountedMedia)
        {
            if (CommonCoreHostFunctions.MediaRules.IsInvalidMountedMedia(media, devices, settings.ConfiguredSlots))
                throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(settings));
        }

        var result = configuration with
        {
            ConfiguredSlots = settings.ConfiguredSlots.ToArray(),
            Media = settings.MountedMedia.Select(media => EmulationMediaConversionFunctions.ToModule(media, configuration.Media)).ToArray()
        };
        ConfigurationFunctions.Validate(result);
        return result;
    }
}