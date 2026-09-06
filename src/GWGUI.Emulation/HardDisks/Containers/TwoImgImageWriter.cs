using System.Buffers.Binary;
using DiscUtils.Streams;

namespace GWGUI.Emulation.HardDisks.Containers;

/// <summary>2IMG version 1, linear 512-byte blocks, no optional comment or creator data.</summary>
public static class TwoImgImageWriter
{
    public static void Write(Stream destination, long capacity, Action<Stream>? initialize = null)
    {
        ContainerValidation.Validate(destination, capacity);
        if (capacity > int.MaxValue - 64) throw new ArgumentOutOfRangeException(nameof(capacity));
        var header = new byte[64];
        "2IMG"u8.CopyTo(header); "GWGU"u8.CopyTo(header.AsSpan(4));
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(8), 64);
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(10), 1);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(12), 1);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(20), (uint)(capacity / 512));
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(24), 64);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(28), (uint)capacity);
        LinearHeaderImageWriter.Write(destination, capacity, header, initialize);
    }
}
