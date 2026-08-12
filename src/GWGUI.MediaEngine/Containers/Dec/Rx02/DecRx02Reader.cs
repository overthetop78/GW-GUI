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
        if (bytes.Length != DecRx02Geometry.Capacity) throw DecRx02Exceptions.IncompleteImage(bytes.Length, DecRx02Geometry.Capacity);
        var logicalSectors = ReadLogicalSectors(bytes, cancellationToken);
        return AssembleLogicalBlocks(logicalSectors, cancellationToken);
    }

    /// <summary>Replace les secteurs physiques de 256 octets dans leur ordre logique.</summary>
    private static byte[][] ReadLogicalSectors(ReadOnlySpan<byte> bytes, CancellationToken cancellationToken)
    {
        var sectors = new byte[DecRx02Geometry.PhysicalSectorCount][];
        for (var logicalSector = 0; logicalSector < sectors.Length; logicalSector++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            sectors[logicalSector] = new byte[DecRx02Geometry.PhysicalSectorSize];
            DecRx02SectorOrder.CopyLogicalSector(bytes, logicalSector, sectors[logicalSector]);
        }
        return sectors;
    }

    /// <summary>Assemble chaque paire de secteurs physiques consécutifs en bloc logique RT-11 de 512 octets.</summary>
    private static SectorImage AssembleLogicalBlocks(IReadOnlyList<byte[]> logicalSectors, CancellationToken cancellationToken)
    {
        var blocks = new SectorBlock[DecRx02Geometry.LogicalBlockCount];
        for (var block = 0; block < blocks.Length; block++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var data = new byte[DecRx02Geometry.LogicalBlockSize];
            for (var part = 0; part < DecRx02Geometry.PhysicalSectorsPerLogicalBlock; part++) logicalSectors[block * DecRx02Geometry.PhysicalSectorsPerLogicalBlock + part].CopyTo(data, part * DecRx02Geometry.PhysicalSectorSize);
            blocks[block] = new(block, new(block / DecRx02Geometry.LogicalBlocksPerTrack, DecRx02Geometry.FirstHead, block % DecRx02Geometry.LogicalBlocksPerTrack + DecRx02Geometry.FirstLogicalSectorNumber), data, true);
        }
        return new(DiskImageFormatIds.DecRx02, DecRx02Geometry.LogicalBlockSize, DecRx02Geometry.TrackCount, DecRx02Geometry.HeadCount, DecRx02Geometry.LogicalBlocksPerTrack, blocks, capacity: DecRx02Geometry.Capacity, logicalBlockCount: DecRx02Geometry.LogicalBlockCount);
    }
}
