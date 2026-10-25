using CommonControllerFunctions = GWGUI.Emulation.Watara.Common.Functions.ControllerFunctions;
using GWGUI.Emulation;
using GWGUI.Emulation.Watara.Common.Machines.Common.Dictionaries;

namespace GWGUI.Emulation.Watara.Common.Machines.Common.Functions;
internal static class InputSettingsFunctions
{
    internal static IReadOnlyDictionary<string, string> KeyboardBindings(InputConfiguration input, MachineModel model) => new Dictionary<string, string>();
    internal static bool SupportsPeripheral(MachineConfiguration configuration, int port, PeripheralCategory peripheral) => Enum.IsDefined(configuration.Model) && port == ControllerConstants.FirstPort && peripheral == PeripheralCategory.Supervision;
    internal static EmulationInputSettings Describe(MachineConfiguration configuration)
    {
        var binding = configuration.Input.Controllers?.SingleOrDefault();
        var choice = new EmulationControllerChoice(
            PeripheralCategory.Supervision.ToString(),
            string.Empty,
            "Supervision",
            ControllerCatalog.Definitions,
            [EmulationControllerVisualIds.WataraSupervisionControls],
            EmulationControllerVisualIds.WataraSupervisionControls,
            ControllerCatalog.VisualCommands
        ) { Category = EmulationControllerCategory.Gamepad };
        var bindings = new EmulationInputBindingSet(
            ControllerCatalog.Definitions,
            binding?.Mappings ?? new Dictionary<string, string>(),
            EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller,
            true,
            [EmulationControllerVisualIds.WataraSupervisionControls],
            EmulationControllerVisualIds.WataraSupervisionControls,
            ControllerCatalog.VisualCommands
        );
        return new(null, null, [new(1, [choice], choice.Id, binding?.DeviceId, bindings, binding?.DeadZonePercent ?? ControllerConstants.DefaultDeadZonePercent, binding?.VisualId ?? EmulationControllerVisualIds.WataraSupervisionControls) { OutputBindings = binding?.OutputBindings ?? new Dictionary<string, EmulationControllerOutputBinding>() }]);
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationInputSettings settings)
    {
        if (settings.Keyboard is not null || settings.Mouse is not null || settings.ControllerPorts.Count != CommonControllerFunctions.PortCount(configuration))
            throw new ArgumentException(nameof(settings));
        var port = settings.ControllerPorts[ControllerConstants.FirstPort];
        if (port.Number != 1 || port.SelectedControllerId != PeripheralCategory.Supervision.ToString() || port.VisualId is not (null or EmulationControllerVisualIds.WataraSupervisionControls))
            throw new ArgumentException(nameof(settings));
        var binding = new ControllerBinding(ControllerConstants.FirstPort, PeripheralCategory.Supervision, port.PhysicalDeviceId, new Dictionary<string, string>(port.Bindings.Values, StringComparer.Ordinal), port.DeadZonePercent, port.VisualId, OutputBindings: port.OutputBindings);
        var result = configuration with
        {
            Input = new InputConfiguration([binding])
        };
        ConfigurationFunctions.Validate(result);
        return new Engine().SelectedAdapter(result)?.ApplyInputConfiguration(result) ?? result;
    }

    internal static void Validate(MachineConfiguration configuration)
    {
        var commands = ControllerCatalog.Definitions.Select(definition => definition.Id).ToHashSet(StringComparer.Ordinal);
        foreach (var binding in configuration.Input.Controllers ?? [])
            if (EmulationSdk.InputProcessing.HasInvalidBindings(binding.Mappings, commands, CommonControllerFunctions.IsValidBinding) || binding.VisualId is not (null or EmulationControllerVisualIds.WataraSupervisionControls))
                throw new ArgumentException(nameof(configuration.Input));
    }
}
