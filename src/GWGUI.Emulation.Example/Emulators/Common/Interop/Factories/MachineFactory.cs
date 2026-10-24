using CoreReleaseService = GWGUI.Emulation.Example.Common.Services.CoreReleaseService;
using CoreReleaseFunctions = GWGUI.Emulation.Example.Common.Functions.CoreReleaseFunctions;
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
internal abstract class MachineFactory(Emulator emulator, string id, string name, string descriptionResourceKey) : IEmulatorAdapter, IEmulatorRuntimePolicy, IEmulatorSettings, IEmulatorFirmwareAdapter
{
    /// <summary>Consulte la configuration des entrées pour déterminer si le pointeur peut être capturé.</summary>
    /// <param name="configuration">Configuration complète empruntée : profil matériel, backend, options, entrées et montages courants.</param>
    /// <returns>True lorsque les associations de cette configuration autorisent la capture.</returns>
    /// <remarks><para>La décision passe par les réglages Common et peut varier selon les périphériques sélectionnés ; elle ne capture pas le pointeur elle-même.</para></remarks>
    public virtual bool SupportsPointerCapture(MachineConfiguration configuration) => InputSettingsFunctions.SupportsPointerCapture(configuration.Input);

    /// <summary>Voie d’exécution choisie par cette fabrique.</summary>
    public Emulator Emulator { get; } = emulator;
    /// <summary>Fournit l&apos;identifiant stable de l&apos;adaptateur transmis à sa construction.</summary>
    /// <remarks><para>Le catalogue et la configuration utilisent cette identité pour sélectionner la fabrique ; elle est distincte du nom affiché.</para></remarks>
    public string EmulatorId { get; } = id;
    /// <summary>Fournit la représentation textuelle de l&apos;identité typée du backend.</summary>
    /// <remarks><para>Cette clé permet au catalogue Common de retrouver la valeur de son énumération sans déduire le backend depuis son libellé.</para></remarks>
    public string EmulatorKey => Emulator.ToString();
    /// <summary>Description stable annonçant explicitement la compatibilité avec les deux profils fictifs.</summary>
    public EmulationEmulatorDefinition Definition { get; } = new(id, name, descriptionResourceKey, HardwareModelCatalog.All.Select(model => model.StableModelId).ToHashSet(StringComparer.Ordinal));
    /// <summary>Aucune bibliothèque native séparée : les exports simulés appartiennent à l’assembly du module.</summary>
    public string LibraryName => string.Empty;
    /// <summary>Autorise l&apos;initialisation du cœur simulé sans média inséré.</summary>
    /// <remarks><para>Common utilise cette capacité lors d&apos;un démarrage ou d&apos;une recréation après éjection ; un lecteur vide reste un état de session valide.</para></remarks>
    public bool SupportsNoContent => true;
    /// <summary>Autorise le contrat de préparation du contenu initial.</summary>
    /// <remarks><para>La fabrique expose ce même point d&apos;interconnexion pour les trois simulations ; la résolution des chemins reste une opération distincte.</para></remarks>
    public bool SupportsInitialContent => true;
    /// <summary>Décrit les touches à maintenir pendant une réinitialisation logicielle.</summary>
    /// <remarks><para>L&apos;ensemble vide sélectionne la commande native commune sans combinaison de touches supplémentaire ; il ne représente pas un clavier physique possédé.</para></remarks>
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

    /// <summary>Fournit le contrat Common des versions avec le catalogue existant du module.</summary>
    /// <remarks>Chaque service est léger et sans ressource possédée ; il reçoit les descriptions par une fonction,
    /// sans connaître cette classe de backend. Les appels ci-dessous passent par cette interface.</remarks>
    private static ICoreReleaseService Releases => new CoreReleaseService(EmulatorCatalog.Get);

    /// <summary>Décrit la version livrée avec l’assembly, disponible sans téléchargement.</summary>
    /// <inheritdoc/>
    public async ValueTask<EmulationEmulatorInstallation> GetInstallationAsync(EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        var installation = await Releases.GetActiveInstallationAsync(Emulator, cancellationToken).ConfigureAwait(false);
        return new EmulationEmulatorInstallation(Definition, installation?.InstalledVersion);
    }

