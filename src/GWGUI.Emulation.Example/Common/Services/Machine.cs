using CommonControllerFunctions = GWGUI.Emulation.Example.Common.Functions.ControllerFunctions;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using GWGUI.Emulation;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Services;
using GWGUI.Emulation.Example.Common.Constants;
using GWGUI.Emulation.Example.Common.Interfaces;
using GWGUI.Emulation.Example.Common.Machines.Common.Constants;
using GWGUI.Emulation.Example.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Example.Common.Machines.Common.Enums;
using GWGUI.Emulation.Example.Common.Machines.Common.Functions;

namespace GWGUI.Emulation.Example.Common.Services;

/// <summary>Machine exposant les facettes de l’API publique et conservant les ressources de sa session.</summary>
/// <remarks>Le constructeur prépare l’état sans démarrer le cœur. StartAsync lance son thread ; DisposeAsync doit être
/// attendu pour fermer la machine. Les parties Commands et Lifecycle portent les opérations exécutées par cette façade.</remarks>
internal sealed partial class Machine : IEmulatedMachine, IEmulationLifecycle, IEmulationInput, IEmulationMedia, IEmulationVideo, IEmulationAudio, IEmulationSavedStates, IEmulationRuntime, IEmulationCassetteTransport, IEmulationControllerFeedbackSource
{
    /// <summary>Standard vidéo annoncé par le cœur courant, ou null si inconnu.</summary>
    public EmulationVideoStandard? Region => _core.Region;
    /// <summary>Trames audio en attente selon le dernier statut du cœur.</summary>
    public int BufferedAudioFrames => _core.BufferedAudioFrames;
    /// <summary>Dépassements du tampon audio signalés par le cœur.</summary>
    public long AudioOverrunCount => _core.AudioOverrunCount;
    /// <summary>Manques de données audio réellement comptés par le cœur.</summary>
    public long AudioUnderrunCount => _core.AudioUnderrunCount;

    /// <summary>Entrées actuelles de session, empruntées au propriétaire de la configuration.</summary>
    private InputConfiguration? CurrentInputConfiguration => _inputConfiguration;
    /// <summary>Autorise les retours uniquement pendant une exécution active, avant destruction et hors suspension média.</summary>
    private bool CanPublishControllerOutputs => !_disposed && State == EmulationMachineState.Running && !_mediaSuspended;

