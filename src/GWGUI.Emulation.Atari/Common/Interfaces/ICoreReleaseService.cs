namespace GWGUI.Emulation.Atari.Common.Interfaces;

public interface ICoreReleaseService
{
    Task<IReadOnlyList<CoreRelease>> GetAvailableAsync(Emulator emulator,
        CancellationToken cancellationToken = default);
    Task<CoreInstallationPaths> InstallAsync(CoreRelease release,
        IProgress<CoreInstallProgress>? progress = null,
        CancellationToken cancellationToken = default);
    Task<CoreInstallationPaths?> GetActiveInstallationAsync(Emulator emulator,
        CancellationToken cancellationToken = default);
}
