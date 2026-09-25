using GWGUI.Emulation;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Enums;

namespace GWGUI.Emulation.Amiga.Common.Functions;

internal static partial class InputSettingsFunctions
{
    internal static EmulationInputSettings Describe(MachineConfiguration configuration)
    {
        var input = configuration.Input ?? new InputConfiguration();
        var model = ModelCatalog.Get(configuration.Model);
        var keyboard = new EmulationInputBindingSet(KeyboardDefinitions(),
            input.KeyboardBindings ?? ToStrings(input.KeyboardMappings), EmulationInputSource.Keyboard);
        var mouse = new EmulationInputBindingSet(MouseDefinitions(model),
            (input.MouseButtonMappings ?? new Dictionary<string, MouseAction>())
                .Where(item => item.Value != MouseAction.None)
                .ToDictionary(item => item.Value.ToString(), item => item.Key, StringComparer.Ordinal),
            EmulationInputSource.Mouse | EmulationInputSource.Keyboard);
        var configured = input.ControllerBindings ?? [];
        var portCount = model.ControllerPortCount + (input.ParallelJoystickAdapterEnabled ? 2 : 0);
        var ports = Enumerable.Range(0, portCount).Select(index =>
        {
            var number = index + 1;
            var current = configured.FirstOrDefault(item => item.Port == index);
            var type = current?.Type ?? (index < model.ControllerPortCount
                ? ControllerCatalog.Default(model) : ControllerType.Joystick);
            var choices = index < model.ControllerPortCount
                ? ControllerCatalog.Types(model) : ControllerCatalog.ParallelPortTypes;
            return new EmulationControllerPort(number,
                choices.Select(Choice).ToArray(), type.ToString(),
                current?.DeviceId,
                new EmulationInputBindingSet(ControllerDefinitions(type), current?.ButtonMappings
                    ?? new Dictionary<string, string>(), EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller,
                    true), VisualId: current?.VisualId);
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
                StringComparer.OrdinalIgnoreCase)
            ?? new Dictionary<string, MouseAction>();
        var controllers = settings.ControllerPorts.Select(port => new ControllerBinding(port.Number - 1,
            Enum.TryParse<ControllerType>(port.SelectedControllerId, true, out var type)
                ? type : ControllerType.None, port.PhysicalDeviceId, port.Bindings.Values,
            port.VisualId)).ToArray();
        var input = current with
        {
            KeyboardBindings = keyboard,
            KeyboardMappings = ToKeys(keyboard),
            MouseButtonMappings = mouse,
            ControllerBindings = controllers
        };
        var options = new Dictionary<string, string>(configuration.Options ?? new Dictionary<string, string>())
        {
            [InputSettingsFunctionsConstants.OptionTurboFire] = controllers.Any(binding => binding.ButtonMappings?
                .Any(item => item.Key == InputSettingsFunctionsConstants.L2 && !string.IsNullOrWhiteSpace(item.Value)) == true)
                ? InputSettingsFunctionsConstants.Enabled : InputSettingsFunctionsConstants.Disabled,
            [InputSettingsFunctionsConstants.OptionTurboFireButton] = InputSettingsFunctionsConstants.L2
        };
        return configuration with { Input = input, Options = options };
    }

    private static IReadOnlyList<InputBindingDefinition> KeyboardDefinitions()
    {
        var keys = Enumerable.Range(1, 10).Select(index => $"F{index}")
            .Select(key => Definition(key, key, key)).ToList();
        keys.Add(Definition(nameof(EmulationKey.Help), InputSettingsFunctionsConstants.ResourceKeyHelp, InputSettingsFunctionsConstants.Insert));
        keys.Add(Definition(nameof(EmulationKey.LeftAmiga), InputSettingsFunctionsConstants.ResourceKeyLeftAmiga, InputSettingsFunctionsConstants.PageUp));
        keys.Add(Definition(nameof(EmulationKey.RightAmiga), InputSettingsFunctionsConstants.ResourceKeyRightAmiga, InputSettingsFunctionsConstants.PageDown));
        return keys;
    }

    private static IReadOnlyList<InputBindingDefinition> MouseDefinitions(Model model)
    {
        var definitions = new List<InputBindingDefinition>
        {
            Definition(nameof(MouseAction.LeftButton), InputSettingsFunctionsConstants.ResourceMouseButtonLeft, InputSettingsFunctionsConstants.MouseLeft),
            Definition(nameof(MouseAction.RightButton), InputSettingsFunctionsConstants.ResourceMouseButtonRight, InputSettingsFunctionsConstants.MouseRight)
        };
        if (model.MouseButtonCount >= 3)
            definitions.Add(Definition(nameof(MouseAction.MiddleButton),
                InputSettingsFunctionsConstants.ResourceMouseButtonMiddle, InputSettingsFunctionsConstants.MouseMiddle));
        return definitions;
    }

    private static IReadOnlyList<InputBindingDefinition> ControllerDefinitions(ControllerType type)
    {
        if (type is ControllerType.None or ControllerType.Keyboard) return [];
        var definitions = new List<InputBindingDefinition>
        {
            Definition(InputSettingsFunctionsConstants.Up, InputSettingsFunctionsConstants.ResourceControllerActionUp, string.Empty),
            Definition(InputSettingsFunctionsConstants.Down, InputSettingsFunctionsConstants.ResourceControllerActionDown, string.Empty),
            Definition(InputSettingsFunctionsConstants.Left, InputSettingsFunctionsConstants.ResourceControllerActionLeft, string.Empty),
            Definition(InputSettingsFunctionsConstants.Right, InputSettingsFunctionsConstants.ResourceControllerActionRight, string.Empty)
        };
        if (type == ControllerType.Cd32Pad)
        {
            definitions.AddRange([
                Definition(InputSettingsFunctionsConstants.B, InputSettingsFunctionsConstants.ResourceAmigaControllerCd32Red, string.Empty),
                Definition(InputSettingsFunctionsConstants.A, InputSettingsFunctionsConstants.ResourceAmigaControllerCd32Blue, string.Empty),
                Definition(InputSettingsFunctionsConstants.Y, InputSettingsFunctionsConstants.ResourceAmigaControllerCd32Green, string.Empty),
                Definition(InputSettingsFunctionsConstants.X, InputSettingsFunctionsConstants.ResourceAmigaControllerCd32Yellow, string.Empty),
                Definition(InputSettingsFunctionsConstants.L, InputSettingsFunctionsConstants.ResourceAmigaControllerCd32Rewind, string.Empty),
                Definition(InputSettingsFunctionsConstants.R, InputSettingsFunctionsConstants.ResourceAmigaControllerCd32FastForward, string.Empty),
                Definition(InputSettingsFunctionsConstants.Start, InputSettingsFunctionsConstants.ResourceAmigaControllerCd32PlayPause, string.Empty)
            ]);
        }
        else
        {
            definitions.Add(Definition(InputSettingsFunctionsConstants.B, InputSettingsFunctionsConstants.ResourceControllerActionFire1, string.Empty));
            definitions.Add(Definition(InputSettingsFunctionsConstants.A, InputSettingsFunctionsConstants.ResourceControllerActionFire2, string.Empty));
        }
        definitions.Add(Definition(InputSettingsFunctionsConstants.L2, InputSettingsFunctionsConstants.ResourceControllerActionTurboFire, string.Empty));
        return definitions;
    }

    private static InputBindingDefinition Definition(string id, string resourceKey, string defaultBinding) =>
        new(id, resourceKey, defaultBinding, resourceKey.Contains('.') ? null : resourceKey);

    private static EmulationControllerChoice Choice(ControllerType type) =>
        new(type.ToString(), ControllerResourceKey(type),
            BindingDefinitions: ControllerDefinitions(type),
            CompatibleVisualIds: CompatibleVisualIds(type),
            DefaultVisualId: DefaultVisualId(type),
            VisualCommandIds: VisualCommandIds(type));
}
