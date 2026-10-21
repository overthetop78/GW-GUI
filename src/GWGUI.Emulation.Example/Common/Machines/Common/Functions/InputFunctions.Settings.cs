using GWGUI.Emulation.Example.Common.Machines.ExampleMachineA.Functions;

namespace GWGUI.Emulation.Example.Common.Machines.Common.Functions;
/// <summary>Description et application des associations d’entrées présentées par les contrats de l’hôte.</summary>
/// <remarks>Les ports sont numérotés à partir de un dans la présentation et indexés à partir de zéro dans
/// ControllerBinding. Les associations sont conservées comme données de configuration ; leur projection
/// sur les instantanés physiques appartient à InputSnapshotFunctions.</remarks>
internal static class InputSettingsFunctions
{
    /// <summary>Décrit les touches particulières et les ports de contrôleur configurables du modèle.</summary>
    /// <param name="configuration">Configuration empruntée, utilisée pour les associations et valeurs courantes.</param>
    /// <returns>Réglages de clavier et liste des ports ; aucune section de souris distincte n’est créée ici.</returns>
    /// <remarks>Les sources clavier, souris et manette peuvent alimenter les associations. Le choix automatique
    /// d’un port est présenté comme un joystick analogique. Les choix proposés viennent des capacités du modèle.</remarks>
    internal static EmulationInputSettings Describe(MachineConfiguration configuration)
    {
        var input = configuration.Input;
        var definitions = ExampleMachineAInputSettingsFunctions.KeyboardDefinitions(configuration);
        var keyboard = new EmulationInputBindingSet(definitions, input.KeyboardBindings ?? GWGUI.Emulation.Functions.EmulationKeyboardBindingFunctions.ToStrings(input.KeyboardMappings, StringComparer.Ordinal), EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller);
        var configured = input.Controllers ?? [];
        var choices = ControllerFunctions.Peripherals(configuration.Model).Where(category => category != PeripheralCategory.Automatic).OrderBy(category => category).Select(Choice).ToArray();
        var ports = Enumerable.Range(ControllerPortConstants.MinimumControllerPort, ConfigurationFunctions.ControllerPortCount(configuration.Model)).Select(index =>
        {
            var current = configured.FirstOrDefault(binding => binding.Port == index);
            var peripheral = current?.Peripheral ?? PeripheralCategory.AnalogJoystick;
            if (peripheral == PeripheralCategory.Automatic) peripheral = PeripheralCategory.AnalogJoystick;
            return new EmulationControllerPort(
                index + BufferConstants.IndexIncrement,
                choices,
                peripheral.ToString(),
                current?.DeviceId,
                new EmulationInputBindingSet(ControllerDefinitions(peripheral), current?.Mappings ?? new Dictionary<string, string>(), EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller, true),
                current?.DeadZonePercent ?? ControllerConstants.DefaultDeadZonePercent,
                current?.VisualId
            );
        }).ToArray();
        return new EmulationInputSettings(keyboard, null, ports);
    }

