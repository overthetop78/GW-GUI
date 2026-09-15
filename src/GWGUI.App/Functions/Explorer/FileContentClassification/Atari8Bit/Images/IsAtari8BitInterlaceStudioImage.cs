using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private const int InterlaceStudioFrameLength = 8000;
    private const int InterlaceStudioFirstFrameOffset = 16;
    private const int InterlaceStudioSecondFrameOffset = 0x2010;
    private const int InterlaceStudioPaletteOffset = 0x4000;
    private const int InterlaceStudioScanlineCount = 200;
    private const int InterlaceStudioPaletteCount = 4;
    private const int InterlaceStudioLength = InterlaceStudioPaletteOffset
        + InterlaceStudioScanlineCount * InterlaceStudioPaletteCount;

    private static bool IsAtari8BitInterlaceStudioImage(FileSystemEntry entry)
    {
        if (!System.IO.Path.GetExtension(entry.Name).Equals(".ist", StringComparison.OrdinalIgnoreCase)
            || entry.Content is not { Count: InterlaceStudioLength } data)
            return false;

        var firstFrame = data.Skip(InterlaceStudioFirstFrameOffset).Take(InterlaceStudioFrameLength);
        var secondFrame = data.Skip(InterlaceStudioSecondFrameOffset).Take(InterlaceStudioFrameLength);
        var palette = data.Skip(InterlaceStudioPaletteOffset);
        return firstFrame.Distinct().Skip(1).Any()
            && secondFrame.Distinct().Skip(1).Any()
            && palette.Distinct().Skip(1).Any();
    }
}
