namespace GWGUI.Emulation.Contracts;

public sealed record EmulationPointerState(
    int DeltaX,
    int DeltaY,
    int Wheel,
    bool Left,
    bool Right,
    bool Middle,
    bool ExtendedButton1 = false,
    bool ExtendedButton2 = false,
    int HorizontalWheel = 0);
