using System.IO;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Example.Common.Functions;
using GWGUI.Emulation.Example.Emulators.Common.Interop.Dictionaries;
using SimulationConstants = GWGUI.Emulation.Example.Emulators.Common.Interop.Constants.CoreConstants;

namespace GWGUI.Emulation.Example.Emulators.Common.Interop.Services;

/// <summary>Cœur fictif partagé par les trois adaptateurs, possédé et exécuté par la machine Common.</summary>
/// <remarks>Le mode interne appelle directement le calcul. Le mode Libretro passe par des exports et des
/// callbacks typés selon l’ABI existante. Le mode externe appelle les mêmes exports mockés puis lit leurs
/// sorties. Aucun mode ne charge ni ne génère de bibliothèque native. Les médias restent empruntés.
/// La machine sérialise les commandes et appelle Dispose dans son finally après arrêt effectif de la boucle.</remarks>
internal sealed partial class ExternalCore : IEmulatorCore
{
    /// <inheritdoc/>
    public int DiskCount => GetDiskStatus().ImageCount;
    /// <inheritdoc/>
    public int CurrentDiskIndex => GetDiskStatus().CurrentIndex;
    /// <inheritdoc/>
    public bool SupportsPointerCapture => true;

    /// <summary>Table d’exports gérés conservée vivante pendant toute la session simulée.</summary>
    private readonly ExternalCoreExports _exports;
    /// <summary>Contrat descriptif Common qui adapte les extensions ; cette instance ne possède aucune ressource de cœur.</summary>
    private readonly IEmulatorContentFormats _contentFormats;
    /// <summary>Tampons vidéo possédés ; les frames publiées en empruntent les tableaux tournants.</summary>
    private readonly VideoBufferSet _video = new();
    /// <summary>File audio indépendante obtenue par le relais Common, avec les compteurs du SDK.</summary>
    private readonly IEmulationAudioBuffer _audio = AudioFunctions.CreateBuffer();
    /// <summary>Stockage des effets possédé par le cœur, créé via Common et fermé après le détachement des callbacks.</summary>
    /// <remarks>Ce service conserve seulement des intensités. Les périphériques physiques restent possédés par l’hôte.</remarks>
    private IEmulationControllerFeedbackAccumulator? _feedback;
    /// <summary>Messages du cœur conservés comme métadonnées, y compris après une destruction sur échec.</summary>
    /// <remarks>Cette liste ne possède aucune ressource native. Le destinataire reçoit toujours une copie.</remarks>
    private readonly List<string> _diagnostics = [];
    /// <summary>Surveillance obtenue par l’interface publique du SDK via le Common.</summary>
    /// <remarks>Les trois modes utilisent le même parcours. Le mock ne déclare aucune signature fatale par défaut ;
    /// un adaptateur réel transmet ici les signatures de son profil, sans modifier ni enregistrer le module dans le SDK.
    /// Capture ne lève pas depuis un callback ; ThrowIfFailed est appelé après retour au code géré.</remarks>
    private readonly IEmulationCoreDiagnosticMonitor _diagnosticMonitor = CoreHostFunctions.Messages.CreateCoreDiagnosticMonitor([], static diagnostic => new InvalidOperationException(diagnostic));
    /// <summary>Configuration courante, remplacée lors d’un changement d’option.</summary>
    private MachineConfiguration? _configuration;
    /// <summary>RAM de la machine ; son adresse reste stable entre initialisation et destruction.</summary>
    private byte[] _memory = [];
    /// <summary>Image initiale de la RAM, utilisée exclusivement pour la réinitialisation matérielle.</summary>
    private byte[] _bootMemory = [];
    /// <summary>Épingle possédée de la RAM exposée par les exports mockés.</summary>
    private GCHandle _memoryPin;
    /// <summary>Compteur logique émulé, restaurable par les sauvegardes.</summary>
    private long _ticks;
    /// <summary>Séquence de publication monotone, indépendante des retours arrière de l’état émulé.</summary>
    private long _sequence;
    /// <summary>Signale une initialisation terminée ; aucune frame ne peut précéder cette étape.</summary>
    private bool _initialized;
    /// <summary>Signale une destruction définitive, y compris après une initialisation interrompue.</summary>
    private bool _disposed;
    /// <summary>Allocations UTF-8 des métadonnées ABI, possédées jusqu’à Dispose.</summary>
    private readonly List<IEmulationNativeString> _strings = [];

