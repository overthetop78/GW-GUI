using System.Buffers.Binary;

namespace GWGUI.MediaEngine.FileSystems.Ucsd;

/// <summary>Lit les entiers UCSD selon l'ordre des octets détecté.</summary>
internal static class UcsdPrimitives
{
    /// <summary>Lit un entier 16 bits dans l'ordre demandé.</summary>
    public static ushort ReadUInt16(ReadOnlySpan<byte> data, int offset, UcsdByteOrder byteOrder) => byteOrder == UcsdByteOrder.LittleEndian ? BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(offset, sizeof(ushort))) : BinaryPrimitives.ReadUInt16BigEndian(data.Slice(offset, sizeof(ushort)));
}
