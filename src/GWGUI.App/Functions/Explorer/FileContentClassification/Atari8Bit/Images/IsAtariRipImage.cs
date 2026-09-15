using System.Buffers.Binary;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariRipImage(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: >= 34 }
            || data[0] != (byte)'R'
            || data[1] != (byte)'I'
            || data[2] != (byte)'P'
            || data[18] != (byte)'T'
            || data[19] != (byte)':')
        {
            return false;
        }

        var mode = data[7];
        if (mode is not (0x0e or 0x0f or 0x4f or 0x8f or 0xcf or 0x1e or 0x10 or 0x20 or 0x30))
            return false;

        var bytes = data as byte[] ?? data.ToArray();
        var headerLength = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(11, 2));
        var contentStride = data[13];
        var height = data[15];
        var textLength = data[17];
        var commentMarkerOffset = 21 + textLength;
        if (headerLength >= data.Count
            || contentStride is 0 or > 80
            || (contentStride & 1) != 0
            || height is 0 or > 239
            || commentMarkerOffset + 3 > headerLength
            || data[20 + textLength] != 9
            || data[commentMarkerOffset] != (byte)'C'
            || data[commentMarkerOffset + 1] != (byte)'M'
            || data[commentMarkerOffset + 2] != (byte)':')
        {
            return false;
        }

        for (var offset = 20; offset < 20 + textLength; offset++)
        {
            if (data[offset] is < 0x20 or > 0x7e)
                return false;
        }

        var unpackedStride = mode < 0x10 ? contentStride / 2 : contentStride;
        var unpackedLength = checked(unpackedStride * height);
        if (mode == 0x30)
            unpackedLength = checked(unpackedLength + ((height + 1) / 2) * 8);

        return data[9] switch
        {
            0 => headerLength + unpackedLength <= data.Count,
            1 => headerLength < data.Count,
            _ => false
        };
    }
}
