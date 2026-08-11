using System.Buffers.Binary;

namespace GWGUI.MediaEngine.Containers.TeleDisk;

internal static class Td0SectorDecoder
{
    public static byte[] Decode(ReadOnlySpan<byte> encoded, Td0SectorEncoding encoding, int expectedLength, int cylinder, int head, int sector)
    {
        var output = new List<byte>(expectedLength);
        switch (encoding)
        {
            case Td0SectorEncoding.Raw:
                output.AddRange(encoded.ToArray());
                break;
            case Td0SectorEncoding.RepeatedWord:
                if (encoded.Length != Td0Layout.RepeatedSectorPayloadSize) throw Td0Exceptions.InvalidRepeatedPayload(cylinder, head, sector, encoded.Length, Td0Layout.RepeatedSectorPayloadSize);
                var repetitions = ReadUInt16(encoded, Td0Layout.RepeatedSectorCountOffset);
                for (var index = 0; index < repetitions; index++)
                {
                    output.Add(encoded[Td0Layout.RepeatedSectorPatternOffset]);
                    output.Add(encoded[Td0Layout.RepeatedSectorSecondPatternByteOffset]);
                }
                break;
            case Td0SectorEncoding.Rle:
                for (var offset = 0; offset < encoded.Length;)
                {
                    if (offset + Td0Layout.RleControlSize > encoded.Length) throw Td0Exceptions.TruncatedEncoding(cylinder, head, sector, encoding, offset, Td0Layout.RleControlSize, encoded.Length - offset);
                    var patternWords = encoded[offset++];
                    var count = encoded[offset++];
                    if (patternWords == 0)
                    {
                        if (offset + count > encoded.Length) throw Td0Exceptions.TruncatedEncoding(cylinder, head, sector, encoding, offset, count, encoded.Length - offset);
                        output.AddRange(encoded.Slice(offset, count).ToArray());
                        offset += count;
                    }
                    else
                    {
                        var patternLength = patternWords * Td0Layout.PatternWordSize;
                        if (offset + patternLength > encoded.Length) throw Td0Exceptions.TruncatedEncoding(cylinder, head, sector, encoding, offset, patternLength, encoded.Length - offset);
                        var pattern = encoded.Slice(offset, patternLength).ToArray();
                        offset += patternLength;
                        for (var repeat = 0; repeat < count; repeat++) output.AddRange(pattern);
                    }
                }
                break;
            default:
                throw Td0Exceptions.UnsupportedEncoding(cylinder, head, sector, encoding);
        }

        if (output.Count != expectedLength) throw Td0Exceptions.InvalidDecodedLength(cylinder, head, sector, encoding, output.Count, expectedLength);
        return output.ToArray();
    }

    private static ushort ReadUInt16(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(data[offset..]);
}
