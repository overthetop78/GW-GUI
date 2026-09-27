using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Machines.Common.Functions;

internal static class EmulationPeripheralConversionFunctions
{
    internal static PeripheralCategory ToAtari(EmulationPeripheralCategory peripheral) => peripheral switch
    {
        EmulationPeripheralCategory.None => PeripheralCategory.None,
        EmulationPeripheralCategory.Automatic => PeripheralCategory.Automatic,
        EmulationPeripheralCategory.Keyboard => PeripheralCategory.Keyboard,
        EmulationPeripheralCategory.Mouse => PeripheralCategory.Mouse,
        EmulationPeripheralCategory.Joystick or EmulationPeripheralCategory.RetroPad => PeripheralCategory.Joystick,
        EmulationPeripheralCategory.AnalogJoystick => PeripheralCategory.AnalogJoystick,
        EmulationPeripheralCategory.Paddle => PeripheralCategory.Paddle,
        EmulationPeripheralCategory.LightGun => PeripheralCategory.LightGun,
        EmulationPeripheralCategory.NumericKeypad => PeripheralCategory.NumericKeypad,
        EmulationPeripheralCategory.DrivingController => PeripheralCategory.DrivingController,
        EmulationPeripheralCategory.ProLineController => PeripheralCategory.ProLineController,
        EmulationPeripheralCategory.EnhancedController => PeripheralCategory.EnhancedController,
        EmulationPeripheralCategory.BoosterGrip => PeripheralCategory.BoosterGrip,
        EmulationPeripheralCategory.GenesisController => PeripheralCategory.GenesisController,
        EmulationPeripheralCategory.Joy2BPlus => PeripheralCategory.Joy2BPlus,
        _ => throw new ArgumentOutOfRangeException(nameof(peripheral), peripheral, null)
    };
}
