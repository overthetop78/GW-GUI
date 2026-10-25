using GWGUI.App.Contracts.Services.Audio;
using System.Runtime.InteropServices;
using GWGUI.Emulation;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Interfaces;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace GWGUI.App.Services.Audio;

public sealed class WasapiAudioOutput : IMultichannelAudioOutput
{
    private readonly string? _deviceId;
    private readonly int _latencyMilliseconds;
    private IWavePlayer? _device;
    private MMDevice? _endpoint;
    private readonly Func<string?, int, IWavePlayer> _createDevice;
    private BufferedWaveProvider? _buffer;
    private byte[] _writeBuffer = [];
    private bool _disposed;
    private int _minimumLatencyMilliseconds;
    private EmulationAudioChannelLayout _channelLayout = EmulationAudioChannelLayout.Stereo;
    private const int MaximumLatencyMilliseconds = 500;

    public WasapiAudioOutput(string? deviceId = null, int latencyMilliseconds = 50)
    {
        _deviceId = string.IsNullOrWhiteSpace(deviceId) ? null : deviceId;
        _latencyMilliseconds = Math.Clamp(latencyMilliseconds, 10, 500);
        _createDevice = CreateDevice;
    }

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
            var layout = _channelLayout;
            Stop();
            Start(sampleRate, layout);
        }
        return true;
    }

    internal WasapiAudioOutput(string? deviceId, int latencyMilliseconds, Func<string?, int, IWavePlayer> createDevice)
        : this(deviceId, latencyMilliseconds)
    {
        _createDevice = createDevice ?? throw new ArgumentNullException(nameof(createDevice));
    }

    public static IReadOnlyList<AudioOutputDevice> GetOutputDevices()
    {
        try
        {
            using var enumerator = new MMDeviceEnumerator();
            var outputs = new List<AudioOutputDevice>();
            foreach (var device in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
                using (device) outputs.Add(new AudioOutputDevice(device.ID, device.FriendlyName));
            return outputs.OrderBy(device => device.Name, StringComparer.CurrentCultureIgnoreCase).ToArray();
        }
        catch (COMException)
        {
            return [];
        }
    }

    public void Start(int sampleRate) => Start(sampleRate, EmulationAudioChannelLayout.Stereo);

    public void Start(int sampleRate, EmulationAudioChannelLayout channelLayout)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_device is not null) throw new InvalidOperationException("Audio output is already started.");
        var format = new AudioChunk(ReadOnlyMemory<short>.Empty, sampleRate, 0, 0, TimeSpan.Zero, channelLayout);
        if (!format.HasValidLength) throw new ArgumentOutOfRangeException(nameof(channelLayout));
        var completed = false;
        try
        {
            _channelLayout = channelLayout;
            _buffer = new BufferedWaveProvider(new WaveFormatExtensible(sampleRate, 16, format.ChannelCount, (int)channelLayout))
            {
                BufferDuration = TimeSpan.FromMilliseconds(Math.Max(250, _minimumLatencyMilliseconds)),
                DiscardOnBufferOverflow = true,
                ReadFully = true
            };
            _device = _createDevice(_deviceId, Math.Max(_latencyMilliseconds, _minimumLatencyMilliseconds));
            _device.Init(_buffer);
            _device.Play();
            completed = true;
        }
        finally { if (!completed) Stop(); }
    }

    private IWavePlayer CreateDevice(string? deviceId, int latencyMilliseconds)
    {
        using var enumerator = new MMDeviceEnumerator();
        var endpoint = deviceId is null ? enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console) : enumerator.GetDevice(deviceId);
        try
        {
            var device = new WasapiOut(endpoint, AudioClientShareMode.Shared, false, latencyMilliseconds);
            _endpoint = endpoint;
            return device;
        }
        catch
        {
            endpoint.Dispose();
            throw;
        }
    }

    public void Write(ReadOnlySpan<short> interleavedSamples)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_buffer is null) throw new InvalidOperationException("Audio output is not started.");
        if (interleavedSamples.Length % _buffer.WaveFormat.Channels != 0) throw new ArgumentException(nameof(interleavedSamples));
        var bytes = MemoryMarshal.AsBytes(interleavedSamples);
        if (_writeBuffer.Length < bytes.Length) _writeBuffer = new byte[bytes.Length];
        bytes.CopyTo(_writeBuffer);
        _buffer.AddSamples(_writeBuffer, 0, bytes.Length);
    }

    public void Flush() => _buffer?.ClearBuffer();

    public void Stop()
    {
        var device = _device;
        var endpoint = _endpoint;
        _device = null;
        _endpoint = null;
        try { device?.Stop(); }
        finally
        {
            try { device?.Dispose(); }
            finally
            {
                try { endpoint?.Dispose(); }
                finally { _buffer = null; _writeBuffer = []; }
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        try { Stop(); }
        finally { _disposed = true; }
    }
}

