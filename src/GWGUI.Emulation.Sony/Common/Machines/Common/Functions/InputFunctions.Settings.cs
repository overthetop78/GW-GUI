namespace GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    internal static EmulationInputSettings Describe(MachineConfiguration configuration)
    {
        var model = ModelCatalog.Get(configuration.Model);
        var input = configuration.Input ?? new InputConfiguration();
        var types = ControllerCatalog.Types(configuration);
        var keyboard = model.HasKeyboard || types.Any(type => type is
            ControllerType.Keyboard or ControllerType.KeyboardAndMouse) ? new EmulationInputBindingSet(
            KeyboardDefinitions(), input.KeyboardBindings ?? ToStrings(input.KeyboardMappings),
            EmulationInputSource.Keyboard) : null;
        var mouse = model.MouseButtonCount > 0 ? new EmulationInputBindingSet(
            MouseDefinitions(), MouseValues(input), EmulationInputSource.Mouse
                | EmulationInputSource.Keyboard | EmulationInputSource.Controller, true) : null;
        var configured = input.ControllerBindings ?? [];
        var ports = Enumerable.Range(ControllerPortConstants.MinimumControllerPort,
            ControllerFunctions.PortCount(configuration)).Select(index =>
        {
            var current = configured.FirstOrDefault(item => item.Port == index);
            var type = ControllerFunctions.Resolve(configuration, index);
            return new EmulationControllerPort(index + 1,
                ControllerFunctions.Supported(configuration, index).Select(choice => Choice(choice, model)).ToArray(),
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
        var controllers = settings.ControllerPorts.Select(port =>
        {
            var index = port.Number - ControllerPortConstants.OneControllerPort;
            if (!Enum.TryParse<ControllerType>(port.SelectedControllerId, true, out var type))
                throw new ArgumentOutOfRangeException(nameof(port.SelectedControllerId),
                    port.SelectedControllerId, null);
            ControllerFunctions.Validate(configuration, index, type);
            return new ControllerBinding(index, type, port.PhysicalDeviceId,
                port.Bindings.Values
                    .Where(item => !string.IsNullOrWhiteSpace(item.Key)
                        && !string.IsNullOrWhiteSpace(item.Value))
                    .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal),
                port.VisualId);
        }).ToArray();
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
        : type == ControllerType.Mouse ? MouseDefinitions()
        : type == ControllerType.Keyboard ? []
        : model.Id == ModelConstants.PocketStation ? Machines.PocketStation.Functions.ControllerFunctions.Definitions
        : model.Id == ModelConstants.Psp ? PspControllerDefinitions()
        : model.Id is ModelConstants.PlayStation or ModelConstants.PlayStation2
            or ModelConstants.PlayStation3
            ? PlayStationControllerDefinitions(type, model) :
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

    private static IReadOnlyList<InputBindingDefinition> PlayStationControllerDefinitions(
        ControllerType type, Model model)
    {
        var buttons = PlayStationButtonDefinitions();
        if (type is ControllerType.GunCon or ControllerType.Justifier)
            return
            [
                Definition(EmulationControllerCommandIds.B,
                    InputSettingsFunctionsConstants.ResourceControllerActionFire1, string.Empty),
                Definition(EmulationControllerCommandIds.A, EmulationControllerCommandIds.A,
                    string.Empty),
                type == ControllerType.Justifier
                    ? Definition(EmulationControllerCommandIds.Start,
                        InputSettingsFunctionsConstants.ControllerStart, string.Empty)
                    : Definition(EmulationControllerCommandIds.X, EmulationControllerCommandIds.B,
                        string.Empty)
            ];
        if (type is ControllerType.NeGcon or ControllerType.NeGconRumble)
            return buttons.Where(definition => definition.Id is
                EmulationControllerCommandIds.Up or EmulationControllerCommandIds.Down
                or EmulationControllerCommandIds.Left or EmulationControllerCommandIds.Right
                or EmulationControllerCommandIds.Start).Concat(
                new InputBindingDefinition[]
                {
                    Definition(EmulationControllerCommandIds.A, EmulationControllerCommandIds.A,
                        string.Empty),
                    Definition(EmulationControllerCommandIds.X, EmulationControllerCommandIds.B,
                        string.Empty),
                    Definition(EmulationControllerCommandIds.R, EmulationControllerCommandIds.R,
                        string.Empty),
                    Definition(EmulationControllerCommandIds.R2,
                        InputSettingsFunctionsConstants.NeGconButtonI, string.Empty),
                    Definition(EmulationControllerCommandIds.L2,
                        InputSettingsFunctionsConstants.NeGconButtonII, string.Empty),
                    Definition(EmulationControllerCommandIds.L, EmulationControllerCommandIds.L,
                        string.Empty),
                    Definition(InputSettingsFunctionsConstants.LeftStickLeft,
                        InputSettingsFunctionsConstants.ResourceSteeringLeft, string.Empty),
                    Definition(InputSettingsFunctionsConstants.LeftStickRight,
                        InputSettingsFunctionsConstants.ResourceSteeringRight, string.Empty)
                }).ToArray();
        if (model.Id == ModelConstants.PlayStation && type == ControllerType.Joystick)
            return buttons.Where(definition => definition.Id is not
                (EmulationControllerCommandIds.L3 or EmulationControllerCommandIds.R3)).ToArray();
        return buttons.Concat(AnalogStickDefinitions()).ToArray();
    }

    private static IReadOnlyList<InputBindingDefinition> AnalogStickDefinitions() =>
    [
        Definition(InputSettingsFunctionsConstants.LeftStickUp,
            InputSettingsFunctionsConstants.LeftStickUpLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.LeftStickDown,
            InputSettingsFunctionsConstants.LeftStickDownLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.LeftStickLeft,
            InputSettingsFunctionsConstants.LeftStickLeftLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.LeftStickRight,
            InputSettingsFunctionsConstants.LeftStickRightLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.RightStickUp,
            InputSettingsFunctionsConstants.RightStickUpLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.RightStickDown,
            InputSettingsFunctionsConstants.RightStickDownLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.RightStickLeft,
            InputSettingsFunctionsConstants.RightStickLeftLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.RightStickRight,
            InputSettingsFunctionsConstants.RightStickRightLabel, string.Empty)
    ];

    private static IReadOnlyList<InputBindingDefinition> PlayStationButtonDefinitions() =>
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
        InvariantDisplayValue: type switch
        {
            ControllerType.NeGcon => InputSettingsFunctionsConstants.NeGcon,
            ControllerType.NeGconRumble => InputSettingsFunctionsConstants.NeGconRumble,
            ControllerType.KeyboardAndMouse => InputSettingsFunctionsConstants.KeyboardAndMouseGlyphs,
            _ => null
        },
        BindingDefinitions: ControllerDefinitions(type, model),
        CompatibleVisualIds: CompatibleVisualIds(type, model),
        DefaultVisualId: DefaultVisualId(type, model),
        VisualCommandIds: VisualCommandIds(type, model));
}
