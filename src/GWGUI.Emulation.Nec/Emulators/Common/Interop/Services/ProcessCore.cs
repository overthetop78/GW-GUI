using CommonCoreHostFunctions = GWGUI.Emulation.Nec.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Exceptions;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Enums;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Services;

using System.IO;
using System.Collections.Concurrent;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO.Pipes;
using System.IO.MemoryMappedFiles;
using System.Text.Json;
using GWGUI.Emulation;
using GWGUI.Emulation.Functions;

namespace GWGUI.Emulation.Nec.Emulators.Common.Interop.Services;

internal sealed class ProcessCore : IEmulatorCore
{
    private readonly string _hostExecutablePath;
    private readonly string? _corePath;
    private readonly string _hostCommand;
    private readonly string _pipePrefix;
    private readonly string _videoMapPrefix;
    private readonly string _displayName;
    private readonly string _errorPrefix;
    private string Error(string name, params object[] arguments) => CoreExceptions.Text(_errorPrefix + "." + name, arguments);
    private readonly ConcurrentQueue<AudioChunk> _audio = new();
    private NamedPipeServerStream? _pipe;
    private BinaryReader? _responseReader;
    private BinaryWriter? _writer;
    private Process? _process;
    private MemoryMappedFile? _videoMemory;
    private MemoryMappedViewAccessor? _videoMap;
    private readonly InputAccumulator _input = new();
    private bool _initialized;
    private bool _disposed;
    private bool _connectionFailed;
    private MachineConfiguration? _configuration;
    private string? _sessionDirectory;
    private string? _saveDirectory;

