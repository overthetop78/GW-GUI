using System.Collections.Frozen;

namespace GWGUI.MediaEngine.Constants;

/// <summary>Invariant Commodore TAP signatures, versions, timing units, platforms, and video standards.</summary>
internal static class CommodoreTapConstants
{
    public const string C64Signature = "C64-TAPE-RAW";
    public const string C16Signature = "C16-TAPE-RAW";
    public const int HeaderSize = 20;
    public const int SignatureOffset = 0;
    public const int SignatureLength = 12;
    public const int VersionOffset = 12;
    public const int PlatformOffset = 13;
    public const int VideoStandardOffset = 14;
    public const int ReservedOffset = 15;
    public const int DataLengthOffset = 16;
    public const int DataLengthSize = 4;
    public const byte OriginalVersion = 0;
    public const byte ExactPulseVersion = 1;
    public const byte HalfWaveVersion = 2;
    public const int ShortPulseCycleMultiplier = 8;
    public const int ExtendedPulseByteCount = 3;
    public const int Version0OverflowCycles = 255 * ShortPulseCycleMultiplier;
    public const byte C64Platform = 0;
    public const byte Vic20Platform = 1;
    public const byte C16Platform = 2;
    public const byte PetPlatform = 3;
    public const byte C5x0Platform = 4;
    public const byte C6x0Platform = 5;
    public const byte PalVideo = 0;
    public const byte NtscVideo = 1;
    public const byte OldNtscVideo = 2;
    public const byte PalNVideo = 3;
    public const string PulseMetadataKey = "commodoreTapPulse";
    public const string SignatureMetadataKey = "commodoreTapSignature";
    public const string VersionMetadataKey = "commodoreTapVersion";
    public const string PlatformMetadataKey = "commodoreTapPlatform";
    public const string VideoStandardMetadataKey = "commodoreTapVideoStandard";
    public const string ClockRateMetadataKey = "commodoreTapClockRate";
    public const string CycleCountMetadataKey = "commodoreTapCycleCount";
    public const string HalfWaveMetadataKey = "commodoreTapHalfWave";
    public const string EncodedLengthMetadataKey = "commodoreTapEncodedLength";
    public const string DecoderId = "commodore-tape";
    public const string EncoderId = "commodore-tape";
    public const int DefaultC64PalClockRate = 985248;
    public const int ShortPulseCycles = 0x30 * ShortPulseCycleMultiplier;
    public const int MediumPulseCycles = 0x42 * ShortPulseCycleMultiplier;
    public const int LongPulseCycles = 0x56 * ShortPulseCycleMultiplier;
    public const double PulseToleranceRatio = 0.25;
    public const int DataBitsPerByte = 8;
    public const int PulsesPerBit = 2;
    public const int ParityBitsPerByte = 1;
    public const int PcmBitsPerSample = 16;
    public const int DefaultPcmSampleRate = 44100;
    public const short PcmAmplitude = 16000;
    public const string SourceKindMetadataKey = "sourceKind";
    public const string ChecksumPresentMetadataKey = "checksumPresent";
    public const string ParityValidMetadataKey = "parityValid";

    public static readonly IReadOnlyDictionary<(byte Platform, byte Video), int> TimerClockRates =
        new Dictionary<(byte, byte), int>
        {
            [(C64Platform, PalVideo)] = 985248,
            [(C64Platform, NtscVideo)] = 1022730,
            [(C64Platform, OldNtscVideo)] = 1022730,
            [(C64Platform, PalNVideo)] = 1023440,
            [(Vic20Platform, PalVideo)] = 1108405,
            [(Vic20Platform, NtscVideo)] = 1022727,
            [(C16Platform, PalVideo)] = 886724,
            [(C16Platform, NtscVideo)] = 894886,
            [(PetPlatform, PalVideo)] = 1000000,
            [(PetPlatform, NtscVideo)] = 1000000,
            [(C5x0Platform, PalVideo)] = 985248,
            [(C5x0Platform, NtscVideo)] = 1022730,
            [(C6x0Platform, PalVideo)] = 2000000,
            [(C6x0Platform, NtscVideo)] = 2000000
        }.ToFrozenDictionary();
}
