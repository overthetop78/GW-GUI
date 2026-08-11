using System.Buffers.Binary;

namespace GWGUI.MediaEngine.Containers.TeleDisk;

internal static class Td0SectorDecoder
{
    public static byte[] Decode(ReadOnlySpan<byte> encoded, Td0SectorEncoding encoding, int expectedLength)
    {
        var output = new List<byte>(expectedLength);
        switch (encoding)
        {
            case Td0SectorEncoding.Raw:
                output.AddRange(encoded.ToArray());
                break;
            case Td0SectorEncoding.RepeatedWord:
                if (encoded.Length != Td0Layout.RepeatedSectorPayloadSize) throw new InvalidDataException("A TeleDisk repeated sector has an invalid payload.");
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
                    if (offset + Td0Layout.RleControlSize > encoded.Length) throw new InvalidDataException("A TeleDisk RLE sector is truncated.");
                    var patternWords = encoded[offset++];
                    var count = encoded[offset++];
                    if (patternWords == 0)
                    {
                        if (offset + count > encoded.Length) throw new InvalidDataException("A TeleDisk literal run is truncated.");
                        output.AddRange(encoded.Slice(offset, count).ToArray());
                        offset += count;
                    }
                    else
                    {
                        var patternLength = patternWords * Td0Layout.PatternWordSize;
                        if (offset + patternLength > encoded.Length) throw new InvalidDataException("A TeleDisk repeated run is truncated.");
                        var pattern = encoded.Slice(offset, patternLength).ToArray();
                        offset += patternLength;
                        for (var repeat = 0; repeat < count; repeat++) output.AddRange(pattern);
                    }
                }
                break;
            default:
                throw new InvalidDataException($"TeleDisk sector encoding {encoding} is not supported.");
        }

        if (output.Count != expectedLength) throw new InvalidDataException($"A TeleDisk sector expands to {output.Count} bytes instead of {expectedLength}.");
        return output.ToArray();
    }

    private static ushort ReadUInt16(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(data[offset..]);
}
