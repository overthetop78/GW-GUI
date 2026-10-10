namespace GWGUI.Emulation.Bandai.Common.Machines.Common.Functions;

internal static class EmulationPeripheralConversionFunctions
{
    internal static PeripheralCategory ToBandai(EmulationPeripheralCategory peripheral, MachineModel model) =>
        peripheral switch
        {
            EmulationPeripheralCategory.None => PeripheralCategory.None,
            EmulationPeripheralCategory.Automatic => PeripheralCategory.Automatic,
            EmulationPeripheralCategory.Joystick or EmulationPeripheralCategory.RetroPad =>
                ConfigurationFunctions.DefaultPeripheral(model),
            _ => throw new ArgumentOutOfRangeException(nameof(peripheral), peripheral, null)
        };
}
