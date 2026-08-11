namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Construit les descriptions techniques du format DEC RX02.</summary>
internal static class DecRx02Descriptions
{
    /// <summary>Décrit un en-tête tronqué.</summary>
    public static string TruncatedHeader() => FluxStructureDescriptions.Truncated(DecRx02Format.StructureDescriptionName, FluxStructureKind.FormatHeader, null, DecRx02Format.SectorHeaderDescription);
    /// <summary>Décrit un en-tête et les CRC associés.</summary>
    public static string Header(DecRx02Header header, DecRx02DataMarkDefinition? mark, bool? dataValid) => FluxStructureDescriptions.Complete(DecRx02Format.StructureDescriptionName, FluxStructureKind.FormatHeader, header.Cylinder, header.Head, header.Sector, mark?.SectorSize ?? 0, mark?.Mark, mark is null ? null : Encoding(mark), header.CrcValid, dataValid, DecRx02Format.HeaderCrcDescription, DecRx02Format.DataCrcDescription);
    /// <summary>Décrit un bloc de données.</summary>
    public static string Data(DecRx02Header header, DecRx02DataMarkDefinition mark, bool? valid) => FluxStructureDescriptions.WithIntegrity(DecRx02Format.StructureDescriptionName, FluxStructureKind.FormatData, header.Cylinder, header.Head, header.Sector, mark.SectorSize, mark.Mark, Encoding(mark), DecRx02Format.DataCrcDescription, valid);
    /// <summary>Décrit une marque de données non appariée.</summary>
    public static string UnpairedData(DecRx02DataMarkDefinition mark) => FluxStructureDescriptions.UnclassifiedMark(DecRx02Format.StructureDescriptionName, FluxStructureKind.FormatData, mark.Mark, DecRx02Format.UnpairedDataDescription);
    /// <summary>Retourne le nom de l'encodage associé à une marque.</summary>
    private static string Encoding(DecRx02DataMarkDefinition mark) => mark.Encoding == DecRx02DataEncoding.M2Fm ? DecRx02Format.M2FmEncodingName : DecRx02Format.FmEncodingName;
}

/// <summary>Représente un en-tête RX02 décodé.</summary>
/// <param name="Cylinder">Cylindre.</param><param name="Head">Face.</param><param name="Sector">Secteur.</param><param name="SizeCode">Code de taille.</param><param name="CrcValid">Validité du CRC.</param>
internal sealed record DecRx02Header(byte Cylinder, byte Head, byte Sector, byte SizeCode, bool CrcValid);