    /// <summary>Verrou des transitions d’état et de la file de commandes.</summary>
    private readonly object _gate = new();
    /// <summary>Cœur confié à cette machine, arrêté et libéré par son cycle de vie.</summary>
    private IEmulatorCore _core;
    /// <summary>Fabrique empruntée permettant de créer un cœur de remplacement après libération du précédent.</summary>
    private readonly Func<IEmulatorCore>? _coreFactory;
    /// <summary>Callback média explicite emprunté, prioritaire lorsqu’il est fourni.</summary>
    private readonly Func<EmulationMediaSlot, string?, bool>? _requiresMediaRestart;
    /// <summary>Politique empruntée du backend pour les changements de médias et d’options.</summary>
    private readonly IEmulatorRuntimePolicy? _runtimePolicy;
    /// <summary>Indique que la réinitialisation du backend exige une recréation de son cœur.</summary>
    private readonly bool _requiresResetRestart;
    /// <summary>Autorise le cœur à fonctionner sans contenu inséré lorsqu’il le prend en charge.</summary>
    private readonly bool _supportsNoContent;
    /// <summary>Restriction de capacité d’états fournie par le backend, combinée à la capacité réelle du cœur.</summary>
    private readonly bool _supportsSavedStates;
    /// <summary>Prise en charge de la capture du pointeur pour les entrées de cette session.</summary>
    private readonly bool _supportsPointerCapture;
    /// <summary>Configuration courante publiée atomiquement lors d’une modification sérialisée.</summary>
    private MachineConfiguration _configuration;
    /// <summary>Dossier temporaire de session, supprimé après destruction du cœur.</summary>
    private readonly string _sessionDirectory;
    /// <summary>Dossier des données persistantes du cœur ; sa durée de vie dépasse celle de la session.</summary>
    private readonly string? _saveDirectory;
    /// <summary>Nettoyage de session propre au backend, ou null pour la règle commune des dossiers GUID.</summary>
    private readonly Action<string>? _deleteSession;
    /// <summary>Adaptation facultative des erreurs de démarrage aux diagnostics du backend.</summary>
    private readonly Func<Exception, Exception>? _startErrorTranslator;
    /// <summary>Actions à exécuter sur le thread du cœur, avec leurs attentes d’achèvement.</summary>
    private readonly ConcurrentQueue<EmulationMachineCommand> _commands = new();
    /// <summary>Contrôleur possédé de la sortie audio, avec distinction des sorties empruntées et créées.</summary>
    private readonly AudioOutputController _audio;
    /// <summary>Accumulation des entrées reçues entre deux images du cœur.</summary>
    private readonly IEmulationInputFrames _input = CommonControllerFunctions.CreateInputFrames();
    /// <summary>Projection des entrées physiques vers les commandes du cœur, empruntée au backend Common.</summary>
    private readonly IEmulatorInputMapper? _inputMapper;
    /// <summary>Contrôleur facultatif des impulsions de cassette, sans ressource physique possédée.</summary>
    private readonly CassetteInputController? _cassetteInput;
    /// <summary>Associations courantes des entrées ; les modifications sont appliquées sur le thread du cœur.</summary>
    private InputConfiguration _inputConfiguration;
    /// <summary>État de la commande de bascule, transmis à la projection d’entrée du backend.</summary>
    private bool _controllerPointerSwitchPressed;
    /// <summary>Indique si un contrôleur commande actuellement le pointeur émulé.</summary>
    private bool _controllerPointerMode;
    /// <summary>Thread possédé de la boucle, absent avant le premier lancement.</summary>
    private Thread? _thread;
    /// <summary>Descriptions des médias actuellement montés, mises à jour après les commandes réussies.</summary>
    private readonly List<MediaConfiguration> _mountedMedia;
    /// <summary>Source possédée permettant de demander l’arrêt du thread.</summary>
    private CancellationTokenSource? _stopSource;
    /// <summary>Tâche signalant la fin de la boucle, utilisée lors de l’arrêt.</summary>
    private Task? _runLoop;
    /// <summary>Signal de réussite, d’échec ou d’annulation du démarrage.</summary>
    private TaskCompletionSource? _started;
    /// <summary>Demande de pause consultée par la boucle sous le verrou commun.</summary>
    private bool _pauseRequested;
    /// <summary>Indique que le cœur a été libéré en attendant un média de remplacement.</summary>
    private bool _mediaSuspended;
    /// <summary>Mémorise si l’insertion suivante doit reprendre une machine suspendue pour média.</summary>
    private bool _resumeAfterMediaInsert;
    /// <summary>Indicateur empêchant une nouvelle utilisation après destruction.</summary>
    private bool _disposed;

