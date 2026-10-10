namespace GWGUI.Emulation.Coleco.Common.Machines.Common.Functions;

internal static class EmulationPeripheralConversionFunctions
{
    internal static PeripheralCategory ToColeco(EmulationPeripheralCategory peripheral, MachineModel model) =>
        peripheral switch
        {
            EmulationPeripheralCategory.None => PeripheralCategory.None,
            EmulationPeripheralCategory.Automatic => PeripheralCategory.Standard,
            EmulationPeripheralCategory.Joystick or EmulationPeripheralCategory.RetroPad =>
                PeripheralCategory.Standard,
            EmulationPeripheralCategory.Paddle => PeripheralCategory.Paddle,
            EmulationPeripheralCategory.DrivingController => PeripheralCategory.Driving,
            EmulationPeripheralCategory.EnhancedController => PeripheralCategory.SuperAction,
            EmulationPeripheralCategory.Mouse => PeripheralCategory.Roller,
            _ => throw new ArgumentOutOfRangeException(nameof(peripheral), peripheral, null)
        };
}


