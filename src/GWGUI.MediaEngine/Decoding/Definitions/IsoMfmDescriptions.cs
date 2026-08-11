namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Construit les descriptions techniques ISO MFM.</summary>
internal static class IsoMfmDescriptions
{
    /// <summary>Décrit un en-tête et ses CRC.</summary>
    public static string Header(IsoMfmHeader header, bool? dataValid) => FluxStructureDescriptions.Complete(IsoMfmFormat.StructureDescriptionName, FluxStructureKind.IdAddressMark, header.Cylinder, header.Head, header.Sector, header.Size, IsoMfmFormat.IdAddressMark, $"N{header.SizeCode}", header.CrcValid, dataValid);
    /// <summary>Décrit un bloc de données et son CRC.</summary>
    public static string Data(IsoMfmHeader header, IsoMfmDataMark mark, bool? valid) => FluxStructureDescriptions.WithIntegrity(IsoMfmFormat.StructureDescriptionName, mark.Definition.Kind, header.Cylinder, header.Head, header.Sector, header.Size, mark.Definition.Mark, null, "CRC", valid);
    /// <summary>Décrit une marque non appariée.</summary>
    public static string Unclassified(IsoMfmDataMark mark) => FluxStructureDescriptions.UnclassifiedMark(IsoMfmFormat.StructureDescriptionName, mark.Definition.Kind, mark.Definition.Mark, null);
}

/// <summary>Représente un en-tête ISO MFM.</summary>
internal sealed record IsoMfmHeader(int Offset, byte Cylinder, byte Head, byte Sector, byte SizeCode, int Size, bool? CrcValid, byte[]? Bytes);
/// <summary>Représente une marque de données ISO MFM.</summary>
internal sealed record IsoMfmDataMark(int Offset, IsoMfmMarkDefinition Definition);
