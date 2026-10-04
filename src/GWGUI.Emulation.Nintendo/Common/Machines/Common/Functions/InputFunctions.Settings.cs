namespace GWGUI.Emulation.Nintendo.Common.Machines.Common.Functions;

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
        : model.Id is ModelConstants.Nes or ModelConstants.FamicomDisk
            ? NintendoControllerDefinitions(false, nes: true)
        : model.Id == ModelConstants.Snes ? NintendoControllerDefinitions(true)
        : model.Id == ModelConstants.Nintendo64 ? Nintendo64ControllerDefinitions()
        : model.Id == ModelConstants.GameCube ? GameCubeControllerDefinitions()
        : model.Id is ModelConstants.GameBoy or ModelConstants.GameBoyColor
            or ModelConstants.GameBoyAdvance
            ? NintendoControllerDefinitions(false,
                model.Id == ModelConstants.GameBoyAdvance) :
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

    private static IReadOnlyList<InputBindingDefinition> NintendoControllerDefinitions(
        bool superNintendo, bool gameBoyAdvance = false, bool nes = false)
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
            Definition(EmulationControllerCommandIds.B,
                InputSettingsFunctionsConstants.ButtonB, string.Empty),
            Definition(EmulationControllerCommandIds.A,
                InputSettingsFunctionsConstants.ButtonA, string.Empty),
            Definition(EmulationControllerCommandIds.Select,
                InputSettingsFunctionsConstants.ButtonSelect, string.Empty),
            Definition(EmulationControllerCommandIds.Start,
                InputSettingsFunctionsConstants.ButtonStart, string.Empty)
        };
        if (nes)
        {
            definitions.Add(Definition(InputSettingsFunctionsConstants.NesTurboB,
                InputSettingsFunctionsConstants.ButtonTurboB, string.Empty));
            definitions.Add(Definition(InputSettingsFunctionsConstants.NesTurboA,
                InputSettingsFunctionsConstants.ButtonTurboA, string.Empty));
        }
        if (superNintendo)
        {
            definitions.Add(Definition(EmulationControllerCommandIds.Y,
                InputSettingsFunctionsConstants.ButtonY, string.Empty));
            definitions.Add(Definition(EmulationControllerCommandIds.X,
                InputSettingsFunctionsConstants.ButtonX, string.Empty));
        }
        if (superNintendo || gameBoyAdvance)
        {
            definitions.Add(Definition(EmulationControllerCommandIds.L,
                InputSettingsFunctionsConstants.ButtonL, string.Empty));
            definitions.Add(Definition(EmulationControllerCommandIds.R,
                InputSettingsFunctionsConstants.ButtonR, string.Empty));
        }
        return definitions;
    }

    private static IReadOnlyList<InputBindingDefinition> Nintendo64ControllerDefinitions() =>
    [
        Definition(InputSettingsFunctionsConstants.Up,
            InputSettingsFunctionsConstants.ResourceControllerActionUp, string.Empty),
        Definition(InputSettingsFunctionsConstants.Down,
            InputSettingsFunctionsConstants.ResourceControllerActionDown, string.Empty),
        Definition(InputSettingsFunctionsConstants.Left,
            InputSettingsFunctionsConstants.ResourceControllerActionLeft, string.Empty),
        Definition(InputSettingsFunctionsConstants.Right,
            InputSettingsFunctionsConstants.ResourceControllerActionRight, string.Empty),
        Definition(InputSettingsFunctionsConstants.Nintendo64StickUp,
            InputSettingsFunctionsConstants.Nintendo64StickUpLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.Nintendo64StickDown,
            InputSettingsFunctionsConstants.Nintendo64StickDownLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.Nintendo64StickLeft,
            InputSettingsFunctionsConstants.Nintendo64StickLeftLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.Nintendo64StickRight,
            InputSettingsFunctionsConstants.Nintendo64StickRightLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.Nintendo64A,
            InputSettingsFunctionsConstants.ButtonA, string.Empty),
        Definition(InputSettingsFunctionsConstants.Nintendo64B,
            InputSettingsFunctionsConstants.ButtonB, string.Empty),
        Definition(InputSettingsFunctionsConstants.Nintendo64CUp,
            InputSettingsFunctionsConstants.Nintendo64CUpLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.Nintendo64CDown,
            InputSettingsFunctionsConstants.Nintendo64CDownLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.Nintendo64CLeft,
            InputSettingsFunctionsConstants.Nintendo64CLeftLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.Nintendo64CRight,
            InputSettingsFunctionsConstants.Nintendo64CRightLabel, string.Empty),
        Definition(EmulationControllerCommandIds.L,
            InputSettingsFunctionsConstants.ButtonL, string.Empty),
        Definition(EmulationControllerCommandIds.R,
            InputSettingsFunctionsConstants.ButtonR, string.Empty),
        Definition(EmulationControllerCommandIds.L2,
            InputSettingsFunctionsConstants.Nintendo64ButtonZ, string.Empty),
        Definition(EmulationControllerCommandIds.Start,
            InputSettingsFunctionsConstants.ButtonStart, string.Empty)
    ];

    private static IReadOnlyList<InputBindingDefinition> GameCubeControllerDefinitions() =>
    [
        Definition(InputSettingsFunctionsConstants.Up,
            InputSettingsFunctionsConstants.ResourceControllerActionUp, string.Empty),
        Definition(InputSettingsFunctionsConstants.Down,
            InputSettingsFunctionsConstants.ResourceControllerActionDown, string.Empty),
        Definition(InputSettingsFunctionsConstants.Left,
            InputSettingsFunctionsConstants.ResourceControllerActionLeft, string.Empty),
        Definition(InputSettingsFunctionsConstants.Right,
            InputSettingsFunctionsConstants.ResourceControllerActionRight, string.Empty),
        Definition(EmulationControllerCommandIds.B,
            InputSettingsFunctionsConstants.ButtonB, string.Empty),
        Definition(EmulationControllerCommandIds.Y,
            InputSettingsFunctionsConstants.ButtonY, string.Empty),
        Definition(EmulationControllerCommandIds.A,
            InputSettingsFunctionsConstants.ButtonA, string.Empty),
        Definition(EmulationControllerCommandIds.X,
            InputSettingsFunctionsConstants.ButtonX, string.Empty),
        Definition(InputSettingsFunctionsConstants.GameCubeZ,
            InputSettingsFunctionsConstants.GameCubeButtonZ, string.Empty),
        Definition(EmulationControllerCommandIds.Start,
            InputSettingsFunctionsConstants.ButtonStart, string.Empty),
        Definition(InputSettingsFunctionsConstants.GameCubeL,
            InputSettingsFunctionsConstants.ButtonL, string.Empty),
        Definition(InputSettingsFunctionsConstants.GameCubeR,
            InputSettingsFunctionsConstants.ButtonR, string.Empty),
        Definition(InputSettingsFunctionsConstants.GameCubeLHalf,
            InputSettingsFunctionsConstants.GameCubeButtonLHalf, string.Empty),
        Definition(InputSettingsFunctionsConstants.GameCubeRHalf,
            InputSettingsFunctionsConstants.GameCubeButtonRHalf, string.Empty),
        Definition(InputSettingsFunctionsConstants.GameCubeStickUp,
            InputSettingsFunctionsConstants.GameCubeStickUpLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.GameCubeStickDown,
            InputSettingsFunctionsConstants.GameCubeStickDownLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.GameCubeStickLeft,
            InputSettingsFunctionsConstants.GameCubeStickLeftLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.GameCubeStickRight,
            InputSettingsFunctionsConstants.GameCubeStickRightLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.GameCubeCUp,
            InputSettingsFunctionsConstants.GameCubeCUpLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.GameCubeCDown,
            InputSettingsFunctionsConstants.GameCubeCDownLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.GameCubeCLeft,
            InputSettingsFunctionsConstants.GameCubeCLeftLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.GameCubeCRight,
            InputSettingsFunctionsConstants.GameCubeCRightLabel, string.Empty)
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
