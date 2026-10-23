using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Microsoft.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Microsoft.Common.Constants;
using GWGUI.Emulation.Microsoft.Common.Dictionaries;
using GWGUI.Emulation.Microsoft.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Microsoft.Common.Machines.Common.Dictionaries;
using GWGUI.Emulation.Microsoft.Common.Services;

namespace GWGUI.Emulation.Microsoft.Modules;

public sealed class MicrosoftEmulationModule : IEmulationModule, IEmulationEmulatorManager,
    IEmulationInputSettingsManager, IEmulationStorageSettingsManager,
    IEmulationModuleLocalization, IEmulationFirmwareManager
{
    private readonly Engine _engine;

    public bool TryGetString(string key, System.Globalization.CultureInfo culture, out string value) => _engine.TryGetString(key, culture, out value);

    public MicrosoftEmulationModule(string configurationDirectory, string pathBase, HttpClient httpClient, string coreDirectory) => _engine = new Engine(configurationDirectory, pathBase, httpClient, coreDirectory);

    public string Id => _engine.Id;

    public string DisplayResourceKey => _engine.DisplayResourceKey;

    public string BrandImageResourceName => _engine.BrandImageResourceName;

    public IReadOnlyList<EmulationMachineDefinition> Machines => _engine.Machines;

    public EmulationSettingsVisibility DefaultVisibility => _engine.DefaultVisibility;

    public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode) => _engine.TryHandleHostCommand(arguments, out exitCode);

    public EmulationMachineSettings Describe(string machineId, IEmulationConfiguration? configuration = null) => _engine.Describe(machineId, configuration);

    public IEmulationConfiguration CreateConfiguration(string machineId) => _engine.CreateConfiguration(machineId);

    public IEmulationConfiguration ChangeMachine(IEmulationConfiguration configuration, string machineId) => _engine.ChangeMachine(configuration, machineId);

    public IEmulationConfiguration ApplySettings(IEmulationConfiguration configuration, IReadOnlyDictionary<string, string?> values) => _engine.ApplySettings(configuration, values);

    public IReadOnlyDictionary<string, string> RuntimeOptions(IEmulationConfiguration configuration) => _engine.RuntimeOptions(configuration);

    public EmulationConfigurationSummary SummarizeConfiguration(IEmulationConfiguration configuration) => _engine.SummarizeConfiguration(configuration);

    public EmulationInputSettings DescribeInputSettings(IEmulationConfiguration configuration) => _engine.DescribeInputSettings(configuration);

    public IEmulationConfiguration ApplyInputSettings(IEmulationConfiguration configuration, EmulationInputSettings settings) => _engine.ApplyInputSettings(configuration, settings);

    public ValueTask SaveInputSettingsAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default) => _engine.SaveInputSettingsAsync(configuration, cancellationToken);

    public EmulationStorageSettings DescribeStorageSettings(IEmulationConfiguration configuration) => _engine.DescribeStorageSettings(configuration);

    public IEmulationConfiguration ApplyStorageSettings(IEmulationConfiguration configuration, EmulationStorageSettings settings) => _engine.ApplyStorageSettings(configuration, settings);

    public ValueTask<EmulationMachineRuntime> CreateRuntimeAsync(IEmulationConfiguration configuration, EmulationRuntimeServices services, CancellationToken cancellationToken = default) => _engine.CreateRuntimeAsync(configuration, services, cancellationToken);

    public ValueTask<IReadOnlyList<IEmulationConfiguration>> LoadConfigurationsAsync(CancellationToken cancellationToken = default) => _engine.LoadConfigurationsAsync(cancellationToken);

    public ValueTask SaveConfigurationAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default) => _engine.SaveConfigurationAsync(configuration, cancellationToken);

    public ValueTask<EmulationEmulatorInstallation> GetEmulatorInstallationAsync(string machineId, CancellationToken cancellationToken = default) => _engine.GetEmulatorInstallationAsync(machineId, cancellationToken);

    public ValueTask<IReadOnlyList<EmulationEmulatorInstallation>> GetEmulatorInstallationsAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default) => _engine.GetEmulatorInstallationsAsync(configuration, cancellationToken);

    public ValueTask<IEmulationConfiguration> UseEmulatorAsync(IEmulationConfiguration configuration, string emulatorId, CancellationToken cancellationToken = default) => _engine.UseEmulatorAsync(configuration, emulatorId, cancellationToken);

    public ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindEmulatorReleasesAsync(string machineId, CancellationToken cancellationToken = default) => _engine.FindEmulatorReleasesAsync(machineId, cancellationToken);

    public ValueTask<string> InstallEmulatorAsync(string machineId, EmulationEmulatorRelease release, IProgress<double>? progress = null, CancellationToken cancellationToken = default) => _engine.InstallEmulatorAsync(machineId, release, progress, cancellationToken);

    public ValueTask DeleteConfigurationAsync(Guid configurationId, CancellationToken cancellationToken = default) => _engine.DeleteConfigurationAsync(configurationId, cancellationToken);
    public string GetFirmwareDirectory(string machineId) => _engine.GetFirmwareDirectory(machineId);

    public ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareAsync(string machineId, IEmulationConfiguration configuration, CancellationToken cancellationToken = default) => _engine.ScanFirmwareAsync(machineId, configuration, cancellationToken);

    public IEmulationConfiguration UseFirmware(IEmulationConfiguration configuration, EmulationFirmwareCandidate firmware) => _engine.UseFirmware(configuration, firmware);
}
