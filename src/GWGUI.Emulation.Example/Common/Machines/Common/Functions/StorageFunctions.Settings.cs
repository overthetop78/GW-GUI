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
        MediaCategory.HardDisk => new EmulationMediaDevice(EmulationMediaSlot.HardDisk0, EmulationMediaType.HardDisk, MediaConstants.HardDiskExtensions, IsRemovable: false, HardDiskFormats: HardDiskFormats.For(model), ConfigurationKind: EmulationStorageConfigurationKind.HardDiskDrive),
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
        return new(devices, configuration.ConfiguredSlots.ToArray(), configuration.Media.Select(EmulationMediaConversionFunctions.ToCommon).ToArray(), adapter?.DescribeDeviceSettings(configuration) ?? []);
    }

    /// <summary>Contrôle la sélection de lecteurs et la conserve dans une copie de la configuration.</summary>
    /// <param name="configuration">Configuration du profil dont les lecteurs disponibles sont consultés.</param>
    /// <param name="slots">Emplacements choisis ; une liste vide désactive tous les lecteurs configurables.</param>
    /// <returns>Configuration dont la sélection est copiée, indépendante de la liste fournie par l’appelant.</returns>
    /// <remarks>Les contrôles de montages appartiennent au traitement du stockage complet, qui peut retirer
    /// les médias des lecteurs désactivés avant de valider la configuration résultante.</remarks>
    internal static MachineConfiguration ApplyStorageConfiguration(MachineConfiguration configuration, IReadOnlyList<EmulationMediaSlot> slots)
    {
        var devices = Describe(configuration).AvailableDevices;
        if (slots.Distinct().Count() != slots.Count || slots.Any(slot => !devices.Any(device => device.Slot == slot)))
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(slots));
        return configuration with { ConfiguredSlots = Array.AsReadOnly(slots.ToArray()) };
    }

    /// <summary>Applique les montages et les paramètres de lecteurs après contrôle de leur compatibilité.</summary>
    /// <param name="configuration">Configuration d’origine à conserver immuable.</param>
    /// <param name="settings">Choix transmis par l’éditeur de stockage de l’hôte.</param>
    /// <returns>Configuration mise à jour ; les médias utilisateur restent empruntés.</returns>
    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationStorageSettings settings)
    {
        var adapter = new Engine().SelectedAdapter(configuration);
        var result = adapter?.ApplyStorageConfiguration(configuration, settings.ConfiguredSlots) ?? ApplyStorageConfiguration(configuration, settings.ConfiguredSlots);
        var devices = Describe(result).AvailableDevices;
        if (settings.MountedMedia.Any(media => !result.ConfiguredSlots.Contains(media.Slot)
                || !devices.Any(device => device.Slot == media.Slot && device.MediaType == media.Type
                    && device.AcceptedExtensions.Contains(Path.GetExtension(media.Path), StringComparer.OrdinalIgnoreCase))))
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(settings));
        var deviceSettings = settings.DeviceSettings ?? [];
        if (deviceSettings.Select(item => item.Slot).Distinct().Count() != deviceSettings.Count
            || deviceSettings.Any(item => !result.ConfiguredSlots.Contains(item.Slot)))
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(settings));
        result = adapter?.ApplyDeviceSettings(result, deviceSettings) ?? result;
        result = result with { Media = settings.MountedMedia.Select(media => EmulationMediaConversionFunctions.ToModule(media, result.Media)).ToArray() };
        ConfigurationFunctions.Validate(result);
        return result;
    }
}
