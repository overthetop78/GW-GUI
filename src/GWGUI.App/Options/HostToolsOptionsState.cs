using GWGUI.Domain.HostTools;
using GWGUI.Domain.Settings;

namespace GWGUI.App.Options;

internal sealed class HostToolsOptionsState
{
    private readonly IGwInstallationManager _manager;

    public HostToolsOptionsState(AppSettings settings, IGwInstallationManager manager)
    {
        _manager = manager;
        CurrentPath = settings.GwExecutablePath;
        PreviousPath = settings.PreviousGwExecutablePath;
        InstalledVersion = settings.InstalledHostToolsVersion;
        AvailableVersion = settings.AvailableHostToolsVersion;
        LastCheckUtc = settings.LastHostToolsCheckUtc;
    }

    public string? CurrentPath { get; private set; }
    public string? PreviousPath { get; private set; }
    public string? InstalledVersion { get; private set; }
    public string? AvailableVersion { get; private set; }
    public DateTimeOffset? LastCheckUtc { get; private set; }

    public HostToolsInstallation? Detect(string? configuredPath) => _manager.Detect(configuredPath).FirstOrDefault();

    public async Task<HostToolsRelease> CheckLatestAsync(CancellationToken cancellationToken = default)
    {
        var release = await _manager.GetLatestReleaseAsync(cancellationToken).ConfigureAwait(false);
        AvailableVersion = release.Version;
        LastCheckUtc = DateTimeOffset.UtcNow;
        return release;
    }

    public async Task<HostToolsInstallation> InstallAsync(HostToolsRelease release, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        var installation = await _manager.InstallAsync(release, progress, cancellationToken).ConfigureAwait(false);
        Select(installation);
        return installation;
    }

    public void Select(HostToolsInstallation installation) => Apply(_manager.Select(CurrentPath, PreviousPath, installation));

    public void Rollback(string? currentPath) => Apply(_manager.Rollback(currentPath, PreviousPath));

    public void SetCurrentPath(string? path) => CurrentPath = string.IsNullOrWhiteSpace(path) ? null : path.Trim();

    public void ApplyTo(AppSettings settings)
    {
        settings.GwExecutablePath = CurrentPath;
        settings.PreviousGwExecutablePath = PreviousPath;
        settings.InstalledHostToolsVersion = InstalledVersion;
        settings.AvailableHostToolsVersion = AvailableVersion;
        settings.LastHostToolsCheckUtc = LastCheckUtc;
    }

    private void Apply(HostToolsSelection selection)
    {
        CurrentPath = selection.ExecutablePath;
        PreviousPath = selection.PreviousExecutablePath;
        InstalledVersion = selection.InstalledVersion;
    }
}
