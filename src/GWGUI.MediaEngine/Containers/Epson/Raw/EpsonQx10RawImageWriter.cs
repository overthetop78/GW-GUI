using GWGUI.MediaEngine.Geometries.Epson;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Epson.Raw;

/// <summary>Écrit une image Epson QX-10 brute en suivant sa géométrie variable cataloguée.</summary>
public sealed class EpsonQx10RawImageWriter
{
    /// <summary>Écrit tous les secteurs attendus dans l'ordre cylindre, face et numéro.</summary>
    public async Task WriteAsync(SectorImage image, string path, string formatId, CancellationToken cancellationToken = default)
    {
        var geometry = EpsonQx10GeometryCatalog.Resolve(formatId);
        if (!image.FormatId.Equals(formatId, StringComparison.OrdinalIgnoreCase) || image.Cylinders != geometry.Cylinders || image.Heads != geometry.Heads) throw new InvalidDataException($"Epson image geometry does not match '{formatId}'.");
        var blocks = image.AvailableBlocks.ToDictionary(block => block.Address);
        var fullPath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath) ?? Directory.GetCurrentDirectory();
        Directory.CreateDirectory(directory);
        var temporaryPath = Path.Combine(directory, $".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            await using (var output = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous))
            {
                for (var cylinder = 0; cylinder < geometry.Cylinders; cylinder++)
                {
                    for (var head = 0; head < geometry.Heads; head++)
                    {
                        var track = geometry.Track(cylinder, head);
                        for (var index = 0; index < track.Count; index++)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            var address = new SectorAddress(cylinder, head, track.FirstSector + index);
                            if (!blocks.TryGetValue(address, out var block)) throw new InvalidDataException($"Epson sector {cylinder}:{head}:{address.Number} is missing.");
                            if (block.Data.Count != track.SectorSize) throw new InvalidDataException($"Epson sector {cylinder}:{head}:{address.Number} has size {block.Data.Count}; expected {track.SectorSize}.");
                            await output.WriteAsync(block.Data.ToArray(), cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
            }
            File.Move(temporaryPath, fullPath, true);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }
}
