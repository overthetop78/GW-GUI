namespace GWGUI.Emulation.Sinclair.Common.Machines.Common.Functions;

internal static class InputSettingsFunctions
{
    internal static IReadOnlyList<PeripheralCategory> Peripherals(Emulator core) => core switch
    {
        Emulator.EightyOne => [PeripheralCategory.None, PeripheralCategory.CursorJoystick],
        Emulator.Fuse => Enum.GetValues<PeripheralCategory>(),
        Emulator.None => [PeripheralCategory.None],
        _ => throw new ArgumentOutOfRangeException(nameof(core))};
    internal static PeripheralCategory DefaultPeripheral(MachineModel model) => model switch { MachineModel.Zx80 => PeripheralCategory.None, MachineModel.Zx81 => PeripheralCategory.CursorJoystick, _ => PeripheralCategory.KempstonJoystick };
    internal static IReadOnlyDictionary<string, string> KeyboardBindings(InputConfiguration input, MachineModel model) => KeyboardDictionary.Bindings(input, model);
    private static IReadOnlyList<InputBindingDefinition> MouseDefinitions() => [
        new(InputMappingConstants.MouseLeft, InputMappingConstants.MouseLeftResource, InputMappingConstants.MousePrefix + InputMappingConstants.MouseLeft),
        new(InputMappingConstants.MouseRight, InputMappingConstants.MouseRightResource, InputMappingConstants.MousePrefix + InputMappingConstants.MouseRight)
    ];
    private static IReadOnlyDictionary<string, string> MouseBindings(InputConfiguration input) => MouseDefinitions().ToDictionary(definition => definition.Id, definition => input.Mouse?.GetValueOrDefault(definition.Id, definition.DefaultBinding) ?? definition.DefaultBinding, StringComparer.Ordinal);

    private static IReadOnlyList<PeripheralCategory> Choices(MachineConfiguration configuration, int port)
    {
        var timex = configuration.Model is MachineModel.TimexTc2068 or MachineModel.TimexTs2068 or MachineModel.SpectrumSe;
        var choices = Peripherals(configuration.Core).Where(category => timex || category is not PeripheralCategory.Timex1Joystick and not PeripheralCategory.Timex2Joystick);
        return choices.ToArray();
    }

    internal static bool SupportsPeripheral(MachineConfiguration configuration, int port, PeripheralCategory peripheral) => port >= ControllerConstants.FirstPort && port < ConfigurationFunctions.ControllerPortCount(configuration.Model) && Choices(configuration, port).Contains(peripheral);

    internal static EmulationInputSettings Describe(MachineConfiguration configuration)
    {
        var keyboard = new EmulationInputBindingSet(KeyboardDictionary.Definitions(configuration.Model), KeyboardBindings(configuration.Input, configuration.Model), EmulationInputSource.Keyboard);
        var mouse = configuration.Core == Emulator.Fuse
            ? new EmulationInputBindingSet(MouseDefinitions(), MouseBindings(configuration.Input), EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller, true)
            : null;
        var ports = Enumerable.Range(ControllerConstants.FirstPort, ConfigurationFunctions.ControllerPortCount(configuration.Model)).Select(port =>
        {
            var binding = configuration.Input.Controllers?.FirstOrDefault(item => item.Port == port);
            var peripheral = binding?.Peripheral ?? DefaultPeripheral(configuration.Model);
            var choices = Choices(configuration, port).Select(category => new EmulationControllerChoice(
                category.ToString(),
                category == PeripheralCategory.None ? "Emulation.Controller.None" : "Emulation.Sinclair.Controller." + category,
                BindingDefinitions: ControllerProtocolDictionary.Actions(configuration.Model, category).Select(action => action.Definition).ToArray(),
                CompatibleVisualIds: ControllerVisualDictionary.VisualIds(category),
                DefaultVisualId: ControllerVisualDictionary.DefaultVisualId(category),
                VisualCommandIds: ControllerVisualDictionary.Commands(category)
            )).ToArray();
            return new EmulationControllerPort(
                port + 1,
                choices,
                peripheral.ToString(),
                binding?.DeviceId,
                new(
                ControllerProtocolDictionary.Actions(configuration.Model, peripheral).Select(action => action.Definition).ToArray(),
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
        if (settings.ControllerPorts.Count != count || !settings.ControllerPorts.Select(port => port.Number).Order().SequenceEqual(Enumerable.Range(1, count)) || configuration.Core != Emulator.Fuse && settings.Mouse is not null)
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
        if (configuration.Input.MouseSpeedPercent is < InputMappingConstants.MinimumMouseSpeedPercent or > InputMappingConstants.MaximumMouseSpeedPercent) throw new ArgumentException(nameof(configuration.Input));
        var mouseIds = MouseDefinitions().Select(definition => definition.Id).ToHashSet(StringComparer.Ordinal);
        if (configuration.Input.Mouse?.Any(pair => !mouseIds.Contains(pair.Key) || !IsSource(pair.Value)) == true || configuration.Core != Emulator.Fuse && configuration.Input.Mouse?.Count > 0) throw new ArgumentException(nameof(configuration.Input));
        foreach (var binding in configuration.Input.Controllers ?? [])
        {
            if (!Choices(configuration, binding.Port).Contains(binding.Peripheral)) throw new ArgumentException(nameof(configuration.Input));
            var ids = ControllerProtocolDictionary.Actions(configuration.Model, binding.Peripheral).Select(action => action.Id).ToHashSet(StringComparer.Ordinal);
            if (binding.Mappings?.Any(pair => !ids.Contains(pair.Key) || !IsSource(pair.Value)) == true) throw new ArgumentException(nameof(configuration.Input));
        }
    }

    private static bool IsSource(string source)
    {
        if (string.IsNullOrEmpty(source)) return true;
        if (source.StartsWith(InputMappingConstants.KeyboardPrefix, StringComparison.OrdinalIgnoreCase)) return Enum.TryParse<EmulationKey>(source[InputMappingConstants.KeyboardPrefix.Length..], out var key) && Enum.IsDefined(key) && key != EmulationKey.Unknown;
        return source.StartsWith(InputMappingConstants.MousePrefix, StringComparison.OrdinalIgnoreCase) || source.StartsWith(InputMappingConstants.ControllerPrefix, StringComparison.OrdinalIgnoreCase);
    }
}
