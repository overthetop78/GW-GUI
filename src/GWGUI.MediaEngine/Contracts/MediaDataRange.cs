using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Interfaces;

namespace GWGUI.MediaEngine.Contracts;

/// <summary>Maps one logical media range to stored data or to an explicit non-stored state.</summary>
public sealed class MediaDataRange
{
    public MediaDataRange(
        long address,
        long length,
        MediaDataRangeKind kind,
        IMediaRandomAccessData? source = null,
        long sourceOffset = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(address);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);
        ArgumentOutOfRangeException.ThrowIfNegative(sourceOffset);

        if (kind == MediaDataRangeKind.Stored)
        {
            ArgumentNullException.ThrowIfNull(source);
            if (sourceOffset > source.Length || length > source.Length - sourceOffset)
                throw new ArgumentOutOfRangeException(nameof(length), "The mapped range exceeds its data source.");
        }
        else
        {
            if (source is not null)
                throw new ArgumentException("Only stored ranges can reference a data source.", nameof(source));
            if (sourceOffset != 0)
                throw new ArgumentOutOfRangeException(nameof(sourceOffset), "A non-stored range cannot have a source offset.");
        }

        Address = address;
        Length = length;
        Kind = kind;
        Source = source;
        SourceOffset = sourceOffset;
    }

    public long Address { get; }

    public long Length { get; }

    public MediaDataRangeKind Kind { get; }

    public IMediaRandomAccessData? Source { get; }

    public long SourceOffset { get; }
}
