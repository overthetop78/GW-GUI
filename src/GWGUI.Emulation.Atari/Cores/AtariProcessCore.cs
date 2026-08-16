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
using GWGUI.Emulation.Common;

namespace GWGUI.Emulation.Atari.Cores;

[SupportedOSPlatform("windows")]
internal sealed class AtariProcessCore : IAtariCore
{
    private readonly string _hostExecutablePath;
    private readonly string _corePath;
    private readonly AtariCoreKind _kind;
    private readonly TimeSpan _responseTimeout;
    private readonly TimeSpan _connectionTimeout;
    private readonly CancellationToken _cancellationToken;
    private readonly ConcurrentQueue<AudioChunk> _audio = new();
    private readonly EmulationInputAccumulator _input = new();
    private readonly SemaphoreSlim _requestGate = new(AtariCoreHostConstants.MaximumPipeInstances,
        AtariCoreHostConstants.MaximumPipeInstances);
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

    internal AtariProcessCore(string hostExecutablePath, string corePath, AtariCoreKind kind,
        TimeSpan? responseTimeout = null, CancellationToken cancellationToken = default,
        TimeSpan? connectionTimeout = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hostExecutablePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(corePath);
        _hostExecutablePath = Path.GetFullPath(hostExecutablePath);
        _corePath = Path.GetFullPath(corePath);
        _kind = kind;
        _responseTimeout = responseTimeout ??
            TimeSpan.FromSeconds(AtariCoreHostConstants.ResponseTimeoutSeconds);
        _cancellationToken = cancellationToken;
        _connectionTimeout = connectionTimeout ??
            TimeSpan.FromMilliseconds(AtariCoreHostConstants.ConnectionTimeoutMilliseconds);
    }

    public AtariCoreKind Kind => _kind;
    public VideoFrame? LatestVideoFrame { get; private set; }
    public AudioChunk? LatestAudioChunk { get; private set; }
    public IReadOnlyList<AtariCoreOption> Options { get; private set; } = [];
    public IReadOnlyList<string> Diagnostics { get; private set; } = [];
    public IReadOnlyDictionary<int, bool> LedStates { get; private set; } = new Dictionary<int, bool>();
    public string CoreName { get; private set; } = string.Empty;
    public string CoreVersion { get; private set; } = string.Empty;
    public string CoreSha256 { get; private set; } = string.Empty;
    public IReadOnlySet<string> SupportedContentExtensions { get; private set; } =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public double FramesPerSecond { get; private set; }
    public int SampleRate { get; private set; }
    public AtariRuntimeRegion? Region { get; private set; }
    public int BufferedAudioFrames { get; private set; }
    public long AudioOverrunCount { get; private set; }
    public long AudioUnderrunCount { get; private set; }
    public AtariHostProcessState HostProcessState =>
        AtariRuntimeFunctions.ProcessState(_process, _connectionFailed, _disposed);
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

    public void Initialize(AtariMachineConfiguration configuration, string sessionDirectory,
        string? saveDirectory = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_initialized) throw new InvalidOperationException(AtariCoreHostErrors.AlreadyInitialized);
        if (!File.Exists(_hostExecutablePath))
            throw new FileNotFoundException(AtariCoreHostErrors.ExecutableMissing, _hostExecutablePath);

