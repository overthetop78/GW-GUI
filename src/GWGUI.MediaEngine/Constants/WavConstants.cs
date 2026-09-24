namespace GWGUI.MediaEngine.Constants;

/// <summary>Invariant RIFF/WAVE identifiers and PCM field dimensions.</summary>
internal static class WavConstants
{
    public const string RiffIdentifier = "RIFF";
    public const string WaveIdentifier = "WAVE";
    public const string FormatChunkIdentifier = "fmt ";
    public const string DataChunkIdentifier = "data";
    public const ushort PcmFormatTag = 1;
    public const int RiffHeaderSize = 12;
    public const int ChunkHeaderSize = 8;
    public const int PcmFormatChunkSize = 16;
    public const int MinimumChannelCount = 1;
    public const int MaximumChannelCount = 32;
    public const int MinimumSampleRate = 1;
    public const int MaximumSampleRate = 768_000;
    public static readonly ushort[] SupportedBitsPerSample = [8, 16, 24, 32];
}
