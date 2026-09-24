namespace GWGUI.MediaEngine.Constants;

/// <summary>Invariant MSX CAS separators, file markers, field sizes, and shared metadata keys.</summary>
internal static class MsxCasConstants
{
    public static ReadOnlySpan<byte> Separator => [0x1f, 0xa6, 0xde, 0xba, 0xcc, 0x13, 0x7d, 0x74];
    public const int SeparatorLength = 8;
    public const int FileTypeMarkerLength = 10;
    public const int FileNameLength = 6;
    public const byte BinaryFileMarker = 0xd0;
    public const byte TokenizedBasicFileMarker = 0xd3;
    public const byte AsciiFileMarker = 0xea;
    public const string SeparatorMetadataKey = "msxCasSeparator";
    public const string FileTypeMetadataKey = "msxCasFileType";
    public const string TimingMetadataKey = "msxCasTiming";
    public const string DecoderId = "msx-tape";
    public const string EncoderId = "msx-tape";
    public const int StandardBaudRate = 1200;
    public const int ZeroFrequency = 1200;
    public const int OneFrequency = 2400;
    public const int DataBitsPerFrame = 8;
    public const int StopBitsPerFrame = 2;
    public const int SerialFrameBitCount = 1 + DataBitsPerFrame + StopBitsPerFrame;
    public const int MinimumSamplesPerBit = 8;
    public const int PcmBitsPerSample = 16;
    public const int DefaultPcmSampleRate = 44100;
    public const short PcmAmplitude = 16000;
    public const double LongSilenceSeconds = 2;
    public const double ShortSilenceSeconds = 1;
    public const double LongCarrierSeconds = 2;
    public const double ShortCarrierSeconds = 0.5;
    public const string SourceKindMetadataKey = "sourceKind";
    public const string ReconstructionMetadataKey = "reconstructedSignal";
    public const string SerialFramesValidMetadataKey = "serialFramesValid";
}
