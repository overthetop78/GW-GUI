using GWGUI.Emulation;
using CommonControllerFunctions = GWGUI.Emulation.Sony.Common.Functions.ControllerFunctions;
namespace GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    internal static EmulationInputSettings Describe(MachineConfiguration configuration)
    {
        var model = ModelCatalog.Get(configuration.Model);
        if (!EmulatorCatalog.GetAll(model.Id).Any(adapter => adapter.Id == configuration.EmulatorId))
            return new EmulationInputSettings(null, null, []);
        var input = configuration.Input ?? new InputConfiguration();
        var usbKeyboard = HasUsbKeyboard(configuration);
        var usbMouse = HasUsbMouse(configuration);
        var keyboard = model.HasKeyboard || usbKeyboard ? new EmulationInputBindingSet(usbKeyboard ? UsbKeyboardDefinitions(configuration) : [], input.KeyboardBindings ?? EmulationSdk.InputProcessing.ToStrings(input.KeyboardMappings), EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller,
            CompatibleVisualIds: usbKeyboard ? [UsbKeyboardVisualId(configuration)] : null, DefaultVisualId: usbKeyboard ? UsbKeyboardVisualId(configuration) : null) : null;
        var mouse = model.MouseButtonCount > 0 || usbMouse ? new EmulationInputBindingSet(usbMouse ? [.. MouseDefinitions(), Definition(nameof(MouseAction.MiddleButton), "Emulation.Mouse.Button.Middle", "Mouse:Middle")] : MouseDefinitions(), EmulationSdk.InputProcessing.MouseBindings(input.MouseButtonMappings, MouseAction.None), EmulationInputSource.Mouse | EmulationInputSource.Keyboard | EmulationInputSource.Controller, true) : null;
        if (usbMouse && mouse is not null) mouse = mouse with
        {
            CompatibleVisualIds = [EmulationControllerVisualIds.SonyUsbMouse],
            DefaultVisualId = EmulationControllerVisualIds.SonyUsbMouse,
            VisualCommandIds = UsbMouseVisualCommands()
        };
        var indexes = Enumerable.Range(ControllerPortConstants.MinimumControllerPort, CommonControllerFunctions.PortCount(configuration));
        if (model.Id == ModelConstants.PlayStation2 && configuration.EmulatorId != "pcee2") indexes = new[] { 8, 9 }.Concat(indexes);
        var ports = indexes.Select(index => DescribePort(configuration, model, index)).ToArray();
        return new EmulationInputSettings(keyboard, mouse, ports);
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationInputSettings settings)
    {
        if (!EmulatorCatalog.GetAll(configuration.Model).Any(adapter => adapter.Id == configuration.EmulatorId))
            return configuration;
        if (settings.ControllerPorts.Any(port => port.DeadZonePercent is < 0 or > 100)) throw new ArgumentOutOfRangeException(nameof(settings), nameof(EmulationControllerPort.DeadZonePercent));
        var current = configuration.Input ?? new InputConfiguration();
        var keyboard = settings.Keyboard?.Values ?? current.KeyboardBindings ?? EmulationSdk.InputProcessing.ToStrings(current.KeyboardMappings);
        var mouse = EmulationSdk.InputProcessing.CreateMouseMappings(settings.Mouse, MouseAction.None) ?? current.MouseButtonMappings ?? new Dictionary<string, MouseAction>();
        var controllers = settings.ControllerPorts.Where(port => port.Number <= CommonControllerFunctions.PortCount(configuration)).Select(port =>
        {
            var index = port.Number - ControllerPortConstants.OneControllerPort;
            if (!Enum.TryParse<PeripheralCategory>(port.SelectedControllerId, true, out var type))
                throw new ArgumentOutOfRangeException(nameof(port.SelectedControllerId), port.SelectedControllerId, null);
            var usb = ControllerCatalog.IsUsbPort(configuration, index);
            if (!ControllerCatalog.Available(configuration, index, usb).Contains(type))
                throw new ArgumentOutOfRangeException(nameof(port.SelectedControllerId), port.SelectedControllerId, null);
            return new ControllerBinding(index, type, port.PhysicalDeviceId,
                EmulationSdk.InputProcessing.CopyNonEmptyBindings(port.Bindings.Values),
                port.VisualId, ValidOutputBindings(configuration, type, port.VisualId, port.OutputBindings), DeadZonePercent: port.DeadZonePercent);
        }).ToArray();
        if (configuration.Model == ModelConstants.PlayStation3)
            controllers = controllers.Concat((current.Controllers ?? [])
                .Where(binding => controllers.All(selected => selected.Port != binding.Port))).ToArray();
        return configuration with
        {
            Input = current with
            {
                KeyboardBindings = keyboard,
                KeyboardMappings = EmulationSdk.InputProcessing.ToKeys(keyboard),
                MouseButtonMappings = mouse,
                Controllers = controllers,
                UsbBindings = configuration.Model == ModelConstants.PlayStation2 && configuration.EmulatorId != "pcee2"
                    ? settings.ControllerPorts.Where(port => port.IsUsb).Select(port =>
                    {
                        if (!Enum.TryParse<PeripheralCategory>(port.SelectedControllerId, out var type)
                            || !ControllerCatalog.Available(configuration, port.Number - 1, true).Contains(type))
                            throw new ArgumentOutOfRangeException(nameof(port.SelectedControllerId));
                        return new ControllerBinding(port.Number - 9, type, port.PhysicalDeviceId,
                            EmulationSdk.InputProcessing.CopyNonEmptyBindings(port.Bindings.Values),
                            port.VisualId, ValidOutputBindings(configuration, type, port.VisualId, port.OutputBindings), DeadZonePercent: port.DeadZonePercent);
                    }).ToArray() : current.UsbBindings
            }
        };
    }

    private static IReadOnlyList<InputBindingDefinition> MouseDefinitions() =>
    [
        Definition(nameof(MouseAction.LeftButton), InputSettingsFunctionsConstants.ResourceMouseButtonLeft, InputSettingsFunctionsConstants.MouseLeft),
        Definition(nameof(MouseAction.RightButton), InputSettingsFunctionsConstants.ResourceMouseButtonRight, InputSettingsFunctionsConstants.MouseRight)
    ];

    private static IReadOnlyList<InputBindingDefinition> ControllerDefinitions(PeripheralCategory type, Model model) => type is PeripheralCategory.None ? []
        : type == PeripheralCategory.Mouse ? MouseDefinitions()
        : type == PeripheralCategory.Keyboard ? []
        : model.Id == ModelConstants.PocketStation ? Machines.PocketStation.Functions.ControllerFunctions.Definitions
        : model.Id == ModelConstants.Psp ? PspControllerDefinitions()
        : model.Id is ModelConstants.PlayStation or ModelConstants.PlayStation2
            or ModelConstants.PlayStation3
            ? PlayStationControllerDefinitions(type, model) :
        [
            Definition(InputSettingsFunctionsConstants.Up, InputSettingsFunctionsConstants.ResourceControllerActionUp, string.Empty),
            Definition(InputSettingsFunctionsConstants.Down, InputSettingsFunctionsConstants.ResourceControllerActionDown, string.Empty),
            Definition(InputSettingsFunctionsConstants.Left, InputSettingsFunctionsConstants.ResourceControllerActionLeft, string.Empty),
            Definition(InputSettingsFunctionsConstants.Right, InputSettingsFunctionsConstants.ResourceControllerActionRight, string.Empty),
            Definition(InputSettingsFunctionsConstants.B, InputSettingsFunctionsConstants.ResourceControllerActionFire1, string.Empty),
            Definition(InputSettingsFunctionsConstants.A, InputSettingsFunctionsConstants.ResourceControllerActionFire2, string.Empty)
        ];

    private static IReadOnlyList<InputBindingDefinition> ControllerDefinitions(PeripheralCategory type, Model model, MachineConfiguration configuration, int port) =>
        (new Engine().Adapter(configuration) as IEmulatorControllerAdapter)?.GetControllerBindingDefinitions(configuration, type, port) ?? ControllerDefinitions(type, model);

    private static IReadOnlyList<InputBindingDefinition> PspControllerDefinitions() =>
    [
        Definition(InputSettingsFunctionsConstants.Up, InputSettingsFunctionsConstants.ResourceControllerActionUp, string.Empty),
        Definition(InputSettingsFunctionsConstants.Down, InputSettingsFunctionsConstants.ResourceControllerActionDown, string.Empty),
        Definition(InputSettingsFunctionsConstants.Left, InputSettingsFunctionsConstants.ResourceControllerActionLeft, string.Empty),
        Definition(InputSettingsFunctionsConstants.Right, InputSettingsFunctionsConstants.ResourceControllerActionRight, string.Empty),
        Definition(EmulationControllerCommandIds.B, InputSettingsFunctionsConstants.ControllerCross, string.Empty),
        Definition(EmulationControllerCommandIds.A, InputSettingsFunctionsConstants.ControllerCircle, string.Empty),
        Definition(EmulationControllerCommandIds.Y, InputSettingsFunctionsConstants.ControllerSquare, string.Empty),
        Definition(EmulationControllerCommandIds.X, InputSettingsFunctionsConstants.ControllerTriangle, string.Empty),
        Definition(EmulationControllerCommandIds.Select, InputSettingsFunctionsConstants.ControllerSelect, string.Empty),
        Definition(EmulationControllerCommandIds.Start, InputSettingsFunctionsConstants.ControllerStart, string.Empty),
        Definition(EmulationControllerCommandIds.L, EmulationControllerCommandIds.L, string.Empty),
        Definition(EmulationControllerCommandIds.R, EmulationControllerCommandIds.R, string.Empty),
        Definition(InputSettingsFunctionsConstants.PspStickUp, InputSettingsFunctionsConstants.PspStickUpLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.PspStickDown, InputSettingsFunctionsConstants.PspStickDownLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.PspStickLeft, InputSettingsFunctionsConstants.PspStickLeftLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.PspStickRight, InputSettingsFunctionsConstants.PspStickRightLabel, string.Empty)
    ];

    private static IReadOnlyList<InputBindingDefinition> PlayStationControllerDefinitions(PeripheralCategory type, Model model)
    {
        var buttons = PlayStationButtonDefinitions();
        if (type == PeripheralCategory.DanceMat)
            return buttons.Where(definition => definition.Id is EmulationControllerCommandIds.Up or EmulationControllerCommandIds.Down
                or EmulationControllerCommandIds.Left or EmulationControllerCommandIds.Right
                or EmulationControllerCommandIds.Start or EmulationControllerCommandIds.Select
                or EmulationControllerCommandIds.B or EmulationControllerCommandIds.A
                or EmulationControllerCommandIds.Y or EmulationControllerCommandIds.X).ToArray();
        if (type == PeripheralCategory.ArcadeStick)
            return buttons.Where(definition => definition.Id is not (EmulationControllerCommandIds.L3 or EmulationControllerCommandIds.R3)).ToArray();
        if (type is PeripheralCategory.GunCon or PeripheralCategory.Justifier)
            return
            [
                Definition(EmulationControllerCommandIds.B, InputSettingsFunctionsConstants.ResourceControllerActionFire1, string.Empty),
                Definition(EmulationControllerCommandIds.Reload, InputSettingsFunctionsConstants.ResourceControllerActionReload, string.Empty),
                Definition(EmulationControllerCommandIds.A, EmulationControllerCommandIds.A, string.Empty),
                type == PeripheralCategory.Justifier ? Definition(EmulationControllerCommandIds.Start, InputSettingsFunctionsConstants.ControllerStart, string.Empty) : Definition(EmulationControllerCommandIds.X, EmulationControllerCommandIds.B, string.Empty)
            ];
        if (type is PeripheralCategory.NeGcon or PeripheralCategory.NeGconRumble)
            return buttons.Where(definition => definition.Id is
                EmulationControllerCommandIds.Up or EmulationControllerCommandIds.Down
                or EmulationControllerCommandIds.Left or EmulationControllerCommandIds.Right
                or EmulationControllerCommandIds.Start).Concat(
                new InputBindingDefinition[]
                {
                    Definition(EmulationControllerCommandIds.A, EmulationControllerCommandIds.A, string.Empty),
                    Definition(EmulationControllerCommandIds.X, EmulationControllerCommandIds.B, string.Empty),
                    Definition(EmulationControllerCommandIds.R, EmulationControllerCommandIds.R, string.Empty),
                    Definition(EmulationControllerCommandIds.R2, InputSettingsFunctionsConstants.NeGconButtonI, string.Empty),
                    Definition(EmulationControllerCommandIds.L2, InputSettingsFunctionsConstants.NeGconButtonII, string.Empty),
                    Definition(EmulationControllerCommandIds.L, EmulationControllerCommandIds.L, string.Empty),
                    Definition(InputSettingsFunctionsConstants.LeftStickLeft, InputSettingsFunctionsConstants.ResourceSteeringLeft, string.Empty),
                    Definition(InputSettingsFunctionsConstants.LeftStickRight, InputSettingsFunctionsConstants.ResourceSteeringRight, string.Empty)
                }).ToArray();
        if (model.Id == ModelConstants.PlayStation && type == PeripheralCategory.Joystick)
            return buttons.Where(definition => definition.Id is not (EmulationControllerCommandIds.L3 or EmulationControllerCommandIds.R3)).ToArray();
        if (type is PeripheralCategory.AnalogController or PeripheralCategory.AnalogJoystick)
            buttons = buttons.Where(definition => definition.Id is not (EmulationControllerCommandIds.L3 or EmulationControllerCommandIds.R3)).ToArray();
        var analogButtons = model.Id == ModelConstants.PlayStation && type is PeripheralCategory.DualShock or PeripheralCategory.AnalogController or PeripheralCategory.AnalogJoystick ? new[] { Definition(InputSettingsFunctionsConstants.Analog, InputSettingsFunctionsConstants.Analog, string.Empty) } : [];
        return buttons.Concat(analogButtons).Concat(AnalogStickDefinitions()).ToArray();
    }

    private static IReadOnlyList<InputBindingDefinition> AnalogStickDefinitions() =>
    [
        Definition(InputSettingsFunctionsConstants.LeftStickUp, InputSettingsFunctionsConstants.LeftStickUpLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.LeftStickDown, InputSettingsFunctionsConstants.LeftStickDownLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.LeftStickLeft, InputSettingsFunctionsConstants.LeftStickLeftLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.LeftStickRight, InputSettingsFunctionsConstants.LeftStickRightLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.RightStickUp, InputSettingsFunctionsConstants.RightStickUpLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.RightStickDown, InputSettingsFunctionsConstants.RightStickDownLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.RightStickLeft, InputSettingsFunctionsConstants.RightStickLeftLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.RightStickRight, InputSettingsFunctionsConstants.RightStickRightLabel, string.Empty)
    ];

    private static IReadOnlyList<InputBindingDefinition> PlayStationButtonDefinitions() =>
    [
        Definition(InputSettingsFunctionsConstants.Up, InputSettingsFunctionsConstants.ResourceControllerActionUp, string.Empty),
        Definition(InputSettingsFunctionsConstants.Down, InputSettingsFunctionsConstants.ResourceControllerActionDown, string.Empty),
        Definition(InputSettingsFunctionsConstants.Left, InputSettingsFunctionsConstants.ResourceControllerActionLeft, string.Empty),
        Definition(InputSettingsFunctionsConstants.Right, InputSettingsFunctionsConstants.ResourceControllerActionRight, string.Empty),
        Definition(EmulationControllerCommandIds.B, InputSettingsFunctionsConstants.ControllerCross, string.Empty),
        Definition(EmulationControllerCommandIds.A, InputSettingsFunctionsConstants.ControllerCircle, string.Empty),
        Definition(EmulationControllerCommandIds.Y, InputSettingsFunctionsConstants.ControllerSquare, string.Empty),
        Definition(EmulationControllerCommandIds.X, InputSettingsFunctionsConstants.ControllerTriangle, string.Empty),
        Definition(EmulationControllerCommandIds.Select, InputSettingsFunctionsConstants.ControllerSelect, string.Empty),
        Definition(EmulationControllerCommandIds.Start, InputSettingsFunctionsConstants.ControllerStart, string.Empty),
        Definition(EmulationControllerCommandIds.L, InputSettingsFunctionsConstants.ControllerL1, string.Empty),
        Definition(EmulationControllerCommandIds.R, InputSettingsFunctionsConstants.ControllerR1, string.Empty),
        Definition(EmulationControllerCommandIds.L2, InputSettingsFunctionsConstants.ControllerL2, string.Empty),
        Definition(EmulationControllerCommandIds.R2, InputSettingsFunctionsConstants.ControllerR2, string.Empty),
        Definition(EmulationControllerCommandIds.L3, InputSettingsFunctionsConstants.ControllerL3, string.Empty),
        Definition(EmulationControllerCommandIds.R3, InputSettingsFunctionsConstants.ControllerR3, string.Empty)
    ];

    private static InputBindingDefinition Definition(string id, string resourceKey, string defaultBinding, string? invariant = null) =>
        new(id, resourceKey, defaultBinding, EmulationSdk.InputProcessing.BindingDisplayValue(resourceKey, invariant))
        {
            OppositeDirectionId = id switch
            {
                InputSettingsFunctionsConstants.PspStickUp => InputSettingsFunctionsConstants.PspStickDown,
                InputSettingsFunctionsConstants.PspStickDown => InputSettingsFunctionsConstants.PspStickUp,
                InputSettingsFunctionsConstants.PspStickLeft => InputSettingsFunctionsConstants.PspStickRight,
                InputSettingsFunctionsConstants.PspStickRight => InputSettingsFunctionsConstants.PspStickLeft,
                InputSettingsFunctionsConstants.LeftStickUp => InputSettingsFunctionsConstants.LeftStickDown,
                InputSettingsFunctionsConstants.LeftStickDown => InputSettingsFunctionsConstants.LeftStickUp,
                InputSettingsFunctionsConstants.LeftStickLeft => InputSettingsFunctionsConstants.LeftStickRight,
                InputSettingsFunctionsConstants.LeftStickRight => InputSettingsFunctionsConstants.LeftStickLeft,
                InputSettingsFunctionsConstants.RightStickUp => InputSettingsFunctionsConstants.RightStickDown,
                InputSettingsFunctionsConstants.RightStickDown => InputSettingsFunctionsConstants.RightStickUp,
                InputSettingsFunctionsConstants.RightStickLeft => InputSettingsFunctionsConstants.RightStickRight,
                InputSettingsFunctionsConstants.RightStickRight => InputSettingsFunctionsConstants.RightStickLeft,
                _ => null
            }
        };

    private static EmulationControllerChoice Choice(PeripheralCategory type, Model model, MachineConfiguration configuration, int port) => new(
        type.ToString(), ControllerCatalog.IsUsbPort(configuration, port) && type == PeripheralCategory.Mouse ? "Emulation.Sony.Controller.UsbMouse"
            : ControllerCatalog.IsUsbPort(configuration, port) && type == PeripheralCategory.Keyboard ? "Emulation.Sony.Controller.UsbKeyboard"
            : ControllerResourceKey(type, model),
        InvariantDisplayValue: type switch
        {
            PeripheralCategory.NeGcon => InputSettingsFunctionsConstants.NeGcon,
            PeripheralCategory.NeGconRumble => InputSettingsFunctionsConstants.NeGconRumble,
            PeripheralCategory.KeyboardAndMouse => InputSettingsFunctionsConstants.KeyboardAndMouseGlyphs,
            _ => null
        },
        BindingDefinitions: PortDefinitions(type, model, configuration, port),
        CompatibleVisualIds: ChoiceVisuals(type, model, configuration, port),
        DefaultVisualId: ChoiceVisuals(type, model, configuration, port)?.FirstOrDefault(),
        VisualCommandIds: ControllerCatalog.IsUsbPort(configuration, port) && type is PeripheralCategory.Keyboard or PeripheralCategory.Mouse or PeripheralCategory.NetPlayController ? null : VisualCommandIds(type, model),
        VisualBindingDefinitions: PeripheralVisualDefinitions(type))
    {
        IsSupportedByEmulator = ControllerCatalog.IsSupported(configuration, type, port, ControllerCatalog.IsUsbPort(configuration, port)),
        VisualOutputDefinitions = VisualOutputs(configuration, type, model),
        Category = type switch
        {
            PeripheralCategory.None => EmulationControllerCategory.None,
            PeripheralCategory.Automatic => EmulationControllerCategory.Unspecified,
            PeripheralCategory.AnalogJoystick => EmulationControllerCategory.Joystick,
            PeripheralCategory.DanceMat => EmulationControllerCategory.DanceSimulation,
            PeripheralCategory.ArcadeStick => EmulationControllerCategory.Joystick,
            PeripheralCategory.NeGcon or PeripheralCategory.NeGconRumble => EmulationControllerCategory.RacingSimulation,
            PeripheralCategory.GunCon or PeripheralCategory.GunCon2 or PeripheralCategory.Justifier or PeripheralCategory.Mouse => EmulationControllerCategory.PointingDevice,
            PeripheralCategory.Keyboard or PeripheralCategory.KeyboardAndMouse => EmulationControllerCategory.OtherSimulation,
            _ => ControllerCatalog.Visuals(type, model.Id == ModelConstants.PlayStation2) is not null
                ? EmulationControllerCategory.Unspecified : EmulationControllerCategory.Gamepad
        }
    };

    private static IReadOnlyList<string>? ChoiceVisuals(PeripheralCategory type, Model model, MachineConfiguration configuration, int port) =>
        ControllerCatalog.IsUsbPort(configuration, port) && type == PeripheralCategory.Keyboard ? [EmulationControllerVisualIds.SonyUsbKeyboard]
        : ControllerCatalog.IsUsbPort(configuration, port) && type == PeripheralCategory.Mouse ? [EmulationControllerVisualIds.SonyUsbMouse]
        : ControllerCatalog.Visuals(type, model.Id == ModelConstants.PlayStation2) ?? CompatibleVisualIds(type, model);
}
