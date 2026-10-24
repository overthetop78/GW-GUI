using CommonControllerFunctions = GWGUI.Emulation.Sharp.Common.Functions.ControllerFunctions;
using GWGUI.Emulation;
namespace GWGUI.Emulation.Sharp.Common.Machines.Common.Functions;
internal static class InputSettingsFunctions
{
    internal static IReadOnlyList<PeripheralCategory> Peripherals(MachineConfiguration configuration) => configuration.Core == Emulator.None ? ControllerCatalog.Peripherals(configuration.Model) : new Engine().Adapter(configuration.Core) is IEmulatorControllerAdapter mapper ? mapper.GetPeripherals(configuration) : [];
    internal static bool SupportsMouse(MachineConfiguration configuration) => configuration.Core != Emulator.None && new Engine().Adapter(configuration) is IEmulatorPointerCapabilities capabilities && capabilities.SupportsPointer(configuration);
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
    private static IReadOnlyDictionary<string, string> MouseBindings(InputConfiguration input) => EmulationSdk.InputProcessing.ResolveBindings(MouseDefinitions(), input.MouseMappings);
    private static IReadOnlyList<PeripheralCategory> Choices(MachineConfiguration configuration, int port)
    {
        var choices = Peripherals(configuration).Where(category => category == PeripheralCategory.None || (ConfigurationFunctions.Family(configuration.Model) == MachineFamily.X1) == (category == PeripheralCategory.X1Joystick));
        if (port != ControllerConstants.FirstPort)
            choices = choices.Where(category => category is not PeripheralCategory.CyberstickAnalog and not PeripheralCategory.CyberstickDigital);
        return choices.ToArray();
    }

    internal static bool SupportsPeripheral(MachineConfiguration configuration, int port, PeripheralCategory peripheral) => port >= ControllerConstants.FirstPort && port < CommonControllerFunctions.PortCount(configuration) && Choices(configuration, port).Contains(peripheral);
    internal static EmulationInputSettings Describe(MachineConfiguration configuration)
    {
        var keyboard = new EmulationInputBindingSet(KeyboardDictionary.Definitions(configuration.Model), KeyboardBindings(configuration.Input, configuration.Model), EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller);
        var mouse = SupportsMouse(configuration) ? new EmulationInputBindingSet(MouseDefinitions(), MouseBindings(configuration.Input), EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller, true) : null;
        var ports = Enumerable.Range(ControllerConstants.FirstPort, CommonControllerFunctions.PortCount(configuration)).Select(port =>
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
            ) { OutputBindings = binding?.OutputBindings ?? new Dictionary<string, EmulationControllerOutputBinding>() };
        }).ToArray();
        return new(keyboard, mouse, ports);
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationInputSettings settings)
    {
        var count = CommonControllerFunctions.PortCount(configuration);
        if (EmulationSdk.InputProcessing.HasInvalidControllerPortNumbers(settings, count) || !SupportsMouse(configuration) && settings.Mouse is not null)
            throw new ArgumentException(nameof(settings));
        var controllers = EmulationSdk.InputProcessing.CreateControllerBindings<PeripheralCategory, ControllerBinding>(
            settings,
            (index, peripheral) => Choices(configuration, index).Contains(peripheral),
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
        if (configuration.Input.MouseSpeedPercent is < InputMappingConstants.MinimumMouseSpeedPercent or > InputMappingConstants.MaximumMouseSpeedPercent)
            throw new ArgumentException(nameof(configuration.Input));
        var mouseIds = MouseDefinitions().Select(definition => definition.Id).ToHashSet(StringComparer.Ordinal);
        if (EmulationSdk.InputProcessing.HasInvalidBindings(configuration.Input.MouseMappings, mouseIds, CommonControllerFunctions.IsValidBinding) || !SupportsMouse(configuration) && configuration.Input.MouseMappings?.Count > 0)
            throw new ArgumentException(nameof(configuration.Input));
        foreach (var binding in configuration.Input.Controllers ?? [])
        {
            if (!Choices(configuration, binding.Port).Contains(binding.Peripheral))
                throw new ArgumentException(nameof(configuration.Input));
            var ids = ControllerCatalog.Actions(configuration.Model, binding.Peripheral).Select(action => action.Id).ToHashSet(StringComparer.Ordinal);
            if (EmulationSdk.InputProcessing.HasInvalidBindings(binding.Mappings, ids, CommonControllerFunctions.IsValidBinding))
                throw new ArgumentException(nameof(configuration.Input));
        }
    }
}
