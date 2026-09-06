using GWGUI.MediaEngine.Reconstruction;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.Containers.Storage;

namespace GWGUI.MediaEngine.Containers.Raw;

/// <summary>Écrit les blocs complets d'une image sectorielle dans leur ordre logique.</summary>
public sealed class LinearSectorImageWriter(IAtomicImageFileWriter? fileWriter = null)
{
    private readonly IAtomicImageFileWriter files = fileWriter ?? new AtomicImageFileWriter();
    /// <summary>Valide la géométrie puis écrit tous les blocs sans remplissage implicite.</summary>
    public async Task WriteAsync(SectorImage image, string path, RegularSectorGeometry geometry, CancellationToken cancellationToken = default)
    {
        if (!image.FormatId.Equals(geometry.FormatId, StringComparison.OrdinalIgnoreCase) || image.BlockSize != geometry.BlockSize || image.Cylinders != geometry.Cylinders || image.Heads != geometry.Heads || image.SectorsPerTrack != geometry.SectorsPerTrack || image.BlockCount != geometry.BlockCount) throw LinearSectorImageWriterExceptions.InvalidGeometry(image.FormatId, image.BlockSize, image.Cylinders, image.Heads, image.SectorsPerTrack, geometry.FormatId, geometry.BlockSize, geometry.Cylinders, geometry.Heads, geometry.SectorsPerTrack);
        await files.WriteAsync(path, async (output, token) =>
        {
            for (var logicalBlock = 0; logicalBlock < geometry.BlockCount; logicalBlock++)
            {
                token.ThrowIfCancellationRequested();
                if (!image.TryGetBlock(logicalBlock, out var block)) throw LinearSectorImageWriterExceptions.MissingBlock(logicalBlock);
                if (block.Data.Count != geometry.BlockSize) throw LinearSectorImageWriterExceptions.InvalidBlockSize(logicalBlock, block.Data.Count, geometry.BlockSize);
                await output.WriteAsync(block.Data.ToArray(), token).ConfigureAwait(false);
            }
        }, cancellationToken).ConfigureAwait(false);
    }
}
