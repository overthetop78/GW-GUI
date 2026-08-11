namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Construit les descriptions techniques du format Heathkit FM.</summary>
internal static class HeathkitFmDescriptions
{
    /// <summary>Décrit un en-tête et les checksums associés.</summary>
    public static string Header(HeathkitHeader header, bool? dataValid) => FluxStructureDescriptions.Complete(HeathkitFmFormat.StructureDescriptionName, FluxStructureKind.FormatHeader, header.Cylinder, HeathkitFmFormat.LogicalHead, header.Sector, HeathkitFmFormat.SectorSize, null, $"volume {header.Volume}", header.ChecksumValid, dataValid, "header checksum", "data checksum");
    /// <summary>Décrit un bloc de données et son checksum.</summary>
    public static string Data(HeathkitHeader header, bool valid) => FluxStructureDescriptions.WithIntegrity(HeathkitFmFormat.StructureDescriptionName, FluxStructureKind.FormatData, header.Cylinder, HeathkitFmFormat.LogicalHead, header.Sector, HeathkitFmFormat.SectorSize, null, null, "checksum", valid);
    /// <summary>Décrit une marque tronquée.</summary>
    public static string TruncatedHeader() => FluxStructureDescriptions.Truncated(HeathkitFmFormat.StructureDescriptionName, FluxStructureKind.FormatHeader, null, "hard-sector header");
    /// <summary>Décrit une marque de données non appariée.</summary>
    public static string UnpairedData() => FluxStructureDescriptions.UnpairedData(HeathkitFmFormat.StructureDescriptionName, null, "data block");
}

/// <summary>Représente l'identité et le checksum d'un en-tête Heathkit.</summary>
internal sealed record HeathkitHeader(byte Volume, byte Cylinder, byte Sector, bool ChecksumValid);
