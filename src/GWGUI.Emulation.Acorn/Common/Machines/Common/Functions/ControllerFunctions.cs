using GWGUI.Emulation;
using GWGUI.Emulation.Acorn.Common.Machines.Common.Constants;

namespace GWGUI.Emulation.Acorn.Common.Machines.Common.Functions;

internal static class ControllerFunctions
{
    internal static short ApplyDeadZone(short value, int percent)
    => GWGUI.Emulation.Functions.EmulationInputMappingFunctions.ApplyDeadZone(value, percent);

    internal static EmulationInputSnapshot ApplyDeadZones(EmulationInputSnapshot snapshot, IReadOnlyList<ControllerBinding>? bindings)
    {
        if (bindings is null || bindings.Count == BufferConstants.EmptyCollectionCount) return snapshot;
        var controllers = snapshot.Controllers.ToArray();
        foreach (var binding in bindings)
        {
            if (binding.Port < ControllerPortConstants.MinimumControllerPort || binding.Port >= controllers.Length) continue;
            if (binding.Peripheral == PeripheralCategory.None)
            {
                controllers[binding.Port] = EmulationControllerState.Empty;
                continue;
            }
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
        return new HashSet<PeripheralCategory>(HardwareModelCatalog.Get(model).Ports.Where(port => port.Capability != HardwarePortCapability.Keyboard).Select(port => FromCapability(port.Capability)).Append(PeripheralCategory.None)
            .Append(PeripheralCategory.Automatic));
    }

    private static PeripheralCategory FromCapability(HardwarePortCapability capability) => capability switch
    {
        HardwarePortCapability.Keyboard => PeripheralCategory.Keyboard,
        HardwarePortCapability.AnalogJoystick => PeripheralCategory.AnalogJoystick,
        _ => throw new ArgumentOutOfRangeException(nameof(capability), capability, null)
    };
}


