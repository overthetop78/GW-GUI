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
    /// <exception cref="InvalidDataException">Le fichier ne contient pas une capture SCP structurellement valide.</exception>
    /// <exception cref="OperationCanceledException">L’opération a été annulée.</exception>
    Task<ScpImage> ReadAsync(string path, CancellationToken cancellationToken = default);
}
