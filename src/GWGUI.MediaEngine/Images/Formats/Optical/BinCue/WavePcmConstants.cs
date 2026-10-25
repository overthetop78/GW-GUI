namespace GWGUI.MediaEngine.Images.Formats.Optical.BinCue;

internal static class WavePcmConstants
{
    public const int RiffHeaderLength = 12;
    public const int ChunkHeaderLength = 8;
    public const int MinimumFormatChunkLength = 16;
    public const ushort PcmFormat = 1;
    public const ushort StereoChannels = 2;
    public const int SampleRate = 44_100;
    public const ushort BitsPerSample = 16;
    public const ushort BlockAlign = 4;
    public const int ByteRate = 176_400;
    public const int AudioSectorSize = 2_352;
    public const string RiffChunk = "RIFF";
    public const string WaveForm = "WAVE";
    public const string FormatChunk = "fmt ";
    public const string DataChunk = "data";
}
