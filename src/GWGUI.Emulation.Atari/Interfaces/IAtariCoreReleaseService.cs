namespace GWGUI.Emulation.Atari.Interfaces;

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
