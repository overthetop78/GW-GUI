using GWGUI.MediaEngine.Images.Models.Sectors;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Inspection;
using GWGUI.MediaEngine.Images.Models.Flux;
using FileSystemRegistry = GWGUI.MediaFileSystems.Exploration.SectorFileSystemRegistry;

namespace GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Decoding.Sectors;

/// <summary>Exécute une sélection explicite ou les candidats SCP par défaut en conservant leurs diagnostics.</summary>
internal sealed class ScpSectorImageReader(ScpCandidateRegistry candidates, FileSystemRegistry fileSystems)
{
    /// <summary>Lit directement le candidat explicite, sinon parcourt les candidats par défaut dans leur ordre.</summary>
    public Task<SectorImage> ReadAsync(string path, string? formatId, CancellationToken cancellationToken) =>
        ReadAsync(path, formatId, null, cancellationToken);

    public async Task<SectorImage> ReadAsync(
        string path,
        string? formatId,
        IProgress<ScpExplorationProgress>? progress,
        CancellationToken cancellationToken)
    {
        var selected = candidates.Selected(formatId);
        if (selected is not null) return await selected.ReadWithProgressAsync(path, formatId, progress, cancellationToken).ConfigureAwait(false);
        return await ReadDefaultAsync(path, formatId, progress, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Lit une représentation de flux commune avec les candidats qui ne dépendent pas du conteneur SCP.</summary>
    public async Task<SectorImage> ReadAsync(
        ProtectedTrackImage image,
        string? formatId,
        IProgress<ScpExplorationProgress>? progress,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(image);
        var selected = candidates.Selected(formatId);
        if (selected is not null) return await selected.ReadWithProgressAsync(image, formatId, progress, cancellationToken).ConfigureAwait(false);
        SectorImage? firstDecoded = null;
        foreach (var candidate in candidates.Default().Where(candidate => candidate.FluxReadAsync is not null))
        {
            try
            {
                var decoded = await candidate.ReadWithProgressAsync(image, null, progress, cancellationToken).ConfigureAwait(false);
                firstDecoded ??= decoded;
                if (HasFileSystem(decoded)) return decoded;
            }
            catch (Exception exception) when (exception is InvalidDataException or NotSupportedException)
            {
            }
        }
        return firstDecoded ?? throw new InvalidDataException("No sector decoder accepted the flux image.");
    }

    /// <summary>Conserve le premier décodage, retourne la première reconnaissance de système de fichiers et poursuit après les rejets prévus.</summary>
    private async Task<SectorImage> ReadDefaultAsync(
        string path,
        string? formatId,
        IProgress<ScpExplorationProgress>? progress,
        CancellationToken cancellationToken)
    {
        SectorImage? firstDecoded = null;
        var failures = new List<ScpCandidateFailure>();
        foreach (var candidate in candidates.Default())
        {
            try
            {
                var image = await candidate.ReadWithProgressAsync(path, null, progress, cancellationToken).ConfigureAwait(false);
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
