namespace GWGUI.MediaEngine.Containers.ImageDisk;

/// <summary>Définit la disponibilité, la compression, la suppression et l'intégrité d'un enregistrement sectoriel ImageDisk.</summary>
public enum ImdSectorRecordType : byte
{
    /// <summary>Données indisponibles.</summary>
    Unavailable = 0,
    /// <summary>Données normales non compressées.</summary>
    Normal = 1,
    /// <summary>Données normales compressées par répétition d'un octet.</summary>
    Compressed = 2,
    /// <summary>Données supprimées non compressées.</summary>
    Deleted = 3,
    /// <summary>Données supprimées compressées.</summary>
    CompressedDeleted = 4,
    /// <summary>Données normales non compressées avec erreur.</summary>
    NormalWithError = 5,
    /// <summary>Données normales compressées avec erreur.</summary>
    CompressedWithError = 6,
    /// <summary>Données supprimées non compressées avec erreur.</summary>
    DeletedWithError = 7,
    /// <summary>Données supprimées compressées avec erreur.</summary>
    CompressedDeletedWithError = 8
}

/// <summary>Interprète les propriétés d'un type d'enregistrement sectoriel ImageDisk.</summary>
internal static class ImdSectorRecordTypeExtensions
{
    /// <summary>Indique si une charge utile est disponible.</summary>
    public static bool HasData(this ImdSectorRecordType type) => type != ImdSectorRecordType.Unavailable;
    /// <summary>Indique si la charge utile est représentée par un octet répété.</summary>
    public static bool IsCompressed(this ImdSectorRecordType type) => type is ImdSectorRecordType.Compressed or ImdSectorRecordType.CompressedDeleted or ImdSectorRecordType.CompressedWithError or ImdSectorRecordType.CompressedDeletedWithError;
    /// <summary>Indique si ImageDisk ne signale aucune erreur sur le secteur.</summary>
    public static bool IsIntegrityValid(this ImdSectorRecordType type) => type is ImdSectorRecordType.Normal or ImdSectorRecordType.Compressed or ImdSectorRecordType.Deleted or ImdSectorRecordType.CompressedDeleted;
}
