using CommonControllerFunctions = GWGUI.Emulation.Nintendo.Common.Functions.ControllerFunctions;
using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Nintendo.Common.Functions.CoreHostFunctions;

using System.IO;
using System.Collections.Concurrent;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO.Pipes;
using System.IO.MemoryMappedFiles;
using System.Text.Json;
using GWGUI.Emulation;
using GWGUI.Emulation.Functions;

namespace GWGUI.Emulation.Nintendo.Common.Services;

internal sealed class ProcessCore : IEmulatorCore
{
    /// <summary>Configuration empruntée pour résoudre les règles du transport de cette session.</summary>
    private MachineConfiguration? _policyConfiguration;
    /// <summary>Adaptation Common empruntée, résolue une seule fois après réception de la configuration.</summary>
    private IEmulatorRuntimePolicy? _runtimePolicy;
    /// <summary>Indique que la présence ou absence de politique a déjà été consultée.</summary>
    private bool _runtimePolicyResolved;
    /// <summary>Consulte la politique sans recréer le catalogue à chaque commande ni posséder de cœur supplémentaire.</summary>
    private IEmulatorRuntimePolicy? RuntimePolicy
    {
        get
        {
            if (!_runtimePolicyResolved && _policyConfiguration is not null)
            {
                _runtimePolicy = new Engine().SelectedAdapter(_policyConfiguration) as IEmulatorRuntimePolicy;
                _runtimePolicyResolved = true;
            }
            return _runtimePolicy;
        }
    }
    private HostCommand _pendingCommand;

    /// <inheritdoc/>
    public bool SupportsPointerCapture => true;
    /// <inheritdoc/>
    public string Emulator => _configuration?.EmulatorId ?? string.Empty;

    /// <inheritdoc/>
    public bool SupportsSaveStates { get; private set; }
    /// <inheritdoc/>
    public EmulationVideoStandard? Region { get; private set; }
    /// <inheritdoc/>
    public int BufferedAudioFrames { get; private set; }
    /// <inheritdoc/>
    public long AudioOverrunCount { get; private set; }
    /// <inheritdoc/>
    public long AudioUnderrunCount { get; private set; }

    /// <summary>Lit le statut courant avec le même codec que le propriétaire du cœur.</summary>
    private void ReadRuntimeStatus(BinaryReader reader) => CommonCoreHostFunctions.Protocol.ReadRuntimeStatus(reader,
        value => Region = CommonCoreHostFunctions.Protocol.ReadRegion(value, allowAllDefinedStandards: false),
        value => BufferedAudioFrames = value, value => AudioOverrunCount = value, value => AudioUnderrunCount = value);

    /// <inheritdoc/>
    public DiskStatus GetDiskStatus()
    {
        Begin(HostCommand.GetDiskStatus);
        CompleteRequest();
        return System.Text.Json.JsonSerializer.Deserialize<DiskStatus>(Response.ReadString(), CommonCoreHostFunctions.JsonOptions) ?? throw new InvalidDataException();
    }

    /// <inheritdoc/>
    public bool ShutdownRequested { get; private set; }

    /// <summary>Dernier instantané des effets validés reçus du processus du cœur.</summary>
    public IReadOnlyList<ControllerFeedbackState> ControllerFeedback { get; private set; } = [];

    private readonly string _hostExecutablePath;
    private readonly string? _corePath;
    private readonly ConcurrentQueue<AudioChunk> _audio = new();
    private NamedPipeServerStream? _pipe;
    private BinaryReader? _responseReader;
    private BinaryWriter? _writer;
    private Process? _process;
    private MemoryMappedFile? _videoMemory;
    private MemoryMappedViewAccessor? _videoMap;
    private readonly InputAccumulator _input = CommonControllerFunctions.CreateInputAccumulator();
    private bool _initialized;
    private bool _disposed;
    private bool _connectionFailed;
    private MachineConfiguration? _configuration;
    private string? _sessionDirectory;
    private string? _saveDirectory;