    internal ProcessCore(string hostExecutablePath, string? corePath = null,
        string? hostCommand = null, string? pipePrefix = null, string? videoMapPrefix = null,
        string? displayName = null, string errorPrefix = "Emulation.Error.Core")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hostExecutablePath);
        _hostExecutablePath = Path.GetFullPath(hostExecutablePath);
        _corePath = corePath;
        _hostCommand = hostCommand ?? ProcessCoreConstants.CoreHost;
        _pipePrefix = pipePrefix ?? ProcessCoreConstants.PipePrefix;
        _videoMapPrefix = videoMapPrefix ?? ProcessCoreConstants.VideoMapPrefix;
        _displayName = displayName ?? CoreHostConstants.DisplayName;
        _errorPrefix = errorPrefix;
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
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_initialized) throw new InvalidOperationException(Error("ProcessAlreadyInitialized"));
        if (!File.Exists(_hostExecutablePath))
            throw new FileNotFoundException(Error("HostExecutableNotFound"), _hostExecutablePath);

        _configuration = configuration;
        _sessionDirectory = Path.GetFullPath(sessionDirectory);
        _saveDirectory = saveDirectory is null ? null : Path.GetFullPath(saveDirectory);
        var pipeName = $"{_pipePrefix}{Guid.NewGuid():N}";
        var videoMapName = $"{_videoMapPrefix}{Guid.NewGuid():N}";
        _videoMemory = MemoryMappedFile.CreateNew(videoMapName, CommonCoreHostFunctions.Protocol.VideoMapCapacity, MemoryMappedFileAccess.ReadWrite);
        try
        {
            _videoMap = _videoMemory.CreateViewAccessor(0, CommonCoreHostFunctions.Protocol.VideoMapCapacity, MemoryMappedFileAccess.ReadWrite);
            _pipe = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, ProcessCoreConstants.PipeBufferSize, ProcessCoreConstants.PipeBufferSize);
            var startInfo = CommonCoreHostFunctions.Protocol.CreateHostStartInfo(_hostExecutablePath,
                Path.GetDirectoryName(_hostExecutablePath)!, _hostCommand, pipeName, videoMapName);
            _process = CommonCoreHostFunctions.Protocol.StartHostProcess(startInfo, () => new InvalidOperationException(Error("ProcessStartFailed")));
            CommonCoreHostFunctions.Protocol.WaitForConnection(_pipe, TimeSpan.FromSeconds(15));
            _writer = new BinaryWriter(_pipe, System.Text.Encoding.UTF8, true);
            Begin(HostCommand.Initialize);
            CommonCoreHostFunctions.Protocol.WriteInitializationRequest(ref _writer, _corePath ?? string.Empty,
                () => _sessionDirectory,
                () => _saveDirectory,
                CoreHostProtocol.WriteString,
                () => JsonSerializer.Serialize(configuration, CoreHostProtocol.JsonOptions));
            CompleteRequest();
            CoreSha256 = Response.ReadString();
            FramesPerSecond = Response.ReadDouble();
            SampleRate = Response.ReadInt32();
            Options = JsonSerializer.Deserialize<IReadOnlyList<CoreOption>>(Response.ReadString(), CoreHostProtocol.JsonOptions) ?? [];
            Diagnostics = JsonSerializer.Deserialize<IReadOnlyList<string>>(Response.ReadString(), CoreHostProtocol.JsonOptions) ?? [];
            CoreName = Response.ReadString();
            CoreVersion = Response.ReadString();
            SupportedContentExtensions = Response.ReadString().Split('|', StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.OrdinalIgnoreCase);
            DiskCount = Response.ReadInt32();
            CurrentDiskIndex = Response.ReadInt32();
            LedStates = CommonCoreHostFunctions.Protocol.ReadLedStates(Response, _displayName);
            _initialized = true;
        }
        finally
        {
            if (!_initialized) ReleaseHost();
        }
    }

    public void RunFrame()
    {
        Begin(HostCommand.RunFrame);
        CoreHostProtocol.WriteInput(_writer!, _input.Consume());
        CompleteRequest();
        LatestVideoFrame = CommonCoreHostFunctions.Protocol.ReadSharedFrame(Response,
            _videoMap ?? throw new InvalidOperationException(Error("VideoBufferUnavailable")), _displayName) ?? LatestVideoFrame;
        foreach (var chunk in CommonCoreHostFunctions.Protocol.ReadAudio(Response, _displayName))
        {
            LatestAudioChunk = chunk;
            _audio.Enqueue(chunk);
        }
        FramesPerSecond = Response.ReadDouble();
        SampleRate = Response.ReadInt32();
        DiskCount = Response.ReadInt32();
        CurrentDiskIndex = Response.ReadInt32();
        if (Response.ReadBoolean())
            Diagnostics = JsonSerializer.Deserialize<IReadOnlyList<string>>(Response.ReadString(), CoreHostProtocol.JsonOptions) ?? [];
        LedStates = CommonCoreHostFunctions.Protocol.ReadLedStates(Response, _displayName);
    }

    public void HardReset()
    {
        var configuration = _configuration ?? throw new InvalidOperationException(Error("ProcessNotInitialized"));
        var sessionDirectory = _sessionDirectory ?? throw new InvalidOperationException(Error("ProcessNotInitialized"));
        var saveDirectory = _saveDirectory;
        ReleaseHost();
        Initialize(configuration, sessionDirectory, saveDirectory);
    }
    public void SoftReset() => SimpleRequest(HostCommand.SoftReset);
    public void Stop() => SimpleRequest(HostCommand.Stop);
    public void SetInput(EmulationInputSnapshot snapshot) => _input.Update(snapshot);
    public void InsertMedia(string path) => StringRequest(HostCommand.InsertMedia, Path.GetFullPath(path));
    public void EjectMedia() => SimpleRequest(HostCommand.EjectMedia);

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
        return CommonCoreHostFunctions.Protocol.ReadBytes(Response, _displayName);
    }

    public void LoadState(ReadOnlySpan<byte> state)
    {
        Begin(HostCommand.LoadState);
        CoreHostProtocol.WriteBytes(_writer!, state);
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
                    _writer!.Write((byte)HostCommand.Dispose);
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
                    CommonCoreHostFunctions.Protocol.WaitForHostExit(process, graceful: true, 5_000);
                }
                catch (Exception) { }
                finally { CommonCoreHostFunctions.Protocol.DisposeTransportIgnoringErrors(process); }
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
        if (_connectionFailed) throw new InvalidOperationException(Error("ProcessUnavailable"));
        if (command != HostCommand.Initialize && !_initialized)
            throw new InvalidOperationException(Error("ProcessNotInitialized"));
        _writer!.Write((byte)command);
    }

    private void CompleteRequest()
    {
        try
        {
            var response = ReadResponseAsync().GetAwaiter().GetResult();
            _responseReader?.Dispose();
            _responseReader = new BinaryReader(new MemoryStream(response, false), System.Text.Encoding.UTF8, false);
            if (!Response.ReadBoolean())
            {
                var kind = (GWGUI.Emulation.Nec.Common.Enums.CoreHostErrorKind)Response.ReadByte();
                var detail = Response.ReadString();
                throw kind == GWGUI.Emulation.Nec.Common.Enums.CoreHostErrorKind.MediaRequired ? new GWGUI.Emulation.Nec.Common.Exceptions.CoreMediaRequiredException(detail) : new InvalidOperationException(detail);
            }
        }
        catch (Exception error) when (error is IOException or EndOfStreamException or OperationCanceledException or InvalidDataException)
        {
            var timedOut = error is OperationCanceledException;
            var exit = _process is { HasExited: true } ? $" It exited with code {_process.ExitCode}." : string.Empty;
            _connectionFailed = true;
            TerminateProcess();
            throw new InvalidOperationException(timedOut ? Error("ProcessTimeout") : Error("ProcessCommunicationFailed", exit), error);
        }
    }

    private Task<byte[]> ReadResponseAsync() =>
        CommonCoreHostFunctions.Protocol.ReadResponseAsync(_pipe!, TimeSpan.FromSeconds(30),
            CancellationToken.None, length => Error("InvalidResponseLength", length));

    private void TerminateProcess()
    {
        CommonCoreHostFunctions.Protocol.DisposeTransportIgnoringErrors(_pipe);
        _pipe = null;
        var process = _process;
        _process = null;
        if (process is null) return;
        try
        {
            CommonCoreHostFunctions.Protocol.WaitForHostExit(process, graceful: false, 5_000);
        }
        catch (Exception) { }
        finally { CommonCoreHostFunctions.Protocol.DisposeTransportIgnoringErrors(process); }
    }

    private BinaryReader Response => _responseReader ?? throw new InvalidOperationException(Error("HostResponseUnavailable"));

}
