using GWGUI.Emulation;
namespace GWGUI.Emulation.Epoch.Common.Machines.Common.Functions;

internal static class InputSettingsFunctions
{
    internal static IReadOnlyList<PeripheralCategory> Peripherals(Emulator core)
    {
        if (!Enum.IsDefined(core)) throw new ArgumentOutOfRangeException(nameof(core));
        return [PeripheralCategory.Standard];
    }

    internal static IReadOnlyDictionary<string, string> KeyboardBindings(InputConfiguration input, MachineModel model)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        var core = ConfigurationFunctions.DefaultEmulator(model);
        var controller = input.Controllers?.FirstOrDefault(item => item.Port == ControllerConstants.FirstPort);
        foreach (var action in InputSnapshotDictionary.Actions(model, core, ControllerConstants.FirstPort))
        {
            if (action.Key is not (EmulationKey.EpochPower or EmulationKey.EpochReset)) continue;
            var mapping = controller?.Mappings?.GetValueOrDefault(action.Id, action.DefaultBinding) ?? action.DefaultBinding;
            if (input.Keyboard?.TryGetValue(action.Key.Value.ToString(), out var keyboardSource) == true)
                mapping = string.IsNullOrEmpty(keyboardSource) ? string.Empty : InputMappingConstants.KeyboardPrefix + keyboardSource;
            if (mapping.StartsWith(InputMappingConstants.KeyboardPrefix, StringComparison.OrdinalIgnoreCase))
                result[action.Key.Value.ToString()] = mapping[InputMappingConstants.KeyboardPrefix.Length..];
        }
        return result;
    }

    internal static EmulationInputSettings Describe(MachineConfiguration configuration)
    {
        var ports = Enumerable.Range(ControllerConstants.FirstPort, ControllerConstants.PortCount).Select(port =>
        {
            var definitions = InputSnapshotDictionary.Actions(configuration.Model, configuration.Core, port).Select(action => action.Definition).ToArray();
            var binding = configuration.Input.Controllers?.FirstOrDefault(item => item.Port == port);
            var choice = new EmulationControllerChoice(PeripheralCategory.Standard.ToString(),
                "Emulation.Epoch.Controller." + configuration.Model, BindingDefinitions: definitions,
                CompatibleVisualIds: Visuals(configuration.Model, port),
                DefaultVisualId: Visuals(configuration.Model, port).FirstOrDefault(),
                VisualCommandIds: VisualCommands(configuration.Model, port, definitions)) { Category = EmulationControllerCategory.Gamepad };
            return new EmulationControllerPort(port + 1, [choice], PeripheralCategory.Standard.ToString(), binding?.DeviceId,
                new(definitions, binding?.Mappings ?? new Dictionary<string, string>(), EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller, true),
                binding?.DeadZonePercent ?? ControllerConstants.DefaultDeadZonePercent, binding?.VisualId);
        }).ToArray();
        return new(null, null, ports);
    }

    private static IReadOnlyList<string> Visuals(MachineModel model, int port) => model is MachineModel.SuperCassetteVision or MachineModel.SuperCassetteVisionYeno or MachineModel.SuperCassetteVisionLady ? [EmulationControllerVisualIds.EpochSuperCassetteVisionController] : [];
    private static IReadOnlyDictionary<EmulationControllerVisualControl, string> VisualCommands(MachineModel model, int port, IReadOnlyList<InputBindingDefinition> definitions)
    {
        var commands = new Dictionary<EmulationControllerVisualControl, string>();
        if (model is MachineModel.SuperCassetteVision or MachineModel.SuperCassetteVisionYeno or MachineModel.SuperCassetteVisionLady)
        {
            commands[EmulationControllerVisualControl.DirectionUp] = "Up";
            commands[EmulationControllerVisualControl.DirectionDown] = "Down";
            commands[EmulationControllerVisualControl.DirectionLeft] = "Left";
            commands[EmulationControllerVisualControl.DirectionRight] = "Right";
            commands[EmulationControllerVisualControl.PrimaryAction] = "FireLeft";
            commands[EmulationControllerVisualControl.SecondaryAction] = "FireRight";
            commands[EmulationControllerVisualControl.Guide] = "Power";
            commands[EmulationControllerVisualControl.Reset] = "Reset";
            commands[EmulationControllerVisualControl.Option] = "Clear";
            commands[EmulationControllerVisualControl.Start] = "Enter";
            commands[EmulationControllerVisualControl.Key0] = "Keypad0";
            commands[EmulationControllerVisualControl.Key1] = "Keypad1";
            commands[EmulationControllerVisualControl.Key2] = "Keypad2";
            commands[EmulationControllerVisualControl.Key3] = "Keypad3";
            commands[EmulationControllerVisualControl.Key4] = "Keypad4";
            commands[EmulationControllerVisualControl.Key5] = "Keypad5";
            commands[EmulationControllerVisualControl.Key6] = "Keypad6";
            commands[EmulationControllerVisualControl.Key7] = "Keypad7";
            commands[EmulationControllerVisualControl.Key8] = "Keypad8";
            commands[EmulationControllerVisualControl.Key9] = "Keypad9";
        }
        else
        {
            commands[EmulationControllerVisualControl.DirectionLeft] = "LeverLeft";
            commands[EmulationControllerVisualControl.DirectionRight] = "LeverRight";
            commands[EmulationControllerVisualControl.PrimaryAction] = "Push1";
            commands[EmulationControllerVisualControl.SecondaryAction] = "Push2";
            commands[EmulationControllerVisualControl.Start] = "Start";
            if (model == MachineModel.CassetteVisionJr)
            {
                commands[EmulationControllerVisualControl.TertiaryAction] = "Push3";
                commands[EmulationControllerVisualControl.QuaternaryAction] = "Push4";
                commands[EmulationControllerVisualControl.Option] = "Select";
            }
            else
            {
                commands[EmulationControllerVisualControl.Option] = "CourseDecrease";
                commands[EmulationControllerVisualControl.TertiaryAction] = "CourseIncrease";
                commands[EmulationControllerVisualControl.QuaternaryAction] = "Select";
                var firstPaddle = port == ControllerConstants.FirstPort ? "Paddle1" : "Paddle4";
                var secondPaddle = port == ControllerConstants.FirstPort ? "Paddle2" : "Paddle3";
                commands[EmulationControllerVisualControl.LeftStick] = firstPaddle + "Decrease";
                commands[EmulationControllerVisualControl.RightStick] = firstPaddle + "Increase";
                commands[EmulationControllerVisualControl.RightStickLeft] = secondPaddle + "Decrease";
                commands[EmulationControllerVisualControl.RightStickRight] = secondPaddle + "Increase";
            }
        }
        var available = definitions.Select(definition => definition.Id).ToHashSet(StringComparer.Ordinal);
        return commands.Where(pair => available.Contains(pair.Value)).ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationInputSettings settings)
    {
        if (settings.ControllerPorts.Count != ControllerConstants.PortCount)
            throw new ArgumentException(nameof(settings));
        var controllers = settings.ControllerPorts.Select(port =>
        {
            if (port.SelectedControllerId != PeripheralCategory.Standard.ToString())
                throw new ArgumentException(nameof(settings));
            return new ControllerBinding(port.Number - 1, PeripheralCategory.Standard, port.PhysicalDeviceId, new Dictionary<string, string>(port.Bindings.Values, StringComparer.Ordinal), port.DeadZonePercent, port.VisualId);
        }).ToArray();
        var result = configuration with { Input = new(configuration.Input.Keyboard, controllers) };
        ConfigurationFunctions.Validate(result);
        return result;
    }

    internal static void Validate(MachineConfiguration configuration)
    {
        if (configuration.Input.Keyboard is not null)
        {
            var allowed = KeyboardBindings(new InputConfiguration(), configuration.Model).Keys.ToHashSet(StringComparer.Ordinal);
            if (configuration.Input.Keyboard.Any(pair => !allowed.Contains(pair.Key)))
                throw new ArgumentException(nameof(configuration.Input));
        }
        foreach (var controller in configuration.Input.Controllers ?? [])
        {
            var actions = InputSnapshotDictionary.Actions(configuration.Model, configuration.Core, controller.Port).Select(action => action.Id).ToHashSet(StringComparer.Ordinal);
            if (EmulationSdk.InputProcessing.HasInvalidBindings(controller.Mappings, actions, EmulationKeyboardBindingFunctions.IsValidSourcePrefix))
                throw new ArgumentException(nameof(configuration.Input));
        }
    }
}

