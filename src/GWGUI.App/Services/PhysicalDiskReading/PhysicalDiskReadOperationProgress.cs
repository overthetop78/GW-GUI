namespace GWGUI.App.Services.PhysicalDiskReading;

public sealed record PhysicalDiskReadOperationProgress(
    PhysicalDiskReadStage Stage,
    int CompletedTracks,
    int TotalTracks,
    int? Cylinder = null,
    int? Head = null,
    int Attempt = 1,
    IReadOnlyList<PhysicalDiskTrackAddress>? Tracks = null);
