namespace GWGUI.Scp.Containers.Apple.TwoImg;

/// <summary>Construit les erreurs produites pendant la validation et la lecture d’un conteneur 2IMG.</summary>
internal static class TwoImgExceptions
{
    /// <summary>Crée l’erreur signalant une signature 2IMG invalide.</summary>
    /// <returns>L’exception décrivant la signature invalide.</returns>
    public static InvalidDataException InvalidSignature() =>
        new("The 2IMG signature is invalid.");

    /// <summary>Crée l’erreur signalant une version 2IMG non prise en charge.</summary>
    /// <param name="version">Version lue dans l’en-tête du conteneur.</param>
    /// <returns>L’exception contenant la version rejetée.</returns>
    public static NotSupportedException UnsupportedVersion(ushort version) =>
        new($"The 2IMG version {version} is not supported.");

    /// <summary>Crée l’erreur signalant un en-tête 2IMG tronqué.</summary>
    /// <returns>L’exception décrivant l’en-tête incomplet.</returns>
    public static InvalidDataException TruncatedHeader() =>
        new("The 2IMG header is truncated.");

    /// <summary>Crée l’erreur signalant une plage de données 2IMG invalide.</summary>
    /// <returns>L’exception décrivant la plage de données invalide.</returns>
    public static InvalidDataException InvalidDataRange() =>
        new("The 2IMG data range is invalid.");

    /// <summary>Crée l’erreur signalant un format de charge utile 2IMG non pris en charge.</summary>
    /// <param name="imageFormat">Valeur du format lue dans l’en-tête.</param>
    /// <returns>L’exception contenant le format rejeté.</returns>
    public static NotSupportedException UnsupportedImageFormat(TwoImgImageFormat imageFormat) =>
        new($"The 2IMG image format {imageFormat} is not supported.");
}
