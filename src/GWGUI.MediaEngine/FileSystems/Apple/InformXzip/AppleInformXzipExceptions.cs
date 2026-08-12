namespace GWGUI.MediaEngine.FileSystems.Apple.InformXzip;

/// <summary>Construit les erreurs propres aux images Inform/XZIP.</summary>
internal static class AppleInformXzipExceptions
{
    /// <summary>Crée l'erreur signalant une disposition non reconnue.</summary>
    public static InvalidDataException UnsupportedLayout(int version, int availableLength) => new($"The image does not contain a supported Apple II Inform/XZIP layout: version {version}, {availableLength} bytes available.");
    /// <summary>Crée l'erreur signalant un secteur logique absent.</summary>
    public static InvalidDataException MissingSector(int logicalSector) => new($"Apple II sector {logicalSector} is missing.");
    /// <summary>Crée l'erreur signalant une longueur d'histoire incohérente.</summary>
    public static InvalidDataException InconsistentStoryLength(int declaredLength, int availableLength) => new($"The Z-machine story declares {declaredLength} bytes but {availableLength} bytes are available.");
    /// <summary>Crée l'erreur signalant un checksum d'histoire invalide.</summary>
    public static InvalidDataException InvalidChecksum(int length) => new($"The Z-machine story checksum is invalid for its declared length of {length} bytes.");
}
