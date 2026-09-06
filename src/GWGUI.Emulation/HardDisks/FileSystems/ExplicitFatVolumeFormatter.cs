using System.Buffers.Binary;
using System.Text;

namespace GWGUI.Emulation.HardDisks.FileSystems;

/// <summary>FAT12/16/32, 512–4096-byte sectors and two FAT copies. No executable boot loader.</summary>
public static class ExplicitFatVolumeFormatter
{
    private sealed record Layout(uint Sectors, int ClusterSectors, int Reserved, int RootSectors, uint FatSectors, uint Clusters);

    public static void Validate(long capacity, string label, FatVariant variant, int sectorsPerCluster = 0, long firstSector = 0,int sectorBytes=512)
    {
        if (label.Length > 11 || label.Any(c => c < 32 || c > 126 || "\"*+,./:;<=>?[\\]|".Contains(c)))
            throw new ArgumentException("Invalid FAT label.", nameof(label));
        if (firstSector < 0 || firstSector > uint.MaxValue) throw new ArgumentOutOfRangeException(nameof(firstSector));
        _ = GetLayout(capacity, variant, sectorsPerCluster,sectorBytes);
    }

    public static void Format(Stream volume, string label, FatVariant variant, int sectorsPerCluster = 0, long firstSector = 0,int sectorBytes=512, DiskChsGeometry? geometry = null)
    {
        Validate(volume.Length, label, variant, sectorsPerCluster, firstSector,sectorBytes);
        var layout = GetLayout(volume.Length, variant, sectorsPerCluster,sectorBytes);
        var fat32 = variant == FatVariant.Fat32;
        var boot = new byte[sectorBytes]; boot[0] = 0xeb; boot[1] = fat32 ? (byte)0x58 : (byte)0x3c; boot[2] = 0x90;
        "GWGUI   "u8.CopyTo(boot.AsSpan(3)); U16(boot, 11, sectorBytes); boot[13] = (byte)layout.ClusterSectors;
        U16(boot, 14, layout.Reserved); boot[16] = 2; U16(boot, 17, fat32 ? 0 : 512);
        if (!fat32 && layout.Sectors < 65536) U16(boot, 19, (int)layout.Sectors); else U32(boot, 32, layout.Sectors);
        boot[21] = 0xf8; U16(boot, 24, geometry?.SectorsPerTrack ?? 63); U16(boot, 26, geometry?.Heads ?? 255); U32(boot, 28, (uint)firstSector);
        var extended = fat32 ? 64 : 36;
        if (fat32)
        {
            U32(boot, 36, layout.FatSectors); U32(boot, 44, 2); U16(boot, 48, 1); U16(boot, 50, 6);
        }
        else U16(boot, 22, (int)layout.FatSectors);
        boot[extended] = 0x80; boot[extended + 2] = 0x29; U32(boot, extended + 3, 0x47574755);
        Encoding.ASCII.GetBytes((string.IsNullOrEmpty(label) ? "NO NAME" : label).ToUpperInvariant().PadRight(11)).CopyTo(boot, extended + 7);
        Encoding.ASCII.GetBytes(($"FAT{(int)variant}").PadRight(8)).CopyTo(boot, extended + 18);
        U16(boot, 510, 0xaa55);
        Zero(volume, 0, (long)layout.Reserved * sectorBytes);
        Put(volume, 0, boot);
        if (fat32)
        {
            Put(volume, 6 * sectorBytes, boot);
            var info = new byte[sectorBytes]; U32(info, 0, 0x41615252); U32(info, 484, 0x61417272);
            U32(info, 488, layout.Clusters - 1); U32(info, 492, 3); U32(info, 508, 0xaa550000);
            Put(volume, sectorBytes, info); Put(volume, 7 * sectorBytes, info);
        }
        var fatHead = new byte[512];
        if (variant == FatVariant.Fat12) { fatHead[0] = 0xf8; fatHead[1] = 0xff; fatHead[2] = 0xff; }
        else if (!fat32) { U16(fatHead, 0, 0xfff8); U16(fatHead, 2, 0xffff); }
        else { U32(fatHead, 0, 0x0ffffff8); U32(fatHead, 4, 0xffffffff); U32(fatHead, 8, 0x0fffffff); }
        for (var copy = 0; copy < 2; copy++)
        {
            var offset = ((long)layout.Reserved + copy * layout.FatSectors) * sectorBytes;
            Zero(volume, offset, (long)layout.FatSectors * sectorBytes); Put(volume, offset, fatHead);
        }
        var rootOffset = ((long)layout.Reserved + 2L * layout.FatSectors) * sectorBytes;
        Zero(volume, rootOffset, (long)(fat32 ? layout.ClusterSectors : layout.RootSectors) * sectorBytes);
        if (!string.IsNullOrEmpty(label))
        {
            var entry = new byte[32]; Encoding.ASCII.GetBytes(label.ToUpperInvariant().PadRight(11)).CopyTo(entry, 0); entry[11] = 8;
            Put(volume, rootOffset, entry);
        }
        volume.Flush();
    }

    private static Layout GetLayout(long capacity, FatVariant variant, int requested,int sectorBytes)
    {
        if (sectorBytes is not (512 or 1024 or 2048 or 4096))throw new ArgumentOutOfRangeException(nameof(sectorBytes));
        if (capacity < 64 * sectorBytes || capacity % sectorBytes != 0 || capacity / sectorBytes > uint.MaxValue ||
            variant is not (FatVariant.Fat12 or FatVariant.Fat16 or FatVariant.Fat32))
            throw new ArgumentOutOfRangeException(nameof(capacity));
        if (requested != 0 && (requested < 1 || requested > 64 || (requested & (requested - 1)) != 0))
            throw new ArgumentOutOfRangeException(nameof(requested));
        var sectors = (uint)(capacity / sectorBytes); var reserved = variant == FatVariant.Fat32 ? 32 : 1;
        var root = variant == FatVariant.Fat32 ? 0 : (512*32+sectorBytes-1)/sectorBytes;
        foreach (var cluster in requested == 0 ? new[] { 1, 2, 4, 8, 16, 32, 64 } : new[] { requested })
        {
            if(cluster*sectorBytes>32768)continue;
            // Upper bound on FAT entries avoids a circular geometry calculation.
            var fat = (uint)((((long)sectors / cluster + 2) * (int)variant + sectorBytes*8-1) / (sectorBytes*8));
            var data = (long)sectors - reserved - root - 2L * fat;
            if (data <= 0) continue;
            var count = (uint)(data / cluster);
            var fits = variant switch
            {
                FatVariant.Fat12 => count is >= 1 and < 4085,
                FatVariant.Fat16 => count is >= 4085 and < 65525,
                _ => count is >= 65525 and < 0x0ffffff5
            };
            if (fits && (variant == FatVariant.Fat32 || fat <= ushort.MaxValue))
                return new(sectors, cluster, reserved, root, fat, count);
        }
        throw new ArgumentException("Capacity and cluster size cannot represent the requested FAT variant.");
    }
    private static void U16(byte[] bytes, int offset, int value) => BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(offset), checked((ushort)value));
    private static void U32(byte[] bytes, int offset, uint value) => BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset), value);
    private static void Put(Stream stream, long offset, byte[] bytes) { stream.Position = offset; stream.Write(bytes); }
    private static void Zero(Stream stream, long offset, long length)
    {
        var zero = new byte[65536]; stream.Position = offset;
        while (length > 0) { var count = (int)Math.Min(length, zero.Length); stream.Write(zero, 0, count); length -= count; }
    }
}
