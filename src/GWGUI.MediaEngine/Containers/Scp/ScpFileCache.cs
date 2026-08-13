using System.Collections.Concurrent;

namespace GWGUI.MediaEngine.Containers.Scp;

/// <summary>Met en cache les images SCP en distinguant chaque version physique d'un fichier.</summary>
internal sealed class ScpFileCache
{
    /// <summary>Associe chaque version physique d'un fichier à son chargement partagé.</summary>
    private readonly ConcurrentDictionary<FileIdentity, Lazy<Task<ScpImage>>> _entries = new();

    /// <summary>Associe immédiatement une image déjà chargée à la version actuelle du fichier.</summary>
    public void Remember(string path, ScpImage image)
    {
        ArgumentNullException.ThrowIfNull(image);
        var file = new FileInfo(path);
        var identity = new FileIdentity(file.FullName, file.Length, file.LastWriteTimeUtc.Ticks);
        foreach (var obsolete in _entries.Keys.Where(key => StringComparer.OrdinalIgnoreCase.Equals(key.Path, identity.Path) && !key.Equals(identity)))
        {
            _entries.TryRemove(obsolete, out _);
        }

        _entries[identity] = new Lazy<Task<ScpImage>>(
            () => Task.FromResult(image),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    /// <summary>Retourne l'image déjà chargée ou appelle le chargeur pour la version actuelle du fichier.</summary>
    /// <param name="path">Chemin du fichier SCP à identifier.</param>
    /// <param name="loader">Fonction chargeant et analysant le fichier lorsqu'il n'est pas en cache.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler l'attente sans interrompre un chargement partagé.</param>
    /// <returns>Image associée à la taille et à la date de modification actuelles du fichier.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="loader"/> est nul.</exception>
    /// <exception cref="ArgumentException"><paramref name="path"/> est vide ou ne représente pas un chemin valide.</exception>
    /// <exception cref="FileNotFoundException">Le fichier désigné par <paramref name="path"/> n’existe pas.</exception>
    /// <exception cref="UnauthorizedAccessException">L’identification du fichier est refusée.</exception>
    /// <exception cref="IOException">Une erreur d’entrée-sortie survient pendant l’identification du fichier.</exception>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> annule l’attente de cet appelant sans interrompre le chargement partagé.</exception>
    /// <remarks>Les exceptions produites par <paramref name="loader"/> sont propagées à l’appelant et retirent le chargement défaillant du cache.</remarks>
    public async Task<ScpImage> GetOrAddAsync(string path, Func<string, Task<ScpImage>> loader, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(loader);
        var file = new FileInfo(path);
        var identity = new FileIdentity(file.FullName, file.Length, file.LastWriteTimeUtc.Ticks);
        foreach (var obsolete in _entries.Keys.Where(key => StringComparer.OrdinalIgnoreCase.Equals(key.Path, identity.Path) && !key.Equals(identity)))
        {
            _entries.TryRemove(obsolete, out _);
        }

        var pending = _entries.GetOrAdd(identity, key => new Lazy<Task<ScpImage>>(() => loader(key.Path), LazyThreadSafetyMode.ExecutionAndPublication));
        var loadTask = pending.Value;
        try
        {
            return await loadTask.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested && !loadTask.IsCanceled)
        {
            throw;
        }
        catch
        {
            _entries.TryRemove(new KeyValuePair<FileIdentity, Lazy<Task<ScpImage>>>(identity, pending));
            throw;
        }
    }

    /// <summary>Identifie une version précise d'un fichier avec une comparaison de chemin insensible à la casse.</summary>
    private readonly struct FileIdentity : IEquatable<FileIdentity>
    {
        /// <summary>Initialise l’identité physique d’un fichier.</summary>
        /// <param name="path">Chemin complet normalisé du fichier.</param>
        /// <param name="length">Taille du fichier, en octets.</param>
        /// <param name="lastWriteTicks">Date de dernière modification UTC, exprimée en graduations de <see cref="DateTime"/>.</param>
        public FileIdentity(string path, long length, long lastWriteTicks) => (Path, Length, LastWriteTicks) = (path, length, lastWriteTicks);

        /// <summary>Obtient le chemin complet normalisé du fichier.</summary>
        public string Path { get; }

        /// <summary>Obtient la taille du fichier, en octets.</summary>
        private long Length { get; }

        /// <summary>Obtient la date de dernière modification UTC en graduations de <see cref="DateTime"/>.</summary>
        private long LastWriteTicks { get; }

        /// <summary>Indique si deux identités désignent la même version physique d’un chemin.</summary>
        /// <param name="other">Identité à comparer.</param>
        /// <returns><see langword="true"/> lorsque chemin, taille et date correspondent.</returns>
        public bool Equals(FileIdentity other) => StringComparer.OrdinalIgnoreCase.Equals(Path, other.Path) && Length == other.Length && LastWriteTicks == other.LastWriteTicks;

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is FileIdentity other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(StringComparer.OrdinalIgnoreCase.GetHashCode(Path), Length, LastWriteTicks);
    }
}
