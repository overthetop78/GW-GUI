using System.Buffers.Binary;

namespace GWGUI.Emulation.HardDisks.Containers;

/// <summary>QCOW v1, standalone, unencrypted, uncompressed, 64 KiB clusters.</summary>
public static class QcowImageWriter
{
    private const int Cluster = 65536, Entries = Cluster / 8;
    public static void Write(Stream destination, long capacity, Action<Stream>? initialize = null)
    {
        ContainerValidation.Validate(destination, capacity);
        if (capacity > 1L << 40) throw new ArgumentOutOfRangeException(nameof(capacity));
        using var content = SparseImageContent.Create(capacity, initialize);
        var units = SparseImageContent.AllocatedUnits(content, Cluster);
        var groups = units.GroupBy(unit => unit / Entries).ToArray();
        var l1Count = (capacity + (long)Cluster * Entries - 1) / ((long)Cluster * Entries);
        var l1Clusters = (l1Count * 8 + Cluster - 1) / Cluster;
        var l1 = new byte[checked((int)l1Clusters * Cluster)];
        var nextTable = 1 + l1Clusters; var nextData = nextTable + groups.Length;
        destination.SetLength((nextData + units.Count) * Cluster);
        var header = new byte[48]; U32(header, 0, 0x514649fb); U32(header, 4, 1);
        U64(header, 24, (ulong)capacity); header[32] = 16; header[33] = 13; U64(header, 40, Cluster);
        Put(destination, 0, header); var buffer = new byte[Cluster];
        foreach (var group in groups)
        {
            U64(l1, checked((int)group.Key * 8), (ulong)nextTable * Cluster);
            var l2 = new byte[Cluster];
            foreach (var unit in group)
            {
                U64(l2, (int)(unit % Entries) * 8, (ulong)nextData * Cluster);
                SparseImageContent.CopyUnit(content, destination, unit * Cluster, nextData++ * Cluster, buffer);
            }
            Put(destination, nextTable++ * Cluster, l2);
        }
        Put(destination, Cluster, l1); destination.Flush();
    }
    private static void U32(byte[] bytes, int offset, uint value) => BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(offset), value);
    private static void U64(byte[] bytes, int offset, ulong value) => BinaryPrimitives.WriteUInt64BigEndian(bytes.AsSpan(offset), value);
    private static void Put(Stream stream, long offset, byte[] bytes) { stream.Position = offset; stream.Write(bytes); }
}
