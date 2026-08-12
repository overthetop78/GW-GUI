namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Regroupe les définitions binaires nécessaires au décodage du format MFM AED 6200P.</summary>
internal static class Aed6200pMfmFormat
{
    /// <summary>Obtient la marque d'en-tête MFM encodée, exprimée en octets bruts.</summary>
    public static IReadOnlyList<byte> HeaderPattern { get; } = Array.AsReadOnly<byte>([0x50, 0x94]);
    /// <summary>Obtient les quatre marques de données MFM encodées, exprimées en octets bruts.</summary>
    public static IReadOnlyList<IReadOnlyList<byte>> DataPatterns { get; } = Array.AsReadOnly<IReadOnlyList<byte>>([Array.AsReadOnly<byte>([0x50, 0x8a]), Array.AsReadOnly<byte>([0x50, 0x89]), Array.AsReadOnly<byte>([0x50, 0x84]), Array.AsReadOnly<byte>([0x50, 0x85])]);
    /// <summary>Obtient les marques de données avec leur motif physique et leur état supprimé.</summary>
    public static IReadOnlyList<Aed6200pDataMarkDefinition> DataMarks { get; } = Array.AsReadOnly<Aed6200pDataMarkDefinition>([new(0xc0, DataPatterns[0], true), new(0xc1, DataPatterns[1], false), new(0xc2, DataPatterns[2], false), new(0xc3, DataPatterns[3], false)]);
    /// <summary>Nombre d'octets décodés composant un en-tête avec son CRC.</summary>
    public const int HeaderByteCount = 7;
    /// <summary>Position de la marque dans l'en-tête décodé, en octets.</summary>
    public const int HeaderMarkOffset = 0;
    /// <summary>Position du cylindre dans l'en-tête décodé, en octets.</summary>
    public const int CylinderOffset = 1;
    /// <summary>Position de l'octet bas de la taille dans l'en-tête décodé.</summary>
    public const int SizeLowOffset = 2;
    /// <summary>Position du numéro de secteur dans l'en-tête décodé, en octets.</summary>
    public const int SectorOffset = 3;
    /// <summary>Position de l'octet haut de la taille dans l'en-tête décodé.</summary>
    public const int SizeHighOffset = 4;
    /// <summary>Marque d'en-tête après décodage MFM.</summary>
    public const byte HeaderAddressMark = 0xc6;
    /// <summary>Première marque de données après décodage MFM.</summary>
    public const byte FirstDataAddressMark = 0xc0;
    /// <summary>Dernière marque de données après décodage MFM.</summary>
    public const byte LastDataAddressMark = 0xc3;
    /// <summary>Étendue maximale de recherche d'une marque de données après un en-tête, en octets bruts.</summary>
    public const int DataSearchWindowByteCount = 104;
    /// <summary>Nombre d'octets décodés occupés par une marque de données.</summary>
    public const int DataMarkByteCount = 1;
    /// <summary>Nombre d'octets décodés occupés par un CRC.</summary>
    public const int CrcByteCount = 2;
    /// <summary>Nombre de bits du gap séparant l'en-tête des données.</summary>
    public const int FirstGapBitCount = 64;
    /// <summary>Nombre de bits du gap suivant les données.</summary>
    public const int SecondGapBitCount = 128;
    /// <summary>Marque de données normale utilisée par l'encodeur.</summary>
    public const byte DataMark = 0xc3;
    /// <summary>Marque de données supprimées utilisée par l'encodeur.</summary>
    public const byte DeletedDataMark = 0xc0;
    /// <summary>Valeur maximale du cylindre stocké sur un octet.</summary>
    public const int MaximumCylinder = byte.MaxValue;
    /// <summary>Valeur maximale du secteur stocké sur un octet.</summary>
    public const int MaximumSector = byte.MaxValue;
    /// <summary>Taille maximale stockée sur deux octets, en octets.</summary>
    public const int MaximumSectorByteCount = ushort.MaxValue;
}

/// <summary>Associe une marque de données AED à son motif physique et à son état supprimé.</summary>
/// <param name="Mark">Octet de marque décodé.</param>
/// <param name="Pattern">Motif physique compacté.</param>
/// <param name="Deleted">Indique une marque de données supprimées.</param>
internal sealed record Aed6200pDataMarkDefinition(byte Mark, IReadOnlyList<byte> Pattern, bool Deleted);
