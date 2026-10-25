using GWGUI.MediaEngine.Images.Reading.Reconstruction;
using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaEngine.Images.Models.Sectors;

namespace GWGUI.MediaEngine.Images.Formats.Floppy.Raw;
/// <summary>Écrit les blocs complets d'une image sectorielle dans leur ordre logique.</summary>
public sealed class LinearSectorImageWriter(IAtomicImageFileWriter? fileWriter = null)
{
    private readonly IAtomicImageFileWriter files = fileWriter ?? new AtomicImageFileWriter();
    /// <summary>Valide la géométrie puis écrit tous les blocs sans remplissage implicite.</summary>
    public async Task WriteAsync(SectorImage image, string path, RegularSectorGeometry geometry, CancellationToken cancellationToken = default)
    {
        ValidateGeometry(image, geometry);
        await files.WriteAsync(path, async (output, token) =>
        {
            foreach (var block in ReadBlocks(image, geometry, token))
                await output.WriteAsync(block.Data.ToArray(), token).ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);
    }

    internal static void ValidateGeometry(SectorImage image, RegularSectorGeometry geometry)
    {
        if (!image.FormatId.Equals(geometry.FormatId,
            StringComparison.OrdinalIgnoreCase) 
            || image.BlockSize 
            != geometry.BlockSize 
            || image.Cylinders 
            != geometry.Cylinders 
            || image.Heads 
            != geometry.Heads 
            || image.SectorsPerTrack 
            != geometry.SectorsPerTrack 
            || image.BlockCount 
            != geometry.BlockCount)
            throw LinearSectorImageWriterExceptions.InvalidGeometry(image.FormatId, image.BlockSize, image.Cylinders, image.Heads, image.SectorsPerTrack, geometry.FormatId, geometry.BlockSize, geometry.Cylinders, geometry.Heads, geometry.SectorsPerTrack);
    }

    internal static IEnumerable<SectorBlock> ReadBlocks(SectorImage image, RegularSectorGeometry geometry, CancellationToken cancellationToken)
    {
        for (var logicalBlock = 0; logicalBlock < geometry.BlockCount; logicalBlock++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!image.TryGetBlock(logicalBlock, out var block))
                throw LinearSectorImageWriterExceptions.MissingBlock(logicalBlock);
            if (block.Data.Count != geometry.BlockSize)
                throw LinearSectorImageWriterExceptions.InvalidBlockSize(logicalBlock, block.Data.Count, geometry.BlockSize);
            yield return block;
        }
    }
}
