using System.Collections.ObjectModel;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaEngine.Reading.Sources;

namespace GWGUI.MediaEngine.Representations.Optical;

/// <summary>Describes optical sessions, tracks, sectors, layers, faces, and associated files only when supplied by the source.</summary>
public sealed class OpticalMediaImageRepresentation : IMediaImageRepresentation
{
    public OpticalMediaImageRepresentation(
        long? logicalLength,
        IReadOnlyList<OpticalTrackDescriptor>? tracks = null,
        int? layerCount = null,
        int? faceCount = null,
        IReadOnlyList<string>? associatedFiles = null)
    {
        if (logicalLength is < 0) throw new ArgumentOutOfRangeException(nameof(logicalLength));
        if (layerCount is <= 0) throw new ArgumentOutOfRangeException(nameof(layerCount));
        if (faceCount is <= 0) throw new ArgumentOutOfRangeException(nameof(faceCount));

        if (tracks is not null)
        {
            Tracks = new ReadOnlyCollection<OpticalTrackDescriptor>(tracks.ToArray());
            Sessions = new ReadOnlyCollection<int>(tracks.Select(track => track.SessionNumber).Distinct().Order().ToArray());
        }

        LogicalLength = logicalLength;
        LayerCount = layerCount;
        FaceCount = faceCount;
        AssociatedFiles = associatedFiles is null ? null : new ReadOnlyCollection<string>(associatedFiles.ToArray());
    }

    /// <summary>Creates a metadata-only representation for callers using the former tuple-based track contract.</summary>
    public OpticalMediaImageRepresentation(
        long? logicalLength,
        IReadOnlyList<(int SessionNumber, int TrackNumber, long FirstSector, long SectorCount)> tracks,
        int? layerCount = null,
        int? faceCount = null,
        IReadOnlyList<string>? associatedFiles = null)
        : this(
            logicalLength,
            tracks.Select(CreateCompatibilityTrack).ToArray(),
            layerCount,
            faceCount,
            associatedFiles)
    {
        ArgumentNullException.ThrowIfNull(tracks);
    }

    public MediaRepresentationKind RepresentationKind => MediaRepresentationKind.OpticalTracks;

    public long? LogicalLength { get; }

    public bool SupportsRandomAccess => true;

    public bool SupportsSequentialAccess => true;

    public IReadOnlyList<int>? Sessions { get; }

    public IReadOnlyList<OpticalTrackDescriptor>? Tracks { get; }

    public int? LayerCount { get; }

    public int? FaceCount { get; }

    public IReadOnlyList<string>? AssociatedFiles { get; }

    private static OpticalTrackDescriptor CreateCompatibilityTrack(
        (int SessionNumber, int TrackNumber, long FirstSector, long SectorCount) track)
    {
        const int sectorSize = 2048;
        var length = checked(track.SectorCount * sectorSize);
        return new OpticalTrackDescriptor(
            track.SessionNumber,
            track.TrackNumber,
            OpticalTrackMode.Mode1Data2048,
            track.FirstSector,
            track.SectorCount,
            sectorSize,
            0,
            sectorSize,
            new UnavailableRandomAccessData(length),
            0);
    }
}
