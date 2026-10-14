namespace GWGUI.Emulation.Coleco.Common.Machines.Common.Functions;

internal static class InputSettingsFunctions
{
    internal static IReadOnlyList<PeripheralCategory> Peripherals(Emulator core) => new Engine().Adapter(core) is IEmulatorInputMapper mapper ? mapper.GetPeripherals() : [];

    internal static IReadOnlyDictionary<string, EmulationKey> KeyboardDefaults { get; } = new Dictionary<string, EmulationKey>
    {
        ["Smart1"] = EmulationKey.F1, ["Smart2"] = EmulationKey.F2, ["Smart3"] = EmulationKey.F3,
        ["Smart4"] = EmulationKey.F4, ["Smart5"] = EmulationKey.F5, ["Smart6"] = EmulationKey.F6,
        ["Undo"] = EmulationKey.F7, ["WildCard"] = EmulationKey.F8, ["Move"] = EmulationKey.PageUp,
        ["Store"] = EmulationKey.PageDown, ["Clear"] = EmulationKey.End, ["Print"] = EmulationKey.F10
    };

    internal static IReadOnlyDictionary<string, string> KeyboardBindings(InputConfiguration input, MachineModel model) =>
        model != MachineModel.Adam ? new Dictionary<string, string>() : KeyboardDefaults.ToDictionary(pair => pair.Key, pair => input.Keyboard?.GetValueOrDefault(pair.Key, pair.Value.ToString()) ?? pair.Value.ToString());

    internal static IReadOnlyList<InputBindingDefinition> Definitions(PeripheralCategory peripheral)
    {
        if (peripheral == PeripheralCategory.None) return [];
        var actions = ControllerActionConstants.All.Where(action => (action is not ("Purple" or "Blue") || peripheral == PeripheralCategory.SuperAction) && (action is not ("SpinnerLeft" or "SpinnerRight") || peripheral is not PeripheralCategory.Standard));
        return actions.Select(action => new InputBindingDefinition(action,
            action is "Up" or "Down" or "Left" or "Right" ? "Emulation.Controller.Action." + action : "Emulation.Coleco.Action." + action,
            string.Empty, action.StartsWith("Keypad", StringComparison.Ordinal) ? action[6..] : action == "Star" ? "*" : action == "Pound" ? "#" : null)
        {
            OppositeDirectionId = action switch
            {
                ControllerActionConstants.Up => ControllerActionConstants.Down,
                ControllerActionConstants.Down => ControllerActionConstants.Up,
                ControllerActionConstants.Left => ControllerActionConstants.Right,
                ControllerActionConstants.Right => ControllerActionConstants.Left,
                ControllerActionConstants.SpinnerLeft => ControllerActionConstants.SpinnerRight,
                ControllerActionConstants.SpinnerRight => ControllerActionConstants.SpinnerLeft,
                _ => null
            }
        }).ToArray();
    }

    private static readonly IReadOnlyDictionary<EmulationControllerVisualControl,string> VisualCommands = new Dictionary<EmulationControllerVisualControl,string>
    {
        [EmulationControllerVisualControl.DirectionUp] = "Up",
        [EmulationControllerVisualControl.DirectionDown] = "Down",
        [EmulationControllerVisualControl.DirectionLeft] = "Left",
        [EmulationControllerVisualControl.DirectionRight] = "Right",
        [EmulationControllerVisualControl.PrimaryAction] = "FireLeft",
        [EmulationControllerVisualControl.SecondaryAction] = "FireRight",
        [EmulationControllerVisualControl.Key1] = "Keypad1",
        [EmulationControllerVisualControl.Key2] = "Keypad2",
        [EmulationControllerVisualControl.Key3] = "Keypad3",
        [EmulationControllerVisualControl.Key4] = "Keypad4",
        [EmulationControllerVisualControl.Key5] = "Keypad5",
        [EmulationControllerVisualControl.Key6] = "Keypad6",
        [EmulationControllerVisualControl.Key7] = "Keypad7",
        [EmulationControllerVisualControl.Key8] = "Keypad8",
        [EmulationControllerVisualControl.Key9] = "Keypad9",
        [EmulationControllerVisualControl.Key0] = "Keypad0",
        [EmulationControllerVisualControl.KeyStar] = "Star",
        [EmulationControllerVisualControl.KeyHash] = "Pound"
    };

    private static string? VisualId(PeripheralCategory peripheral) => peripheral switch
    {
        PeripheralCategory.Standard => EmulationControllerVisualIds.ColecoController,
        PeripheralCategory.Driving => EmulationControllerVisualIds.ColecoDrivingController,
        PeripheralCategory.SuperAction => EmulationControllerVisualIds.ColecoSuperActionController,
        _ => null
    };

    private static readonly IReadOnlyDictionary<EmulationControllerVisualControl, string> DrivingVisualCommands = new Dictionary<EmulationControllerVisualControl, string>
    {
        [EmulationControllerVisualControl.DirectionLeft] = ControllerActionConstants.SpinnerLeft,
        [EmulationControllerVisualControl.DirectionRight] = ControllerActionConstants.SpinnerRight,
        [EmulationControllerVisualControl.PrimaryAction] = ControllerActionConstants.FireLeft
    };

