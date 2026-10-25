namespace GWGUI.Emulation.Philips.Common.Machines.Common.Constants;

internal static class VoiceSampleConstants
{
    internal const string DirectoryName = "voice";
    internal const string FileNamePattern = "^E[489A-F][89A-F][0-9A-F]\\.WAV$";
    internal const int HeaderBytes = 44;
    internal const int WaveOffset = 8;
    internal const int FormatOffset = 12;
    internal const int FormatSizeOffset = 16;
    internal const int FormatTagOffset = 20;
    internal const int ChannelsOffset = 22;
    internal const int SampleRateOffset = 24;
    internal const int BitsOffset = 34;
    internal const int DataOffset = 36;
    internal const int DataSizeOffset = 40;
    internal const int PlainFormatBytes = 16;
    internal const int IntegerPcm = 1;
    internal const int FloatPcm = 3;
    internal const int BytePcmBits = 8;
    internal const int WordPcmBits = 16;
    internal const int FloatPcmBits = 32;
    internal static readonly byte[] RiffMagic = "RIFF"u8.ToArray();
    internal static readonly byte[] WaveMagic = "WAVE"u8.ToArray();
    internal static readonly byte[] FormatMagic = "fmt "u8.ToArray();
    internal static readonly byte[] DataMagic = "data"u8.ToArray();
}
