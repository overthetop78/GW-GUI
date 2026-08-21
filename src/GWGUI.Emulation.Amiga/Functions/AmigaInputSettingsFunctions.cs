using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga;

internal static class AmigaInputSettingsFunctions
{
    internal static EmulationInputSettings Describe(AmigaMachineConfiguration configuration)
    {
        var input = configuration.Input ?? new AmigaInputConfiguration();
        var model = AmigaModelCatalog.Get(configuration.Model);
        var keyboard = new EmulationInputBindingSet(KeyboardDefinitions(),
            input.KeyboardBindings ?? ToStrings(input.KeyboardMappings), EmulationInputSource.Keyboard);
        var mouse = new EmulationInputBindingSet(MouseDefinitions(model),
            (input.MouseButtonMappings ?? new Dictionary<string, AmigaMouseAction>())
                .Where(item => item.Value != AmigaMouseAction.None)
                .ToDictionary(item => item.Value.ToString(), item => item.Key, StringComparer.Ordinal),
            EmulationInputSource.Mouse | EmulationInputSource.Keyboard);
        var configured = input.ControllerBindings ?? [];
        var ports = Enumerable.Range(0, model.ControllerPortCount).Select(index =>
        {
            var number = index + 1;
            var current = configured.FirstOrDefault(item => item.Port == number);
            var type = current?.Type ?? AmigaControllerCatalog.Default(model);
            return new EmulationControllerPort(number,
                AmigaControllerCatalog.Types(model).Select(Choice).ToArray(), type.ToString(),
                current?.DeviceId,
                new EmulationInputBindingSet(ControllerDefinitions(type), current?.ButtonMappings
                    ?? new Dictionary<string, string>(), EmulationInputSource.Keyboard | EmulationInputSource.Controller,
                    true));
        }).ToArray();
        return new EmulationInputSettings(keyboard, mouse, ports);
    }

    internal static AmigaMachineConfiguration Apply(AmigaMachineConfiguration configuration,
        EmulationInputSettings settings)
    {
        var current = configuration.Input ?? new AmigaInputConfiguration();
        var keyboard = settings.Keyboard?.Values ?? new Dictionary<string, string>();
        var mouse = settings.Mouse?.Values
            .Where(item => !string.IsNullOrWhiteSpace(item.Value)
                && Enum.TryParse<AmigaMouseAction>(item.Key, true, out var action)
                && action != AmigaMouseAction.None)
            .ToDictionary(item => item.Value, item => Enum.Parse<AmigaMouseAction>(item.Key, true),
                StringComparer.OrdinalIgnoreCase)
            ?? new Dictionary<string, AmigaMouseAction>();
        var controllers = settings.ControllerPorts.Select(port => new AmigaControllerBinding(port.Number,
            Enum.TryParse<AmigaControllerType>(port.SelectedControllerId, true, out var type)
                ? type : AmigaControllerType.None, port.PhysicalDeviceId, port.Bindings.Values)).ToArray();
        var input = current with
        {
            KeyboardBindings = keyboard,
            KeyboardMappings = ToKeys(keyboard),
            MouseButtonMappings = mouse,
            ControllerBindings = controllers
        };
        return configuration with { Input = input };
    }

    private static IReadOnlyList<InputBindingDefinition> KeyboardDefinitions()
    {
        var keys = Enumerable.Range(1, 10).Select(index => $"F{index}")
            .Select(key => Definition(key, key, key)).ToList();
        keys.Add(Definition(nameof(EmulationKey.Help), "Emulation.Key.Help", "Insert"));
        keys.Add(Definition(nameof(EmulationKey.LeftAmiga), "Emulation.Key.LeftAmiga", "PageUp"));
        keys.Add(Definition(nameof(EmulationKey.RightAmiga), "Emulation.Key.RightAmiga", "PageDown"));
        return keys;
    }

