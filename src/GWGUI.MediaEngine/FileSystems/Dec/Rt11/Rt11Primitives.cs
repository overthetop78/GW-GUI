using System.Buffers.Binary;

namespace GWGUI.MediaEngine.FileSystems.Dec.Rt11;

/// <summary>Lit les primitives binaires et ASCII RT-11.</summary>
public static class Rt11Primitives
{
    /// <summary>Lit un entier 16 bits little-endian.</summary>
    public static ushort ReadUInt16(ReadOnlySpan<byte> source, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(source.Slice(offset, sizeof(ushort)));

    /// <summary>Décode une plage ASCII et retire les espaces et zéros terminaux.</summary>
    public static string DecodeAscii(ReadOnlySpan<byte> source) => System.Text.Encoding.ASCII.GetString(source).TrimEnd('\0', ' ');
}
