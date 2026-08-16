namespace GWGUI.Emulation.Atari.Cores;

public sealed record AtariCoreRelease(
    AtariCoreKind Kind,
    string Id,
    string DeclaredVersion,
    Uri DownloadUri,
    DateTimeOffset PublishedUtc,
    long? ExpectedArchiveSize);

public sealed record AtariCoreInstallProgress(long DownloadedBytes, long? TotalBytes)
{
    public double? Fraction => TotalBytes is > 0 ? DownloadedBytes / (double)TotalBytes.Value : null;
}
