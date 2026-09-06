using System.Buffers.Binary;

namespace GWGUI.Emulation.HardDisks.FileSystems;

/// <summary>FATX/XTAF volume, 512-byte sectors, one FAT and an empty root directory.</summary>
public static class FatxVolumeFormatter
{
    public static void Validate(long capacity, int sectorsPerCluster = 32, bool bigEndian = false)
    {
        if (capacity < 1L << 20 || capacity > 1L << 40 || capacity % 512 != 0 ||
            sectorsPerCluster < (bigEndian?8:1) || sectorsPerCluster > 128 || (sectorsPerCluster & (sectorsPerCluster - 1)) != 0)
            throw new ArgumentException("Invalid FATX volume capacity or cluster size.");
    }
    public static void Format(Stream volume, int sectorsPerCluster = 32, bool bigEndian = false)
    {
        Validate(volume.Length, sectorsPerCluster,bigEndian);
        void U32(Span<byte> bytes,uint value){if(bigEndian)BinaryPrimitives.WriteUInt32BigEndian(bytes,value);else BinaryPrimitives.WriteUInt32LittleEndian(bytes,value);}
        void U16(Span<byte> bytes,ushort value){if(bigEndian)BinaryPrimitives.WriteUInt16BigEndian(bytes,value);else BinaryPrimitives.WriteUInt16LittleEndian(bytes,value);}
        var clusterBytes = sectorsPerCluster * 512;
        var entries = volume.Length / clusterBytes + 1;
        var entryBytes = (bigEndian?entries-1:entries) < 0xfff0 ? 2 : 4;
        var fatBytes = (entries * entryBytes + 4095) / 4096 * 4096;
        var header = new byte[4096]; if(bigEndian)"XTAF"u8.CopyTo(header);else "FATX"u8.CopyTo(header);
        U32(header.AsSpan(4), 0x47574755);
        U32(header.AsSpan(8), (uint)sectorsPerCluster);
        U32(header.AsSpan(12), 1);
        volume.Position = 0; volume.Write(header);
        var zero = new byte[65536];
        for (long remaining = fatBytes; remaining > 0;)
        {
            var count = (int)Math.Min(remaining, zero.Length); volume.Write(zero, 0, count); remaining -= count;
        }
        var reserved = new byte[entryBytes * 2];
        if (entryBytes == 2)
        {
            U16(reserved, 0xfff8);
            U16(reserved.AsSpan(2), 0xffff);
        }
        else
        {
            U32(reserved, 0xfffffff8);
            U32(reserved.AsSpan(4), uint.MaxValue);
        }
        volume.Position = 4096; volume.Write(reserved);
        var root = new byte[clusterBytes]; Array.Fill(root, (byte)0xff);
        volume.Position = 4096 + fatBytes; volume.Write(root); volume.Flush();
    }
}
