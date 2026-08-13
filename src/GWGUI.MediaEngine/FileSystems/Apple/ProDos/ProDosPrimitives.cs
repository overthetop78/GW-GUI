using System.Buffers.Binary;

namespace GWGUI.MediaEngine.FileSystems.Apple.ProDos;

/// <summary>Lit les primitives binaires et textuelles ProDOS.</summary>
internal static class ProDosPrimitives
{
    /// <summary>Lit un entier 16 bits little-endian.</summary>
    public static ushort ReadUInt16(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(offset, sizeof(ushort)));

    /// <summary>Lit un entier non signé de 24 bits little-endian.</summary>
    public static int ReadUInt24(ReadOnlySpan<byte> data, int offset) => data[offset] | data[offset + 1] << ProDosFileSystemLayout.BitsPerByte | data[offset + 2] << (ProDosFileSystemLayout.BitsPerByte * 2);

    /// <summary>Écrit un entier 16 bits little-endian.</summary>
    public static void WriteUInt16(Span<byte> data, int offset, int value) => BinaryPrimitives.WriteUInt16LittleEndian(data.Slice(offset, sizeof(ushort)), checked((ushort)value));

    /// <summary>Écrit un entier non signé de 24 bits little-endian.</summary>
    public static void WriteUInt24(Span<byte> data, int offset, int value)
    {
        if (value is < 0 or > ProDosFileSystemLayout.MaximumFileLength) throw new ArgumentOutOfRangeException(nameof(value));
        data[offset] = (byte)value;
        data[offset + 1] = (byte)(value >> ProDosFileSystemLayout.BitsPerByte);
        data[offset + 2] = (byte)(value >> (ProDosFileSystemLayout.BitsPerByte * 2));
    }

    /// <summary>Écrit un pointeur dans les deux moitiés d'un bloc d'index.</summary>
    public static void WriteIndexPointer(Span<byte> block, int index, int value)
    {
        block[index] = (byte)value;
        block[index + ProDosFileSystemLayout.IndexHighBytesOffset] = (byte)(value >> ProDosFileSystemLayout.BitsPerByte);
    }

    /// <summary>Lit un nom ASCII dont la longueur est contenue dans l'octet de stockage.</summary>
    public static string ReadName(ReadOnlySpan<byte> data, int offset)
    {
        var length = data[offset] & ProDosFileSystemLayout.NameLengthMask;
        return System.Text.Encoding.ASCII.GetString(data.Slice(offset + ProDosFileSystemLayout.NameOffset, length));
    }

    /// <summary>Lit un pointeur dont les octets bas et hauts occupent deux moitiés du bloc d'index.</summary>
    public static int ReadIndexPointer(IReadOnlyList<byte> block, int index) => block[index] | block[index + ProDosFileSystemLayout.IndexHighBytesOffset] << ProDosFileSystemLayout.BitsPerByte;
}
