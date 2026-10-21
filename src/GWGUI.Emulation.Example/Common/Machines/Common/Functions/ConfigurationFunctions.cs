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

    /// <summary>Calcule la quantité de ports de jeu du profil.</summary>
    /// <param name="model">Profil matériel dont les ports sont décrits.</param>
    /// <returns>Nombre de ports acceptant les contrôleurs configurables.</returns>
    internal static int ControllerPortCount(MachineModel model) => HardwareModelCatalog.Get(model).Ports.Where(item => item.Capability == HardwarePortCapability.AnalogJoystick).Sum(item => item.Count);

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
        if (configuration.Firmwares.Any(firmware => !model.Firmware.Contains(firmware.Category) || string.IsNullOrWhiteSpace(firmware.Path))) throw new ArgumentException(ErrorMessages.IncompatibleFirmware, nameof(configuration));
        if (configuration.Core != Emulator.None && !EmulatorCatalog.Get(configuration.Core).MachineIds.Contains(model.StableModelId))
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(configuration));
        var slots = new HashSet<EmulationMediaSlot>();
        foreach (var media in configuration.Media)
        {
            if (string.IsNullOrWhiteSpace(media.Path) || !model.Media.Contains(media.Category) || !slots.Add(media.Slot)
                || !StorageSettingsFunctions.DescribeDevices(configuration.Model).Any(device => device.Slot == media.Slot && device.MediaType == EmulationMediaConversionFunctions.ToType(media.Category)))
                throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(configuration));
        }
        var ports = new HashSet<int>();
        foreach (var binding in configuration.Input.Controllers ?? [])
        {
            if (binding.Port < 0 || binding.Port >= ControllerPortCount(configuration.Model) || !ports.Add(binding.Port)
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
