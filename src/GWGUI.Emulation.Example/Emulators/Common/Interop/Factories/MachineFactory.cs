using CommonMediaRuntimeFunctions = GWGUI.Emulation.Example.Common.Machines.Common.Functions.MediaRuntimeFunctions;
using System.Globalization;
using System.IO;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Example.Emulators.Common.Interop.Dictionaries;
using ExternalCore = GWGUI.Emulation.Example.Emulators.Common.Interop.Services.ExternalCore;

namespace GWGUI.Emulation.Example.Emulators.Common.Interop.Factories;

/// <summary>Fabrique commune aux trois simulations, sous le même contrat d’adaptateur que les modules existants.</summary>
/// <param name="emulator">Identité typée de la voie d’exécution simulée.</param>
/// <param name="id">Identifiant public de sélection du cœur.</param>
/// <param name="name">Nom technique invariant de l’émulateur.</param>
/// <param name="descriptionResourceKey">Clé de description connue de la localisation du module.</param>
/// <remarks>Les classes Libretro, External et Internal fournissent uniquement ces métadonnées.
/// Cette fabrique réutilise les règles du Common pour les deux profils. Les fichiers utilisateur sont empruntés.
/// Une machine créée possède son cœur et sa session ; en cas d’échec de création, le finally reprend leur nettoyage.</remarks>
internal abstract class MachineFactory(Emulator emulator, string id, string name, string descriptionResourceKey) : IEmulatorAdapter, IEmulatorFirmwareAdapter
{
    /// <inheritdoc/>
    public virtual bool SupportsPointerCapture(MachineConfiguration configuration) => InputSettingsFunctions.SupportsPointerCapture(configuration.Input);

    /// <summary>Voie d’exécution choisie par cette fabrique.</summary>
    public Emulator Emulator { get; } = emulator;
    /// <inheritdoc/>
    public string EmulatorId { get; } = id;
    /// <inheritdoc/>
    public string EmulatorKey => Emulator.ToString();
    /// <summary>Description stable annonçant explicitement la compatibilité avec les deux profils fictifs.</summary>
    public EmulationEmulatorDefinition Definition { get; } = new(id, name, descriptionResourceKey, HardwareModelCatalog.All.Select(model => model.StableModelId).ToHashSet(StringComparer.Ordinal));
    /// <summary>Aucune bibliothèque native séparée : les exports simulés appartiennent à l’assembly du module.</summary>
    public string LibraryName => string.Empty;
    /// <inheritdoc/>
    public bool SupportsNoContent => true;
    /// <inheritdoc/>
    public bool SupportsInitialContent => true;
    /// <summary>Le démarrage de la simulation ne nécessite aucun firmware externe.</summary>
    public bool RequiresExternalFirmware => false;
    /// <inheritdoc/>
    public IReadOnlySet<EmulationKey> SoftResetKeys { get; } = new HashSet<EmulationKey>();
    /// <summary>Extensions déclarées par l’ensemble des lecteurs des deux profils.</summary>
    public IReadOnlyList<string> SupportedContentExtensions => HardwareModelCatalog.All.SelectMany(model => StorageSettingsFunctions.DescribeDevices(model.Model)).SelectMany(device => device.AcceptedExtensions).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

