namespace GWGUI.Emulation.Apple.Common.Machines.Common.Functions;

internal static class EmulationPeripheralConversionFunctions
{
    internal static PeripheralCategory ToModule(EmulationPeripheralCategory peripheral) => peripheral switch
    {
        EmulationPeripheralCategory.None => PeripheralCategory.None,
        EmulationPeripheralCategory.Automatic => PeripheralCategory.Automatic,
        EmulationPeripheralCategory.Keyboard => PeripheralCategory.Keyboard,
        EmulationPeripheralCategory.Mouse => PeripheralCategory.Mouse,
        EmulationPeripheralCategory.Joystick or EmulationPeripheralCategory.RetroPad
            or EmulationPeripheralCategory.AnalogJoystick => PeripheralCategory.AnalogJoystick,
        _ => throw new ArgumentOutOfRangeException(nameof(peripheral), peripheral, null)
    };
}
