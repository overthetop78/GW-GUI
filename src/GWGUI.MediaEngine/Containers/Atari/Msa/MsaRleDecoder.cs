using System.Buffers.Binary;

namespace GWGUI.MediaEngine.Containers.Atari.Msa;

internal static class MsaRleDecoder
{
    public static byte[] Unpack(ReadOnlySpan<byte> packed, int expected)
    {
        var output = new byte[expected];
        var input = 0;
        var written = 0;
        while (input < packed.Length && written < output.Length)
        {
            if (packed[input] != 0xE5)
            {
                output[written++] = packed[input++];
                continue;
            }
            if (input + 4 > packed.Length) throw new InvalidDataException("An MSA compressed run is truncated.");
            var value = packed[input + 1];
            var count = BinaryPrimitives.ReadUInt16BigEndian(packed[(input + 2)..]);
            input += 4;
            if (count == 0 || written + count > output.Length) throw new InvalidDataException("An MSA compressed run exceeds its track.");
            output.AsSpan(written, count).Fill(value);
            written += count;
        }
        if (input != packed.Length || written != expected) throw new InvalidDataException("The decompressed MSA track has an invalid length.");
        return output;
    }
}
