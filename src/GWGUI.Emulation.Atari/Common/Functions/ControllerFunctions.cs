using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Functions;

internal static class ControllerFunctions
{
    internal static short ApplyDeadZone(short value, int percent)
    {
        var threshold = ControllerConstants.MaximumAxisMagnitude * percent /
                        ControllerConstants.PercentageDivisor;
        return Math.Abs((int)value) <= threshold ? ControllerConstants.NeutralAxis : value;
    }

    internal static EmulationInputSnapshot ApplyDeadZones(EmulationInputSnapshot snapshot,
        IReadOnlyList<ControllerBinding>? bindings)
    {
        if (bindings is null || bindings.Count == CommonConstants.EmptyCollectionCount) return snapshot;
        var controllers = snapshot.Controllers.ToArray();
        foreach (var binding in bindings)
        {
            if (binding.Port < CommonConstants.MinimumControllerPort || binding.Port >= controllers.Length) continue;
            var controller = controllers[binding.Port];
            controllers[binding.Port] = controller with
            {
                LeftX = ApplyDeadZone(controller.LeftX, binding.DeadZonePercent),
                LeftY = ApplyDeadZone(controller.LeftY, binding.DeadZonePercent),
                RightX = ApplyDeadZone(controller.RightX, binding.DeadZonePercent),
                RightY = ApplyDeadZone(controller.RightY, binding.DeadZonePercent),
                LeftTrigger = ApplyDeadZone(controller.LeftTrigger, binding.DeadZonePercent),
                RightTrigger = ApplyDeadZone(controller.RightTrigger, binding.DeadZonePercent)
            };
        }
        return snapshot with { Controllers = controllers };
    }

    internal static IReadOnlySet<PeripheralCategory> Peripherals(MachineModel model)
    {
        if (model == MachineModel.Atari2600)
            return new HashSet<PeripheralCategory>
            {
                PeripheralCategory.None, PeripheralCategory.Automatic, PeripheralCategory.Joystick,
                PeripheralCategory.Paddle, PeripheralCategory.DrivingController,
                PeripheralCategory.BoosterGrip, PeripheralCategory.GenesisController,
                PeripheralCategory.Joy2BPlus
            };
        if (ConfigurationFunctions.GetFamily(model) == MachineFamily.St)
            return new HashSet<PeripheralCategory>
            {
                PeripheralCategory.None, PeripheralCategory.Automatic,
                PeripheralCategory.Joystick
            };
        return new HashSet<PeripheralCategory>(ClassicModelCatalog.Get(model).Ports
            .Where(port => port.Capability != ClassicPortCapability.Keyboard)
            .Select(port => FromCapability(port.Capability)).Append(PeripheralCategory.None)
            .Append(PeripheralCategory.Automatic));
    }

    private static PeripheralCategory FromCapability(ClassicPortCapability capability) => capability switch
    {
        ClassicPortCapability.Keyboard => PeripheralCategory.Keyboard,
        ClassicPortCapability.Joystick => PeripheralCategory.Joystick,
        ClassicPortCapability.AnalogJoystick => PeripheralCategory.AnalogJoystick,
        ClassicPortCapability.Paddle => PeripheralCategory.Paddle,
        ClassicPortCapability.DrivingController => PeripheralCategory.DrivingController,
        ClassicPortCapability.NumericKeypad => PeripheralCategory.NumericKeypad,
        ClassicPortCapability.LightGun => PeripheralCategory.LightGun,
        ClassicPortCapability.ProLineController => PeripheralCategory.ProLineController,
        ClassicPortCapability.EnhancedController => PeripheralCategory.EnhancedController,
        _ => throw new ArgumentOutOfRangeException(nameof(capability), capability, null)
    };
}

internal static class ControllerPortFunctions
{
    internal static void Configure(ExternalCoreExports exports, ExternalHostCallbacks callbacks,
        MachineConfiguration configuration)
    {
        var portCount = CompatibilityCatalog.Get(configuration.Model).ControllerPortCount;
        for (var port = CommonConstants.MinimumControllerPort; port < portCount; port++)
        {
            var binding = configuration.Input.Controllers?.FirstOrDefault(item => item.Port == port);
            exports.SetControllerPortDevice((uint)port, ResolveDevice(callbacks.ControllerPorts, port,
                binding?.Peripheral ?? PeripheralCategory.Automatic, configuration.Core));
        }
    }

