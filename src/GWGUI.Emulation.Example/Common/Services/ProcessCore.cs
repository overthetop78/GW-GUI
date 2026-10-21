using System.IO;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO.MemoryMappedFiles;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Runtime.Versioning;
using GWGUI.Emulation;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Example.Common.Constants;
using GWGUI.Emulation.Example.Common.Enums;
using GWGUI.Emulation.Example.Common.Exceptions;
using GWGUI.Emulation.Example.Common.Functions;
using GWGUI.Emulation.Example.Common.Interfaces;
using GWGUI.Emulation.Example.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Example.Common.Machines.Common.Enums;
using GWGUI.Emulation.Example.Common.Machines.Common.Functions;

namespace GWGUI.Emulation.Example.Common.Services;

/// <summary>Cœur déporté exposant IEmulatorCore par un processus hôte et son protocole de communication.</summary>
/// <remarks>Le constructeur ne lance rien. Initialize crée le tube et le processus ; les pixels passent par mémoire partagée,
/// l’audio et les commandes par le tube. Dispose ferme les transports, attend la sortie du processus et libère les segments vidéo.</remarks>
[SupportedOSPlatform(CoreHostConstants.Windows)]
internal sealed partial class ProcessCore : IEmulatorCore
{
    /// <summary>Chemin absolu de l’exécutable lancé comme hôte du cœur.</summary>
    private readonly string _hostExecutablePath;
    /// <summary>Argument identifiant le mode hôte à activer dans l’exécutable.</summary>
    private readonly string _hostCommand;
    /// <summary>Chemin absolu de la bibliothèque du cœur, transmis au processus enfant.</summary>
    private readonly string _corePath;
    /// <summary>Identifiant typé de l’émulateur confié au processus enfant.</summary>
    private readonly Emulator _emulator;
    /// <summary>Durée maximale d’attente d’une réponse au protocole.</summary>
    private readonly TimeSpan _responseTimeout;
    /// <summary>Durée maximale d’attente de connexion du processus enfant au tube.</summary>
    private readonly TimeSpan _connectionTimeout;
    /// <summary>Annulation commune à la connexion et aux requêtes de cette instance.</summary>
    private readonly CancellationToken _cancellationToken;
    /// <summary>Blocs audio reçus et pas encore consommés par la machine.</summary>
    private readonly ConcurrentQueue<AudioChunk> _audio = new();
    /// <summary>Entrées accumulées puis consommées lors de la requête d’image suivante.</summary>
    private readonly IEmulationInputFrames _input = EmulationSdk.CreateInputFrames();
    /// <summary>Verrou possédé sérialisant les requêtes et leurs réponses sur le tube unique.</summary>
    private readonly SemaphoreSlim _requestGate = new(CoreHostConstants.MaximumPipeInstances, CoreHostConstants.MaximumPipeInstances);
    /// <summary>Tube bidirectionnel possédé, limité à l’utilisateur courant.</summary>
    private NamedPipeServerStream? _pipe;
    /// <summary>Rédacteur possédé du tube ; sa fermeture laisse le tube à son propriétaire.</summary>
    private BinaryWriter? _writer;
    /// <summary>Processus enfant possédé dont la sortie doit être attendue avant libération du handle.</summary>
    private Process? _process;
    /// <summary>Handle possédé du segment vidéo actuellement ouvert.</summary>
    private MemoryMappedFile? _videoMemory;
    /// <summary>Vue possédée permettant de lire les pixels du segment vidéo.</summary>
    private MemoryMappedViewAccessor? _videoMap;
    /// <summary>Indique que l’initialisation distante a été reçue avec succès.</summary>
    private bool _initialized;
    /// <summary>Indique une rupture définitive de communication pour cette instance.</summary>
    private bool _connectionFailed;
    /// <summary>Indique que les ressources de l’instance ont été libérées.</summary>
    private bool _disposed;
    /// <summary>Identifiant du processus enfant mémorisé après son lancement.</summary>
    private int? _hostProcessId;
    /// <summary>Nom unique du tube de la session, défini lors de l’initialisation.</summary>
    private string? _pipeName;
    /// <summary>Préfixe initial des segments vidéo de cette session.</summary>
    private string? _videoMapName;
    /// <summary>Nom du segment vidéo effectivement ouvert, pouvant changer lorsque l’image grandit.</summary>
    private string? _activeVideoMapName;

