namespace GWGUI.Emulation.Sony.Emulators.Rpcs3.Enums;

// Native numeric values; nonnumeric modes use reserved values.
internal enum FrameLimit
{
    Auto = -1,
    Ps3 = -3,
    Off = -2,
    _30 = 30,
    _50 = 50,
    _60 = 60,
    _120 = 120,
    _144 = 144,
    _240 = 240,
}
