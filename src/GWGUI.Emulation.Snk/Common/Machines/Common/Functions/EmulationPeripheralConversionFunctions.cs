namespace GWGUI.Emulation.Snk.Common.Machines.Common.Functions;

internal static class EmulationPeripheralConversionFunctions
{
    internal static PeripheralCategory ToModule(EmulationPeripheralCategory peripheral, MachineModel model)
    {
        if (peripheral is EmulationPeripheralCategory.None or EmulationPeripheralCategory.Automatic or EmulationPeripheralCategory.RetroPad or EmulationPeripheralCategory.Joystick) return InputSettingsFunctions.DefaultPeripheral(model);
        throw new ArgumentOutOfRangeException(nameof(peripheral));
    }
}
