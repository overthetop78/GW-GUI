namespace GWGUI.Emulation.Sharp.Common.Machines.Common.Functions;
internal static class InputSettingsFunctions
{
    internal static IReadOnlyList<PeripheralCategory> Peripherals(MachineModel model, Emulator core) => core == Emulator.None ? ControllerCatalog.Peripherals(model) : new Engine().Adapter(core) is IEmulatorInputMapper mapper ? mapper.GetPeripherals(model) : [];
    internal static bool SupportsMouse(MachineConfiguration configuration) => configuration.Core != Emulator.None && new Engine().Adapter(configuration) is IEmulatorRuntimeCapabilities { SupportsPointerSettings: true };
    private static string ControllerResource(PeripheralCategory peripheral) => peripheral switch
    {
        PeripheralCategory.None => "Emulation.Controller.None",
        PeripheralCategory.Joystick => "Controllers.Enum.Joystick",
        _ => "Emulation.Sharp.Controller." + peripheral
    };

    internal static PeripheralCategory DefaultPeripheral(MachineModel model) => ConfigurationFunctions.Family(model) == MachineFamily.X68000 ? PeripheralCategory.Joystick : PeripheralCategory.X1Joystick;
    internal static IReadOnlyDictionary<string, string> KeyboardBindings(InputConfiguration input, MachineModel model) => KeyboardDictionary.Bindings(input, model);
    private static IReadOnlyList<InputBindingDefinition> MouseDefinitions() => [
        new(InputMappingConstants.MouseLeft, InputMappingConstants.MouseLeftResource, InputMappingConstants.MousePrefix + InputMappingConstants.MouseLeft),
        new(InputMappingConstants.MouseRight, InputMappingConstants.MouseRightResource, InputMappingConstants.MousePrefix + InputMappingConstants.MouseRight)
    ];
    private static IReadOnlyDictionary<string, string> MouseBindings(InputConfiguration input) => MouseDefinitions().ToDictionary(definition => definition.Id, definition => input.Mouse?.GetValueOrDefault(definition.Id, definition.DefaultBinding) ?? definition.DefaultBinding, StringComparer.Ordinal);
    private static IReadOnlyList<PeripheralCategory> Choices(MachineConfiguration configuration, int port)
    {
        var choices = Peripherals(configuration.Model, configuration.Core).Where(category => category == PeripheralCategory.None || (ConfigurationFunctions.Family(configuration.Model) == MachineFamily.X1) == (category == PeripheralCategory.X1Joystick));
        if (port != ControllerConstants.FirstPort)
            choices = choices.Where(category => category is not PeripheralCategory.CyberstickAnalog and not PeripheralCategory.CyberstickDigital);
        return choices.ToArray();
    }

    internal static bool SupportsPeripheral(MachineConfiguration configuration, int port, PeripheralCategory peripheral) => port >= ControllerConstants.FirstPort && port < ConfigurationFunctions.ControllerPortCount(configuration.Model) && Choices(configuration, port).Contains(peripheral);
    internal static EmulationInputSettings Describe(MachineConfiguration configuration)
    {
        var keyboard = new EmulationInputBindingSet(KeyboardDictionary.Definitions(configuration.Model), KeyboardBindings(configuration.Input, configuration.Model), EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller);
        var mouse = SupportsMouse(configuration) ? new EmulationInputBindingSet(MouseDefinitions(), MouseBindings(configuration.Input), EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller, true) : null;
        var ports = Enumerable.Range(ControllerConstants.FirstPort, ConfigurationFunctions.ControllerPortCount(configuration.Model)).Select(port =>
        {
            var binding = configuration.Input.Controllers?.FirstOrDefault(item => item.Port == port);
            var peripheral = binding?.Peripheral ?? DefaultPeripheral(configuration.Model);
            var choices = Choices(configuration, port).Select(category => new EmulationControllerChoice(
                    category.ToString(),
                    ControllerResource(category),
                    BindingDefinitions: ControllerCatalog.Actions(configuration.Model, category).Select(action => action.Definition).ToArray(),
                    CompatibleVisualIds: ControllerVisualDictionary.VisualIds(category),
                    DefaultVisualId: ControllerVisualDictionary.DefaultVisualId(category),
                    VisualCommandIds: ControllerVisualDictionary.Commands(category)
                )
                {
                    Category = category switch
                    {
                        PeripheralCategory.None => EmulationControllerCategory.None,
                        PeripheralCategory.Joystick or PeripheralCategory.X1Joystick => EmulationControllerCategory.Joystick,
                        PeripheralCategory.CpsfMd or PeripheralCategory.CpsfSfc => EmulationControllerCategory.Gamepad,
                        PeripheralCategory.CyberstickDigital or PeripheralCategory.CyberstickAnalog => EmulationControllerCategory.FlightSimulation,
                        _ => throw new ArgumentOutOfRangeException(nameof(category))
                    }
                }).ToArray();
            return new EmulationControllerPort(
                port + 1,
                choices,
                peripheral.ToString(),
                binding?.DeviceId,
                new(
                    ControllerCatalog.Actions(configuration.Model, peripheral).Select(action => action.Definition).ToArray(),
                    binding?.Mappings ?? new Dictionary<string, string>(),
                    EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller,
                    true
                ),
                binding?.DeadZonePercent ?? ControllerConstants.DefaultDeadZonePercent,
                binding?.VisualId
            );
        }).ToArray();
        return new(keyboard, mouse, ports);
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationInputSettings settings)
    {
        var count = ConfigurationFunctions.ControllerPortCount(configuration.Model);
        if (settings.ControllerPorts.Count != count || !settings.ControllerPorts.Select(port => port.Number).Order().SequenceEqual(Enumerable.Range(1, count)) || !SupportsMouse(configuration) && settings.Mouse is not null)
            throw new ArgumentException(nameof(settings));
        var controllers = settings.ControllerPorts.Select(port =>
        {
            if (!Enum.TryParse<PeripheralCategory>(port.SelectedControllerId, out var peripheral) || !Choices(configuration, port.Number - 1).Contains(peripheral))
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
        if (configuration.Input.MouseSpeedPercent is < InputMappingConstants.MinimumMouseSpeedPercent or > InputMappingConstants.MaximumMouseSpeedPercent)
            throw new ArgumentException(nameof(configuration.Input));
        var mouseIds = MouseDefinitions().Select(definition => definition.Id).ToHashSet(StringComparer.Ordinal);
        if (configuration.Input.Mouse?.Any(pair => !mouseIds.Contains(pair.Key) || !IsSource(pair.Value)) == true || !SupportsMouse(configuration) && configuration.Input.Mouse?.Count > 0)
            throw new ArgumentException(nameof(configuration.Input));
        foreach (var binding in configuration.Input.Controllers ?? [])
        {
            if (!Choices(configuration, binding.Port).Contains(binding.Peripheral))
                throw new ArgumentException(nameof(configuration.Input));
            var ids = ControllerCatalog.Actions(configuration.Model, binding.Peripheral).Select(action => action.Id).ToHashSet(StringComparer.Ordinal);
            if (binding.Mappings?.Any(pair => !ids.Contains(pair.Key) || !IsSource(pair.Value)) == true)
                throw new ArgumentException(nameof(configuration.Input));
        }
    }

    private static bool IsSource(string source) => EmulationKeyboardBindingFunctions.IsValid(source);
}
