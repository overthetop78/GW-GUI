using System.Buffers.Binary;
using DiscUtils.Streams;

namespace GWGUI.Emulation.HardDisks.Containers;

/// <summary>QCOW2 v2/v3, configurable clusters, 16-bit refcounts, no backing file or compression.</summary>
public static class Qcow2ImageWriter
{
    public static void Validate(long capacity, int version = 3, int clusterBits = 16)
    {
        if (version is not (2 or 3)) throw new ArgumentOutOfRangeException(nameof(version));
        if (clusterBits is < 9 or > 21) throw new ArgumentOutOfRangeException(nameof(clusterBits));
        if (capacity < 512 || capacity % 512 != 0 || capacity > 1L << 40)
            throw new ArgumentOutOfRangeException(nameof(capacity));
        var coverage = (1L << clusterBits) * ((1L << clusterBits) / 8);
        if ((capacity + coverage - 1) / coverage * 8 > 32L << 20)
            throw new ArgumentOutOfRangeException(nameof(capacity), "The L1 table exceeds 32 MiB.");
    }

    public static void Write(Stream destination, long capacity, Action<Stream>? initialize = null,
        int version = 3, int clusterBits = 16)
    {
        Validate(capacity, version, clusterBits);
        ContainerValidation.Validate(destination, capacity);
        var Cluster = 1 << clusterBits;
        var Entries = Cluster / 8;
        using var content = SparseImageContent.Create(capacity, initialize);
        var guests = SparseImageContent.AllocatedUnits(content, Cluster);
        var groups = guests.GroupBy(index => index / Entries).ToArray();
        var l1Size = checked((int)((capacity + (long)Cluster * Entries - 1) / ((long)Cluster * Entries)));
        var l1Clusters = (l1Size * 8 + Cluster - 1) / Cluster;
        var baseClusters = checked(1 + l1Clusters + groups.Length + guests.Count);
        var refEntries = Cluster / 2;
        var refBlocks = 1;
        var refTableClusters = 1;
        while (true)
        {
            var needed = checked((int)(((long)baseClusters + refBlocks + refTableClusters + refEntries - 1) / refEntries));
            var tables = checked((int)(((long)needed * 8 + Cluster - 1) / Cluster));
            if (needed == refBlocks && tables == refTableClusters) break;
            refBlocks = needed; refTableClusters = tables;
        }
        var total = checked(baseClusters + refBlocks + refTableClusters);
        var l1Start = 1 + refTableClusters;
        var refStart = l1Start + l1Clusters;
        destination.SetLength((long)total * Cluster);
        var header = new byte[Cluster];
        U32(header, 0, 0x514649fb); U32(header, 4, (uint)version); U32(header, 20, (uint)clusterBits);
        U64(header, 24, (ulong)capacity); U32(header, 36, (uint)l1Size);
        U64(header, 40, (ulong)l1Start * (uint)Cluster); U64(header, 48, (uint)Cluster); U32(header, 56, (uint)refTableClusters);
        if (version == 3) { U32(header, 96, 4); U32(header, 100, 104); }
        Put(destination, 0, header);
        var refs = new byte[checked(refTableClusters * Cluster)];
        for (var i = 0; i < refBlocks; i++)
        {
            U64(refs, i * 8, (ulong)(refStart + i) * (uint)Cluster);
            var block = new byte[Cluster];
            for (var j = 0; j < refEntries && (long)i * refEntries + j < total; j++)
                BinaryPrimitives.WriteUInt16BigEndian(block.AsSpan(j * 2), 1);
            Put(destination, (long)(refStart + i) * Cluster, block);
        }
        Put(destination, Cluster, refs);
        var l1 = new byte[l1Clusters * Cluster];
        var next = refStart + refBlocks;
        var dataIndex = next + groups.Length;
        foreach (var group in groups)
        {
            U64(l1, checked((int)group.Key * 8), (1UL << 63) | (ulong)next * (uint)Cluster);
            var l2 = new byte[Cluster];
            foreach (var guest in group)
            {
                U64(l2, (int)(guest % Entries) * 8, (1UL << 63) | (ulong)dataIndex * (uint)Cluster);
                var data = new byte[Cluster];
                content.Position = guest * Cluster;
                content.ReadExactly(data.AsSpan(0, (int)Math.Min(Cluster, capacity - content.Position)));
                Put(destination, (long)dataIndex++ * Cluster, data);
            }
            Put(destination, (long)next++ * Cluster, l2);
        }
        Put(destination, (long)l1Start * Cluster, l1);
        destination.Flush();
    }
    private static void U32(byte[] bytes, int offset, uint value) => BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(offset), value);
    private static void U64(byte[] bytes, int offset, ulong value) => BinaryPrimitives.WriteUInt64BigEndian(bytes.AsSpan(offset), value);
    private static void Put(Stream stream, long position, byte[] bytes) { stream.Position = position; stream.Write(bytes); }
}
