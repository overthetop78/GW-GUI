namespace GWGUI.App.Contracts.Services.PhysicalDiskWriting;

public sealed record PhysicalDiskWriteResult(
    int WrittenTracks,
    int TotalTracks,
    bool Cancelled,
    IReadOnlyList<PhysicalTrackWriteFailure> Failures)
{
    public bool IsSuccess => !Cancelled && Failures.Count == 0 && WrittenTracks == TotalTracks;
}
