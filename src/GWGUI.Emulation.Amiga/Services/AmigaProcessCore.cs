using System.Collections.Concurrent;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO.Pipes;
using System.IO.MemoryMappedFiles;
using System.Text.Json;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga.Services;

internal sealed class AmigaProcessCore : IAmigaCore
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
    private readonly AmigaInputAccumulator _input = new();
    private bool _initialized;
    private bool _disposed;
    private bool _connectionFailed;

    internal AmigaProcessCore(string hostExecutablePath, string? corePath = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hostExecutablePath);
        _hostExecutablePath = Path.GetFullPath(hostExecutablePath);
        _corePath = corePath;
    }

    public VideoFrame? LatestVideoFrame { get; private set; }
    public AudioChunk? LatestAudioChunk { get; private set; }
    public IReadOnlyList<AmigaCoreOption> Options { get; private set; } = [];
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

    public void Initialize(AmigaMachineConfiguration configuration, string sessionDirectory, string? saveDirectory = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_initialized) throw new InvalidOperationException(AmigaProcessCoreConstants.TheAmigaCoreProcessIsAlreadyInitialized);
        if (!File.Exists(_hostExecutablePath))
            throw new FileNotFoundException(AmigaProcessCoreConstants.TheGWGUIExecutableUsedToHostTheAmigaCoreWasNotFound, _hostExecutablePath);

        var pipeName = $"gwgui-amiga-{Guid.NewGuid():N}";
        var videoMapName = $"gwgui-amiga-video-{Guid.NewGuid():N}";
        _videoMemory = MemoryMappedFile.CreateNew(videoMapName, EmulationHostProtocolConstants.VideoMapCapacity,
            MemoryMappedFileAccess.ReadWrite);
        _videoMap = _videoMemory.CreateViewAccessor(0, EmulationHostProtocolConstants.VideoMapCapacity,
            MemoryMappedFileAccess.ReadWrite);
        const int pipeBufferSize = 8 * 1024 * 1024;
        _pipe = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous, pipeBufferSize, pipeBufferSize);
        var startInfo = new ProcessStartInfo(_hostExecutablePath)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(_hostExecutablePath)!
        };
        startInfo.ArgumentList.Add(AmigaProcessCoreConstants.AmigaCoreHost);
        startInfo.ArgumentList.Add(pipeName);
        startInfo.ArgumentList.Add(videoMapName);
        _process = Process.Start(startInfo) ?? throw new InvalidOperationException(AmigaProcessCoreConstants.TheAmigaCoreHostProcessCouldNotBeStarted);
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            _pipe.WaitForConnectionAsync(timeout.Token).GetAwaiter().GetResult();
            _writer = new BinaryWriter(_pipe, System.Text.Encoding.UTF8, true);
            Begin(AmigaHostCommand.Initialize);
            _writer.Write(_corePath ?? string.Empty);
            _writer.Write(Path.GetFullPath(sessionDirectory));
            AmigaCoreHostProtocol.WriteString(_writer, saveDirectory is null ? null : Path.GetFullPath(saveDirectory));
            _writer.Write(JsonSerializer.Serialize(configuration, AmigaCoreHostProtocol.JsonOptions));
            CompleteRequest();
            CoreSha256 = Response.ReadString();
            FramesPerSecond = Response.ReadDouble();
            SampleRate = Response.ReadInt32();
            Options = JsonSerializer.Deserialize<IReadOnlyList<AmigaCoreOption>>(Response.ReadString(), AmigaCoreHostProtocol.JsonOptions) ?? [];
            Diagnostics = JsonSerializer.Deserialize<IReadOnlyList<string>>(Response.ReadString(), AmigaCoreHostProtocol.JsonOptions) ?? [];
            CoreName = Response.ReadString();
            CoreVersion = Response.ReadString();
            SupportedContentExtensions = Response.ReadString().Split('|', StringSplitOptions.RemoveEmptyEntries)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            DiskCount = Response.ReadInt32();
            CurrentDiskIndex = Response.ReadInt32();
            LedStates = AmigaCoreHostProtocol.ReadLedStates(Response);
            _initialized = true;
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    public void RunFrame()
    {
        Begin(AmigaHostCommand.RunFrame);
        AmigaCoreHostProtocol.WriteInput(_writer!, _input.Consume());
        CompleteRequest();
        LatestVideoFrame = AmigaCoreHostProtocol.ReadSharedFrame(Response,
            _videoMap ?? throw new InvalidOperationException(AmigaProcessCoreConstants.TheSharedAmigaVideoBufferIsUnavailable)) ?? LatestVideoFrame;
        foreach (var chunk in AmigaCoreHostProtocol.ReadAudio(Response))
        {
            LatestAudioChunk = chunk;
            _audio.Enqueue(chunk);
        }
        FramesPerSecond = Response.ReadDouble();
        SampleRate = Response.ReadInt32();
        DiskCount = Response.ReadInt32();
        CurrentDiskIndex = Response.ReadInt32();
        if (Response.ReadBoolean())
            Diagnostics = JsonSerializer.Deserialize<IReadOnlyList<string>>(Response.ReadString(), AmigaCoreHostProtocol.JsonOptions) ?? [];
        LedStates = AmigaCoreHostProtocol.ReadLedStates(Response);
    }

    public void HardReset() => SimpleRequest(AmigaHostCommand.HardReset);
    public void Stop() => SimpleRequest(AmigaHostCommand.Stop);
    public void SetInput(EmulationInputSnapshot snapshot) => _input.Update(snapshot);
    public void InsertMedia(string path) => StringRequest(AmigaHostCommand.InsertMedia, Path.GetFullPath(path));
    public void EjectMedia() => SimpleRequest(AmigaHostCommand.EjectMedia);

    public void SelectDisk(int index)
    {
        Begin(AmigaHostCommand.SelectDisk);
        _writer!.Write(index);
        CompleteRequest();
        CurrentDiskIndex = index;
    }

    public byte[] SaveState()
    {
        Begin(AmigaHostCommand.SaveState);
        CompleteRequest();
        return AmigaCoreHostProtocol.ReadBytes(Response);
    }

    public void LoadState(ReadOnlySpan<byte> state)
    {
        Begin(AmigaHostCommand.LoadState);
        AmigaCoreHostProtocol.WriteBytes(_writer!, state);
        CompleteRequest();
    }

    public void SetOption(string key, string value)
    {
        Begin(AmigaHostCommand.SetOption);
        _writer!.Write(key);
        _writer.Write(value);
        CompleteRequest();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (!_connectionFailed && _pipe?.IsConnected == true)
        {
            try
            {
                _writer!.Write((byte)AmigaHostCommand.Dispose);
                CompleteRequest();
            }
            catch (Exception) { }
        }
        _responseReader?.Dispose();
        _writer?.Dispose();
        _pipe?.Dispose();
        _videoMap?.Dispose();
        _videoMemory?.Dispose();
        if (_process is not null)
        {
            try
            {
                if (!_process.WaitForExit(5_000)) _process.Kill(true);
                _process.WaitForExit(5_000);
            }
            catch (Exception) { }
            _process.Dispose();
        }
        while (_audio.TryDequeue(out _)) { }
    }

    private void StringRequest(AmigaHostCommand command, string value)
    {
        Begin(command);
        _writer!.Write(value);
        CompleteRequest();
    }

    private void SimpleRequest(AmigaHostCommand command)
    {
        Begin(command);
        CompleteRequest();
    }

    private void Begin(AmigaHostCommand command)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_connectionFailed) throw new InvalidOperationException(AmigaProcessCoreConstants.TheAmigaCoreProcessIsNoLongerAvailable);
        if (command != AmigaHostCommand.Initialize && !_initialized)
            throw new InvalidOperationException(AmigaProcessCoreConstants.TheAmigaCoreProcessIsNotInitialized);
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
                ? AmigaProcessCoreConstants.TheAmigaCoreProcessDidNotAnswerWithin30SecondsAndWasStopped
                : $"Communication with the Amiga core process failed.{exit}", error);
        }
    }

    private async Task<byte[]> ReadResponseAsync()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var header = new byte[sizeof(int)];
        await _pipe!.ReadExactlyAsync(header, timeout.Token).ConfigureAwait(false);
        var length = BinaryPrimitives.ReadInt32LittleEndian(header);
        if (length is < 0 or > EmulationHostProtocolConstants.MaximumBlobLength)
            throw new InvalidDataException($"The Amiga core process sent invalid response length {length}.");
        var response = GC.AllocateUninitializedArray<byte>(length);
        await _pipe.ReadExactlyAsync(response, timeout.Token).ConfigureAwait(false);
        return response;
    }

    private void TerminateProcess()
    {
        _pipe?.Dispose();
        if (_process is null) return;
        try
        {
            if (!_process.HasExited) _process.Kill(true);
            _process.WaitForExit(5_000);
        }
        catch (Exception) { }
    }

    private BinaryReader Response => _responseReader ?? throw new InvalidOperationException(AmigaProcessCoreConstants.TheAmigaHostResponseIsUnavailable);

}
