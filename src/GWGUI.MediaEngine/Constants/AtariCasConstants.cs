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
    public const string ChunkCountMetadataKey = "chunkCount";
    public const string ChunkTypesMetadataKey = "chunkTypes";
    public const string DataChunkCountMetadataKey = "dataChunkCount";
    public const string FskChunkCountMetadataKey = "fskChunkCount";
    public const string BaudRatesMetadataKey = "baudRates";
    public const string InternalNameMetadataKey = "internalName";
    public const int StandardRecordLength = 132;
    public const byte RecordSyncByte = 0x55;
    public const byte FullRecordType = 0xfc;
    public const byte PartialRecordType = 0xfa;
    public const byte EndRecordType = 0xfe;
    public const int RecordDataOffset = 3;
    public const int RecordDataLength = 128;
    public const int PartialRecordLengthOffset = 130;
    public const int RecordChecksumOffset = 131;
}
