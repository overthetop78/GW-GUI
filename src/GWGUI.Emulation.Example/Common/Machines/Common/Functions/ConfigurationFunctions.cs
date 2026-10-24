using System.IO;

namespace GWGUI.Emulation.Example.Common.Machines.Common.Functions;

/// <summary>Validation des identités, des montages et des associations des deux machines fictives.</summary>
/// <remarks>Les contrôles descriptifs ne lisent pas les fichiers. Leur présence est contrôlée séparément
/// au moment d’enregistrer ou de préparer le runtime, afin qu’une configuration puisse être éditée hors session.</remarks>
internal static class ConfigurationFunctions
{
    /// <summary>Indique si le catalogue propose un cœur compatible avec ce profil.</summary>
    /// <param name="model">Profil matériel déclaré par le module.</param>
    /// <returns>True si au moins une définition de cœur accepte son identifiant.</returns>
    internal static bool SupportsRuntime(MachineModel model) => EmulatorCatalog.GetAll(HardwareModelCatalog.Get(model).StableModelId).Count > 0;

    /// <summary>Retourne la préférence déclarée dans le catalogue, sans modifier une configuration existante.</summary>
    /// <param name="model">Profil dont le cœur initial est demandé.</param>
    /// <returns>Identité du premier cœur recommandé et compatible.</returns>
    internal static Emulator DefaultEmulator(MachineModel model) => EmulatorCatalog.DefaultEmulator(model);

    /// <summary>Décrit la sélection initiale de lecteurs du profil matériel.</summary>
    /// <param name="model">Profil dont les emplacements disponibles doivent être activés initialement.</param>
    /// <returns>Nouvelle liste protégée contre les modifications, dans l’ordre du catalogue matériel.</returns>
    /// <remarks>La création d’une configuration et le changement de profil utilisent cette même sélection.
    /// Les choix explicites d’une configuration restent distincts de ces valeurs initiales.</remarks>
    internal static IReadOnlyList<EmulationMediaSlot> DefaultSlots(MachineModel model) => Array.AsReadOnly(StorageSettingsFunctions.DescribeDevices(model).Select(device => device.Slot).ToArray());

    /// <summary>Calcule la quantité de ports de jeu du profil.</summary>
    /// <param name="model">Profil matériel dont les ports sont décrits.</param>
    /// <returns>Nombre de ports acceptant les contrôleurs configurables.</returns>
    internal static int ControllerPortCount(MachineModel model) => HardwareModelCatalog.Get(model).Ports.Where(item => item.Capability == HardwarePortCapability.AnalogJoystick).Sum(item => item.Count);

    /// <summary>Résout les ports utilisables par la session, y compris l’extension activée dans les entrées.</summary>
    /// <param name="configuration">Configuration empruntée, avec son modèle et son choix d’adaptateur de ports.</param>
    /// <returns>Quatre ports avec l’extension ; les ports matériels du modèle dans le cas contraire.</returns>
    /// <remarks>Les descriptions, les validations de commandes et les trois simulations utilisent ce même calcul.
    /// Le catalogue matériel conserve son nombre de ports de base ; l’extension est un choix de session.</remarks>
    internal static int ControllerPortCount(MachineConfiguration configuration) => configuration.Input.ParallelJoystickAdapterEnabled ? ControllerPortConstants.FourControllerPorts : ControllerPortCount(configuration.Model);

