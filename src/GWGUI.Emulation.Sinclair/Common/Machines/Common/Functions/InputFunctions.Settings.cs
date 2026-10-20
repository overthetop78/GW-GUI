namespace GWGUI.Emulation.Sinclair.Common.Machines.Common.Functions;
internal static class InputSettingsFunctions
{
    internal static IReadOnlyList<PeripheralCategory> Peripherals(MachineModel model, Emulator core) => core == Emulator.None ? [PeripheralCategory.None] : new Engine().Adapter(core) is IEmulatorInputSettings input ? input.GetPeripherals(model) : [];
    internal static bool SupportsMouse(MachineConfiguration configuration) => configuration.Core != Emulator.None
        && new Engine().Adapter(configuration) is IEmulatorRuntimeCapabilities { SupportsPointerSettings: true };
    internal static bool SupportsInitialContent(MachineConfiguration configuration) => configuration.Core != Emulator.None
        && new Engine().Adapter(configuration) is IEmulatorInitialContentSettings { SupportsInitialContent: true };
    internal static PeripheralCategory DefaultPeripheral(MachineModel model) => model switch
    {
        MachineModel.Zx80 => PeripheralCategory.None,
        MachineModel.Zx81 => PeripheralCategory.CursorJoystick,
        _ => PeripheralCategory.KempstonJoystick
    };
    internal static IReadOnlyDictionary<string, string> KeyboardBindings(InputConfiguration input, MachineModel model) => KeyboardDictionary.Bindings(input, model);
    private static IReadOnlyList<InputBindingDefinition> MouseDefinitions() => [
        new(InputMappingConstants.MouseLeft, InputMappingConstants.MouseLeftResource, InputMappingConstants.MousePrefix + InputMappingConstants.MouseLeft),
        new(InputMappingConstants.MouseRight, InputMappingConstants.MouseRightResource, InputMappingConstants.MousePrefix + InputMappingConstants.MouseRight)
    ];
    private static IReadOnlyDictionary<string, string> MouseBindings(InputConfiguration input) => EmulationInputMappingFunctions.ResolveBindings(MouseDefinitions(), input.Mouse);
    private static IReadOnlyList<PeripheralCategory> Choices(MachineConfiguration configuration, int port)
    {
        var timex = configuration.Model is MachineModel.TimexTc2068 or MachineModel.TimexTs2068 or MachineModel.SpectrumSe;
        var choices = Peripherals(configuration.Model, configuration.Core).Where(category => timex || category is not PeripheralCategory.Timex1Joystick and not PeripheralCategory.Timex2Joystick);
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
            var available = Choices(configuration, port);
            var defaultPeripheral = DefaultPeripheral(configuration.Model);
            var peripheral = binding?.Peripheral ?? (available.Contains(defaultPeripheral) ? defaultPeripheral : available.FirstOrDefault(PeripheralCategory.None));
            var choices = available.Select(category => new EmulationControllerChoice(
                    category.ToString(),
                    category == PeripheralCategory.None ? "Emulation.Controller.None" : "Emulation.Sinclair.Controller." + category,
                    BindingDefinitions: ControllerCatalog.Actions(configuration.Model, category).Select(action => action.Definition).ToArray(),
                    CompatibleVisualIds: ControllerVisualDictionary.VisualIds(category),
                    DefaultVisualId: ControllerVisualDictionary.DefaultVisualId(category),
                    VisualCommandIds: ControllerVisualDictionary.Commands(category)
                ) { Category = category == PeripheralCategory.None ? EmulationControllerCategory.None : EmulationControllerCategory.Joystick }).ToArray();
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
        if (EmulationInputMappingFunctions.HasInvalidControllerPortNumbers(settings, count) || !SupportsMouse(configuration) && settings.Mouse is not null)
            throw new ArgumentException(nameof(settings));
        var controllers = EmulationInputMappingFunctions.CreateControllerBindings<PeripheralCategory, ControllerBinding>(
            settings,
            (index, peripheral) => Choices(configuration, index).Contains(peripheral),
            static (port, peripheral, mappings) => new(port.Number - 1, peripheral, port.PhysicalDeviceId, mappings, port.DeadZonePercent, port.VisualId));
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
        if (EmulationInputMappingFunctions.HasInvalidBindings(configuration.Input.Mouse, mouseIds, EmulationKeyboardBindingFunctions.IsValid) || !SupportsMouse(configuration) && configuration.Input.Mouse?.Count > 0)
            throw new ArgumentException(nameof(configuration.Input));
        foreach (var binding in configuration.Input.Controllers ?? [])
        {
            if (!Choices(configuration, binding.Port).Contains(binding.Peripheral))
                throw new ArgumentException(nameof(configuration.Input));
            var ids = ControllerCatalog.Actions(configuration.Model, binding.Peripheral).Select(action => action.Id).ToHashSet(StringComparer.Ordinal);
            if (EmulationInputMappingFunctions.HasInvalidBindings(binding.Mappings, ids, EmulationKeyboardBindingFunctions.IsValid))
                throw new ArgumentException(nameof(configuration.Input));
        }
    }
}
