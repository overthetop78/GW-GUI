using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text.Json;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga.Cores;

internal sealed class AmigaProcessCore : IAmigaCore
{
    private readonly string _hostExecutablePath;
    private readonly string? _corePath;
    private readonly ConcurrentQueue<AudioChunk> _audio = new();
    private NamedPipeServerStream? _pipe;
    private BinaryReader? _reader;
    private BinaryReader? _responseReader;
    private BinaryWriter? _writer;
    private Process? _process;
    private EmulationInputSnapshot _input = EmulationInputSnapshot.Empty;
    private bool _initialized;
    private bool _disposed;

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
        if (_initialized) throw new InvalidOperationException("The Amiga core process is already initialized.");
        if (!File.Exists(_hostExecutablePath))
            throw new FileNotFoundException("The GW GUI executable used to host the Amiga core was not found.", _hostExecutablePath);

        var pipeName = $"gwgui-amiga-{Guid.NewGuid():N}";
        const int pipeBufferSize = 8 * 1024 * 1024;
        _pipe = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous, pipeBufferSize, pipeBufferSize);
        var startInfo = new ProcessStartInfo(_hostExecutablePath)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(_hostExecutablePath)!
        };
        startInfo.ArgumentList.Add("--amiga-core-host");
        startInfo.ArgumentList.Add(pipeName);
        _process = Process.Start(startInfo) ?? throw new InvalidOperationException("The Amiga core host process could not be started.");
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            _pipe.WaitForConnectionAsync(timeout.Token).GetAwaiter().GetResult();
            _reader = new BinaryReader(_pipe, System.Text.Encoding.UTF8, true);
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
            DiskCount = Response.ReadInt32();
            CurrentDiskIndex = Response.ReadInt32();
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
        AmigaCoreHostProtocol.WriteInput(_writer!, Volatile.Read(ref _input));
        CompleteRequest();
        LatestVideoFrame = AmigaCoreHostProtocol.ReadFrame(Response) ?? LatestVideoFrame;
        foreach (var chunk in AmigaCoreHostProtocol.ReadAudio(Response))
        {
            LatestAudioChunk = chunk;
            _audio.Enqueue(chunk);
        }
        DiskCount = Response.ReadInt32();
        CurrentDiskIndex = Response.ReadInt32();
    }

    public void HardReset() => SimpleRequest(AmigaHostCommand.HardReset);
    public void Stop() => SimpleRequest(AmigaHostCommand.Stop);
    public void SetInput(EmulationInputSnapshot snapshot) => Volatile.Write(ref _input, snapshot ?? EmulationInputSnapshot.Empty);
    public void InsertFloppy(string path) => StringRequest(AmigaHostCommand.InsertFloppy, Path.GetFullPath(path));
    public void EjectFloppy() => SimpleRequest(AmigaHostCommand.EjectFloppy);

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
        if (_pipe?.IsConnected == true)
        {
            try
            {
                _writer!.Write((byte)AmigaHostCommand.Dispose);
                CompleteRequest();
            }
            catch (Exception) { }
        }
        _reader?.Dispose();
        _responseReader?.Dispose();
        _writer?.Dispose();
        _pipe?.Dispose();
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
        if (command != AmigaHostCommand.Initialize && !_initialized)
            throw new InvalidOperationException("The Amiga core process is not initialized.");
        _writer!.Write((byte)command);
    }

    private void CompleteRequest()
    {
        try
        {
            var response = AmigaCoreHostProtocol.ReadBytes(_reader!);
            _responseReader?.Dispose();
            _responseReader = new BinaryReader(new MemoryStream(response, false), System.Text.Encoding.UTF8, false);
            if (!Response.ReadBoolean()) throw new InvalidOperationException(Response.ReadString());
        }
        catch (Exception error) when (error is IOException or EndOfStreamException)
        {
            var exit = _process is { HasExited: true } ? $" It exited with code {_process.ExitCode}." : string.Empty;
            throw new InvalidOperationException($"Communication with the Amiga core process failed.{exit}", error);
        }
    }

    private BinaryReader Response => _responseReader ?? throw new InvalidOperationException("The Amiga host response is unavailable.");

}
