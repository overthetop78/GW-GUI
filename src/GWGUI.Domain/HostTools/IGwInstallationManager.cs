namespace GWGUI.Domain.HostTools;

public sealed record HostToolsRelease(string Version, Uri DownloadUri, string AssetName, string? Sha256 = null);
public sealed record HostToolsInstallation(string ExecutablePath, string? Version, bool Managed);
public sealed record HostToolsSelection(string? ExecutablePath, string? PreviousExecutablePath, string? InstalledVersion);

public interface IGwInstallationManager
{
    IReadOnlyList<HostToolsInstallation> Detect(string? configuredPath = null);
    Task<HostToolsRelease> GetLatestReleaseAsync(CancellationToken cancellationToken = default);
    Task<HostToolsInstallation> InstallAsync(HostToolsRelease release, IProgress<double>? progress = null, CancellationToken cancellationToken = default);
    HostToolsSelection Select(string? currentPath, string? previousPath, HostToolsInstallation selected);
    HostToolsSelection Rollback(string? currentPath, string? previousPath);
}
