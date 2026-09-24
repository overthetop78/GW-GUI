using System.Collections.Frozen;
using GWGUI.MediaEngine.Constants;

namespace GWGUI.MediaEngine.Images.Formats.Tape.Wav;

/// <summary>Declares the bounded integer PCM RIFF/WAVE profile used as sequential tape samples.</summary>
internal static class WavTapeFormat
{
    public static string FormatId => TapeImageFormatIds.Wav;

    public static readonly IReadOnlySet<string> Extensions =
        new[] { DiskImageFileExtensions.Wav }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public static bool CanRead => true;

    public static bool CanWrite => true;

    public static bool IsSupportedPcm(int channels, int sampleRate, int bitsPerSample) =>
        channels is >= WavConstants.MinimumChannelCount and <= WavConstants.MaximumChannelCount
        && sampleRate is >= WavConstants.MinimumSampleRate and <= WavConstants.MaximumSampleRate
        && WavConstants.SupportedBitsPerSample.Contains((ushort)bitsPerSample);
}
