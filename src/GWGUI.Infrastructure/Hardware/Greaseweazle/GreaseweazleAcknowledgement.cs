namespace GWGUI.Infrastructure.Hardware.Greaseweazle;

public enum GreaseweazleAcknowledgement : byte
{
    Okay = 0,
    BadCommand = 1,
    NoIndex = 2,
    NoTrackZero = 3,
    FluxOverflow = 4,
    FluxUnderflow = 5,
    WriteProtected = 6,
    NoUnit = 7,
    NoBus = 8,
    BadUnit = 9,
    BadPin = 10,
    BadCylinder = 11,
    OutOfSram = 12,
    OutOfFlash = 13
}
