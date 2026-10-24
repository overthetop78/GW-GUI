using CommonControllerFunctions = GWGUI.Emulation.Amstrad.Common.Functions.ControllerFunctions;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Amstrad.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    internal static EmulationInputSettings Describe(MachineConfiguration configuration)
    {
        var model = ModelCatalog.Get(configuration.Model);
        var input = configuration.Input ?? new InputConfiguration();
        var keyboard = model.HasKeyboard ? new EmulationInputBindingSet((new Engine().SelectedAdapter(configuration)?.GetKeyboardDefinitions(configuration) ?? KeyboardDefinitions()), KeyboardBindingFunctions.Normalize(input), EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller) : null;
        var mouse = model.MouseButtonCount > BufferConstants.EmptyCollectionCount ? new EmulationInputBindingSet(MouseDefinitions(), EmulationSdk.InputProcessing.MouseBindings(input.MouseButtonMappings, MouseAction.None), EmulationInputSource.Mouse | EmulationInputSource.Keyboard | EmulationInputSource.Controller, true) : null;
        var configured = input.Controllers ?? [];
        var ports = Enumerable.Range(BufferConstants.FirstCollectionIndex, CommonControllerFunctions.PortCount(configuration)).Select(index =>
        {
            var current = configured.FirstOrDefault(item => item.Port == index);
            var type = current?.Peripheral ?? ControllerCatalog.Default(model);
            return new EmulationControllerPort(index + BufferConstants.IndexIncrement,
                ControllerCatalog.Types(model).Select(Choice).ToArray(), type.ToString(),
                current?.DeviceId,
                new EmulationInputBindingSet(ControllerDefinitions(type), current?.Mappings ?? new Dictionary<string, string>(), EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller, true),
                DeadZonePercent: current?.DeadZonePercent ?? 0, VisualId: current?.VisualId) { OutputBindings = current?.OutputBindings ?? new Dictionary<string, EmulationControllerOutputBinding>() };
        }).ToArray();
        return new EmulationInputSettings(keyboard, mouse, ports);
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationInputSettings settings)
    {
        if (settings.ControllerPorts.Any(port => port.DeadZonePercent is < 0 or > 100)) throw new ArgumentOutOfRangeException(nameof(settings), nameof(EmulationControllerPort.DeadZonePercent));
        var current = configuration.Input ?? new InputConfiguration();
        var keyboard = KeyboardBindingFunctions.Normalize(current with { KeyboardBindings = settings.Keyboard?.Values ?? new Dictionary<string, string>() });
        var mouse = EmulationSdk.InputProcessing.CreateMouseMappings(settings.Mouse, MouseAction.None) ?? new Dictionary<string, MouseAction>();
        var controllers = settings.ControllerPorts.Select(port => new ControllerBinding(
            port.Number - BufferConstants.IndexIncrement,
            Enum.TryParse<PeripheralCategory>(port.SelectedControllerId, true, out var type) ? type : PeripheralCategory.None,
            port.PhysicalDeviceId,
            EmulationSdk.InputProcessing.CopyNonEmptyBindings(port.Bindings.Values),
            port.VisualId, OutputBindings: port.OutputBindings, DeadZonePercent: port.DeadZonePercent)).ToArray();
        var adjustedConfiguration = configuration with
        {
            Input = current with
            {
                KeyboardBindings = keyboard,
                KeyboardMappings = EmulationSdk.InputProcessing.ToKeys(keyboard),
                MouseButtonMappings = mouse,
                Controllers = controllers
            }
        };
        return new Engine().SelectedAdapter(adjustedConfiguration)?.ApplyInputConfiguration(adjustedConfiguration) ?? adjustedConfiguration;
    }

    private static IReadOnlyList<InputBindingDefinition> KeyboardDefinitions() => KeyboardConstants.SpecialKeys.Select(key => Definition(key.ToString(), key.ToString(), KeyboardDictionary.DefaultHostKeys[key].ToString(), KeyboardDictionary.Labels[key])).ToArray();
    private static IReadOnlyList<InputBindingDefinition> MouseDefinitions() =>
    [
        Definition(nameof(MouseAction.LeftButton), InputSettingsFunctionsConstants.ResourceMouseButtonLeft, InputSettingsFunctionsConstants.MouseLeft),
        Definition(nameof(MouseAction.RightButton), InputSettingsFunctionsConstants.ResourceMouseButtonRight, InputSettingsFunctionsConstants.MouseRight)
    ];

    private static IReadOnlyList<InputBindingDefinition> ControllerDefinitions(PeripheralCategory type) =>
        type is PeripheralCategory.None ? [] :
        [
            Definition(InputSettingsFunctionsConstants.Up, InputSettingsFunctionsConstants.ResourceControllerActionUp, string.Empty),
            Definition(InputSettingsFunctionsConstants.Down, InputSettingsFunctionsConstants.ResourceControllerActionDown, string.Empty),
            Definition(InputSettingsFunctionsConstants.Left, InputSettingsFunctionsConstants.ResourceControllerActionLeft, string.Empty),
            Definition(InputSettingsFunctionsConstants.Right, InputSettingsFunctionsConstants.ResourceControllerActionRight, string.Empty),
            Definition(InputSettingsFunctionsConstants.B, InputSettingsFunctionsConstants.ResourceControllerActionFire1, string.Empty),
            Definition(InputSettingsFunctionsConstants.A, InputSettingsFunctionsConstants.ResourceControllerActionFire2, string.Empty)
        ];

    private static InputBindingDefinition Definition(string id, string resourceKey, string defaultBinding, string? invariant = null) => new(id, resourceKey, defaultBinding, EmulationSdk.InputProcessing.BindingDisplayValue(resourceKey, invariant));

    private static EmulationControllerChoice Choice(PeripheralCategory type) => new(
        type.ToString(), ControllerResourceKey(type), BindingDefinitions: ControllerDefinitions(type),
        CompatibleVisualIds: CompatibleVisualIds(type), DefaultVisualId: DefaultVisualId(type),
        VisualCommandIds: VisualCommandIds(type))
        {
            Category = type switch
            {
                PeripheralCategory.None => EmulationControllerCategory.None,
                PeripheralCategory.Gamepad => EmulationControllerCategory.Gamepad,
                _ => EmulationControllerCategory.Joystick
            }
        };

    /// <summary>Décrit les touches du clavier principal selon les règles matérielles déjà utilisées.</summary>
    /// <param name="configuration">Profil et associations empruntés à consulter.</param>
    /// <returns>Définitions de clavier du profil ; liste vide sans clavier pris en charge.</returns>
    internal static IReadOnlyList<InputBindingDefinition> KeyboardDefinitions(MachineConfiguration configuration) { return ModelCatalog.Get(configuration.Model).HasKeyboard ? KeyboardDefinitions() : []; }
}
