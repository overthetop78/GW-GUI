namespace GWGUI.App.Contracts.Services.PhysicalDiskWriting;

public sealed record PhysicalTrackWriteProgress(
    int CompletedTracks,
    int TotalTracks,
    int Cylinder,
    int Head,
    bool IsVerification,
    IReadOnlyList<PhysicalTrackWriteAddress> Tracks);

public sealed record PhysicalTrackWriteAddress(int Cylinder, int Head);
