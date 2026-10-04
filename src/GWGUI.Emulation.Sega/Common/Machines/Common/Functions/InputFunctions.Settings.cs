namespace GWGUI.Emulation.Sega.Common.Machines.Common.Functions;

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
            var type = ControllerCatalog.Normalize(model, current?.Type ?? ControllerType.Automatic);
            var visualId = NormalizeVisualId(type, current?.VisualId);
            return new EmulationControllerPort(index + 1,
                ControllerCatalog.Types(model).Select(Choice).ToArray(), type.ToString(),
                current?.DeviceId,
                new EmulationInputBindingSet(ControllerDefinitions(type),
                    current?.ButtonMappings ?? new Dictionary<string, string>(),
                    EmulationInputSource.Keyboard | EmulationInputSource.Mouse
                        | EmulationInputSource.Controller, true),
                VisualId: visualId);
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
        var model = ModelCatalog.Get(configuration.Model);
        var controllers = settings.ControllerPorts.Select(port => new ControllerBinding(
            port.Number - 1,
            ControllerCatalog.Normalize(model, Enum.TryParse<ControllerType>(
                port.SelectedControllerId, true, out var type) ? type : ControllerType.None),
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

    private static IReadOnlyList<InputBindingDefinition> ControllerDefinitions(ControllerType type) =>
        type switch
        {
            ControllerType.None => [],
            ControllerType.SegaMasterSystemController or ControllerType.SegaControlStick =>
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
                    InputSettingsFunctionsConstants.ButtonOne, string.Empty),
                Definition(InputSettingsFunctionsConstants.A,
                    InputSettingsFunctionsConstants.ButtonTwo, string.Empty)
            ],
            ControllerType.SegaGameGearController =>
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
                    InputSettingsFunctionsConstants.ButtonOne, string.Empty),
                Definition(InputSettingsFunctionsConstants.A,
                    InputSettingsFunctionsConstants.ButtonTwo, string.Empty),
                Definition(InputSettingsFunctionsConstants.Start,
                    InputSettingsFunctionsConstants.ButtonStart, string.Empty)
            ],
            ControllerType.SegaMegaDriveThreeButton or ControllerType.SegaMegaDriveSixButton
                or ControllerType.SegaArcadePowerStick or ControllerType.SegaArcadePowerStickSixButton =>
                MegaDriveDefinitions(type is ControllerType.SegaMegaDriveSixButton
                    or ControllerType.SegaArcadePowerStickSixButton),
            ControllerType.SegaSaturnController or ControllerType.SegaSaturnThreeDControlPad
                or ControllerType.SegaSaturnVirtuaStick =>
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
                    InputSettingsFunctionsConstants.ButtonA, string.Empty),
                Definition(InputSettingsFunctionsConstants.A,
                    InputSettingsFunctionsConstants.ButtonB, string.Empty),
                Definition(InputSettingsFunctionsConstants.R,
                    InputSettingsFunctionsConstants.ButtonC, string.Empty),
                Definition(InputSettingsFunctionsConstants.Y,
                    InputSettingsFunctionsConstants.ButtonX, string.Empty),
                Definition(InputSettingsFunctionsConstants.X,
                    InputSettingsFunctionsConstants.ButtonY, string.Empty),
                Definition(InputSettingsFunctionsConstants.L,
                    InputSettingsFunctionsConstants.ButtonZ, string.Empty),
                Definition(InputSettingsFunctionsConstants.Start,
                    InputSettingsFunctionsConstants.ButtonStart, string.Empty),
                Definition(InputSettingsFunctionsConstants.L2,
                    InputSettingsFunctionsConstants.TriggerLeft, string.Empty),
                Definition(InputSettingsFunctionsConstants.R2,
                    InputSettingsFunctionsConstants.TriggerRight, string.Empty)
            ],
            ControllerType.SegaDreamcastController =>
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
                    InputSettingsFunctionsConstants.ButtonA, string.Empty),
                Definition(InputSettingsFunctionsConstants.A,
                    InputSettingsFunctionsConstants.ButtonB, string.Empty),
                Definition(InputSettingsFunctionsConstants.Y,
                    InputSettingsFunctionsConstants.ButtonX, string.Empty),
                Definition(InputSettingsFunctionsConstants.X,
                    InputSettingsFunctionsConstants.ButtonY, string.Empty),
                Definition(InputSettingsFunctionsConstants.Start,
                    InputSettingsFunctionsConstants.ButtonStart, string.Empty),
                Definition(InputSettingsFunctionsConstants.L2,
                    InputSettingsFunctionsConstants.TriggerLeft, string.Empty),
                Definition(InputSettingsFunctionsConstants.R2,
                    InputSettingsFunctionsConstants.TriggerRight, string.Empty)
            ],
            _ =>
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
            ]
        };

    private static IReadOnlyList<InputBindingDefinition> MegaDriveDefinitions(bool sixButton)
    {
        var definitions = new List<InputBindingDefinition>
        {
            Definition(InputSettingsFunctionsConstants.Up,
                InputSettingsFunctionsConstants.ResourceControllerActionUp, string.Empty),
            Definition(InputSettingsFunctionsConstants.Down,
                InputSettingsFunctionsConstants.ResourceControllerActionDown, string.Empty),
            Definition(InputSettingsFunctionsConstants.Left,
                InputSettingsFunctionsConstants.ResourceControllerActionLeft, string.Empty),
            Definition(InputSettingsFunctionsConstants.Right,
                InputSettingsFunctionsConstants.ResourceControllerActionRight, string.Empty),
            Definition(InputSettingsFunctionsConstants.Y,
                InputSettingsFunctionsConstants.ButtonA, string.Empty),
            Definition(InputSettingsFunctionsConstants.B,
                InputSettingsFunctionsConstants.ButtonB, string.Empty),
            Definition(InputSettingsFunctionsConstants.A,
                InputSettingsFunctionsConstants.ButtonC, string.Empty),
            Definition(InputSettingsFunctionsConstants.Start,
                InputSettingsFunctionsConstants.ButtonStart, string.Empty)
        };
        if (sixButton)
        {
            definitions.Add(Definition(InputSettingsFunctionsConstants.L,
                InputSettingsFunctionsConstants.ButtonX, string.Empty));
            definitions.Add(Definition(InputSettingsFunctionsConstants.X,
                InputSettingsFunctionsConstants.ButtonY, string.Empty));
            definitions.Add(Definition(InputSettingsFunctionsConstants.R,
                InputSettingsFunctionsConstants.ButtonZ, string.Empty));
            definitions.Add(Definition(InputSettingsFunctionsConstants.Select,
                InputSettingsFunctionsConstants.ButtonMode, string.Empty));
        }
        return definitions;
    }

    private static InputBindingDefinition Definition(string id, string resourceKey,
        string defaultBinding, string? invariant = null) => new(id, resourceKey, defaultBinding,
            invariant ?? (resourceKey.Contains('.') ? null : resourceKey));

    private static EmulationControllerChoice Choice(ControllerType type) => new(
        type.ToString(), ControllerResourceKey(type), BindingDefinitions: ControllerDefinitions(type),
        CompatibleVisualIds: CompatibleVisualIds(type), DefaultVisualId: DefaultVisualId(type),
        VisualCommandIds: VisualCommandIds(type));
}