    /// <summary>Traite le mode hôte habituel pour les deux simulations hébergées dans un processus séparé.</summary>
    /// <inheritdoc/>
    /// <remarks>L’hôte crée le cœur demandé par le champ Emulator de la requête, puis le libère dans son finally.
    /// Aucun exécutable ni aucune DLL de simulation supplémentaire n’est généré.</remarks>
    public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        exitCode = 0;
        if (arguments is not [CoreHostConstants.CommandLineArgument, var pipeName, var videoMapName]) return false;
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        CoreHost.Run(pipeName, videoMapName, static (path, selected) => new ExternalCore(path, selected));
        return true;
    }

    /// <summary>Décrit la version livrée avec l’assembly, disponible sans téléchargement.</summary>
    /// <inheritdoc/>
    public ValueTask<EmulationEmulatorInstallation> GetInstallationAsync(EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(new EmulationEmulatorInstallation(Definition, Engine.CoreVersion));
    }

    /// <summary>Propose la version intégrée comme version requise de cette simulation.</summary>
    /// <inheritdoc/>
    public ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindReleasesAsync(EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult<IReadOnlyList<EmulationEmulatorRelease>>([new(EmulatorId, Definition.DisplayName + " " + Engine.CoreVersion, Engine.CoreVersion, IsRequired: true)]);
    }

    /// <summary>Résout la version intégrée demandée ; aucune archive ni DLL n’est créée.</summary>
    /// <inheritdoc/>
    public ValueTask<string> InstallAsync(EmulatorManagementContext context, EmulationEmulatorRelease release, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (release.Id != EmulatorId || release.Version != Engine.CoreVersion) throw new ArgumentOutOfRangeException(nameof(release));
        progress?.Report(1);
        return ValueTask.FromResult(Engine.CoreImplementationPath);
    }

    /// <summary>Retourne l’assembly existant qui porte la simulation, sans charger de bibliothèque native.</summary>
    /// <inheritdoc/>
    public ValueTask<string?> FindInstalledCorePathAsync(EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult<string?>(Engine.CoreImplementationPath);
    }

    /// <summary>Crée une session dont les ressources sont transférées à la machine uniquement après construction réussie.</summary>
    /// <inheritdoc/>
    /// <remarks>Libretro et External utilisent le ProcessCore et le CoreHost ordinaires ; Internal reste dans le processus appelant.
    /// Le propriétaire qui reçoit la machine attend DisposeAsync. En cas d’échec, le cœur est détruit avant
    /// la suppression du dossier GUID de session. Les sauvegardes persistantes restent en dehors de ce dossier.</remarks>
    public Machine Create(MachineConfiguration configuration, EmulatorCreationContext context)
    {
        ValidateConfiguration(configuration);
        if (configuration.Core != Emulator) throw new ArgumentException(nameof(configuration));
        var sessionId = Guid.NewGuid();
        var sessionDirectory = Path.Combine(context.SessionsDirectory, sessionId.ToString(ConfigurationStoreConstants.MachineIdentifierFormat));
        IEmulatorCore? core = null;
        var transferred = false;
        IEmulatorCore CreateCore()
        {
            if (Emulator == Emulator.Internal) return new ExternalCore(context.CorePath, Emulator);
            if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
            return new ProcessCore(context.HostExecutablePath, context.CorePath, Emulator);
        }
        try
        {
            Directory.CreateDirectory(sessionDirectory);
            ((IEmulatorFirmwareAdapter)this).PrepareFirmware(configuration, Path.Combine(sessionDirectory, CoreDirectoryConstants.SystemDirectoryName));
            var saves = context.SaveDirectoryResolver?.Invoke(configuration);
            if (saves is not null) ((IEmulatorPersistentStorage)this).EnsurePersistentStorage(configuration, saves);
            core = CreateCore();
            var machine = new Machine(sessionId, configuration, core, sessionDirectory, saveDirectory: saves,
                audioOutputFactory: context.AudioOutputFactory, coreFactory: CreateCore,                 supportsNoContent: ((IEmulatorCapabilities)this).SupportsNoContent, supportsPointerCapture: SupportsPointer(configuration), inputMapper: this,
                cassetteInput: configuration.Model == MachineModel.ExampleMachineA ? new CassetteInputController(configuration) : null, runtimePolicy: this);
            transferred = true;
            return machine;
        }
        finally
        {
            if (!transferred)
            {
                try { core?.Dispose(); }
                finally { MachineFunctions.DeleteSessionDirectory(sessionDirectory); }
            }
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<EmulationMedia> ResolveConfiguredMedia(MachineConfiguration configuration) => configuration.Media.Select(EmulationMediaConversionFunctions.ToCommon).ToArray();
    /// <summary>Les simulations consomment leurs contenus comme données opaques ; aucune conversion de format n’est nécessaire.</summary>
    /// <inheritdoc/>
    public ValueTask<EmulationMedia> PrepareMediaAsync(EmulationMedia media, string conversionDirectory) => ValueTask.FromResult(media);
    /// <inheritdoc/>
    public Task<MachineConfiguration> PrepareRuntimeConfigurationAsync(MachineConfiguration configuration, string conversionDirectory)
    {
        ValidateConfiguration(configuration);
        return Task.FromResult(configuration);
    }
    /// <inheritdoc/>
    public IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => configuration.Options;
    /// <inheritdoc/>
    public IReadOnlyDictionary<string, string> GetDefaultOptions(MachineConfiguration configuration) => OptionCatalog.All.ToDictionary(option => option.Key, option => option.DefaultValue, StringComparer.Ordinal);
    /// <summary>Contrôle les choix natifs connus et les unités des deux réglages audio communs.</summary>
    /// <inheritdoc/>
    public void ValidateOptions(MachineConfiguration configuration)
    {
        foreach (var option in OptionCatalog.All)
            if (configuration.Options.TryGetValue(option.Key, out var value) && !option.Values.Any(choice => choice.Value == value)) throw new ArgumentException(nameof(configuration));
        if (configuration.Options.TryGetValue(SettingsConstants.AudioLatency, out var latency)
            && (!int.TryParse(latency, NumberStyles.Integer, CultureInfo.InvariantCulture, out var milliseconds) || !AudioConstants.LatencyChoicesMilliseconds.Contains(milliseconds))) throw new ArgumentException(nameof(configuration));
        if (configuration.Options.TryGetValue(VideoAudioSettingsConstants.AudioVolumeOption, out var volume)
            && (!int.TryParse(volume, NumberStyles.Integer, CultureInfo.InvariantCulture, out var percent) || percent is < 0 or > 100)) throw new ArgumentException(nameof(configuration));
    }
    /// <inheritdoc/>
    public void ValidateConfiguration(MachineConfiguration configuration) => ConfigurationFunctions.Validate(configuration);
    /// <inheritdoc/>
    public MachineConfiguration ApplySettings(MachineConfiguration configuration, IReadOnlyDictionary<string, string?> values) => SettingsApplicationFunctions.Apply(configuration, values);
    /// <inheritdoc/>
    public EmulationConfigurationSummary Summarize(MachineConfiguration configuration) => ConfigurationSummaryFunctions.Create(configuration);
    /// <inheritdoc/>
    public IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null, EmulatorManagementContext? context = null) => OptionCatalog.For(configuration);
    /// <inheritdoc/>
    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null) => SettingsDescriptionFunctions.Create(configuration, GetSettingsOptions(configuration, context));
    /// <inheritdoc/>
    public IReadOnlyList<int> GetAudioLatencyChoices(MachineConfiguration configuration) => AudioConstants.LatencyChoicesMilliseconds;
    /// <inheritdoc/>
    public AudioConfiguration GetAudioConfiguration(MachineConfiguration configuration)
    {
        var output = configuration.Options.GetValueOrDefault(SettingsConstants.AudioOutput, SettingsConstants.DefaultOutput);
        var latency = configuration.Options.GetValueOrDefault(SettingsConstants.AudioLatency, AudioConstants.DefaultLatencyMilliseconds.ToString(CultureInfo.InvariantCulture));
        return new(output == SettingsConstants.DefaultOutput ? null : output, int.Parse(latency, CultureInfo.InvariantCulture));
    }
    /// <inheritdoc/>
    public bool IsCpuClock(string fieldId) => fieldId == SettingsConstants.CpuFrequency;
    /// <inheritdoc/>
    public (int Width, int Height)? GetOutputResolution(MachineConfiguration configuration) => OptionCatalog.Resolution(configuration);
    /// <inheritdoc/>
    public bool SupportsPointer(MachineConfiguration configuration) => InputSettingsFunctions.SupportsMouse(configuration);
    /// <inheritdoc/>
    public IReadOnlyList<PeripheralCategory> GetPeripherals(MachineConfiguration configuration) => Enum.GetValues<PeripheralCategory>();
    /// <inheritdoc/>
    public int GetControllerPortCount(MachineConfiguration configuration) => ConfigurationFunctions.ControllerPortCount(configuration.Model);
    /// <inheritdoc/>
    public IReadOnlyList<InputActionDefinition> DescribeInput(MachineConfiguration configuration, int port, PeripheralCategory peripheral) => InputSettingsFunctions.ControllerDefinitions(peripheral).Select(definition => new InputActionDefinition(definition)).ToArray();
    /// <inheritdoc/>
    public EmulationInputSnapshot MapInput(EmulationInputSnapshot snapshot, MachineConfiguration configuration) => InputSnapshotFunctions.Apply(snapshot, configuration.Input);
    /// <inheritdoc/>
    public IReadOnlyList<InputBindingDefinition> GetKeyboardDefinitions(MachineConfiguration configuration) => InputSettingsFunctions.KeyboardDefinitions(configuration);
    /// <inheritdoc/>
    public MachineConfiguration ApplyInputConfiguration(MachineConfiguration configuration) => configuration;
    /// <inheritdoc/>
    public IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration) => StorageSettingsFunctions.DescribeDevices(configuration.Model);
    /// <inheritdoc/>
    public EmulationStorageSettings DescribeStorage(MachineConfiguration configuration) => StorageSettingsFunctions.Describe(configuration);
    /// <inheritdoc/>
    public MachineConfiguration ApplyStorage(MachineConfiguration configuration, EmulationStorageSettings settings) => StorageSettingsFunctions.Apply(configuration, settings);
    /// <inheritdoc/>
    public MachineConfiguration ApplyStorageConfiguration(MachineConfiguration configuration, IReadOnlyList<EmulationMediaSlot> slots) => StorageSettingsFunctions.ApplyStorageConfiguration(configuration, slots);
    /// <inheritdoc/>
    public int GetMemoryCardCount(MachineConfiguration configuration) => HardwareModelCatalog.Get(configuration.Model).Media.Contains(MediaCategory.MemoryCard) ? 1 : 0;
    /// <inheritdoc/>
    public string? GetMemoryCardDisplayLabel(int index) => null;
    /// <summary>Le montage du lecteur représente l’insertion ; aucune option native concurrente ne la remplace.</summary>
    public string? MemoryCardInsertedOptionKey => null;
    /// <inheritdoc/>
    public bool HasFixedMemoryCardDirectories => false;
    /// <inheritdoc/>
    public string? GetMemoryCardDirectory(int index) => null;
    /// <inheritdoc/>
    public void EnsurePersistentStorage(MachineConfiguration configuration, string directory) => Directory.CreateDirectory(directory);
    /// <inheritdoc/>
    public bool RequiresMediaRestart(MachineConfiguration configuration, EmulationMediaSlot slot, string? path) => CommonMediaRuntimeFunctions.RequiresMediaRestart(slot);
    /// <inheritdoc/>
    public bool RequiresOptionRestart(MachineConfiguration configuration, string key) => OptionCatalog.All.Any(option => option.Key == key && option.RequiresRestart);
    /// <inheritdoc/>
    public IReadOnlyDictionary<EmulationMediaSlot, bool> MediaActivity(MachineConfiguration configuration, IReadOnlyDictionary<int, bool> ledStates) => DescribeMediaDevices(configuration).ToDictionary(device => device.Slot, device => ledStates.GetValueOrDefault(device.Slot.ProtocolValue));
    /// <inheritdoc/>
    public IReadOnlySet<string>? GetCartridgeExtensions(MachineConfiguration configuration) => HardwareModelCatalog.Get(configuration.Model).Media.Contains(MediaCategory.Cartridge) ? MediaConstants.CartridgeExtensions.ToHashSet(StringComparer.OrdinalIgnoreCase) : null;
    /// <inheritdoc/>
    public IReadOnlyList<FirmwareSlot> GetFirmwareSlots(MachineConfiguration configuration) => FirmwareRoles(configuration.Model).Select(role => new FirmwareSlot(role.ToString(), role + ".rom", "Emulation.Firmware.Rom.System", IsFirmwareRequired(configuration.Model, role))).ToArray();
    /// <inheritdoc/>
    public IReadOnlyList<FirmwareCategory> FirmwareRoles(MachineModel model) => HardwareModelCatalog.Get(model).Firmware;
    /// <summary>Le cœur fictif fonctionne sans firmware externe obligatoire, quel que soit le rôle proposé.</summary>
    /// <remarks>Un backend réel fournit ici sa règle par modèle et catégorie. Le mock conserve ses fichiers
    /// facultatifs pour illustrer leur sélection et leur préparation sans imposer un fichier externe au démarrage.</remarks>
    /// <inheritdoc/>
    public bool IsFirmwareRequired(MachineModel model, FirmwareCategory category) => false;
    /// <summary>Le catalogue de simulation ne prétend pas identifier une ROM réelle inconnue.</summary>
    /// <inheritdoc/>
    public ScannedFirmware? IdentifyFirmware(ScannedFirmware firmware) => null;
    /// <inheritdoc/>
    public bool IsFirmwareCompatible(MachineConfiguration configuration, string fieldId, string path)
    {
        var hardware = HardwareModelCatalog.Get(configuration.Model);
        if (!Enum.TryParse<FirmwareCategory>(fieldId, out var role) || !hardware.Firmware.Contains(role) || !File.Exists(path)) return false;
        var length = new FileInfo(path).Length;
        return length > 0 && length <= hardware.MainMemoryBytes;
    }
    /// <inheritdoc/>
    public async ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareAsync(MachineConfiguration configuration, string directory, CancellationToken cancellationToken)
    {
        var candidates = new List<EmulationFirmwareCandidate>();
        foreach (var path in FirmwareScanFunctions.EnumerateCandidates(directory)) candidates.AddRange(await ScanFirmwareFileAsync(configuration, path, cancellationToken).ConfigureAwait(false));
        return candidates;
    }
    /// <inheritdoc/>
    public async ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareFileAsync(MachineConfiguration configuration, string path, CancellationToken cancellationToken)
    {
        var scanned = await FirmwareScanFunctions.ScanFileAsync([this], path, configuration.Model, null, cancellationToken).ConfigureAwait(false);
        return [new(scanned.Md5 ?? scanned.Path, scanned.Path, Path.GetFileName(scanned.Path), scanned.Definition?.Version,
            scanned.Detection == FirmwareDetectionStatus.Unreadable || !IsFirmwareCompatible(configuration, FirmwareCategory.OperatingSystem.ToString(), path) ? EmulationFirmwareCompatibility.Incompatible : EmulationFirmwareCompatibility.PartiallyCompatible,
            FirmwareCategory.OperatingSystem.ToString())];
    }
    /// <summary>Copie les firmwares empruntés dans le seul dossier système de la session.</summary>
    /// <inheritdoc/>
    public void PrepareFirmware(MachineConfiguration configuration, string systemDirectory)
    {
        Directory.CreateDirectory(systemDirectory);
        foreach (var firmware in configuration.Firmwares)
        {
            if (!IsFirmwareCompatible(configuration, firmware.Category.ToString(), firmware.Path)) throw new ArgumentException(nameof(configuration));
            var destination = Path.Combine(systemDirectory, firmware.Category + ".rom");
            if (!string.Equals(Path.GetFullPath(firmware.Path), Path.GetFullPath(destination), StringComparison.OrdinalIgnoreCase)) File.Copy(firmware.Path, destination, overwrite: true);
        }
    }
}
