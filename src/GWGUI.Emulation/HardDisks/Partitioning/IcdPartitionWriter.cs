using System.Buffers.Binary;
using System.Text;

namespace GWGUI.Emulation.HardDisks.Partitioning;

/// <summary>Writes the ICD/Supra root-sector layout: four standard and eight additional entries.</summary>
public static class IcdPartitionWriter
{
    public static void Validate(long capacity, IReadOnlyList<DiskVolumePlan> volumes)
    {
        if (volumes.Count > 12 || volumes.Any(v => v.AhdiLogical || v.MbrLogical))
            throw new ArgumentException("ICD supports up to twelve direct partitions without an extended chain.");
        AhdiPartitionWriter.Validate(capacity, []);
        foreach (var group in volumes.Chunk(4)) AhdiPartitionWriter.Validate(capacity, group);
        long end = 512;
        foreach (var volume in volumes.OrderBy(v => v.OffsetBytes))
        {
            if (volume.OffsetBytes < end || !new[] { "GEM", "BGM", "RAW", "LNX", "SWP" }.Contains(Type(volume)))
                throw new ArgumentException("Invalid ICD partition type or overlapping volumes.");
            end = checked(volume.OffsetBytes + volume.LengthBytes);
        }
    }

    public static void Write(Stream disk, IReadOnlyList<DiskVolumePlan> volumes)
    {
        Validate(disk.Length, volumes);
        var root = new byte[512];
        BinaryPrimitives.WriteUInt32BigEndian(root.AsSpan(0x1c2), checked((uint)(disk.Length / 512)));
        for (var i = 0; i < volumes.Count; i++)
        {
            var volume = volumes[i];
            var offset = i < 4 ? 0x1c6 + i * 12 : 0x156 + (i - 4) * 12;
            root[offset] = 1;
            Encoding.ASCII.GetBytes(Type(volume)).CopyTo(root, offset + 1);
            BinaryPrimitives.WriteUInt32BigEndian(root.AsSpan(offset + 4), checked((uint)(volume.OffsetBytes / 512)));
            BinaryPrimitives.WriteUInt32BigEndian(root.AsSpan(offset + 8), checked((uint)(volume.LengthBytes / 512)));
        }
        disk.Position = 0; disk.Write(root); disk.Flush();
    }

    private static string Type(DiskVolumePlan volume) => volume.PartitionType ?? (volume.LengthBytes < 32L << 20 ? "GEM" : "BGM");
}
