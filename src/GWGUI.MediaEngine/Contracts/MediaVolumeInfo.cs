using MediaVolumeDescriptor = global::GWGUI.MediaFileSystems.Contracts.MediaVolumeDescriptor;

namespace GWGUI.MediaEngine.Contracts;

/// <summary>Public view of a volume located in a decoded media image.</summary>
public sealed class MediaVolumeInfo
{
    internal MediaVolumeInfo(MediaVolumeDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        Start = descriptor.Start;
        Length = descriptor.Length;
        Origin = descriptor.Origin;
        PartitionTable = descriptor.PartitionTable;
        PartitionNumber = descriptor.PartitionNumber;
        SessionNumber = descriptor.SessionNumber;
        TrackNumber = descriptor.TrackNumber;
        FileSystemId = descriptor.FileSystemId;
        PartitionType = descriptor.PartitionType;
        PartitionId = descriptor.PartitionId;
        Name = descriptor.Name;
    }

    public long Start { get; }
    public long Length { get; }
    public string Origin { get; }
    public string? PartitionTable { get; }
    public int? PartitionNumber { get; }
    public int? SessionNumber { get; }
    public int? TrackNumber { get; }
    public string? FileSystemId { get; }
    public string? PartitionType { get; }
    public string? PartitionId { get; }
    public string? Name { get; }
}
