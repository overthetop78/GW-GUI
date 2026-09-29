namespace GWGUI.Emulation.Microsoft.Common.Machines.Common.Functions;

internal static class StorageSettingsFunctions
{
    internal static EmulationStorageSettings Describe(MachineConfiguration configuration)
    {
        var model = ModelCatalog.Get(configuration.Model);
        var devices = new List<EmulationMediaDevice>();
        if (model.SupportsCompactDiscDrive)
            devices.Add(new EmulationMediaDevice(EmulationMediaSlot.Cd0,
                EmulationMediaType.CompactDisc, StorageSettingsFunctionsConstants.XboxExtensions,
                RequiresMachineRecreation: true,
                DisplayLabel: StorageSettingsFunctionsConstants.CompactDiscDriveLabel,
                IsPermanent: model.HasBuiltInCompactDiscDrive));
        var configured = devices.Where(device => device.Slot.Category switch
        {
            EmulationMediaCategory.CompactDiscDrive => model.HasBuiltInCompactDiscDrive,
            _ => false
        }).Select(device => device.Slot).ToArray();
        var mounted = EmulationMediaConversionFunctions.ToCommon(configuration.Media ?? []);
        return new EmulationStorageSettings(devices, configured, mounted);
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration,
        EmulationStorageSettings settings)
    {
        var media = settings.MountedMedia.Select((item, index) => new MediaConfiguration(
            item.Path, item.Type switch
            {
                EmulationMediaType.CompactDisc => MediaCategory.CompactDisc,
                _ => throw new ArgumentOutOfRangeException(nameof(settings), item.Type, null)
            }, IsReadOnly: item.IsReadOnly, IsInserted: item.IsInserted, MountOrder: index)).ToArray();
        var model = ModelCatalog.Get(configuration.Model);
        foreach (var item in media)
            if (!ConfigurationValidationFunctions.Supports(model, item.Category))
                throw new ArgumentOutOfRangeException(nameof(settings), item.Category, null);
        return configuration with { Media = media };
    }
}
