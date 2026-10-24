using CommonCoreHostFunctions = GWGUI.Emulation.Philips.Common.Functions.CoreHostFunctions;
using CommonStorageSettingsFunctions = GWGUI.Emulation.Philips.Common.Machines.Common.Functions.StorageSettingsFunctions;
using GWGUI.Emulation.Philips.Emulators.Common.Contracts;
using GWGUI.Emulation.Philips.Emulators.Common.Dictionaries;
using System.IO;

namespace GWGUI.Emulation.Philips.Emulators.Common.Factories;

internal abstract class MachineFactory(EmulatorCatalogEntry catalogEntry) : IEmulatorAdapter
{
    /// <inheritdoc/>
    public virtual void PrepareFirmware(MachineConfiguration configuration, string systemDirectory) => GWGUI.Emulation.Philips.Emulators.Common.Interop.Functions.FirmwareRuntimeFunctions.Prepare(configuration, systemDirectory);


    /// <inheritdoc/>
    public abstract IReadOnlyList<PeripheralCategory> GetPeripherals(MachineConfiguration configuration);

    /// <inheritdoc/>
    public abstract IReadOnlyList<InputActionDefinition> DescribeInput(MachineConfiguration configuration, int port, PeripheralCategory peripheral);

    /// <inheritdoc/>
    public virtual MachineConfiguration SelectConfiguration(MachineConfiguration configuration)
    {
        var core = configuration.Core;
        var options = CommonCoreHostFunctions.OptionProcessing.FilterForEmulator(CoreOptionValidationFunctions.ForCore(core).Select(option => option.Key), configuration.Options, SettingsConstants.AudioOutput, SettingsConstants.AudioLatency, VideoAudioSettingsConstants.AudioVolumeOption);
        var controllers = configuration.Input.Controllers?.Select(binding => InputSettingsFunctions.Peripherals(configuration).Contains(binding.Peripheral)
            ? binding
            : binding with { Peripheral = configuration.Family == MachineFamily.Cdi ? PeripheralCategory.CdiGamepad : PeripheralCategory.Joystick }).ToArray();
        var result = configuration with { Core = core, Options = options, Input = configuration.Input with { Controllers = controllers } };
        return result;
    }

    /// <inheritdoc/>
    public abstract ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareFileAsync(MachineConfiguration configuration, string path, CancellationToken cancellationToken);

    /// <inheritdoc/>
    public virtual EmulationStorageSettings DescribeStorage(MachineConfiguration configuration) => CommonStorageSettingsFunctions.Describe(configuration);

    /// <inheritdoc/>
    public virtual MachineConfiguration ApplyStorage(MachineConfiguration configuration, EmulationStorageSettings settings) => CommonStorageSettingsFunctions.Apply(configuration, settings);


    /// <inheritdoc/>
    public abstract EmulationInputSnapshot MapInput(EmulationInputSnapshot snapshot, MachineConfiguration configuration);

    /// <inheritdoc/>
    public abstract IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null);

    /// <inheritdoc/>
    public virtual bool SupportsPointerCapture(MachineConfiguration configuration) => false;

    public EmulatorCatalogEntry CatalogEntry { get; } = catalogEntry;
    public Emulator Emulator => CatalogEntry.Emulator;
    public string EmulatorId => CatalogEntry.Id;
    public string EmulatorKey => Emulator.ToString();
    public EmulationEmulatorDefinition Definition => CoreCatalog.GetDefinition(CatalogEntry);

    public abstract bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode);
    public abstract ValueTask<EmulationEmulatorInstallation> GetInstallationAsync(EmulatorManagementContext context, CancellationToken cancellationToken);
    public abstract ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindReleasesAsync(EmulatorManagementContext context, CancellationToken cancellationToken);
    public abstract ValueTask<string> InstallAsync(EmulatorManagementContext context, EmulationEmulatorRelease release, IProgress<double>? progress, CancellationToken cancellationToken);
    public abstract ValueTask<string?> FindInstalledCorePathAsync(EmulatorManagementContext context, CancellationToken cancellationToken);

    protected abstract IEmulatorCore CreateCore(MachineConfiguration configuration, EmulatorCreationContext context);
    public abstract bool RequiresMediaRestart(MachineConfiguration configuration, EmulationMediaSlot slot, string? path);

    public IReadOnlyList<EmulationMedia> ResolveConfiguredMedia(MachineConfiguration configuration) => configuration.Media.Select(EmulationMediaConversionFunctions.ToCommon).ToArray();

    public virtual IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => configuration.Options;

    public Machine Create(MachineConfiguration configuration, EmulatorCreationContext context)
    {
        ConfigurationFunctions.Validate(configuration);
        if (configuration.Core != Emulator || !CatalogEntry.Models.Contains(configuration.Model))
            throw new ArgumentException(nameof(configuration));
        var id = Guid.NewGuid();
        var saveDirectory = context.SaveDirectoryResolver?.Invoke(configuration);
        if (saveDirectory is not null) ((IEmulatorPersistentStorage)this).EnsurePersistentStorage(configuration, saveDirectory);
        var core = CreateCore(configuration, context);
        var completed = false;
        try
        {
            var machine = new Machine(id, configuration, core,
                Path.Combine(context.SessionsDirectory, id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat)),                 audioOutputFactory: configuration.AudioEnabled ? context.AudioOutputFactory : null,
                saveDirectory: saveDirectory,
                coreFactory: () => CreateCore(configuration, context), inputMapper: this, runtimePolicy: this);
            completed = true;
            return machine;
        }
        finally { if (!completed) core.Dispose(); }
    }

    /// <summary>Projette les indicateurs du cœur vers les emplacements de médias de la configuration.</summary>
    /// <param name="configuration">Configuration complète empruntée, donnant le modèle, l'adaptateur et les lecteurs configurés.</param>
    /// <param name="ledStates">États des LED indexés selon le protocole du cœur, consultés sans modification.</param>
    /// <returns>Activité par emplacement de média ; un résultat vide signifie qu'aucune association n'est annoncée.</returns>
    /// <remarks>Les associations conservent les conventions réelles du cœur ; un indice inconnu ne permet pas de
    /// déduire un lecteur. La consultation n'ouvre aucun média et ne possède ni périphérique ni processus.
    /// La machine emprunte cette facette à son adaptateur et l'utilise pour publier le statut commun.</remarks>
    public virtual IReadOnlyDictionary<EmulationMediaSlot, bool> MediaActivity(MachineConfiguration configuration, IReadOnlyDictionary<int, bool> ledStates) => new Dictionary<EmulationMediaSlot, bool>();
}
