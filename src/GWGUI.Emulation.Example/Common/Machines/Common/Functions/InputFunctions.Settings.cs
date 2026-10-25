using GWGUI.Emulation;
using CommonControllerFunctions = GWGUI.Emulation.Example.Common.Functions.ControllerFunctions;
using System.Reflection;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Example.Common.Functions;

namespace GWGUI.Emulation.Example.Common.Machines.Common.Functions;

/// <summary>Descriptions et associations d’un périphérique générique par catégorie prise en charge.</summary>
/// <remarks>Le catalogue reprend les commandes du contrat commun et toutes les touches de son énumération.
/// Il ne catalogue pas de modèles physiques : l’application reste propriétaire des visuels et des sources physiques.</remarks>
internal static class InputSettingsFunctions
{
    /// <summary>Identifiants de commandes déjà définis par le SDK, sans seconde copie de leurs valeurs.</summary>
    private static readonly IReadOnlyList<string> ControllerCommands = Array.AsReadOnly(typeof(EmulationControllerCommandIds)
        .GetFields(BindingFlags.Public | BindingFlags.Static).Where(field => field.IsLiteral && field.FieldType == typeof(string))
        .Select(field => (string)field.GetRawConstantValue()!).Distinct(StringComparer.Ordinal).ToArray());

    /// <summary>Commandes de boutons et molettes disponibles dans le contrat de pointeur.</summary>
    private static readonly IReadOnlyList<string> MouseCommands = Array.AsReadOnly<string>(["Left", "Right", "Middle", "XButton1", "XButton2", "WheelUp", "WheelDown", "WheelLeft", "WheelRight"]);

    /// <summary>Définitions communes au clavier principal et au clavier branché sur un port.</summary>
    /// <remarks>La table native existante fournit les touches réellement représentables. Les deux chemins
    /// utilisent ainsi les mêmes identifiants, libellés et associations, sans catalogue parallèle de touches.</remarks>
    private static readonly IReadOnlyList<InputBindingDefinition> KeyboardCommands = Array.AsReadOnly(CommonControllerFunctions.CreateNativeKeyMap().Keys
        .OrderBy(key => key).Select(key => new InputBindingDefinition(key.ToString(), "Emulation.Keyboard", key.ToString(), key.ToString())).ToArray());

    /// <summary>Décrit toutes les touches représentables par le contrat de clavier.</summary>
    /// <param name="configuration">Configuration consultée par les mêmes interfaces que les autres adaptateurs.</param>
    /// <returns>Une action par touche définie, à l’exception de la valeur Unknown.</returns>
    internal static IReadOnlyList<InputBindingDefinition> KeyboardDefinitions(MachineConfiguration configuration) => KeyboardCommands;

    /// <summary>Décrit les commandes génériques du périphérique sélectionné.</summary>
    /// <param name="peripheral">Catégorie choisie sur le port ; None ne possède aucune commande.</param>
    /// <returns>Commandes du contrat commun, sans association physique imposée par le module.</returns>
    /// <remarks>Les directions LeftStick transportent les axes LeftX/LeftY : une position absolue pour le
    /// pistolet ou le stylet, une direction pour le volant et le joystick. Les directions RightStick restent
    /// disponibles pour le second couple d’axes, et L2/R2 portent les pressions des pédales ou gâchettes.
    /// Le lecteur natif du pistolet interroge B pour tirer, A/X/Y pour les auxiliaires, puis Start, Select,
    /// les directions et Reload. Le stylet interroge A pour son contact. Les autres commandes restent dans
    /// Controls pour les adaptations qui les utilisent : présenter une commande conserve son identité,
    /// même lorsqu’elle ne possède pas de numéro de bouton dans une classe native particulière.</remarks>
    internal static IReadOnlyList<InputBindingDefinition> ControllerDefinitions(PeripheralCategory peripheral)
    {
        if (peripheral == PeripheralCategory.None) return [];
        if (peripheral == PeripheralCategory.Keyboard) return KeyboardCommands;
        var commands = peripheral == PeripheralCategory.Mouse ? MouseCommands : ControllerCommands;
        return commands.Select(command => new InputBindingDefinition(command, "Emulation.Controller.Action." + command, string.Empty, command)).ToArray();
    }

    /// <summary>Projette les associations enregistrées vers l’éditeur d’entrées public.</summary>
    /// <param name="configuration">Configuration du profil et de ses deux ports de jeu.</param>
    /// <returns>Clavier complet, souris et ports présentant chacun les catégories génériques.</returns>
    internal static EmulationInputSettings Describe(MachineConfiguration configuration)
    {
        var input = configuration.Input;
        var adapter = new Engine().SelectedAdapter(configuration);
        var usbPorts = adapter?.GetUsbControllerPorts(configuration) ?? [];
        var sources = EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller;
        var keyboard = new EmulationInputBindingSet((new Engine().SelectedAdapter(configuration)?.GetKeyboardDefinitions(configuration) ?? KeyboardDefinitions(configuration)), input.KeyboardBindings
            ?? input.KeyboardMappings?.ToDictionary(item => item.Key, item => item.Value.ToString(), StringComparer.Ordinal) ?? new Dictionary<string, string>(), sources);
        var mouse = new EmulationInputBindingSet(ControllerDefinitions(PeripheralCategory.Mouse), input.MouseMappings ?? new Dictionary<string, string>(), sources, true);
        // Le joystick analogique présente à lui seul directions, axes et boutons ; sa variante
        // numérique reste un type de protocole, sans créer un second exemple dans l'éditeur.
        var choices = Enum.GetValues<PeripheralCategory>().Where(item => item is not (PeripheralCategory.Automatic or PeripheralCategory.Joystick)).Select(Choice).ToArray();
        var ports = Enumerable.Range(0, CommonControllerFunctions.PortCount(configuration)).Select(index =>
        {
            var binding = input.Controllers?.FirstOrDefault(item => item.Port == index);
            var peripheral = binding?.Peripheral ?? PeripheralCategory.RetroPad;
            if (peripheral == PeripheralCategory.Automatic) peripheral = PeripheralCategory.RetroPad;
            peripheral = adapter?.ResolveControllerPeripheral(configuration, index, peripheral) ?? peripheral;
            return new EmulationControllerPort(index + 1, choices, peripheral.ToString(), binding?.DeviceId,
                new EmulationInputBindingSet(ControllerDefinitions(peripheral), binding?.Mappings ?? new Dictionary<string, string>(), sources, true),
                binding?.DeadZonePercent ?? ControllerConstants.DefaultDeadZonePercent, binding?.VisualId)
            { IsUsb = usbPorts.Contains(index), OutputBindings = binding?.OutputBindings ?? new Dictionary<string, EmulationControllerOutputBinding>() };
        }).ToArray();
        return new(keyboard, mouse, ports);
    }

