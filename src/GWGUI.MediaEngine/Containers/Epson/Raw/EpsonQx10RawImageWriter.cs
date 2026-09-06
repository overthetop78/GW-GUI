using GWGUI.MediaEngine.Geometries.Epson;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Epson.Raw;

/// <summary>Écrit une image Epson QX-10 brute en suivant sa géométrie variable cataloguée.</summary>
public sealed class EpsonQx10RawImageWriter(GWGUI.MediaEngine.Containers.Storage.IAtomicImageFileWriter? fileSystem = null)
{
    private readonly GWGUI.MediaEngine.Containers.Storage.IAtomicImageFileWriter files = fileSystem ?? new GWGUI.MediaEngine.Containers.Storage.AtomicImageFileWriter();
    /// <summary>Écrit tous les secteurs attendus dans l'ordre cylindre, face et numéro.</summary>
    public async Task WriteAsync(SectorImage image, string path, string formatId, CancellationToken cancellationToken = default)
    {
        var geometry = EpsonQx10GeometryCatalog.Resolve(formatId);
        if (!image.FormatId.Equals(formatId, StringComparison.OrdinalIgnoreCase) || image.Cylinders != geometry.Cylinders || image.Heads != geometry.Heads) throw new InvalidDataException($"Epson image geometry does not match '{formatId}'.");
        var blocks = image.AvailableBlocks.ToDictionary(block => block.Address);
        await files.WriteAsync(path, async (output, cancellationToken) =>
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
        }, cancellationToken).ConfigureAwait(false);
    }
}
