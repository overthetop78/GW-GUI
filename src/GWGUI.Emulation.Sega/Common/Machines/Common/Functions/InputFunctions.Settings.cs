using GWGUI.Emulation;
namespace GWGUI.Emulation.Sega.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    internal static EmulationInputSettings Describe(MachineConfiguration configuration)
    {
        var model = ModelCatalog.Get(configuration.Model);
        var input = configuration.Input ?? new InputConfiguration();
        var keyboard = model.HasKeyboard ? new EmulationInputBindingSet(KeyboardDefinitions(model), input.KeyboardBindings ?? EmulationSdk.InputProcessing.ToStrings(input.KeyboardMappings), EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller) : null;
        var mouse = model.MouseButtonCount > 0 ? new EmulationInputBindingSet(MouseDefinitions(), EmulationSdk.InputProcessing.MouseBindings(input.MouseButtonMappings, MouseAction.None), EmulationInputSource.Mouse | EmulationInputSource.Keyboard | EmulationInputSource.Controller, true) : null;
        var configured = input.ControllerBindings ?? [];
        var ports = Enumerable.Range(0, model.ControllerPortCount).Select(index =>
        {
            var current = configured.FirstOrDefault(item => item.Port == index);
            var type = ControllerCatalog.Normalize(model, current?.Type ?? PeripheralCategory.Automatic);
            var visualId = NormalizeVisualId(type, current?.VisualId);
            return new EmulationControllerPort(index + 1,
                ControllerCatalog.Types(model).Select(Choice).ToArray(), type.ToString(),
                current?.DeviceId,
                new EmulationInputBindingSet(ControllerDefinitions(type), current?.ButtonMappings ?? new Dictionary<string, string>(), EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller, true),
                VisualId: visualId) { OutputBindings = current?.OutputBindings ?? new Dictionary<string, EmulationControllerOutputBinding>() };
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
            ControllerCatalog.Normalize(model, Enum.TryParse<PeripheralCategory>(port.SelectedControllerId, true, out var type) ? type : PeripheralCategory.None),
            port.PhysicalDeviceId,
            EmulationSdk.InputProcessing.CopyNonEmptyBindings(port.Bindings.Values),
            port.VisualId, OutputBindings: port.OutputBindings)).ToArray();
        return configuration with
        {
            Input = current with
            {
                KeyboardBindings = keyboard,
                KeyboardMappings = EmulationSdk.InputProcessing.ToKeys(keyboard),
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

    private static IReadOnlyList<InputBindingDefinition> ControllerDefinitions(PeripheralCategory type)
    {
        var definitions = DigitalControllerDefinitions(type);
        if (type is PeripheralCategory.SegaSaturnMissionStick or PeripheralCategory.SegaSaturnArcadeRacer)
            definitions = DigitalControllerDefinitions(PeripheralCategory.SegaSaturnController)
                .Where(item => item.Id is not EmulationControllerCommandIds.Up and not EmulationControllerCommandIds.Down
                    and not EmulationControllerCommandIds.Left and not EmulationControllerCommandIds.Right).ToArray();
        if (type == PeripheralCategory.SegaDreamcastFishingController)
            definitions = DigitalControllerDefinitions(PeripheralCategory.SegaDreamcastController)
                .Where(item => item.Id is not EmulationControllerCommandIds.Up and not EmulationControllerCommandIds.Down
                    and not EmulationControllerCommandIds.Left and not EmulationControllerCommandIds.Right).ToArray();
        if (type is not PeripheralCategory.SegaSaturnThreeDControlPad and not PeripheralCategory.SegaDreamcastController and not PeripheralCategory.SegaPaddleControl
            and not PeripheralCategory.SegaSaturnMissionStick and not PeripheralCategory.SegaSaturnArcadeRacer and not PeripheralCategory.SegaXe1Ap and not PeripheralCategory.SegaSportsPad
            and not PeripheralCategory.SegaDreamcastFishingController and not PeripheralCategory.SegaDreamcastMaracas)
            return definitions;
        var horizontal = new[]
        {
            new InputBindingDefinition(EmulationControllerCommandIds.LeftStickLeft, string.Empty, string.Empty, "X−") { OppositeDirectionId = EmulationControllerCommandIds.LeftStickRight },
            new InputBindingDefinition(EmulationControllerCommandIds.LeftStickRight, string.Empty, string.Empty, "X+") { OppositeDirectionId = EmulationControllerCommandIds.LeftStickLeft }
        };
        if (type is PeripheralCategory.SegaPaddleControl or PeripheralCategory.SegaSaturnArcadeRacer) return definitions.Concat(horizontal).ToArray();
        var axes = definitions.Concat(horizontal).Concat(new[]
        {
            new InputBindingDefinition(EmulationControllerCommandIds.LeftStickUp, string.Empty, string.Empty, "Y−") { OppositeDirectionId = EmulationControllerCommandIds.LeftStickDown },
            new InputBindingDefinition(EmulationControllerCommandIds.LeftStickDown, string.Empty, string.Empty, "Y+") { OppositeDirectionId = EmulationControllerCommandIds.LeftStickUp }
        }).ToArray();
        if (type is not PeripheralCategory.SegaSaturnMissionStick and not PeripheralCategory.SegaXe1Ap
            and not PeripheralCategory.SegaDreamcastFishingController and not PeripheralCategory.SegaDreamcastMaracas) return axes;
        return axes.Concat(new[]
        {
            new InputBindingDefinition(EmulationControllerCommandIds.RightStickUp, string.Empty, string.Empty, "RY−") { OppositeDirectionId = EmulationControllerCommandIds.RightStickDown },
            new InputBindingDefinition(EmulationControllerCommandIds.RightStickDown, string.Empty, string.Empty, "RY+") { OppositeDirectionId = EmulationControllerCommandIds.RightStickUp }
        }).Concat(type is PeripheralCategory.SegaXe1Ap or PeripheralCategory.SegaDreamcastFishingController or PeripheralCategory.SegaDreamcastMaracas ? new[]
        {
            new InputBindingDefinition(EmulationControllerCommandIds.RightStickLeft, string.Empty, string.Empty, "RX−") { OppositeDirectionId = EmulationControllerCommandIds.RightStickRight },
            new InputBindingDefinition(EmulationControllerCommandIds.RightStickRight, string.Empty, string.Empty, "RX+") { OppositeDirectionId = EmulationControllerCommandIds.RightStickLeft }
        } : Array.Empty<InputBindingDefinition>()).ToArray();
    }

    private static IReadOnlyList<InputBindingDefinition> DigitalControllerDefinitions(PeripheralCategory type) =>
        type switch
        {
            PeripheralCategory.None => [],
            PeripheralCategory.SegaMasterSystemController or PeripheralCategory.SegaControlStick =>
            [
                Definition(InputSettingsFunctionsConstants.Up, InputSettingsFunctionsConstants.ResourceControllerActionUp, string.Empty),
                Definition(InputSettingsFunctionsConstants.Down, InputSettingsFunctionsConstants.ResourceControllerActionDown, string.Empty),
                Definition(InputSettingsFunctionsConstants.Left, InputSettingsFunctionsConstants.ResourceControllerActionLeft, string.Empty),
                Definition(InputSettingsFunctionsConstants.Right, InputSettingsFunctionsConstants.ResourceControllerActionRight, string.Empty),
                Definition(InputSettingsFunctionsConstants.B, InputSettingsFunctionsConstants.ButtonOne, string.Empty),
                Definition(InputSettingsFunctionsConstants.A, InputSettingsFunctionsConstants.ButtonTwo, string.Empty)
            ],
            PeripheralCategory.SegaGameGearController =>
            [
                Definition(InputSettingsFunctionsConstants.Up, InputSettingsFunctionsConstants.ResourceControllerActionUp, string.Empty),
                Definition(InputSettingsFunctionsConstants.Down, InputSettingsFunctionsConstants.ResourceControllerActionDown, string.Empty),
                Definition(InputSettingsFunctionsConstants.Left, InputSettingsFunctionsConstants.ResourceControllerActionLeft, string.Empty),
                Definition(InputSettingsFunctionsConstants.Right, InputSettingsFunctionsConstants.ResourceControllerActionRight, string.Empty),
                Definition(InputSettingsFunctionsConstants.B, InputSettingsFunctionsConstants.ButtonOne, string.Empty),
                Definition(InputSettingsFunctionsConstants.A, InputSettingsFunctionsConstants.ButtonTwo, string.Empty),
                Definition(InputSettingsFunctionsConstants.Start, InputSettingsFunctionsConstants.ButtonStart, string.Empty)
            ],
            PeripheralCategory.SegaMegaDriveThreeButton or PeripheralCategory.SegaMegaDriveSixButton
                or PeripheralCategory.SegaArcadePowerStick or PeripheralCategory.SegaArcadePowerStickSixButton =>
                MegaDriveDefinitions(type is PeripheralCategory.SegaMegaDriveSixButton or PeripheralCategory.SegaArcadePowerStickSixButton),
            PeripheralCategory.SegaXe1Ap =>
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
            PeripheralCategory.SegaSaturnController or PeripheralCategory.SegaSaturnThreeDControlPad
                or PeripheralCategory.SegaSaturnVirtuaStick =>
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
            PeripheralCategory.SegaDreamcastMaracas =>
            [
                Definition(EmulationControllerCommandIds.B, string.Empty, string.Empty, "A"),
                Definition(EmulationControllerCommandIds.A, string.Empty, string.Empty, "B"),
                Definition(EmulationControllerCommandIds.X, string.Empty, string.Empty, "C"),
                Definition(EmulationControllerCommandIds.L, string.Empty, string.Empty, "D"),
                Definition(EmulationControllerCommandIds.R, string.Empty, string.Empty, "Z"),
                Definition(EmulationControllerCommandIds.Start, string.Empty, string.Empty, "START")
            ],
            PeripheralCategory.SegaDreamcastController =>
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

    private static EmulationControllerChoice Choice(PeripheralCategory type) => new(
        type.ToString(), ControllerResourceKey(type), BindingDefinitions: ControllerDefinitions(type),
        CompatibleVisualIds: CompatibleVisualIds(type), DefaultVisualId: DefaultVisualId(type),
        VisualCommandIds: VisualCommandIds(type))
    {
        Category = type switch
        {
            PeripheralCategory.None => EmulationControllerCategory.None,
            PeripheralCategory.Automatic => EmulationControllerCategory.Unspecified,
            PeripheralCategory.SegaSg1000Joystick or PeripheralCategory.SegaControlStick or PeripheralCategory.SegaArcadePowerStick
                or PeripheralCategory.SegaArcadePowerStickSixButton or PeripheralCategory.SegaSaturnTwinStick or PeripheralCategory.SegaSaturnVirtuaStick
                or PeripheralCategory.SegaDreamcastArcadeStick or PeripheralCategory.SegaDreamcastTwinStick or PeripheralCategory.SegaPaddleControl => EmulationControllerCategory.Joystick,
            PeripheralCategory.SegaXe1Ap or PeripheralCategory.SegaSaturnMissionStick => EmulationControllerCategory.FlightSimulation,
            PeripheralCategory.SegaHandleController or PeripheralCategory.SegaSaturnArcadeRacer => EmulationControllerCategory.RacingSimulation,
            PeripheralCategory.SegaLightPhaser or PeripheralCategory.SegaMegaMouse or PeripheralCategory.SegaMenacer or PeripheralCategory.SegaSportsPad
                or PeripheralCategory.SegaSaturnVirtuaGun or PeripheralCategory.SegaSaturnShuttleMouse or PeripheralCategory.SegaDreamcastMouse
                or PeripheralCategory.SegaDreamcastLightGun => EmulationControllerCategory.PointingDevice,
            PeripheralCategory.SegaSc3000Keyboard or PeripheralCategory.SegaDreamcastKeyboard or PeripheralCategory.SegaActivator
                or PeripheralCategory.SegaDreamcastFishingController or PeripheralCategory.SegaDreamcastMaracas => EmulationControllerCategory.OtherSimulation,
            _ => EmulationControllerCategory.Gamepad
        }
    };
}
