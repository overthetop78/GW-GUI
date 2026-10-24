namespace GWGUI.Emulation.Enums;

public enum EmulationPeripheralCategory
{
    None,
    Automatic,
    Keyboard,
    Mouse,
    RetroPad,
    Cd32Pad,
    Joystick,
    AnalogJoystick,
    Paddle,
    LightGun,
    NumericKeypad,
    DrivingController,
    ProLineController,
    EnhancedController,
    BoosterGrip,
    GenesisController,
    Joy2BPlus,
    /// <summary>Tapis de danse dont les panneaux utilisent les commandes communes de contrôleur.</summary>
    DanceMat,
    /// <summary>Stylet ou tablette transmettant une position absolue et un contact.</summary>
    Pointer
}
