using System.Buffers.Binary;

namespace GWGUI.MediaEngine.FileSystems.Apple.Macintosh;

/// <summary>Fournit les primitives binaires et textuelles communes aux systèmes de fichiers Macintosh.</summary>
internal static class MacFileSystemPrimitives
{
    /// <summary>Lit un entier non signé de 16 bits encodé en ordre big-endian.</summary>
    public static ushort ReadUInt16(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt16BigEndian(data.Slice(offset, sizeof(ushort)));

    /// <summary>Lit un entier non signé de 32 bits encodé en ordre big-endian.</summary>
    public static uint ReadUInt32(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt32BigEndian(data.Slice(offset, sizeof(uint)));

    /// <summary>Décode une chaîne Pascal dont la longueur est limitée par le format appelant.</summary>
    public static string ReadPascalString(ReadOnlySpan<byte> data, int offset, int maximumLength)
    {
        var length = Math.Min(data[offset], maximumLength);
        return DecodeName(data.Slice(offset + 1, length));
    }

    /// <summary>Décode un nom Macintosh et remplace son séparateur historique par une barre oblique.</summary>
    public static string DecodeName(ReadOnlySpan<byte> value) => System.Text.Encoding.Latin1.GetString(value).Replace(':', '/');
}
