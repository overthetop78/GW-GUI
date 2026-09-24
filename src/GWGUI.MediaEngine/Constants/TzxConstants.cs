namespace GWGUI.MediaEngine.Constants;

/// <summary>Invariant TZX 1.20 header, block identifiers, and standard Spectrum signal timings.</summary>
internal static class TzxConstants
{
    public const string Signature = "ZXTape!";
    public const byte EndOfTextMarker = 0x1a;
    public const byte MajorVersion = 1;
    public const byte MinorVersion = 20;
    public const int HeaderSize = 10;

    public const byte StandardSpeedData = 0x10;
    public const byte TurboSpeedData = 0x11;
    public const byte PureTone = 0x12;
    public const byte PulseSequence = 0x13;
    public const byte PureData = 0x14;
    public const byte DirectRecording = 0x15;
    public const byte CswRecording = 0x18;
    public const byte GeneralizedData = 0x19;
    public const byte Pause = 0x20;
    public const byte GroupStart = 0x21;
    public const byte GroupEnd = 0x22;
    public const byte Jump = 0x23;
    public const byte LoopStart = 0x24;
    public const byte LoopEnd = 0x25;
    public const byte CallSequence = 0x26;
    public const byte Return = 0x27;
    public const byte Select = 0x28;
    public const byte StopIf48K = 0x2a;
    public const byte SetSignalLevel = 0x2b;
    public const byte TextDescription = 0x30;
    public const byte Message = 0x31;
    public const byte ArchiveInfo = 0x32;
    public const byte HardwareType = 0x33;
    public const byte CustomInfo = 0x35;
    public const byte Glue = 0x5a;

    public const int TStatesPerSecond = 3_500_000;
    public const ushort StandardPilotPulseTStates = 2168;
    public const ushort StandardSyncFirstPulseTStates = 667;
    public const ushort StandardSyncSecondPulseTStates = 735;
    public const ushort StandardZeroPulseTStates = 855;
    public const ushort StandardOnePulseTStates = 1710;
    public const ushort StandardHeaderPilotPulseCount = 8063;
    public const ushort StandardDataPilotPulseCount = 3223;
    public const byte FullLastByteBitCount = 8;
    public const ushort StandardPauseMilliseconds = 1000;

    public const string BlockIdMetadataKey = "tzxBlockId";
    public const string DecoderId = "tzx-signal";
    public const string EncoderId = "tzx-signal";
}
