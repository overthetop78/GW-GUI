using GWGUI.Emulation.Nec.Common.Machines.PcFx.Constants;

namespace GWGUI.Emulation.Nec.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    internal static EmulationInputSettings Describe(MachineConfiguration configuration)
    {
        var model = ModelCatalog.Get(configuration.Model);
        var input = configuration.Input ?? new InputConfiguration();
        var mouseType = model.Id == PcFxMachineConstants.Id
            ? ControllerType.PcFxMouse : ControllerType.PcEngineMouse;
        var keyboard = model.HasKeyboard ? new EmulationInputBindingSet(
            KeyboardDefinitions(model), input.KeyboardBindings ?? ToStrings(input.KeyboardMappings),
            EmulationInputSource.Keyboard) : null;
        var mouse = model.MouseButtonCount > InputSettingsFunctionsConstants.FirstPortIndex
            ? new EmulationInputBindingSet(
            MouseDefinitions(), MouseValues(input), EmulationInputSource.Mouse
                | EmulationInputSource.Keyboard | EmulationInputSource.Controller, true,
            CompatibleVisualIds(mouseType), DefaultVisualId(mouseType),
            VisualCommandIds(mouseType)) : null;
        var configured = input.ControllerBindings ?? [];
        var ports = Enumerable.Range(InputSettingsFunctionsConstants.FirstPortIndex,
            model.ControllerPortCount).Select(index =>
        {
            var current = configured.FirstOrDefault(item => item.Port == index);
            var type = ControllerCatalog.Normalize(model,
                current?.Type ?? (index == InputSettingsFunctionsConstants.FirstPortIndex
                    ? ControllerCatalog.Default(model)
                    : ControllerType.None));
            return new EmulationControllerPort(index
                    + InputSettingsFunctionsConstants.FirstPortNumber,
                ControllerCatalog.Types(model).Select(Choice).ToArray(), type.ToString(),
                current?.DeviceId,
                new EmulationInputBindingSet(ControllerDefinitions(type),
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
            port.Number - InputSettingsFunctionsConstants.FirstPortNumber,
            Enum.TryParse<ControllerType>(port.SelectedControllerId, true, out var type)
                ? ControllerCatalog.Normalize(ModelCatalog.Get(configuration.Model), type)
                : port.Number == InputSettingsFunctionsConstants.FirstPortNumber
                    ? ControllerCatalog.Default(ModelCatalog.Get(configuration.Model))
                    : ControllerType.None,
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

    private static IReadOnlyList<InputBindingDefinition> ControllerDefinitions(ControllerType type)
    {
        if (type == ControllerType.None || ControllerCatalog.IsMouse(type)) return [];
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
            Definition(InputSettingsFunctionsConstants.B,
                InputSettingsFunctionsConstants.ResourceActionII, string.Empty),
            Definition(InputSettingsFunctionsConstants.A,
                InputSettingsFunctionsConstants.ResourceActionI, string.Empty),
            Definition(EmulationControllerCommandIds.Option,
                InputSettingsFunctionsConstants.ResourceActionSelect, string.Empty),
            Definition(EmulationControllerCommandIds.Start,
                InputSettingsFunctionsConstants.ResourceActionRun, string.Empty)
        };
        if (ControllerCatalog.HasSixButtons(type))
        {
            definitions.Add(Definition(EmulationControllerCommandIds.Y,
                InputSettingsFunctionsConstants.ResourceActionIII, string.Empty));
            definitions.Add(Definition(EmulationControllerCommandIds.X,
                InputSettingsFunctionsConstants.ResourceActionIV, string.Empty));
            definitions.Add(Definition(EmulationControllerCommandIds.L,
                InputSettingsFunctionsConstants.ResourceActionV, string.Empty));
            definitions.Add(Definition(EmulationControllerCommandIds.R,
                InputSettingsFunctionsConstants.ResourceActionVI, string.Empty));
            definitions.Add(Definition(EmulationControllerCommandIds.L2,
                InputSettingsFunctionsConstants.ResourceActionMode, string.Empty));
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
