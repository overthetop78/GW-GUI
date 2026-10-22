namespace GWGUI.Emulation.Panasonic.Common.Machines.Common.Functions;

internal static class EmulationPeripheralConversionFunctions
{
    internal static PeripheralCategory ToModule(EmulationPeripheralCategory category, PeripheralCategory selected) => category switch
    {
        EmulationPeripheralCategory.None => PeripheralCategory.None,
        EmulationPeripheralCategory.Mouse => PeripheralCategory.Mouse,
        EmulationPeripheralCategory.LightGun => selected == PeripheralCategory.ArcadeLightgun ? selected : PeripheralCategory.Lightgun,
        EmulationPeripheralCategory.AnalogJoystick => PeripheralCategory.Flightstick,
        EmulationPeripheralCategory.Joystick => selected == PeripheralCategory.OrbatakTrackball ? selected : PeripheralCategory.ThreeDOPad,
        EmulationPeripheralCategory.Automatic or EmulationPeripheralCategory.RetroPad => PeripheralCategory.ThreeDOPad,
        _ => throw new ArgumentOutOfRangeException(nameof(category))
    };
}
