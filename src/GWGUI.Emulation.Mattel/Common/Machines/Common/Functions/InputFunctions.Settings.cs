namespace GWGUI.Emulation.Mattel.Common.Machines.Common.Functions;

internal static class InputSettingsFunctions
{
    internal static IReadOnlyList<PeripheralCategory> Peripherals(MachineModel model, Emulator core) => new Engine().Adapter(core) is IEmulatorInputMapper mapper ? mapper.GetPeripherals(model) : [];
    internal static PeripheralCategory DefaultPeripheral(MachineModel model) => PeripheralCategory.Intellivision;
    internal static IReadOnlyDictionary<string, string> KeyboardBindings(InputConfiguration input, MachineModel model) => new Dictionary<string, string>();

    internal static EmulationInputSettings Describe(MachineConfiguration configuration)
    {
        EmulationInputBindingSet? keyboard = null;
        var ports = Enumerable.Range(ControllerConstants.FirstPort, ConfigurationFunctions.ControllerPortCount(configuration.Model))
            .Select(port =>
            {
                var binding = configuration.Input.Controllers?.FirstOrDefault(item => item.Port == port);
                var peripheral = binding?.Peripheral ?? DefaultPeripheral(configuration.Model);
                var choices = Peripherals(configuration.Model, configuration.Core).Select(category => new EmulationControllerChoice(category.ToString(),
                    "Emulation.Mattel.Controller." + category,
                    BindingDefinitions: InputSnapshotDictionary.Actions(configuration.Model, configuration.Core, category).Select(action => action.Definition).ToArray(),
                    CompatibleVisualIds: ControllerVisualDictionary.VisualId(category) is { } visualId ? [visualId] : [],
                    DefaultVisualId: ControllerVisualDictionary.VisualId(category),
                    VisualCommandIds: ControllerVisualDictionary.Commands(category)) { Category = EmulationControllerCategory.Gamepad }).ToArray();
                return new EmulationControllerPort(port + 1, choices, peripheral.ToString(), binding?.DeviceId,
                    new(InputSnapshotDictionary.Actions(configuration.Model, configuration.Core, peripheral).Select(action => action.Definition).ToArray(),
                        binding?.Mappings ?? new Dictionary<string, string>(),
                        EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller, true),
                    binding?.DeadZonePercent ?? ControllerConstants.DefaultDeadZonePercent, binding?.VisualId);
            }).ToArray();
        return new(keyboard, null, ports);
    }
    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationInputSettings settings)
    {
        var count = ConfigurationFunctions.ControllerPortCount(configuration.Model);
        if (EmulationInputMappingFunctions.HasInvalidControllerPortNumbers(settings, count)
            || settings.Mouse is not null || settings.Keyboard?.Values.Count > 0)
            throw new ArgumentException(nameof(settings));
        var controllers = EmulationInputMappingFunctions.CreateControllerBindings<PeripheralCategory, ControllerBinding>(
            settings,
            (index, peripheral) => Peripherals(configuration.Model, configuration.Core).Contains(peripheral),
            static (port, peripheral, mappings) => new(port.Number - 1, peripheral, port.PhysicalDeviceId, mappings, port.DeadZonePercent, port.VisualId));
        var result = configuration with { Input = new(settings.Keyboard?.Values, controllers) };
        ConfigurationFunctions.Validate(result);
        return result;
    }
    internal static void Validate(MachineConfiguration configuration)
    {
        if (configuration.Input.Keyboard?.Count > 0) throw new ArgumentException(nameof(configuration.Input));
        foreach (var binding in configuration.Input.Controllers ?? [])
        {
            if (!Peripherals(configuration.Model, configuration.Core).Contains(binding.Peripheral)) throw new ArgumentException(nameof(configuration.Input));
            var ids = InputSnapshotDictionary.Actions(configuration.Model, configuration.Core, binding.Peripheral).Select(action => action.Id).ToHashSet(StringComparer.Ordinal);
            if (binding.Mappings?.Any(pair => !ids.Contains(pair.Key) || !IsSource(pair.Value)) == true) throw new ArgumentException(nameof(configuration.Input));
        }
    }
    private static bool IsSource(string source) => string.IsNullOrEmpty(source)
        || source.StartsWith(InputMappingConstants.KeyboardPrefix, StringComparison.OrdinalIgnoreCase)
        || source.StartsWith(InputMappingConstants.MousePrefix, StringComparison.OrdinalIgnoreCase)
        || source.StartsWith(InputMappingConstants.ControllerPrefix, StringComparison.OrdinalIgnoreCase);
}
