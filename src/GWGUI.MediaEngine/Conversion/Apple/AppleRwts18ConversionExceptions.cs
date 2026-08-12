namespace GWGUI.MediaEngine.Conversion.Apple;

/// <summary>Construit les erreurs propres à la conversion RWTS18.</summary>
internal static class AppleRwts18ConversionExceptions
{
    /// <summary>Crée l'erreur signalant que la source reconnue n'est pas RWTS18.</summary>
    public static InvalidDataException InvalidSource(string path, string observedFormat) => new($"La source '{path}' a été reconnue comme '{observedFormat}' au lieu d'Apple II RWTS18.");
    /// <summary>Crée l'erreur signalant une sortie non prise en charge.</summary>
    public static NotSupportedException UnsupportedOutput(string path, string extension) => new($"La sortie RWTS18 '{path}' utilise l'extension non prise en charge '{extension}'.");
}
