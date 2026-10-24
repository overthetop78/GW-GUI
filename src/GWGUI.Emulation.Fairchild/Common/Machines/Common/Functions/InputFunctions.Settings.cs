using CommonControllerFunctions = GWGUI.Emulation.Fairchild.Common.Functions.ControllerFunctions;
using GWGUI.Emulation;
namespace GWGUI.Emulation.Fairchild.Common.Machines.Common.Functions;

internal static class InputSettingsFunctions
{
    internal static IReadOnlyList<PeripheralCategory> Peripherals(Emulator core)
    {
        if (core != Emulator.FreeChaF) throw new ArgumentOutOfRangeException(nameof(core));
        return [PeripheralCategory.Standard];
    }

    internal static IReadOnlyDictionary<string, string> KeyboardBindings(InputConfiguration input, MachineModel model) => new Dictionary<string, string>(StringComparer.Ordinal);

    internal static EmulationInputSettings Describe(MachineConfiguration configuration)
    {
        var ports = Enumerable.Range(ControllerConstants.FirstPort, CommonControllerFunctions.PortCount(configuration)).Select(port =>
        {
            var definitions = InputSnapshotDictionary.Actions(configuration, port).Select(action => action.Definition).ToArray();
            var binding = configuration.Input.Controllers?.FirstOrDefault(item => item.Port == port);
            var choice = new EmulationControllerChoice(PeripheralCategory.Standard.ToString(),
                "Emulation.Fairchild.Controller.ChannelF", BindingDefinitions: definitions,
                CompatibleVisualIds: [EmulationControllerVisualIds.FairchildChannelFController],
                DefaultVisualId: EmulationControllerVisualIds.FairchildChannelFController,
                VisualCommandIds: new Dictionary<EmulationControllerVisualControl, string>
                {
                    [EmulationControllerVisualControl.DirectionUp] = "Up",
                    [EmulationControllerVisualControl.DirectionDown] = "Down",
                    [EmulationControllerVisualControl.DirectionLeft] = "Left",
                    [EmulationControllerVisualControl.DirectionRight] = "Right",
                    [EmulationControllerVisualControl.PrimaryAction] = "Push",
                    [EmulationControllerVisualControl.SecondaryAction] = "Pull",
                    [EmulationControllerVisualControl.TertiaryAction] = "RotateLeft",
                    [EmulationControllerVisualControl.QuaternaryAction] = "RotateRight"
                }) { Category = EmulationControllerCategory.Joystick };
            return new EmulationControllerPort(port + 1, [choice], PeripheralCategory.Standard.ToString(), binding?.DeviceId,
                new(definitions, binding?.Mappings ?? new Dictionary<string, string>(), EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller, true),
                binding?.DeadZonePercent ?? ControllerConstants.DefaultDeadZonePercent, binding?.VisualId) { OutputBindings = binding?.OutputBindings ?? new Dictionary<string, EmulationControllerOutputBinding>() };
        }).ToArray();
        return new(null, null, ports);
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationInputSettings settings)
    {
        if (EmulationSdk.InputProcessing.HasInvalidControllerPortNumbers(settings, CommonControllerFunctions.PortCount(configuration)))
            throw new ArgumentException(nameof(settings));
        var controllers = settings.ControllerPorts.Select(port =>
        {
            if (port.SelectedControllerId != PeripheralCategory.Standard.ToString())
                throw new ArgumentException(nameof(settings));
            return new ControllerBinding(port.Number - 1, PeripheralCategory.Standard, port.PhysicalDeviceId, new Dictionary<string, string>(port.Bindings.Values, StringComparer.Ordinal), port.DeadZonePercent, port.VisualId, OutputBindings: port.OutputBindings);
        }).ToArray();
        var result = configuration with { Input = new(configuration.Input.KeyboardBindings, controllers) };
        ConfigurationFunctions.Validate(result);
        return result;
    }

    internal static void Validate(MachineConfiguration configuration)
    {
        if (configuration.Input.KeyboardBindings?.Count > 0) throw new ArgumentException(nameof(configuration.Input));
        foreach (var controller in configuration.Input.Controllers ?? [])
        {
            var actions = InputSnapshotDictionary.Actions(configuration, controller.Port).Select(action => action.Id).ToHashSet(StringComparer.Ordinal);
            if (EmulationSdk.InputProcessing.HasInvalidBindings(controller.Mappings, actions, EmulationSdk.InputProcessing.IsValidSourcePrefix))
                throw new ArgumentException(nameof(configuration.Input));
        }
    }
}
