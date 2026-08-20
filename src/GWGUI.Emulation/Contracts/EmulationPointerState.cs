namespace GWGUI.Emulation;

public sealed record EmulationPointerState(
    int DeltaX,
    int DeltaY,
    int Wheel,
    bool Left,
    bool Right,
    bool Middle);
