namespace GWGUI.MediaEngine.Containers.Hfe;

/// <summary>Regroupe les valeurs conventionnelles du format HFE version 1.</summary>
public static class HfeFormat
{
    public static ReadOnlySpan<byte> Signature => "HXCPICFE"u8;
    public const byte Revision = 0;
    public const byte IsoMfmEncoding = 0x00;
    public const byte AmigaMfmEncoding = 0x01;
    public const byte IsoFmEncoding = 0x02;
    public const byte UnknownInterfaceMode = 0xff;
    public const byte WriteProtected = 1;
    public const byte WriteAllowed = 0xff;
    public const byte SingleStep = 0xff;
    public const byte HeaderPadding = 0xff;
    public const byte TrackPadding = 0x88;
    public const ushort UnspecifiedRpm = 0;
    public const int MaximumHeadCount = 2;
    public const int TickNanoseconds = 25;
    public const int NanosecondsPerSecond = 1_000_000_000;
    public const int BitsPerDataBit = 2;
    public const int BitsPerByte = 8;
}
