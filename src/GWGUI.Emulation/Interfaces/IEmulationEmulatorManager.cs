namespace GWGUI.Emulation.Interfaces;

public interface IEmulationEmulatorManager
{
    ValueTask<EmulationEmulatorInstallation> GetEmulatorInstallationAsync(string machineId,
        CancellationToken cancellationToken = default);
    ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindEmulatorReleasesAsync(string machineId,
        CancellationToken cancellationToken = default);
    ValueTask<string> InstallEmulatorAsync(string machineId, EmulationEmulatorRelease release,
        IProgress<double>? progress = null, CancellationToken cancellationToken = default);
}