    internal ProcessCore(string hostExecutablePath, string? corePath = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hostExecutablePath);
        _hostExecutablePath = Path.GetFullPath(hostExecutablePath);
        _corePath = corePath;
    }

    public VideoFrame? LatestVideoFrame { get; private set; }
    public AudioChunk? LatestAudioChunk { get; private set; }
    public IReadOnlyList<CoreOption> Options { get; private set; } = [];
    public IReadOnlyList<string> Diagnostics { get; private set; } = [];
    public IReadOnlyDictionary<int, bool> LedStates { get; private set; } = new Dictionary<int, bool>();
    public string CoreName { get; private set; } = string.Empty;
    public string CoreVersion { get; private set; } = string.Empty;
    public IReadOnlySet<string> SupportedContentExtensions { get; private set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public string CoreSha256 { get; private set; } = string.Empty;
    public double FramesPerSecond { get; private set; } = 50;
    public int SampleRate { get; private set; } = 44100;
    public int DiskCount { get; private set; }
    public int CurrentDiskIndex { get; private set; } = -1;

    public bool TryDequeueAudio(out AudioChunk? chunk)
    => _audio.TryDequeue(out chunk);

    public void Initialize(MachineConfiguration configuration, string sessionDirectory, string? saveDirectory = null)
    {
        _policyConfiguration = configuration;
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_initialized) throw new InvalidOperationException(CoreExceptions.ProcessAlreadyInitialized());
        if (!File.Exists(_hostExecutablePath))
            throw new FileNotFoundException(CoreExceptions.HostExecutableNotFound(), _hostExecutablePath);

        _configuration = configuration;
        _sessionDirectory = Path.GetFullPath(sessionDirectory);
        _saveDirectory = saveDirectory is null ? null : Path.GetFullPath(saveDirectory);
        var pipeName = $"{CoreHostConstants.PipePrefix}{Guid.NewGuid():N}";
        var videoMapName = $"{CoreHostConstants.VideoMapPrefix}{Guid.NewGuid():N}";
        _videoMemory = MemoryMappedFile.CreateNew(videoMapName, CommonCoreHostFunctions.Protocol.VideoMapCapacity, MemoryMappedFileAccess.ReadWrite);
        try
        {
            _videoMap = _videoMemory.CreateViewAccessor(0, CommonCoreHostFunctions.Protocol.VideoMapCapacity, MemoryMappedFileAccess.ReadWrite);
            _pipe = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, CoreHostConstants.PipeBufferSize, CoreHostConstants.PipeBufferSize);
            var startInfo = CommonCoreHostFunctions.Protocol.CreateHostStartInfo(_hostExecutablePath,
                Path.GetDirectoryName(_hostExecutablePath)!, CoreHostConstants.CommandLineArgument, pipeName, videoMapName);
            _process = CommonCoreHostFunctions.Protocol.StartHostProcess(startInfo, () => new InvalidOperationException(CoreExceptions.ProcessStartFailed()));
            _hostProcessId = _process.Id;
            _hostProcessFaulted = false;
            CommonCoreHostFunctions.Protocol.WaitForConnection(_pipe, TimeSpan.FromMilliseconds(CoreHostConstants.ConnectionTimeoutMilliseconds));
            _writer = new BinaryWriter(_pipe, System.Text.Encoding.UTF8, true);
            Begin(HostCommand.Initialize);
            CommonCoreHostFunctions.Protocol.WriteInitializationRequest(ref _writer, _corePath ?? string.Empty,
                () => _sessionDirectory,
                () => _saveDirectory,
                CommonCoreHostFunctions.WriteString,
                () => JsonSerializer.Serialize(configuration, CommonCoreHostFunctions.JsonOptions));
            CompleteRequest();
            CoreSha256 = Response.ReadString();
            FramesPerSecond = Response.ReadDouble();
            SampleRate = Response.ReadInt32();
            MinimumAudioLatencyMilliseconds = Response.ReadInt32();
            SupportsSaveStates = Response.ReadBoolean();
            ReadRuntimeStatus(Response);
            Options = JsonSerializer.Deserialize<IReadOnlyList<CoreOption>>(Response.ReadString(), CommonCoreHostFunctions.JsonOptions) ?? [];
            Diagnostics = JsonSerializer.Deserialize<IReadOnlyList<string>>(Response.ReadString(), CommonCoreHostFunctions.JsonOptions) ?? [];
            CoreName = Response.ReadString();
            CoreVersion = Response.ReadString();
            SupportedContentExtensions = Response.ReadString().Split('|', StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.OrdinalIgnoreCase);
            DiskCount = Response.ReadInt32();
            CurrentDiskIndex = Response.ReadInt32();
            LedStates = CommonCoreHostFunctions.ReadLedStates(Response);
            ShutdownRequested = Response.ReadBoolean();
            ControllerFeedback = CommonCoreHostFunctions.Protocol.ReadControllerFeedback(Response);
            Options = RuntimePolicy?.NormalizeRuntimeOptions(configuration, Options) ?? Options;
            _initialized = true;
        }
        finally
        {
            if (!_initialized) { _hostProcessFaulted = true; ReleaseHost(); }
        }
    }

    public void RunFrame()
    {
        Begin(HostCommand.RunFrame);
        CommonCoreHostFunctions.WriteInput(_writer!, _input.Consume());
        CommonCoreHostFunctions.Protocol.WriteAudioBufferStatus(_writer!, OutputAudioStatus);
        CompleteRequest();
        LatestVideoFrame = CommonCoreHostFunctions.ReadSharedFrame(Response, _videoMap ?? throw new InvalidOperationException(CoreExceptions.VideoBufferUnavailable())) ?? LatestVideoFrame;
        foreach (var chunk in CommonCoreHostFunctions.ReadAudio(Response))
        {
            LatestAudioChunk = chunk;
            _audio.Enqueue(chunk);
        }
        FramesPerSecond = Response.ReadDouble();
        SampleRate = Response.ReadInt32();
        MinimumAudioLatencyMilliseconds = Response.ReadInt32();
        ReadRuntimeStatus(Response);
        DiskCount = Response.ReadInt32();
        CurrentDiskIndex = Response.ReadInt32();
        if (Response.ReadBoolean())
            Diagnostics = JsonSerializer.Deserialize<IReadOnlyList<string>>(Response.ReadString(), CommonCoreHostFunctions.JsonOptions) ?? [];
        LedStates = CommonCoreHostFunctions.ReadLedStates(Response);
        ShutdownRequested = Response.ReadBoolean();
        ControllerFeedback = CommonCoreHostFunctions.Protocol.ReadControllerFeedback(Response);
    }

    public void HardReset()
    {
        var configuration = _configuration ?? throw new InvalidOperationException(CoreExceptions.ProcessNotInitialized());
        var sessionDirectory = _sessionDirectory ?? throw new InvalidOperationException(CoreExceptions.ProcessNotInitialized());
        var saveDirectory = _saveDirectory;
        ReleaseHost();
        Initialize(configuration, sessionDirectory, saveDirectory);
    }
    public void SoftReset() => SimpleRequest(HostCommand.SoftReset);
    public void Stop() => SimpleRequest(HostCommand.Stop);
    public void SetInput(EmulationInputSnapshot snapshot) => _input.Update(snapshot);
    public void InsertMedia(MediaConfiguration media)
    {
        StringRequest(HostCommand.InsertMedia, JsonSerializer.Serialize(media with { Path = Path.GetFullPath(media.Path) }, CommonCoreHostFunctions.JsonOptions));
        if (media.MountOrder < DiskCount) CurrentDiskIndex = media.MountOrder;
    }
    public void SetControllerPortDevice(int port, PeripheralCategory peripheral)
    {
        Begin(HostCommand.SetControllerPortDevice);
        _writer!.Write(port);
        _writer.Write((int)peripheral);
        CompleteRequest();
    }

    public void EjectMedia(EmulationMediaSlot slot)
    {
        Begin(HostCommand.EjectMedia);
        CommonCoreHostFunctions.Protocol.WriteMediaSlot(_writer!, slot);
        CompleteRequest();
        CurrentDiskIndex = slot.Index;
    }

    public void SelectDisk(int index)
    {
        Begin(HostCommand.SelectDisk);
        _writer!.Write(index);
        CompleteRequest();
        CurrentDiskIndex = index;
    }

    public byte[] SaveState()
    {
        Begin(HostCommand.SaveState);
        CompleteRequest();
        return CommonCoreHostFunctions.ReadBytes(Response);
    }

    public void LoadState(ReadOnlySpan<byte> state)
    {
        Begin(HostCommand.LoadState);
        CommonCoreHostFunctions.WriteBytes(_writer!, state);
        CompleteRequest();
    }

    public void SetOption(string key, string value)
    {
        Begin(HostCommand.SetOption);
        _writer!.Write(key);
        _writer.Write(value);
        CompleteRequest();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        ControllerFeedback = [];
        ReleaseHost();
    }

    private void ReleaseHost()
    {
        try
        {
            if (!_connectionFailed && _pipe?.IsConnected == true)
            {
                try
                {
                    _pendingCommand = HostCommand.Dispose;
                    CommonCoreHostFunctions.WriteRequestHeader(_writer!, HostCommand.Dispose);
                    CompleteRequest();
                }
                catch (Exception) { }
            }
        }
        finally
        {
            CommonCoreHostFunctions.Protocol.DisposeTransportIgnoringErrors(_responseReader); _responseReader = null;
            CommonCoreHostFunctions.Protocol.DisposeTransportIgnoringErrors(_writer); _writer = null;
            CommonCoreHostFunctions.Protocol.DisposeTransportIgnoringErrors(_pipe); _pipe = null;
            CommonCoreHostFunctions.Protocol.DisposeTransportIgnoringErrors(_videoMap); _videoMap = null;
            CommonCoreHostFunctions.Protocol.DisposeTransportIgnoringErrors(_videoMemory); _videoMemory = null;
            var process = _process;
            _process = null;
            if (process is not null)
            {
                try
                {
                    CommonCoreHostFunctions.Protocol.WaitForHostExit(process, graceful: true, CoreHostConstants.GracefulExitTimeoutMilliseconds);
                }
                finally { process.Dispose(); }
            }
            while (_audio.TryDequeue(out _)) { }
            LatestVideoFrame = null;
            LatestAudioChunk = null;
            Options = [];
            Diagnostics = [];
            LedStates = new Dictionary<int, bool>();
            DiskCount = 0;
            CurrentDiskIndex = -1;
            _input.Reset();
            _initialized = false;
            _connectionFailed = false;
        }
    }

    private void StringRequest(HostCommand command, string value)
    {
        Begin(command);
        _writer!.Write(value);
        CompleteRequest();
    }

    private void SimpleRequest(HostCommand command)
    {
        Begin(command);
        CompleteRequest();
    }

    private void Begin(HostCommand command)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_connectionFailed) throw new InvalidOperationException(CoreExceptions.ProcessUnavailable());
        if (command != HostCommand.Initialize && !_initialized)
            throw new InvalidOperationException(CoreExceptions.ProcessNotInitialized());
        _pendingCommand = command;
        CommonCoreHostFunctions.WriteRequestHeader(_writer!, command);
    }

    private void CompleteRequest()
    {
        try
        {
            var response = ReadResponseAsync().GetAwaiter().GetResult();
            _responseReader?.Dispose();
            _responseReader = new BinaryReader(new MemoryStream(response, false), System.Text.Encoding.UTF8, false);
            var status = CommonCoreHostFunctions.ReadResponseHeader(Response);
            if (status == HostResponseStatus.Failure) ThrowRemoteError(CommonCoreHostFunctions.ReadError(Response));
            if (status != HostResponseStatus.Success) throw new InvalidDataException();
        }
        catch (Exception error) when (error is IOException or EndOfStreamException or OperationCanceledException or InvalidDataException)
        {
            var timedOut = error is OperationCanceledException;
            var exit = _process is { HasExited: true } ? $" It exited with code {_process.ExitCode}." : string.Empty;
            _connectionFailed = true;
            _hostProcessFaulted = true;
            TerminateProcess();
            throw new InvalidOperationException(timedOut ? CoreExceptions.ProcessTimeout() : CoreExceptions.ProcessCommunicationFailed(exit), error);
        }
    }

    private Task<byte[]> ReadResponseAsync() =>
        CommonCoreHostFunctions.Protocol.ReadResponseAsync(_pipe!, RequestTimeoutFor(_pendingCommand, TimeSpan.FromSeconds(CoreHostConstants.ResponseTimeoutSeconds)),
            CancellationToken.None, length => CoreExceptions.InvalidResponseLength(length));

    private void TerminateProcess()
    {
        CommonCoreHostFunctions.Protocol.DisposeTransportIgnoringErrors(_pipe);
        _pipe = null;
        var process = _process;
        _process = null;
        if (process is null) return;
        try { CommonCoreHostFunctions.Protocol.WaitForHostExit(process, graceful: false, CoreHostConstants.GracefulExitTimeoutMilliseconds); }
        finally { process.Dispose(); }
    }

    private BinaryReader Response => _responseReader ?? throw new InvalidOperationException(CoreExceptions.HostResponseUnavailable());

    /// <inheritdoc/>
    public int MinimumAudioLatencyMilliseconds { get; private set; }
    /// <summary>Valeurs de la sortie empruntée, copiées dans chaque requête de frame.</summary>
    private AudioOutputBufferStatus? OutputAudioStatus { get; set; }
    /// <inheritdoc/>
    public void SetAudioBufferStatus(AudioOutputBufferStatus? status) => OutputAudioStatus = status;
    /// <summary>Identifiant géré conservé après fermeture, sans garder de handle du processus.</summary>
    private int? _hostProcessId;
    /// <summary>Échec mémorisé même si le nettoyage remet le transport à son état initial.</summary>
    private bool _hostProcessFaulted;
    /// <inheritdoc/>
    public HostProcessState HostProcessState => CommonCoreHostFunctions.Protocol.GetProcessState(_process, _hostProcessFaulted || _connectionFailed, _disposed, _hostProcessId);
    /// <inheritdoc/>
    public int? HostProcessId => _hostProcessId;
    /// <inheritdoc/>
    public void SaveMediaChanges(EmulationMediaSlot slot)
    {
        Begin(HostCommand.SaveMediaChanges);
        CommonCoreHostFunctions.Protocol.WriteMediaSlot(_writer!, slot);
        CompleteRequest();
    }
    /// <inheritdoc/>
    public bool HasUnsavedMediaChanges(EmulationMediaSlot slot)
    {
        Begin(HostCommand.HasUnsavedMediaChanges);
        CommonCoreHostFunctions.Protocol.WriteMediaSlot(_writer!, slot);
        CompleteRequest();
        return Response.ReadBoolean();
    }
    /// <summary>Résout le délai de cette commande par le contrat runtime Common.</summary>
    /// <param name="command">Commande dont le transport attend la réponse.</param>
    /// <param name="fallback">Délai du transport utilisé lorsque le profil ne définit aucune adaptation.</param>
    /// <returns>Délai de cette attente ; la consultation ne lance ni ne détruit de ressource.</returns>
    private TimeSpan RequestTimeoutFor(HostCommand command, TimeSpan fallback) => _policyConfiguration is null ? fallback : RuntimePolicy?.GetHostCommandTimeout(_policyConfiguration, (byte)command, fallback) ?? fallback;
    /// <summary>Restitue les codes publics du diagnostic reçu sans charger son type d’exception.</summary>
    private static void ThrowRemoteError(HostError error)
    {
        if (error.Category is { } category && error.Code is { } code)
            throw CommonCoreHostFunctions.Messages.CreateError(category, code, new InvalidOperationException(error.Message));
        throw new InvalidOperationException(error.Message);
    }
}
