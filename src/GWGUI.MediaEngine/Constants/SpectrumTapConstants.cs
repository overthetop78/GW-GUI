namespace GWGUI.MediaEngine.Constants;

/// <summary>Invariant Spectrum TAP structure, flags, and standard signal values.</summary>
internal static class SpectrumTapConstants
{
    public const int LengthFieldSize = 2;
    public const byte HeaderFlag = 0x00;
    public const byte DataFlag = 0xff;
    public const int HeaderBlockLength = 19;
    public const ushort PilotPulseTStates = TzxConstants.StandardPilotPulseTStates;
    public const ushort SyncFirstPulseTStates = TzxConstants.StandardSyncFirstPulseTStates;
    public const ushort SyncSecondPulseTStates = TzxConstants.StandardSyncSecondPulseTStates;
    public const ushort ZeroPulseTStates = TzxConstants.StandardZeroPulseTStates;
    public const ushort OnePulseTStates = TzxConstants.StandardOnePulseTStates;
    public const ushort HeaderPilotPulseCount = TzxConstants.StandardHeaderPilotPulseCount;
    public const ushort DataPilotPulseCount = TzxConstants.StandardDataPilotPulseCount;
    public const int MinimumDetectedPilotPulseCount = 256;
    public const double PulseToleranceRatio = 0.35;
    public const int PcmBitsPerSample = 16;
    public const int PcmChannelCount = 1;
    public const short PcmAmplitude = 24576;
    public const string BlockMetadataKey = "spectrumTapBlock";
    public const string DecoderId = "spectrum-tape";
    public const string EncoderId = "spectrum-tape";
}
