using System.Collections.Frozen;

namespace GWGUI.MediaEngine.Constants;

/// <summary>Invariant UEF header, chunk identifiers, limits, and shared metadata keys.</summary>
internal static class UefConstants
{
    public const string Signature = "UEF File!\0";
    public const int SignatureLength = 10;
    public const int HeaderSize = 12;
    public const byte SupportedMajorVersion = 0;
    public const byte SupportedMinorVersion = 10;
    public const int ChunkHeaderSize = 6;
    public const int MaximumDecompressedLength = 256 * 1024 * 1024;
    public const byte GzipIdentificationFirst = 0x1f;
    public const byte GzipIdentificationSecond = 0x8b;

    public const ushort ImplicitData = 0x0100;
    public const ushort MultiplexedData = 0x0101;
    public const ushort ExplicitData = 0x0102;
    public const ushort DefinedData = 0x0104;
    public const ushort CarrierTone = 0x0110;
    public const ushort CarrierToneWithDummy = 0x0111;
    public const ushort IntegerGap = 0x0112;
    public const ushort BaseFrequency = 0x0113;
    public const ushort SecurityCycles = 0x0114;
    public const ushort PhaseChange = 0x0115;
    public const ushort FloatingGap = 0x0116;
    public const ushort BaudRate = 0x0117;
    public const ushort PositionMarker = 0x0120;
    public const ushort TapeSetInformation = 0x0130;
    public const ushort TapeSide = 0x0131;

    public const int DefaultBaseFrequency = 1200;
    public const int DefaultBaudRate = 1200;
    public const int AcornZeroFrequency = 1200;
    public const int AcornOneFrequency = 2400;
    public const int AcornDataBitsPerFrame = 8;
    public const int AcornStopBitsPerFrame = 1;
    public const int MinimumSamplesPerBit = 8;
    public const byte AcornBlockSync = 0x2a;
    public const int AcornMaximumFileNameLength = 10;
    public const int AcornMaximumDataLength = 256;
    public const int AcornLoadAddressLength = 4;
    public const int AcornExecutionAddressLength = 4;
    public const int AcornBlockNumberLength = 2;
    public const int AcornDataLengthFieldLength = 2;
    public const int AcornFlagsLength = 1;
    public const int AcornNextAddressLength = 4;
    public const int AcornChecksumLength = 2;
    public const ushort AcornChecksumPolynomial = 0x1021;
    public const ushort AcornChecksumInitialValue = 0x0000;
    public const int DefaultPcmSampleRate = 44100;
    public const int PcmBitsPerSample = 16;
    public const short PcmAmplitude = 16000;
    public const double InitialCarrierSeconds = 2;
    public const double InterBlockCarrierSeconds = 0.5;
    public const double InterBlockGapSeconds = 0.25;
    public const string ChunkIdMetadataKey = "uefChunkId";
    public const string GzipMetadataKey = "uefGzip";
    public const string MajorVersionMetadataKey = "uefMajorVersion";
    public const string MinorVersionMetadataKey = "uefMinorVersion";
    public const string BaseFrequencyMetadataKey = "uefBaseFrequency";
    public const string BaudRateMetadataKey = "uefBaudRate";
    public const string PhaseMetadataKey = "uefPhase";
    public const string SourceKindMetadataKey = "sourceKind";
    public const string FileNameMetadataKey = "fileName";
    public const string LoadAddressMetadataKey = "loadAddress";
    public const string ExecutionAddressMetadataKey = "executionAddress";
    public const string BlockNumberMetadataKey = "blockNumber";
    public const string FlagsMetadataKey = "flags";
    public const string NextAddressMetadataKey = "nextAddress";
    public const string ReconstructionMetadataKey = "reconstructedSignal";
    public const string DecoderId = "acorn-kcs-tape";
    public const string EncoderId = "acorn-kcs-tape";

    public static readonly IReadOnlySet<ushort> UnderstoodTapeChunks = new ushort[]
    {
        ImplicitData, MultiplexedData, ExplicitData, DefinedData, CarrierTone, CarrierToneWithDummy,
        IntegerGap, BaseFrequency, SecurityCycles, PhaseChange, FloatingGap, BaudRate,
        PositionMarker, TapeSetInformation, TapeSide
    }.ToFrozenSet();
}
