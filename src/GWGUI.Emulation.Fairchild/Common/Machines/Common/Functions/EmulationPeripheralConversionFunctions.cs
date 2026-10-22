namespace GWGUI.Emulation.Fairchild.Common.Machines.Common.Functions;

internal static class EmulationPeripheralConversionFunctions
{
    internal static PeripheralCategory ToModule(EmulationPeripheralCategory peripheral, MachineModel model) =>
        peripheral switch
        {
            EmulationPeripheralCategory.Automatic or EmulationPeripheralCategory.Joystick
                or EmulationPeripheralCategory.RetroPad => PeripheralCategory.Standard,
            _ => throw new ArgumentOutOfRangeException(nameof(peripheral), peripheral, null)
        };
}
