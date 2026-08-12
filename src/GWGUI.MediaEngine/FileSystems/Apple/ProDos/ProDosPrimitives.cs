using System.Buffers.Binary;

namespace GWGUI.MediaEngine.FileSystems.Apple.ProDos;

/// <summary>Lit les primitives binaires et textuelles ProDOS.</summary>
internal static class ProDosPrimitives
{
    /// <summary>Lit un entier 16 bits little-endian.</summary>
    public static ushort ReadUInt16(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(offset, sizeof(ushort)));

    /// <summary>Lit un entier non signé de 24 bits little-endian.</summary>
    public static int ReadUInt24(ReadOnlySpan<byte> data, int offset) => data[offset] | data[offset + 1] << ProDosFileSystemLayout.BitsPerByte | data[offset + 2] << (ProDosFileSystemLayout.BitsPerByte * 2);

    /// <summary>Lit un nom ASCII dont la longueur est contenue dans l'octet de stockage.</summary>
    public static string ReadName(ReadOnlySpan<byte> data, int offset)
    {
        var length = data[offset] & ProDosFileSystemLayout.NameLengthMask;
        return System.Text.Encoding.ASCII.GetString(data.Slice(offset + ProDosFileSystemLayout.NameOffset, length));
    }

    /// <summary>Lit un pointeur dont les octets bas et hauts occupent deux moitiés du bloc d'index.</summary>
    public static int ReadIndexPointer(IReadOnlyList<byte> block, int index) => block[index] | block[index + ProDosFileSystemLayout.IndexHighBytesOffset] << ProDosFileSystemLayout.BitsPerByte;
}
