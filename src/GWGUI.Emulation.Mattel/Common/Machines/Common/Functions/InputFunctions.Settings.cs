using CommonControllerFunctions = GWGUI.Emulation.Mattel.Common.Functions.ControllerFunctions;
using GWGUI.Emulation;
namespace GWGUI.Emulation.Mattel.Common.Machines.Common.Functions;

internal static class InputSettingsFunctions
{
    internal static IReadOnlyList<PeripheralCategory> Peripherals(MachineConfiguration configuration) => new Engine().Adapter(configuration.Core) is IEmulatorControllerAdapter mapper ? mapper.GetPeripherals(configuration) : [];
    internal static PeripheralCategory DefaultPeripheral(MachineModel model) => PeripheralCategory.Intellivision;
    internal static IReadOnlyDictionary<string, string> KeyboardBindings(InputConfiguration input, MachineModel model) => new Dictionary<string, string>();

    internal static EmulationInputSettings Describe(MachineConfiguration configuration)
    {
        EmulationInputBindingSet? keyboard = null;
        var ports = Enumerable.Range(ControllerConstants.FirstPort, CommonControllerFunctions.PortCount(configuration))
            .Select(port =>
            {
                var binding = configuration.Input.Controllers?.FirstOrDefault(item => item.Port == port);
                var peripheral = binding?.Peripheral ?? DefaultPeripheral(configuration.Model);
                var choices = Peripherals(configuration).Select(category => new EmulationControllerChoice(category.ToString(),
                    "Emulation.Mattel.Controller." + category,
                    BindingDefinitions: InputSnapshotDictionary.Actions(configuration, port, category).Select(action => action.Definition).ToArray(),
                    CompatibleVisualIds: ControllerVisualDictionary.VisualId(category) is { } visualId ? [visualId] : [],
                    DefaultVisualId: ControllerVisualDictionary.VisualId(category),
                    VisualCommandIds: ControllerVisualDictionary.Commands(category)) { Category = EmulationControllerCategory.Gamepad }).ToArray();
                return new EmulationControllerPort(port + 1, choices, peripheral.ToString(), binding?.DeviceId,
                    new(InputSnapshotDictionary.Actions(configuration, port, peripheral).Select(action => action.Definition).ToArray(),
                        binding?.Mappings ?? new Dictionary<string, string>(),
                        EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller, true),
                    binding?.DeadZonePercent ?? ControllerConstants.DefaultDeadZonePercent, binding?.VisualId) { OutputBindings = binding?.OutputBindings ?? new Dictionary<string, EmulationControllerOutputBinding>() };
            }).ToArray();
        return new(keyboard, null, ports);
    }
    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationInputSettings settings)
    {
        var count = CommonControllerFunctions.PortCount(configuration);
        if (EmulationSdk.InputProcessing.HasInvalidControllerPortNumbers(settings, count)
            || settings.Mouse is not null || settings.Keyboard?.Values.Count > 0)
            throw new ArgumentException(nameof(settings));
        var controllers = EmulationSdk.InputProcessing.CreateControllerBindings<PeripheralCategory, ControllerBinding>(
            settings,
            (index, peripheral) => Peripherals(configuration).Contains(peripheral),
            static (port, peripheral, mappings) => new(port.Number - 1, peripheral, port.PhysicalDeviceId, mappings, port.DeadZonePercent, port.VisualId, OutputBindings: port.OutputBindings));
        var result = configuration with { Input = new(settings.Keyboard?.Values, controllers) };
        ConfigurationFunctions.Validate(result);
        return new Engine().SelectedAdapter(result)?.ApplyInputConfiguration(result) ?? result;
    }
    internal static void Validate(MachineConfiguration configuration)
    {
        if (configuration.Input.KeyboardBindings?.Count > 0) throw new ArgumentException(nameof(configuration.Input));
        foreach (var binding in configuration.Input.Controllers ?? [])
        {
            if (!Peripherals(configuration).Contains(binding.Peripheral)) throw new ArgumentException(nameof(configuration.Input));
            var ids = InputSnapshotDictionary.Actions(configuration, binding.Port, binding.Peripheral).Select(action => action.Id).ToHashSet(StringComparer.Ordinal);
            if (EmulationSdk.InputProcessing.HasInvalidBindings(binding.Mappings, ids, EmulationSdk.InputProcessing.IsValidSourcePrefix)) throw new ArgumentException(nameof(configuration.Input));
        }
    }
}
