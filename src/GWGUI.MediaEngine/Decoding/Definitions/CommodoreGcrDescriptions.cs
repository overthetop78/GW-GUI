namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Construit les descriptions techniques du format Commodore GCR.</summary>
internal static class CommodoreGcrDescriptions
{
    /// <summary>Décrit une synchronisation GCR.</summary>
    /// <returns>Description technique.</returns>
    public static string Sync() => FluxStructureDescriptions.UnclassifiedMark(CommodoreGcrFormat.StructureDescriptionName, FluxStructureKind.CommodoreSync, null, CommodoreGcrFormat.SyncDescription);

    /// <summary>Décrit un bloc de données et son checksum.</summary>
    /// <param name="valid">Validité du checksum.</param>
    /// <returns>Description technique.</returns>
    public static string Data(bool? valid) => $"{FluxStructureDescriptions.Identity(CommodoreGcrFormat.StructureDescriptionName, FluxStructureKind.FormatData, 0, CommodoreGcrFormat.LogicalHead, 0, CommodoreGcrFormat.SectorByteCount, null, CommodoreGcrFormat.DataBlockDescription)}, {FluxStructureDescriptions.Integrity(CommodoreGcrFormat.DataChecksumDescription, valid)}";

    /// <summary>Décrit un en-tête et les checksums de l'en-tête et des données associées.</summary>
    /// <param name="track">Piste.</param>
    /// <param name="sector">Secteur.</param>
    /// <param name="headerValid">Validité du checksum d'en-tête.</param>
    /// <param name="dataValid">Validité du checksum des données.</param>
    /// <returns>Description technique.</returns>
    public static string Header(byte track, byte sector, bool? headerValid, bool? dataValid) => FluxStructureDescriptions.Complete(CommodoreGcrFormat.StructureDescriptionName, FluxStructureKind.CommodoreHeader, track, CommodoreGcrFormat.LogicalHead, sector, CommodoreGcrFormat.SectorByteCount, null, null, headerValid, dataValid, CommodoreGcrFormat.HeaderChecksumDescription, CommodoreGcrFormat.DataChecksumDescription);
}
