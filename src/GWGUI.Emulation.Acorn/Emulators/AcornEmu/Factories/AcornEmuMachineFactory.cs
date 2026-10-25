using System.IO;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Functions;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Factories;

/// <summary>Adaptateur du cœur interne, découvert par le catalogue Common sans chemin spécial dans l'application.</summary>
/// <remarks>Le cœur est livré dans l'assembly du module. La sélection et l'inspection des ROM réutilisent les facettes existantes.</remarks>
internal sealed class AcornEmuMachineFactory : IEmulatorAdapter
{
    public string EmulatorId => EmulatorConstants.Id;
    public string EmulatorKey => Emulator.AcornEmu.ToString();
    public EmulationEmulatorDefinition Definition { get; } = new(EmulatorConstants.Id, EmulatorConstants.Name, "Emulation.Emulator.acornemu.Description", new HashSet<string> { nameof(MachineModel.BbcMicroModelA) });
    public bool SupportsNoContent => true;
    private static string AssemblyPath => typeof(AcornEmuMachineFactory).Assembly.Location;
    public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode) { exitCode = 0; return false; }
    public ValueTask<EmulationEmulatorInstallation> GetInstallationAsync(EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(new EmulationEmulatorInstallation(Definition, EmulatorConstants.Version));
    }
    public ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindReleasesAsync(EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult<IReadOnlyList<EmulationEmulatorRelease>>([new(EmulatorConstants.Id, EmulatorConstants.Name, EmulatorConstants.Version, IsRequired: true)]);
    }
    public ValueTask<string> InstallAsync(EmulatorManagementContext context, EmulationEmulatorRelease release, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (release.Id != EmulatorConstants.Id || release.Version != EmulatorConstants.Version) throw new ArgumentException(nameof(release));
        progress?.Report(1);
        return ValueTask.FromResult(AssemblyPath);
    }
    public ValueTask<string?> FindInstalledCorePathAsync(EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult<string?>(AssemblyPath);
    }
    public IReadOnlyList<EmulationMedia> ResolveConfiguredMedia(MachineConfiguration configuration) => [];
    public IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => configuration.Options;
    public bool SupportsPointerCapture(MachineConfiguration configuration) => false;
    public IReadOnlySet<string>? GetCartridgeExtensions(MachineConfiguration configuration) => null;
    public EmulationConfigurationSummary Summarize(MachineConfiguration configuration) => ConfigurationSummaryFunctions.Create(configuration);
    public EmulationStorageSettings DescribeStorage(MachineConfiguration configuration) => StorageSettingsFunctions.Describe(configuration);
    public MachineConfiguration ApplyStorage(MachineConfiguration configuration, EmulationStorageSettings settings) => StorageSettingsFunctions.Apply(configuration, settings);
    public IReadOnlyDictionary<EmulationMediaSlot, bool> MediaActivity(MachineConfiguration configuration, IReadOnlyDictionary<int, bool> ledStates) => new Dictionary<EmulationMediaSlot, bool>();
    public ValueTask<EmulationMedia> PrepareMediaAsync(EmulationMedia media, string conversionDirectory) => ValueTask.FromResult(media);
    public Task<MachineConfiguration> PrepareRuntimeConfigurationAsync(MachineConfiguration configuration, string conversionDirectory) => Task.FromResult(configuration);
    public IReadOnlyList<InputBindingDefinition> GetKeyboardDefinitions(MachineConfiguration configuration) => InputSettingsFunctions.KeyboardDefinitions(configuration);
    public MachineConfiguration ApplyInputConfiguration(MachineConfiguration configuration) => configuration;

    /// <summary>Construit la session commune et lui transfère le cœur seulement après création réussie.</summary>
    public Machine Create(MachineConfiguration configuration, EmulatorCreationContext context)
    {
        if (configuration.Model != MachineModel.BbcMicroModelA || configuration.Core != Emulator.AcornEmu) throw new ArgumentException(nameof(configuration));
        var id = Guid.NewGuid();
        var core = new EmulatorCore();
        var transferred = false;
        try
        {
            var machine = new Machine(id, configuration, core, Path.Combine(context.SessionsDirectory, id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat)), audioOutputFactory: null, saveDirectory: context.SaveDirectoryResolver?.Invoke(configuration), inputMapper: this, runtimePolicy: this);
            transferred = true;
            return machine;
        }
        finally { if (!transferred) core.Dispose(); }
    }

    public IReadOnlyList<FirmwareSlot> GetFirmwareSlots(MachineConfiguration configuration) =>
    [
        new(EmulatorConstants.SystemFirmware, "", "Emulation.Firmware.Rom.System", IsRequired: true),
        new(EmulatorConstants.BasicFirmware, "", "Emulation.Firmware.Rom.Basic", IsRequired: true)
    ];
    public ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareFileAsync(MachineConfiguration configuration, string path, CancellationToken cancellationToken) => FirmwareFunctions.ScanAsync(path, cancellationToken);
    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null)
    {
        var fields = GetFirmwareSlots(configuration).Select(slot => new EmulationSettingsField(slot.FieldId, EmulationMachineTab.Rom, "Rom", slot.LabelResourceKey, EmulationSettingsEditor.Path, configuration.Options.GetValueOrDefault(slot.FieldId), DefaultFolderCategory: EmulationDefaultFolderCategory.Firmware, RequiresRestart: true)).ToArray();
        return [new("Rom", EmulationMachineTab.Rom, "Emulation.Firmware", fields)];
    }
}
