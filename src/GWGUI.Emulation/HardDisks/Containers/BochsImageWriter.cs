using System.Buffers.Binary;

namespace GWGUI.Emulation.HardDisks.Containers;

/// <summary>Autonomous Growing redolog, versions 1 and 2, with initialized sector bitmaps.</summary>
public static class BochsImageWriter
{
    public static void Validate(long capacity, int version = 2, int extentBytes = 1 << 20)
    {
        if (version is not (1 or 2) || extentBytes < 4096 || extentBytes > 8 << 20 || (extentBytes & (extentBytes - 1)) != 0)
            throw new ArgumentOutOfRangeException(nameof(extentBytes));
        if (capacity < 512 || capacity % 512 != 0 || capacity > (long)extentBytes * 0x100000)
            throw new ArgumentOutOfRangeException(nameof(capacity));
    }

    public static void Write(Stream destination, long capacity, Action<Stream>? initialize = null, int version = 2, int extentBytes = 1 << 20)
    {
        Validate(capacity, version, extentBytes); ContainerValidation.Validate(destination, capacity);
        using var content = SparseImageContent.Create(capacity, initialize);
        var units = SparseImageContent.AllocatedUnits(content, extentBytes);
        var entries = checked((int)(((capacity + extentBytes - 1) / extentBytes + 127) / 128 * 128));
        var bitmapBytes = extentBytes / 4096;
        var bitmapArea = (bitmapBytes + 511) / 512 * 512;
        var header = new byte[512];
        "Bochs Virtual HD Image"u8.CopyTo(header); "Redolog"u8.CopyTo(header.AsSpan(32)); "Growing"u8.CopyTo(header.AsSpan(48));
        U32(header, 64, (uint)version << 16); U32(header, 68, 512); U32(header, 72, (uint)entries);
        U32(header, 76, (uint)bitmapBytes); U32(header, 80, (uint)extentBytes);
        BinaryPrimitives.WriteUInt64LittleEndian(header.AsSpan(version == 1 ? 84 : 88), (ulong)capacity);
        var catalog = new byte[entries * 4]; Array.Fill(catalog, (byte)0xff);
        var dataStart = 512L + catalog.Length;
        var ordinal = 0;
        var buffer = new byte[extentBytes];
        foreach (var unit in units)
        {
            U32(catalog, checked((int)unit * 4), (uint)ordinal);
            var physical = dataStart + ordinal * (long)(bitmapArea + extentBytes);
            var bitmap = new byte[bitmapArea];
            var sectors = Math.Min(extentBytes, capacity - unit * extentBytes) / 512;
            for (var sector = 0; sector < sectors; sector++) bitmap[sector / 8] |= (byte)(1 << (sector % 8));
            destination.Position = physical; destination.Write(bitmap);
            SparseImageContent.CopyUnit(content, destination, unit * extentBytes, physical + bitmapArea, buffer);
            ordinal++;
        }
        destination.Position = 0; destination.Write(header); destination.Write(catalog); destination.Flush();
    }

    private static void U32(byte[] bytes, int at, uint value) => BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(at), value);
}
