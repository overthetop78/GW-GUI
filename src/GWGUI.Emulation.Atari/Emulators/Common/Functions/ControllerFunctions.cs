using GWGUI.Emulation;
using GWGUI.Emulation.Atari.Common.Machines.Common.Constants;

namespace GWGUI.Emulation.Atari.Emulators.Common.Functions;

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
        if (bindings is null || bindings.Count == BufferConstants.EmptyCollectionCount) return snapshot;
        var controllers = snapshot.Controllers.ToArray();
        foreach (var binding in bindings)
        {
            if (binding.Port < ControllerPortConstants.MinimumControllerPort || binding.Port >= controllers.Length) continue;
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
        return new HashSet<PeripheralCategory>(HardwareModelCatalog.Get(model).Ports
            .Where(port => port.Capability != HardwarePortCapability.Keyboard)
            .Select(port => FromCapability(port.Capability)).Append(PeripheralCategory.None)
            .Append(PeripheralCategory.Automatic));
    }

    private static PeripheralCategory FromCapability(HardwarePortCapability capability) => capability switch
    {
        HardwarePortCapability.Keyboard => PeripheralCategory.Keyboard,
        HardwarePortCapability.Joystick => PeripheralCategory.Joystick,
        HardwarePortCapability.AnalogJoystick => PeripheralCategory.AnalogJoystick,
        HardwarePortCapability.Paddle => PeripheralCategory.Paddle,
        HardwarePortCapability.DrivingController => PeripheralCategory.DrivingController,
        HardwarePortCapability.NumericKeypad => PeripheralCategory.NumericKeypad,
        HardwarePortCapability.LightGun => PeripheralCategory.LightGun,
        HardwarePortCapability.ProLineController => PeripheralCategory.ProLineController,
        HardwarePortCapability.EnhancedController => PeripheralCategory.EnhancedController,
        _ => throw new ArgumentOutOfRangeException(nameof(capability), capability, null)
    };
}