    /// <summary>Reconstruit les associations de clavier et de contrôleurs puis valide la configuration résultante.</summary>
    /// <param name="configuration">Configuration immuable de départ.</param>
    /// <param name="settings">Valeurs reçues de l’éditeur de l’hôte.</param>
    /// <returns>Nouvelle configuration avec les associations d’entrées mises à jour.</returns>
    /// <exception cref="ArgumentException">Une catégorie de périphérique ne peut pas être reconnue.</exception>
    /// <remarks>Les nouvelles associations clavier sont filtrées par identifiant autorisé et syntaxe reconnue.
    /// Si aucune section clavier n’est fournie, les associations courantes sont conservées. Les numéros de ports
    /// sont convertis en indices avant la validation Common des ports, catégories et zones mortes.</remarks>
    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationInputSettings settings)
    {
        var allowedKeys = ExampleMachineAInputSettingsFunctions.KeyboardDefinitions(configuration).Select(definition => definition.Id).ToHashSet(StringComparer.Ordinal);
        var keyboardBindings = settings.Keyboard is null 
            ? configuration.Input.KeyboardBindings ?? GWGUI.Emulation.Functions.EmulationKeyboardBindingFunctions.ToStrings(configuration.Input.KeyboardMappings, StringComparer.Ordinal)
            : settings.Keyboard.Values.Where(item => allowedKeys.Contains(item.Key) && EmulationKeyboardBindingFunctions.IsValid(item.Value)).ToDictionary(item => item.Key,
                item => string.IsNullOrWhiteSpace(item.Value) ? string.Empty : item.Value,
                StringComparer.Ordinal);
        var keyboard = keyboardBindings.ToKeys();
        var controllers = settings.ControllerPorts.Select(port =>
        {
            if (!Enum.TryParse<PeripheralCategory>(port.SelectedControllerId, true, out var peripheral) || !Enum.IsDefined(peripheral))
                throw new ArgumentException(ErrorMessages.UnsupportedControllerDevice, nameof(settings));
            return new ControllerBinding(port.Number - BufferConstants.IndexIncrement, peripheral, port.PhysicalDeviceId, port.Bindings.Values, port.DeadZonePercent, port.VisualId);
        }).ToArray();
        var result = configuration with
        {
            Input = configuration.Input with
            {
                KeyboardMappings = keyboard,
                KeyboardBindings = keyboardBindings,
                Controllers = controllers
            }
        };
        ConfigurationFunctions.Validate(result);
        return result;
    }

    /// <summary>Construit le choix de périphérique avec son libellé, ses actions et ses associations de visuel.</summary>
    /// <param name="category">Choix désactivé, automatique ou joystick analogique.</param>
    /// <returns>Description du choix ; le joystick associe ses directions et ses boutons visuels à ses commandes.</returns>
    /// <exception cref="ArgumentOutOfRangeException">La catégorie n’a pas de libellé déclaré dans ce parcours.</exception>
    /// <remarks>Les deux actions de bouton du visuel sélectionné désignent ici le même premier bouton de tir.</remarks>
    private static EmulationControllerChoice Choice(PeripheralCategory category) => new(
        category.ToString(),
        category switch
        {
            PeripheralCategory.None => InputSettingsFunctionsConstants.ResourceControllerNone,
            PeripheralCategory.Automatic => InputSettingsFunctionsConstants.ResourceControllerAutomatic,
            PeripheralCategory.AnalogJoystick => "Controllers.Enum.Joystick",
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
        },
        BindingDefinitions: ControllerDefinitions(category),
        CompatibleVisualIds: category == PeripheralCategory.AnalogJoystick ? [EmulationControllerVisualIds.CompetitionPro5000] : [],
        DefaultVisualId: category == PeripheralCategory.AnalogJoystick ? EmulationControllerVisualIds.CompetitionPro5000 : null,
        VisualCommandIds: category == PeripheralCategory.AnalogJoystick ? new Dictionary<EmulationControllerVisualControl, string>
        {
            [EmulationControllerVisualControl.DirectionUp] = EmulationControllerCommandIds.Up,
            [EmulationControllerVisualControl.DirectionDown] = EmulationControllerCommandIds.Down,
            [EmulationControllerVisualControl.DirectionLeft] = EmulationControllerCommandIds.Left,
            [EmulationControllerVisualControl.DirectionRight] = EmulationControllerCommandIds.Right,
            [EmulationControllerVisualControl.PrimaryAction] = EmulationControllerCommandIds.Fire1,
            [EmulationControllerVisualControl.SecondaryAction] = EmulationControllerCommandIds.Fire1
        } : null,
        VisualBindingDefinitions: category == PeripheralCategory.AnalogJoystick ? new Dictionary<string, IReadOnlyList<InputBindingDefinition>>
        {
            [EmulationControllerVisualIds.CompetitionPro5000] = ControllerDefinitions(category)
        } : null
    ) { Category = category == PeripheralCategory.None ? EmulationControllerCategory.None : EmulationControllerCategory.Joystick };
    /// <summary>Décrit les actions associables du périphérique et les paires de directions opposées.</summary>
    /// <param name="category">Catégorie servant à activer ou désactiver les actions et leurs liaisons analogiques.</param>
    /// <returns>Liste vide pour un port désactivé ; directions et bouton de tir pour les autres catégories.</returns>
    /// <remarks>Les choix analogique et automatique relient haut à bas et gauche à droite pour présenter
    /// les deux sens d’un même axe dans l’éditeur.</remarks>
    private static IReadOnlyList<InputBindingDefinition> ControllerDefinitions(PeripheralCategory category) => category == PeripheralCategory.None 
        ? [] 
        : InputSettingsFunctionsConstants.ControllerActions.Select(action => new InputBindingDefinition(action, InputSettingsFunctionsConstants.ResourceActionPrefix + action, string.Empty)
        {
            OppositeDirectionId = category is PeripheralCategory.AnalogJoystick or PeripheralCategory.Automatic ? action switch
            {
                EmulationControllerCommandIds.Up => EmulationControllerCommandIds.Down,
                EmulationControllerCommandIds.Down => EmulationControllerCommandIds.Up,
                EmulationControllerCommandIds.Left => EmulationControllerCommandIds.Right,
                EmulationControllerCommandIds.Right => EmulationControllerCommandIds.Left,
                _ => null
            } : null
        }).ToArray();
}
