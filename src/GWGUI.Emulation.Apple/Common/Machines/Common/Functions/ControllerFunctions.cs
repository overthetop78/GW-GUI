using GWGUI.Emulation;

namespace GWGUI.Emulation.Apple.Common.Machines.Common.Functions;

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
            controllers[binding.Port] = EmulationInputMappingFunctions.ApplyDeadZones(controller, binding.DeadZonePercent);
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
        HardwarePortCapability.Mouse => PeripheralCategory.Mouse,
        HardwarePortCapability.AnalogJoystick => PeripheralCategory.AnalogJoystick,
        _ => throw new ArgumentOutOfRangeException(nameof(capability), capability, null)
    };
}