    /// <summary>Valide les expressions de sources puis conserve les choix de l’éditeur.</summary>
    /// <param name="configuration">Configuration d’origine, conservée immuable.</param>
    /// <param name="settings">Associations proposées par l’hôte.</param>
    /// <returns>Configuration validée ; aucun périphérique physique n’est ouvert.</returns>
    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationInputSettings settings)
    {
        var bindings = new List<IReadOnlyDictionary<string, string>>();
        if (settings.Keyboard is { } keyboard) bindings.Add(keyboard.Values);
        if (settings.Mouse is { } mouse) bindings.Add(mouse.Values);
        bindings.AddRange(settings.ControllerPorts.Select(port => port.Bindings.Values));
        if (bindings.SelectMany(values => values.Values).Any(value => !EmulationSdk.InputProcessing.IsValidBinding(value))) throw new ArgumentException(nameof(settings));
        var controllers = settings.ControllerPorts.Select(port =>
        {
            if (!Enum.TryParse<PeripheralCategory>(port.SelectedControllerId, out var peripheral) || !Enum.IsDefined(peripheral)) throw new ArgumentException(nameof(settings));
            return new ControllerBinding(port.Number - 1, peripheral, port.PhysicalDeviceId, new Dictionary<string, string>(port.Bindings.Values), port.DeadZonePercent, port.VisualId,
                new Dictionary<string, EmulationControllerOutputBinding>(port.OutputBindings));
        }).ToArray();
        var result = configuration with { Input = configuration.Input with
        {
            KeyboardBindings = settings.Keyboard is null ? configuration.Input.KeyboardBindings : new Dictionary<string, string>(settings.Keyboard.Values),
            MouseMappings = settings.Mouse is null ? configuration.Input.MouseMappings : new Dictionary<string, string>(settings.Mouse.Values),
            Controllers = controllers
        } };
        ConfigurationFunctions.Validate(result);
        return new Engine().SelectedAdapter(result)?.ApplyInputConfiguration(result) ?? result;
    }

    /// <summary>Indique si le profil expose un port de pointeur.</summary>
    /// <param name="configuration">Configuration dont les capacités matérielles sont consultées.</param>
    /// <returns>True pour les deux profils de démonstration possédant ce port.</returns>
    internal static bool SupportsMouse(MachineConfiguration configuration) => HardwareModelCatalog.Get(configuration.Model).Ports.Any(port => port.Capability == HardwarePortCapability.Mouse && port.Count > 0);

    /// <summary>Indique si les choix d’entrée nécessitent la capture du pointeur.</summary>
    /// <param name="input">Choix persistants des sources et périphériques.</param>
    /// <returns>True pour une capture demandée ou un périphérique de pointage sélectionné.</returns>
    internal static bool SupportsPointerCapture(InputConfiguration input) => input.CaptureMouse || input.Controllers?.Any(binding => binding.Peripheral is PeripheralCategory.Mouse or PeripheralCategory.LightGun or PeripheralCategory.Pointer) == true;

    /// <summary>Classe une catégorie générique pour que l’application choisisse sa présentation.</summary>
    /// <param name="peripheral">Catégorie du module à décrire, indépendante d’un produit physique.</param>
    /// <returns>Choix public avec ses commandes et sa famille de présentation.</returns>
    private static EmulationControllerChoice Choice(PeripheralCategory peripheral) => new(peripheral.ToString(), "Emulation.Controller." + peripheral, peripheral.ToString(), BindingDefinitions: ControllerDefinitions(peripheral))
    {
        Category = peripheral switch
        {
            PeripheralCategory.None => EmulationControllerCategory.None,
            PeripheralCategory.RetroPad => EmulationControllerCategory.Gamepad,
            PeripheralCategory.Joystick or PeripheralCategory.AnalogJoystick or PeripheralCategory.Paddle => EmulationControllerCategory.Joystick,
            PeripheralCategory.Mouse or PeripheralCategory.LightGun or PeripheralCategory.Pointer => EmulationControllerCategory.PointingDevice,
            PeripheralCategory.DrivingController => EmulationControllerCategory.RacingSimulation,
            PeripheralCategory.DanceMat => EmulationControllerCategory.DanceSimulation,
            _ => EmulationControllerCategory.OtherSimulation
        }
    };
}
