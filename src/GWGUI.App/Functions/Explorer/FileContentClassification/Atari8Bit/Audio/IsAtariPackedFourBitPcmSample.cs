using System.IO;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    /// <summary>
    /// Recognizes headerless Atari sample streams containing two unsigned 4-bit PCM values per byte.
    /// The player consumes the high nibble first, then the low nibble, through POKEY volume-only output.
    /// </summary>
    private static bool IsAtariPackedFourBitPcmSample(FileSystemEntry entry)
    {
        var extension = Path.GetExtension(entry.Name);
        if (extension.Length < 4
            || !extension.StartsWith(".SP", StringComparison.OrdinalIgnoreCase)
            || !extension.AsSpan(3).ToArray().All(char.IsAsciiDigit)
            || entry.Content is not { Count: >= 8192 } data)
            return false;

        Span<long> levelCounts = stackalloc long[16];
        Span<bool> byteValues = stackalloc bool[256];
        var distinctByteCount = 0;
        var previousSample = -1;
        long sampleSum = 0;
        long absoluteLevelDelta = 0;

        foreach (var value in data)
        {
            if (!byteValues[value])
            {
                byteValues[value] = true;
                distinctByteCount++;
            }

            var highSample = value >> 4;
            levelCounts[highSample]++;
            sampleSum += highSample;
            if (previousSample >= 0) absoluteLevelDelta += Math.Abs(highSample - previousSample);
            previousSample = highSample;

            var lowSample = value & 0x0f;
            levelCounts[lowSample]++;
            sampleSum += lowSample;
            absoluteLevelDelta += Math.Abs(lowSample - previousSample);
            previousSample = lowSample;
        }

        var sampleCount = checked((long)data.Count * 2);
        var transitionCount = sampleCount - 1;
        return distinctByteCount >= 128
            && levelCounts.ToArray().All(count => count >= sampleCount / 2000)
            && sampleSum >= sampleCount * 6
            && sampleSum <= sampleCount * 9
            && absoluteLevelDelta <= transitionCount * 3;
    }
}
