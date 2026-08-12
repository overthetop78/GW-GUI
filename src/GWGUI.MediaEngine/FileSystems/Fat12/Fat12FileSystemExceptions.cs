namespace GWGUI.MediaEngine.FileSystems.Fat12;

/// <summary>Construit les diagnostics propres aux volumes FAT12.</summary>
public static class Fat12FileSystemExceptions
{
    /// <summary>Crée l'erreur signalant une disposition non prise en charge.</summary>
    public static InvalidDataException UnsupportedLayout(string formatId, IReadOnlyList<byte>? boot)
    {
        if (boot is null || boot.Count < FatBootSectorLayout.MinimumLength) return new($"Le format d'image '{formatId}' ne contient pas de système FAT12 pris en charge ; son secteur d'amorçage est absent ou tronqué.");
        var bytes = boot.ToArray();
        var sectorSize = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(FatBootSectorLayout.BytesPerSectorOffset));
        var sectorsPerCluster = bytes[FatBootSectorLayout.SectorsPerClusterOffset];
        var reserved = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(FatBootSectorLayout.ReservedSectorCountOffset));
        var fatCount = bytes[FatBootSectorLayout.FatCountOffset];
        var rootEntries = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(FatBootSectorLayout.RootEntryCountOffset));
        var fatSectors = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(FatBootSectorLayout.SectorsPerFatOffset));
        return new($"Le format d'image '{formatId}' ne contient pas de système FAT12 pris en charge (secteur {sectorSize}, secteurs/cluster {sectorsPerCluster}, réservés {reserved}, FAT {fatCount}, entrées racine {rootEntries}, secteurs/FAT {fatSectors}).");
    }
    /// <summary>Crée l'erreur signalant une chaîne invalide ou cyclique.</summary>
    public static InvalidDataException InvalidChain(string name, int cluster) => new($"La chaîne FAT de '{name}' est invalide ou cyclique au cluster {cluster}.");
    /// <summary>Crée l'erreur signalant une entrée FAT tronquée.</summary>
    public static InvalidDataException TruncatedTable(int cluster) => new($"L'entrée FAT12 du cluster {cluster} est tronquée.");
    /// <summary>Construit l'avertissement signalant des secteurs absents.</summary>
    public static string MissingSectors(int firstSector, int count) => $"La plage de secteurs {firstSector}..{firstSector + count - 1} est incomplète.";
    /// <summary>Construit l'avertissement signalant la limite de profondeur.</summary>
    public static string DepthLimit(string path, int depth) => $"La limite d'imbrication des répertoires FAT est atteinte à '{path}' (profondeur {depth}).";
    /// <summary>Construit l'avertissement d'un secteur absent ou mal dimensionné.</summary>
    public static string MissingSector(int sector, int observedSize, int expectedSize) => $"Le secteur FAT {sector} contient {observedSize} octet(s) au lieu de {expectedSize}.";
    /// <summary>Construit l'avertissement d'un cycle de clusters.</summary>
    public static string CyclicChain(string name, int cluster) => $"{name}: la chaîne FAT12 est cyclique au cluster {cluster}.";
    /// <summary>Construit l'avertissement d'un cluster hors plage.</summary>
    public static string ClusterOutsideRange(string name, int cluster) => $"{name}: le cluster FAT12 {cluster} est hors de la plage de données.";
    /// <summary>Construit l'avertissement d'un contenu plus court que la taille déclarée.</summary>
    public static string IncompleteContent(string name, long declaredSize, long availableSize) => $"{name}: la taille déclarée est {declaredSize} octets mais seulement {availableSize} sont valides.";
}