        var pipeName = AtariCoreHostFunctions.CreatePipeName();
        var videoMapName = AtariCoreHostFunctions.CreateVideoMapName();
        _pipeName = pipeName;
        _videoMapName = videoMapName;
        try
        {
            _pipe = new NamedPipeServerStream(pipeName, PipeDirection.InOut,
                AtariCoreHostConstants.MaximumPipeInstances, PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly,
                AtariCoreHostConstants.PipeBufferSize, AtariCoreHostConstants.PipeBufferSize);
            _process = StartHostProcess(pipeName, videoMapName);
            _hostProcessId = _process.Id;
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
            timeout.CancelAfter(_connectionTimeout);
            _pipe.WaitForConnectionAsync(timeout.Token).GetAwaiter().GetResult();
            _writer = new BinaryWriter(_pipe, Encoding.UTF8, leaveOpen: true);
            Request(AtariHostCommand.Initialize, writer =>
            {
                writer.Write(_corePath);
                writer.Write((int)_kind);
                writer.Write(Path.GetFullPath(sessionDirectory));
                AtariCoreHostFunctions.WriteString(writer,
                    saveDirectory is null ? null : Path.GetFullPath(saveDirectory));
                writer.Write(JsonSerializer.Serialize(configuration, AtariCoreHostFunctions.JsonOptions));
            }, ReadInitialization, allowUninitialized: true);
            _initialized = true;
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    public void RunFrame() => Request(AtariHostCommand.RunFrame,
        writer => AtariCoreHostFunctions.WriteInput(writer, _input.Consume()), ReadFrame);

    public void HardReset() => Request(AtariHostCommand.HardReset);
    public void Stop()
    {
        if (_initialized && !_disposed && !_connectionFailed) Request(AtariHostCommand.Stop);
    }
    public void SetInput(EmulationInputSnapshot snapshot) => _input.Update(snapshot);
    public void InsertMedia(AtariMediaConfiguration media) => Request(AtariHostCommand.InsertMedia,
        writer => writer.Write(JsonSerializer.Serialize(media, AtariCoreHostFunctions.JsonOptions)));
    public void EjectMedia(EmulationMediaSlot slot) => Request(AtariHostCommand.EjectMedia,
        writer => writer.Write((int)slot));
    public void SelectDisk(int index) => Request(AtariHostCommand.SelectDisk, writer => writer.Write(index));
    public void SaveMediaChanges(EmulationMediaSlot slot) => Request(AtariHostCommand.SaveMediaChanges,
        writer => writer.Write((int)slot));
    public AtariDiskStatus GetDiskStatus()
    {
        AtariDiskStatus? status = null;
        Request(AtariHostCommand.GetDiskStatus, read: reader => status = AtariCoreHostFunctions.ReadDiskStatus(reader));
        return status ?? throw new InvalidDataException(AtariCoreHostErrors.CommunicationFailed);
    }
    public bool HasUnsavedMediaChanges(EmulationMediaSlot slot)
    {
        var hasChanges = false;
        Request(AtariHostCommand.HasUnsavedMediaChanges, writer => writer.Write((int)slot),
            reader => hasChanges = reader.ReadBoolean());
        return hasChanges;
    }
    public byte[] SaveState()
    {
        byte[]? state = null;
        Request(AtariHostCommand.SaveState, read: reader => state = AtariCoreHostFunctions.ReadBytes(reader));
        return state ?? throw new InvalidDataException(AtariCoreHostErrors.CommunicationFailed);
    }
    public void LoadState(ReadOnlySpan<byte> state)
    {
        var copy = state.ToArray();
        Request(AtariHostCommand.LoadState, writer => AtariCoreHostFunctions.WriteBytes(writer, copy));
    }
    public void SetOption(string key, string value) => Request(AtariHostCommand.SetOption, writer =>
    {
        writer.Write(key);
        writer.Write(value);
    });

    private Process StartHostProcess(string pipeName, string videoMapName)
    {
        var startInfo = new ProcessStartInfo(_hostExecutablePath)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(_hostExecutablePath)!
        };
        startInfo.ArgumentList.Add(AtariCoreHostConstants.CommandLineArgument);
        startInfo.ArgumentList.Add(pipeName);
        startInfo.ArgumentList.Add(videoMapName);
        return Process.Start(startInfo) ?? throw new InvalidOperationException(AtariCoreHostErrors.ProcessStartFailed);
    }

