namespace GWGUI.App.Contracts.Services.PhysicalDiskWriting;

public sealed record PhysicalTrackWriteProgress(
    int CompletedTracks,
    int TotalTracks,
    int Cylinder,
    int Head,
    bool IsVerification);
