using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari;

internal static class EmulationPeripheralConversionFunctions
{
    internal static AtariPeripheralCategory ToAtari(EmulationPeripheralCategory peripheral) => peripheral switch
    {
        EmulationPeripheralCategory.None => AtariPeripheralCategory.None,
        EmulationPeripheralCategory.Automatic => AtariPeripheralCategory.Automatic,
        EmulationPeripheralCategory.Keyboard => AtariPeripheralCategory.Keyboard,
        EmulationPeripheralCategory.Mouse => AtariPeripheralCategory.Mouse,
        EmulationPeripheralCategory.Joystick or EmulationPeripheralCategory.RetroPad => AtariPeripheralCategory.Joystick,
        EmulationPeripheralCategory.AnalogJoystick => AtariPeripheralCategory.AnalogJoystick,
        EmulationPeripheralCategory.Paddle => AtariPeripheralCategory.Paddle,
        EmulationPeripheralCategory.LightGun => AtariPeripheralCategory.LightGun,
        EmulationPeripheralCategory.NumericKeypad => AtariPeripheralCategory.NumericKeypad,
        EmulationPeripheralCategory.DrivingController => AtariPeripheralCategory.DrivingController,
        EmulationPeripheralCategory.ProLineController => AtariPeripheralCategory.ProLineController,
        EmulationPeripheralCategory.EnhancedController => AtariPeripheralCategory.EnhancedController,
        EmulationPeripheralCategory.BoosterGrip => AtariPeripheralCategory.BoosterGrip,
        EmulationPeripheralCategory.GenesisController => AtariPeripheralCategory.GenesisController,
        EmulationPeripheralCategory.Joy2BPlus => AtariPeripheralCategory.Joy2BPlus,
        _ => throw new ArgumentOutOfRangeException(nameof(peripheral), peripheral, null)
    };
}
