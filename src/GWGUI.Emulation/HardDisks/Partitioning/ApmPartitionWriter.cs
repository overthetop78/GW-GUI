using System.Buffers.Binary;
using System.Text;

namespace GWGUI.Emulation.HardDisks.Partitioning;

/// <summary>APM with 512-byte blocks and a reserved map of 63 entries. No driver is installed.</summary>
public static class ApmPartitionWriter
{
    public static void Validate(long capacity, IReadOnlyList<DiskVolumePlan> volumes)
    {
        if (capacity < 64 * 512 || capacity % 512 != 0 || capacity / 512 > uint.MaxValue || volumes.Count > 62)
            throw new ArgumentOutOfRangeException(nameof(capacity));
        long end = 64 * 512;
        foreach (var volume in volumes.OrderBy(v => v.OffsetBytes))
        {
            if (volume.OffsetBytes < end || volume.OffsetBytes % 512 != 0 || volume.LengthBytes <= 0 ||
                volume.LengthBytes % 512 != 0 || volume.OffsetBytes > capacity || volume.LengthBytes > capacity - volume.OffsetBytes)
                throw new ArgumentException("Invalid APM partition bounds.");
            ValidateText(volume.EffectivePartitionName);
            ValidateText(volume.PartitionType ?? throw new ArgumentException("An APM partition type is required."));
            end = volume.OffsetBytes + volume.LengthBytes;
        }
    }
    public static void Write(Stream disk, IReadOnlyList<DiskVolumePlan> volumes)
    {
        Validate(disk.Length, volumes);
        var metadata = new byte[64 * 512];
        BinaryPrimitives.WriteUInt16BigEndian(metadata, 0x4552);
        BinaryPrimitives.WriteUInt16BigEndian(metadata.AsSpan(2), 512);
        U32(metadata, 4, (uint)(disk.Length / 512));
        Entry(metadata.AsSpan(512, 512), (uint)volumes.Count + 1, 1, 63, "Partition map", "Apple_partition_map");
        for (var i = 0; i < volumes.Count; i++)
            Entry(metadata.AsSpan((i + 2) * 512, 512), (uint)volumes.Count + 1,
                (uint)(volumes[i].OffsetBytes / 512), (uint)(volumes[i].LengthBytes / 512), volumes[i].EffectivePartitionName, volumes[i].PartitionType!);
        disk.Position = 0; disk.Write(metadata); disk.Flush();
    }
    private static void Entry(Span<byte> entry, uint count, uint start, uint length, string name, string type)
    {
        BinaryPrimitives.WriteUInt16BigEndian(entry, 0x504d);
        U32(entry, 4, count); U32(entry, 8, start); U32(entry, 12, length);
        Encoding.ASCII.GetBytes(name, entry[16..48]); Encoding.ASCII.GetBytes(type, entry[48..80]);
        U32(entry, 84, length); U32(entry, 88, 0x33); // valid, allocated, readable, writable
    }
    private static void ValidateText(string value)
    {
        if (value.Length is < 1 or > 31 || value.Any(c => c < 32 || c > 126))
            throw new ArgumentException("APM names and types require 1–31 printable ASCII characters.");
    }
    private static void U32(Span<byte> bytes, int offset, uint value) => BinaryPrimitives.WriteUInt32BigEndian(bytes[offset..], value);
}