    /// <summary>Prépare une connexion de cœur déporté sans démarrer de processus.</summary>
    /// <param name="hostExecutablePath">Chemin de l’exécutable hôte, normalisé en chemin absolu.</param>
    /// <param name="corePath">Chemin de la bibliothèque du cœur, normalisé en chemin absolu.</param>
    /// <param name="emulator">Identifiant de l’émulateur à demander à l’hôte.</param>
    /// <param name="responseTimeout">Délai facultatif des réponses ; sinon valeur en secondes des constantes du protocole.</param>
    /// <param name="cancellationToken">Annulation des opérations de communication.</param>
    /// <param name="connectionTimeout">Délai facultatif de connexion ; sinon valeur en millisecondes des constantes.</param>
    /// <param name="hostCommand">Argument du mode hôte utilisé au lancement.</param>
    /// <exception cref="ArgumentException">Un chemin ou l’argument de commande est vide, blanc ou invalide.</exception>
    internal ProcessCore(string hostExecutablePath, string corePath, Emulator emulator,
        TimeSpan? responseTimeout = null, CancellationToken cancellationToken = default,
        TimeSpan? connectionTimeout = null, string hostCommand = CoreHostConstants.CommandLineArgument)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hostExecutablePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(corePath);
        _hostExecutablePath = Path.GetFullPath(hostExecutablePath);
        _corePath = Path.GetFullPath(corePath);
        _emulator = emulator;
        ArgumentException.ThrowIfNullOrWhiteSpace(hostCommand);
        _hostCommand = hostCommand;
        _responseTimeout = responseTimeout ?? TimeSpan.FromSeconds(CoreHostConstants.ResponseTimeoutSeconds);
        _cancellationToken = cancellationToken;
        _connectionTimeout = connectionTimeout ?? TimeSpan.FromMilliseconds(CoreHostConstants.ConnectionTimeoutMilliseconds);
    }

    /// <summary>Identifiant de l’émulateur sélectionné.</summary>
    public Emulator Emulator => _emulator;
    /// <summary>Dernière image reçue, conservée lorsqu’une réponse n’en contient pas de nouvelle.</summary>
    public VideoFrame? LatestVideoFrame { get; private set; }
    /// <summary>Dernier bloc audio reçu du processus enfant.</summary>
    public AudioChunk? LatestAudioChunk { get; private set; }
    /// <summary>Options reçues pendant l’initialisation distante.</summary>
    public IReadOnlyList<CoreOption> Options { get; private set; } = [];
    /// <summary>Dernière liste de diagnostics transmise par l’hôte.</summary>
    public IReadOnlyList<string> Diagnostics { get; private set; } = [];
    /// <summary>Derniers états des LED reçus, indexés par leur identifiant.</summary>
    public IReadOnlyDictionary<int, bool> LedStates { get; private set; } = new Dictionary<int, bool>();
    /// <summary>Nom du cœur annoncé par l’hôte ; vide avant initialisation.</summary>
    public string CoreName { get; private set; } = string.Empty;
    /// <summary>Version du cœur annoncée par l’hôte ; vide avant initialisation.</summary>
    public string CoreVersion { get; private set; } = string.Empty;
    /// <summary>Empreinte SHA-256 de la bibliothèque chargée par l’hôte.</summary>
    public string CoreSha256 { get; private set; } = string.Empty;
    /// <summary>Extensions acceptées reçues de l’hôte, comparées sans distinction de casse.</summary>
    public IReadOnlySet<string> SupportedContentExtensions { get; private set; } =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    /// <summary>Capacité de sauvegarde d’état déclarée pendant l’initialisation.</summary>
    public bool SupportsSaveStates { get; private set; }
    /// <summary>Statut du tampon physique fourni par la machine avant la prochaine requête d’image.</summary>
    /// <remarks>Ce statut ne possède aucune sortie audio ; ses nombres de frames et sa fréquence en Hz
    /// sont transmis au cœur distant avec un booléen indiquant la présence du bloc.</remarks>
    public AudioOutputBufferStatus? OutputAudioStatus { get; set; }
    /// <summary>Latence audio minimale en millisecondes reçue à l’initialisation puis après chaque frame.</summary>
    public int MinimumAudioLatencyMilliseconds { get; private set; }
    /// <summary>Demande de fermeture annoncée par le cœur distant, traitée par la boucle de la machine.</summary>
    public bool ShutdownRequested { get; private set; }
    /// <summary>Dernier instantané des retours de contrôleurs annoncé par le cœur distant.</summary>
    /// <remarks>La machine applique ses associations de sortie avant de transmettre ces valeurs à l’hôte.
    /// Une liste vide arrête les effets précédents ; elle ne signifie pas que le transport doit répéter son dernier instantané.</remarks>
    public IReadOnlyList<ControllerFeedbackState> ControllerFeedback { get; private set; } = [];
    /// <summary>Cadence du cœur en images par seconde, actualisée dans les réponses d’image.</summary>
    public double FramesPerSecond { get; private set; }
    /// <summary>Fréquence audio en hertz, actualisée dans les réponses d’image.</summary>
    public int SampleRate { get; private set; }
    /// <summary>Standard vidéo annoncé, ou null si le code du protocole n’est pas reconnu.</summary>
    public EmulationVideoStandard? Region { get; private set; }
    /// <summary>Nombre de frames audio restant dans le tampon du cœur distant.</summary>
    public int BufferedAudioFrames { get; private set; }
    /// <summary>Compteur de dépassements du tampon audio annoncé par le cœur.</summary>
    public long AudioOverrunCount { get; private set; }
    /// <summary>Compteur de manques de données audio annoncé par le cœur.</summary>
    public long AudioUnderrunCount { get; private set; }
    /// <summary>État du processus calculé à partir du handle, de la connexion et de la destruction.</summary>
    public HostProcessState HostProcessState => _disposed && _hostProcessId.HasValue
        ? _connectionFailed ? HostProcessState.Faulted : HostProcessState.Exited
        : RuntimeFunctions.ProcessState(_process, _connectionFailed, _disposed);
    /// <summary>Identifiant du processus hôte mémorisé, ou null avant lancement.</summary>
    public int? HostProcessId => _hostProcessId;
    /// <summary>Nom du tube créé, disponible pour le diagnostic de session.</summary>
    internal string? PipeName => _pipeName;
    /// <summary>Nom du segment vidéo actif, ou préfixe initial avant ouverture de la première image.</summary>
    internal string? VideoMapName => _activeVideoMapName ?? _videoMapName;

    /// <summary>Retire le prochain bloc audio de la file reçue.</summary>
    /// <param name="chunk">Bloc retiré, ou null si la file est vide.</param>
    /// <returns>Vrai lorsqu’un bloc a été retiré.</returns>
    public bool TryDequeueAudio(out AudioChunk? chunk)
    => _audio.TryDequeue(out chunk);

    /// <summary>Crée le transport, lance l’hôte et attend sa réponse d’initialisation.</summary>
    /// <param name="configuration">Configuration sérialisée et transmise au cœur distant.</param>
    /// <param name="sessionDirectory">Répertoire de session à utiliser dans l’hôte.</param>
    /// <param name="saveDirectory">Répertoire facultatif des données persistantes du cœur.</param>
    /// <exception cref="ObjectDisposedException">Cette instance a été détruite.</exception>
    /// <exception cref="InvalidOperationException">Elle a déjà été initialisée ou la communication échoue.</exception>
    /// <exception cref="FileNotFoundException">L’exécutable hôte est absent.</exception>
    /// <remarks>Les noms du tube et du segment vidéo sont uniques. Si une étape échoue, le finally appelle Dispose
    /// pour fermer les ressources déjà acquises et attendre la fin du processus.</remarks>
    public void Initialize(MachineConfiguration configuration, string sessionDirectory, string? saveDirectory = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_initialized) throw new InvalidOperationException(CoreHostErrors.AlreadyInitialized);
        if (!File.Exists(_hostExecutablePath))
            throw new FileNotFoundException(CoreHostErrors.ExecutableMissing, _hostExecutablePath);

        var pipeName = CoreHostFunctions.CreatePipeName();
        var videoMapName = CoreHostFunctions.CreateVideoMapName();
        _pipeName = pipeName;
        _videoMapName = videoMapName;
        try
        {
            _pipe = new NamedPipeServerStream(pipeName, PipeDirection.InOut,
                CoreHostConstants.MaximumPipeInstances, PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly,
                CoreHostConstants.PipeBufferSize, CoreHostConstants.PipeBufferSize);
            _process = StartHostProcess(pipeName, videoMapName);
            _hostProcessId = _process.Id;
            EmulationSdk.HostProtocol.WaitForConnection(_pipe, _connectionTimeout, _cancellationToken);
            _writer = new BinaryWriter(_pipe, Encoding.UTF8, leaveOpen: true);
            Request(HostCommand.Initialize, writer => EmulationSdk.HostProtocol.WriteInitializationRequest(writer, _corePath, (int)_emulator,
                sessionDirectory, saveDirectory, configuration, CoreHostFunctions.JsonOptions), ReadInitialization, allowUninitialized: true);
            _initialized = true;
        }
        finally
        {
            if (!_initialized) Dispose();
        }
    }

    /// <summary>Transmet les entrées accumulées et le statut de sortie audio, puis lit les sorties d’une frame distante.</summary>
    /// <remarks>Le service d’entrée fige les mouvements une seule fois par requête. Les entrées sont suivies
    /// du booléen de présence du tampon audio, puis, si présent, de fréquence, remplissage, capacité et activité.
    /// CoreHost lit ces champs dans le même ordre avant d’exécuter le cœur.</remarks>
    public void RunFrame() => Request(HostCommand.RunFrame, writer =>
    {
        _input.Poll();
        CoreHostFunctions.WriteInput(writer, _input.Polled);
        var status = OutputAudioStatus;
        writer.Write(status.HasValue);
        if (status is { } buffer)
        {
            writer.Write(buffer.SampleRate);
            writer.Write(buffer.BufferedFrames);
            writer.Write(buffer.CapacityFrames);
            writer.Write(buffer.IsActive);
        }
    }, ReadFrame);

    /// <summary>Transmet la commande de réinitialisation matérielle et attend sa réponse.</summary>
    public void HardReset() => Request(HostCommand.HardReset);
    /// <summary>Demande la réinitialisation logicielle par la commande propre à cette opération.</summary>
    /// <remarks>Le serveur appelle SoftReset du cœur ; cette commande n’est pas remplacée par HardReset.</remarks>
    public void SoftReset() => Request(HostCommand.SoftReset);
    /// <summary>Demande l’arrêt du cœur si la connexion est encore initialisée, disponible et non détruite.</summary>
    public void Stop()
    {
        if (_initialized && !_disposed && !_connectionFailed) Request(HostCommand.Stop);
    }
    /// <summary>Accumule les entrées pour la prochaine requête d’image.</summary>
    /// <param name="snapshot">Instantané reçu de la machine.</param>
    public void SetInput(EmulationInputSnapshot snapshot) => _input.Update(snapshot);
    /// <summary>Transmet le périphérique à connecter sur un port du cœur distant.</summary>
    /// <param name="port">Index du port.</param>
    /// <param name="peripheral">Catégorie sérialisée sous sa valeur entière du protocole.</param>
    public void SetControllerPortDevice(int port, PeripheralCategory peripheral) =>
        Request(HostCommand.SetControllerPortDevice, writer =>
        {
            writer.Write(port);
            writer.Write((int)peripheral);
        });
    /// <summary>Transmet une insertion de média sérialisée avec les options JSON du protocole.</summary>
    /// <param name="media">Description du média à insérer.</param>
    public void InsertMedia(MediaConfiguration media) => Request(HostCommand.InsertMedia, writer => writer.Write(JsonSerializer.Serialize(media, CoreHostFunctions.JsonOptions)));
    /// <summary>Demande l’éjection du média d’un emplacement.</summary>
    /// <param name="slot">Emplacement transmis sous sa valeur de protocole.</param>
    public void EjectMedia(EmulationMediaSlot slot) => Request(HostCommand.EjectMedia, writer => writer.Write(slot.ProtocolValue));
    /// <summary>Demande la sélection d’un disque dans le cœur.</summary>
    /// <param name="index">Index transmis sans modification.</param>
    public void SelectDisk(int index) => Request(HostCommand.SelectDisk, writer => writer.Write(index));
    /// <summary>Demande au cœur d’enregistrer les changements du média indiqué.</summary>
    /// <param name="slot">Emplacement du média dont les changements doivent être enregistrés.</param>
    public void SaveMediaChanges(EmulationMediaSlot slot) => Request(HostCommand.SaveMediaChanges, writer => writer.Write(slot.ProtocolValue));
    /// <summary>Interroge le statut des disques du cœur distant.</summary>
    /// <returns>Statut décodé dans la réponse.</returns>
    /// <exception cref="InvalidDataException">Aucun statut n’a été reçu.</exception>
    public DiskStatus GetDiskStatus()
    {
        DiskStatus? status = null;
        Request(HostCommand.GetDiskStatus, read: reader => status = CoreHostFunctions.ReadDiskStatus(reader));
        return status ?? throw new InvalidDataException(CoreHostErrors.CommunicationFailed);
    }
    /// <summary>Interroge la présence de changements de média non enregistrés.</summary>
    /// <param name="slot">Emplacement concerné.</param>
    /// <returns>Valeur booléenne fournie par le cœur distant.</returns>
    public bool HasUnsavedMediaChanges(EmulationMediaSlot slot)
    {
        var hasChanges = false;
        Request(HostCommand.HasUnsavedMediaChanges, writer => writer.Write(slot.ProtocolValue), reader => hasChanges = reader.ReadBoolean());
        return hasChanges;
    }
    /// <summary>Demande la sérialisation de l’état binaire du cœur distant.</summary>
    /// <returns>Nouveau tableau d’octets reçu, appartenant à l’appelant.</returns>
    /// <exception cref="InvalidDataException">La réponse ne fournit aucun état.</exception>
    /// <remarks>L’enveloppe de fichier et la validation de compatibilité sont assurées par Machine, au-dessus de ce transport.</remarks>
    public byte[] SaveState()
    {
        byte[]? state = null;
        Request(HostCommand.SaveState, read: reader => state = CoreHostFunctions.ReadBytes(reader));
        return state ?? throw new InvalidDataException(CoreHostErrors.CommunicationFailed);
    }
    /// <summary>Transmet un état binaire au cœur distant pour restauration.</summary>
    /// <param name="state">Octets empruntés, copiés avant leur capture par le rédacteur de requête.</param>
    public void LoadState(ReadOnlySpan<byte> state)
    {
        var copy = state.ToArray();
        Request(HostCommand.LoadState, writer => CoreHostFunctions.WriteBytes(writer, copy));
    }
    /// <summary>Transmet une valeur d’option au cœur distant.</summary>
    /// <param name="key">Identifiant textuel de l’option.</param>
    /// <param name="value">Valeur textuelle attendue par le cœur.</param>
    public void SetOption(string key, string value) => Request(HostCommand.SetOption, writer =>
    {
        writer.Write(key);
        writer.Write(value);
    });
}
