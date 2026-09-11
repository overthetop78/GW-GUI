using System.Collections.ObjectModel;

namespace GWGUI.MediaEngine.Contracts;

/// <summary>Contains one protocol block decoded from sequential media and the source segments that produced it.</summary>
public sealed class SequentialDecodedBlock
{
    public SequentialDecodedBlock(
        long position,
        ReadOnlyMemory<byte> data,
        IReadOnlyList<long> sourceSegmentPositions,
        bool? integrityValid,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(position);
        ArgumentNullException.ThrowIfNull(sourceSegmentPositions);
        if (sourceSegmentPositions.Any(sourcePosition => sourcePosition < 0))
            throw new ArgumentOutOfRangeException(nameof(sourceSegmentPositions));

        Position = position;
        Data = data;
        SourceSegmentPositions = new ReadOnlyCollection<long>(sourceSegmentPositions.ToArray());
        IntegrityValid = integrityValid;
        Metadata = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(metadata ?? new Dictionary<string, string>(), StringComparer.Ordinal));
    }

    public long Position { get; }
    public ReadOnlyMemory<byte> Data { get; }
    public IReadOnlyList<long> SourceSegmentPositions { get; }
    public bool? IntegrityValid { get; }
    public IReadOnlyDictionary<string, string> Metadata { get; }
}
