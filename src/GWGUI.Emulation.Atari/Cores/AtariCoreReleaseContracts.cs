namespace GWGUI.Emulation.Atari.Cores;

public sealed record AtariCoreRelease(
    AtariEmulator Emulator,
    string Id,
    string DeclaredVersion,
    Uri DownloadUri,
    DateTimeOffset PublishedUtc,
    long? ExpectedArchiveSize);

public sealed record AtariCoreInstallProgress(long DownloadedBytes, long? TotalBytes)
{
    public double? Fraction => TotalBytes is > 0 ? DownloadedBytes / (double)TotalBytes.Value : null;
}

public interface IAtariCoreReleaseService
{
    Task<IReadOnlyList<AtariCoreRelease>> GetAvailableAsync(AtariEmulator emulator,
        CancellationToken cancellationToken = default);
    Task<AtariCoreInstallationPaths> InstallAsync(AtariCoreRelease release,
        IProgress<AtariCoreInstallProgress>? progress = null,
        CancellationToken cancellationToken = default);
    Task<AtariCoreInstallationPaths?> GetActiveInstallationAsync(AtariEmulator emulator,
        CancellationToken cancellationToken = default);
}
