using System.Collections.ObjectModel;
using GWGUI.MediaEngine.Enums;
using IMediaOpticalTrack = global::GWGUI.MediaFileSystems.Interfaces.IMediaOpticalTrack;
using IMediaRandomAccessData = global::GWGUI.MediaFileSystems.Interfaces.IMediaRandomAccessData;

namespace GWGUI.MediaEngine.Contracts;

/// <summary>Describes one readable optical track without exposing its container format to consumers.</summary>
public sealed class OpticalTrackDescriptor : IMediaOpticalTrack
{
    public OpticalTrackDescriptor(
        int sessionNumber,
        int trackNumber,
        OpticalTrackMode mode,
        long firstSector,
        long sectorCount,
        int storedSectorSize,
        int userDataOffset,
        int userDataLength,
        IMediaRandomAccessData dataSource,
        long sourceOffset,
        IReadOnlyList<OpticalTrackIndex>? indexes = null,
        long pregapSectors = 0,
        long postgapSectors = 0,
        IReadOnlyList<string>? flags = null,
        string? catalogNumber = null,
        string? isrc = null,
        IMediaRandomAccessData? subchannelSource = null,
        long subchannelSourceOffset = 0,
        int subchannelBytesPerSector = 0,
        long storedPregapSectors = 0,
        long? storedPregapSourceOffset = null,
        int subchannelStride = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sessionNumber);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(trackNumber);
        ArgumentOutOfRangeException.ThrowIfNegative(firstSector);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sectorCount);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(storedSectorSize);
        ArgumentOutOfRangeException.ThrowIfNegative(userDataOffset);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userDataLength);
        ArgumentOutOfRangeException.ThrowIfNegative(sourceOffset);
        ArgumentOutOfRangeException.ThrowIfNegative(pregapSectors);
        ArgumentOutOfRangeException.ThrowIfNegative(postgapSectors);
        ArgumentOutOfRangeException.ThrowIfNegative(subchannelSourceOffset);
        ArgumentOutOfRangeException.ThrowIfNegative(subchannelBytesPerSector);
        ArgumentOutOfRangeException.ThrowIfNegative(storedPregapSectors);
        ArgumentOutOfRangeException.ThrowIfNegative(subchannelStride);
        ArgumentNullException.ThrowIfNull(dataSource);
        if (userDataOffset > storedSectorSize - userDataLength)
            throw new ArgumentException("The user data window exceeds one stored sector.");
        if (sourceOffset > dataSource.Length - checked(sectorCount * storedSectorSize))
            throw new ArgumentException("The track data exceeds its source.", nameof(sourceOffset));
        if (subchannelSource is null && (subchannelSourceOffset != 0 || subchannelBytesPerSector != 0 || subchannelStride != 0))
            throw new ArgumentException("Subchannel dimensions require a subchannel source.");
        var effectiveSubchannelStride = subchannelStride == 0 ? subchannelBytesPerSector : subchannelStride;
        if (subchannelSource is not null
            && (subchannelBytesPerSector <= 0
                || effectiveSubchannelStride < subchannelBytesPerSector
                || subchannelSourceOffset > subchannelSource.Length
                    - checked((sectorCount - 1) * effectiveSubchannelStride + subchannelBytesPerSector)))
            throw new ArgumentException("The subchannel data exceeds its source.", nameof(subchannelSourceOffset));
        if (storedPregapSectors == 0 && storedPregapSourceOffset is not null)
            throw new ArgumentException("A stored pregap source offset requires stored pregap sectors.", nameof(storedPregapSourceOffset));
        if (storedPregapSectors > 0
            && (storedPregapSourceOffset is null
                || storedPregapSourceOffset < 0
                || storedPregapSourceOffset > dataSource.Length - checked(storedPregapSectors * storedSectorSize)))
            throw new ArgumentException("The stored pregap exceeds its source.", nameof(storedPregapSourceOffset));

        SessionNumber = sessionNumber;
        TrackNumber = trackNumber;
        Mode = mode;
        FirstSector = firstSector;
        SectorCount = sectorCount;
        StoredSectorSize = storedSectorSize;
        UserDataOffset = userDataOffset;
        UserDataLength = userDataLength;
        DataSource = dataSource;
        SourceOffset = sourceOffset;
        Indexes = new ReadOnlyCollection<OpticalTrackIndex>((indexes ?? []).OrderBy(index => index.Number).ToArray());
        PregapSectors = pregapSectors;
        PostgapSectors = postgapSectors;
        Flags = new ReadOnlyCollection<string>((flags ?? []).ToArray());
        CatalogNumber = catalogNumber;
        Isrc = isrc;
        SubchannelSource = subchannelSource;
        SubchannelSourceOffset = subchannelSourceOffset;
        SubchannelBytesPerSector = subchannelBytesPerSector;
        StoredPregapSectors = storedPregapSectors;
        StoredPregapSourceOffset = storedPregapSourceOffset;
        SubchannelStride = effectiveSubchannelStride;
    }

    public int SessionNumber { get; }
    public int TrackNumber { get; }
    public OpticalTrackMode Mode { get; }
    public long FirstSector { get; }
    public long SectorCount { get; }
    public int StoredSectorSize { get; }
    public int UserDataOffset { get; }
    public int UserDataLength { get; }
    public IMediaRandomAccessData DataSource { get; }
    public long SourceOffset { get; }
    public IReadOnlyList<OpticalTrackIndex> Indexes { get; }
    public long PregapSectors { get; }
    public long PostgapSectors { get; }
    public IReadOnlyList<string> Flags { get; }
    public string? CatalogNumber { get; }
    public string? Isrc { get; }
    public IMediaRandomAccessData? SubchannelSource { get; }
    public long SubchannelSourceOffset { get; }
    public int SubchannelBytesPerSector { get; }
    public long StoredPregapSectors { get; }
    public long? StoredPregapSourceOffset { get; }
    public int SubchannelStride { get; }
    public bool HasSubchannels => SubchannelSource is not null;
    public bool IsAudio => Mode == OpticalTrackMode.Audio;
}
