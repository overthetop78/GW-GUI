namespace GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

internal static class ControllerFunctions
{
    internal static int PortCount(MachineConfiguration configuration) =>
        new Engine().Adapter(configuration).GetControllerPortCount(configuration);
    internal static IReadOnlyList<ControllerType> Supported(MachineConfiguration configuration) =>
        new Engine().Adapter(configuration).GetControllerTypes(configuration);
    internal static IReadOnlyList<ControllerType> Supported(MachineConfiguration configuration, int port) =>
        new Engine().Adapter(configuration).GetControllerTypes(configuration, port);

    internal static ControllerType Resolve(MachineConfiguration configuration, int port)
    {
        ValidatePort(configuration, port);
        var binding = configuration.Input?.ControllerBindings?.FirstOrDefault(item => item.Port == port);
        var type = binding?.Type ?? configuration.Controllers?.ElementAtOrDefault(port)
            ?? ControllerType.Automatic;
        if (type == ControllerType.Automatic) type = ControllerCatalog.Default(configuration);
        Validate(configuration, port, type);
        return type;
    }

    internal static void ValidatePort(MachineConfiguration configuration, int port)
    {
        if (port < ControllerPortConstants.MinimumControllerPort
            || port >= PortCount(configuration))
            throw new ArgumentOutOfRangeException(nameof(port), port, null);
    }

    internal static void Validate(MachineConfiguration configuration, int port, ControllerType type)
    {
        ValidatePort(configuration, port);
        if (!Supported(configuration, port).Contains(type))
            throw new ArgumentOutOfRangeException(nameof(type), type, null);
    }

    internal static MachineConfiguration WithControllerType(MachineConfiguration configuration,
        int port, ControllerType type)
    {
        Validate(configuration, port, type);
        var input = configuration.Input ?? new InputConfiguration();
        var bindings = input.ControllerBindings ?? [];
        var binding = bindings.FirstOrDefault(item => item.Port == port)
            ?? new ControllerBinding(port, type);
        return configuration with
        {
            Input = input with
            {
                ControllerBindings = bindings.Where(item => item.Port != port)
                    .Append(binding with { Type = type }).ToArray()
            }
        };
    }

    internal static ControllerType FromCommon(EmulationPeripheralCategory peripheral,
        MachineConfiguration configuration)
    {
        var types = Supported(configuration);
        var type = peripheral switch
        {
            EmulationPeripheralCategory.None => ControllerType.None,
            EmulationPeripheralCategory.Automatic => ControllerCatalog.Default(configuration),
            EmulationPeripheralCategory.RetroPad or EmulationPeripheralCategory.Joystick => ControllerType.Joystick,
            EmulationPeripheralCategory.AnalogJoystick => types.Contains(ControllerType.AnalogJoystick)
                ? ControllerType.AnalogJoystick : ControllerType.Joystick,
            EmulationPeripheralCategory.Keyboard => ControllerType.Keyboard,
            EmulationPeripheralCategory.Mouse => ControllerType.Mouse,
            EmulationPeripheralCategory.LightGun => types.Contains(ControllerType.GunCon)
                ? ControllerType.GunCon : ControllerType.Justifier,
            EmulationPeripheralCategory.DrivingController => ControllerType.NeGcon,
            _ => throw new ArgumentOutOfRangeException(nameof(peripheral), peripheral, null)
        };
        if (!types.Contains(type))
            throw new ArgumentOutOfRangeException(nameof(peripheral), peripheral, null);
        return type;
    }
}
