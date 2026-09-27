namespace GWGUI.Emulation.Interfaces;

public interface IEmulationEmulatorManager
{
    ValueTask<EmulationEmulatorInstallation> GetEmulatorInstallationAsync(string machineId,
        CancellationToken cancellationToken = default);

    ValueTask<EmulationEmulatorInstallation> GetEmulatorInstallationAsync(
        IEmulationConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return GetEmulatorInstallationAsync(configuration.MachineId, cancellationToken);
    }

    async ValueTask<IReadOnlyList<EmulationEmulatorInstallation>> GetEmulatorInstallationsAsync(
        IEmulationConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        var installation = await GetEmulatorInstallationAsync(configuration, cancellationToken)
            .ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(installation.EmulatorId)
            || string.IsNullOrWhiteSpace(installation.DisplayName)
            || string.IsNullOrWhiteSpace(installation.DescriptionResourceKey))
            throw new InvalidOperationException(nameof(EmulationEmulatorInstallation));
        return [installation];
    }

    async ValueTask<IEmulationConfiguration> UseEmulatorAsync(
        IEmulationConfiguration configuration,
        string emulatorId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(emulatorId);
        var current = await GetEmulatorInstallationAsync(configuration, cancellationToken).ConfigureAwait(false);
        if (!string.Equals(current.EmulatorId, emulatorId, StringComparison.Ordinal))
            throw new ArgumentOutOfRangeException(nameof(emulatorId), emulatorId, null);
        return configuration;
    }

    ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindEmulatorReleasesAsync(string machineId,
        CancellationToken cancellationToken = default);

    ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindEmulatorReleasesAsync(
        IEmulationConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return FindEmulatorReleasesAsync(configuration.MachineId, cancellationToken);
    }

    ValueTask<string> InstallEmulatorAsync(string machineId, EmulationEmulatorRelease release,
        IProgress<double>? progress = null, CancellationToken cancellationToken = default);

    ValueTask<string> InstallEmulatorAsync(IEmulationConfiguration configuration,
        EmulationEmulatorRelease release, IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return InstallEmulatorAsync(configuration.MachineId, release, progress, cancellationToken);
    }
}
