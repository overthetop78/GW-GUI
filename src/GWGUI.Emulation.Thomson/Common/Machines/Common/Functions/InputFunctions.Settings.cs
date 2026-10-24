using CommonControllerFunctions = GWGUI.Emulation.Thomson.Common.Functions.ControllerFunctions;
using GWGUI.Emulation;
namespace GWGUI.Emulation.Thomson.Common.Machines.Common.Functions;
internal static class InputSettingsFunctions
{
    internal static IReadOnlyDictionary<string, string> KeyboardBindings(InputConfiguration input, MachineModel model) => KeyboardDictionary.Bindings(input, model);
    internal static bool SupportsPeripheral(MachineConfiguration configuration, int port, PeripheralCategory peripheral) => Enum.IsDefined(configuration.Model) && port >= ControllerConstants.FirstPort && port < CommonControllerFunctions.PortCount(configuration) && Enum.IsDefined(peripheral);
    private static IReadOnlyList<InputBindingDefinition> MouseDefinitions() => [
        new(InputMappingConstants.MouseLeft, SettingsConstants.ResourceLightPenContact, InputMappingConstants.MousePrefix + InputMappingConstants.MouseLeft)
    ];
    internal static EmulationInputSettings Describe(MachineConfiguration configuration)
    {
        var keyboard = new EmulationInputBindingSet(KeyboardDictionary.Definitions(configuration.Model), KeyboardBindings(configuration.Input, configuration.Model), EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller);
        var mouseValues = EmulationSdk.InputProcessing.ResolveBindings(MouseDefinitions(), configuration.Input.MouseMappings);
        var mouse = new EmulationInputBindingSet(MouseDefinitions(), mouseValues, EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller, true);
        var choices = new[]
        {
            new EmulationControllerChoice(PeripheralCategory.None.ToString(), "Emulation.Controller.None", BindingDefinitions: []) { Category = EmulationControllerCategory.None },
            new EmulationControllerChoice(
                PeripheralCategory.Joystick.ToString(),
                "Emulation.Thomson.Controller.Joystick",
                BindingDefinitions: ControllerCatalog.Definitions(PeripheralCategory.Joystick),
                CompatibleVisualIds: [EmulationControllerVisualIds.ThomsonMj90102],
                DefaultVisualId: EmulationControllerVisualIds.ThomsonMj90102,
                VisualCommandIds: ControllerCatalog.VisualCommands
            ) { Category = EmulationControllerCategory.Joystick }
        };
        var ports = Enumerable.Range(ControllerConstants.FirstPort, CommonControllerFunctions.PortCount(configuration)).Select(port =>
        {
            var binding = configuration.Input.Controllers?.FirstOrDefault(item => item.Port == port);
            var peripheral = binding?.Peripheral ?? PeripheralCategory.Joystick;
            return new EmulationControllerPort(
                port + 1,
                choices,
                peripheral.ToString(),
                binding?.DeviceId,
                new(
                    ControllerCatalog.Definitions(peripheral),
                    binding?.Mappings ?? new Dictionary<string, string>(),
                    EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller,
                    true,
                    CompatibleVisualIds: peripheral == PeripheralCategory.Joystick ? [EmulationControllerVisualIds.ThomsonMj90102] : [],
                    DefaultVisualId: peripheral == PeripheralCategory.Joystick ? EmulationControllerVisualIds.ThomsonMj90102 : null,
                    VisualCommandIds: peripheral == PeripheralCategory.Joystick ? ControllerCatalog.VisualCommands : null
                ),
                binding?.DeadZonePercent ?? ControllerConstants.DefaultDeadZonePercent,
                binding?.VisualId ?? (peripheral == PeripheralCategory.Joystick ? EmulationControllerVisualIds.ThomsonMj90102 : null)
            ) { OutputBindings = binding?.OutputBindings ?? new Dictionary<string, EmulationControllerOutputBinding>() };
        }).ToArray();
        return new(keyboard, mouse, ports);
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationInputSettings settings)
    {
        if (EmulationSdk.InputProcessing.HasInvalidControllerPortNumbers(settings, CommonControllerFunctions.PortCount(configuration)))
            throw new ArgumentException(nameof(settings));
        var controllers = EmulationSdk.InputProcessing.CreateControllerBindings<PeripheralCategory, ControllerBinding>(
            settings,
            (index, peripheral) => SupportsPeripheral(configuration, index, peripheral),
            static (port, peripheral, mappings) => new(port.Number - 1, peripheral, port.PhysicalDeviceId, mappings, port.DeadZonePercent, port.VisualId, OutputBindings: port.OutputBindings));
        var input = configuration.Input with
        {
            KeyboardBindings = settings.Keyboard is null ? configuration.Input.KeyboardBindings : new Dictionary<string, string>(settings.Keyboard.Values, StringComparer.Ordinal),
            MouseMappings = settings.Mouse is null ? configuration.Input.MouseMappings : new Dictionary<string, string>(settings.Mouse.Values, StringComparer.Ordinal),
            Controllers = controllers
        };
        var result = configuration with
        {
            Input = input
        };
        ConfigurationFunctions.Validate(result);
        return result;
    }

    internal static void Validate(MachineConfiguration configuration)
    {
        _ = KeyboardBindings(configuration.Input, configuration.Model);
        var mouseIds = MouseDefinitions().Select(definition => definition.Id).ToHashSet(StringComparer.Ordinal);
        mouseIds.Add(InputMappingConstants.MouseRight);
        if (EmulationSdk.InputProcessing.HasInvalidBindings(configuration.Input.MouseMappings, mouseIds, CommonControllerFunctions.IsValidBinding))
            throw new ArgumentException(nameof(configuration.Input));
        var ids = ControllerCatalog.Definitions(PeripheralCategory.Joystick).Select(definition => definition.Id).ToHashSet(StringComparer.Ordinal);
        foreach (var binding in configuration.Input.Controllers ?? [])
            if (!SupportsPeripheral(configuration, binding.Port, binding.Peripheral) || EmulationSdk.InputProcessing.HasInvalidBindings(binding.Mappings, ids, CommonControllerFunctions.IsValidBinding))
                throw new ArgumentException(nameof(configuration.Input));
    }
}
