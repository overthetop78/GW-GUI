using System.Buffers.Binary;

namespace GWGUI.MediaEngine.FileSystems.Macintosh;

/// <summary>Définit les signatures des blocs maîtres Macintosh MFS et HFS.</summary>
internal static class MacintoshVolumeSignatures
{
    /// <summary>Numéro du bloc logique du bloc maître.</summary>
    public const int MasterDirectoryBlock = 2;
    /// <summary>Taille d'un bloc logique Macintosh en octets.</summary>
    public const int BlockSize = 512;
    /// <summary>Décalage en octets du bloc maître dans une image linéaire.</summary>
    public const int ByteOffset = MasterDirectoryBlock * BlockSize;
    /// <summary>Longueur d'une signature Macintosh en octets.</summary>
    public const int Length = sizeof(ushort);
    /// <summary>Signature du système de fichiers MFS.</summary>
    public const ushort Mfs = 0xD2D7;
    /// <summary>Signature du système de fichiers HFS.</summary>
    public const ushort Hfs = 0x4244;
    /// <summary>Longueur minimale d'une image contenant entièrement la signature du bloc maître.</summary>
    public const int MinimumImageLength = ByteOffset + Length;

    /// <summary>Lit la signature du bloc maître lorsqu'elle est entièrement disponible.</summary>
    public static bool TryRead(ReadOnlySpan<byte> data, out ushort signature)
    {
        if (data.Length < MinimumImageLength) { signature = 0; return false; }
        signature = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(ByteOffset, Length));
        return true;
    }

    /// <summary>Indique si la valeur désigne MFS ou HFS.</summary>
    public static bool IsSupported(ushort signature) => signature is Mfs or Hfs;
}
