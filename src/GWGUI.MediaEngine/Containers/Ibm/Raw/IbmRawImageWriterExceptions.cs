namespace GWGUI.MediaEngine.Containers.Ibm.Raw;

/// <summary>Construit les erreurs propres au Writer brut IBM.</summary>
public static class IbmRawImageWriterExceptions
{
    /// <summary>Signale un profil cible non pris en charge.</summary>
    public static InvalidDataException UnsupportedFormat(string formatId) => new($"Disk-image format '{formatId}' is not a supported explicit IBM raw-image profile.");

    /// <summary>Signale une source incompatible avec la géométrie cible.</summary>
    public static InvalidDataException GeometryMismatch(string sourceFormatId, string targetFormatId) => new($"IBM sector image '{sourceFormatId}' cannot be written as '{targetFormatId}' because their geometries differ.");
}