    /// <summary>Propose la version intégrée comme version requise de cette simulation.</summary>
    /// <inheritdoc/>
    public async ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindReleasesAsync(EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        var releases = await Releases.GetAvailableAsync(Emulator, cancellationToken).ConfigureAwait(false);
        return releases.Select(release => new EmulationEmulatorRelease(release.Id, Definition.DisplayName + " " + release.DeclaredVersion, release.DeclaredVersion, IsRequired: true)).ToArray();
    }

    /// <summary>Résout la version intégrée demandée ; aucune archive ni DLL n’est créée.</summary>
    /// <inheritdoc/>
    public async ValueTask<string> InstallAsync(EmulatorManagementContext context, EmulationEmulatorRelease release, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        var available = await Releases.GetAvailableAsync(Emulator, cancellationToken).ConfigureAwait(false);
        var selected = available.Single() with { Id = release.Id, DeclaredVersion = release.Version };
        // Common produit les compteurs typés ; le relais commun présente leur fraction au contrat public.
        // La notification suit le contexte de progression de l'appelant, même pour une installation intégrée immédiate.
        var adapter = CoreReleaseFunctions.Releases.CreateInstallProgress<CoreInstallProgress>(progress, static value => value.Fraction);
        var installation = await Releases.InstallAsync(selected, adapter, cancellationToken).ConfigureAwait(false);
        return installation.LibraryPath;
    }

    /// <summary>Retourne l’assembly existant qui porte la simulation, sans charger de bibliothèque native.</summary>
    /// <inheritdoc/>
    public async ValueTask<string?> FindInstalledCorePathAsync(EmulatorManagementContext context, CancellationToken cancellationToken) =>
        await new CoreProvider(Releases).FindInstalledPathAsync(Emulator, cancellationToken).ConfigureAwait(false);

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

