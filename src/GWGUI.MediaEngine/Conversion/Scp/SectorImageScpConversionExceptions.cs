namespace GWGUI.MediaEngine.Conversion.Scp;

/// <summary>Construit les diagnostics de création SCP depuis une image sectorielle.</summary>
internal static class SectorImageScpConversionExceptions
{
    public static InvalidDataException MissingTrack(string formatId) => new($"No encodable track is available for sector image format {formatId}.");
}
