namespace GWGUI.Emulation.Example.Common.Machines.Common.Functions;

/// <summary>Adaptation des paramètres de lecteurs entre le contrat public et les options persistantes du module.</summary>
/// <remarks>Les clés reprennent le rangement par emplacement des réglages de stockage existants. Une seule
/// configuration conserve les choix, même sans média inséré ; aucune collection de lecteurs parallèle n’est créée.
/// Ces opérations décrivent une simulation : elles n’ouvrent pas les médias et ne modifient aucun fichier utilisateur.</remarks>
internal static partial class StorageSettingsFunctions
{
    /// <summary>Projette les réglages explicitement enregistrés des lecteurs configurés.</summary>
    /// <param name="configuration">Configuration empruntée ; ses options restent inchangées.</param>
    /// <returns>Paramètres indépendants des collections d’origine ; les lecteurs sans choix explicite utilisent les valeurs du dialogue.</returns>
    /// <remarks>Un réglage reste distinct du montage d’un contenu. Le chemin associé à une carte mémoire
    /// est donc conservé même lorsque cette carte n’est pas montée dans le runtime.</remarks>
    internal static IReadOnlyList<EmulationStorageDeviceSettings> DescribeDeviceSettings(MachineConfiguration configuration)
    {
        var result = new List<EmulationStorageDeviceSettings>();
        foreach (var device in DescribeDevices(configuration.Model).Where(device => configuration.ConfiguredSlots.Contains(device.Slot)))
        {
            var slot = device.Slot;
            var options = ReadDeviceOptions(configuration.Options, slot);
            var model = configuration.Options.GetValueOrDefault(StorageSettingsFunctionsConstants.StorageModel + slot);
            var speed = configuration.Options.GetValueOrDefault(StorageSettingsFunctionsConstants.StorageSpeed + slot);
            var writeProtected = configuration.Options.GetValueOrDefault(StorageSettingsFunctionsConstants.StorageWriteProtected + slot);
            var redirectWrites = configuration.Options.GetValueOrDefault(StorageSettingsFunctionsConstants.StorageRedirectWrites + slot);
            var selectedInterface = configuration.Options.GetValueOrDefault(StorageSettingsFunctionsConstants.StorageInterface + slot);
            var cartridge = configuration.Options.GetValueOrDefault(StorageSettingsFunctionsConstants.CartridgeOptionPrefix + slot);
            var memoryCardPath = configuration.Options.GetValueOrDefault(StorageSettingsFunctionsConstants.MemoryCardPathOptionPrefix + slot.Index);
            if (device.MediaType == EmulationMediaType.Floppy && (model is not null || speed is not null || writeProtected is not null || redirectWrites is not null || options.Count > 0))
                result.Add(new(slot, Floppy: new(model ?? StorageSettingsFunctionsConstants.FloppyModel, speed ?? StorageSettingsFunctionsConstants.DefaultFloppySpeed, bool.TryParse(writeProtected, out var protect) && protect, bool.TryParse(redirectWrites, out var redirect) && redirect, options)));
            else if (device.MediaType == EmulationMediaType.HardDisk && (selectedInterface is not null || options.Count > 0))
                result.Add(new(slot, InterfaceId: selectedInterface, Options: options));
            else if (device.MediaType == EmulationMediaType.Cartridge && (cartridge is not null || options.Count > 0))
                result.Add(new(slot, Cartridge: new(bool.TryParse(cartridge, out var enabled) && enabled, options)));
            else if (device.MediaType == EmulationMediaType.MemoryCard && (memoryCardPath is not null || options.Count > 0))
                result.Add(new(slot, MemoryCardPath: memoryCardPath, Options: options));
            else if (device.MediaType == EmulationMediaType.Cassette && options.Count > 0)
                result.Add(new(slot, Options: options));
        }
        return result;
    }

