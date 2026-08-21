namespace GWGUI.App.Services.PhysicalDiskWriting;

public sealed record PhysicalTrackWriteProgress(
    int CompletedTracks,
    int TotalTracks,
    int Cylinder,
    int Head,
    bool IsVerification);

public sealed record PhysicalTrackWriteFailure(
    int? Cylinder,
    int? Head,
    PhysicalDiskWriteFailureCategory Category,
    Exception? Exception = null);

public sealed record PhysicalDiskWriteResult(
    int WrittenTracks,
    int TotalTracks,
    bool Cancelled,
    IReadOnlyList<PhysicalTrackWriteFailure> Failures)
{
    public bool IsSuccess => !Cancelled && Failures.Count == 0 && WrittenTracks == TotalTracks;
}
