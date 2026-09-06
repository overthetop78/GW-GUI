using System.Buffers.Binary;
using DiscUtils.Streams;

namespace GWGUI.Emulation.HardDisks.Containers;

/// <summary>Standalone QED, 64 KiB clusters, one cluster per table, no backing image.</summary>
public static class QedImageWriter
{
    private const int Cluster = 65536;
    private const int Entries = Cluster / 8;
    public static void Write(Stream destination, long capacity, Action<Stream>? initialize = null)
    {
        ContainerValidation.Validate(destination, capacity);
        if (capacity > (long)Entries * Entries * Cluster) throw new ArgumentOutOfRangeException(nameof(capacity));
        using var content = SparseImageContent.Create(capacity, initialize);
        if (content.Length != capacity) throw new InvalidOperationException("The initializer changed the disk capacity.");
        var guests = new SortedSet<long>();
        foreach (var extent in content.Extents)
            for (var index = extent.Start / Cluster; index < (extent.Start + extent.Length + Cluster - 1) / Cluster; index++)
                guests.Add(index);
        var groups = guests.GroupBy(index => index / Entries).ToArray();
        destination.SetLength((2L + groups.Length + guests.Count) * Cluster);
        var header = new byte[Cluster];
        U32(header, 0, 0x00444551); U32(header, 4, Cluster); U32(header, 8, 1); U32(header, 12, 1);
        U64(header, 40, Cluster); U64(header, 48, (ulong)capacity); Put(destination, 0, header);
        var l1 = new byte[Cluster]; long nextTable = 2, nextData = 2 + groups.Length;
        foreach (var group in groups)
        {
            U64(l1, checked((int)group.Key * 8), (ulong)nextTable * Cluster);
            var l2 = new byte[Cluster];
            foreach (var guest in group)
            {
                U64(l2, (int)(guest % Entries) * 8, (ulong)nextData * Cluster);
                var data = new byte[Cluster]; content.Position = guest * Cluster;
                content.ReadExactly(data.AsSpan(0, (int)Math.Min(Cluster, capacity - content.Position)));
                Put(destination, nextData++ * Cluster, data);
            }
            Put(destination, nextTable++ * Cluster, l2);
        }
        Put(destination, Cluster, l1); destination.Flush();
    }
    private static void U32(byte[] bytes, int offset, uint value) => BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset), value);
    private static void U64(byte[] bytes, int offset, ulong value) => BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(offset), value);
    private static void Put(Stream stream, long offset, byte[] bytes) { stream.Position = offset; stream.Write(bytes); }
}
