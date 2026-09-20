using MediaVolumeOrigins = global::GWGUI.MediaFileSystems.Constants.MediaVolumeOrigins;
using PartitionSchemeIds = global::GWGUI.MediaFileSystems.Constants.PartitionSchemeIds;
using MediaVolumeDescriptor = global::GWGUI.MediaFileSystems.Contracts.MediaVolumeDescriptor;
using MediaVolumeDetectionResult = global::GWGUI.MediaFileSystems.Contracts.MediaVolumeDetectionResult;
using IMediaImageDocument = global::GWGUI.MediaFileSystems.Interfaces.IMediaImageDocument;
using IMediaOpticalRepresentation = global::GWGUI.MediaFileSystems.Interfaces.IMediaOpticalRepresentation;
using GWGUI.MediaFileSystems.Interfaces.Exploration;




namespace GWGUI.MediaFileSystems.Exploration.Partitioning;

/// <summary>Exposes each optical data track as a distinct file-system candidate volume.</summary>
public sealed class OpticalTrackVolumeDetector : IMediaVolumeDetector
{
    public ValueTask<MediaVolumeDetectionResult?> DetectAsync(
        IMediaImageDocument document,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        cancellationToken.ThrowIfCancellationRequested();
        if (document.Representation is not IMediaOpticalRepresentation optical)
            return new ValueTask<MediaVolumeDetectionResult?>((MediaVolumeDetectionResult?)null);
        var dataTracks = optical.Tracks?.Where(track => !track.IsAudio).ToArray() ?? [];
        if (dataTracks.Length == 0)
            return new ValueTask<MediaVolumeDetectionResult?>(new MediaVolumeDetectionResult(
                [],
                ["The optical image contains no addressable data track."]));
        var volumes = dataTracks.Select(track => new MediaVolumeDescriptor(
            checked(track.FirstSector * track.UserDataLength),
            checked(track.SectorCount * track.UserDataLength),
            MediaVolumeOrigins.OpticalTrack,
            PartitionSchemeIds.Direct,
            sessionNumber: track.SessionNumber,
            trackNumber: track.TrackNumber)).ToArray();
        return new ValueTask<MediaVolumeDetectionResult?>(new MediaVolumeDetectionResult(volumes, []));
    }
}
