using GWGUI.Emulation;
namespace GWGUI.Emulation.Microsoft.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    internal static EmulationInputSettings Describe(MachineConfiguration configuration)
    {
        var model = ModelCatalog.Get(configuration.Model);
        var input = configuration.Input ?? new InputConfiguration();
        var keyboard = model.HasKeyboard ? new EmulationInputBindingSet(KeyboardDefinitions(model), input.KeyboardBindings ?? GWGUI.Emulation.Functions.EmulationKeyboardBindingFunctions.ToStrings(input.KeyboardMappings), EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller) : null;
        var mouse = model.MouseButtonCount > 0 ? new EmulationInputBindingSet(MouseDefinitions(), EmulationSdk.InputProcessing.MouseBindings(input.MouseButtonMappings, MouseAction.None), EmulationInputSource.Mouse | EmulationInputSource.Keyboard | EmulationInputSource.Controller, true) : null;
        var configured = input.ControllerBindings ?? [];
        var ports = Enumerable.Range(0, model.ControllerPortCount).Select(index =>
        {
            var current = configured.FirstOrDefault(item => item.Port == index);
            var type = current?.Type ?? ControllerCatalog.Default(model);
            return new EmulationControllerPort(index + 1,
                ControllerCatalog.Types(model).Select(Choice).ToArray(), type.ToString(),
                current?.DeviceId,
                new EmulationInputBindingSet(ControllerDefinitions(type), current?.ButtonMappings ?? new Dictionary<string, string>(), EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller, true),
                VisualId: current?.VisualId);
        }).ToArray();
        return new EmulationInputSettings(keyboard, mouse, ports);
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationInputSettings settings)
    {
        var current = configuration.Input ?? new InputConfiguration();
        var keyboard = settings.Keyboard?.Values ?? new Dictionary<string, string>();
        var mouse = EmulationSdk.InputProcessing.CreateMouseMappings(settings.Mouse, MouseAction.None) ?? new Dictionary<string, MouseAction>();
        var controllers = settings.ControllerPorts.Select(port => new ControllerBinding(
            port.Number - 1,
            Enum.TryParse<ControllerType>(port.SelectedControllerId, true, out var type) ? type : ControllerType.None,
            port.PhysicalDeviceId,
            EmulationSdk.InputProcessing.CopyNonEmptyBindings(port.Bindings.Values),
            port.VisualId)).ToArray();
        return configuration with
        {
            Input = current with
            {
                KeyboardBindings = keyboard,
                KeyboardMappings = GWGUI.Emulation.Functions.EmulationKeyboardBindingFunctions.ToKeys(keyboard),
                MouseButtonMappings = mouse,
                ControllerBindings = controllers
            }
        };
    }

    private static IReadOnlyList<InputBindingDefinition> KeyboardDefinitions(Model model) => [];

    private static IReadOnlyList<InputBindingDefinition> MouseDefinitions() =>
    [
        Definition(nameof(MouseAction.LeftButton), InputSettingsFunctionsConstants.ResourceMouseButtonLeft, InputSettingsFunctionsConstants.MouseLeft),
        Definition(nameof(MouseAction.RightButton), InputSettingsFunctionsConstants.ResourceMouseButtonRight, InputSettingsFunctionsConstants.MouseRight)
    ];

    private static IReadOnlyList<InputBindingDefinition> ControllerDefinitions(ControllerType type) =>
        type is ControllerType.None ? [] :
        [
            Definition(InputSettingsFunctionsConstants.Up, InputSettingsFunctionsConstants.ResourceControllerActionUp, string.Empty),
            Definition(InputSettingsFunctionsConstants.Down, InputSettingsFunctionsConstants.ResourceControllerActionDown, string.Empty),
            Definition(InputSettingsFunctionsConstants.Left, InputSettingsFunctionsConstants.ResourceControllerActionLeft, string.Empty),
            Definition(InputSettingsFunctionsConstants.Right, InputSettingsFunctionsConstants.ResourceControllerActionRight, string.Empty),
            Definition(InputSettingsFunctionsConstants.B, InputSettingsFunctionsConstants.ResourceControllerActionFire1, string.Empty),
            Definition(InputSettingsFunctionsConstants.A, InputSettingsFunctionsConstants.ResourceControllerActionFire2, string.Empty)
        ];

    private static InputBindingDefinition Definition(string id, string resourceKey, string defaultBinding, string? invariant = null) => new(id, resourceKey, defaultBinding, EmulationSdk.InputProcessing.BindingDisplayValue(resourceKey, invariant));

    private static EmulationControllerChoice Choice(ControllerType type) => new(
        type.ToString(), ControllerResourceKey(type), BindingDefinitions: ControllerDefinitions(type),
        CompatibleVisualIds: CompatibleVisualIds(type), DefaultVisualId: DefaultVisualId(type),
        VisualCommandIds: VisualCommandIds(type)) { Category = type == ControllerType.None ? EmulationControllerCategory.None : EmulationControllerCategory.Joystick };
}
