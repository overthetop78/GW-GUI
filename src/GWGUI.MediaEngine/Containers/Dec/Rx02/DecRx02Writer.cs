using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Dec;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.Containers.Storage;

namespace GWGUI.MediaEngine.Containers.Dec.Rx02;

/// <summary>Écrit les blocs logiques RT-11 dans l'ordre physique entrelacé d'un dump DEC RX02.</summary>
public sealed class DecRx02Writer(IAtomicImageFileWriter? fileWriter = null)
{
    private readonly IAtomicImageFileWriter files = fileWriter ?? new AtomicImageFileWriter();
    /// <summary>Valide la géométrie, sépare chaque bloc en deux secteurs puis écrit atomiquement le dump.</summary>
    public async Task WriteAsync(SectorImage image, string path, CancellationToken cancellationToken = default)
    {
        if (!image.FormatId.Equals(DiskImageFormatIds.DecRx02, StringComparison.OrdinalIgnoreCase) || image.BlockSize != DecRx02Geometry.LogicalBlockSize || image.BlockCount != DecRx02Geometry.LogicalBlockCount || image.Cylinders != DecRx02Geometry.TrackCount || image.Heads != DecRx02Geometry.HeadCount) throw new InvalidDataException("The sector image does not use the DEC RX02 geometry.");
        var bytes = new byte[DecRx02Geometry.Capacity];
        for (var blockIndex = 0; blockIndex < DecRx02Geometry.LogicalBlockCount; blockIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!image.TryGetBlock(blockIndex, out var block)) throw new InvalidDataException($"DEC RX02 logical block {blockIndex} is missing.");
            if (block.Data.Count != DecRx02Geometry.LogicalBlockSize) throw new InvalidDataException($"DEC RX02 logical block {blockIndex} has an invalid size.");
            var data = block.Data.ToArray();
            for (var part = 0; part < DecRx02Geometry.PhysicalSectorsPerLogicalBlock; part++) DecRx02SectorOrder.WriteLogicalSector(bytes, blockIndex * DecRx02Geometry.PhysicalSectorsPerLogicalBlock + part, data.AsSpan(part * DecRx02Geometry.PhysicalSectorSize, DecRx02Geometry.PhysicalSectorSize));
        }
        await files.WriteAsync(path, (output, token) => output.WriteAsync(bytes, token).AsTask(), cancellationToken).ConfigureAwait(false);
    }
}
