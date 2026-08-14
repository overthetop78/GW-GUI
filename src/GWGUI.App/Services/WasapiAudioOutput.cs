using System.Runtime.InteropServices;
using GWGUI.Emulation;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace GWGUI.App.Services;

public sealed class WasapiAudioOutput : IAudioOutput
{
    private readonly string? _deviceId;
    private readonly int _latencyMilliseconds;
    private WasapiOut? _device;
    private BufferedWaveProvider? _buffer;
    private byte[] _writeBuffer = [];
    private bool _disposed;

    public WasapiAudioOutput(string? deviceId = null, int latencyMilliseconds = 50)
    {
        _deviceId = string.IsNullOrWhiteSpace(deviceId) ? null : deviceId;
        _latencyMilliseconds = Math.Clamp(latencyMilliseconds, 10, 500);
    }

    public static IReadOnlyList<AudioOutputDevice> GetOutputDevices()
    {
        try
        {
            using var enumerator = new MMDeviceEnumerator();
            return enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)
                .Select(device => new AudioOutputDevice(device.ID, device.FriendlyName))
                .OrderBy(device => device.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToArray();
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
        _buffer = new BufferedWaveProvider(new WaveFormat(sampleRate, 16, 2))
        {
            BufferDuration = TimeSpan.FromMilliseconds(250),
            DiscardOnBufferOverflow = true,
            ReadFully = true
        };
        if (_deviceId is null)
            _device = new WasapiOut(AudioClientShareMode.Shared, false, _latencyMilliseconds);
        else
        {
            using var enumerator = new MMDeviceEnumerator();
            var endpoint = enumerator.GetDevice(_deviceId);
            _device = new WasapiOut(endpoint, AudioClientShareMode.Shared, false, _latencyMilliseconds);
        }
        _device.Init(_buffer);
        _device.Play();
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
        _device?.Stop();
        _device?.Dispose();
        _device = null;
        _buffer = null;
        _writeBuffer = [];
    }

    public void Dispose()
    {
        if (_disposed) return;
        Stop();
        _disposed = true;
    }
}

public sealed record AudioOutputDevice(string Id, string Name)
{
    public override string ToString() => Name;
}
