namespace GWGUI.Emulation.Thomson.Common.Machines.Common.Functions;
internal static class EmulationPeripheralConversionFunctions
{
    internal static PeripheralCategory ToModule(EmulationPeripheralCategory peripheral, MachineModel model)
    {
        _ = ConfigurationFunctions.Family(model);
        return peripheral switch
        {
            EmulationPeripheralCategory.None => PeripheralCategory.None,
            EmulationPeripheralCategory.Automatic or EmulationPeripheralCategory.RetroPad or EmulationPeripheralCategory.Joystick => PeripheralCategory.Joystick,
            _ => throw new ArgumentOutOfRangeException(nameof(peripheral))
        };
    }
}