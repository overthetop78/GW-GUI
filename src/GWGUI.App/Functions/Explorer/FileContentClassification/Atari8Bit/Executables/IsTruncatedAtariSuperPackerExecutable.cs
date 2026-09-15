using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsTruncatedAtariSuperPackerExecutable(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: >= 7 } || ReadUInt16(data, 0) != 0xffff) return false;

        var offset = 2;
        var previousStart = 0;
        var previousDataOffset = 0;
        var previousLength = 0;
        while (offset < data.Count)
        {
            while (offset + 1 < data.Count && ReadUInt16(data, offset) == 0xffff) offset += 2;
            if (offset + 4 > data.Count) return false;

            var start = ReadUInt16(data, offset);
            var end = ReadUInt16(data, offset + 2);
            if (end < start) return false;

            var dataOffset = offset + 4;
            var length = end - start + 1;
            if (dataOffset + length > data.Count)
            {
                return dataOffset < data.Count
                    && previousLength == 471
                    && IsSuperPackerDepacker(data, previousDataOffset, previousStart);
            }

            previousStart = start;
            previousDataOffset = dataOffset;
            previousLength = length;
            offset = dataOffset + length;
        }

        return false;
    }

    private static bool IsSuperPackerDepacker(IReadOnlyList<byte> data, int offset, int start)
    {
        if (start < 25 || offset < 0 || offset + 471 > data.Count) return false;

        var buffer = start - 25;
        return Matches(data, offset, 0xa2, 0x13, 0xb5, 0x20, 0x48, 0xbd)
            && ReadUInt16(data, offset + 6) == buffer
            && Matches(data, offset + 8, 0x95, 0x20, 0xca, 0x10, 0xf5, 0x20)
            && ReadUInt16(data, offset + 14) == start + 0x89
            && Matches(data, offset + 16, 0x90, 0x03, 0x20)
            && ReadUInt16(data, offset + 19) == start + 0xcc
            && data[offset + 21] == 0x20
            && ReadUInt16(data, offset + 22) == start + 0x89
            && Matches(data, offset + 24, 0x90, 0x03, 0x20)
            && ReadUInt16(data, offset + 27) == start + 0x15e
            && data[offset + 29] == 0x20
            && ReadUInt16(data, offset + 30) == start + 0x89
            && Matches(data, offset + 32, 0x90, 0x03, 0x20)
            && ReadUInt16(data, offset + 35) == start + 0x1ad
            && Matches(data, offset + 37,
                0xa5, 0x2a, 0x45, 0x27, 0xd0, 0x0a, 0xaa, 0x68,
                0x95, 0x20, 0xe8, 0xe0, 0x14, 0x90, 0xf8, 0x60,
                0x18, 0x8e, 0x1a, 0xd0, 0xe8, 0x90, 0xfa, 0xa5,
                0x2b, 0xc5, 0x22, 0xd0, 0x15, 0xa5, 0x2c, 0xc5,
                0x23, 0xd0, 0x0f, 0xa4, 0x2d, 0xc0, 0x19, 0x90,
                0x03, 0xa6, 0x2e, 0x9a, 0xb9)
            && ReadUInt16(data, offset + 82) == buffer
            && Matches(data, offset + 430, 0x86, 0x2e, 0x20)
            && ReadUInt16(data, offset + 433) == start + 0x3c
            && Matches(data, offset + 435, 0x85, 0x3f, 0x20)
            && ReadUInt16(data, offset + 438) == start + 0x3c
            && Matches(data, offset + 440, 0xc5, 0x3f, 0xf0, 0x06, 0x20)
            && ReadUInt16(data, offset + 445) == start + 0x62
            && data[offset + 447] == 0x4c
            && ReadUInt16(data, offset + 448) == start + 0x1b5
            && data[offset + 450] == 0x20
            && ReadUInt16(data, offset + 451) == start + 0x3c
            && Matches(data, offset + 453, 0x85, 0x32, 0x20)
            && ReadUInt16(data, offset + 456) == start + 0x3c
            && Matches(data, offset + 458, 0x85, 0x33, 0xa5, 0x33, 0x20)
            && ReadUInt16(data, offset + 463) == start + 0x62
            && Matches(data, offset + 465, 0xc6, 0x32, 0xd0, 0xf7, 0xf0, 0xde);
    }

    private static bool Matches(IReadOnlyList<byte> data, int offset, params byte[] expected)
    {
        if (offset < 0 || offset + expected.Length > data.Count) return false;
        for (var index = 0; index < expected.Length; index++)
        {
            if (data[offset + index] != expected[index]) return false;
        }
        return true;
    }
}
