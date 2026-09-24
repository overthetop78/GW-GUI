namespace GWGUI.MediaFileSystems.Contracts;

/// <summary>Volume sectoriel construit à injecter dans un conteneur cible par le moteur.</summary>
public sealed class MediaSectorWritePlan
{
    public MediaSectorWritePlan(
        string formatId,
        int blockSize,
        int cylinders,
        int heads,
        int sectorsPerTrack,
        IEnumerable<MediaSectorWriteBlock> blocks,
        long? capacity = null,
        int? logicalBlockCount = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(formatId);
        ArgumentNullException.ThrowIfNull(blocks);
        FormatId = formatId;
        BlockSize = blockSize;
        Cylinders = cylinders;
        Heads = heads;
        SectorsPerTrack = sectorsPerTrack;
        AvailableBlocks = Array.AsReadOnly(blocks.ToArray());
        Capacity = capacity;
        LogicalBlockCount = logicalBlockCount;
    }

    public string FormatId { get; }

    public int BlockSize { get; }

    public int Cylinders { get; }

    public int Heads { get; }

    public int SectorsPerTrack { get; }

    public IReadOnlyList<MediaSectorWriteBlock> AvailableBlocks { get; }

    public long? Capacity { get; }

    public int? LogicalBlockCount { get; }
}
