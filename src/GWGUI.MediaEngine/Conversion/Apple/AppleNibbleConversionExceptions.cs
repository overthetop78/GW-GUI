namespace GWGUI.MediaEngine.Conversion.Apple;

/// <summary>Construit les erreurs propres aux conversions Apple NIB et WOZ.</summary>
internal static class AppleNibbleConversionExceptions
{
    /// <summary>Crée l'erreur signalant que la source n'est pas représentable dans le conteneur demandé.</summary>
    public static InvalidDataException InvalidSource(string path, string observedFormat) => new($"La source '{path}' reconnue comme '{observedFormat}' n'est pas représentable dans un conteneur Apple NIB ou WOZ.");
    /// <summary>Crée l'erreur signalant une extension de sortie incompatible.</summary>
    public static NotSupportedException UnsupportedOutput(string path, string extension) => new($"La sortie Apple '{path}' utilise l'extension non prise en charge '{extension}'.");
}