    /// <summary>Crée les exports du mode choisi sans ouvrir le chemin d’assembly reçu.</summary>
    /// <param name="libraryPath">Chemin d’identité utilisé par les hôtes habituels ; aucun chargement natif.</param>
    /// <param name="emulator">Adaptateur interne, Libretro ou externe qui possède ce cœur.</param>
    internal ExternalCore(string libraryPath, Emulator emulator)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(libraryPath);
        if (emulator == Emulator.None || !Enum.IsDefined(emulator)) throw new ArgumentOutOfRangeException(nameof(emulator));
        Emulator = emulator;
        _contentFormats = EmulatorCatalog.CreateAdapter(emulator);
        _exports = new ExternalCoreExports(callback => _environment = callback, callback => _videoCallback = callback,
            callback => _sampleCallback = callback, callback => _batchCallback = callback, callback => _pollCallback = callback,
            callback => _inputCallback = callback, () => { }, Stop, ReadSystemInfo, ReadAvInfo,
            (port, device) => { _ports[checked((int)port)] = device; }, HardReset, CalculateFrame, _ => _initialized, Stop,
            () => 1u, _ => _memoryPin.IsAllocated ? _memoryPin.AddrOfPinnedObject() : 0, _ => (nuint)_memory.Length,
            () => (nuint)(SimulationConstants.StateHeaderLength + _memory.Length), SerializeNative, UnserializeNative);
    }

    /// <inheritdoc/>
    public Emulator Emulator { get; }
    /// <summary>Dernier statut reçu de la sortie de la machine ; instantané conservé par le cœur fictif.</summary>
    private AudioOutputBufferStatus? OutputAudioStatus { get; set; }
    /// <inheritdoc/>
    public void SetAudioBufferStatus(AudioOutputBufferStatus? status) => OutputAudioStatus = status;
    /// <inheritdoc/>
    public int MinimumAudioLatencyMilliseconds => AudioConstants.DefaultLatencyMilliseconds;
    /// <inheritdoc/>
    public bool ShutdownRequested { get; private set; }
    /// <inheritdoc/>
    public HostProcessState HostProcessState => HostProcessState.InProcess;
    /// <inheritdoc/>
    public int? HostProcessId => null;
    /// <inheritdoc/>
    public bool SupportsSaveStates => true;
    /// <inheritdoc/>
    public EmulationVideoStandard? Region => EmulationVideoStandard.Pal;
    /// <inheritdoc/>
    public int BufferedAudioFrames => _audio.BufferedFrames;
    /// <inheritdoc/>
    public long AudioOverrunCount => _audio.OverrunCount;
    /// <inheritdoc/>
    public long AudioUnderrunCount => _audio.UnderrunCount;
    /// <inheritdoc/>
    public string CoreName => EmulatorCatalog.Get(Emulator).Id;
    /// <inheritdoc/>
    public string CoreVersion => Engine.CoreVersion;
    /// <inheritdoc/>
    public string CoreSha256 => Engine.CoreSha256;
    /// <inheritdoc/>
    public double FramesPerSecond => SimulationConstants.FramesPerSecond;
    /// <inheritdoc/>
    public int SampleRate => AudioConstants.DefaultSampleRate;
    /// <inheritdoc/>
    public IReadOnlyList<CoreOption> Options => OptionCatalog.For(_configuration);
    /// <inheritdoc/>
    public IReadOnlyList<string> Diagnostics => _diagnostics.ToArray();
    /// <inheritdoc/>
    /// <remarks>Les formats du profil passent par le contrat Common, qui conserve ici les extensions déclarées et leur comparateur.</remarks>
    public IReadOnlySet<string> SupportedContentExtensions => _configuration is null ? new HashSet<string>() : _contentFormats.ResolveContentExtensions(StorageSettingsFunctions.DescribeDevices(_configuration.Model).SelectMany(device => device.AcceptedExtensions).ToHashSet(StringComparer.OrdinalIgnoreCase), requiresFilePath: true);

    /// <summary>Valide le profil, alloue la RAM et raccorde les callbacks avant de rendre le cœur exécutable.</summary>
    /// <remarks>Le firmware facultatif préparé par Common est lu sans écriture. En cas d’échec, finally
    /// libère aussi les allocations d’une initialisation partielle ; la machine conserve la propriété des dossiers.</remarks>
    public void Initialize(MachineConfiguration configuration, string sessionDirectory, string? saveDirectory = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_initialized) throw new InvalidOperationException();
        var completed = false;
        try
        {
            ConfigurationFunctions.Validate(configuration);
            _configuration = configuration;
            _feedback = CoreHostFunctions.EnvironmentProcessing.CreateControllerFeedbackAccumulator(ConfigurationFunctions.ControllerPortCount(configuration.Model));
            CaptureDiagnostic(CoreName);
            _memory = new byte[checked((int)HardwareModelCatalog.Get(configuration.Model).MainMemoryBytes)];
            var firmware = Path.Combine(sessionDirectory, "System", "OperatingSystem.rom");
            if (File.Exists(firmware))
            {
                using var input = File.OpenRead(firmware);
                if (input.Length > _memory.Length) throw new InvalidDataException(nameof(firmware));
                input.ReadExactly(_memory.AsSpan(0, checked((int)input.Length)));
            }
            _bootMemory = _memory.ToArray();
            _memoryPin = GCHandle.Alloc(_memory, GCHandleType.Pinned);
            _initialized = true;
            foreach (var media in configuration.Media.Where(media => media.IsInserted)) InsertMedia(media);
            if (Emulator != Emulator.Internal)
            {
                _exports.SetEnvironment((_, _) => false);
                _exports.SetInputPoll(() => { });
                _exports.SetInputState(ControllerInput);
                if (Emulator == Emulator.Libretro)
                {
                    _exports.SetVideo(ReceiveVideo);
                    _exports.SetAudioBatch(ReceiveAudio);
                }
                _exports.Initialize();
                if (!_exports.LoadGame(0)) throw new InvalidOperationException();
            }
            _diagnosticMonitor.ThrowIfFailed();
            completed = true;
        }
        finally
        {
            if (!completed) Dispose();
        }
    }

    /// <summary>Expose des métadonnées ABI dont les chaînes restent possédées par ce cœur.</summary>
    private void ReadSystemInfo(out ExternalCoreApi.SystemInfo info) => info = new() { LibraryName = OwnString(CoreName), LibraryVersion = OwnString(CoreVersion), ValidExtensions = OwnString(string.Join('|', SupportedContentExtensions)), NeedFullPath = true };
    /// <summary>Demande une chaîne UTF-8 au Common et en transfère la propriété à la liste du cœur.</summary>
    /// <param name="text">Métadonnée copiée, dont l’adresse est empruntée par le destinataire des exports.</param>
    /// <returns>Pointeur valide jusqu’à Dispose du cœur ; le destinataire ne le libère pas.</returns>
    /// <remarks>Si l’inscription échoue, le finally libère la chaîne qui n’a pas pu être transférée.
    /// Le nettoyage du cœur libère les chaînes inscrites après le détachement des callbacks.</remarks>
    private nint OwnString(string text)
    {
        var value = CoreHostFunctions.NativeResources.CreateString(text);
        var transferred = false;
        try
        {
            _strings.Add(value);
            transferred = true;
            return value.Pointer;
        }
        finally { if (!transferred) value.Dispose(); }
    }
    /// <summary>Expose la géométrie et les unités temporelles du même calcul pour tous les adaptateurs.</summary>
    private void ReadAvInfo(out ExternalCoreApi.SystemAvInfo info)
    {
        var (width, height) = OptionCatalog.Resolution(_configuration!);
        info = new() { Geometry = new() { BaseWidth = (uint)width, BaseHeight = (uint)height, MaximumWidth = (uint)width, MaximumHeight = (uint)height, AspectRatio = (float)width / height }, Timing = new() { FramesPerSecond = FramesPerSecond, SampleRate = SampleRate } };
    }

    /// <summary>Arrête la production ; la destruction des ressources reste une opération séparée du propriétaire.</summary>
    public void Stop() { ShutdownRequested = true; ClearOutputs(); }

    /// <summary>Détache les callbacks et libère RAM épinglée, chaînes natives et tableaux vidéo possédés.</summary>
    /// <remarks>Aucun processus ni thread n’est créé ici. La machine attend son thread avant cet appel.
    /// Les fichiers empruntés et les répertoires préparés par Common restent sous la responsabilité de Common.</remarks>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try { Stop(); }
        finally
        {
            try
            {
                _environment = null; _videoCallback = null; _sampleCallback = null; _batchCallback = null; _pollCallback = null; _inputCallback = null;
                if (_memoryPin.IsAllocated) _memoryPin.Free();
                foreach (var value in _strings) value.Dispose();
                _strings.Clear();
                _memory = []; _bootMemory = []; _mounted.Clear(); _ports.Clear(); _initialized = false;
            }
            finally
            {
                try { _feedback?.Dispose(); _feedback = null; }
                finally { _video.Dispose(); }
            }
        }
    }

    /// <summary>Protège les commandes contre les appels avant initialisation ou après destruction.</summary>
    private void EnsureReady()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!_initialized) throw new InvalidOperationException();
    }
    /// <summary>Reçoit un message du cœur simulé par le même point que les trois modes d’exécution.</summary>
    /// <param name="message">Diagnostic emprunté ; ici le nom invariant du cœur illustre une métadonnée sans traduction.</param>
    /// <remarks>Le message est conservé pour le statut et le transport Common, puis présenté au moniteur.
    /// Une capture fatale ne lève rien ici : l’initialisation effectue le contrôle géré après ses appels d’exports.
    /// Les textes restent disponibles si finally libère une initialisation partielle, afin que l’hôte publie sa cause.</remarks>
    private void CaptureDiagnostic(string message)
    {
        _diagnostics.Add(message);
        _diagnosticMonitor.Capture(message);
    }
}
