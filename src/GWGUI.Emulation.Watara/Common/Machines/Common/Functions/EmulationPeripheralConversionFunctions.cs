namespace GWGUI.Emulation.Watara.Common.Machines.Common.Functions;

internal static class EmulationPeripheralConversionFunctions
{
    internal static PeripheralCategory ToModule(EmulationPeripheralCategory peripheral, MachineModel model)
    {
        if (!Enum.IsDefined(model)) throw new ArgumentOutOfRangeException(nameof(model));
        return peripheral switch { EmulationPeripheralCategory.Automatic or EmulationPeripheralCategory.RetroPad or EmulationPeripheralCategory.Joystick => PeripheralCategory.Supervision, _ => throw new ArgumentOutOfRangeException(nameof(peripheral)) };
    }
}