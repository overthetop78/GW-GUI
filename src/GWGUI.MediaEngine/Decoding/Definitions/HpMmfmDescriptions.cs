namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Construit les descriptions techniques du format HP MMFM.</summary>
internal static class HpMmfmDescriptions
{
    /// <summary>Décrit un en-tête et son CRC.</summary>
    public static string Header(HpMmfmHeader header) => FluxStructureDescriptions.WithIntegrity(HpMmfmFormat.StructureDescriptionName, FluxStructureKind.FormatHeader, header.Cylinder, header.Head, header.Sector, HpMmfmFormat.SectorSize, null, null, "header CRC", header.CrcValid);
    /// <summary>Décrit des données et leur CRC.</summary>
    public static string Data(HpMmfmHeader header, bool valid) => FluxStructureDescriptions.WithIntegrity(HpMmfmFormat.StructureDescriptionName, FluxStructureKind.FormatData, header.Cylinder, header.Head, header.Sector, HpMmfmFormat.SectorSize, null, null, "data CRC", valid);
}

/// <summary>Représente l'identité et le CRC d'un en-tête HP MMFM.</summary>
internal sealed record HpMmfmHeader(byte Cylinder, byte Head, byte Sector, bool CrcValid);
