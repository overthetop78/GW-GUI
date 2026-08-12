using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Recognition.Scp;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images.ScpDetection;

/// <summary>Sélectionne la première reconstruction SCP exploitable par un système de fichiers.</summary>
/// <param name="candidates">Registre des reconstructeurs candidats.</param>
/// <param name="fileSystems">Registre servant à valider les images reconstruites.</param>
internal sealed class ScpSectorImageReader(ScpCandidateRegistry candidates, FileSystemRegistry fileSystems)
{
    /// <summary>Reconstruit le format demandé ou essaie les candidats inscrits.</summary>
    public async Task<SectorImage> ReadAsync(string path, string? formatId, CancellationToken cancellationToken)
    {
        var selected = candidates.Selected(path, formatId, cancellationToken);
        if (selected is not null) return await selected().ConfigureAwait(false);

        SectorImage? firstDecoded = null;
        foreach (var read in candidates.Default(path, cancellationToken))
        {
            try
            {
                var candidate = await read().ConfigureAwait(false);
                firstDecoded ??= candidate;
                if (fileSystems.TryRead(candidate, null, out _)) return candidate;
            }
            catch (InvalidDataException) { }
        }
        return firstDecoded ?? throw ScpDetectionExceptions.NoDecodedSector(formatId ?? nameof(ScpFormatFamily));
    }
}
