using System.Buffers.Binary;
using GWGUI.MediaEngine.Geometries.Apple;

namespace GWGUI.MediaEngine.FileSystems.Apple.Lisa;

/// <summary>Définit les champs communs des en-têtes de volume Lisa Office System.</summary>
internal static class LisaVolumeHeader
{
    /// <summary>Capacité attendue d'une charge utile Lisa brute.</summary>
    public const int Capacity = MacintoshGcrGeometry.Capacity400K;
    /// <summary>Taille d'une page Lisa examinée par le sondage.</summary>
    public const int PageSize = MacintoshGcrGeometry.BlockSize;
    /// <summary>Longueur minimale d'une page candidate.</summary>
    public const int MinimumLength = 64;
    /// <summary>Décalage de la version sur deux octets.</summary>
    public const int VersionOffset = 0;
    /// <summary>Décalage de la longueur du nom.</summary>
    public const int NameLengthOffset = 12;
    /// <summary>Décalage du premier caractère du nom.</summary>
    public const int NameOffset = NameLengthOffset + 1;
    /// <summary>Longueur maximale du nom.</summary>
    public const int MaximumNameLength = 31;
    /// <summary>Première valeur ASCII imprimable acceptée.</summary>
    public const byte MinimumPrintableCharacter = 0x20;
    /// <summary>Dernière valeur ASCII imprimable acceptée.</summary>
    public const byte MaximumPrintableCharacter = 0x7E;
    /// <summary>Vérifie la version, la longueur et les caractères du nom d'une page candidate.</summary>
    public static bool IsValid(ReadOnlySpan<byte> page)
    {
        if (page.Length < MinimumLength) return false;
        var version = BinaryPrimitives.ReadUInt16BigEndian(page.Slice(VersionOffset, sizeof(ushort)));
        var nameLength = page[NameLengthOffset];
        return IsKnownVersion(version) && nameLength is > 0 and <= MaximumNameLength && NameOffset + nameLength <= page.Length && IsPrintableName(page.Slice(NameOffset, nameLength));
    }

    /// <summary>Indique si la version appartient aux trois variantes Lisa Office connues.</summary>
    public static bool IsKnownVersion(ushort version) => Enum.IsDefined((LisaCatalogVersion)version);

    /// <summary>Vérifie que chaque octet du nom appartient à la plage ASCII imprimable.</summary>
    public static bool IsPrintableName(ReadOnlySpan<byte> name)
    {
        foreach (var value in name) if (value is < MinimumPrintableCharacter or > MaximumPrintableCharacter) return false;
        return true;
    }

    /// <summary>Décode un nom Lisa en supprimant sa fin nulle et ses espaces périphériques.</summary>
    public static string DecodeName(ReadOnlySpan<byte> bytes)
    {
        var end = bytes.IndexOf((byte)0);
        if (end >= 0) bytes = bytes[..end];
        return System.Text.Encoding.Latin1.GetString(bytes).Trim(' ', '\0');
    }
}