    /// <summary>Prépare une machine et sa sortie audio sans lancer l’émulation.</summary>
    /// <param name="id">Identifiant de cette instance de machine.</param>
    /// <param name="configuration">Configuration conservée pour les réglages et l’identité du modèle.</param>
    /// <param name="core">Cœur dont la machine prend en charge l’arrêt et la destruction.</param>
    /// <param name="sessionDirectory">Dossier temporaire possédé par cette session.</param>
    /// <param name="audioOutput">Sortie facultative empruntée à l’appelant.</param>
    /// <param name="saveDirectory">Dossier facultatif des données persistantes du cœur.</param>
    /// <param name="audioOutputFactory">Fabrique facultative de sorties possédées par le contrôleur.</param>
    /// <param name="coreFactory">Fabrique du même backend pour recréer le cœur après arrêt et destruction.</param>
    /// <param name="requiresMediaRestart">Callback média emprunté et prioritaire ; sans callback, la politique du backend est consultée.</param>
    /// <param name="requiresResetRestart">Demande de recréation du cœur pour une réinitialisation.</param>
    /// <param name="supportsNoContent">Capacité de poursuivre la session sans média.</param>
    /// <param name="supportsPointerCapture">Capacité supplémentaire de capture du pointeur, combinée aux périphériques déclarés.</param>
    /// <param name="supportsSavedStates">Capacité autorisée par le backend, combinée à SupportsSaveStates du cœur.</param>
    /// <param name="inputMapper">Mapper explicite ; sans mapper fourni, le contrat de l’adaptateur sélectionné est utilisé.</param>
    /// <param name="cassetteInput">Contrôleur facultatif de commandes de cassette préparé par Common.</param>
    /// <param name="resolvedMedia">Montages déjà préparés pour la session, ou null pour ceux de la configuration.</param>
    /// <param name="deleteSession">Nettoyage facultatif du dossier possédé après destruction effective du cœur.</param>
    /// <param name="startErrorTranslator">Traduction facultative d’une erreur de démarrage.</param>
    /// <param name="runtimePolicy">Politique empruntée à la fabrique ; sans politique fournie, celle de l’adaptateur sélectionné est utilisée.</param>
    /// <remarks>Les montages insérés et éjectés sont conservés dans la même liste : un média éjecté peut être
    /// réinséré sans perdre son chemin ou ses paramètres. Le volume en pourcentage est converti en gain.
    /// Les services et callbacks reçus restent empruntés ; le cœur et le contrôleur audio deviennent possédés
    /// par la machine après construction réussie. La fabrique appelante libère le cœur si ce constructeur échoue.</remarks>
    internal Machine(Guid id, MachineConfiguration configuration, IEmulatorCore core, string sessionDirectory, IAudioOutput? audioOutput = null, string? saveDirectory = null,
        Func<IAudioOutput?>? audioOutputFactory = null, Func<IEmulatorCore>? coreFactory = null, Func<EmulationMediaSlot, string?, bool>? requiresMediaRestart = null,
        bool requiresResetRestart = false, bool supportsNoContent = false, bool supportsPointerCapture = false, bool supportsSavedStates = true, IEmulatorInputMapper? inputMapper = null,
        CassetteInputController? cassetteInput = null, IReadOnlyList<MediaConfiguration>? resolvedMedia = null, Action<string>? deleteSession = null, Func<Exception, Exception>? startErrorTranslator = null, IEmulatorRuntimePolicy? runtimePolicy = null)
    {
        Id = id;
        _configuration = configuration;
        var adapter = new Engine().SelectedAdapter(configuration);
        _runtimePolicy = runtimePolicy ?? adapter;
        _requiresMediaRestart = requiresMediaRestart;
        _coreFactory = coreFactory;
        _requiresResetRestart = requiresResetRestart;
        _supportsNoContent = supportsNoContent;
        _supportsSavedStates = supportsSavedStates;
        _supportsPointerCapture = supportsPointerCapture || InputSettingsFunctions.SupportsMouse(configuration) || InputSettingsFunctions.SupportsPointerCapture(configuration.Input);
        _inputMapper = inputMapper ?? adapter;
        _cassetteInput = cassetteInput;
        _inputConfiguration = configuration.Input;
        _mountedMedia = (resolvedMedia ?? configuration.Media).ToList();
        _core = core;
        _sessionDirectory = sessionDirectory;
        _audio = new AudioOutputController(audioOutput, audioOutputFactory);
        _audio.SetMuted(!configuration.AudioEnabled);
        if (configuration.Options.TryGetValue(VideoAudioSettingsConstants.AudioVolumeOption, out var volume)
            && int.TryParse(volume, NumberStyles.Integer, CultureInfo.InvariantCulture, out var volumePercent))
            _audio.SetVolume(volumePercent / 100f);
        _saveDirectory = saveDirectory;
        _deleteSession = deleteSession;
        _startErrorTranslator = startErrorTranslator;
    }

