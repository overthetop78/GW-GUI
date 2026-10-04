namespace GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    internal static EmulationInputSettings Describe(MachineConfiguration configuration)
    {
        var model = ModelCatalog.Get(configuration.Model);
        var input = configuration.Input ?? new InputConfiguration();
        var keyboard = model.HasKeyboard ? new EmulationInputBindingSet(
            KeyboardDefinitions(model), input.KeyboardBindings ?? ToStrings(input.KeyboardMappings),
            EmulationInputSource.Keyboard) : null;
        var mouse = model.MouseButtonCount > 0 ? new EmulationInputBindingSet(
            MouseDefinitions(), MouseValues(input), EmulationInputSource.Mouse
                | EmulationInputSource.Keyboard | EmulationInputSource.Controller, true) : null;
        var configured = input.ControllerBindings ?? [];
        var ports = Enumerable.Range(0, model.ControllerPortCount).Select(index =>
        {
            var current = configured.FirstOrDefault(item => item.Port == index);
            var type = current?.Type ?? ControllerCatalog.Default(model);
            return new EmulationControllerPort(index + 1,
                ControllerCatalog.Types(model).Select(choice => Choice(choice, model)).ToArray(),
                type.ToString(),
                current?.DeviceId,
                new EmulationInputBindingSet(ControllerDefinitions(type, model),
                    current?.ButtonMappings ?? new Dictionary<string, string>(),
                    EmulationInputSource.Keyboard | EmulationInputSource.Mouse
                        | EmulationInputSource.Controller, true),
                VisualId: current?.VisualId);
        }).ToArray();
        return new EmulationInputSettings(keyboard, mouse, ports);
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration,
        EmulationInputSettings settings)
    {
        var current = configuration.Input ?? new InputConfiguration();
        var keyboard = settings.Keyboard?.Values ?? new Dictionary<string, string>();
        var mouse = settings.Mouse?.Values
            .Where(item => !string.IsNullOrWhiteSpace(item.Value)
                && Enum.TryParse<MouseAction>(item.Key, true, out var action)
                && action != MouseAction.None)
            .ToDictionary(item => item.Value, item => Enum.Parse<MouseAction>(item.Key, true),
                StringComparer.OrdinalIgnoreCase) ?? new Dictionary<string, MouseAction>();
        var controllers = settings.ControllerPorts.Select(port => new ControllerBinding(
            port.Number - 1,
            Enum.TryParse<ControllerType>(port.SelectedControllerId, true, out var type)
                ? type : ControllerType.None,
            port.PhysicalDeviceId,
            port.Bindings.Values
                .Where(item => !string.IsNullOrWhiteSpace(item.Key)
                    && !string.IsNullOrWhiteSpace(item.Value))
                .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal),
            port.VisualId)).ToArray();
        return configuration with
        {
            Input = current with
            {
                KeyboardBindings = keyboard,
                KeyboardMappings = ToKeys(keyboard),
                MouseButtonMappings = mouse,
                ControllerBindings = controllers
            }
        };
    }

    private static IReadOnlyList<InputBindingDefinition> KeyboardDefinitions(Model model)
        => [];

    private static IReadOnlyList<InputBindingDefinition> MouseDefinitions() =>
    [
        Definition(nameof(MouseAction.LeftButton),
            InputSettingsFunctionsConstants.ResourceMouseButtonLeft,
            InputSettingsFunctionsConstants.MouseLeft),
        Definition(nameof(MouseAction.RightButton),
            InputSettingsFunctionsConstants.ResourceMouseButtonRight,
            InputSettingsFunctionsConstants.MouseRight)
    ];

    private static IReadOnlyDictionary<string, string> MouseValues(InputConfiguration input) =>
        (input.MouseButtonMappings ?? new Dictionary<string, MouseAction>())
            .Where(item => item.Value != MouseAction.None)
            .ToDictionary(item => item.Value.ToString(), item => item.Key, StringComparer.Ordinal);

    private static IReadOnlyList<InputBindingDefinition> ControllerDefinitions(
        ControllerType type, Model model) => type is ControllerType.None ? []
        : model.Id == ModelConstants.Psp ? PspControllerDefinitions()
        : model.Id is ModelConstants.PlayStation or ModelConstants.PlayStation2
            ? PlayStationControllerDefinitions() :
        [
            Definition(InputSettingsFunctionsConstants.Up,
                InputSettingsFunctionsConstants.ResourceControllerActionUp, string.Empty),
            Definition(InputSettingsFunctionsConstants.Down,
                InputSettingsFunctionsConstants.ResourceControllerActionDown, string.Empty),
            Definition(InputSettingsFunctionsConstants.Left,
                InputSettingsFunctionsConstants.ResourceControllerActionLeft, string.Empty),
            Definition(InputSettingsFunctionsConstants.Right,
                InputSettingsFunctionsConstants.ResourceControllerActionRight, string.Empty),
            Definition(InputSettingsFunctionsConstants.B,
                InputSettingsFunctionsConstants.ResourceControllerActionFire1, string.Empty),
            Definition(InputSettingsFunctionsConstants.A,
                InputSettingsFunctionsConstants.ResourceControllerActionFire2, string.Empty)
        ];

    private static IReadOnlyList<InputBindingDefinition> PspControllerDefinitions() =>
    [
        Definition(InputSettingsFunctionsConstants.Up,
            InputSettingsFunctionsConstants.ResourceControllerActionUp, string.Empty),
        Definition(InputSettingsFunctionsConstants.Down,
            InputSettingsFunctionsConstants.ResourceControllerActionDown, string.Empty),
        Definition(InputSettingsFunctionsConstants.Left,
            InputSettingsFunctionsConstants.ResourceControllerActionLeft, string.Empty),
        Definition(InputSettingsFunctionsConstants.Right,
            InputSettingsFunctionsConstants.ResourceControllerActionRight, string.Empty),
        Definition(EmulationControllerCommandIds.B, InputSettingsFunctionsConstants.ControllerCross,
            string.Empty),
        Definition(EmulationControllerCommandIds.A, InputSettingsFunctionsConstants.ControllerCircle,
            string.Empty),
        Definition(EmulationControllerCommandIds.Y, InputSettingsFunctionsConstants.ControllerSquare,
            string.Empty),
        Definition(EmulationControllerCommandIds.X, InputSettingsFunctionsConstants.ControllerTriangle,
            string.Empty),
        Definition(EmulationControllerCommandIds.Select,
            InputSettingsFunctionsConstants.ControllerSelect, string.Empty),
        Definition(EmulationControllerCommandIds.Start,
            InputSettingsFunctionsConstants.ControllerStart, string.Empty),
        Definition(EmulationControllerCommandIds.L, EmulationControllerCommandIds.L,
            string.Empty),
        Definition(EmulationControllerCommandIds.R, EmulationControllerCommandIds.R,
            string.Empty),
        Definition(InputSettingsFunctionsConstants.PspStickUp,
            InputSettingsFunctionsConstants.PspStickUpLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.PspStickDown,
            InputSettingsFunctionsConstants.PspStickDownLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.PspStickLeft,
            InputSettingsFunctionsConstants.PspStickLeftLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.PspStickRight,
            InputSettingsFunctionsConstants.PspStickRightLabel, string.Empty)
    ];

    private static IReadOnlyList<InputBindingDefinition> PlayStationControllerDefinitions() =>
    [
        Definition(InputSettingsFunctionsConstants.Up,
            InputSettingsFunctionsConstants.ResourceControllerActionUp, string.Empty),
        Definition(InputSettingsFunctionsConstants.Down,
            InputSettingsFunctionsConstants.ResourceControllerActionDown, string.Empty),
        Definition(InputSettingsFunctionsConstants.Left,
            InputSettingsFunctionsConstants.ResourceControllerActionLeft, string.Empty),
        Definition(InputSettingsFunctionsConstants.Right,
            InputSettingsFunctionsConstants.ResourceControllerActionRight, string.Empty),
        Definition(EmulationControllerCommandIds.B, InputSettingsFunctionsConstants.ControllerCross,
            string.Empty),
        Definition(EmulationControllerCommandIds.A, InputSettingsFunctionsConstants.ControllerCircle,
            string.Empty),
        Definition(EmulationControllerCommandIds.Y, InputSettingsFunctionsConstants.ControllerSquare,
            string.Empty),
        Definition(EmulationControllerCommandIds.X, InputSettingsFunctionsConstants.ControllerTriangle,
            string.Empty),
        Definition(EmulationControllerCommandIds.Select,
            InputSettingsFunctionsConstants.ControllerSelect, string.Empty),
        Definition(EmulationControllerCommandIds.Start,
            InputSettingsFunctionsConstants.ControllerStart, string.Empty),
        Definition(EmulationControllerCommandIds.L, InputSettingsFunctionsConstants.ControllerL1,
            string.Empty),
        Definition(EmulationControllerCommandIds.R, InputSettingsFunctionsConstants.ControllerR1,
            string.Empty),
        Definition(EmulationControllerCommandIds.L2, InputSettingsFunctionsConstants.ControllerL2,
            string.Empty),
        Definition(EmulationControllerCommandIds.R2, InputSettingsFunctionsConstants.ControllerR2,
            string.Empty),
        Definition(EmulationControllerCommandIds.L3, InputSettingsFunctionsConstants.ControllerL3,
            string.Empty),
        Definition(EmulationControllerCommandIds.R3, InputSettingsFunctionsConstants.ControllerR3,
            string.Empty)
    ];

    private static InputBindingDefinition Definition(string id, string resourceKey,
        string defaultBinding, string? invariant = null) => new(id, resourceKey, defaultBinding,
            invariant ?? (resourceKey.Contains('.') ? null : resourceKey));

    private static EmulationControllerChoice Choice(ControllerType type, Model model) => new(
        type.ToString(), ControllerResourceKey(type, model),
        BindingDefinitions: ControllerDefinitions(type, model),
        CompatibleVisualIds: CompatibleVisualIds(type, model),
        DefaultVisualId: DefaultVisualId(type, model),
        VisualCommandIds: VisualCommandIds(type, model));
}
