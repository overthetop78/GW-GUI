using System.IO;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    /// <summary>
    /// Recognizes Atari Hard Interlace Pictures made of equal Graphics 9 and Graphics 10 planes.
    /// HIP stores either the planes directly, optionally followed by nine palette bytes, or prefixes
    /// each plane with an Atari executable segment header.
    /// </summary>
    private static bool IsAtariHardInterlacePicture(FileSystemEntry entry)
    {
        if (!string.Equals(Path.GetExtension(entry.Name), ".hip", StringComparison.OrdinalIgnoreCase)
            || entry.Content is not { Count: >= 80 } data)
            return false;

        return IsRaw(data) || IsSegmented(data);
    }

    private static bool IsRaw(IReadOnlyList<byte> data)
    {
        var paletteLength = data.Count % 80 == 9 ? 9 : 0;
        var pixelLength = data.Count - paletteLength;
        if (pixelLength % 80 != 0) return false;

        var height = pixelLength / 80;
        if (height is < 1 or > 240) return false;

        var frameLength = height * 40;
        return data.Take(frameLength).Distinct().Take(2).Count() == 2
            && data.Skip(frameLength).Take(frameLength).Distinct().Take(2).Count() == 2;
    }

    private static bool IsSegmented(IReadOnlyList<byte> data)
    {
        if (data.Count < 92 || ReadUInt16(data, 0) != 0xffff) return false;

        var firstStart = ReadUInt16(data, 2);
        var firstEnd = ReadUInt16(data, 4);
        var frameLength = firstEnd - firstStart + 1;
        if (frameLength <= 0 || frameLength % 40 != 0) return false;

        var height = frameLength / 40;
        var secondHeaderOffset = 6 + frameLength;
        if (height > 240 || data.Count != 12 + frameLength * 2
            || ReadUInt16(data, secondHeaderOffset) != 0xffff)
            return false;

        var secondStart = ReadUInt16(data, secondHeaderOffset + 2);
        var secondEnd = ReadUInt16(data, secondHeaderOffset + 4);
        return secondEnd - secondStart + 1 == frameLength
            && data.Skip(6).Take(frameLength).Distinct().Take(2).Count() == 2
            && data.Skip(secondHeaderOffset + 6).Take(frameLength).Distinct().Take(2).Count() == 2;
    }
}