    internal static uint ResolveDevice(IReadOnlyList<ControllerPort> ports, int port,
        PeripheralCategory peripheral, Emulator? core = null)
    {
        if (peripheral == PeripheralCategory.None)
            return CoreLifecycleConstants.NoDevice;

        var devices = port < ports.Count ? ports[port].Devices : [];
        if (core == Emulator.Stella && peripheral is not PeripheralCategory.None)
            return devices.FirstOrDefault(device =>
                       device.Description.Contains(ControllerPortFunctionsConstants.Automatic, StringComparison.OrdinalIgnoreCase))?.Id
                   ?? devices.FirstOrDefault()?.Id
                   ?? CoreLifecycleConstants.DefaultJoypadDevice;
        var name = peripheral switch
        {
            PeripheralCategory.Automatic => CoreLifecycleConstants.JoypadDeviceName,
            PeripheralCategory.Keyboard => CoreLifecycleConstants.KeyboardDeviceName,
            PeripheralCategory.Mouse => CoreLifecycleConstants.MouseDeviceName,
            PeripheralCategory.Joystick => CoreLifecycleConstants.JoystickDeviceName,
            PeripheralCategory.AnalogJoystick => CoreLifecycleConstants.AnalogDeviceName,
            PeripheralCategory.Paddle => CoreLifecycleConstants.PaddleDeviceName,
            PeripheralCategory.LightGun => CoreLifecycleConstants.LightGunDeviceName,
            PeripheralCategory.NumericKeypad => CoreLifecycleConstants.NumericKeypadDeviceName,
            PeripheralCategory.DrivingController => CoreLifecycleConstants.DrivingControllerDeviceName,
            PeripheralCategory.ProLineController => CoreLifecycleConstants.ProLineControllerDeviceName,
            PeripheralCategory.EnhancedController => CoreLifecycleConstants.EnhancedControllerDeviceName,
            PeripheralCategory.BoosterGrip => ControllerPortFunctionsConstants.BoosterGrip,
            PeripheralCategory.GenesisController => ControllerPortFunctionsConstants.Genesis,
            PeripheralCategory.Joy2BPlus => ControllerPortFunctionsConstants.Joy2B,
            _ => throw new ArgumentOutOfRangeException(nameof(peripheral))
        };
        var selected = devices.FirstOrDefault(device =>
            device.Description.Contains(name, StringComparison.OrdinalIgnoreCase));
        if (selected is not null)
            return selected.Id;
        // Several cores expose the physical emulated controller only as a generic
        // Libretro joypad. The selected semantic type still drives GW GUI's mappings.
        if (peripheral is PeripheralCategory.Joystick or PeripheralCategory.AnalogJoystick
            or PeripheralCategory.Paddle or PeripheralCategory.DrivingController
            or PeripheralCategory.ProLineController or PeripheralCategory.EnhancedController
            or PeripheralCategory.LightGun or PeripheralCategory.NumericKeypad
            or PeripheralCategory.BoosterGrip or PeripheralCategory.GenesisController
            or PeripheralCategory.Joy2BPlus)
            return devices.FirstOrDefault(device =>
                       device.Description.Contains(CoreLifecycleConstants.JoypadDeviceName,
                           StringComparison.OrdinalIgnoreCase)
                       || device.Description.Contains(CoreLifecycleConstants.JoystickDeviceName,
                           StringComparison.OrdinalIgnoreCase))?.Id
                   ?? devices.FirstOrDefault()?.Id
                   ?? CoreLifecycleConstants.DefaultJoypadDevice;
        if (peripheral == PeripheralCategory.Automatic)
            return devices.FirstOrDefault()?.Id ?? CoreLifecycleConstants.DefaultJoypadDevice;
        throw new InvalidDataException(ErrorMessages.UnsupportedControllerDevice);
    }

    internal static void ConfigurePort(ExternalCoreExports exports, ExternalHostCallbacks callbacks,
        MachineConfiguration configuration, int port, PeripheralCategory peripheral)
    {
        var definition = CompatibilityCatalog.Get(configuration.Model);
        if (port < CommonConstants.MinimumControllerPort || port >= definition.ControllerPortCount)
            throw new ArgumentOutOfRangeException(nameof(port), ErrorMessages.InvalidControllerPort);
        if (!ControllerFunctions.Peripherals(configuration.Model).Contains(peripheral))
            throw new InvalidDataException(ErrorMessages.UnsupportedControllerDevice);
        exports.SetControllerPortDevice(checked((uint)port), ResolveDevice(callbacks.ControllerPorts, port,
            peripheral, configuration.Core));
    }
}
