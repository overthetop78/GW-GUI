namespace GWGUI.MediaEngine.Containers.Scp;

/// <summary>Définit l'écriture d'une capture SuperCard Pro vers un fichier.</summary>
public interface IScpWriter
{
    /// <summary>Écrit l'image SCP vers la destination indiquée.</summary>
    Task WriteAsync(string path, ScpImage image, CancellationToken cancellationToken = default);
}
