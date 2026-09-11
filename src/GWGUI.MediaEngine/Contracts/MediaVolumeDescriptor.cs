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
        string? fileSystemId = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(start);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);
        if (string.IsNullOrWhiteSpace(origin)) throw new ArgumentException("A volume origin is required.", nameof(origin));
        if (partitionNumber is <= 0) throw new ArgumentOutOfRangeException(nameof(partitionNumber));
        if (sessionNumber is <= 0) throw new ArgumentOutOfRangeException(nameof(sessionNumber));

        Start = start;
        Length = length;
        Origin = origin;
        PartitionScheme = partitionScheme;
        PartitionNumber = partitionNumber;
        SessionNumber = sessionNumber;
        FileSystemId = fileSystemId;
    }

    public long Start { get; }

    public long Length { get; }

    public string Origin { get; }

    public string? PartitionScheme { get; }

    public int? PartitionNumber { get; }

    public int? SessionNumber { get; }

    public string? FileSystemId { get; }
}
