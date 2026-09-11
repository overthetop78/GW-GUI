using GWGUI.MediaEngine.Contracts;

namespace GWGUI.MediaEngine.Reading.Optical;

/// <summary>Reads stored or user-data sectors through a container-independent optical track descriptor.</summary>
public sealed class OpticalSectorReader
{
    public async ValueTask<byte[]> ReadStoredSectorAsync(
        OpticalTrackDescriptor track,
        long relativeSector,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(track);
        ValidateRelativeSector(track, relativeSector);
        var sector = new byte[track.StoredSectorSize];
        await track.DataSource.ReadExactlyAsync(
            checked(track.SourceOffset + relativeSector * track.StoredSectorSize),
            sector,
            cancellationToken).ConfigureAwait(false);
        return sector;
    }

    public async ValueTask<byte[]> ReadUserDataAsync(
        OpticalTrackDescriptor track,
        long relativeSector,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(track);
        ValidateRelativeSector(track, relativeSector);
        var data = new byte[track.UserDataLength];
        await track.DataSource.ReadExactlyAsync(
            checked(track.SourceOffset + relativeSector * track.StoredSectorSize + track.UserDataOffset),
            data,
            cancellationToken).ConfigureAwait(false);
        return data;
    }

    public async ValueTask<byte[]?> ReadSubchannelAsync(
        OpticalTrackDescriptor track,
        long relativeSector,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(track);
        ValidateRelativeSector(track, relativeSector);
        if (track.SubchannelSource is null)
            return null;

        var data = new byte[track.SubchannelBytesPerSector];
        await track.SubchannelSource.ReadExactlyAsync(
            checked(track.SubchannelSourceOffset + relativeSector * track.SubchannelStride),
            data,
            cancellationToken).ConfigureAwait(false);
        return data;
    }

    private static void ValidateRelativeSector(OpticalTrackDescriptor track, long relativeSector)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(relativeSector);
        if (relativeSector >= track.SectorCount)
            throw new ArgumentOutOfRangeException(nameof(relativeSector), "The sector is outside the optical track.");
    }
}
