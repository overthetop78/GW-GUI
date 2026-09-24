namespace GWGUI.MediaFileSystems.Interfaces;

/// <summary>Exposes the sectors and geometry of one already decoded media image.</summary>
public interface IMediaSectorImage
{
    string FormatId { get; }

    int BlockSize { get; }

    int Cylinders { get; }

    int Heads { get; }

    int SectorsPerTrack { get; }

    int BlockCount { get; }

    long Capacity { get; }

    IReadOnlyCollection<IMediaSectorBlock> AvailableBlocks { get; }

    bool TryGetBlock(int logicalBlock, out IMediaSectorBlock block);
}
