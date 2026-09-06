using System.Buffers.Binary;

namespace GWGUI.Emulation.HardDisks.Containers;

/// <summary>Expandable v2 image, extended offsets in clusters, no XML descriptor or parent.</summary>
public static class ParallelsImageWriter
{
    private const int Cluster = 1024 * 1024;
    public static void Write(Stream destination, long capacity, Action<Stream>? initialize = null)
    {
        ContainerValidation.Validate(destination, capacity);
        if (capacity > 1L << 40) throw new ArgumentOutOfRangeException(nameof(capacity));
        using var content = SparseImageContent.Create(capacity, initialize);
        var units = SparseImageContent.AllocatedUnits(content, Cluster);
        var count = checked((int)((capacity + Cluster - 1) / Cluster));
        var table = new byte[count * 4]; var firstData = (64L + table.Length + Cluster - 1) / Cluster;
        destination.SetLength((firstData + units.Count) * Cluster);
        var header = new byte[64]; "WithouFreSpacExt"u8.CopyTo(header);
        U32(header, 16, 2); U32(header, 20, 1); U32(header, 24, (uint)count); U32(header, 28, Cluster / 512);
        U32(header, 32, (uint)count); BinaryPrimitives.WriteUInt64LittleEndian(header.AsSpan(36), (ulong)(capacity / 512));
        U32(header, 44, 0x312e3276); U32(header, 48, checked((uint)(firstData * Cluster / 512)));
        destination.Position = 0; destination.Write(header);
        var buffer = new byte[Cluster]; var next = firstData;
        foreach (var unit in units)
        {
            U32(table, checked((int)unit * 4), checked((uint)next));
            SparseImageContent.CopyUnit(content, destination, unit * Cluster, next++ * Cluster, buffer);
        }
        destination.Position = 64; destination.Write(table); destination.Flush();
    }
    private static void U32(byte[] bytes, int offset, uint value) => BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset), value);
}
