using System.Buffers.Binary;

namespace GWGUI.Emulation.HardDisks.Containers;

/// <summary>Unencrypted sparseimage v3 with one 4096-byte index header and no continuation headers.</summary>
public static class SparseImageWriter
{
    public const int DefaultBandBytes = 8 << 20;
    public const int MaximumBands = (4096 - 64) / 4;

    public static void Validate(long capacity, int bandBytes = DefaultBandBytes)
    {
        if (bandBytes < 1 << 20 || bandBytes > 128 << 20 || (bandBytes & (bandBytes - 1)) != 0)
            throw new ArgumentOutOfRangeException(nameof(bandBytes));
        if (capacity < 512 || capacity % 512 != 0 || capacity > (long)MaximumBands * bandBytes)
            throw new ArgumentOutOfRangeException(nameof(capacity));
    }

    public static void Write(Stream destination, long capacity, Action<Stream>? initialize = null,
        int bandBytes = DefaultBandBytes)
    {
        ContainerValidation.Validate(destination, capacity);
        Validate(capacity, bandBytes);
        using var content = SparseImageContent.Create(capacity, initialize);
        var header = new byte[4096];
        "sprs"u8.CopyTo(header);
        Put(4, 3); Put(8, (uint)(bandBytes / 512)); Put(12, 1); Put(16, (uint)(capacity / 512));
        destination.Write(header);
        var buffer = new byte[bandBytes];
        var index = 0;
        foreach (var band in SparseImageContent.AllocatedUnits(content, bandBytes))
        {
            Array.Clear(buffer);
            content.Position = band * bandBytes;
            content.ReadExactly(buffer.AsSpan(0, (int)Math.Min(bandBytes, capacity - content.Position)));
            if (!buffer.AsSpan().ContainsAnyExcept((byte)0)) continue;
            Put(64 + index++ * 4, checked((uint)band + 1));
            destination.Write(buffer);
        }
        destination.Position = 0; destination.Write(header); destination.Flush();
        void Put(int offset, uint value) => BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(offset), value);
    }
}
