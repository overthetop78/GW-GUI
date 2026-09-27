namespace GWGUI.MediaEngine.Images.Formats.Tape.Amstrad;

/// <summary>Documented CPC firmware cassette record layout and standard timing values.</summary>
internal static class AmstradCpcTapeConstants
{
    internal const string DecoderId = "amstrad-cpc-tape";
    internal const string EncoderId = "amstrad-cpc-tape";

    internal const byte HeaderSync = 0x2c;
    internal const byte DataSync = 0x16;
    internal const int HeaderLength = 64;
    internal const int SegmentDataLength = 256;
    internal const int SegmentCrcLength = 2;
    internal const int TrailerByteCount = 8;
    internal const byte TrailerByte = 0xff;

    internal const int FileNameOffset = 0;
    internal const int FileNameLength = 16;
    internal const int BlockNumberOffset = 16;
    internal const int LastBlockOffset = 17;
    internal const int FileTypeOffset = 18;
    internal const int DataLengthOffset = 19;
    internal const int DataLocationOffset = 21;
    internal const int FirstBlockOffset = 23;
    internal const int LogicalLengthOffset = 24;
    internal const int EntryAddressOffset = 26;

    internal const ushort CrcPolynomial = 0x1021;
    internal const ushort CrcSeed = 0xffff;

    // CDT stores timings in 3.5 MHz T-states, including for the 4 MHz CPC.
    internal const ushort OnePulseTStates = 1162;
    internal const ushort ZeroPulseTStates = 581;
    internal const ushort PilotPulseCount = 4096;
    internal const ushort PauseMilliseconds = 10;
    internal const byte UsedBitsInLastByte = 8;
    internal const int PcmBitsPerSample = 16;
    internal const short PcmAmplitude = 24576;
    internal const int MinimumPilotPulseCount = 2048;
    internal const double PulseToleranceRatio = 0.35;
}
