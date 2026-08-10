namespace GWGUI.MediaEngine.Recognition;

/// <summary>Conserve les informations et le contenu partagés pendant la reconnaissance d'une image de média.</summary>
public sealed class DiskImageRecognitionContext
{
    /// <summary>Contenu du fichier chargé lors de la première demande.</summary>
    private byte[]? bytes;

    /// <summary>Crée le contexte associé à un fichier et au format éventuellement demandé.</summary>
    /// <param name="path">Chemin du fichier à reconnaître.</param>
    /// <param name="requestedFormatId">Identifiant de format explicitement demandé, ou <see langword="null"/>.</param>
    public DiskImageRecognitionContext(string path, string? requestedFormatId)
    {
        Path = path;
        Length = new FileInfo(path).Length;
        Extension = System.IO.Path.GetExtension(path).ToLowerInvariant();
        RequestedFormatId = requestedFormatId;
    }

    /// <summary>Obtient le chemin reçu lors de la création du contexte.</summary>
    public string Path { get; }

    /// <summary>Obtient la longueur du fichier en octets.</summary>
    public long Length { get; }

    /// <summary>Obtient l'extension normalisée en minuscules, point initial inclus.</summary>
    public string Extension { get; }

    /// <summary>Obtient l'identifiant de format demandé, ou <see langword="null"/> en détection automatique.</summary>
    public string? RequestedFormatId { get; }

    /// <summary>Lit une seule fois le contenu complet du fichier puis réutilise les mêmes octets.</summary>
    /// <param name="cancellationToken">Jeton permettant d'annuler la première lecture.</param>
    /// <returns>Tableau partagé contenant exactement les octets du fichier.</returns>
    /// <exception cref="OperationCanceledException">Le jeton est annulé avant ou pendant la première lecture.</exception>
    /// <exception cref="IOException">Une erreur d'entrée-sortie empêche la lecture.</exception>
    /// <exception cref="UnauthorizedAccessException">L'accès au fichier est refusé.</exception>
    public async Task<byte[]> ReadBytesAsync(CancellationToken cancellationToken = default)
    {
        bytes ??= await File.ReadAllBytesAsync(Path, cancellationToken).ConfigureAwait(false);
        return bytes;
    }
}