    /// <summary>Identifiant de l’instance.</summary>
    public Guid Id { get; }
    /// <summary>Configuration conservée au moment de la création ; les médias courants sont suivis séparément.</summary>
    public MachineConfiguration Configuration { get => Volatile.Read(ref _configuration); private set => Volatile.Write(ref _configuration, value); }
    /// <summary>Facette de démarrage, pause, reprise, réinitialisation et arrêt.</summary>
    public IEmulationLifecycle Lifecycle => this;
    /// <summary>Facette de réception et de configuration des entrées.</summary>
    public IEmulationInput Input => this;
    /// <summary>Facette de montage, d’éjection et de sélection des disques.</summary>
    public IEmulationMedia Media => this;
    /// <summary>Facette de consultation et de publication des images.</summary>
    public IEmulationVideo Video => this;
    /// <summary>Facette de consultation et de commande de la sortie audio.</summary>
    public IEmulationAudio Audio => this;
    /// <summary>Facette de sauvegarde et restauration de l’état du cœur.</summary>
    public IEmulationSavedStates SavedStates => this;
    /// <summary>Facette d’informations et d’options du cœur en cours d’exécution.</summary>
    public IEmulationRuntime Runtime => this;
    /// <summary>Facette cassette lorsque Common a fourni le contrôleur correspondant, sinon null.</summary>
    public IEmulationCassetteTransport? CassetteTransport => _cassetteInput is null ? null : this;
    /// <summary>Capacité de capture annoncée pour les entrées de la session.</summary>
    bool IEmulationInput.SupportsPointerCapture => _supportsPointerCapture;
    /// <summary>Demande de capture au clic lorsque cette capacité existe et que l’utilisateur l’a activée.</summary>
    bool IEmulationInput.CapturePointerOnClick => _supportsPointerCapture && _inputConfiguration.CaptureMouse;
    /// <summary>Associations textuelles configurées, ou projection des associations typées, ou table vide.</summary>
    IReadOnlyDictionary<string, string> IEmulationInput.KeyboardBindings => _inputConfiguration.KeyboardBindings ?? _inputConfiguration.KeyboardMappings?.ToDictionary(item => item.Key, item => item.Value.ToString(), StringComparer.Ordinal) ?? new Dictionary<string, string>();
    /// <summary>Permet la bascule contrôleur/pointeur lorsqu’une entrée de pointeur est prise en charge.</summary>
    bool IEmulationInput.SupportsControllerPointerSwitch => _supportsPointerCapture;
    /// <summary>État courant de commande du pointeur par un contrôleur.</summary>
    bool IEmulationInput.ControllerPointerMode => _controllerPointerMode;
    /// <summary>État de cycle de vie, initialement Created et modifié par les opérations de la machine.</summary>
    public EmulationMachineState State { get; private set; } = EmulationMachineState.Created;
    /// <summary>Dernière image fournie par le cœur, ou null avant sa première publication.</summary>
    public VideoFrame? LatestVideoFrame => _mediaSuspended ? null : _core.LatestVideoFrame;
    /// <summary>Dernier bloc audio fourni par le cœur, ou null si aucun n’est disponible.</summary>
    public AudioChunk? LatestAudioChunk => _mediaSuspended ? null : _core.LatestAudioChunk;
    /// <summary>Options et valeurs courantes annoncées par le cœur.</summary>
    public IReadOnlyList<CoreOption> AvailableOptions => _core.Options;
    /// <summary>États des LED du cœur, indexés par leur identifiant.</summary>
    public IReadOnlyDictionary<int, bool> LedStates => _core.LedStates;
    /// <summary>Nom du cœur annoncé à l’initialisation.</summary>
    public string CoreName => _core.CoreName;
    /// <summary>Version du cœur annoncée à l’initialisation.</summary>
    public string CoreVersion => _core.CoreVersion;
    /// <summary>Extensions de contenus prises en charge par le cœur.</summary>
    public IReadOnlySet<string> SupportedContentExtensions => _core.SupportedContentExtensions;
    /// <summary>Capacité réelle de sérialisation et restauration d’état annoncée par le cœur.</summary>
    public bool SupportsSaveStates => _supportsSavedStates && !_mediaSuspended && _core.SupportsSaveStates;
    /// <summary>Indique si le contrôleur audio coupe le son.</summary>
    public bool IsAudioMuted => _audio.IsMuted;
    /// <summary>Volume courant sous forme de facteur normalisé par le contrôleur.</summary>
    public float AudioVolume => _audio.Volume;
    /// <summary>Instantané des mesures vidéo, audio et de l’état du processus hôte.</summary>
    public RuntimeStatus RuntimeStatus => RuntimeFunctions.Status(CurrentConfiguration(), _core);
    /// <summary>Notification d’une nouvelle image depuis le thread du cœur ; les buffers restent possédés par leur producteur.</summary>
    public event EventHandler<VideoFrame>? VideoFrameReady;
    /// <summary>Notification d’un bloc audio après son envoi à la sortie, depuis le thread du cœur.</summary>
    public event EventHandler<AudioChunk>? AudioChunkReady;
    /// <summary>État du transport cassette, déduit du média inséré et de l’activité du lecteur.</summary>
    /// <remarks>La présence d’un média ne signifie pas qu’une lecture est en cours ; l’activité reste une information distincte.</remarks>
    EmulationCassetteState IEmulationCassetteTransport.State => CassetteStateFunctions.From(
        _mountedMedia.Any(media => media.Category == MediaCategory.Cassette && media.IsInserted),
        ((IEmulationRuntime)this).MediaActivity.GetValueOrDefault(EmulationMediaSlot.Cassette0));
    /// <summary>Commande active lorsque le lecteur cassette annonce une lecture.</summary>
    EmulationCassetteCommand? IEmulationCassetteTransport.ActiveOperation => ((IEmulationCassetteTransport)this).State == EmulationCassetteState.Playing ? EmulationCassetteCommand.Play : null;
    /// <summary>Commandes déclarées par le contrôleur de cassette fourni à cette session.</summary>
    IReadOnlySet<EmulationCassetteCommand> IEmulationCassetteTransport.AvailableCommands => CassetteInputController.AvailableCommands;
    /// <summary>Nouvelle liste des médias montés, convertie vers les descriptions publiques.</summary>
    IReadOnlyList<EmulationMedia> IEmulationMedia.MountedMedia => _mountedMedia.Select(EmulationMediaConversionFunctions.ToCommon).OfType<EmulationMedia>().ToArray();
    /// <inheritdoc/>
    /// <remarks>Convertit le contrat public en description interne, conserve le libellé existant du slot puis passe par la file de commandes.</remarks>
    ValueTask IEmulationMedia.InsertAsync(EmulationMedia media, CancellationToken cancellationToken) => InsertMediaAsync(EmulationMediaConversionFunctions.ToModule(media, _mountedMedia), cancellationToken);
    /// <inheritdoc/>
    /// <remarks>La liste montée est mise à jour seulement après l’éjection par le cœur.</remarks>
    ValueTask IEmulationMedia.EjectAsync(EmulationMediaSlot slot, CancellationToken cancellationToken) => EjectMediaAsync(slot, cancellationToken);
    /// <inheritdoc/>
    /// <remarks>Le cœur utilise ici une sélection globale par index ; le slot du contrat n’est pas transmis à cette opération.</remarks>
    ValueTask IEmulationMedia.SelectDiskAsync(EmulationMediaSlot slot, int index, CancellationToken cancellationToken) => SelectDiskAsync(index, cancellationToken);
    /// <summary>Bloc audio courant exposé par la facette publique.</summary>
    AudioChunk? IEmulationAudio.LatestChunk => LatestAudioChunk;
    /// <summary>Fréquence d’échantillonnage du cœur, en hertz.</summary>
    int IEmulationAudio.SampleRate => _core.SampleRate;
    /// <summary>État muet du contrôleur de sortie.</summary>
    bool IEmulationAudio.IsMuted => IsAudioMuted;
    /// <summary>Facteur de volume du contrôleur de sortie.</summary>
    float IEmulationAudio.Volume => AudioVolume;
    /// <summary>Abonnements publics relayés vers la notification audio de la machine.</summary>
    event EventHandler<AudioChunk>? IEmulationAudio.ChunkReady
    {
        add => AudioChunkReady += value;
        remove => AudioChunkReady -= value;
    }
    /// <inheritdoc/>
    /// <remarks>Délègue au contrôleur de sortie ; le cœur continue de fonctionner.</remarks>
    void IEmulationAudio.SetMuted(bool muted) => SetAudioMuted(muted);
    /// <inheritdoc/>
    /// <remarks>Le contrôleur normalise le facteur de volume reçu.</remarks>
    void IEmulationAudio.SetVolume(float volume) => SetAudioVolume(volume);
    /// <inheritdoc/>
    /// <remarks>Le contrôleur détache l’ancienne sortie suivant sa propriété et utilise la nouvelle fabrique.</remarks>
    void IEmulationAudio.SetOutputFactory(Func<IAudioOutput?>? factory) => SetAudioOutputFactory(factory);
    /// <summary>Nom du cœur dans le contrat public de runtime.</summary>
    string IEmulationRuntime.EmulatorName => CoreName;
    /// <summary>Version du cœur dans le contrat public de runtime.</summary>
    string IEmulationRuntime.EmulatorVersion => CoreVersion;
    /// <summary>Extensions annoncées par le cœur dans le contrat public de runtime.</summary>
    IReadOnlySet<string> IEmulationRuntime.SupportedContentExtensions => SupportedContentExtensions;
    /// <summary>Activité des médias obtenue depuis l’instantané de statut.</summary>
    IReadOnlyDictionary<EmulationMediaSlot, bool> IEmulationRuntime.MediaActivity => RuntimeStatus.MediaActivity;
    /// <summary>Projection des options internes vers les options publiques avec libellés, valeurs et contraintes de redémarrage.</summary>
    /// <remarks>Les clés de traduction sont préférées aux textes invariants lorsqu’elles existent. La nécessité d’un
    /// redémarrage est résolue dans les réglages déclarés par l’adaptateur du cœur sélectionné.</remarks>
    IReadOnlyList<EmulationOption> IEmulationRuntime.AvailableOptions => AvailableOptions
        .Select(option => option with { Name = option.ResourceKey ?? option.Name, Description = option.ExplanationResourceKey ?? option.Description, Values = option.Values.Select(value => value with { Label = SettingsDescriptionFunctions.ChoiceLabel(value) }).ToArray(), RequiresRestart = option.RequiresRestart || _runtimePolicy?.RequiresOptionRestart(Configuration, option.Key) == true || RuntimeOptionFunctions.RequiresRestart(Configuration.Core, option.Key) })
        .ToArray();
    /// <summary>Dernière image exposée par la facette vidéo, avec la durée de validité des buffers du producteur.</summary>
    VideoFrame? IEmulationVideo.LatestFrame => LatestVideoFrame;
    /// <summary>Cadence vidéo actuelle annoncée par le cœur, en images par seconde.</summary>
    double IEmulationVideo.FramesPerSecond => _core.FramesPerSecond;
    /// <summary>Abonnements publics relayés vers la notification vidéo de la machine.</summary>
    event EventHandler<VideoFrame>? IEmulationVideo.FrameReady
    {
        add => VideoFrameReady += value;
        remove => VideoFrameReady -= value;
    }
    /// <summary>Capacité de sauvegarde d’état réellement annoncée par le cœur.</summary>
    bool IEmulationSavedStates.IsSupported => SupportsSaveStates;
    /// <inheritdoc/>
    /// <remarks>La sauvegarde est exécutée sur le thread du cœur et enveloppée dans le format d’état commun.</remarks>
    ValueTask IEmulationSavedStates.SaveAsync(string path, CancellationToken cancellationToken) => SaveStateAsync(path, cancellationToken);
    /// <inheritdoc/>
    /// <remarks>La restauration vérifie l’identité du cœur, la configuration et les empreintes des contenus avant de charger l’état.</remarks>
    ValueTask IEmulationSavedStates.LoadAsync(string path, CancellationToken cancellationToken) => LoadStateAsync(path, cancellationToken);
    /// <inheritdoc/>
    /// <remarks>Convertit le périphérique public vers sa catégorie interne puis transmet la commande au thread du cœur.</remarks>
    void IEmulationInput.SetControllerPortDevice(int port, EmulationPeripheralCategory peripheral) => SetControllerPortDevice(port, CommonControllerFunctions.FromCommon(peripheral, CurrentConfiguration(), port));
    /// <inheritdoc/>
    public HostProcessState HostProcessState => _core.HostProcessState;
    /// <inheritdoc/>
    public int? HostProcessId => _core.HostProcessId;
}
