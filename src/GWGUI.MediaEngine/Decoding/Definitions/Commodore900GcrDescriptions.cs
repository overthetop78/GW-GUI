namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Construit les descriptions techniques du format Commodore 900 GCR.</summary>
internal static class Commodore900GcrDescriptions
{
    /// <summary>Décrit une synchronisation GCR.</summary>
    /// <returns>Description technique.</returns>
    public static string Sync() => FluxStructureDescriptions.UnclassifiedMark(Commodore900GcrFormat.StructureDescriptionName, FluxStructureKind.CommodoreSync, null, Commodore900GcrFormat.SyncDescription);
    /// <summary>Décrit un en-tête et son checksum.</summary>
    /// <param name="cylinder">Cylindre.</param><param name="sector">Secteur.</param><param name="valid">Validité du checksum.</param><returns>Description technique.</returns>
    public static string Header(byte cylinder, byte sector, bool valid) => FluxStructureDescriptions.WithIntegrity(Commodore900GcrFormat.StructureDescriptionName, FluxStructureKind.CommodoreHeader, cylinder, Commodore900GcrFormat.LogicalHead, sector, Commodore900GcrFormat.SectorByteCount, null, null, Commodore900GcrFormat.ChecksumDescription, valid);
    /// <summary>Décrit un bloc de données apparié et son checksum.</summary>
    /// <param name="cylinder">Cylindre.</param><param name="sector">Secteur.</param><param name="valid">Validité du checksum.</param><returns>Description technique.</returns>
    public static string Data(byte cylinder, byte sector, bool valid) => FluxStructureDescriptions.WithIntegrity(Commodore900GcrFormat.StructureDescriptionName, FluxStructureKind.FormatData, cylinder, Commodore900GcrFormat.LogicalHead, sector, Commodore900GcrFormat.SectorByteCount, null, null, Commodore900GcrFormat.ChecksumDescription, valid);
    /// <summary>Décrit un bloc de données sans en-tête associé.</summary>
    /// <param name="valid">Validité du checksum.</param><returns>Description technique.</returns>
    public static string UnpairedData(bool valid) => $"{FluxStructureDescriptions.UnpairedData(Commodore900GcrFormat.StructureDescriptionName, null, Commodore900GcrFormat.UnpairedDataDescription)}, {FluxStructureDescriptions.Integrity(Commodore900GcrFormat.ChecksumDescription, valid)}";
}
