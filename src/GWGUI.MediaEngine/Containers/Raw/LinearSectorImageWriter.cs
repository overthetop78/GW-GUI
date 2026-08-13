using GWGUI.MediaEngine.Reconstruction;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Raw;

/// <summary>Écrit les blocs complets d'une image sectorielle dans leur ordre logique.</summary>
public sealed class LinearSectorImageWriter
{
    /// <summary>Valide la géométrie puis écrit tous les blocs sans remplissage implicite.</summary>
    public async Task WriteAsync(SectorImage image, string path, RegularSectorGeometry geometry, CancellationToken cancellationToken = default)
    {
        if (!image.FormatId.Equals(geometry.FormatId, StringComparison.OrdinalIgnoreCase) || image.BlockSize != geometry.BlockSize || image.Cylinders != geometry.Cylinders || image.Heads != geometry.Heads || image.SectorsPerTrack != geometry.SectorsPerTrack || image.BlockCount != geometry.BlockCount) throw LinearSectorImageWriterExceptions.InvalidGeometry(image.FormatId, image.BlockSize, image.Cylinders, image.Heads, image.SectorsPerTrack, geometry.FormatId, geometry.BlockSize, geometry.Cylinders, geometry.Heads, geometry.SectorsPerTrack);
        var fullPath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath) ?? Directory.GetCurrentDirectory();
        Directory.CreateDirectory(directory);
        var temporaryPath = Path.Combine(directory, $".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            await using (var output = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, geometry.BlockSize, FileOptions.Asynchronous))
            {
                for (var logicalBlock = 0; logicalBlock < geometry.BlockCount; logicalBlock++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!image.TryGetBlock(logicalBlock, out var block)) throw LinearSectorImageWriterExceptions.MissingBlock(logicalBlock);
                    if (block.Data.Count != geometry.BlockSize) throw LinearSectorImageWriterExceptions.InvalidBlockSize(logicalBlock, block.Data.Count, geometry.BlockSize);
                    await output.WriteAsync(block.Data.ToArray(), cancellationToken).ConfigureAwait(false);
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
