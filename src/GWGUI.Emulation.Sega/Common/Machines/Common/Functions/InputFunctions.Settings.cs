using GWGUI.Emulation;
namespace GWGUI.Emulation.Sega.Common.Machines.Common.Functions;

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
            var type = ControllerCatalog.Normalize(model, current?.Type ?? ControllerType.Automatic);
            var visualId = NormalizeVisualId(type, current?.VisualId);
            return new EmulationControllerPort(index + 1,
                ControllerCatalog.Types(model).Select(Choice).ToArray(), type.ToString(),
                current?.DeviceId,
                new EmulationInputBindingSet(ControllerDefinitions(type), current?.ButtonMappings ?? new Dictionary<string, string>(), EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller, true),
                VisualId: visualId);
        }).ToArray();
        return new EmulationInputSettings(keyboard, mouse, ports);
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationInputSettings settings)
    {
        var current = configuration.Input ?? new InputConfiguration();
        var keyboard = settings.Keyboard?.Values ?? new Dictionary<string, string>();
        var mouse = EmulationSdk.InputProcessing.CreateMouseMappings(settings.Mouse, MouseAction.None) ?? new Dictionary<string, MouseAction>();
        var model = ModelCatalog.Get(configuration.Model);
        var controllers = settings.ControllerPorts.Select(port => new ControllerBinding(
            port.Number - 1,
            ControllerCatalog.Normalize(model, Enum.TryParse<ControllerType>(port.SelectedControllerId, true, out var type) ? type : ControllerType.None),
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

    private static IReadOnlyList<InputBindingDefinition> ControllerDefinitions(ControllerType type)
    {
        var definitions = DigitalControllerDefinitions(type);
        if (type is ControllerType.SegaSaturnMissionStick or ControllerType.SegaSaturnArcadeRacer)
            definitions = DigitalControllerDefinitions(ControllerType.SegaSaturnController)
                .Where(item => item.Id is not EmulationControllerCommandIds.Up and not EmulationControllerCommandIds.Down
                    and not EmulationControllerCommandIds.Left and not EmulationControllerCommandIds.Right).ToArray();
        if (type == ControllerType.SegaDreamcastFishingController)
            definitions = DigitalControllerDefinitions(ControllerType.SegaDreamcastController)
                .Where(item => item.Id is not EmulationControllerCommandIds.Up and not EmulationControllerCommandIds.Down
                    and not EmulationControllerCommandIds.Left and not EmulationControllerCommandIds.Right).ToArray();
        if (type is not ControllerType.SegaSaturnThreeDControlPad and not ControllerType.SegaDreamcastController and not ControllerType.SegaPaddleControl
            and not ControllerType.SegaSaturnMissionStick and not ControllerType.SegaSaturnArcadeRacer and not ControllerType.SegaXe1Ap and not ControllerType.SegaSportsPad
            and not ControllerType.SegaDreamcastFishingController and not ControllerType.SegaDreamcastMaracas)
            return definitions;
        var horizontal = new[]
        {
            new InputBindingDefinition(EmulationControllerCommandIds.LeftStickLeft, string.Empty, string.Empty, "X−") { OppositeDirectionId = EmulationControllerCommandIds.LeftStickRight },
            new InputBindingDefinition(EmulationControllerCommandIds.LeftStickRight, string.Empty, string.Empty, "X+") { OppositeDirectionId = EmulationControllerCommandIds.LeftStickLeft }
        };
        if (type is ControllerType.SegaPaddleControl or ControllerType.SegaSaturnArcadeRacer) return definitions.Concat(horizontal).ToArray();
        var axes = definitions.Concat(horizontal).Concat(new[]
        {
            new InputBindingDefinition(EmulationControllerCommandIds.LeftStickUp, string.Empty, string.Empty, "Y−") { OppositeDirectionId = EmulationControllerCommandIds.LeftStickDown },
            new InputBindingDefinition(EmulationControllerCommandIds.LeftStickDown, string.Empty, string.Empty, "Y+") { OppositeDirectionId = EmulationControllerCommandIds.LeftStickUp }
        }).ToArray();
        if (type is not ControllerType.SegaSaturnMissionStick and not ControllerType.SegaXe1Ap
            and not ControllerType.SegaDreamcastFishingController and not ControllerType.SegaDreamcastMaracas) return axes;
        return axes.Concat(new[]
        {
            new InputBindingDefinition(EmulationControllerCommandIds.RightStickUp, string.Empty, string.Empty, "RY−") { OppositeDirectionId = EmulationControllerCommandIds.RightStickDown },
            new InputBindingDefinition(EmulationControllerCommandIds.RightStickDown, string.Empty, string.Empty, "RY+") { OppositeDirectionId = EmulationControllerCommandIds.RightStickUp }
        }).Concat(type is ControllerType.SegaXe1Ap or ControllerType.SegaDreamcastFishingController or ControllerType.SegaDreamcastMaracas ? new[]
        {
            new InputBindingDefinition(EmulationControllerCommandIds.RightStickLeft, string.Empty, string.Empty, "RX−") { OppositeDirectionId = EmulationControllerCommandIds.RightStickRight },
            new InputBindingDefinition(EmulationControllerCommandIds.RightStickRight, string.Empty, string.Empty, "RX+") { OppositeDirectionId = EmulationControllerCommandIds.RightStickLeft }
        } : Array.Empty<InputBindingDefinition>()).ToArray();
    }

    private static IReadOnlyList<InputBindingDefinition> DigitalControllerDefinitions(ControllerType type) =>
        type switch
        {
            ControllerType.None => [],
            ControllerType.SegaMasterSystemController or ControllerType.SegaControlStick =>
            [
                Definition(InputSettingsFunctionsConstants.Up, InputSettingsFunctionsConstants.ResourceControllerActionUp, string.Empty),
                Definition(InputSettingsFunctionsConstants.Down, InputSettingsFunctionsConstants.ResourceControllerActionDown, string.Empty),
                Definition(InputSettingsFunctionsConstants.Left, InputSettingsFunctionsConstants.ResourceControllerActionLeft, string.Empty),
                Definition(InputSettingsFunctionsConstants.Right, InputSettingsFunctionsConstants.ResourceControllerActionRight, string.Empty),
                Definition(InputSettingsFunctionsConstants.B, InputSettingsFunctionsConstants.ButtonOne, string.Empty),
                Definition(InputSettingsFunctionsConstants.A, InputSettingsFunctionsConstants.ButtonTwo, string.Empty)
            ],
            ControllerType.SegaGameGearController =>
            [
                Definition(InputSettingsFunctionsConstants.Up, InputSettingsFunctionsConstants.ResourceControllerActionUp, string.Empty),
                Definition(InputSettingsFunctionsConstants.Down, InputSettingsFunctionsConstants.ResourceControllerActionDown, string.Empty),
                Definition(InputSettingsFunctionsConstants.Left, InputSettingsFunctionsConstants.ResourceControllerActionLeft, string.Empty),
                Definition(InputSettingsFunctionsConstants.Right, InputSettingsFunctionsConstants.ResourceControllerActionRight, string.Empty),
                Definition(InputSettingsFunctionsConstants.B, InputSettingsFunctionsConstants.ButtonOne, string.Empty),
                Definition(InputSettingsFunctionsConstants.A, InputSettingsFunctionsConstants.ButtonTwo, string.Empty),
                Definition(InputSettingsFunctionsConstants.Start, InputSettingsFunctionsConstants.ButtonStart, string.Empty)
            ],
            ControllerType.SegaMegaDriveThreeButton or ControllerType.SegaMegaDriveSixButton
                or ControllerType.SegaArcadePowerStick or ControllerType.SegaArcadePowerStickSixButton =>
                MegaDriveDefinitions(type is ControllerType.SegaMegaDriveSixButton or ControllerType.SegaArcadePowerStickSixButton),
            ControllerType.SegaXe1Ap =>
            [
                Definition(EmulationControllerCommandIds.R, string.Empty, string.Empty, "A"),
                Definition(EmulationControllerCommandIds.R2, string.Empty, string.Empty, "B"),
                Definition(EmulationControllerCommandIds.L, string.Empty, string.Empty, "C"),
                Definition(EmulationControllerCommandIds.L2, string.Empty, string.Empty, "D"),
                Definition(EmulationControllerCommandIds.Y, string.Empty, string.Empty, "E1"),
                Definition(EmulationControllerCommandIds.B, string.Empty, string.Empty, "E2"),
                Definition(EmulationControllerCommandIds.Select, string.Empty, string.Empty, "SELECT"),
                Definition(EmulationControllerCommandIds.Start, string.Empty, string.Empty, "START")
            ],
            ControllerType.SegaSaturnController or ControllerType.SegaSaturnThreeDControlPad
                or ControllerType.SegaSaturnVirtuaStick =>
            [
                Definition(InputSettingsFunctionsConstants.Up, InputSettingsFunctionsConstants.ResourceControllerActionUp, string.Empty),
                Definition(InputSettingsFunctionsConstants.Down, InputSettingsFunctionsConstants.ResourceControllerActionDown, string.Empty),
                Definition(InputSettingsFunctionsConstants.Left, InputSettingsFunctionsConstants.ResourceControllerActionLeft, string.Empty),
                Definition(InputSettingsFunctionsConstants.Right, InputSettingsFunctionsConstants.ResourceControllerActionRight, string.Empty),
                Definition(InputSettingsFunctionsConstants.B, InputSettingsFunctionsConstants.ButtonA, string.Empty),
                Definition(InputSettingsFunctionsConstants.A, InputSettingsFunctionsConstants.ButtonB, string.Empty),
                Definition(InputSettingsFunctionsConstants.R, InputSettingsFunctionsConstants.ButtonC, string.Empty),
                Definition(InputSettingsFunctionsConstants.Y, InputSettingsFunctionsConstants.ButtonX, string.Empty),
                Definition(InputSettingsFunctionsConstants.X, InputSettingsFunctionsConstants.ButtonY, string.Empty),
                Definition(InputSettingsFunctionsConstants.L, InputSettingsFunctionsConstants.ButtonZ, string.Empty),
                Definition(InputSettingsFunctionsConstants.Start, InputSettingsFunctionsConstants.ButtonStart, string.Empty),
                Definition(InputSettingsFunctionsConstants.L2, InputSettingsFunctionsConstants.TriggerLeft, string.Empty),
                Definition(InputSettingsFunctionsConstants.R2, InputSettingsFunctionsConstants.TriggerRight, string.Empty)
            ],
            ControllerType.SegaDreamcastMaracas =>
            [
                Definition(EmulationControllerCommandIds.B, string.Empty, string.Empty, "A"),
                Definition(EmulationControllerCommandIds.A, string.Empty, string.Empty, "B"),
                Definition(EmulationControllerCommandIds.X, string.Empty, string.Empty, "C"),
                Definition(EmulationControllerCommandIds.L, string.Empty, string.Empty, "D"),
                Definition(EmulationControllerCommandIds.R, string.Empty, string.Empty, "Z"),
                Definition(EmulationControllerCommandIds.Start, string.Empty, string.Empty, "START")
            ],
            ControllerType.SegaDreamcastController =>
            [
                Definition(InputSettingsFunctionsConstants.Up, InputSettingsFunctionsConstants.ResourceControllerActionUp, string.Empty),
                Definition(InputSettingsFunctionsConstants.Down, InputSettingsFunctionsConstants.ResourceControllerActionDown, string.Empty),
                Definition(InputSettingsFunctionsConstants.Left, InputSettingsFunctionsConstants.ResourceControllerActionLeft, string.Empty),
                Definition(InputSettingsFunctionsConstants.Right, InputSettingsFunctionsConstants.ResourceControllerActionRight, string.Empty),
                Definition(InputSettingsFunctionsConstants.B, InputSettingsFunctionsConstants.ButtonA, string.Empty),
                Definition(InputSettingsFunctionsConstants.A, InputSettingsFunctionsConstants.ButtonB, string.Empty),
                Definition(InputSettingsFunctionsConstants.Y, InputSettingsFunctionsConstants.ButtonX, string.Empty),
                Definition(InputSettingsFunctionsConstants.X, InputSettingsFunctionsConstants.ButtonY, string.Empty),
                Definition(InputSettingsFunctionsConstants.Start, InputSettingsFunctionsConstants.ButtonStart, string.Empty),
                Definition(InputSettingsFunctionsConstants.L2, InputSettingsFunctionsConstants.TriggerLeft, string.Empty),
                Definition(InputSettingsFunctionsConstants.R2, InputSettingsFunctionsConstants.TriggerRight, string.Empty)
            ],
            _ =>
            [
            Definition(InputSettingsFunctionsConstants.Up, InputSettingsFunctionsConstants.ResourceControllerActionUp, string.Empty),
            Definition(InputSettingsFunctionsConstants.Down, InputSettingsFunctionsConstants.ResourceControllerActionDown, string.Empty),
            Definition(InputSettingsFunctionsConstants.Left, InputSettingsFunctionsConstants.ResourceControllerActionLeft, string.Empty),
            Definition(InputSettingsFunctionsConstants.Right, InputSettingsFunctionsConstants.ResourceControllerActionRight, string.Empty),
            Definition(InputSettingsFunctionsConstants.B, InputSettingsFunctionsConstants.ResourceControllerActionFire1, string.Empty),
            Definition(InputSettingsFunctionsConstants.A, InputSettingsFunctionsConstants.ResourceControllerActionFire2, string.Empty)
            ]
        };

    private static IReadOnlyList<InputBindingDefinition> MegaDriveDefinitions(bool sixButton)
    {
        var definitions = new List<InputBindingDefinition>
        {
            Definition(InputSettingsFunctionsConstants.Up, InputSettingsFunctionsConstants.ResourceControllerActionUp, string.Empty),
            Definition(InputSettingsFunctionsConstants.Down, InputSettingsFunctionsConstants.ResourceControllerActionDown, string.Empty),
            Definition(InputSettingsFunctionsConstants.Left, InputSettingsFunctionsConstants.ResourceControllerActionLeft, string.Empty),
            Definition(InputSettingsFunctionsConstants.Right, InputSettingsFunctionsConstants.ResourceControllerActionRight, string.Empty),
            Definition(InputSettingsFunctionsConstants.Y, InputSettingsFunctionsConstants.ButtonA, string.Empty),
            Definition(InputSettingsFunctionsConstants.B, InputSettingsFunctionsConstants.ButtonB, string.Empty),
            Definition(InputSettingsFunctionsConstants.A, InputSettingsFunctionsConstants.ButtonC, string.Empty),
            Definition(InputSettingsFunctionsConstants.Start, InputSettingsFunctionsConstants.ButtonStart, string.Empty)
        };
        if (sixButton)
        {
            definitions.Add(Definition(InputSettingsFunctionsConstants.L, InputSettingsFunctionsConstants.ButtonX, string.Empty));
            definitions.Add(Definition(InputSettingsFunctionsConstants.X, InputSettingsFunctionsConstants.ButtonY, string.Empty));
            definitions.Add(Definition(InputSettingsFunctionsConstants.R, InputSettingsFunctionsConstants.ButtonZ, string.Empty));
            definitions.Add(Definition(InputSettingsFunctionsConstants.Select, InputSettingsFunctionsConstants.ButtonMode, string.Empty));
        }
        return definitions;
    }

    private static InputBindingDefinition Definition(string id, string resourceKey, string defaultBinding, string? invariant = null) => new(id, resourceKey, defaultBinding, EmulationSdk.InputProcessing.BindingDisplayValue(resourceKey, invariant));

    private static EmulationControllerChoice Choice(ControllerType type) => new(
        type.ToString(), ControllerResourceKey(type), BindingDefinitions: ControllerDefinitions(type),
        CompatibleVisualIds: CompatibleVisualIds(type), DefaultVisualId: DefaultVisualId(type),
        VisualCommandIds: VisualCommandIds(type))
    {
        Category = type switch
        {
            ControllerType.None => EmulationControllerCategory.None,
            ControllerType.Automatic => EmulationControllerCategory.Unspecified,
            ControllerType.SegaSg1000Joystick or ControllerType.SegaControlStick or ControllerType.SegaArcadePowerStick
                or ControllerType.SegaArcadePowerStickSixButton or ControllerType.SegaSaturnTwinStick or ControllerType.SegaSaturnVirtuaStick
                or ControllerType.SegaDreamcastArcadeStick or ControllerType.SegaDreamcastTwinStick or ControllerType.SegaPaddleControl => EmulationControllerCategory.Joystick,
            ControllerType.SegaXe1Ap or ControllerType.SegaSaturnMissionStick => EmulationControllerCategory.FlightSimulation,
            ControllerType.SegaHandleController or ControllerType.SegaSaturnArcadeRacer => EmulationControllerCategory.RacingSimulation,
            ControllerType.SegaLightPhaser or ControllerType.SegaMegaMouse or ControllerType.SegaMenacer or ControllerType.SegaSportsPad
                or ControllerType.SegaSaturnVirtuaGun or ControllerType.SegaSaturnShuttleMouse or ControllerType.SegaDreamcastMouse
                or ControllerType.SegaDreamcastLightGun => EmulationControllerCategory.PointingDevice,
            ControllerType.SegaSc3000Keyboard or ControllerType.SegaDreamcastKeyboard or ControllerType.SegaActivator
                or ControllerType.SegaDreamcastFishingController or ControllerType.SegaDreamcastMaracas => EmulationControllerCategory.OtherSimulation,
            _ => EmulationControllerCategory.Gamepad
        }
    };
}
