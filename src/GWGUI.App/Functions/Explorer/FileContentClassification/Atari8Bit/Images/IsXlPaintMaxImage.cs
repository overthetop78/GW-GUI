namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsXlPaintMaxImage(IReadOnlyList<byte>? data)
    {
        const int compressedOffset = 1732;
        const int decodedLength = 15360;

        if (data is not { Count: >= compressedOffset }
            || data[0] != 'X'
            || data[1] != 'L'
            || data[2] != 'P'
            || data[3] != 'M')
            return false;

        var sourceOffset = compressedOffset;
        var decodedOffset = 0;
        while (decodedOffset < decodedLength)
        {
            if (sourceOffset >= data.Count) return false;

            var command = data[sourceOffset++];
            var repeated = command >= 128;
            var count = command & 0x7f;
            if (count >= 64)
            {
                if (sourceOffset >= data.Count) return false;
                count = (count - 64 << 8) | data[sourceOffset++];
            }

            if (count == 0) continue;
            if (count > decodedLength - decodedOffset) return false;

            if (repeated)
            {
                if (sourceOffset >= data.Count) return false;
                sourceOffset++;
            }
            else
            {
                if (count > data.Count - sourceOffset) return false;
                sourceOffset += count;
            }
            decodedOffset += count;
        }

        return true;
    }
}
