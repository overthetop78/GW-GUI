namespace GWGUI.MediaEngine.Exploration;

/// <summary>Construit les erreurs produites par la façade publique d'exploration.</summary>
internal static class DiskImageExplorationExceptions
{
    /// <summary>Crée l'erreur signalant que le chemin demandé n'existe pas.</summary>
    public static FileNotFoundException MissingImage(string path) => new($"L'image de média '{path}' n'existe pas.", path);
}
