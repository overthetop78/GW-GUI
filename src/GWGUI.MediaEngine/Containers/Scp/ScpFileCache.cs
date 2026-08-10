using System.Collections.Concurrent;

namespace GWGUI.MediaEngine.Containers.Scp;

/// <summary>
/// Met en cache les images SCP en distinguant chaque version physique d'un fichier.
/// </summary>
internal sealed class ScpFileCache
{
    /// <summary>
    /// Associe chaque version physique d'un fichier à son chargement partagé.
    /// </summary>
    private readonly ConcurrentDictionary<FileIdentity, Lazy<Task<ScpImage>>> _entries = new();

    /// <summary>
    /// Retourne l'image déjà chargée ou appelle le chargeur pour la version actuelle du fichier.
    /// </summary>
    /// <param name="path">Chemin du fichier SCP à identifier.</param>
    /// <param name="loader">Fonction chargeant et analysant le fichier lorsqu'il n'est pas en cache.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler l'attente sans interrompre un chargement partagé.</param>
    /// <returns>Image associée à la taille et à la date de modification actuelles du fichier.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="loader"/> est nul.</exception>
    public async Task<ScpImage> GetOrAddAsync(string path, Func<string, Task<ScpImage>> loader, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(loader);
        var file = new FileInfo(path);
        var identity = new FileIdentity(file.FullName, file.Length, file.LastWriteTimeUtc.Ticks);
        foreach (var obsolete in _entries.Keys.Where(key => key.Path.Equals(identity.Path, StringComparison.OrdinalIgnoreCase) && key != identity)) _entries.TryRemove(obsolete, out _);
        var pending = _entries.GetOrAdd(identity, key => new Lazy<Task<ScpImage>>(() => loader(key.Path), LazyThreadSafetyMode.ExecutionAndPublication));
        try { return await pending.Value.WaitAsync(cancellationToken).ConfigureAwait(false); }
        catch
        {
            _entries.TryRemove(identity, out _);
            throw;
        }
    }

    /// <summary>
    /// Identifie une version précise d'un fichier.
    /// </summary>
    /// <param name="Path">Chemin complet normalisé du fichier.</param>
    /// <param name="Length">Taille du fichier, en octets.</param>
    /// <param name="LastWriteTicks">Date de dernière modification UTC, exprimée en graduations de <see cref="DateTime"/>.</param>
    private readonly record struct FileIdentity(string Path, long Length, long LastWriteTicks);
}
