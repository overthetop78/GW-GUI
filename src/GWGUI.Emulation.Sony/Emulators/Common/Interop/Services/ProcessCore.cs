using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Sony.Common.Functions.CoreHostFunctions;
using CommonControllerFunctions = GWGUI.Emulation.Sony.Common.Functions.ControllerFunctions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Exceptions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Enums;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;

using System.IO;
using System.Collections.Concurrent;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO.Pipes;
using System.IO.MemoryMappedFiles;
using System.Text.Json;
using GWGUI.Emulation;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Dictionaries;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;

internal sealed class ProcessCore : IEmulatorCore
{
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
    private TimeSpan _requestTimeout = TimeSpan.FromSeconds(ProcessCoreConstants.ResponseTimeoutSeconds);

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
    public bool ShutdownRequested { get; private set; }
    public IReadOnlyList<ControllerFeedbackState> ControllerFeedback { get; private set; } = [];
    public string CoreName { get; private set; } = string.Empty;
    public string CoreVersion { get; private set; } = string.Empty;
    public IReadOnlySet<string> SupportedContentExtensions { get; private set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public string CoreSha256 { get; private set; } = string.Empty;
    public double FramesPerSecond { get; private set; } = ExternalCoreConstants.FallbackFramesPerSecond;
    public int SampleRate { get; private set; } = ExternalCoreConstants.FallbackSampleRate;
    public int DiskCount { get; private set; }
    public int CurrentDiskIndex { get; private set; } = -1;

    public bool TryDequeueAudio(out AudioChunk? chunk)
    => _audio.TryDequeue(out chunk);

    public void Initialize(MachineConfiguration configuration, string sessionDirectory, string? saveDirectory = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_initialized) throw new InvalidOperationException(Error(CoreExceptionsResourceConstants.ProcessAlreadyInitialized));
        if (!File.Exists(_hostExecutablePath))
            throw new FileNotFoundException(Error(CoreExceptionsResourceConstants.HostExecutableNotFound), _hostExecutablePath);

        _configuration = configuration;
        _sessionDirectory = Path.GetFullPath(sessionDirectory);
        _saveDirectory = saveDirectory is null ? null : Path.GetFullPath(saveDirectory);
        var pipeName = $"{ProcessCoreConstants.PipePrefix}{Guid.NewGuid():N}";
        var videoMapName = $"{ProcessCoreConstants.VideoMapPrefix}{Guid.NewGuid():N}";
        _videoMemory = MemoryMappedFile.CreateNew(videoMapName, CommonCoreHostFunctions.Protocol.VideoMapCapacity, MemoryMappedFileAccess.ReadWrite);
        try
        {
            _videoMap = _videoMemory.CreateViewAccessor(0, CommonCoreHostFunctions.Protocol.VideoMapCapacity, MemoryMappedFileAccess.ReadWrite);
            _pipe = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, ProcessCoreConstants.PipeBufferSize, ProcessCoreConstants.PipeBufferSize);
            var startInfo = CommonCoreHostFunctions.Protocol.CreateHostStartInfo(_hostExecutablePath,
                Path.GetDirectoryName(_hostExecutablePath)!, ProcessCoreConstants.CoreHost, pipeName, videoMapName);
            _process = CommonCoreHostFunctions.Protocol.StartHostProcess(startInfo, () => new InvalidOperationException(Error(CoreExceptionsResourceConstants.ProcessStartFailed)));
            CommonCoreHostFunctions.Protocol.WaitForConnection(_pipe, TimeSpan.FromSeconds(ProcessCoreConstants.ConnectionTimeoutSeconds));
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
            var definition = CoreCatalog.Get(configuration.EmulatorId);
            Options = CoreOptionMetadataFunctions.Merge(Options, definition.ResolveOptions?.Invoke(configuration) ?? definition.Options, configuration.GraphicsApi);
            Diagnostics = JsonSerializer.Deserialize<IReadOnlyList<string>>(Response.ReadString(), CoreHostProtocol.JsonOptions) ?? [];
            CoreName = Response.ReadString();
            CoreVersion = Response.ReadString();
            SupportedContentExtensions = Response.ReadString().Split('|', StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.OrdinalIgnoreCase);
            DiskCount = Response.ReadInt32();
            CurrentDiskIndex = Response.ReadInt32();
            LedStates = CoreHostProtocol.ReadLedStates(Response);
            ShutdownRequested = Response.ReadBoolean();
            ControllerFeedback = ControllerFeedbackProtocol.Read(Response);
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
        LatestVideoFrame = CoreHostProtocol.ReadSharedFrame(Response, _videoMap ?? throw new InvalidOperationException(Error(CoreExceptionsResourceConstants.VideoBufferUnavailable))) ?? LatestVideoFrame;
        foreach (var chunk in CoreHostProtocol.ReadAudio(Response))
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
        LedStates = CoreHostProtocol.ReadLedStates(Response);
        ShutdownRequested = Response.ReadBoolean();
        ControllerFeedback = ControllerFeedbackProtocol.Read(Response);
    }

