using System.IO;

namespace GWGUI.Emulation.Example.Common.Machines.Common.Functions;

/// <summary>Description et validation des cinq catégories de lecteurs des profils fictifs.</summary>
internal static class StorageSettingsFunctions
{
    /// <summary>Décrit les lecteurs du profil sans consulter une installation de cœur.</summary>
    /// <param name="model">Profil A ou B dont les lecteurs doivent être exposés.</param>
    /// <returns>Un lecteur par catégorie de média déclarée dans le profil matériel.</returns>
    internal static IReadOnlyList<EmulationMediaDevice> DescribeDevices(MachineModel model) => HardwareModelCatalog.Get(model).Media.Select(category => category switch
    {
        MediaCategory.Floppy => new EmulationMediaDevice(EmulationMediaSlot.Floppy0, EmulationMediaType.Floppy, MediaConstants.FloppyExtensions),
        MediaCategory.Cassette => new EmulationMediaDevice(EmulationMediaSlot.Cassette0, EmulationMediaType.Cassette, MediaConstants.CassetteExtensions),
        MediaCategory.HardDisk => new EmulationMediaDevice(EmulationMediaSlot.HardDisk0, EmulationMediaType.HardDisk, MediaConstants.HardDiskExtensions, IsRemovable: false),
        MediaCategory.Cartridge => new EmulationMediaDevice(EmulationMediaSlot.Cartridge0, EmulationMediaType.Cartridge, MediaConstants.CartridgeExtensions),
        MediaCategory.MemoryCard => new EmulationMediaDevice(EmulationMediaSlot.MemoryCard0, EmulationMediaType.MemoryCard, MediaConstants.MemoryCardExtensions),
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    }).ToArray();

    /// <summary>Projette les lecteurs et montages de la configuration vers le contrat de réglages.</summary>
    /// <param name="configuration">Configuration courante du profil.</param>
    /// <returns>Descriptions des lecteurs et contenus, sans ouvrir leurs fichiers.</returns>
    internal static EmulationStorageSettings Describe(MachineConfiguration configuration)
    {
        var adapter = new Engine().SelectedAdapter(configuration);
        var devices = adapter?.DescribeMediaDevices(configuration) ?? DescribeDevices(configuration.Model);
        return new(devices, devices.Select(item => item.Slot).ToArray(), configuration.Media.Select(EmulationMediaConversionFunctions.ToCommon).ToArray(), adapter?.DescribeDeviceSettings(configuration) ?? []);
    }

    /// <summary>Applique les montages et les paramètres de lecteurs après contrôle de leur compatibilité.</summary>
    /// <param name="configuration">Configuration d’origine à conserver immuable.</param>
    /// <param name="settings">Choix transmis par l’éditeur de stockage de l’hôte.</param>
    /// <returns>Configuration mise à jour ; les médias utilisateur restent empruntés.</returns>
    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationStorageSettings settings)
    {
        var devices = Describe(configuration).AvailableDevices;
        if (settings.ConfiguredSlots.Distinct().Count() != settings.ConfiguredSlots.Count
            || settings.ConfiguredSlots.Any(slot => !devices.Any(device => device.Slot == slot))
            || settings.MountedMedia.Any(media => !settings.ConfiguredSlots.Contains(media.Slot)
                || !devices.Any(device => device.Slot == media.Slot && device.MediaType == media.Type
                    && device.AcceptedExtensions.Contains(Path.GetExtension(media.Path), StringComparer.OrdinalIgnoreCase))))
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(settings));
        var deviceSettings = settings.DeviceSettings ?? [];
        if (deviceSettings.Select(item => item.Slot).Distinct().Count() != deviceSettings.Count
            || deviceSettings.Any(item => !settings.ConfiguredSlots.Contains(item.Slot)))
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(settings));
        var result = new Engine().SelectedAdapter(configuration)?.ApplyDeviceSettings(configuration, deviceSettings) ?? configuration;
        result = result with { Media = settings.MountedMedia.Select(media => EmulationMediaConversionFunctions.ToModule(media, configuration.Media)).ToArray() };
        ConfigurationFunctions.Validate(result);
        return result;
    }
}
