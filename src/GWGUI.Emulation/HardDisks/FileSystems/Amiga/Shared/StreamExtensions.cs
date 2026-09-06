using System.Buffers.Binary;

namespace Hst.Core.Extensions;

internal static class StreamExtensions
{
    public static async Task<byte[]> ReadBytes(this Stream stream, int count)
    {
        var bytes = new byte[count];
        var offset = 0;
        while (offset < count)
        {
            var read = await stream.ReadAsync(bytes.AsMemory(offset, count - offset)).ConfigureAwait(false);
            if (read == 0) break;
            offset += read;
        }
        return offset == count ? bytes : bytes[..offset];
    }

    public static Task WriteBytes(this Stream stream, byte[] bytes)
        => stream.WriteAsync(bytes).AsTask();

    public static async Task WriteBigEndianInt32(this Stream stream, int value)
    {
        var bytes = new byte[4];
        BinaryPrimitives.WriteInt32BigEndian(bytes, value);
        await stream.WriteAsync(bytes).ConfigureAwait(false);
    }

    public static async Task<uint> ReadBigEndianUInt32(this Stream stream)
    {
        var bytes = await stream.ReadBytes(4).ConfigureAwait(false);
        if (bytes.Length != 4) throw new EndOfStreamException();
        return BinaryPrimitives.ReadUInt32BigEndian(bytes);
    }

    public static async Task<int> ReadBigEndianInt32(this Stream stream)
    {
        var bytes = await stream.ReadBytes(4).ConfigureAwait(false);
        if (bytes.Length != 4) throw new EndOfStreamException();
        return BinaryPrimitives.ReadInt32BigEndian(bytes);
    }
}
