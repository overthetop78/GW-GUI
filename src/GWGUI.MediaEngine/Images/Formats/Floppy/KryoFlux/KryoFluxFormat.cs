namespace GWGUI.MediaEngine.Images.Formats.Floppy.KryoFlux;

internal static class KryoFluxFormat
{
    public const byte Nop1 = 8;
    public const byte Nop2 = 9;
    public const byte Nop3 = 10;
    public const byte Overflow16 = 11;
    public const byte Flux3 = 12;
    public const byte OutOfBand = 13;

    public const byte StreamInfo = 1;
    public const byte Index = 2;
    public const byte StreamEnd = 3;
    public const byte KryoFluxInfo = 4;
    public const byte EndOfFile = 13;

    public const double DefaultMasterClock = 18_432_000d * 73d / 14d / 2d;
    public const double DefaultSampleClock = DefaultMasterClock / 2d;
    public const int MaximumTrack = 99;
    public const int MaximumHead = 1;
}
