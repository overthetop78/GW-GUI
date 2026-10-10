using GWGUI.Emulation.Philips.Common.Machines.P2000.Dictionaries;

namespace GWGUI.Emulation.Philips.Common.Machines.Common.Functions;

internal static class InputSettingsFunctions
{
    internal static IReadOnlyList<PeripheralCategory> Peripherals(Emulator core) => core switch
    {
        Emulator.O2em => [PeripheralCategory.Joystick, PeripheralCategory.None],
        Emulator.SameCdi => [PeripheralCategory.CdiGamepad, PeripheralCategory.Mouse, PeripheralCategory.LightGun, PeripheralCategory.None],
        Emulator.M2000 => [],
        _ => throw new ArgumentOutOfRangeException(nameof(core))
    };
    internal static PeripheralCategory DefaultPeripheral(MachineModel model) =>
        ConfigurationFunctions.Family(model) == MachineFamily.Cdi ? PeripheralCategory.CdiGamepad : PeripheralCategory.Joystick;
    internal static IReadOnlyDictionary<string, string> KeyboardBindings(InputConfiguration input, MachineModel model) =>
        ConfigurationFunctions.Family(model) == MachineFamily.P2000
            ? P2000KeyboardDictionary.Definitions.ToDictionary(definition => definition.Id,
                definition => input.Keyboard?.GetValueOrDefault(definition.Id, definition.DefaultBinding) ?? definition.DefaultBinding,
                StringComparer.Ordinal)
            : new Dictionary<string, string>();

    internal static EmulationInputSettings Describe(MachineConfiguration configuration)
    {
        var keyboard = configuration.Family == MachineFamily.P2000
            ? new EmulationInputBindingSet(P2000KeyboardDictionary.Definitions,
                configuration.Input.Keyboard ?? new Dictionary<string, string>(), EmulationInputSource.Keyboard)
            : null;
        var ports = Enumerable.Range(ControllerConstants.FirstPort, ConfigurationFunctions.ControllerPortCount(configuration.Model))
            .Select(port =>
            {
                var binding = configuration.Input.Controllers?.FirstOrDefault(item => item.Port == port);
                var peripheral = binding?.Peripheral ?? DefaultPeripheral(configuration.Model);
                var choices = Peripherals(configuration.Core).Select(category => new EmulationControllerChoice(category.ToString(),
                    "Emulation.Philips.Controller." + category,
                    BindingDefinitions: ControllerProtocolDictionary.Actions(configuration.Model, category).Select(action => action.Definition).ToArray(),
                    CompatibleVisualIds: ControllerVisualDictionary.VisualId(category) is { } visualId ? [visualId] : [],
                    DefaultVisualId: ControllerVisualDictionary.VisualId(category),
                    VisualCommandIds: ControllerVisualDictionary.Commands(category))).ToArray();
                return new EmulationControllerPort(port + 1, choices, peripheral.ToString(), binding?.DeviceId,
                    new(ControllerProtocolDictionary.Actions(configuration.Model, peripheral).Select(action => action.Definition).ToArray(),
                        binding?.Mappings ?? new Dictionary<string, string>(),
                        EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller, true),
                    binding?.DeadZonePercent ?? ControllerConstants.DefaultDeadZonePercent, binding?.VisualId);
            }).ToArray();
        return new(keyboard, null, ports);
    }
    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationInputSettings settings)
    {
        var count = ConfigurationFunctions.ControllerPortCount(configuration.Model);
        if (settings.ControllerPorts.Count != count || !settings.ControllerPorts.Select(port => port.Number).Order().SequenceEqual(Enumerable.Range(1, count))
            || settings.Mouse is not null || (configuration.Family != MachineFamily.P2000 && settings.Keyboard?.Values.Count > 0))
            throw new ArgumentException(nameof(settings));
        var controllers = settings.ControllerPorts.Select(port =>
        {
            if (!Enum.TryParse<PeripheralCategory>(port.SelectedControllerId, out var peripheral) || !Peripherals(configuration.Core).Contains(peripheral))
                throw new ArgumentException(nameof(settings));
            return new ControllerBinding(port.Number - 1, peripheral, port.PhysicalDeviceId,
                new Dictionary<string, string>(port.Bindings.Values, StringComparer.Ordinal), port.DeadZonePercent, port.VisualId);
        }).ToArray();
        var result = configuration with { Input = new(settings.Keyboard?.Values, controllers) };
        ConfigurationFunctions.Validate(result);
        return result;
    }
    internal static void Validate(MachineConfiguration configuration)
    {
        if (configuration.Family == MachineFamily.P2000) P2000KeyboardDictionary.Validate(configuration.Input.Keyboard);
        else if (configuration.Input.Keyboard?.Count > 0) throw new ArgumentException(nameof(configuration.Input));
        foreach (var binding in configuration.Input.Controllers ?? [])
        {
            if (!Peripherals(configuration.Core).Contains(binding.Peripheral)) throw new ArgumentException(nameof(configuration.Input));
            var ids = ControllerProtocolDictionary.Actions(configuration.Model, binding.Peripheral).Select(action => action.Id).ToHashSet(StringComparer.Ordinal);
            if (binding.Mappings?.Any(pair => !ids.Contains(pair.Key) || !IsSource(pair.Value)) == true) throw new ArgumentException(nameof(configuration.Input));
        }
    }
    private static bool IsSource(string source) => string.IsNullOrEmpty(source)
        || source.StartsWith(InputMappingConstants.KeyboardPrefix, StringComparison.OrdinalIgnoreCase)
        || source.StartsWith(InputMappingConstants.MousePrefix, StringComparison.OrdinalIgnoreCase)
        || source.StartsWith(InputMappingConstants.ControllerPrefix, StringComparison.OrdinalIgnoreCase);
}
