namespace GWGUI.MediaEngine.Containers.ImageDisk;

/// <summary>Identifie une section binaire ImageDisk utilisée dans les diagnostics de troncature.</summary>
internal enum ImdSection
{
    /// <summary>En-tête de piste.</summary>
    TrackHeader,
    /// <summary>Carte des numéros de secteurs.</summary>
    SectorNumberMap,
    /// <summary>Carte optionnelle des cylindres.</summary>
    CylinderMap,
    /// <summary>Carte optionnelle des faces.</summary>
    HeadMap,
    /// <summary>Carte optionnelle des tailles sectorielles.</summary>
    SectorSizeMap,
    /// <summary>Type d'enregistrement sectoriel.</summary>
    SectorRecord,
    /// <summary>Octet répété d'un secteur compressé.</summary>
    CompressedValue,
    /// <summary>Charge utile non compressée d'un secteur.</summary>
    SectorData
}
