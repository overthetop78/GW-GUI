using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.App.Contracts.Services.PhysicalDiskReading;

public sealed record PhysicalDiskReadProgress(
    int CompletedTracks,
    int TotalTracks,
    int Cylinder,
    int Head,
    int Attempt,
    ScpTrack? CapturedTrack = null);
