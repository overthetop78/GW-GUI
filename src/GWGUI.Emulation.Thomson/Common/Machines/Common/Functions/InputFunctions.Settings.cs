namespace GWGUI.Emulation.Thomson.Common.Machines.Common.Functions;
internal static class InputSettingsFunctions
{
    internal static IReadOnlyDictionary<string, string> KeyboardBindings(InputConfiguration input, MachineModel model) => KeyboardDictionary.Bindings(input, model);
    internal static bool SupportsPeripheral(MachineConfiguration configuration, int port, PeripheralCategory peripheral) => Enum.IsDefined(configuration.Model) && port >= ControllerConstants.FirstPort && port < ControllerConstants.PortCount && Enum.IsDefined(peripheral);
    private static IReadOnlyList<InputBindingDefinition> MouseDefinitions() => [
        new(InputMappingConstants.MouseLeft, "Emulation.Mouse.Button.Left", InputMappingConstants.MousePrefix + InputMappingConstants.MouseLeft),
        new(InputMappingConstants.MouseRight, "Emulation.Mouse.Button.Right", InputMappingConstants.MousePrefix + InputMappingConstants.MouseRight)
    ];
    internal static EmulationInputSettings Describe(MachineConfiguration configuration)
    {
        var keyboard = new EmulationInputBindingSet(KeyboardDictionary.Definitions(configuration.Model), KeyboardBindings(configuration.Input, configuration.Model), EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller);
        var mouseValues = MouseDefinitions().ToDictionary(definition => definition.Id, definition => configuration.Input.Mouse?.GetValueOrDefault(definition.Id, definition.DefaultBinding) ?? definition.DefaultBinding, StringComparer.Ordinal);
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
        var ports = Enumerable.Range(ControllerConstants.FirstPort, ControllerConstants.PortCount).Select(port =>
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
            );
        }).ToArray();
        return new(keyboard, mouse, ports);
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationInputSettings settings)
    {
        if (settings.ControllerPorts.Count != ControllerConstants.PortCount || !settings.ControllerPorts.Select(port => port.Number).Order().SequenceEqual(Enumerable.Range(1, ControllerConstants.PortCount)))
            throw new ArgumentException(nameof(settings));
        var controllers = settings.ControllerPorts.Select(port =>
        {
            if (!Enum.TryParse<PeripheralCategory>(port.SelectedControllerId, out var peripheral) || !SupportsPeripheral(configuration, port.Number - 1, peripheral))
                throw new ArgumentException(nameof(settings));
            return new ControllerBinding(port.Number - 1, peripheral, port.PhysicalDeviceId, new Dictionary<string, string>(port.Bindings.Values, StringComparer.Ordinal), port.DeadZonePercent, port.VisualId);
        }).ToArray();
        var input = configuration.Input with
        {
            Keyboard = settings.Keyboard is null ? configuration.Input.Keyboard : new Dictionary<string, string>(settings.Keyboard.Values, StringComparer.Ordinal),
            Mouse = settings.Mouse is null ? configuration.Input.Mouse : new Dictionary<string, string>(settings.Mouse.Values, StringComparer.Ordinal),
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
        if (configuration.Input.Mouse?.Any(pair => !mouseIds.Contains(pair.Key) || !IsSource(pair.Value)) == true)
            throw new ArgumentException(nameof(configuration.Input));
        var ids = ControllerCatalog.Definitions(PeripheralCategory.Joystick).Select(definition => definition.Id).ToHashSet(StringComparer.Ordinal);
        foreach (var binding in configuration.Input.Controllers ?? [])
            if (!SupportsPeripheral(configuration, binding.Port, binding.Peripheral) || binding.Mappings?.Any(pair => !ids.Contains(pair.Key) || !IsSource(pair.Value)) == true)
                throw new ArgumentException(nameof(configuration.Input));
    }

    private static bool IsSource(string source) => EmulationKeyboardBindingFunctions.IsValid(source);
}
