using GWGUI.MediaEngine.Containers.Commodore.D64;
using GWGUI.MediaEngine.Containers.Commodore.D71;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Commodore;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.Containers.Storage;

namespace GWGUI.MediaEngine.Containers.Commodore;

/// <summary>Écrit l'ordre logique zoné commun aux conteneurs Commodore D64 et D71.</summary>
public sealed class CommodoreDosContainerWriter(IAtomicImageFileWriter? fileWriter = null)
{
    private readonly IAtomicImageFileWriter files = fileWriter ?? new AtomicImageFileWriter();
    /// <summary>Écrit les données puis, séparément, la carte facultative de diagnostics.</summary>
    public async Task WriteAsync(SectorImage image, string path, CommodoreDosErrorMapMode errorMapMode = CommodoreDosErrorMapMode.None, CancellationToken cancellationToken = default)
    {
        var dataBlockCount = Commodore1541Geometry.BlocksPerSide(image.Cylinders) * image.Heads;
        var validFormat = image.FormatId.Equals(DiskImageFormatIds.Commodore1541, StringComparison.OrdinalIgnoreCase) && image.Heads == 1 || image.FormatId.Equals(DiskImageFormatIds.Commodore1571, StringComparison.OrdinalIgnoreCase) && image.Heads == Commodore1571Geometry.SideCount;
        if (!validFormat || image.BlockSize != Commodore1541Geometry.SectorSize || image.BlockCount != dataBlockCount || !Commodore1541Geometry.SupportedTrackCounts.Contains(image.Cylinders)) throw CommodoreDosContainerExceptions.UnsupportedGeometry(image);
        await files.WriteAsync(path, async (output, token) =>
        {
            for (var logical = 0; logical < image.BlockCount; logical++)
            {
                token.ThrowIfCancellationRequested();
                if (!image.TryGetBlock(logical, out var block)) throw CommodoreDosContainerExceptions.MissingBlock(logical);
                if (block.Data.Count != Commodore1541Geometry.SectorSize) throw CommodoreDosContainerExceptions.InvalidBlockSize(logical, block.Data.Count, Commodore1541Geometry.SectorSize);
                await output.WriteAsync(block.Data.ToArray(), token).ConfigureAwait(false);
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
        }, cancellationToken).ConfigureAwait(false);
    }
}
