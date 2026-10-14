namespace GWGUI.Emulation.Panasonic.Common.Machines.Common.Functions;
internal static class InputSettingsFunctions
{
    private static IReadOnlyList<string> Visuals(PeripheralCategory peripheral) => peripheral == PeripheralCategory.ThreeDOPad ? [EmulationControllerVisualIds.PanasonicFzJp1] : [];
    internal static bool SupportsPointerCapture(InputConfiguration input) => input.Controllers?.Any(binding => binding.Peripheral is PeripheralCategory.Mouse or PeripheralCategory.Lightgun or PeripheralCategory.ArcadeLightgun or PeripheralCategory.OrbatakTrackball) == true;
    internal static bool SupportsPeripheral(MachineConfiguration configuration, int port, PeripheralCategory peripheral) => Enum.IsDefined(configuration.Model) && port >= ControllerConstants.FirstPort && port < ControllerConstants.PortCount && Enum.IsDefined(peripheral);
    internal static PeripheralCategory Peripheral(InputConfiguration input, int port) => input.Controllers?.FirstOrDefault(binding => binding.Port == port)?.Peripheral ?? (port == ControllerConstants.FirstPort ? PeripheralCategory.ThreeDOPad : PeripheralCategory.None);
    internal static EmulationInputSettings Describe(MachineConfiguration configuration)
    {
        var choices = Enum.GetValues<PeripheralCategory>().Select(peripheral => new EmulationControllerChoice(
                peripheral.ToString(),
                peripheral == PeripheralCategory.None ? "Emulation.Controller.None" : "Emulation.Panasonic.Controller." + peripheral,
                null,
                ControllerCatalog.Definitions(peripheral),
                Visuals(peripheral),
                Visuals(peripheral).FirstOrDefault(),
                ControllerCatalog.VisualCommands(peripheral)
            )
            {
                Category = peripheral switch
                {
                    PeripheralCategory.None => EmulationControllerCategory.None,
                    PeripheralCategory.ThreeDOPad => EmulationControllerCategory.Gamepad,
                    PeripheralCategory.Flightstick => EmulationControllerCategory.FlightSimulation,
                    PeripheralCategory.Mouse or PeripheralCategory.Lightgun or PeripheralCategory.ArcadeLightgun or PeripheralCategory.OrbatakTrackball => EmulationControllerCategory.PointingDevice,
                    _ => throw new ArgumentOutOfRangeException(nameof(peripheral))
                }
            }).ToArray();
        var ports = Enumerable.Range(ControllerConstants.FirstPort, ControllerConstants.PortCount).Select(port =>
        {
            var binding = configuration.Input.Controllers?.FirstOrDefault(item => item.Port == port);
            var peripheral = Peripheral(configuration.Input, port);
            var bindings = new EmulationInputBindingSet(
                ControllerCatalog.Definitions(peripheral),
                binding?.Mappings ?? new Dictionary<string, string>(),
                EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller,
                true,
                Visuals(peripheral),
                Visuals(peripheral).FirstOrDefault(),
                ControllerCatalog.VisualCommands(peripheral)
            );
            return new EmulationControllerPort(port + 1, choices, peripheral.ToString(), binding?.DeviceId, bindings, binding?.DeadZonePercent ?? ControllerConstants.DefaultDeadZonePercent, binding?.VisualId ?? Visuals(peripheral).FirstOrDefault());
        }).ToArray();
        return new(null, null, ports);
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationInputSettings settings)
    {
        if (settings.Keyboard is not null || settings.Mouse is not null || settings.ControllerPorts.Count != ControllerConstants.PortCount)
            throw new ArgumentException(nameof(settings));
        var controllers = new List<ControllerBinding>();
        foreach (var port in settings.ControllerPorts)
        {
            if (port.Number < 1 || port.Number > ControllerConstants.PortCount || !Enum.TryParse<PeripheralCategory>(port.SelectedControllerId, out var peripheral) || !Enum.IsDefined(peripheral))
                throw new ArgumentException(nameof(settings));
            controllers.Add(new(port.Number - 1, peripheral, port.PhysicalDeviceId, new Dictionary<string, string>(port.Bindings.Values, StringComparer.Ordinal), port.DeadZonePercent, port.VisualId));
        }

        var result = configuration with
        {
            Input = new(controllers)
        };
        ConfigurationFunctions.Validate(result);
        return result;
    }

    internal static void Validate(MachineConfiguration configuration)
    {
        var controllers = configuration.Input.Controllers ?? [];
        if (controllers.Select(binding => binding.Port).Distinct().Count() != controllers.Count)
            throw new ArgumentException(nameof(configuration.Input));
        foreach (var binding in controllers)
        {
            if (!SupportsPeripheral(configuration, binding.Port, binding.Peripheral) || binding.DeadZonePercent < ControllerConstants.MinimumDeadZonePercent || binding.DeadZonePercent > ControllerConstants.MaximumDeadZonePercent)
                throw new ArgumentException(nameof(configuration.Input));
            var commands = ControllerCatalog.Commands(binding.Peripheral).Select(command => command.ToString()).ToHashSet(StringComparer.Ordinal);
            if (binding.Mappings?.Any(pair => !commands.Contains(pair.Key) || !IsSource(pair.Value)) == true)
                throw new ArgumentException(nameof(configuration.Input));
        }
    }

    private static bool IsSource(string source)
    {
        if (source.StartsWith(InputMappingConstants.MousePrefix, StringComparison.OrdinalIgnoreCase) && source[InputMappingConstants.MousePrefix.Length..] is "X+" or "X−" or "Y+" or "Y−")
            return true;
        return EmulationKeyboardBindingFunctions.IsValid(source);
    }
}
