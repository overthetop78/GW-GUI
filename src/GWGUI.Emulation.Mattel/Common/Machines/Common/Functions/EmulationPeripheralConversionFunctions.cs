namespace GWGUI.Emulation.Mattel.Common.Machines.Common.Functions;

internal static class EmulationPeripheralConversionFunctions
{
    internal static PeripheralCategory ToModule(EmulationPeripheralCategory peripheral,MachineModel model)
    {
        _=ConfigurationFunctions.Family(model);
        return peripheral is EmulationPeripheralCategory.Automatic or EmulationPeripheralCategory.RetroPad or EmulationPeripheralCategory.Joystick ? PeripheralCategory.Intellivision : throw new ArgumentOutOfRangeException(nameof(peripheral));
    }
}
