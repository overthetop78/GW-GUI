namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Construit les descriptions techniques du format Data General 2F.</summary>
internal static class DataGeneralFmDescriptions
{
    /// <summary>Décrit l'identité d'un en-tête.</summary>
    /// <param name="identity">Identité du secteur.</param>
    /// <returns>Description technique.</returns>
    public static string Header(DataGeneralSectorIdentity identity) => FluxStructureDescriptions.Identity(DataGeneralFmFormat.StructureDescriptionName, FluxStructureKind.FormatHeader, identity.Cylinder, identity.Head, identity.Sector, DataGeneralFmFormat.SectorSize, null, null);

    /// <summary>Décrit un bloc de données et son checksum.</summary>
    /// <param name="identity">Identité du secteur.</param>
    /// <param name="valid">Validité du checksum.</param>
    /// <returns>Description technique.</returns>
    public static string Data(DataGeneralSectorIdentity identity, bool valid) => FluxStructureDescriptions.WithIntegrity(DataGeneralFmFormat.StructureDescriptionName, FluxStructureKind.FormatData, identity.Cylinder, identity.Head, identity.Sector, DataGeneralFmFormat.SectorSize, null, null, DataGeneralFmFormat.ChecksumDescription, valid);
}

/// <summary>Représente l'identité d'un secteur Data General.</summary>
/// <param name="Cylinder">Cylindre.</param>
/// <param name="Head">Face.</param>
/// <param name="Sector">Secteur.</param>
internal sealed record DataGeneralSectorIdentity(byte Cylinder, byte Head, byte Sector);