    private static readonly IReadOnlyDictionary<EmulationControllerVisualControl, string> SuperActionVisualCommands =
        VisualCommands.Where(pair => pair.Key is not (EmulationControllerVisualControl.PrimaryAction or EmulationControllerVisualControl.SecondaryAction))
            .Concat(new Dictionary<EmulationControllerVisualControl, string>
            {
                [EmulationControllerVisualControl.StickLeft] = ControllerActionConstants.SpinnerLeft,
                [EmulationControllerVisualControl.StickRight] = ControllerActionConstants.SpinnerRight
            }).ToDictionary(pair => pair.Key, pair => pair.Value);

    internal static EmulationInputSettings Describe(MachineConfiguration configuration)
    {
        var keyboard = configuration.Model == MachineModel.Adam ? new EmulationInputBindingSet(
            KeyboardDefaults.Select(pair => new InputBindingDefinition(pair.Key, "Emulation.Coleco.Key." + pair.Key, pair.Value.ToString(), pair.Key.StartsWith("Smart", StringComparison.Ordinal) ? "Smart " + pair.Key[5..] : null)).ToArray(),
            configuration.Input.Keyboard ?? new Dictionary<string, string>(), EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller) : null;
        var mapper = new Engine().Adapter(configuration.Core) as IEmulatorInputMapper;
        var ports = Enumerable.Range(0, ControllerConstants.PortCount).Select(index =>
        {
            var choices = (mapper?.GetPeripherals() ?? []).Select(category => new EmulationControllerChoice(category.ToString(),
                category == PeripheralCategory.None ? "Emulation.Controller.None" : "Emulation.Coleco.Controller." + category,
                BindingDefinitions: mapper?.DescribeInput(category, index) ?? [],
                CompatibleVisualIds: VisualId(category) is { } visualId ? [visualId] : [],
                DefaultVisualId: VisualId(category),
                VisualCommandIds: category == PeripheralCategory.Standard ? VisualCommands : category == PeripheralCategory.Driving
                    ? DrivingVisualCommands.Where(pair => (mapper?.DescribeInput(category, index) ?? []).Any(definition => definition.Id == pair.Value))
                        .ToDictionary(pair => pair.Key, pair => pair.Value) : category == PeripheralCategory.SuperAction
                    ? SuperActionVisualCommands.Where(pair => (mapper?.DescribeInput(category, index) ?? []).Any(definition => definition.Id == pair.Value))
                        .ToDictionary(pair => pair.Key, pair => pair.Value) : null,
                VisualBindingDefinitions: category == PeripheralCategory.Driving ? new Dictionary<string, IReadOnlyList<InputBindingDefinition>>
                {
                    [EmulationControllerVisualIds.ColecoDrivingController] = (mapper?.DescribeInput(category, index) ?? [])
                        .Where(definition => DrivingVisualCommands.Values.Contains(definition.Id, StringComparer.Ordinal))
                        .Select(definition => definition.Id == ControllerActionConstants.FireLeft ? definition with { DisplayResourceKey = "Controllers.Throttle" } : definition).ToArray()
                } : category == PeripheralCategory.SuperAction ? new Dictionary<string, IReadOnlyList<InputBindingDefinition>>
                {
                    [EmulationControllerVisualIds.ColecoSuperActionController] = mapper?.DescribeInput(category, index) ?? []
                } : null)
            {
                Category = category switch
                {
                    PeripheralCategory.None => EmulationControllerCategory.None,
                    PeripheralCategory.Standard => EmulationControllerCategory.Gamepad,
                    PeripheralCategory.SuperAction or PeripheralCategory.Paddle => EmulationControllerCategory.Joystick,
                    PeripheralCategory.Roller => EmulationControllerCategory.PointingDevice,
                    PeripheralCategory.Driving => EmulationControllerCategory.RacingSimulation,
                    _ => EmulationControllerCategory.Unspecified
                }
            }).ToArray();
            var binding = configuration.Input.Controllers?.FirstOrDefault(item => item.Port == index);
            var peripheral = binding?.Peripheral ?? PeripheralCategory.Standard;
            return new EmulationControllerPort(index + 1, choices, peripheral.ToString(), binding?.DeviceId,
                new(mapper?.DescribeInput(peripheral, index) ?? [], binding?.Mappings ?? new Dictionary<string, string>(), EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller, true),
                binding?.DeadZonePercent ?? ControllerConstants.DefaultDeadZonePercent, binding?.VisualId);
        }).ToArray();
        return new(keyboard, null, ports);
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationInputSettings settings)
    {
        var controllers = settings.ControllerPorts.Select(port =>
        {
            if (!Enum.TryParse<PeripheralCategory>(port.SelectedControllerId, out var peripheral)
                || !Peripherals(configuration.Core).Contains(peripheral)) throw new ArgumentException(nameof(settings));
            return new ControllerBinding(port.Number - 1, peripheral, port.PhysicalDeviceId, port.Bindings.Values, port.DeadZonePercent, port.VisualId);
        }).ToArray();
        var result = configuration with { Input = new(settings.Keyboard?.Values ?? configuration.Input.Keyboard, controllers) };
        ConfigurationFunctions.Validate(result);
        return result;
    }
}
