using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.App.Services.PhysicalDiskReading;

public sealed record PhysicalDiskReadProgress(
    int CompletedTracks,
    int TotalTracks,
    int Cylinder,
    int Head,
    int Attempt,
    ScpTrack? CapturedTrack = null);
