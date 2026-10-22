namespace GWGUI.Emulation.Sinclair.Common.Machines.Common.Functions;

internal static class EmulationPeripheralConversionFunctions
{
    internal static PeripheralCategory ToModule(EmulationPeripheralCategory peripheral, MachineModel model)
    {
        if (peripheral == EmulationPeripheralCategory.None) return PeripheralCategory.None;
        if (peripheral is EmulationPeripheralCategory.Automatic or EmulationPeripheralCategory.RetroPad or EmulationPeripheralCategory.Joystick) return InputSettingsFunctions.DefaultPeripheral(model);
        throw new ArgumentOutOfRangeException(nameof(peripheral));
    }
}
