using System.Buffers;

namespace GWGUI.Emulation.Atari.Common.Services;

internal sealed class VideoBufferSet : IDisposable
{
    private readonly ArrayPool<byte> _pool;
    private readonly byte[]?[] _buffers = new byte[VideoConstants.BufferCount][];
    private int _nextBuffer = VideoConstants.FirstBuffer;

    internal VideoBufferSet(ArrayPool<byte>? pool = null) => _pool = pool ?? ArrayPool<byte>.Shared;

    internal byte[] Rent(int length)
    {
        var index = _nextBuffer;
        _nextBuffer = (_nextBuffer + VideoConstants.NextBufferStep) % VideoConstants.BufferCount;
        var buffer = _buffers[index];
        if (buffer is not null && buffer.Length >= length) return buffer;
        if (buffer is not null) _pool.Return(buffer);
        buffer = _pool.Rent(length);
        _buffers[index] = buffer;
        return buffer;
    }

    public void Dispose()
    {
        for (var index = VideoConstants.FirstBuffer; index < _buffers.Length; index++)
        {
            if (_buffers[index] is not { } buffer) continue;
            _pool.Return(buffer);
            _buffers[index] = null;
        }
    }
}
