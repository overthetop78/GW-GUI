namespace GWGUI.MediaEngine.Containers.Adf;

/// <summary>Construit les erreurs propres au Writer ADF Amiga.</summary>
public static class AmigaAdfWriterExceptions
{
    /// <summary>Signale un identifiant ADF Amiga inconnu.</summary>
    public static InvalidDataException UnsupportedFormat(string formatId) => new($"Disk-image format '{formatId}' is not an Amiga ADF DD or HD format.");

    /// <summary>Signale que la source ne correspond pas au format ADF demandé.</summary>
    public static InvalidDataException FormatMismatch(string sourceFormatId, string targetFormatId) => new($"Amiga sector image '{sourceFormatId}' cannot be written as '{targetFormatId}' without a geometry transformation.");
}
