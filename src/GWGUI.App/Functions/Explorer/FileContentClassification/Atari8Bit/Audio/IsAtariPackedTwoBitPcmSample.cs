using System.IO;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    /// <summary>
    /// Recognizes headerless Atari sample streams containing four unsigned 2-bit PCM values per byte.
    /// The logical extension narrows the headerless format; the signal checks reject uncorrelated binary data.
    /// </summary>
    private static bool IsAtariPackedTwoBitPcmSample(FileSystemEntry entry)
    {
        if (!string.Equals(Path.GetExtension(entry.Name), ".snd", StringComparison.OrdinalIgnoreCase)
            || entry.Content is not { Count: >= 8192 } data)
            return false;

        Span<long> levelCounts = stackalloc long[4];
        Span<bool> byteValues = stackalloc bool[256];
        var distinctByteCount = 0;
        var previousSample = -1;
        long repeatedSamples = 0;
        long absoluteLevelDelta = 0;

        foreach (var value in data)
        {
            if (!byteValues[value])
            {
                byteValues[value] = true;
                distinctByteCount++;
            }

            for (var shift = 6; shift >= 0; shift -= 2)
            {
                var sample = value >> shift & 0x03;
                levelCounts[sample]++;
                if (previousSample >= 0)
                {
                    if (sample == previousSample) repeatedSamples++;
                    absoluteLevelDelta += Math.Abs(sample - previousSample);
                }
                previousSample = sample;
            }
        }

        var sampleCount = checked((long)data.Count * 4);
        var transitionCount = sampleCount - 1;
        return distinctByteCount >= 128
            && levelCounts.ToArray().All(count => count >= sampleCount / 20)
            && repeatedSamples * 100 >= transitionCount * 35
            && absoluteLevelDelta <= transitionCount;
    }
}
