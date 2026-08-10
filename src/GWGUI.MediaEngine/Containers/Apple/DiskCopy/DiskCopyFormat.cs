namespace GWGUI.MediaEngine.Containers.Apple.DiskCopy;

/// <summary>Regroupe les marqueurs et valeurs conventionnelles propres au format Apple DiskCopy 4.2.</summary>
public static class DiskCopyFormat
{
    /// <summary>Valeur indiquant qu’aucun checksum exploitable n’est stocké dans l’en-tête.</summary>
    public const uint MissingChecksum = 0;

    /// <summary>Octets ASCII mémorisés du marqueur MacWorks PREBOOT.</summary>
    private static readonly byte[] PrebootMarkerBytes = [0x50, 0x52, 0x45, 0x42, 0x4F, 0x4F, 0x54];

    /// <summary>Obtient le marqueur binaire utilisé pour reconnaître une charge utile Lisa MacWorks.</summary>
    public static ReadOnlySpan<byte> PrebootMarker => PrebootMarkerBytes;
}
