namespace GWGUI.MediaEngine.Recognition;

/// <summary>Conserve les informations et le contenu partagés pendant la reconnaissance d'une image de média.</summary>
public sealed class DiskImageRecognitionContext
{
    /// <summary>Protège la création unique de la tâche de lecture.</summary>
    private readonly object readLock = new();

    /// <summary>Tâche unique de lecture, conservée également lorsqu'elle est annulée ou en erreur.</summary>
    private Task<byte[]>? readTask;

    /// <summary>Crée le contexte associé à un fichier et au format éventuellement demandé.</summary>
    /// <param name="path">Chemin du fichier à reconnaître.</param>
    /// <param name="requestedFormatId">Identifiant de format explicitement demandé, ou <see langword="null"/>.</param>
    public DiskImageRecognitionContext(string path, string? requestedFormatId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        Path = path;
        Length = new FileInfo(path).Length;
        Extension = System.IO.Path.GetExtension(path).ToLowerInvariant();
        RequestedFormatId = requestedFormatId;
    }

    /// <summary>Obtient le chemin reçu lors de la création du contexte.</summary>
    public string Path { get; }

    /// <summary>Obtient la longueur du fichier observée lors de la création du contexte ; le Reader valide ensuite le contenu effectivement lu.</summary>
    public long Length { get; }

    /// <summary>Obtient l'extension normalisée en minuscules, point initial inclus.</summary>
    public string Extension { get; }

    /// <summary>Obtient l'identifiant de format demandé, ou <see langword="null"/> en détection automatique.</summary>
    public string? RequestedFormatId { get; }

    /// <summary>Crée une seule tâche de lecture puis réutilise son résultat, son annulation ou son erreur pour tous les appels.</summary>
    /// <param name="cancellationToken">Jeton appliqué uniquement lors de la création de l'unique tâche de lecture.</param>
    /// <returns>Tableau partagé contenant exactement les octets du fichier.</returns>
    /// <exception cref="OperationCanceledException">Le jeton est annulé avant ou pendant la première lecture.</exception>
    /// <exception cref="IOException">Une erreur d'entrée-sortie empêche la lecture.</exception>
    /// <exception cref="UnauthorizedAccessException">L'accès au fichier est refusé.</exception>
    public async Task<ReadOnlyMemory<byte>> ReadBytesAsync(CancellationToken cancellationToken = default)
    {
        Task<byte[]> task;
        lock (readLock) task = readTask ??= File.ReadAllBytesAsync(Path, cancellationToken);
        return await task.ConfigureAwait(false);
    }
}
