using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Dec;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Dec.Rx02;

/// <summary>Lit un dump DEC RX02 en ordre physique et produit les blocs logiques RT-11.</summary>
public sealed class DecRx02Reader
{
    /// <summary>Lit un dump RX02 depuis son chemin.</summary>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default) => Read(await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false), cancellationToken);

    /// <summary>Lit un dump RX02 déjà chargé en mémoire.</summary>
    public Task<SectorImage> ReadAsync(ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default) => Task.FromResult(Read(bytes.Span, cancellationToken));

    /// <summary>Valide la capacité puis remet les secteurs du dump en ordre logique.</summary>
    private static SectorImage Read(ReadOnlySpan<byte> bytes, CancellationToken cancellationToken)
    {
        if (bytes.Length != DecRx02Geometry.Capacity) throw new InvalidDataException("The image is not a complete DEC RX02 dump.");
        var blocks = new SectorBlock[DecRx02Geometry.LogicalBlockCount];
        for (var block = 0; block < blocks.Length; block++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var data = new byte[DecRx02Geometry.LogicalBlockSize];
            for (var part = 0; part < DecRx02Geometry.PhysicalSectorsPerLogicalBlock; part++) DecRx02SectorOrder.CopyLogicalSector(bytes, block * DecRx02Geometry.PhysicalSectorsPerLogicalBlock + part, data.AsSpan(part * DecRx02Geometry.PhysicalSectorSize));
            blocks[block] = new(block, new(block / DecRx02Geometry.LogicalBlocksPerTrack, 0, block % DecRx02Geometry.LogicalBlocksPerTrack + 1), data, true);
        }
        return new(DiskImageFormatIds.DecRx02, DecRx02Geometry.LogicalBlockSize, DecRx02Geometry.TrackCount, DecRx02Geometry.HeadCount, DecRx02Geometry.LogicalBlocksPerTrack, blocks, capacity: DecRx02Geometry.Capacity, logicalBlockCount: DecRx02Geometry.LogicalBlockCount);
    }
}
