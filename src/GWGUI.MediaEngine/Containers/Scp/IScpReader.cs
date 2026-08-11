namespace GWGUI.MediaEngine.Containers.Scp;

/// <summary>
/// Définit le contrat de lecture d’une capture SuperCard Pro depuis un fichier.
/// </summary>
public interface IScpReader
{
    /// <summary>
    /// Lit et valide une capture SCP.
    /// </summary>
    /// <param name="path">Chemin du fichier SCP à lire.</param>
    /// <param name="cancellationToken">Jeton permettant d’annuler l’opération de lecture asynchrone.</param>
    /// <returns>L’image SCP analysée, comprenant son en-tête, ses pistes et ses révolutions.</returns>
    /// <exception cref="ArgumentException"><paramref name="path"/> est vide ou ne représente pas un chemin de fichier valide.</exception>
    /// <exception cref="FileNotFoundException">Le fichier désigné par <paramref name="path"/> n’existe pas.</exception>
    /// <exception cref="UnauthorizedAccessException">L’accès en lecture au fichier est refusé.</exception>
    /// <exception cref="IOException">Une erreur d’entrée-sortie survient pendant la lecture.</exception>
    /// <exception cref="InvalidDataException">Le fichier ne contient pas une capture SCP structurellement valide.</exception>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> demande l’annulation de l’opération.</exception>
    Task<ScpImage> ReadAsync(string path, CancellationToken cancellationToken = default);
}
