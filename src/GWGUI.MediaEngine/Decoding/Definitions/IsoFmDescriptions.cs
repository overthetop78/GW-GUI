namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Construit les descriptions techniques du format ISO FM.</summary>
internal static class IsoFmDescriptions
{
    /// <summary>Décrit un en-tête et ses CRC.</summary>
    public static string Header(IsoFmHeader header, bool? dataValid) => FluxStructureDescriptions.Complete(IsoFmFormat.StructureDescriptionName, FluxStructureKind.IdAddressMark, header.Cylinder, header.Head, header.Sector, header.Size, IsoFmFormat.IdAddressMark, $"N{header.SizeCode}", header.CrcValid, dataValid);
    /// <summary>Décrit un bloc de données et son CRC.</summary>
    public static string Data(IsoFmHeader header, IsoFmMarkDefinition mark, bool? valid) => FluxStructureDescriptions.WithIntegrity(IsoFmFormat.StructureDescriptionName, mark.Kind, header.Cylinder, header.Head, header.Sector, header.Size, mark.Mark, null, "CRC", valid);
    /// <summary>Décrit une marque non appariée.</summary>
    public static string Unclassified(IsoFmMarkDefinition mark) => FluxStructureDescriptions.UnclassifiedMark(IsoFmFormat.StructureDescriptionName, mark.Kind, mark.Mark, null);
}

/// <summary>Représente un en-tête ISO FM décodé.</summary>
internal sealed record IsoFmHeader(int Offset, byte Cylinder, byte Head, byte Sector, byte SizeCode, int Size, bool? CrcValid, byte[]? Bytes);
/// <summary>Représente une marque de données ISO FM.</summary>
internal sealed record IsoFmDataMark(int Offset, IsoFmMarkDefinition Definition);
