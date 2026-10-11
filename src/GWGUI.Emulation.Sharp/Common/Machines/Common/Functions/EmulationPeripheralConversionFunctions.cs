namespace GWGUI.Emulation.Sharp.Common.Machines.Common.Functions;

internal static class EmulationPeripheralConversionFunctions
{
    internal static PeripheralCategory ToSharp(EmulationPeripheralCategory peripheral, MachineModel model)
    {
        if (peripheral == EmulationPeripheralCategory.None) return PeripheralCategory.None;
        if (peripheral is EmulationPeripheralCategory.Automatic or EmulationPeripheralCategory.RetroPad or EmulationPeripheralCategory.Joystick) return InputSettingsFunctions.DefaultPeripheral(model);
        if (peripheral == EmulationPeripheralCategory.AnalogJoystick && ConfigurationFunctions.Family(model) == MachineFamily.X68000) return PeripheralCategory.CyberstickAnalog;
        throw new ArgumentOutOfRangeException(nameof(peripheral));
    }
}
