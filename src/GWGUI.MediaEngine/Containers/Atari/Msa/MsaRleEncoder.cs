using System.Buffers.Binary;

namespace GWGUI.MediaEngine.Containers.Atari.Msa;

/// <summary>Compresse une piste MSA avec l'encodage par répétitions du conteneur.</summary>
internal static class MsaRleEncoder
{
    /// <summary>Encode les répétitions utiles et protège chaque octet égal au marqueur RLE.</summary>
    public static byte[] Pack(ReadOnlySpan<byte> source)
    {
        using var output = new MemoryStream(source.Length);
        var count = new byte[sizeof(ushort)];
        var position = 0;
        while (position < source.Length)
        {
            var value = source[position];
            var runLength = 1;
            while (position + runLength < source.Length && source[position + runLength] == value && runLength < ushort.MaxValue) runLength++;
            if (value == MsaFormat.RleMarker || runLength > MsaLayout.RleSequenceSize)
            {
                output.WriteByte(MsaFormat.RleMarker);
                output.WriteByte(value);
                BinaryPrimitives.WriteUInt16BigEndian(count, checked((ushort)runLength));
                output.Write(count);
            }
            else output.Write(source.Slice(position, runLength));
            position += runLength;
        }
        return output.ToArray();
    }
}
