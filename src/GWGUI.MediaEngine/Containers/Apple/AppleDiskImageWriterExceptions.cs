namespace GWGUI.MediaEngine.Containers.Apple;

/// <summary>Construit les erreurs de la façade d'écriture Apple.</summary>
internal static class AppleDiskImageWriterExceptions
{
    /// <summary>Crée l'erreur signalant une extension de destination non prise en charge.</summary>
    public static NotSupportedException UnsupportedExtension(string extension) => new($"L'extension de sortie Apple '{extension}' n'est pas prise en charge ; utilisez NIB ou WOZ.");
}