    public void HardReset()
    {
        var configuration = _configuration ?? throw new InvalidOperationException(Error(CoreExceptionsResourceConstants.ProcessNotInitialized));
        var sessionDirectory = _sessionDirectory ?? throw new InvalidOperationException(Error(CoreExceptionsResourceConstants.ProcessNotInitialized));
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
        return CoreHostProtocol.ReadBytes(Response);
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

    public void SetControllerPortDevice(int port, ControllerType type)
    {
        var configuration = _configuration ?? throw new InvalidOperationException(Error(CoreExceptionsResourceConstants.ProcessNotInitialized));
        CommonControllerFunctions.Validate(configuration, port, type);
        Begin(HostCommand.SetControllerPortDevice);
        _writer!.Write(port);
        _writer.Write((int)type);
        CompleteRequest();
        _configuration = CommonControllerFunctions.WithControllerType(configuration, port, type);
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
                    _requestTimeout = RequestTimeoutFor(HostCommand.Dispose);
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
            try
            {
                if (process is not null)
                    CommonCoreHostFunctions.Protocol.WaitForHostExit(process, graceful: true, ProcessCoreConstants.GracefulShutdownTimeoutMilliseconds);
            }
            finally
            {
                CommonCoreHostFunctions.Protocol.DisposeTransportIgnoringErrors(process);
                while (_audio.TryDequeue(out _)) { }
                LatestVideoFrame = null;
                LatestAudioChunk = null;
                Options = [];
                Diagnostics = [];
                ShutdownRequested = false;
                ControllerFeedback = [];
                LedStates = new Dictionary<int, bool>();
                DiskCount = 0;
                CurrentDiskIndex = -1;
                _input.Reset();
                _initialized = false;
                _connectionFailed = false;
            }
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
        if (_connectionFailed) throw new InvalidOperationException(Error(CoreExceptionsResourceConstants.ProcessUnavailable));
        if (command != HostCommand.Initialize && !_initialized)
            throw new InvalidOperationException(Error(CoreExceptionsResourceConstants.ProcessNotInitialized));
        _requestTimeout = RequestTimeoutFor(command);
        _writer!.Write((byte)command);
    }

    private TimeSpan RequestTimeoutFor(HostCommand command) => CoreTimeoutFunctions.For(_configuration is null ? null : CoreCatalog.Get(_configuration.EmulatorId), command);

    private void CompleteRequest()
    {
        try
        {
            var response = ReadResponseAsync().GetAwaiter().GetResult();
            _responseReader?.Dispose();
            _responseReader = new BinaryReader(new MemoryStream(response, false), System.Text.Encoding.UTF8, false);
            if (!Response.ReadBoolean()) throw new InvalidOperationException(Response.ReadString());
        }
        catch (Exception error) when (error is IOException or EndOfStreamException or OperationCanceledException or InvalidDataException)
        {
            var timedOut = error is OperationCanceledException;
            var exit = _process is { HasExited: true } ? $" It exited with code {_process.ExitCode}." : string.Empty;
            _connectionFailed = true;
            TerminateProcess();
            throw new InvalidOperationException(timedOut ? Error(CoreExceptionsResourceConstants.ProcessTimeout) : Error(CoreExceptionsResourceConstants.ProcessCommunicationFailed, exit), error);
        }
    }

    private Task<byte[]> ReadResponseAsync() =>
        CommonCoreHostFunctions.Protocol.ReadResponseAsync(_pipe!, _requestTimeout,
            CancellationToken.None, length => Error(CoreExceptionsResourceConstants.InvalidResponseLength, length));

    private void TerminateProcess()
    {
        CommonCoreHostFunctions.Protocol.DisposeTransportIgnoringErrors(_pipe);
        _pipe = null;
        var process = _process;
        _process = null;
        if (process is null) return;
        try
        {
            CommonCoreHostFunctions.Protocol.WaitForHostExit(process, graceful: false, ProcessCoreConstants.GracefulShutdownTimeoutMilliseconds);
        }
        finally { CommonCoreHostFunctions.Protocol.DisposeTransportIgnoringErrors(process); }
    }

    private string Error(string key, params object[] arguments) => CoreExceptions.ForEmulator(_configuration?.EmulatorId, key, arguments);

    private BinaryReader Response => _responseReader ?? throw new InvalidOperationException(Error(CoreExceptionsResourceConstants.HostResponseUnavailable));

}
