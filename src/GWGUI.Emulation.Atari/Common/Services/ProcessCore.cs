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

namespace GWGUI.Emulation.Atari.Common.Services;

[SupportedOSPlatform(ProcessCoreConstants.Windows)]
internal sealed partial class ProcessCore : IEmulatorCore
{
    private readonly string _hostExecutablePath;
    private readonly string _corePath;
    private readonly Emulator _emulator;
    private readonly TimeSpan _responseTimeout;
    private readonly TimeSpan _connectionTimeout;
    private readonly CancellationToken _cancellationToken;
    private readonly ConcurrentQueue<AudioChunk> _audio = new();
    private readonly EmulationInputAccumulator _input = new();
    private readonly SemaphoreSlim _requestGate = new(CoreHostConstants.MaximumPipeInstances,
        CoreHostConstants.MaximumPipeInstances);
    private NamedPipeServerStream? _pipe;
    private BinaryWriter? _writer;
    private Process? _process;
    private MemoryMappedFile? _videoMemory;
    private MemoryMappedViewAccessor? _videoMap;
    private bool _initialized;
    private bool _connectionFailed;
    private bool _disposed;
    private int? _hostProcessId;
    private string? _pipeName;
    private string? _videoMapName;
    private string? _activeVideoMapName;

    internal ProcessCore(string hostExecutablePath, string corePath, Emulator emulator,
        TimeSpan? responseTimeout = null, CancellationToken cancellationToken = default,
        TimeSpan? connectionTimeout = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hostExecutablePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(corePath);
        _hostExecutablePath = Path.GetFullPath(hostExecutablePath);
        _corePath = Path.GetFullPath(corePath);
        _emulator = emulator;
        _responseTimeout = responseTimeout ??
            TimeSpan.FromSeconds(CoreHostConstants.ResponseTimeoutSeconds);
        _cancellationToken = cancellationToken;
        _connectionTimeout = connectionTimeout ??
            TimeSpan.FromMilliseconds(CoreHostConstants.ConnectionTimeoutMilliseconds);
    }

    public Emulator Emulator => _emulator;
    public VideoFrame? LatestVideoFrame { get; private set; }
    public AudioChunk? LatestAudioChunk { get; private set; }
    public IReadOnlyList<CoreOption> Options { get; private set; } = [];
    public IReadOnlyList<string> Diagnostics { get; private set; } = [];
    public IReadOnlyDictionary<int, bool> LedStates { get; private set; } = new Dictionary<int, bool>();
    public string CoreName { get; private set; } = string.Empty;
    public string CoreVersion { get; private set; } = string.Empty;
    public string CoreSha256 { get; private set; } = string.Empty;
    public IReadOnlySet<string> SupportedContentExtensions { get; private set; } =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public bool SupportsSaveStates { get; private set; }
    public double FramesPerSecond { get; private set; }
    public int SampleRate { get; private set; }
    public RuntimeRegion? Region { get; private set; }
    public int BufferedAudioFrames { get; private set; }
    public long AudioOverrunCount { get; private set; }
    public long AudioUnderrunCount { get; private set; }
    public HostProcessState HostProcessState =>
        RuntimeFunctions.ProcessState(_process, _connectionFailed, _disposed);
    public int? HostProcessId => _hostProcessId;
    internal string? PipeName => _pipeName;
    internal string? VideoMapName => _activeVideoMapName ?? _videoMapName;

    public bool TryDequeueAudio(out AudioChunk? chunk)
    {
        if (_audio.TryDequeue(out var value))
        {
            chunk = value;
            return true;
        }
        chunk = null;
        return false;
    }

    public void Initialize(MachineConfiguration configuration, string sessionDirectory,
        string? saveDirectory = null)
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
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
            timeout.CancelAfter(_connectionTimeout);
            _pipe.WaitForConnectionAsync(timeout.Token).GetAwaiter().GetResult();
            _writer = new BinaryWriter(_pipe, Encoding.UTF8, leaveOpen: true);
            Request(HostCommand.Initialize, writer =>
            {
                writer.Write(_corePath);
                writer.Write((int)_emulator);
                writer.Write(Path.GetFullPath(sessionDirectory));
                CoreHostFunctions.WriteString(writer,
                    saveDirectory is null ? null : Path.GetFullPath(saveDirectory));
                writer.Write(JsonSerializer.Serialize(configuration, CoreHostFunctions.JsonOptions));
            }, ReadInitialization, allowUninitialized: true);
            _initialized = true;
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    public void RunFrame() => Request(HostCommand.RunFrame,
        writer => CoreHostFunctions.WriteInput(writer, _input.Consume()), ReadFrame);

    public void HardReset() => Request(HostCommand.HardReset);
    public void Stop()
    {
        if (_initialized && !_disposed && !_connectionFailed) Request(HostCommand.Stop);
    }
    public void SetInput(EmulationInputSnapshot snapshot) => _input.Update(snapshot);
    public void SetControllerPortDevice(int port, PeripheralCategory peripheral) =>
        Request(HostCommand.SetControllerPortDevice, writer =>
        {
            writer.Write(port);
            writer.Write((int)peripheral);
        });
    public void InsertMedia(MediaConfiguration media) => Request(HostCommand.InsertMedia,
        writer => writer.Write(JsonSerializer.Serialize(media, CoreHostFunctions.JsonOptions)));
    public void EjectMedia(EmulationMediaSlot slot) => Request(HostCommand.EjectMedia,
        writer => writer.Write(slot.ProtocolValue));
    public void SelectDisk(int index) => Request(HostCommand.SelectDisk, writer => writer.Write(index));
    public void SaveMediaChanges(EmulationMediaSlot slot) => Request(HostCommand.SaveMediaChanges,
        writer => writer.Write(slot.ProtocolValue));
    public DiskStatus GetDiskStatus()
    {
        DiskStatus? status = null;
        Request(HostCommand.GetDiskStatus, read: reader => status = CoreHostFunctions.ReadDiskStatus(reader));
        return status ?? throw new InvalidDataException(CoreHostErrors.CommunicationFailed);
    }
    public bool HasUnsavedMediaChanges(EmulationMediaSlot slot)
    {
        var hasChanges = false;
        Request(HostCommand.HasUnsavedMediaChanges, writer => writer.Write(slot.ProtocolValue),
            reader => hasChanges = reader.ReadBoolean());
        return hasChanges;
    }
    public byte[] SaveState()
    {
        byte[]? state = null;
        Request(HostCommand.SaveState, read: reader => state = CoreHostFunctions.ReadBytes(reader));
        return state ?? throw new InvalidDataException(CoreHostErrors.CommunicationFailed);
    }
    public void LoadState(ReadOnlySpan<byte> state)
    {
        var copy = state.ToArray();
        Request(HostCommand.LoadState, writer => CoreHostFunctions.WriteBytes(writer, copy));
    }
    public void SetOption(string key, string value) => Request(HostCommand.SetOption, writer =>
    {
        writer.Write(key);
        writer.Write(value);
    });
}
