using System.Buffers.Binary;
using DiscUtils.ExFat.Internal;
using DiscUtils.ExFat.Internal.Partition;

namespace GWGUI.Emulation.HardDisks.FileSystems;

public static class ExFatVolumeFormatter
{
    public static void Validate(long capacity, string label)
    {
        if (capacity < 32L << 20 || capacity > 1L << 40 || capacity % 512 != 0)
            throw new ArgumentOutOfRangeException(nameof(capacity));
        if (label.Length > 11 || label.Any(c => c < 32 || "\"*/:<>?\\|".Contains(c) || char.IsSurrogate(c)))
            throw new ArgumentException("Invalid exFAT volume label.", nameof(label));
    }
    public static void Format(Stream volume, string label, long firstSector = 0)
    {
        Validate(volume.Length, label);
        if (firstSector < 0) throw new ArgumentOutOfRangeException(nameof(firstSector));
        using (var partition = ExFatPartition.Format(volume, new ExFatFormatOptions { BytesPerSector = 512 }, label)) { }
        // Set the location of the volume and regenerate both boot-region checksums.
        for (var copy = 0; copy < 2; copy++)
        {
            var region = new byte[12 * 512]; volume.Position = copy * region.Length; volume.ReadExactly(region);
            BinaryPrimitives.WriteUInt64LittleEndian(region.AsSpan(64), (ulong)firstSector);
            uint checksum = 0;
            for (var i = 0; i < 11 * 512; i++)
                if (i is not (106 or 107 or 112)) checksum = unchecked(((checksum >> 1) | (checksum << 31)) + region[i]);
            for (var i = 11 * 512; i < region.Length; i += 4) BinaryPrimitives.WriteUInt32LittleEndian(region.AsSpan(i), checksum);
            volume.Position = copy * region.Length; volume.Write(region);
        }
        volume.Flush();
    }
}
