using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.MediaEngine.SectorImages.Scp;

/// <summary>Exécute une sélection explicite ou les candidats SCP par défaut en conservant leurs diagnostics.</summary>
internal sealed class ScpSectorImageReader(ScpCandidateRegistry candidates, FileSystemRegistry fileSystems)
{
    /// <summary>Lit directement le candidat explicite, sinon parcourt les candidats par défaut dans leur ordre.</summary>
    public async Task<SectorImage> ReadAsync(string path, string? formatId, CancellationToken cancellationToken)
    {
        var selected = candidates.Selected(formatId);
        if (selected is not null) return await selected.ReadAsync(path, formatId, cancellationToken).ConfigureAwait(false);
        return await ReadDefaultAsync(path, formatId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Conserve le premier décodage, retourne la première reconnaissance de système de fichiers et poursuit après les rejets prévus.</summary>
    private async Task<SectorImage> ReadDefaultAsync(string path, string? formatId, CancellationToken cancellationToken)
    {
        SectorImage? firstDecoded = null;
        var failures = new List<ScpCandidateFailure>();
        foreach (var candidate in candidates.Default())
        {
            try
            {
                var image = await candidate.ReadAsync(path, null, cancellationToken).ConfigureAwait(false);
                firstDecoded ??= image;
                if (HasFileSystem(image)) return image;
            }
            catch (Exception exception) when (exception is InvalidDataException or NotSupportedException)
            {
                failures.Add(new(candidate.Id, exception));
            }
        }
        return firstDecoded ?? throw ScpSectorImageExceptions.AllCandidatesRejected(path, formatId, failures);
    }

    /// <summary>Indique si au moins un Reader reconnaît l'image.</summary>
    private bool HasFileSystem(SectorImage image) => fileSystems.TryRead(image, null, out _);
}
