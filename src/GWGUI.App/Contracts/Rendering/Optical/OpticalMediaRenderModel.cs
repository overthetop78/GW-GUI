using GWGUI.App.Enums.Rendering.Optical;

namespace GWGUI.App.Contracts.Rendering.Optical;

public sealed record OpticalMediaRenderModel(
    long? LogicalLength,
    int? FaceCount,
    int? LayerCount,
    IReadOnlyList<OpticalMediaTrack> Tracks,
    IReadOnlyList<string> AssociatedFiles);

public sealed record OpticalMediaTrack(
    int SessionNumber,
    int TrackNumber,
    long FirstSector,
    long SectorCount,
    OpticalTrackKind Kind,
    int? FaceNumber = null,
    int? LayerNumber = null,
    IReadOnlyList<OpticalMediaIndex>? Indexes = null);

public sealed record OpticalMediaIndex(
    int Number,
    long FirstSector,
    long SectorCount);
