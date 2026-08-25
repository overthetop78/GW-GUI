using System.Buffers;

namespace GWGUI.Emulation.Atari.Services;

internal sealed class AtariVideoBufferSet : IDisposable
{
    private readonly ArrayPool<byte> _pool;
    private readonly byte[]?[] _buffers = new byte[AtariVideoConstants.BufferCount][];
    private int _nextBuffer = AtariVideoConstants.FirstBuffer;

    internal AtariVideoBufferSet(ArrayPool<byte>? pool = null) => _pool = pool ?? ArrayPool<byte>.Shared;

    internal byte[] Rent(int length)
    {
        var index = _nextBuffer;
        _nextBuffer = (_nextBuffer + AtariVideoConstants.NextBufferStep) % AtariVideoConstants.BufferCount;
        var buffer = _buffers[index];
        if (buffer is not null && buffer.Length >= length) return buffer;
        if (buffer is not null) _pool.Return(buffer);
        buffer = _pool.Rent(length);
        _buffers[index] = buffer;
        return buffer;
    }

    public void Dispose()
    {
        for (var index = AtariVideoConstants.FirstBuffer; index < _buffers.Length; index++)
        {
            if (_buffers[index] is not { } buffer) continue;
            _pool.Return(buffer);
            _buffers[index] = null;
        }
    }
}
