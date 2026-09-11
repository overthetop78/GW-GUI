namespace GWGUI.MediaEngine.Contracts;

/// <summary>Describes a volume range and the partition, session, or file system information actually identified for it.</summary>
public sealed record MediaVolumeDescriptor
{
    public MediaVolumeDescriptor(
        long start,
        long length,
        string origin,
        string? partitionScheme = null,
        int? partitionNumber = null,
        int? sessionNumber = null,
        int? trackNumber = null,
        string? fileSystemId = null,
        string? partitionType = null,
        string? partitionId = null,
        string? name = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(start);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);
        if (string.IsNullOrWhiteSpace(origin)) throw new ArgumentException("A volume origin is required.", nameof(origin));
        if (partitionNumber is <= 0) throw new ArgumentOutOfRangeException(nameof(partitionNumber));
        if (sessionNumber is <= 0) throw new ArgumentOutOfRangeException(nameof(sessionNumber));
        if (trackNumber is <= 0) throw new ArgumentOutOfRangeException(nameof(trackNumber));

        Start = start;
        Length = length;
        Origin = origin;
        PartitionScheme = partitionScheme;
        PartitionNumber = partitionNumber;
        SessionNumber = sessionNumber;
        TrackNumber = trackNumber;
        FileSystemId = fileSystemId;
        PartitionType = partitionType;
        PartitionId = partitionId;
        Name = name;
    }

    public long Start { get; }

    public long Length { get; }

    public string Origin { get; }

    public string? PartitionScheme { get; }

    public int? PartitionNumber { get; }

    public int? SessionNumber { get; }

    public int? TrackNumber { get; }

    public string? FileSystemId { get; }

    public string? PartitionType { get; }

    public string? PartitionId { get; }

    public string? Name { get; }
}
