namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Construit les descriptions des en-têtes et blocs de données Centurion MFM.</summary>
internal static class CenturionMfmDescriptions
{
    /// <summary>Décrit un en-tête avec les états des CRC d'en-tête et de données.</summary>
    /// <param name="cylinder">Cylindre.</param><param name="sector">Secteur.</param><param name="size">Taille déclarée.</param><param name="headerValid">Validité du CRC d'en-tête.</param><param name="dataValid">Validité du CRC des données.</param><returns>Description complète.</returns>
    public static string Header(byte cylinder, byte sector, int size, bool headerValid, bool? dataValid) => FluxStructureDescriptions.Complete(CenturionMfmFormat.StructureDescriptionName, FluxStructureKind.FormatHeader, cylinder, CenturionMfmFormat.LogicalHead, sector, size, null, null, headerValid, dataValid);

    /// <summary>Décrit un bloc de données et son CRC.</summary>
    /// <param name="cylinder">Cylindre.</param><param name="sector">Secteur.</param><param name="size">Taille déclarée.</param><param name="key">Clé du bloc.</param><param name="valid">Validité du CRC.</param><returns>Description complète.</returns>
    public static string Data(byte cylinder, byte sector, int size, byte key, bool valid) => FluxStructureDescriptions.WithIntegrity(CenturionMfmFormat.StructureDescriptionName, FluxStructureKind.FormatData, cylinder, CenturionMfmFormat.LogicalHead, sector, size, null, $"key {key}", CenturionMfmFormat.CrcDescription, valid);

    /// <summary>Décrit un préfixe de données tronqué ou non pris en charge.</summary>
    /// <param name="key">Clé observée, ou valeur nulle si elle n'a pas pu être lue.</param><returns>Description du bloc incomplet.</returns>
    public static string TruncatedData(byte? key = null) => FluxStructureDescriptions.Truncated(CenturionMfmFormat.StructureDescriptionName, FluxStructureKind.FormatData, null, key is null ? CenturionMfmFormat.UnavailableCrcDescription : key == CenturionMfmFormat.SupportedDataKey ? CenturionMfmFormat.UnavailableCrcDescription : CenturionMfmFormat.UnsupportedKeyDescription(key.Value));

    /// <summary>Décrit une marque de données sans en-tête associé.</summary>
    /// <returns>Description de la marque non appariée.</returns>
    public static string UnpairedData() => FluxStructureDescriptions.UnpairedData(CenturionMfmFormat.StructureDescriptionName, null, CenturionMfmFormat.DataBlockDescription);
}
