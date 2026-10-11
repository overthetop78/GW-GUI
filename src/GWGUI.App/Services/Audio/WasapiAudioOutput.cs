using GWGUI.App.Contracts.Services.Audio;
using System.Runtime.InteropServices;
using GWGUI.Emulation;
using GWGUI.Emulation.Contracts;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace GWGUI.App.Services.Audio;

public sealed class WasapiAudioOutput : IAudioOutput
{
    private readonly string? _deviceId;
    private readonly int _latencyMilliseconds;
    private IWavePlayer? _device;
    private readonly Func<string?, int, IWavePlayer> _createDevice;
    private BufferedWaveProvider? _buffer;
    private byte[] _writeBuffer = [];
    private bool _disposed;
    private int _minimumLatencyMilliseconds;
    private const int MaximumLatencyMilliseconds = 500;

    public WasapiAudioOutput(string? deviceId = null, int latencyMilliseconds = 50) : this(deviceId, latencyMilliseconds, CreateDevice) { }

    public AudioOutputBufferStatus? BufferStatus
    {
        get
        {
            var buffer = _buffer;
            if (buffer is null) return null;
            return new AudioOutputBufferStatus(buffer.WaveFormat.SampleRate, buffer.BufferedBytes / buffer.WaveFormat.BlockAlign, buffer.BufferLength / buffer.WaveFormat.BlockAlign, _device?.PlaybackState == PlaybackState.Playing);
        }
    }

    public bool TrySetMinimumLatency(int latencyMilliseconds)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (latencyMilliseconds < 0 || latencyMilliseconds > MaximumLatencyMilliseconds) return false;
        if (_minimumLatencyMilliseconds == latencyMilliseconds) return true;
        var previousLatency = Math.Max(_latencyMilliseconds, _minimumLatencyMilliseconds);
        _minimumLatencyMilliseconds = latencyMilliseconds;
        if (_buffer is { } buffer && previousLatency != Math.Max(_latencyMilliseconds, _minimumLatencyMilliseconds))
        {
            var sampleRate = buffer.WaveFormat.SampleRate;
            Stop();
            Start(sampleRate);
        }
        return true;
    }

    internal WasapiAudioOutput(string? deviceId, int latencyMilliseconds, Func<string?, int, IWavePlayer> createDevice)
    {
        _createDevice = createDevice ?? throw new ArgumentNullException(nameof(createDevice));
        _deviceId = string.IsNullOrWhiteSpace(deviceId) ? null : deviceId;
        _latencyMilliseconds = Math.Clamp(latencyMilliseconds, 10, 500);
    }

    public static IReadOnlyList<AudioOutputDevice> GetOutputDevices()
    {
        try
        {
            using var enumerator = new MMDeviceEnumerator();
            return enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active) .Select(device => new AudioOutputDevice(device.ID, device.FriendlyName)) .OrderBy(device => device.Name, StringComparer.CurrentCultureIgnoreCase) .ToArray();
        }
        catch (COMException)
        {
            return [];
        }
    }

    public void Start(int sampleRate)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_device is not null) throw new InvalidOperationException("Audio output is already started.");
        var completed = false;
        try
        {
            _buffer = new BufferedWaveProvider(new WaveFormat(sampleRate, 16, 2)) { BufferDuration = TimeSpan.FromMilliseconds(Math.Max(250, _minimumLatencyMilliseconds)), DiscardOnBufferOverflow = true, ReadFully = true };
            _device = _createDevice(_deviceId, Math.Max(_latencyMilliseconds, _minimumLatencyMilliseconds));
            _device.Init(_buffer);
            _device.Play();
            completed = true;
        }
        finally { if (!completed) Stop(); }
    }

    private static IWavePlayer CreateDevice(string? deviceId, int latencyMilliseconds)
    {
        if (deviceId is null) return new WasapiOut(AudioClientShareMode.Shared, false, latencyMilliseconds);
        using var enumerator = new MMDeviceEnumerator();
        var endpoint = enumerator.GetDevice(deviceId);
        return new WasapiOut(endpoint, AudioClientShareMode.Shared, false, latencyMilliseconds);
    }

    public void Write(ReadOnlySpan<short> interleavedStereo)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_buffer is null) throw new InvalidOperationException("Audio output is not started.");
        var bytes = MemoryMarshal.AsBytes(interleavedStereo);
        if (_writeBuffer.Length < bytes.Length) _writeBuffer = new byte[bytes.Length];
        bytes.CopyTo(_writeBuffer);
        _buffer.AddSamples(_writeBuffer, 0, bytes.Length);
    }

    public void Flush() => _buffer?.ClearBuffer();

    public void Stop()
    {
        var device = _device;
        _device = null;
        try { device?.Stop(); }
        finally
        {
            try { device?.Dispose(); }
            finally { _buffer = null; _writeBuffer = []; }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        try { Stop(); }
        finally { _disposed = true; }
    }
}

