using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Atari;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Atari.St;

/// <summary>Écrit une image sectorielle Atari ST brute dans l'ordre logique des secteurs.</summary>
public sealed class AtariStWriter
{
    /// <summary>Écrit tous les blocs annoncés et matérialise les secteurs absents avec des zéros.</summary>
    public async Task WriteAsync(SectorImage image, string path, CancellationToken cancellationToken = default)
    {
        if (!image.FormatId.StartsWith(DiskImageFormatIds.AtariStPrefix, StringComparison.OrdinalIgnoreCase) || image.BlockSize != AtariStGeometry.SectorSize)
            throw AtariStExceptions.UnsupportedSectorImage(image.FormatId, AtariStGeometry.SectorSize);
        await using var output = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, AtariStGeometry.SectorSize, FileOptions.Asynchronous);
        var empty = new byte[AtariStGeometry.SectorSize];
        for (var logical = 0; logical < image.BlockCount; logical++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!image.TryGetBlock(logical, out var block)) await output.WriteAsync(empty, cancellationToken).ConfigureAwait(false);
            else
            {
                if (block.Data.Count != AtariStGeometry.SectorSize) throw AtariStExceptions.InvalidLogicalSectorSize(logical, block.Data.Count, AtariStGeometry.SectorSize);
                await output.WriteAsync(block.Data.ToArray(), cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
