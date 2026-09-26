using GWGUI.Emulation.Amstrad.Emulators.Caprice32.Exceptions;
using GWGUI.Emulation.Amstrad.Emulators.Caprice32.Constants;
using GWGUI.Emulation.Amstrad.Emulators.Caprice32.Contracts;
using GWGUI.Emulation.Amstrad.Emulators.Caprice32.Factories;
using GWGUI.Emulation.Amstrad.Emulators.Caprice32.Functions;
using GWGUI.Emulation.Amstrad.Emulators.Caprice32.Services;

using System.IO;
using System.Collections.Concurrent;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO.Pipes;
using System.IO.MemoryMappedFiles;
using System.Text.Json;
using GWGUI.Emulation;
using GWGUI.Emulation.Functions;

namespace GWGUI.Emulation.Amstrad.Emulators.Caprice32.Services;

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
    private readonly InputAccumulator _input = new();
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
    {
        if (_audio.TryDequeue(out var value))
        {
            chunk = value;
            return true;
        }
        chunk = null;
        return false;
    }

    public void Initialize(MachineConfiguration configuration, string sessionDirectory, string? saveDirectory = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_initialized) throw new InvalidOperationException(Caprice32Exceptions.ProcessAlreadyInitialized());
        if (!File.Exists(_hostExecutablePath))
            throw new FileNotFoundException(Caprice32Exceptions.HostExecutableNotFound(), _hostExecutablePath);

        _configuration = configuration;
        _sessionDirectory = Path.GetFullPath(sessionDirectory);
        _saveDirectory = saveDirectory is null ? null : Path.GetFullPath(saveDirectory);
        var pipeName = $"{ProcessCoreConstants.PipePrefix}{Guid.NewGuid():N}";
        var videoMapName = $"{ProcessCoreConstants.VideoMapPrefix}{Guid.NewGuid():N}";
        _videoMemory = MemoryMappedFile.CreateNew(videoMapName, EmulationHostProtocolConstants.VideoMapCapacity,
            MemoryMappedFileAccess.ReadWrite);
        try
        {
            _videoMap = _videoMemory.CreateViewAccessor(0,
                EmulationHostProtocolConstants.VideoMapCapacity, MemoryMappedFileAccess.ReadWrite);
            _pipe = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1,
                PipeTransmissionMode.Byte, PipeOptions.Asynchronous,
                ProcessCoreConstants.PipeBufferSize, ProcessCoreConstants.PipeBufferSize);
            var startInfo = new ProcessStartInfo(_hostExecutablePath)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(_hostExecutablePath)!
            };
            startInfo.ArgumentList.Add(ProcessCoreConstants.CoreHost);
            startInfo.ArgumentList.Add(pipeName);
            startInfo.ArgumentList.Add(videoMapName);
            _process = Process.Start(startInfo)
                ?? throw new InvalidOperationException(Caprice32Exceptions.ProcessStartFailed());
            EmulationChildProcessLifetime.Attach(_process);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            _pipe.WaitForConnectionAsync(timeout.Token).GetAwaiter().GetResult();
            _writer = new BinaryWriter(_pipe, System.Text.Encoding.UTF8, true);
            Begin(HostCommand.Initialize);
            _writer.Write(_corePath ?? string.Empty);
            _writer.Write(_sessionDirectory);
            CoreHostProtocol.WriteString(_writer, _saveDirectory);
            _writer.Write(JsonSerializer.Serialize(configuration, CoreHostProtocol.JsonOptions));
            CompleteRequest();
            CoreSha256 = Response.ReadString();
            FramesPerSecond = Response.ReadDouble();
            SampleRate = Response.ReadInt32();
            Options = JsonSerializer.Deserialize<IReadOnlyList<CoreOption>>(Response.ReadString(), CoreHostProtocol.JsonOptions) ?? [];
            Diagnostics = JsonSerializer.Deserialize<IReadOnlyList<string>>(Response.ReadString(), CoreHostProtocol.JsonOptions) ?? [];
            CoreName = Response.ReadString();
            CoreVersion = Response.ReadString();
            SupportedContentExtensions = Response.ReadString().Split('|', StringSplitOptions.RemoveEmptyEntries)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            DiskCount = Response.ReadInt32();
            CurrentDiskIndex = Response.ReadInt32();
            LedStates = CoreHostProtocol.ReadLedStates(Response);
            _initialized = true;
        }
        catch
        {
            ReleaseHost();
            throw;
        }
    }

    public void RunFrame()
    {
        Begin(HostCommand.RunFrame);
        CoreHostProtocol.WriteInput(_writer!, _input.Consume());
        CompleteRequest();
        LatestVideoFrame = CoreHostProtocol.ReadSharedFrame(Response,
            _videoMap ?? throw new InvalidOperationException(Caprice32Exceptions.VideoBufferUnavailable())) ?? LatestVideoFrame;
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
    }

    public void HardReset()
    {
        var configuration = _configuration
            ?? throw new InvalidOperationException(Caprice32Exceptions.ProcessNotInitialized());
        var sessionDirectory = _sessionDirectory
            ?? throw new InvalidOperationException(Caprice32Exceptions.ProcessNotInitialized());
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
            DisposeSafely(_responseReader); _responseReader = null;
            DisposeSafely(_writer); _writer = null;
            DisposeSafely(_pipe); _pipe = null;
            DisposeSafely(_videoMap); _videoMap = null;
            DisposeSafely(_videoMemory); _videoMemory = null;
            var process = _process;
            _process = null;
            if (process is not null)
            {
                try
                {
                    if (!process.WaitForExit(5_000)) process.Kill(true);
                    process.WaitForExit(5_000);
                }
                catch (Exception) { }
                finally { DisposeSafely(process); }
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
        if (_connectionFailed) throw new InvalidOperationException(Caprice32Exceptions.ProcessUnavailable());
        if (command != HostCommand.Initialize && !_initialized)
            throw new InvalidOperationException(Caprice32Exceptions.ProcessNotInitialized());
        _writer!.Write((byte)command);
    }

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
            throw new InvalidOperationException(timedOut
                ? Caprice32Exceptions.ProcessTimeout()
                : Caprice32Exceptions.ProcessCommunicationFailed(exit), error);
        }
    }

    private async Task<byte[]> ReadResponseAsync()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var header = new byte[sizeof(int)];
        await _pipe!.ReadExactlyAsync(header, timeout.Token).ConfigureAwait(false);
        var length = BinaryPrimitives.ReadInt32LittleEndian(header);
        if (length is < 0 or > EmulationHostProtocolConstants.MaximumBlobLength)
            throw new InvalidDataException(Caprice32Exceptions.InvalidResponseLength(length));
        var response = GC.AllocateUninitializedArray<byte>(length);
        await _pipe.ReadExactlyAsync(response, timeout.Token).ConfigureAwait(false);
        return response;
    }

    private void TerminateProcess()
    {
        DisposeSafely(_pipe);
        _pipe = null;
        var process = _process;
        _process = null;
        if (process is null) return;
        try
        {
            if (!process.HasExited) process.Kill(true);
            process.WaitForExit(5_000);
        }
        catch (Exception) { }
        finally { DisposeSafely(process); }
    }

    private static void DisposeSafely(IDisposable? value)
    {
        try { value?.Dispose(); }
        catch (Exception) { }
    }

    private BinaryReader Response => _responseReader ?? throw new InvalidOperationException(Caprice32Exceptions.HostResponseUnavailable());

}
