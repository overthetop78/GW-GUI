namespace GWGUI.MediaEngine.Constants;

/// <summary>Invariant Atari CAS chunk identifiers and binary dimensions.</summary>
internal static class AtariCasConstants
{
    public const string FileMarkerChunk = "FUJI";
    public const string BaudRateChunk = "baud";
    public const string DataChunk = "data";
    public const string FskChunk = "fsk ";
    public const int ChunkHeaderSize = 8;
    public const int IdentifierLength = 4;
    public const int DefaultBaudRate = 600;
    public const double FskDurationUnitMilliseconds = 0.1;
    public const int SpaceFrequency = 3995;
    public const int MarkFrequency = 5327;
    public const int SerialFrameBitCount = 10;
    public const int SerialDataBitCount = 8;
    public const int MinimumSamplesPerBit = 8;
    public const int InitialMarkDurationMilliseconds = 19200;
    public const int SubsequentMarkDurationMilliseconds = 260;
    public const string DecoderId = "atari-cassette";
    public const string EncoderId = "atari-cassette";
    public const string ChunkIdMetadataKey = "chunkId";
    public const string AuxiliaryMetadataKey = "auxiliary";
    public const string BaudRateMetadataKey = "baudRate";
}
