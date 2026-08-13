using GWGUI.MediaEngine.Containers.Commodore.D64;
using GWGUI.MediaEngine.Containers.Commodore.D71;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Commodore;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Commodore;

/// <summary>Écrit l'ordre logique zoné commun aux conteneurs Commodore D64 et D71.</summary>
public sealed class CommodoreDosContainerWriter
{
    /// <summary>Écrit les données puis, séparément, la carte facultative de diagnostics.</summary>
    public async Task WriteAsync(SectorImage image, string path, CommodoreDosErrorMapMode errorMapMode = CommodoreDosErrorMapMode.None, CancellationToken cancellationToken = default)
    {
        var dataBlockCount = Commodore1541Geometry.BlocksPerSide(image.Cylinders) * image.Heads;
        var validFormat = image.FormatId.Equals(DiskImageFormatIds.Commodore1541, StringComparison.OrdinalIgnoreCase) && image.Heads == 1 || image.FormatId.Equals(DiskImageFormatIds.Commodore1571, StringComparison.OrdinalIgnoreCase) && image.Heads == Commodore1571Geometry.SideCount;
        if (!validFormat || image.BlockSize != Commodore1541Geometry.SectorSize || image.BlockCount != dataBlockCount || !Commodore1541Geometry.SupportedTrackCounts.Contains(image.Cylinders)) throw CommodoreDosContainerExceptions.UnsupportedGeometry(image);
        var fullPath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath) ?? Directory.GetCurrentDirectory();
        Directory.CreateDirectory(directory);
        var temporaryPath = Path.Combine(directory, $".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            await using (var output = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, Commodore1541Geometry.SectorSize, FileOptions.Asynchronous))
            {
                for (var logical = 0; logical < image.BlockCount; logical++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!image.TryGetBlock(logical, out var block)) throw CommodoreDosContainerExceptions.MissingBlock(logical);
                    if (block.Data.Count != Commodore1541Geometry.SectorSize) throw CommodoreDosContainerExceptions.InvalidBlockSize(logical, block.Data.Count, Commodore1541Geometry.SectorSize);
                    await output.WriteAsync(block.Data.ToArray(), cancellationToken).ConfigureAwait(false);
                }
                if (errorMapMode == CommodoreDosErrorMapMode.Preserve)
                {
                    for (var logical = 0; logical < image.BlockCount; logical++)
                    {
                        if (!image.TryGetBlock(logical, out var block)) throw CommodoreDosContainerExceptions.MissingBlock(logical);
                        if (block.DiagnosticCode is not { } diagnostic) throw CommodoreDosContainerExceptions.MissingDiagnostic(logical);
                        output.WriteByte(diagnostic);
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
