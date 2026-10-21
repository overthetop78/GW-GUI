using System.Buffers;

namespace GWGUI.Emulation.Services;

internal class EmulationVideoBufferSet : IEmulationVideoBufferSet
{
    private readonly ArrayPool<byte> _pool;
    private readonly byte[]?[] _buffers;
    private int _nextBuffer = 0;

    internal EmulationVideoBufferSet(int bufferCount, ArrayPool<byte>? pool = null)
    {
        _pool = pool ?? ArrayPool<byte>.Shared;
        _buffers = new byte[bufferCount][];
    }

    public byte[] Rent(int length)
    {
        var index = _nextBuffer;
        _nextBuffer = (_nextBuffer + 1) % _buffers.Length;
        var buffer = _buffers[index];
        if (buffer is not null && buffer.Length >= length) return buffer;
        _buffers[index] = null;
        if (buffer is not null) _pool.Return(buffer);
        buffer = _pool.Rent(length);
        _buffers[index] = buffer;
        return buffer;
    }

    public void Dispose()
    {
        Exception? failure = null;
        for (var index = 0; index < _buffers.Length; index++)
        {
            if (_buffers[index] is not { } buffer) continue;
            _buffers[index] = null;
            try { _pool.Return(buffer); }
            catch (Exception exception) { failure ??= exception; }
        }
        if (failure is not null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
    }

    /// <inheritdoc/>
    public byte[] CopyFramePixels(nint data, uint height, nuint pitch)
    {
        if (data == nint.Zero || height == 0 || pitch == 0) throw new ArgumentException(nameof(data));
        var rows = checked((int)height);
        var rowBytes = checked((int)pitch);
        var buffer = Rent(EmulationVideoFunctions.FrameLength(height, pitch));
        EmulationVideoFunctions.CopyRows(data, buffer, rows, rowBytes);
        return buffer;
    }
}
