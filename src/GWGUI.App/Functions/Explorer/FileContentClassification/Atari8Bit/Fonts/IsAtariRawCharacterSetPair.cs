namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariRawCharacterSetPair(IReadOnlyList<byte>? data)
    {
        const int bankLength = 1024;
        const int glyphLength = 8;
        const int glyphCount = bankLength / glyphLength;

        if (data is not { Count: bankLength * 2 }) return false;

        var commonBlankGlyphs = 0;
        for (var bank = 0; bank < 2; bank++)
        {
            var uniqueGlyphs = new HashSet<ulong>();
            var blankGlyphs = 0;
            for (var glyph = 0; glyph < glyphCount; glyph++)
            {
                ulong bitmap = 0;
                var offset = bank * bankLength + glyph * glyphLength;
                for (var row = 0; row < glyphLength; row++)
                    bitmap = bitmap << 8 | data[offset + row];

                uniqueGlyphs.Add(bitmap);
                if (bitmap is 0 or ulong.MaxValue) blankGlyphs++;

                if (bank == 0 && bitmap is 0 or ulong.MaxValue)
                {
                    ulong pairedBitmap = 0;
                    var pairedOffset = bankLength + glyph * glyphLength;
                    for (var row = 0; row < glyphLength; row++)
                        pairedBitmap = pairedBitmap << 8 | data[pairedOffset + row];
                    if (pairedBitmap is 0 or ulong.MaxValue) commonBlankGlyphs++;
                }
            }

            if (blankGlyphs is < 1 or > 64 || uniqueGlyphs.Count is < 32 or > 120)
                return false;
        }

        return commonBlankGlyphs >= 4;
    }
}
