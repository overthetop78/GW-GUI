using System.Runtime.InteropServices;
using GWGUI.Emulation;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace GWGUI.App.Services;

public sealed class WasapiAudioOutput : IAudioOutput
{
    private WasapiOut? _device;
    private BufferedWaveProvider? _buffer;
    private byte[] _writeBuffer = [];
    private bool _disposed;

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
        _device = new WasapiOut(AudioClientShareMode.Shared, false, 50);
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
