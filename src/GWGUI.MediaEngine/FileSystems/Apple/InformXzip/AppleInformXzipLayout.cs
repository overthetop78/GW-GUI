namespace GWGUI.MediaEngine.FileSystems.Apple.InformXzip;

/// <summary>Définit la disposition Apple II Inform/XZIP et l'en-tête Z-machine reconnu.</summary>
public static class AppleInformXzipLayout
{
    /// <summary>Taille d'un secteur, en octets.</summary>
    public const int SectorSize = 256;
    /// <summary>Nombre de secteurs occupés par l'interpréteur.</summary>
    public const int InterpreterSectorCount = 64;
    /// <summary>Nombre maximal de secteurs occupés par l'histoire.</summary>
    public const int MaximumStorySectorCount = 394;
    /// <summary>Nombre de secteurs par piste.</summary>
    public const int SectorsPerTrack = 16;
    /// <summary>Nombre de pistes.</summary>
    public const int TrackCount = 35;
    /// <summary>Version Z-machine reconnue.</summary>
    public const byte ZMachineVersion = 5;
    /// <summary>Taille minimale de l'en-tête Z-machine.</summary>
    public const int MinimumHeaderLength = 64;
    /// <summary>Offset de la version.</summary>
    public const int VersionOffset = 0;
    /// <summary>Offset de high memory.</summary>
    public const int HighMemoryOffset = 0x04;
    /// <summary>Offset du compteur ordinal initial.</summary>
    public const int InitialProgramCounterOffset = 0x06;
    /// <summary>Offset du dictionnaire.</summary>
    public const int DictionaryOffset = 0x08;
    /// <summary>Offset de la table d'objets.</summary>
    public const int ObjectsOffset = 0x0a;
    /// <summary>Offset des variables globales.</summary>
    public const int GlobalsOffset = 0x0c;
    /// <summary>Offset de la mémoire statique.</summary>
    public const int StaticMemoryOffset = 0x0e;
    /// <summary>Offset de la longueur déclarée.</summary>
    public const int LengthOffset = 0x1a;
    /// <summary>Offset du checksum déclaré.</summary>
    public const int ChecksumOffset = 0x1c;
    /// <summary>Multiplicateur de longueur de la version 5.</summary>
    public const int LengthUnit = 4;
    /// <summary>Premier octet couvert par le checksum.</summary>
    public const int ChecksumDataOffset = 0x40;
    /// <summary>Masque du numéro de secteur dans une piste.</summary>
    public const int SectorInTrackMask = 0x0f;
    /// <summary>Masque conservant les pistes du numéro de secteur d'histoire.</summary>
    public const int StoryTrackMask = 0xff0;
    /// <summary>Ordre d'entrelacement exact des seize secteurs.</summary>
    public static IReadOnlyList<int> Interleave { get; } = Array.AsReadOnly(new[] { 0, 7, 14, 6, 13, 5, 12, 4, 11, 3, 10, 2, 9, 1, 8, 15 });
    private static readonly IReadOnlyList<int> InverseInterleave = BuildInverseInterleave();

    /// <summary>Retourne la position stockée d'un secteur logique dans la table d'entrelacement.</summary>
    public static int StoredSectorIndex(int logicalSectorWithinTrack)
    {
        if (logicalSectorWithinTrack < 0 || logicalSectorWithinTrack >= InverseInterleave.Count) throw new ArgumentOutOfRangeException(nameof(logicalSectorWithinTrack));
        return InverseInterleave[logicalSectorWithinTrack];
    }

    /// <summary>Calcule le nombre de secteurs nécessaires pour une longueur en octets.</summary>
    public static int RequiredStorySectors(int byteLength) => checked((byteLength + SectorSize - 1) / SectorSize);

    private static IReadOnlyList<int> BuildInverseInterleave()
    {
        var inverse = new int[Interleave.Count];
        for (var index = 0; index < Interleave.Count; index++) inverse[Interleave[index]] = index;
        return Array.AsReadOnly(inverse);
    }
}
