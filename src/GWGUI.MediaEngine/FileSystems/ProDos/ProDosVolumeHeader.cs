namespace GWGUI.MediaEngine.FileSystems.ProDos;

/// <summary>Définit et valide l'en-tête de volume ProDOS partagé par le lecteur et le sondage brut.</summary>
internal static class ProDosVolumeHeader
{
    /// <summary>Numéro du bloc contenant l'en-tête de volume.</summary>
    public const int BlockNumber = 2;
    /// <summary>Taille d'un bloc ProDOS en octets.</summary>
    public const int BlockSize = 512;
    /// <summary>Décalage de l'octet combinant type de stockage et longueur du nom.</summary>
    public const int StorageAndNameLengthOffset = 4;
    /// <summary>Type de stockage d'un en-tête de volume.</summary>
    public const int VolumeStorageType = 0x0F;
    /// <summary>Longueur maximale du nom de volume.</summary>
    public const int MaximumNameLength = 15;
    /// <summary>Décalage de la longueur d'une entrée de répertoire.</summary>
    public const int EntryLengthOffset = 0x23;
    /// <summary>Longueur attendue d'une entrée de répertoire.</summary>
    public const int EntryLength = 0x27;
    /// <summary>Longueur minimale requise pour valider les champs utilisés.</summary>
    public const int MinimumLength = EntryLengthOffset + 1;

    /// <summary>Vérifie le type de stockage, le nom et la longueur des entrées de l'en-tête de volume.</summary>
    public static bool IsValid(ReadOnlySpan<byte> block)
    {
        if (block.Length < MinimumLength) return false;
        var header = block[StorageAndNameLengthOffset];
        return header >> 4 == VolumeStorageType && (header & 0x0F) is > 0 and <= MaximumNameLength && block[EntryLengthOffset] == EntryLength;
    }
}
