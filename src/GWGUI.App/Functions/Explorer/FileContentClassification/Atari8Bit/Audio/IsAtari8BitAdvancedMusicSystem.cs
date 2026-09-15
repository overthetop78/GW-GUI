using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtari8BitAdvancedMusicSystem(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: >= 64 }) return false;
        var values = new int[10];
        var offset = 0;
        for (var field = 0; field < values.Length; field++)
        {
            while (offset < data.Count && data[offset] == (byte)' ') offset++;

            var digits = 0;
            var value = 0;
            var decimalPoint = false;
            var fractionalDigits = 0;
            while (offset < data.Count && data[offset] != 0x9b)
            {
                if (field == 8 && data[offset] == (byte)'.' && digits > 0 && !decimalPoint)
                {
                    decimalPoint = true;
                    offset++;
                    continue;
                }

                if (data[offset] is not (>= (byte)'0' and <= (byte)'9') || digits == 6) return false;
                value = checked(value * 10 + data[offset] - (byte)'0');
                offset++;
                digits++;
                if (decimalPoint) fractionalDigits++;
            }
            if (digits == 0
                || decimalPoint && fractionalDigits == 0
                || offset >= data.Count
                || data[offset++] != 0x9b)
                return false;
            values[field] = value;
        }

        var measureCount = (long)values[0] + values[2] + values[4] + values[6];
        var expectedPayloadLength = 2L * values[7] + measureCount;
        return values[0] > 0
            && values[0] == values[2] && values[2] == values[4] && values[4] == values[6]
            && values[1] < values[3] && values[3] < values[5] && values[5] < values[7]
            && values[8] > 0
            && values[9] is > 0 and <= 255
            && data.Count - offset == expectedPayloadLength;
    }
}
