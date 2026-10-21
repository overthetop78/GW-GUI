
namespace GWGUI.Emulation.Example.Common.Machines.Common.Functions;

/// <summary>Résolution des capacités et validation structurelle d’une configuration de machine.</summary>
/// <remarks>Le catalogue matériel définit les ports ; les adaptateurs définissent les backends et lecteurs.
/// La validation conserve la configuration reçue et signale les incohérences par les diagnostics du module.</remarks>
internal static class ConfigurationFunctions
{
    /// <summary>Indique si au moins un adaptateur du catalogue déclare prendre en charge le modèle.</summary>
    /// <param name="model">Modèle dont le nom technique est comparé aux identifiants acceptés par les adaptateurs.</param>
    /// <returns>True lorsqu’une description compatible existe ; cette présence ne vérifie pas l’installation du cœur.</returns>
    internal static bool SupportsRuntime(MachineModel model) => EmulatorCatalog.GetAll(model.ToString()).Count > 0;

    /// <summary>Compte les ports de joysticks analogiques déclarés dans le matériel du modèle.</summary>
    /// <param name="model">Modèle à rechercher dans le catalogue matériel.</param>
    /// <returns>Somme des quantités de ports analogiques, sans compter les ports clavier.</returns>
    internal static int ControllerPortCount(MachineModel model) => HardwareModelCatalog.Get(model).Ports.Where(port => port.Capability == HardwarePortCapability.AnalogJoystick).Sum(port => port.Count);

    /// <summary>Choisit le premier backend compatible dans l’ordre des identifiants du catalogue.</summary>
    /// <param name="model">Modèle déclaré par l’énumération du module.</param>
    /// <returns>Identité du backend sélectionné, ou Emulator.None lorsqu’aucun adaptateur ne correspond.</returns>
    /// <exception cref="ArgumentOutOfRangeException">La valeur de modèle n’est pas déclarée.</exception>
    internal static Emulator DefaultEmulator(MachineModel model)
    {
        if (!Enum.IsDefined(model)) throw new ArgumentOutOfRangeException(nameof(model));
        var definition = EmulatorCatalog.GetAll(model.ToString()).FirstOrDefault();
        return definition is null ? Emulator.None : Enum.Parse<Emulator>(new Engine().Adapter(definition.Id).EmulatorKey);
    }

    /// <summary>Contrôle schéma, modèle, backend, options, montages et associations de contrôleurs.</summary>
    /// <param name="configuration">Configuration empruntée, non modifiée par cette méthode.</param>
    /// <exception cref="ArgumentOutOfRangeException">Version de schéma, identité, port ou zone morte hors des valeurs admises.</exception>
    /// <exception cref="ArgumentException">Firmware externe, montage ou association de périphérique refusés.</exception>
    /// <remarks>Dans ce parcours, la liste de firmwares externes doit être vide. L’adaptateur contrôle ses options
    /// et décrit les lecteurs acceptés. Chaque média exige un chemin non vide et un slot unique compatible avec
    /// son type. Chaque association de contrôleur exige un port unique, une zone morte valide et une catégorie
    /// de périphérique admise par le modèle. Les fichiers des médias ne sont pas ouverts par ces contrôles de montage.</remarks>
    internal static void Validate(MachineConfiguration configuration)
    {
        if (configuration.SchemaVersion != ConfigurationStoreConstants.CurrentSchemaVersion)
            throw new ArgumentOutOfRangeException(nameof(configuration), ErrorMessages.UnsupportedSchema);
        if (!Enum.IsDefined(configuration.Model) || !Enum.IsDefined(configuration.Core)
            || configuration.Core != Emulator.None && !EmulatorCatalog.Get(configuration.Core).MachineIds.Contains(configuration.MachineId))
            throw new ArgumentOutOfRangeException(nameof(configuration));
        if (configuration.Firmwares.Count > 0)
            throw new ArgumentException(ErrorMessages.IncompatibleFirmware, nameof(configuration));
        if (configuration.Core != Emulator.None && new Engine().Adapter(configuration) is IEmulatorConfigurationOptions options)
            options.ValidateOptions(configuration);
        var devices = configuration.Core != Emulator.None && new Engine().Adapter(configuration) is IEmulatorStorageSettings storage ? storage.DescribeMediaDevices(configuration) : [];
        var mediaSlots = new HashSet<EmulationMediaSlot>();
        foreach (var media in configuration.Media)
        {
            if (string.IsNullOrWhiteSpace(media.Path))
                throw new ArgumentException(ErrorMessages.EmptyMediaPath, nameof(configuration));
            if (!mediaSlots.Add(media.Slot))
                throw new ArgumentException(ErrorMessages.DuplicateMediaSlot, nameof(configuration));
            var mounted = EmulationMediaConversionFunctions.ToCommon(media);
            if (!devices.Any(device => device.Slot == media.Slot && device.MediaType == mounted.Type))
                throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(configuration));
        }
        var ports = new HashSet<int>();
        foreach (var controller in configuration.Input.Controllers ?? [])
        {
            if (controller.Port < ControllerPortConstants.MinimumControllerPort
                || controller.Port >= ControllerPortCount(configuration.Model))
                throw new ArgumentOutOfRangeException(nameof(configuration), ErrorMessages.InvalidControllerPort);
            if (!ports.Add(controller.Port))
                throw new ArgumentException(ErrorMessages.DuplicateControllerPort, nameof(configuration));
            if (controller.DeadZonePercent is < ControllerConstants.MinimumDeadZonePercent
                or > ControllerConstants.MaximumDeadZonePercent)
                throw new ArgumentOutOfRangeException(nameof(configuration), ErrorMessages.InvalidControllerDeadZone);
            if (!ControllerFunctions.Peripherals(configuration.Model).Contains(controller.Peripheral))
                throw new ArgumentException(ErrorMessages.UnsupportedControllerDevice, nameof(configuration));
        }
    }
}
