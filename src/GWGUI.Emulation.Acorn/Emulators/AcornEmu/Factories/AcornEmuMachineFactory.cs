using System.IO;
using CommonCoreHostFunctions = GWGUI.Emulation.Acorn.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Functions;
using GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Constants;
using GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Dictionaries;
using NativeModelCatalog = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Dictionaries.ModelCatalog;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Functions;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;
using CassetteConstants = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Constants.CassetteConstants;
using NativeStorage = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Functions.StorageFunctions;
using NativeStorageConstants = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.StorageConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Factories;

/// <summary>Adaptateur du cœur interne, découvert par le catalogue Common sans chemin spécial dans l'application.</summary>
/// <remarks>Le cœur est livré dans l'assembly du module. La sélection et l'inspection des ROM réutilisent les facettes existantes.</remarks>
internal sealed class AcornEmuMachineFactory : IEmulatorAdapter
{
    public string EmulatorId => EmulatorConstants.Id;
    public string EmulatorKey => Emulator.AcornEmu.ToString();
    public EmulationEmulatorDefinition Definition { get; } = new(EmulatorConstants.Id, EmulatorConstants.Name, "Emulation.Emulator.acornemu.Description", NativeModelCatalog.All.Select(model => model.Model.ToString()).ToHashSet(StringComparer.Ordinal));
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
    public IReadOnlyList<EmulationMedia> ResolveConfiguredMedia(MachineConfiguration configuration) => configuration.Media.Select(EmulationMediaConversionFunctions.ToCommon).ToArray();
    /// <summary>Annonce les seuls lecteurs explicitement raccordés, cassette et carte disque optionnelles comprises.</summary>
    public IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration) => (CassetteConnected(configuration) ? CassetteDevices(configuration) : []).Concat(NativeStorage.Devices(configuration).Where(device => NativeStorage.Connected(configuration, device.Slot.Index))).Concat(NativeStorage.HardDiskDevices(configuration).Where(device => NativeStorage.HardDiskConnected(configuration, device.Slot.Index))).ToArray();
    private static IReadOnlyList<EmulationMediaDevice> CassetteDevices(MachineConfiguration configuration) => [new(EmulationMediaSlot.Cassette0, EmulationMediaType.Cassette, CassetteConstants.Extensions, ConfigurationKind: EmulationStorageConfigurationKind.CassetteDrive, ConfigurationBlocks: EmulationCassetteSettings.DescribeBlocks(configuration.Options, [300, 1200]))];
    /// <summary>Un média déjà choisi exprime un branchement explicite ; une configuration vide n'ajoute aucun lecteur.</summary>
    private static bool CassetteConnected(MachineConfiguration configuration) => configuration.Options.TryGetValue(CassetteConstants.ConnectedOption, out var value) ? bool.Parse(value) : configuration.Media.Any(media => media.Slot == EmulationMediaSlot.Cassette0);
    public IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => configuration.Options;
    public bool SupportsPointerCapture(MachineConfiguration configuration) => false;
    public IReadOnlySet<string>? GetCartridgeExtensions(MachineConfiguration configuration) => null;
    public EmulationConfigurationSummary Summarize(MachineConfiguration configuration) => ConfigurationSummaryFunctions.Create(configuration);
    /// <summary>Propose le lecteur à Ajouter, indépendamment des seuls périphériques effectivement raccordés au runtime.</summary>
    public EmulationStorageSettings DescribeStorage(MachineConfiguration configuration) => StorageSettingsFunctions.Describe(configuration) with { AvailableDevices = CassetteDevices(configuration).Concat(NativeStorage.Devices(configuration)).Concat(NativeStorage.HardDiskDevices(configuration)).ToArray() };
    public IReadOnlyList<EmulationStorageDeviceSettings> DescribeDeviceSettings(MachineConfiguration configuration) => NativeStorage.DescribeSettings(configuration).Concat(NativeStorage.DescribeHardDiskSettings(configuration)).Concat(CassetteConnected(configuration) ? [new EmulationStorageDeviceSettings(EmulationMediaSlot.Cassette0, Options: configuration.Options.Where(pair => pair.Key.StartsWith(EmulationCassetteSettings.Prefix, StringComparison.Ordinal)).ToDictionary())] : []).ToArray();
    public MachineConfiguration ApplyDeviceSettings(MachineConfiguration configuration, IReadOnlyList<EmulationStorageDeviceSettings> settings) => NativeStorage.ApplySettings(configuration, settings);
    public bool RequiresOptionRestart(MachineConfiguration configuration, string key) => !NativeStorage.IsMechanicalSoundOption(key) && (key.StartsWith(NativeStorageConstants.FloppyPrefix, StringComparison.Ordinal) || key.StartsWith(NativeStorageConstants.HardDiskPrefix, StringComparison.Ordinal) || key.StartsWith(NetworkConstants.Prefix, StringComparison.Ordinal) || key == NetworkConstants.Firmware || key == NativeStorageConstants.DfsFirmware);
    /// <summary>Conserve le branchement même sans cassette et retire le lecteur lorsque l'utilisateur le supprime.</summary>
    public MachineConfiguration ApplyStorage(MachineConfiguration configuration, EmulationStorageSettings settings)
    {
        var options = new Dictionary<string, string>(configuration.Options, StringComparer.Ordinal) { [CassetteConstants.ConnectedOption] = settings.ConfiguredSlots.Contains(EmulationMediaSlot.Cassette0).ToString() };
        foreach (var pair in settings.DeviceSettings?.FirstOrDefault(device => device.Slot == EmulationMediaSlot.Cassette0)?.Options ?? new Dictionary<string, string>())
            if (pair.Key.StartsWith(EmulationCassetteSettings.Prefix, StringComparison.Ordinal)) options[pair.Key] = pair.Value;
        return StorageSettingsFunctions.Apply(NativeStorage.SelectSlots(configuration with { Options = options }, settings.ConfiguredSlots), settings);
    }
    public IReadOnlyDictionary<EmulationMediaSlot, bool> MediaActivity(MachineConfiguration configuration, IReadOnlyDictionary<int, bool> ledStates)
    {
        var activity = CommonCoreHostFunctions.MachineExecution.FromLedStates(ledStates).ToDictionary();
        activity[EmulationMediaSlot.Floppy1] = ledStates.GetValueOrDefault(NativeStorageConstants.SecondDriveLed);
        for (var unit = 0; unit < NativeStorageConstants.HardDiskCount; unit++) activity[new(EmulationMediaCategory.HardDisk, unit)] = ledStates.GetValueOrDefault(NativeStorageConstants.HardDiskLedStart + unit);
        return activity;
    }
    public ValueTask<EmulationMedia> PrepareMediaAsync(EmulationMedia media, string conversionDirectory) => ValueTask.FromResult(media);
    public Task<MachineConfiguration> PrepareRuntimeConfigurationAsync(MachineConfiguration configuration, string conversionDirectory) => Task.FromResult(configuration);
    public IReadOnlyList<InputBindingDefinition> GetKeyboardDefinitions(MachineConfiguration configuration) => InputSettingsFunctions.KeyboardDefinitions(configuration);
    public MachineConfiguration ApplyInputConfiguration(MachineConfiguration configuration) => configuration;

    /// <summary>Annonce et valide la RAM réellement allouée par l'implémentation du modèle.</summary>
    public decimal? GetMemoryKib(MachineConfiguration configuration) => BbcMicroMemoryFunctions.ModelABytes(configuration) / (decimal)SettingsConstants.Kibibyte;
    public void ValidateOptions(MachineConfiguration configuration)
    {
        NativeStorage.Validate(configuration);
        _ = NetworkFunctions.Read(configuration);
        if (EmulationCassetteSettings.Read(configuration.Options).BaudRate is not (0 or 300 or 1200)) throw new ArgumentOutOfRangeException(nameof(configuration));
        _ = BbcMicroMemoryFunctions.ModelABytes(configuration);
        if (configuration.Options.TryGetValue(CassetteConstants.ConnectedOption, out var connected) && !bool.TryParse(connected, out _)) throw new ArgumentException(ErrorMessages.OptionValueInvalidFormat, nameof(configuration));
        if (!EmulatorConstants.VideoOutputs.Contains(configuration.Options.GetValueOrDefault(EmulatorConstants.VideoOutput, EmulatorConstants.UhfOutput), StringComparer.Ordinal)) throw new ArgumentException(ErrorMessages.OptionValueInvalidFormat, nameof(configuration));
    }

    /// <summary>Construit la session commune et lui transfère le cœur seulement après création réussie.</summary>
    public Machine Create(MachineConfiguration configuration, EmulatorCreationContext context)
    {
        if (configuration.Core != Emulator.AcornEmu) throw new ArgumentException(nameof(configuration));
        var id = Guid.NewGuid();
        var core = new EmulatorCore(configuration.Model);
        var transferred = false;
        try
        {
            var machine = new Machine(id, configuration, core, Path.Combine(context.SessionsDirectory, id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat)), audioOutputFactory: context.AudioOutputFactory, saveDirectory: context.SaveDirectoryResolver?.Invoke(configuration), inputMapper: this, runtimePolicy: this);
            transferred = true;
            return machine;
        }
        finally { if (!transferred) core.Dispose(); }
    }

    public IReadOnlyList<FirmwareSlot> GetFirmwareSlots(MachineConfiguration configuration) => BbcMicroModelCatalog.All.Single(model => model.Model == configuration.Model).Firmware
        .Where(category => FirmwareFunctions.Field(category) is not null)
        .Select(category => new FirmwareSlot(FirmwareFunctions.Field(category)!, "", category switch { FirmwareCategory.OperatingSystem => "Emulation.Firmware.Rom.System", FirmwareCategory.CharacterGenerator => "Emulation.Acorn.Firmware.CharacterGenerator", _ => "Emulation.Firmware.Rom.Basic" }, IsRequired: true, MachineIds: new HashSet<string> { configuration.Model.ToString() }))
        .Concat(NativeStorage.NeedsDiskRom(configuration) ? [new FirmwareSlot(NativeStorageConstants.DfsFirmware, "", "Emulation.Acorn.Firmware.Dfs", IsRequired: true, MachineIds: new HashSet<string> { configuration.Model.ToString() })] : [])
        .Concat(NetworkFunctions.Read(configuration).Enabled ? [new FirmwareSlot(NetworkConstants.Firmware, "", "Emulation.Acorn.Network.Rom", IsRequired: true, MachineIds: new HashSet<string> { configuration.Model.ToString() })] : []).ToArray();
    public ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareFileAsync(MachineConfiguration configuration, string path, CancellationToken cancellationToken) => BbcMicroFirmwareFunctions.ScanAsync(configuration.Model, path, FirmwareFunctions.Field, cancellationToken);
    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null)
    {
        var fields = GetFirmwareSlots(configuration).Select(slot => new EmulationSettingsField(slot.FieldId, EmulationMachineTab.Rom, "Rom", slot.LabelResourceKey, EmulationSettingsEditor.Path, configuration.Options.GetValueOrDefault(slot.FieldId), DefaultFolderCategory: EmulationDefaultFolderCategory.Firmware, RequiresRestart: true)).ToArray();
        return
        [
            NetworkFunctions.Describe(configuration),
            new(SettingsConstants.MainMemory, EmulationMachineTab.Ram, SettingsConstants.ResourceMemory,
                [new(SettingsConstants.MainMemory, EmulationMachineTab.Ram, SettingsConstants.MainMemory, SettingsConstants.ResourceMemory, EmulationSettingsEditor.Selection,
                    configuration.Options.GetValueOrDefault(SettingsConstants.MainMemory, BbcMicroModelAConstants.StandardMemoryKib),
                    [new(BbcMicroModelAConstants.StandardMemoryKib, string.Empty, "16 KiB"), new(BbcMicroModelAConstants.ExpandedMemoryKib, string.Empty, "32 KiB")], RequiresRestart: true, RefreshSettingsOnChange: true)]),
            new("Rom", EmulationMachineTab.Rom, "Emulation.Firmware", fields),
            new(EmulatorConstants.VideoOutput, EmulationMachineTab.Video, "Emulation.Acorn.Video.Output",
                [new(EmulatorConstants.VideoOutput, EmulationMachineTab.Video, EmulatorConstants.VideoOutput, "Emulation.Acorn.Video.Output", EmulationSettingsEditor.Selection,
                    configuration.Options.GetValueOrDefault(EmulatorConstants.VideoOutput, EmulatorConstants.UhfOutput),
                    [new(EmulatorConstants.UhfOutput, "Emulation.Acorn.Video.Output.Uhf"), new(EmulatorConstants.MonochromeOutput, "Emulation.Acorn.Video.Output.Monochrome"), new(EmulatorConstants.ColourCompositeOutput, "Emulation.Acorn.Video.Output.ColourComposite"), new(EmulatorConstants.RgbOutput, "Emulation.Acorn.Video.Output.Rgb")])])
        ];
    }
}
