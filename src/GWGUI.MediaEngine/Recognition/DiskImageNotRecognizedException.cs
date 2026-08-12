namespace GWGUI.MediaEngine.Recognition;

/// <summary>Signale qu'aucune politique de reconnaissance ne correspond au contenu fourni.</summary>
public sealed class DiskImageNotRecognizedException : NotSupportedException
{
    /// <summary>Crée l'erreur pour le chemin qui n'a été reconnu par aucune politique.</summary>
    public DiskImageNotRecognizedException(string message, string path) : base(message) => Path = path;

    /// <summary>Chemin du contenu non reconnu.</summary>
    public string Path { get; }
}
