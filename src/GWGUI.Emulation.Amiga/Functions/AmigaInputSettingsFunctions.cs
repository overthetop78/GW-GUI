using GWGUI.Emulation;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Enums;

namespace GWGUI.Emulation.Amiga.Functions;

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
        var portCount = model.ControllerPortCount + (input.ParallelJoystickAdapterEnabled ? 2 : 0);
        var ports = Enumerable.Range(0, portCount).Select(index =>
        {
            var number = index + 1;
            var current = configured.FirstOrDefault(item => item.Port == index);
            var type = current?.Type ?? (index < model.ControllerPortCount
                ? AmigaControllerCatalog.Default(model) : AmigaControllerType.Joystick);
            var choices = index < model.ControllerPortCount
                ? AmigaControllerCatalog.Types(model) : AmigaControllerCatalog.ParallelPortTypes;
            return new EmulationControllerPort(number,
                choices.Select(Choice).ToArray(), type.ToString(),
                current?.DeviceId,
                new EmulationInputBindingSet(ControllerDefinitions(type), current?.ButtonMappings
                    ?? new Dictionary<string, string>(), EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller,
                    true), VisualId: current?.VisualId);
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
        var controllers = settings.ControllerPorts.Select(port => new AmigaControllerBinding(port.Number - 1,
            Enum.TryParse<AmigaControllerType>(port.SelectedControllerId, true, out var type)
                ? type : AmigaControllerType.None, port.PhysicalDeviceId, port.Bindings.Values,
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
            [AmigaInputSettingsFunctionsConstants.OptionTurboFire] = controllers.Any(binding => binding.ButtonMappings?
                .Any(item => item.Key == AmigaInputSettingsFunctionsConstants.L2 && !string.IsNullOrWhiteSpace(item.Value)) == true)
                ? AmigaInputSettingsFunctionsConstants.Enabled : AmigaInputSettingsFunctionsConstants.Disabled,
            [AmigaInputSettingsFunctionsConstants.OptionTurboFireButton] = AmigaInputSettingsFunctionsConstants.L2
        };
        return configuration with { Input = input, Options = options };
    }

    private static IReadOnlyList<InputBindingDefinition> KeyboardDefinitions()
    {
        var keys = Enumerable.Range(1, 10).Select(index => $"F{index}")
            .Select(key => Definition(key, key, key)).ToList();
        keys.Add(Definition(nameof(EmulationKey.Help), AmigaInputSettingsFunctionsConstants.ResourceKeyHelp, AmigaInputSettingsFunctionsConstants.Insert));
        keys.Add(Definition(nameof(EmulationKey.LeftAmiga), AmigaInputSettingsFunctionsConstants.ResourceKeyLeftAmiga, AmigaInputSettingsFunctionsConstants.PageUp));
        keys.Add(Definition(nameof(EmulationKey.RightAmiga), AmigaInputSettingsFunctionsConstants.ResourceKeyRightAmiga, AmigaInputSettingsFunctionsConstants.PageDown));
        return keys;
    }

    private static IReadOnlyList<InputBindingDefinition> MouseDefinitions(AmigaModel model)
    {
        var definitions = new List<InputBindingDefinition>
        {
            Definition(nameof(AmigaMouseAction.LeftButton), AmigaInputSettingsFunctionsConstants.ResourceMouseButtonLeft, AmigaInputSettingsFunctionsConstants.MouseLeft),
            Definition(nameof(AmigaMouseAction.RightButton), AmigaInputSettingsFunctionsConstants.ResourceMouseButtonRight, AmigaInputSettingsFunctionsConstants.MouseRight)
        };
        if (model.MouseButtonCount >= 3)
            definitions.Add(Definition(nameof(AmigaMouseAction.MiddleButton),
                AmigaInputSettingsFunctionsConstants.ResourceMouseButtonMiddle, AmigaInputSettingsFunctionsConstants.MouseMiddle));
        return definitions;
    }

    private static IReadOnlyList<InputBindingDefinition> ControllerDefinitions(AmigaControllerType type)
    {
        if (type is AmigaControllerType.None or AmigaControllerType.Keyboard) return [];
        var definitions = new List<InputBindingDefinition>
        {
            Definition(AmigaInputSettingsFunctionsConstants.Up, AmigaInputSettingsFunctionsConstants.ResourceControllerActionUp, string.Empty),
            Definition(AmigaInputSettingsFunctionsConstants.Down, AmigaInputSettingsFunctionsConstants.ResourceControllerActionDown, string.Empty),
            Definition(AmigaInputSettingsFunctionsConstants.Left, AmigaInputSettingsFunctionsConstants.ResourceControllerActionLeft, string.Empty),
            Definition(AmigaInputSettingsFunctionsConstants.Right, AmigaInputSettingsFunctionsConstants.ResourceControllerActionRight, string.Empty)
        };
        if (type == AmigaControllerType.Cd32Pad)
        {
            definitions.AddRange([
                Definition(AmigaInputSettingsFunctionsConstants.B, AmigaInputSettingsFunctionsConstants.ResourceAmigaControllerCd32Red, string.Empty),
                Definition(AmigaInputSettingsFunctionsConstants.A, AmigaInputSettingsFunctionsConstants.ResourceAmigaControllerCd32Blue, string.Empty),
                Definition(AmigaInputSettingsFunctionsConstants.Y, AmigaInputSettingsFunctionsConstants.ResourceAmigaControllerCd32Green, string.Empty),
                Definition(AmigaInputSettingsFunctionsConstants.X, AmigaInputSettingsFunctionsConstants.ResourceAmigaControllerCd32Yellow, string.Empty),
                Definition(AmigaInputSettingsFunctionsConstants.L, AmigaInputSettingsFunctionsConstants.ResourceAmigaControllerCd32Rewind, string.Empty),
                Definition(AmigaInputSettingsFunctionsConstants.R, AmigaInputSettingsFunctionsConstants.ResourceAmigaControllerCd32FastForward, string.Empty),
                Definition(AmigaInputSettingsFunctionsConstants.Start, AmigaInputSettingsFunctionsConstants.ResourceAmigaControllerCd32PlayPause, string.Empty)
            ]);
        }
        else
        {
            definitions.Add(Definition(AmigaInputSettingsFunctionsConstants.B, AmigaInputSettingsFunctionsConstants.ResourceControllerActionFire1, string.Empty));
            definitions.Add(Definition(AmigaInputSettingsFunctionsConstants.A, AmigaInputSettingsFunctionsConstants.ResourceControllerActionFire2, string.Empty));
        }
        definitions.Add(Definition(AmigaInputSettingsFunctionsConstants.L2, AmigaInputSettingsFunctionsConstants.ResourceControllerActionTurboFire, string.Empty));
        return definitions;
    }

    private static InputBindingDefinition Definition(string id, string resourceKey, string defaultBinding) =>
        new(id, resourceKey, defaultBinding, resourceKey.Contains('.') ? null : resourceKey);

    private static EmulationControllerChoice Choice(AmigaControllerType type) =>
        new(type.ToString(), ControllerResourceKey(type),
            BindingDefinitions: ControllerDefinitions(type),
            CompatibleVisualIds: CompatibleVisualIds(type),
            DefaultVisualId: DefaultVisualId(type),
            VisualCommandIds: VisualCommandIds(type));

    private static IReadOnlyList<string>? CompatibleVisualIds(AmigaControllerType type) => type switch
    {
        AmigaControllerType.Joystick =>
        [
            EmulationControllerVisualIds.QuickShot,
            EmulationControllerVisualIds.QuickShotDeluxe,
            EmulationControllerVisualIds.QuickShotIiTurbo,
            EmulationControllerVisualIds.CompetitionPro5000,
            EmulationControllerVisualIds.ZipstikSuperPro,
            EmulationControllerVisualIds.KonixSpeedkingLeftHand,
            EmulationControllerVisualIds.KonixSpeedkingRightHand,
            EmulationControllerVisualIds.SuncomTac2,
            EmulationControllerVisualIds.PowerplayCruiser,
            EmulationControllerVisualIds.SuzoTheArcadeTurbo,
            EmulationControllerVisualIds.AdvancedGravisGamepad
        ],
        AmigaControllerType.AnalogJoystick => [EmulationControllerVisualIds.KonixSpeedkingAnalog],
        AmigaControllerType.Cd32Pad =>
            [EmulationControllerVisualIds.CommodoreCd32, EmulationControllerVisualIds.CompetitionProCd32],
        _ => null
    };

    private static string? DefaultVisualId(AmigaControllerType type) => type switch
    {
        AmigaControllerType.Joystick => EmulationControllerVisualIds.QuickShot,
        AmigaControllerType.AnalogJoystick => EmulationControllerVisualIds.KonixSpeedkingAnalog,
        AmigaControllerType.Cd32Pad => EmulationControllerVisualIds.CommodoreCd32,
        _ => null
    };

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string>? VisualCommandIds(
        AmigaControllerType type) => type switch
    {
        AmigaControllerType.Joystick or AmigaControllerType.AnalogJoystick =>
            new Dictionary<EmulationControllerVisualControl, string>
            {
                [EmulationControllerVisualControl.DirectionUp] = EmulationControllerCommandIds.Up,
                [EmulationControllerVisualControl.DirectionDown] = EmulationControllerCommandIds.Down,
                [EmulationControllerVisualControl.DirectionLeft] = EmulationControllerCommandIds.Left,
                [EmulationControllerVisualControl.DirectionRight] = EmulationControllerCommandIds.Right,
                [EmulationControllerVisualControl.PrimaryAction] = EmulationControllerCommandIds.B,
                [EmulationControllerVisualControl.SecondaryAction] = EmulationControllerCommandIds.A,
                [EmulationControllerVisualControl.Turbo] = EmulationControllerCommandIds.L2
            },
        AmigaControllerType.Cd32Pad => new Dictionary<EmulationControllerVisualControl, string>
        {
            [EmulationControllerVisualControl.DirectionUp] = EmulationControllerCommandIds.Up,
            [EmulationControllerVisualControl.DirectionDown] = EmulationControllerCommandIds.Down,
            [EmulationControllerVisualControl.DirectionLeft] = EmulationControllerCommandIds.Left,
            [EmulationControllerVisualControl.DirectionRight] = EmulationControllerCommandIds.Right,
            [EmulationControllerVisualControl.PrimaryAction] = EmulationControllerCommandIds.B,
            [EmulationControllerVisualControl.SecondaryAction] = EmulationControllerCommandIds.A,
            [EmulationControllerVisualControl.TertiaryAction] = EmulationControllerCommandIds.Y,
            [EmulationControllerVisualControl.QuaternaryAction] = EmulationControllerCommandIds.X,
            [EmulationControllerVisualControl.LeftShoulder] = EmulationControllerCommandIds.L,
            [EmulationControllerVisualControl.RightShoulder] = EmulationControllerCommandIds.R,
            [EmulationControllerVisualControl.Start] = EmulationControllerCommandIds.Start,
            [EmulationControllerVisualControl.Turbo] = EmulationControllerCommandIds.L2
        },
        _ => null
    };

    private static string ControllerResourceKey(AmigaControllerType type) => type switch
    {
        AmigaControllerType.Joystick => AmigaInputSettingsFunctionsConstants.ResourceAmigaControllerJoystick,
        AmigaControllerType.AnalogJoystick => AmigaInputSettingsFunctionsConstants.ResourceControllerAnalogJoystick,
        AmigaControllerType.Cd32Pad => AmigaInputSettingsFunctionsConstants.ResourceAmigaControllerCd32,
        AmigaControllerType.Automatic => AmigaInputSettingsFunctionsConstants.ResourceControllerAutomatic,
        AmigaControllerType.None => AmigaInputSettingsFunctionsConstants.ResourceControllerNone,
        _ => $"Emulation.Controller.{type}"
    };

    private static IReadOnlyDictionary<string, string> ToStrings(
        IReadOnlyDictionary<string, EmulationKey>? values) => values?.ToDictionary(item => item.Key,
        item => item.Value.ToString(), StringComparer.Ordinal) ?? new Dictionary<string, string>();

    private static IReadOnlyDictionary<string, EmulationKey> ToKeys(IReadOnlyDictionary<string, string> values) =>
        values.Where(item => Enum.TryParse<EmulationKey>(item.Value, true, out _)).ToDictionary(item => item.Key,
            item => Enum.Parse<EmulationKey>(item.Value, true), StringComparer.Ordinal);
}
