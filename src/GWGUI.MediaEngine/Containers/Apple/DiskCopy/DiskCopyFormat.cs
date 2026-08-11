namespace GWGUI.MediaEngine.Containers.Apple.DiskCopy;

/// <summary>Regroupe les marqueurs et valeurs conventionnelles propres au format Apple DiskCopy 4.2.</summary>
public static class DiskCopyFormat
{
    /// <summary>Mot magique big-endian stocké à la fin de l'en-tête DiskCopy 4.2.</summary>
    public const ushort PrivateWord = 0x0100;

    /// <summary>Valeur indiquant qu’aucun checksum exploitable n’est stocké dans l’en-tête.</summary>
    public const uint MissingChecksum = 0;
    /// <summary>Valeur initiale du registre de checksum avant le premier mot.</summary>
    public const uint InitialChecksum = 0;
    /// <summary>Taille d'un mot traité par le checksum DiskCopy, en octets.</summary>
    public const int ChecksumWordSize = sizeof(ushort);
    /// <summary>Nombre de bits de la rotation appliquée après chaque addition.</summary>
    public const int ChecksumRotation = 1;
    /// <summary>Nombre de bits du registre de checksum DiskCopy.</summary>
    public const int ChecksumBitCount = sizeof(uint) * 8;

    /// <summary>Octets ASCII mémorisés du marqueur MacWorks PREBOOT.</summary>
    private static readonly byte[] PrebootMarkerBytes = [0x50, 0x52, 0x45, 0x42, 0x4F, 0x4F, 0x54];

    /// <summary>Obtient le marqueur binaire utilisé pour reconnaître une charge utile Lisa MacWorks.</summary>
    public static ReadOnlySpan<byte> PrebootMarker => PrebootMarkerBytes;
}