    private static IReadOnlyList<InputBindingDefinition> MouseDefinitions(AmigaModel model)
    {
        var definitions = new List<InputBindingDefinition>
        {
            Definition(nameof(AmigaMouseAction.LeftButton), "Emulation.Mouse.Button.Left", "Mouse:Left"),
            Definition(nameof(AmigaMouseAction.RightButton), "Emulation.Mouse.Button.Right", "Mouse:Right")
        };
        if (model.MouseButtonCount >= 3)
            definitions.Add(Definition(nameof(AmigaMouseAction.MiddleButton),
                "Emulation.Mouse.Button.Middle", "Mouse:Middle"));
        return definitions;
    }

    private static IReadOnlyList<InputBindingDefinition> ControllerDefinitions(AmigaControllerType type)
    {
        if (type is AmigaControllerType.None or AmigaControllerType.Keyboard) return [];
        var definitions = new List<InputBindingDefinition>
        {
            Definition("Up", "Emulation.Controller.Action.Up", string.Empty),
            Definition("Down", "Emulation.Controller.Action.Down", string.Empty),
            Definition("Left", "Emulation.Controller.Action.Left", string.Empty),
            Definition("Right", "Emulation.Controller.Action.Right", string.Empty)
        };
        if (type == AmigaControllerType.Cd32Pad)
        {
            definitions.AddRange([
                Definition("B", "Emulation.Amiga.Controller.Cd32.Red", string.Empty),
                Definition("A", "Emulation.Amiga.Controller.Cd32.Blue", string.Empty),
                Definition("Y", "Emulation.Amiga.Controller.Cd32.Green", string.Empty),
                Definition("X", "Emulation.Amiga.Controller.Cd32.Yellow", string.Empty),
                Definition("L", "Emulation.Amiga.Controller.Cd32.Rewind", string.Empty),
                Definition("R", "Emulation.Amiga.Controller.Cd32.FastForward", string.Empty),
                Definition("Start", "Emulation.Amiga.Controller.Cd32.PlayPause", string.Empty)
            ]);
        }
        else
        {
            definitions.Add(Definition("B", "Emulation.Controller.Action.Fire1", string.Empty));
            definitions.Add(Definition("A", "Emulation.Controller.Action.Fire2", string.Empty));
        }
        definitions.Add(Definition("L2", "Emulation.Controller.Action.TurboFire", string.Empty));
        return definitions;
    }

    private static InputBindingDefinition Definition(string id, string resourceKey, string defaultBinding) =>
        new(id, resourceKey, defaultBinding, resourceKey.Contains('.') ? null : resourceKey);

    private static EmulationControllerChoice Choice(AmigaControllerType type) =>
        new(type.ToString(), ControllerResourceKey(type), BindingDefinitions: ControllerDefinitions(type));

    private static string ControllerResourceKey(AmigaControllerType type) => type switch
    {
        AmigaControllerType.Joystick => "Emulation.Amiga.Controller.Joystick",
        AmigaControllerType.AnalogJoystick => "Emulation.Controller.AnalogJoystick",
        AmigaControllerType.Cd32Pad => "Emulation.Amiga.Controller.Cd32",
        AmigaControllerType.Automatic => "Emulation.Controller.Automatic",
        AmigaControllerType.None => "Emulation.Controller.None",
        _ => $"Emulation.Controller.{type}"
    };

    private static IReadOnlyDictionary<string, string> ToStrings(
        IReadOnlyDictionary<string, EmulationKey>? values) => values?.ToDictionary(item => item.Key,
        item => item.Value.ToString(), StringComparer.Ordinal) ?? new Dictionary<string, string>();

    private static IReadOnlyDictionary<string, EmulationKey> ToKeys(IReadOnlyDictionary<string, string> values) =>
        values.Where(item => Enum.TryParse<EmulationKey>(item.Value, true, out _)).ToDictionary(item => item.Key,
            item => Enum.Parse<EmulationKey>(item.Value, true), StringComparer.Ordinal);
}