    /// <summary>Remplace les paramètres de lecteurs et conserve les autres options du cœur.</summary>
    /// <param name="configuration">Configuration immuable décrivant les lecteurs sélectionnés.</param>
    /// <param name="settings">Paramètres complets ; une liste vide retire les choix explicites des lecteurs.</param>
    /// <returns>Nouvelle configuration dont les dictionnaires ne dépendent plus de ceux de l’appelant.</returns>
    /// <exception cref="ArgumentException">Un emplacement, un modèle, une vitesse, une interface ou une association de paramètres est incompatible.</exception>
    /// <remarks>Le modèle de disquette et sa vitesse sont validés contre les choix présentés. Les options
    /// supplémentaires restent des données du backend ; des clés contradictoires sont refusées. La protection
    /// et la redirection sont transmises comme choix de la simulation, sans écriture dans les contenus empruntés.</remarks>
    internal static MachineConfiguration ApplyDeviceSettings(MachineConfiguration configuration, IReadOnlyList<EmulationStorageDeviceSettings> settings)
    {
        var devices = DescribeDevices(configuration.Model).ToDictionary(device => device.Slot);
        var options = configuration.Options.Where(option => !IsDeviceOption(option.Key)).ToDictionary(option => option.Key, option => option.Value, StringComparer.Ordinal);
        var slots = new HashSet<EmulationMediaSlot>();
        foreach (var setting in settings)
        {
            if (!slots.Add(setting.Slot) || !configuration.ConfiguredSlots.Contains(setting.Slot) || !devices.TryGetValue(setting.Slot, out var device)
                || setting.Floppy is not null && device.MediaType != EmulationMediaType.Floppy
                || setting.Cartridge is not null && device.MediaType != EmulationMediaType.Cartridge
                || setting.InterfaceId is not null && device.MediaType != EmulationMediaType.HardDisk
                || setting.MemoryCardPath is not null && device.MediaType != EmulationMediaType.MemoryCard || setting.CompactDisc is not null)
                throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(settings));
            if (setting.Floppy is { } floppy)
            {
                if (floppy.Model != StorageSettingsFunctionsConstants.FloppyModel || !StorageSettingsFunctionsConstants.FloppySpeeds.Contains(floppy.Speed))
                    throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(settings));
                options[StorageSettingsFunctionsConstants.StorageModel + setting.Slot] = floppy.Model;
                options[StorageSettingsFunctionsConstants.StorageSpeed + setting.Slot] = floppy.Speed;
                options[StorageSettingsFunctionsConstants.StorageWriteProtected + setting.Slot] = floppy.WriteProtected.ToString();
                options[StorageSettingsFunctionsConstants.StorageRedirectWrites + setting.Slot] = floppy.RedirectWrites.ToString();
            }
            if (setting.InterfaceId is { } selectedInterface)
            {
                if (!string.Equals(selectedInterface, StorageSettingsFunctionsConstants.InterfaceName, StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(settings));
                options[StorageSettingsFunctionsConstants.StorageInterface + setting.Slot] = StorageSettingsFunctionsConstants.InterfaceName;
            }
            if (setting.Cartridge is { } cartridge) options[StorageSettingsFunctionsConstants.CartridgeOptionPrefix + setting.Slot] = cartridge.OptionEnabled.ToString();
            if (!string.IsNullOrWhiteSpace(setting.MemoryCardPath)) options[StorageSettingsFunctionsConstants.MemoryCardPathOptionPrefix + setting.Slot.Index] = setting.MemoryCardPath;
            foreach (var option in (setting.Options ?? new Dictionary<string, string>()).Concat(setting.Floppy?.Options ?? new Dictionary<string, string>()).Concat(setting.Cartridge?.Options ?? new Dictionary<string, string>()))
            {
                var key = StorageSettingsFunctionsConstants.DeviceOptionsPrefix + setting.Slot + "." + option.Key;
                if (string.IsNullOrWhiteSpace(option.Key) || options.TryGetValue(key, out var previous) && previous != option.Value)
                    throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(settings));
                options[key] = option.Value;
            }
        }
        return configuration with { Options = options };
    }

    /// <summary>Extrait une copie des options supplémentaires d’un seul emplacement.</summary>
    /// <param name="options">Dictionnaire persistant emprunté.</param>
    /// <param name="slot">Identité complète du lecteur, sans rapprochement par simple indice.</param>
    /// <returns>Clés natives privées de leur préfixe de stockage ; leurs valeurs restent inchangées.</returns>
    private static IReadOnlyDictionary<string, string> ReadDeviceOptions(IReadOnlyDictionary<string, string> options, EmulationMediaSlot slot)
    {
        var prefix = StorageSettingsFunctionsConstants.DeviceOptionsPrefix + slot + ".";
        return options.Where(option => option.Key.StartsWith(prefix, StringComparison.Ordinal)).ToDictionary(option => option.Key[prefix.Length..], option => option.Value, StringComparer.Ordinal);
    }

    /// <summary>Reconnaît les clés appartenant aux paramètres de lecteurs lors de leur remplacement complet.</summary>
    /// <param name="key">Clé d’option dont le domaine est contrôlé.</param>
    /// <returns>True pour les champs de stockage gérés ici ; les autres options du cœur sont conservées.</returns>
    private static bool IsDeviceOption(string key) => key.StartsWith(StorageSettingsFunctionsConstants.StorageModel, StringComparison.Ordinal)
        || key.StartsWith(StorageSettingsFunctionsConstants.StorageSpeed, StringComparison.Ordinal) || key.StartsWith(StorageSettingsFunctionsConstants.StorageWriteProtected, StringComparison.Ordinal)
        || key.StartsWith(StorageSettingsFunctionsConstants.StorageRedirectWrites, StringComparison.Ordinal) || key.StartsWith(StorageSettingsFunctionsConstants.StorageInterface, StringComparison.Ordinal)
        || key.StartsWith(StorageSettingsFunctionsConstants.CartridgeOptionPrefix, StringComparison.Ordinal) || key.StartsWith(StorageSettingsFunctionsConstants.MemoryCardPathOptionPrefix, StringComparison.Ordinal)
        || key.StartsWith(StorageSettingsFunctionsConstants.DeviceOptionsPrefix, StringComparison.Ordinal);
}
