namespace GWGUI.MediaEngine.Containers.Apple;

/// <summary>Construit les erreurs propres au routage des conteneurs et représentations Apple.</summary>
internal static class AppleContainerExceptions
{
    /// <summary>Crée l'erreur signalant qu'aucun format Apple n'a validé le contenu.</summary>
    public static InvalidDataException NoValidatedFormat(string extension, string? requestedFormatId) => new($"No Apple format validated content with extension '{extension}' and requested format '{requestedFormatId ?? "none"}'.");

    /// <summary>Crée l'erreur signalant une variante Apple reconnue mais non prise en charge.</summary>
    public static NotSupportedException UnsupportedVariant(string format) => new($"Apple format variant '{format}' is not supported.");
}
