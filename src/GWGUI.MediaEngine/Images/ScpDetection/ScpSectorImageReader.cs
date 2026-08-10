using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images.ScpDetection;

internal sealed class ScpSectorImageReader(ScpCandidateRegistry candidates, FileSystemRegistry fileSystems)
{
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
        return firstDecoded ?? throw new InvalidDataException("No supported sectors could be decoded from the SCP image.");
    }
}
