using CommonCoreHostFunctions = GWGUI.Emulation.Thomson.Common.Functions.CoreHostFunctions;
using System.IO;

namespace GWGUI.Emulation.Thomson.Common.Machines.Common.Functions;
internal static class StorageSettingsFunctions
{
    internal static EmulationStorageSettings Describe(MachineConfiguration configuration)
    {
        var engine = new Engine();
        var adapter = configuration.Core == Emulator.None
            ? engine.Adapters(configuration.MachineId).FirstOrDefault()
            : engine.Adapter(configuration);
        var devices = adapter is IEmulatorMediaCapabilities capabilities
            ? capabilities.DescribeMediaDevices(configuration)
            : [];
        return new(devices, configuration.ConfiguredSlots, configuration.Media.Select(EmulationMediaConversionFunctions.ToCommon).ToArray());
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationStorageSettings settings)
    {
        var devices = Describe(configuration).AvailableDevices;
        if (CommonCoreHostFunctions.MediaRules.HasInvalidStorageSlots(settings, devices)
            || settings.DeviceSettings?.Count 
            > 0)
            throw new ArgumentException(nameof(settings));
        foreach (var media in settings.MountedMedia)
        {
            if (CommonCoreHostFunctions.MediaRules.IsInvalidMountedMedia(media, devices, settings.ConfiguredSlots))
                throw new ArgumentException(nameof(settings));
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
