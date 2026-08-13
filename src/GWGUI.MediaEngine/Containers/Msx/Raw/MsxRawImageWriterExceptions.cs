namespace GWGUI.MediaEngine.Containers.Msx.Raw;

/// <summary>Construit les erreurs propres au Writer brut MSX.</summary>
public static class MsxRawImageWriterExceptions
{
    /// <summary>Signale un profil MSX inconnu.</summary>
    public static InvalidDataException UnsupportedFormat(string formatId) => new($"Disk-image format '{formatId}' is not a supported MSX raw-image profile.");

    /// <summary>Signale une géométrie source incompatible avec le profil cible.</summary>
    public static InvalidDataException GeometryMismatch(string sourceFormatId, string targetFormatId) => new($"MSX sector image '{sourceFormatId}' cannot be written as '{targetFormatId}' because their geometries differ.");
}
