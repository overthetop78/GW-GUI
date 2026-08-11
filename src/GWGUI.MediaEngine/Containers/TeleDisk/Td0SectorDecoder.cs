namespace GWGUI.MediaEngine.Containers.TeleDisk;

internal static class Td0SectorDecoder
{
    public static byte[] Decode(ReadOnlySpan<byte> encoded, byte encoding, int expectedLength)
    {
        var output = new List<byte>(expectedLength);
        switch (encoding)
        {
            case 0:
                output.AddRange(encoded.ToArray());
                break;
            case 1:
                if (encoded.Length != 4) throw new InvalidDataException("A TeleDisk repeated sector has an invalid payload.");
                var repetitions = ReadUInt16(encoded, 0);
                for (var index = 0; index < repetitions; index++)
                {
                    output.Add(encoded[2]);
                    output.Add(encoded[3]);
                }
                break;
            case 2:
                for (var offset = 0; offset < encoded.Length;)
                {
                    if (offset + 2 > encoded.Length) throw new InvalidDataException("A TeleDisk RLE sector is truncated.");
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
                        var patternLength = patternWords * 2;
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

    private static ushort ReadUInt16(ReadOnlySpan<byte> data, int offset) => (ushort)(data[offset] | data[offset + 1] << 8);
}
