using GWGUI.Emulation;
using GWGUI.Emulation.Philips.Common.Machines.P2000.Dictionaries;

namespace GWGUI.Emulation.Philips.Common.Machines.Common.Functions;

internal static class InputSettingsFunctions
{
    internal static IReadOnlyList<PeripheralCategory> Peripherals(MachineModel model, Emulator core) => new Engine().Adapter(core) is IEmulatorControllerAdapter mapper ? mapper.GetPeripherals(model) : [];
    private static string ControllerResource(PeripheralCategory peripheral) => peripheral switch
    {
        PeripheralCategory.Joystick => "Controllers.Enum.Joystick",
        PeripheralCategory.Mouse => "Emulation.Tab.Mouse",
        PeripheralCategory.None => "Emulation.Controller.None",
        _ => "Emulation.Philips.Controller." + peripheral
    };

    internal static PeripheralCategory DefaultPeripheral(MachineModel model) => ConfigurationFunctions.Family(model) == MachineFamily.Cdi ? PeripheralCategory.CdiGamepad : PeripheralCategory.Joystick;
    internal static IReadOnlyDictionary<string, string> KeyboardBindings(InputConfiguration input, MachineModel model) =>
        ConfigurationFunctions.Family(model) == MachineFamily.P2000
            ? EmulationSdk.InputProcessing.ResolveBindings(P2000KeyboardDictionary.Definitions, input.Keyboard)
            : new Dictionary<string, string>();

    internal static EmulationInputSettings Describe(MachineConfiguration configuration)
    {
        var keyboard = configuration.Family == MachineFamily.P2000 ? new EmulationInputBindingSet(P2000KeyboardDictionary.Definitions, configuration.Input.Keyboard ?? new Dictionary<string, string>(),
            EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller) : null;
        var ports = Enumerable.Range(ControllerConstants.FirstPort, ConfigurationFunctions.ControllerPortCount(configuration.Model))
            .Select(port =>
            {
                var binding = configuration.Input.Controllers?.FirstOrDefault(item => item.Port == port);
                var peripheral = binding?.Peripheral ?? DefaultPeripheral(configuration.Model);
                var choices = Peripherals(configuration.Model, configuration.Core).Select(category => new EmulationControllerChoice(category.ToString(),
                    ControllerResource(category),
                    BindingDefinitions: InputSnapshotDictionary.Actions(configuration.Model, configuration.Core, category).Select(action => action.Definition).ToArray(),
                    CompatibleVisualIds: ControllerVisualDictionary.VisualId(category) is { } visualId ? [visualId] : [],
                    DefaultVisualId: ControllerVisualDictionary.VisualId(category),
                    VisualCommandIds: ControllerVisualDictionary.Commands(category))
                {
                    Category = category switch
                    {
                        PeripheralCategory.None => EmulationControllerCategory.None,
                        PeripheralCategory.Joystick => EmulationControllerCategory.Joystick,
                        PeripheralCategory.CdiGamepad => EmulationControllerCategory.Gamepad,
                        PeripheralCategory.Mouse or PeripheralCategory.LightGun => EmulationControllerCategory.PointingDevice,
                        _ => throw new ArgumentOutOfRangeException(nameof(category))
                    }
                }).ToArray();
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
        if (EmulationSdk.InputProcessing.HasInvalidControllerPortNumbers(settings, count)
            || settings.Mouse is not null || (configuration.Family != MachineFamily.P2000 && settings.Keyboard?.Values.Count > 0))
            throw new ArgumentException(nameof(settings));
        var controllers = EmulationSdk.InputProcessing.CreateControllerBindings<PeripheralCategory, ControllerBinding>(
            settings,
            (index, peripheral) => Peripherals(configuration.Model, configuration.Core).Contains(peripheral),
            static (port, peripheral, mappings) => new(port.Number - 1, peripheral, port.PhysicalDeviceId, mappings, port.DeadZonePercent, port.VisualId));
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
            if (!Peripherals(configuration.Model, configuration.Core).Contains(binding.Peripheral)) throw new ArgumentException(nameof(configuration.Input));
            var ids = InputSnapshotDictionary.Actions(configuration.Model, configuration.Core, binding.Peripheral).Select(action => action.Id).ToHashSet(StringComparer.Ordinal);
            if (EmulationSdk.InputProcessing.HasInvalidBindings(binding.Mappings, ids, EmulationSdk.InputProcessing.IsValidSourcePrefix)) throw new ArgumentException(nameof(configuration.Input));
        }
    }
}
