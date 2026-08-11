using System.Buffers.Binary;

namespace GWGUI.MediaEngine.Containers.Atari.Msa;

internal static class MsaRleDecoder
{
    public static byte[] Unpack(ReadOnlySpan<byte> packed, int expected, int cylinder, int head)
    {
        var output = new byte[expected];
        var input = 0;
        var written = 0;
        while (input < packed.Length && written < output.Length)
        {
            if (packed[input] != MsaFormat.RleMarker)
            {
                output[written++] = packed[input++];
                continue;
            }
            if (input + MsaLayout.RleSequenceSize > packed.Length) throw MsaExceptions.TruncatedRun(cylinder, head, input, packed.Length);
            var value = packed[input + MsaLayout.RleValueOffset];
            var count = BinaryPrimitives.ReadUInt16BigEndian(packed[(input + MsaLayout.RleCountOffset)..]);
            input += MsaLayout.RleSequenceSize;
            if (count == 0 || written + count > output.Length) throw MsaExceptions.InvalidRun(cylinder, head, input - MsaLayout.RleSequenceSize, count, written, expected);
            output.AsSpan(written, count).Fill(value);
            written += count;
        }
        if (input != packed.Length || written != expected) throw MsaExceptions.InvalidUnpackedLength(cylinder, head, input, packed.Length, written, expected);
        return output;
    }
}
