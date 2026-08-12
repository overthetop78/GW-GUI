namespace GWGUI.MediaEngine.FileSystems.Definitions;

/// <summary>Construit les avertissements communs aux lecteurs de systèmes de fichiers.</summary>
internal static class FileSystemWarningMessages
{
    /// <summary>Construit l'avertissement signalant l'échec de lecture d'une entrée.</summary>
    /// <param name="entryName">Nom de l'entrée concernée.</param>
    /// <param name="exception">Exception contenant le diagnostic d'origine.</param>
    /// <returns>Avertissement contenant le nom de l'entrée et le diagnostic d'origine.</returns>
    public static string EntryReadFailure(string entryName, Exception exception) => $"{entryName}: {exception.Message}";

    /// <summary>Construit l'avertissement signalant des blocs de données manquants.</summary>
    /// <param name="entryName">Nom de l'entrée concernée.</param>
    /// <returns>Avertissement contenant le nom de l'entrée.</returns>
    public static string MissingDataBlocks(string entryName) => $"{entryName}: one or more data blocks are missing.";
}
