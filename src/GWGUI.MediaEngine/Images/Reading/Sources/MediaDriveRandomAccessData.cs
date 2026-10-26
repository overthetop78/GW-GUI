using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Interfaces;
using IMediaRandomAccessData = GWGUI.MediaFileSystems.Interfaces.IMediaRandomAccessData;

namespace GWGUI.MediaEngine.Images.Reading.Sources;

/// <summary>Raccorde les accès bornés d’une source aux positions du média, sans posséder la source ou le lecteur.</summary>
internal sealed class MediaDriveRandomAccessData(IMediaRandomAccessData source, IMediaDriveMechanics drive,
    Func<long, MediaDrivePosition> position, Func<long, long> contiguousLength, bool writing) : IMediaRandomAccessData
{
    internal IMediaDriveMechanics Drive => drive;
    internal bool Writing => writing;
    public long Length => source.Length;

    public async ValueTask ReadExactlyAsync(long offset, Memory<byte> destination, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        if (offset > Length || destination.Length > Length - offset) throw new ArgumentOutOfRangeException(nameof(destination));
        var completed = 0;
        while (completed < destination.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var address = checked(offset + completed);
            var count = (int)Math.Min(destination.Length - completed, contiguousLength(address));
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
            if (writing) drive.BeginWrite(position(address));
            else drive.BeginRead(position(address));
            try
            {
                await source.ReadExactlyAsync(address, destination.Slice(completed, count), cancellationToken).ConfigureAwait(false);
                drive.Advance(position(checked(address + count - 1)));
            }
            finally { drive.EndAccess(); }
            completed += count;
        }
    }
}