    private void ReadInitialization(BinaryReader reader)
    {
        CoreSha256 = reader.ReadString();
        FramesPerSecond = reader.ReadDouble();
        SampleRate = reader.ReadInt32();
        ReadRuntimeStatus(reader);
        Options = JsonSerializer.Deserialize<IReadOnlyList<AtariCoreOption>>(reader.ReadString(),
            AtariCoreHostFunctions.JsonOptions) ?? [];
        Diagnostics = JsonSerializer.Deserialize<IReadOnlyList<string>>(reader.ReadString(),
            AtariCoreHostFunctions.JsonOptions) ?? [];
        CoreName = reader.ReadString();
        CoreVersion = reader.ReadString();
        SupportedContentExtensions = reader.ReadString()
            .Split(AtariCoreHostConstants.ExtensionListSeparator, StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        LedStates = AtariCoreHostFunctions.ReadLedStates(reader);
    }

    private void ReadFrame(BinaryReader reader)
    {
        LatestVideoFrame = AtariCoreHostFunctions.ReadResizableSharedFrame(reader,
            ref _videoMemory, ref _videoMap, ref _activeVideoMapName) ?? LatestVideoFrame;
        foreach (var chunk in AtariCoreHostFunctions.ReadAudio(reader))
        {
            LatestAudioChunk = chunk;
            _audio.Enqueue(chunk);
        }
        FramesPerSecond = reader.ReadDouble();
        SampleRate = reader.ReadInt32();
        ReadRuntimeStatus(reader);
        if (reader.ReadBoolean())
            Diagnostics = JsonSerializer.Deserialize<IReadOnlyList<string>>(reader.ReadString(),
                AtariCoreHostFunctions.JsonOptions) ?? [];
        LedStates = AtariCoreHostFunctions.ReadLedStates(reader);
    }

    private void ReadRuntimeStatus(BinaryReader reader)
    {
        Region = AtariRuntimeFunctions.ReadRegion(reader.ReadInt32());
        BufferedAudioFrames = reader.ReadInt32();
        AudioOverrunCount = reader.ReadInt64();
        AudioUnderrunCount = reader.ReadInt64();
    }

    private void Request(AtariHostCommand command, Action<BinaryWriter>? write = null,
        Action<BinaryReader>? read = null, bool allowUninitialized = false)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_connectionFailed) throw new InvalidOperationException(AtariCoreHostErrors.ProcessUnavailable);
        if (_cancellationToken.IsCancellationRequested)
            FailConnection(new OperationCanceledException(_cancellationToken));
        if (!allowUninitialized && !_initialized)
            throw new InvalidOperationException(AtariCoreHostErrors.ProcessNotInitialized);
        _requestGate.Wait();
        try
        {
            var writer = _writer ?? throw new InvalidOperationException(AtariCoreHostErrors.ProcessUnavailable);
            AtariCoreHostFunctions.WriteRequestHeader(writer, command);
            write?.Invoke(writer);
            writer.Flush();
            using var reader = new BinaryReader(new MemoryStream(ReadResponse(), writable: false), Encoding.UTF8,
                leaveOpen: false);
            var status = AtariCoreHostFunctions.ReadResponseHeader(reader);
            if (status == AtariHostResponseStatus.Failure) ThrowRemoteError(AtariCoreHostFunctions.ReadError(reader));
            if (status != AtariHostResponseStatus.Success)
                throw new InvalidDataException(AtariCoreHostErrors.CommunicationFailed);
            read?.Invoke(reader);
        }
        catch (Exception error) when (error is IOException or EndOfStreamException or OperationCanceledException
                                      or InvalidDataException)
        {
            FailConnection(error);
        }
        finally
        {
            _requestGate.Release();
        }
    }

    private byte[] ReadResponse()
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
        timeout.CancelAfter(_responseTimeout);
        var header = new byte[sizeof(int)];
        _pipe!.ReadExactlyAsync(header, timeout.Token).AsTask().GetAwaiter().GetResult();
        var length = BinaryPrimitives.ReadInt32LittleEndian(header);
        if (length is < 0 or > EmulationHostProtocolConstants.MaximumBlobLength)
            throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture,
                AtariCoreHostErrors.InvalidResponseLengthFormat, length));
        var response = GC.AllocateUninitializedArray<byte>(length);
        _pipe.ReadExactlyAsync(response, timeout.Token).AsTask().GetAwaiter().GetResult();
        return response;
    }

    private static void ThrowRemoteError(AtariHostError error)
    {
        if (error.Kind is { } kind && error.Code is { } code)
            throw new AtariEmulationException(kind, code, error.Message, error.Context);
        throw new InvalidOperationException(error.Message);
    }

    private void FailConnection(Exception error)
    {
        var cancelled = _cancellationToken.IsCancellationRequested;
        var timedOut = error is OperationCanceledException && !cancelled;
        var exitSuffix = _process is { HasExited: true }
            ? string.Format(CultureInfo.InvariantCulture, AtariCoreHostErrors.ProcessExitSuffixFormat,
                _process.ExitCode)
            : string.Empty;
        _connectionFailed = true;
        TerminateHostProcess();
        var message = cancelled
            ? AtariCoreHostErrors.RequestCancelled
            : timedOut
                ? AtariCoreHostErrors.ResponseTimeout
                : AtariCoreHostErrors.CommunicationFailed + exitSuffix;
        throw new InvalidOperationException(message, error);
    }

    private void TerminateHostProcess()
    {
        _pipe?.Dispose();
        if (_process is null) return;
        try
        {
            if (!_process.HasExited) _process.Kill(entireProcessTree: true);
            _process.WaitForExit(AtariCoreHostConstants.GracefulExitTimeoutMilliseconds);
        }
        catch (Exception)
        {
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        if (!_connectionFailed && _pipe?.IsConnected == true)
        {
            try
            {
                Request(AtariHostCommand.Dispose, allowUninitialized: true);
            }
            catch (Exception)
            {
            }
        }
        _disposed = true;
        AtariCoreHostFunctions.DisposeTransport(_writer);
        AtariCoreHostFunctions.DisposeTransport(_pipe);
        _videoMap?.Dispose();
        _videoMemory?.Dispose();
        if (_process is not null)
        {
            try
            {
                if (!_process.WaitForExit(AtariCoreHostConstants.GracefulExitTimeoutMilliseconds))
                    _process.Kill(entireProcessTree: true);
                _process.WaitForExit(AtariCoreHostConstants.GracefulExitTimeoutMilliseconds);
            }
            catch (Exception)
            {
            }
            _process.Dispose();
        }
        _requestGate.Dispose();
        while (_audio.TryDequeue(out _))
        {
        }
    }
}