    /// <summary>Convertit les montages du module en descriptions de médias publiques.</summary>
    /// <param name="configuration">Configuration complète empruntée : profil matériel, backend, options, entrées et montages courants.</param>
    /// <returns>Nouveau tableau de descriptions publiques dans l&apos;ordre des montages configurés.</returns>
    /// <remarks><para>Chaque description conserve l&apos;emplacement et le chemin du montage. La conversion ne lit ni ne modifie les fichiers associés.</para></remarks>
    public IReadOnlyList<EmulationMedia> ResolveConfiguredMedia(MachineConfiguration configuration) => configuration.Media.Select(EmulationMediaConversionFunctions.ToCommon).ToArray();
    /// <summary>Les simulations consomment leurs contenus comme données opaques ; aucune conversion de format n’est nécessaire.</summary>
    /// <inheritdoc/>
    public ValueTask<EmulationMedia> PrepareMediaAsync(EmulationMedia media, string conversionDirectory) => ValueTask.FromResult(media);
    /// <summary>Valide la configuration avant sa remise au runtime simulé.</summary>
    /// <param name="configuration">Configuration complète empruntée : profil matériel, backend, options, entrées et montages courants.</param>
    /// <param name="conversionDirectory">Dossier temporaire prévu pour les conversions ; le mock valide seulement la configuration et n&apos;y crée aucun fichier.</param>
    /// <returns>Tâche déjà terminée contenant la même configuration après sa validation.</returns>
    /// <remarks><para>Le mock conserve ici la configuration telle quelle : il n&apos;effectue aucune conversion de fichier et n&apos;utilise pas le dossier temporaire reçu. Les erreurs de validation sont propagées.</para></remarks>
    public Task<MachineConfiguration> PrepareRuntimeConfigurationAsync(MachineConfiguration configuration, string conversionDirectory)
    {
        ValidateConfiguration(configuration);
        return Task.FromResult(configuration);
    }
    /// <summary>Fournit les options courantes à transmettre au cœur.</summary>
    /// <param name="configuration">Configuration complète empruntée : profil matériel, backend, options, entrées et montages courants.</param>
    /// <returns>Dictionnaire courant des clés et valeurs d&apos;options.</returns>
    /// <remarks><para>Le dictionnaire appartient à la configuration ; le consommateur le consulte sans le modifier et sans reconstruire ses valeurs par défaut.</para></remarks>
    public IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => configuration.Options;
    /// <summary>Construit les valeurs initiales depuis le catalogue d&apos;options du module.</summary>
    /// <param name="configuration">Configuration complète empruntée : profil matériel, backend, options, entrées et montages courants.</param>
    /// <returns>Nouveau dictionnaire associant chaque clé d&apos;option à sa valeur initiale.</returns>
    /// <remarks><para>Les identifiants utilisent une comparaison ordinale. Le dictionnaire créé appartient à l&apos;appelant et ne modifie pas le catalogue partagé.</para></remarks>
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
    /// <summary>Applique les contrôles Common à la configuration de machine.</summary>
    /// <param name="configuration">Configuration complète empruntée : profil matériel, backend, options, entrées et montages courants.</param>
    /// <remarks><para>Cette étape consulte les données sans démarrer de cœur. Une configuration invalide conserve les erreurs explicites du validateur.</para></remarks>
    public void ValidateConfiguration(MachineConfiguration configuration) => ConfigurationFunctions.Validate(configuration);
    /// <summary>Confie les valeurs d&apos;éditeurs au traitement Common des réglages.</summary>
    /// <param name="configuration">Configuration complète empruntée : profil matériel, backend, options, entrées et montages courants.</param>
    /// <param name="values">Valeurs proposées par les éditeurs, indexées par identifiant de champ ; la collection reste empruntée.</param>
    /// <returns>Configuration résultant de l&apos;application des champs proposés.</returns>
    /// <remarks><para>Le traitement retourne la configuration résultante au lieu de modifier les collections de l&apos;appelant. Il conserve la séparation entre réglages de machine et options du cœur.</para></remarks>
    public MachineConfiguration ApplySettings(MachineConfiguration configuration, IReadOnlyDictionary<string, string?> values) => SettingsApplicationFunctions.Apply(configuration, values);
    /// <summary>Construit le résumé de configuration présenté par l&apos;hôte.</summary>
    /// <param name="configuration">Configuration complète empruntée : profil matériel, backend, options, entrées et montages courants.</param>
    /// <returns>Résumé public des données matérielles et du cœur sélectionné.</returns>
    /// <remarks><para>Common résout le profil matériel et l&apos;identité du backend ; la fabrique ne crée ni vue ni libellé de contrôle propre à l&apos;application.</para></remarks>
    public virtual EmulationConfigurationSummary Summarize(MachineConfiguration configuration) => ConfigurationSummaryFunctions.Create(configuration);
    /// <summary>Consulte le catalogue d&apos;options correspondant à la configuration.</summary>
    /// <param name="configuration">Configuration complète empruntée : profil matériel, backend, options, entrées et montages courants.</param>
    /// <param name="context">Contexte de gestion emprunté avec ses services et chemins ; null pour une consultation sans contexte d&apos;installation.</param>
    /// <returns>Descriptions des options applicables, conservées par le catalogue.</returns>
    /// <remarks><para>Sans configuration, le catalogue fournit sa description générale. Cette consultation ne recherche ni ne télécharge d&apos;installation.</para></remarks>
    public IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null, EmulatorManagementContext? context = null) => OptionCatalog.For(configuration);
    /// <summary>Décrit les blocs d&apos;éditeurs à partir des réglages Common et des options applicables.</summary>
    /// <param name="configuration">Configuration complète empruntée : profil matériel, backend, options, entrées et montages courants.</param>
    /// <param name="context">Contexte de gestion emprunté avec ses services et chemins ; null pour une consultation sans contexte d&apos;installation.</param>
    /// <returns>Blocs de champs, choix et aides de la configuration.</returns>
    /// <remarks><para>Le résultat transporte des données de présentation ; la création des contrôles visuels reste dans l&apos;application.</para></remarks>
    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null) => SettingsDescriptionFunctions.Create(configuration, GetSettingsOptions(configuration, context));
    /// <summary>Fournit les latences proposées par le module, en millisecondes.</summary>
    /// <param name="configuration">Configuration complète empruntée : profil matériel, backend, options, entrées et montages courants.</param>
    /// <returns>Choix de latence en millisecondes.</returns>
    /// <remarks><para>La liste est partagée en lecture seule ; le consommateur choisit une valeur sans modifier ce catalogue.</para></remarks>
    public IReadOnlyList<int> GetAudioLatencyChoices(MachineConfiguration configuration) => AudioConstants.LatencyChoicesMilliseconds;
    /// <summary>Résout le périphérique de sortie et la latence depuis les options courantes.</summary>
    /// <param name="configuration">Configuration complète empruntée : profil matériel, backend, options, entrées et montages courants.</param>
    /// <returns>Description de sortie audio et de latence en millisecondes.</returns>
    /// <remarks><para>La valeur désignant la sortie par défaut est convertie en null. La latence est lue avec la culture invariante ; cette opération n&apos;ouvre aucun périphérique.</para></remarks>
    public AudioConfiguration GetAudioConfiguration(MachineConfiguration configuration)
    {
        var output = configuration.Options.GetValueOrDefault(SettingsConstants.AudioOutput, SettingsConstants.DefaultOutput);
        var latency = configuration.Options.GetValueOrDefault(SettingsConstants.AudioLatency, AudioConstants.DefaultLatencyMilliseconds.ToString(CultureInfo.InvariantCulture));
        return new(output == SettingsConstants.DefaultOutput ? null : output, int.Parse(latency, CultureInfo.InvariantCulture));
    }
    /// <summary>Identifie le champ Common représentant la fréquence du CPU.</summary>
    /// <param name="fieldId">Identifiant technique du champ de réglage ou de firmware, distinct de son libellé traduit.</param>
    /// <returns>True pour le champ de fréquence CPU du module.</returns>
    /// <remarks><para>La comparaison porte sur l&apos;identifiant technique du champ, et non sur son libellé traduit.</para></remarks>
    public bool IsCpuClock(string fieldId) => fieldId == SettingsConstants.CpuFrequency;
    /// <summary>Consulte la résolution de sortie décrite par les options.</summary>
    /// <param name="configuration">Configuration complète empruntée : profil matériel, backend, options, entrées et montages courants.</param>
    /// <returns>Dimensions configurées ou absence de résolution explicite selon le catalogue.</returns>
    /// <remarks><para>Les dimensions sont exprimées en pixels. Le résultat ne possède ni fenêtre ni surface graphique.</para></remarks>
    public (int Width, int Height)? GetOutputResolution(MachineConfiguration configuration) => OptionCatalog.Resolution(configuration);
    /// <summary>Consulte les réglages Common pour savoir si une souris est configurée.</summary>
    /// <param name="configuration">Configuration complète empruntée : profil matériel, backend, options, entrées et montages courants.</param>
    /// <returns>True si la configuration prévoit les entrées de souris prises en charge.</returns>
    /// <remarks><para>La capacité déclarée dépend des associations et du modèle de la session ; elle ne lit aucun périphérique physique.</para></remarks>
    public bool SupportsPointer(MachineConfiguration configuration) => InputSettingsFunctions.SupportsMouse(configuration);
    /// <summary>Expose les catégories de périphériques définies par le module d&apos;exemple.</summary>
    /// <param name="configuration">Configuration complète empruntée : profil matériel, backend, options, entrées et montages courants.</param>
    /// <returns>Nouveau tableau des catégories déclarées dans l&apos;énumération.</returns>
    /// <remarks><para>Les catégories génériques regroupent les commandes disponibles sans multiplier les modèles de périphériques physiques.</para></remarks>
    public IReadOnlyList<PeripheralCategory> GetPeripherals(MachineConfiguration configuration) => Enum.GetValues<PeripheralCategory>();
    /// <summary>Résout le nombre de ports effectif depuis la configuration Common.</summary>
    /// <param name="configuration">Configuration complète empruntée : profil matériel, backend, options, entrées et montages courants.</param>
    /// <returns>Nombre de ports utilisables dans cette configuration.</returns>
    /// <remarks><para>Le résultat comprend l&apos;extension de ports lorsqu&apos;elle est activée ; il ne dépend pas du nombre de périphériques physiques connectés.</para></remarks>
    public int GetControllerPortCount(MachineConfiguration configuration) => ConfigurationFunctions.ControllerPortCount(configuration);
    /// <summary>Décrit les commandes d&apos;une catégorie de périphérique pour un port.</summary>
    /// <param name="configuration">Configuration complète empruntée : profil matériel, backend, options, entrées et montages courants.</param>
    /// <param name="port">Index du port d&apos;entrée à configurer, validé par les règles Common de la session.</param>
    /// <param name="peripheral">Catégorie de périphérique à décrire ou à appliquer au port.</param>
    /// <returns>Nouveau tableau d&apos;actions et de définitions d&apos;associations.</returns>
    /// <remarks><para>Les définitions génériques sont projetées vers le contrat d&apos;actions Common. Les images et les zones cliquables appartiennent à l&apos;application.</para></remarks>
    public IReadOnlyList<InputActionDefinition> DescribeInput(MachineConfiguration configuration, int port, PeripheralCategory peripheral) => InputSettingsFunctions.ControllerDefinitions(peripheral).Select(definition => new InputActionDefinition(definition)).ToArray();
    /// <summary>Projette les entrées physiques vers les commandes attendues par le cœur simulé.</summary>
    /// <param name="snapshot">Instantané physique emprunté, dont les collections ne sont pas modifiées par la projection.</param>
    /// <param name="configuration">Configuration complète empruntée : profil matériel, backend, options, entrées et montages courants.</param>
    /// <returns>Instantané projeté selon les associations de la configuration.</returns>
    /// <remarks><para>La projection Common applique les associations de clavier, de souris et de ports. Les collections physiques reçues restent empruntées et ne sont pas modifiées.</para></remarks>
    public EmulationInputSnapshot MapInput(EmulationInputSnapshot snapshot, MachineConfiguration configuration) => InputSnapshotFunctions.Apply(snapshot, configuration.Input);
    /// <summary>Expose les commandes de clavier configurables par le contrat Common.</summary>
    /// <param name="configuration">Configuration complète empruntée : profil matériel, backend, options, entrées et montages courants.</param>
    /// <returns>Définitions des commandes du clavier correspondant à la configuration.</returns>
    /// <remarks><para>Les identifiants de touches servent aux associations ; la fabrique ne construit pas de clavier visuel et ne capture pas les événements de l&apos;hôte.</para></remarks>
    public IReadOnlyList<InputBindingDefinition> GetKeyboardDefinitions(MachineConfiguration configuration) => InputSettingsFunctions.KeyboardDefinitions(configuration);
    /// <summary>Accepte la configuration d&apos;entrées préparée par Common.</summary>
    /// <param name="configuration">Configuration complète empruntée : profil matériel, backend, options, entrées et montages courants.</param>
    /// <returns>Même instance de configuration.</returns>
    /// <remarks><para>Le backend simulé n&apos;ajoute aucune contrainte supplémentaire à cette étape ; l&apos;instance et ses collections sont conservées.</para></remarks>
    public MachineConfiguration ApplyInputConfiguration(MachineConfiguration configuration) => configuration;
    /// <summary>Décrit les lecteurs et supports du profil matériel choisi.</summary>
    /// <param name="configuration">Configuration complète empruntée : profil matériel, backend, options, entrées et montages courants.</param>
    /// <returns>Descriptions Common des périphériques de médias du modèle.</returns>
    /// <remarks><para>Les descriptions annoncent les catégories, emplacements et extensions acceptées ; elles ne montent aucun fichier.</para></remarks>
    public IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration) => StorageSettingsFunctions.DescribeDevices(configuration.Model);
    /// <summary>Consulte les réglages de stockage de la configuration.</summary>
    /// <param name="configuration">Configuration complète empruntée : profil matériel, backend, options, entrées et montages courants.</param>
    /// <returns>Description publique des réglages de stockage courants.</returns>
    /// <remarks><para>Les données sont projetées vers le contrat public pour être présentées et modifiées par l&apos;hôte, sans ouvrir les supports.</para></remarks>
    public EmulationStorageSettings DescribeStorage(MachineConfiguration configuration) => StorageSettingsFunctions.Describe(configuration);
    /// <summary>Applique une description de stockage par le traitement Common.</summary>
    /// <param name="configuration">Configuration complète empruntée : profil matériel, backend, options, entrées et montages courants.</param>
    /// <param name="settings">Réglages publics proposés par l&apos;hôte, consultés sans modifier les collections de l&apos;appelant.</param>
    /// <returns>Configuration résultant des réglages de stockage proposés.</returns>
    /// <remarks><para>Les règles du profil matériel sont conservées et la source reste sous la responsabilité de son appelant.</para></remarks>
    public MachineConfiguration ApplyStorage(MachineConfiguration configuration, EmulationStorageSettings settings) => StorageSettingsFunctions.Apply(configuration, settings);
    /// <summary>Applique la liste des emplacements de stockage configurés.</summary>
    /// <param name="configuration">Configuration complète empruntée : profil matériel, backend, options, entrées et montages courants.</param>
    /// <param name="slots">Liste des emplacements de stockage à conserver dans la configuration.</param>
    /// <returns>Configuration portant les emplacements retenus.</returns>
    /// <remarks><para>La résolution des lecteurs et des catégories reste dans Common. Cet appel prépare une configuration et ne commande pas une machine en cours d&apos;exécution.</para></remarks>
    public MachineConfiguration ApplyStorageConfiguration(MachineConfiguration configuration, IReadOnlyList<EmulationMediaSlot> slots) => StorageSettingsFunctions.ApplyStorageConfiguration(configuration, slots);
    /// <summary>Décrit les réglages particuliers de chaque périphérique de stockage.</summary>
    /// <param name="configuration">Configuration complète empruntée : profil matériel, backend, options, entrées et montages courants.</param>
    /// <returns>Réglages publics des périphériques de stockage de la configuration.</returns>
    /// <remarks><para>Le résultat reste une description gérée : aucun fichier de disque ou de carte mémoire n&apos;est ouvert par la fabrique.</para><para>Les trois simulations utilisent le même propriétaire Common des paramètres persistants de lecteurs.</para></remarks>
    public IReadOnlyList<EmulationStorageDeviceSettings> DescribeDeviceSettings(MachineConfiguration configuration) => StorageSettingsFunctions.DescribeDeviceSettings(configuration);
    /// <summary>Applique les réglages de périphériques par leur traitement Common.</summary>
    /// <param name="configuration">Configuration complète empruntée : profil matériel, backend, options, entrées et montages courants.</param>
    /// <param name="settings">Réglages publics proposés par l&apos;hôte, consultés sans modifier les collections de l&apos;appelant.</param>
    /// <returns>Configuration résultant des réglages des périphériques.</returns>
    /// <remarks><para>Les données proposées sont interprétées selon leurs catégories et emplacements ; la fabrique ne crée pas de seconde logique de stockage.</para><para>Common valide les associations de catégories et copie les options ; aucun média n’est ouvert ici.</para></remarks>
    public MachineConfiguration ApplyDeviceSettings(MachineConfiguration configuration, IReadOnlyList<EmulationStorageDeviceSettings> settings) => StorageSettingsFunctions.ApplyDeviceSettings(configuration, settings);
    /// <summary>Détermine le nombre de cartes mémoire du profil matériel.</summary>
    /// <param name="configuration">Configuration complète empruntée : profil matériel, backend, options, entrées et montages courants.</param>
    /// <returns>Un si le matériel accepte une carte mémoire, sinon zéro.</returns>
    /// <remarks><para>Le profil disposant de cette catégorie expose une carte ; le profil dépourvu de carte retourne zéro.</para></remarks>
    public int GetMemoryCardCount(MachineConfiguration configuration) => HardwareModelCatalog.Get(configuration.Model).Media.Contains(MediaCategory.MemoryCard) ? 1 : 0;
    /// <summary>Laisse le contrat Common choisir le libellé générique de la carte mémoire.</summary>
    /// <param name="index">Index de l&apos;emplacement de carte demandé ; le mock ne fournit pas de valeur particulière par index.</param>
    /// <returns>Null pour utiliser le libellé commun.</returns>
    /// <remarks><para>Le mock ne fournit pas de nom spécifique par emplacement et ne résout aucune ressource de l&apos;application.</para></remarks>
    public string? GetMemoryCardDisplayLabel(int index) => null;
    /// <summary>Le montage du lecteur représente l’insertion ; aucune option native concurrente ne la remplace.</summary>
    public string? MemoryCardInsertedOptionKey => null;
    /// <summary>Annonce que le mock n&apos;impose aucun dossier fixe aux cartes mémoire.</summary>
    /// <remarks><para>L&apos;hôte et la configuration conservent le choix des emplacements ; cette capacité ne supprime aucun fichier existant.</para></remarks>
    public bool HasFixedMemoryCardDirectories => false;
    /// <summary>Laisse la configuration choisir le dossier de carte mémoire.</summary>
    /// <param name="index">Index de l&apos;emplacement de carte demandé ; le mock ne fournit pas de valeur particulière par index.</param>
    /// <returns>Null, car aucun dossier fixe n&apos;est déclaré.</returns>
    /// <remarks><para>Aucun chemin matériel propre au backend simulé n&apos;est imposé par cette opération.</para></remarks>
    public string? GetMemoryCardDirectory(int index) => null;
    /// <summary>Prépare le dossier persistant utilisé par le cœur.</summary>
    /// <param name="configuration">Configuration complète empruntée : profil matériel, backend, options, entrées et montages courants.</param>
    /// <param name="directory">Dossier persistant à préparer ; il reste distinct des fichiers temporaires de session.</param>
    /// <remarks><para>Le dossier existe après cet appel. Sa durée de vie dépasse celle de la session : il ne doit pas être supprimé avec les fichiers temporaires d&apos;émulation.</para></remarks>
    public void EnsurePersistentStorage(MachineConfiguration configuration, string directory) => Directory.CreateDirectory(directory);
    /// <summary>Consulte la politique Common de recréation du cœur lors d&apos;un changement de média.</summary>
    /// <param name="configuration">Configuration complète empruntée : profil matériel, backend, options, entrées et montages courants.</param>
    /// <param name="slot">Emplacement de média désigné par le contrat public.</param>
    /// <param name="path">Chemin du fichier à consulter ou à modifier selon l&apos;opération ; son propriétaire reste l&apos;appelant.</param>
    /// <returns>True lorsque la politique de cet emplacement demande un redémarrage.</returns>
    /// <remarks><para>La décision dépend de l&apos;emplacement ; la recréation éventuelle et la propriété du cœur restent gérées par la machine Common.</para></remarks>
    public bool RequiresMediaRestart(MachineConfiguration configuration, EmulationMediaSlot slot, string? path) => CommonMediaRuntimeFunctions.RequiresMediaRestart(slot);
    /// <summary>Consulte les métadonnées de l&apos;option pour décider d&apos;un redémarrage.</summary>
    /// <param name="configuration">Configuration complète empruntée : profil matériel, backend, options, entrées et montages courants.</param>
    /// <param name="key">Identifiant technique de l&apos;option du cœur à consulter.</param>
    /// <returns>True pour une option connue dont les métadonnées demandent une recréation.</returns>
    /// <remarks><para>La recherche porte sur la clé technique et conserve la valeur RequiresRestart du catalogue ; elle ne relance rien elle-même.</para></remarks>
    public bool RequiresOptionRestart(MachineConfiguration configuration, string key) => OptionCatalog.All.Any(option => option.Key == key && option.RequiresRestart);
    /// <summary>Associe l&apos;activité annoncée par le cœur aux emplacements de médias décrits.</summary>
    /// <param name="configuration">Configuration complète empruntée : profil matériel, backend, options, entrées et montages courants.</param>
    /// <param name="ledStates">Instantané emprunté des indicateurs d&apos;activité, indexés par identifiant de protocole.</param>
    /// <returns>Nouveau dictionnaire d&apos;activité, indexé par emplacement de média.</returns>
    /// <remarks><para>Chaque emplacement utilise son identifiant de protocole pour consulter les LED ; une indication absente est interprétée comme inactive.</para></remarks>
    public virtual IReadOnlyDictionary<EmulationMediaSlot, bool> MediaActivity(MachineConfiguration configuration, IReadOnlyDictionary<int, bool> ledStates) => DescribeMediaDevices(configuration).ToDictionary(device => device.Slot, device => ledStates.GetValueOrDefault(device.Slot.ProtocolValue));
    /// <summary>Expose les extensions de cartouches lorsque le profil possède ce support.</summary>
    /// <param name="configuration">Configuration complète empruntée : profil matériel, backend, options, entrées et montages courants.</param>
    /// <returns>Ensemble des extensions acceptées, ou null pour un profil sans cartouche.</returns>
    /// <remarks><para>Les extensions sont copiées dans un ensemble insensible à la casse. Le résultat ne signifie pas qu&apos;un fichier est déjà monté.</para></remarks>
    public IReadOnlySet<string>? GetCartridgeExtensions(MachineConfiguration configuration) => HardwareModelCatalog.Get(configuration.Model).Media.Contains(MediaCategory.Cartridge) ? MediaConstants.CartridgeExtensions.ToHashSet(StringComparer.OrdinalIgnoreCase) : null;
    /// <summary>Décrit les rôles du profil comme emplacements de firmware facultatifs.</summary>
    /// <param name="configuration">Configuration complète dont le profil matériel fournit les rôles proposés.</param>
    /// <returns>Champs stables, noms de fichiers et libellés utilisés par la sélection et la préparation.</returns>
    /// <remarks>La simulation fonctionne sans fichier externe obligatoire. Un backend réel décrit ses
    /// exigences individuelles avec IsRequired et peut préciser une obligation globale entre alternatives
    /// par RequiresExternalFirmware. Les règles ne sont pas répétées dans un second catalogue de rôles.</remarks>
    public IReadOnlyList<FirmwareSlot> GetFirmwareSlots(MachineConfiguration configuration) => FirmwareRuntimeFunctions.GetFirmwareSlots(configuration);
    /// <summary>Vérifie la compatibilité d&apos;un fichier avec le champ de firmware demandé.</summary>
    /// <param name="configuration">Configuration complète empruntée : profil matériel, backend, options, entrées et montages courants.</param>
    /// <param name="fieldId">Identifiant technique du champ de réglage ou de firmware, distinct de son libellé traduit.</param>
    /// <param name="path">Chemin du fichier de firmware à inspecter ; l&apos;inspection ne transfère pas sa propriété au cœur.</param>
    /// <returns>True si le fichier est compatible avec le champ de destination.</returns>
    /// <remarks><para>Le traitement Common inspecte les contraintes du profil ; le résultat ne copie ni n&apos;installe le firmware dans une session.</para></remarks>
    public bool IsFirmwareCompatible(MachineConfiguration configuration, string fieldId, string path) => FirmwareRuntimeFunctions.IsCompatible(configuration, fieldId, path);
    /// <summary>Inspecte les fichiers candidats d&apos;un dossier par le contrat de firmware.</summary>
    /// <param name="configuration">Configuration complète empruntée : profil matériel, backend, options, entrées et montages courants.</param>
    /// <param name="directory">Dossier source dont les fichiers candidats doivent être inspectés, sans modification de leur contenu.</param>
    /// <param name="cancellationToken">Annulation transmise à l&apos;opération Common, sans transfert de propriété des ressources de session.</param>
    /// <returns>Liste des candidats et de leurs champs de destination possibles.</returns>
    /// <remarks><para>Chaque candidat passe par l&apos;inspection de fichier Common et ses résultats sont réunis. L&apos;annulation est transmise aux inspections ; aucun cœur n&apos;est démarré.</para></remarks>
    public async ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareAsync(MachineConfiguration configuration, string directory, CancellationToken cancellationToken)
    {
        var candidates = new List<EmulationFirmwareCandidate>();
        foreach (var path in FirmwareScanFunctions.EnumerateCandidates(directory)) candidates.AddRange(await ScanFirmwareFileAsync(configuration, path, cancellationToken).ConfigureAwait(false));
        return candidates;
    }
    /// <summary>Confie l&apos;inspection d&apos;un fichier de firmware au traitement Common.</summary>
    /// <param name="configuration">Configuration complète empruntée : profil matériel, backend, options, entrées et montages courants.</param>
    /// <param name="path">Chemin du fichier de firmware à inspecter ; l&apos;inspection ne transfère pas sa propriété au cœur.</param>
    /// <param name="cancellationToken">Annulation transmise à l&apos;opération Common, sans transfert de propriété des ressources de session.</param>
    /// <returns>Attente des observations et propositions de destination pour ce fichier.</returns>
    /// <remarks><para>La fabrique fournit ses règles d&apos;identification et de compatibilité par les interfaces existantes, sans conserver de copie du fichier.</para></remarks>
    public ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareFileAsync(MachineConfiguration configuration, string path, CancellationToken cancellationToken) => FirmwareRuntimeFunctions.ScanFileAsync([this], configuration, path, cancellationToken);
    /// <summary>Copie les firmwares empruntés dans le seul dossier système de la session.</summary>
    /// <inheritdoc/>
    public void PrepareFirmware(MachineConfiguration configuration, string systemDirectory) => FirmwareRuntimeFunctions.Prepare(configuration, systemDirectory);
}