    /// <summary>Contrôle les données du module sans démarrer ni installer l’émulateur sélectionné.</summary>
    /// <param name="configuration">Configuration empruntée à vérifier.</param>
    /// <exception cref="ArgumentException">Un type de contenu, un emplacement ou une association est incompatible.</exception>
    internal static void Validate(MachineConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        if (configuration.SchemaVersion != ConfigurationStoreConstants.CurrentSchemaVersion || configuration.Id == Guid.Empty
            || !Enum.IsDefined(configuration.Model) || !Enum.IsDefined(configuration.Core))
            throw new ArgumentException(nameof(configuration));
        var model = HardwareModelCatalog.Get(configuration.Model);
        var compatibility = ModelCatalog.GetCompatibility(configuration);
        if (configuration.Firmwares.Any(firmware => !compatibility.Firmware.Contains(firmware.Category) || string.IsNullOrWhiteSpace(firmware.Path))) throw new ArgumentException(ErrorMessages.IncompatibleFirmware, nameof(configuration));
        if (configuration.Core != Emulator.None && !EmulatorCatalog.Get(configuration.Core).MachineIds.Contains(model.StableModelId))
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(configuration));
        var devices = StorageSettingsFunctions.DescribeDevices(configuration.Model);
        if (configuration.ConfiguredSlots is null || configuration.ConfiguredSlots.Distinct().Count() != configuration.ConfiguredSlots.Count
            || configuration.ConfiguredSlots.Any(slot => !devices.Any(device => device.Slot == slot)))
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(configuration));
        var slots = new HashSet<EmulationMediaSlot>();
        foreach (var media in configuration.Media)
        {
            if (string.IsNullOrWhiteSpace(media.Path) || !compatibility.Media.Any(rule => rule.Category == media.Category && rule.Availability == MediaAvailability.Available && rule.Slots.Contains(media.Slot)) || !slots.Add(media.Slot) || !configuration.ConfiguredSlots.Contains(media.Slot))
                throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(configuration));
            // Les valeurs automatiques restent null. Les identifiants de format sont des données du backend.
            if (media.StorageBus is { } bus && !Enum.IsDefined(bus)
                || media.CartridgePlatform is { } platform && !Enum.IsDefined(platform)
                || media.CartridgeRegion is { } region && !Enum.IsDefined(region))
                throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(configuration));
        }
        var ports = new HashSet<int>();
        foreach (var binding in configuration.Input.Controllers ?? [])
        {
            // Les associations de l’extension restent mémorisées lorsqu’elle est désactivée.
            // Le nombre effectif de ports limite séparément les commandes et les lectures de session.
            if (binding.Port < 0 || binding.Port >= Math.Max(compatibility.ControllerPortCount, ControllerPortConstants.FourControllerPorts) || !ports.Add(binding.Port)
                || !Enum.IsDefined(binding.Peripheral) || binding.DeadZonePercent is < 0 or > 100)
                throw new ArgumentException(ErrorMessages.InvalidControllerPort, nameof(configuration));
            foreach (var output in binding.OutputBindings ?? new Dictionary<string, EmulationControllerOutputBinding>())
                if (string.IsNullOrWhiteSpace(output.Key) || string.IsNullOrWhiteSpace(output.Value.DeviceId) || !Enum.IsDefined(output.Value.Motor)) throw new ArgumentException(nameof(configuration));
        }
        if (configuration.Core != Emulator.None) new Engine().Adapter(configuration).ValidateOptions(configuration);
    }

    /// <summary>Ajoute les contrôles nécessaires à une configuration persistante utilisable.</summary>
    /// <param name="configuration">Configuration à enregistrer sous le verrou du magasin.</param>
    /// <remarks>Le choix du cœur doit être explicite et chaque contenu inséré doit être accessible.
    /// Les contenus éjectés conservent leur chemin sans imposer leur présence au moment d’enregistrer.</remarks>
    internal static void ValidateForSave(MachineConfiguration configuration)
    {
        Validate(configuration);
        if (configuration.Core == Emulator.None) throw new InvalidDataException();
        foreach (var media in configuration.Media.Where(item => item.IsInserted))
            if (!File.Exists(media.Path) && !Directory.Exists(media.Path)) throw new FileNotFoundException(media.Path);
        foreach (var firmware in configuration.Firmwares.Where(item => item.IsRequired))
            if (!File.Exists(firmware.Path)) throw new FileNotFoundException(firmware.Path);
    }
}
