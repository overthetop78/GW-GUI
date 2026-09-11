using System.Collections.ObjectModel;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Interfaces;

namespace GWGUI.MediaEngine.Representations.Optical;

/// <summary>Describes optical sessions, tracks, sectors, layers, faces, and associated files only when supplied by the source.</summary>
public sealed class OpticalMediaImageRepresentation : IMediaImageRepresentation
{
    public OpticalMediaImageRepresentation(
        long? logicalLength,
        IReadOnlyList<(int SessionNumber, int TrackNumber, long FirstSector, long SectorCount)>? tracks = null,
        int? layerCount = null,
        int? faceCount = null,
        IReadOnlyList<string>? associatedFiles = null)
    {
        if (logicalLength is < 0) throw new ArgumentOutOfRangeException(nameof(logicalLength));
        if (layerCount is <= 0) throw new ArgumentOutOfRangeException(nameof(layerCount));
        if (faceCount is <= 0) throw new ArgumentOutOfRangeException(nameof(faceCount));

        if (tracks is not null)
        {
            foreach (var track in tracks)
            {
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(track.SessionNumber);
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(track.TrackNumber);
                ArgumentOutOfRangeException.ThrowIfNegative(track.FirstSector);
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(track.SectorCount);
            }

            Tracks = new ReadOnlyCollection<(int SessionNumber, int TrackNumber, long FirstSector, long SectorCount)>(tracks.ToArray());
            Sessions = new ReadOnlyCollection<int>(tracks.Select(track => track.SessionNumber).Distinct().Order().ToArray());
        }

        LogicalLength = logicalLength;
        LayerCount = layerCount;
        FaceCount = faceCount;
        AssociatedFiles = associatedFiles is null ? null : new ReadOnlyCollection<string>(associatedFiles.ToArray());
    }

    public MediaRepresentationKind RepresentationKind => MediaRepresentationKind.OpticalTracks;

    public long? LogicalLength { get; }

    public bool SupportsRandomAccess => true;

    public bool SupportsSequentialAccess => true;

    public IReadOnlyList<int>? Sessions { get; }

    public IReadOnlyList<(int SessionNumber, int TrackNumber, long FirstSector, long SectorCount)>? Tracks { get; }

    public int? LayerCount { get; }

    public int? FaceCount { get; }

    public IReadOnlyList<string>? AssociatedFiles { get; }
}
