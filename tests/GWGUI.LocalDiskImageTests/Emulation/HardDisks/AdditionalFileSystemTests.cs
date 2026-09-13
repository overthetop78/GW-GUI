using System.Buffers.Binary;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.HardDisks.FileSystems;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class AdditionalFileSystemTests
{
    [Theory]
    [InlineData(32)] [InlineData(2048)]
    public void FatxRootAndAllocationMatchBothEntryWidths(int mib)
    {
        using var volume = new SparseMemoryStream(); volume.SetLength((long)mib << 20);
        FatxVolumeFormatter.Format(volume);
        var header = new byte[4096]; volume.Position = 0; volume.ReadExactly(header);
        Assert.Equal("FATX"u8.ToArray(), header[..4]);
        var clusterSize = BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(8)) * 512;
        Assert.Equal(1u, BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(12)));
        var entries = volume.Length / clusterSize + 1;
        var width = entries < 0xfff0 ? 2 : 4;
        var fatBytes = (entries * width + 4095) & ~4095L;
        var reserved = new byte[8]; volume.ReadExactly(reserved);
        Assert.Equal(width == 2 ? 0xffffu : uint.MaxValue, width == 2
            ? BinaryPrimitives.ReadUInt16LittleEndian(reserved.AsSpan(2))
            : BinaryPrimitives.ReadUInt32LittleEndian(reserved.AsSpan(4)));
        volume.Position = 4096 + fatBytes; Assert.Equal(0xff, volume.ReadByte());
        Assert.True(4096 + fatBytes + clusterSize < volume.Length);
    }

    [Theory]
    [InlineData(8, 2)] [InlineData(256, 2)] [InlineData(300, 2)]
    [InlineData(8, 3)] [InlineData(256, 3)] [InlineData(300, 3)]
    [InlineData(8, 4)] [InlineData(256, 4)] [InlineData(300, 4)]
    public void ExtVolumesCanReopenRootAndHaveConsistentGroupFreeCounts(int mib, int version)
    {
        using var volume = new SparseMemoryStream(); volume.SetLength((long)mib << 20);
        if (version == 2) Ext2VolumeFormatter.Format(volume, "DATA");
        else if (version == 3) Ext3VolumeFormatter.Format(volume, "DATA");
        else Ext4VolumeFormatter.Format(volume, "DATA");
        volume.Position = 0;
        using var fs = new DiscUtils.Ext.ExtFileSystem(volume);
        Assert.Empty(fs.Root.GetFiles()); Assert.Empty(fs.Root.GetDirectories());
        var super = new byte[1024]; volume.Position = 1024; volume.ReadExactly(super);
        var blocks = BinaryPrimitives.ReadUInt32LittleEndian(super.AsSpan(4));
        var expectedFree = BinaryPrimitives.ReadUInt32LittleEndian(super.AsSpan(12));
        var groups = (blocks + 32767) / 32768;
        var descriptors = new byte[groups * 32]; volume.Position = 4096; volume.ReadExactly(descriptors);
        long countedFree = 0;
        for (var group = 0; group < groups; group++)
        {
            var bitmapBlock = BinaryPrimitives.ReadUInt32LittleEndian(descriptors.AsSpan(group * 32));
            var bitmap = new byte[4096]; volume.Position = (long)bitmapBlock * 4096; volume.ReadExactly(bitmap);
            var free = bitmap.Sum(value => 8 - System.Numerics.BitOperations.PopCount((uint)value));
            Assert.Equal(BinaryPrimitives.ReadUInt16LittleEndian(descriptors.AsSpan(group * 32 + 12)), free);
            countedFree += free;
        }
        Assert.Equal(expectedFree, countedFree);
        Assert.Equal(version >= 3 ? 4u : 0u, BinaryPrimitives.ReadUInt32LittleEndian(super.AsSpan(92)));
        Assert.Equal(version == 4 ? 0x42u : 2u, BinaryPrimitives.ReadUInt32LittleEndian(super.AsSpan(96)));
        if (version >= 3)
        {
            Assert.Equal(8u, BinaryPrimitives.ReadUInt32LittleEndian(super.AsSpan(224)));
            var inodeTable = BinaryPrimitives.ReadUInt32LittleEndian(descriptors.AsSpan(8));
            var inode = new byte[128]; volume.Position = (long)inodeTable * 4096 + 7 * 128; volume.ReadExactly(inode);
            var journalStart = BinaryPrimitives.ReadUInt32LittleEndian(inode.AsSpan(version == 4 ? 60 : 40));
            var journal = new byte[1024]; volume.Position = (long)journalStart * 4096; volume.ReadExactly(journal);
            Assert.Equal(0xc03b3998u, BinaryPrimitives.ReadUInt32BigEndian(journal));
            Assert.Equal(4u, BinaryPrimitives.ReadUInt32BigEndian(journal.AsSpan(4)));
            Assert.Equal(1024u, BinaryPrimitives.ReadUInt32BigEndian(journal.AsSpan(16)));
            Assert.Equal(super[104..120], journal[48..64]);
            Assert.Equal(4u << 20, BinaryPrimitives.ReadUInt32LittleEndian(inode.AsSpan(4)));
            var bitmapBlock = BinaryPrimitives.ReadUInt32LittleEndian(descriptors);
            var bitmap = new byte[4096]; volume.Position = (long)bitmapBlock * 4096; volume.ReadExactly(bitmap);
            for (var block = journalStart; block < journalStart + 1024; block++)
                Assert.NotEqual(0, bitmap[block / 8] & (1 << (int)(block % 8)));
            if (version == 4)
            {
                Assert.Equal(0xf30a, BinaryPrimitives.ReadUInt16LittleEndian(inode.AsSpan(40)));
                Assert.Equal(1024, BinaryPrimitives.ReadUInt16LittleEndian(inode.AsSpan(56)));
            }
            else
            {
                var indirect = BinaryPrimitives.ReadUInt32LittleEndian(inode.AsSpan(88));
                var pointers = new byte[4096]; volume.Position = (long)indirect * 4096; volume.ReadExactly(pointers);
                Assert.Equal(journalStart + 12, BinaryPrimitives.ReadUInt32LittleEndian(pointers));
                Assert.Equal(journalStart + 1023, BinaryPrimitives.ReadUInt32LittleEndian(pointers.AsSpan(1011 * 4)));
                Assert.NotEqual(0, bitmap[indirect / 8] & (1 << (int)(indirect % 8)));
            }
        }
    }

    [Theory]
    [InlineData(FatVariant.Fat12, 2)]
    [InlineData(FatVariant.Fat16, 32)]
    [InlineData(FatVariant.Fat32, 64)]
    public void ExplicitFatVariantsReopenAndAllowDataAllocation(FatVariant variant, int mib)
    {
        using var volume = new SparseMemoryStream(); volume.SetLength((long)mib << 20);
        ExplicitFatVolumeFormatter.Format(volume, "DATA", variant, firstSector: 2048);
        volume.Position = 0;
        using var fs = new DiscUtils.Fat.FatFileSystem(volume);
        var data = Enumerable.Range(0, 10000).Select(i => (byte)i).ToArray();
        using (var file = fs.OpenFile("check.bin", FileMode.Create, FileAccess.Write)) file.Write(data);
        using var read = fs.OpenFile("check.bin", FileMode.Open, FileAccess.Read);
        var actual = new byte[data.Length]; read.ReadExactly(actual); Assert.Equal(data, actual);
    }

    [Fact]
    public void ExFatReopensAndPreservesFileDataAndBootCopies()
    {
        using var volume = new SparseMemoryStream(); volume.SetLength(64L << 20);
        ExFatVolumeFormatter.Format(volume, "DATA", 2048);
        var boot = new byte[12 * 512]; volume.Position = 0; volume.ReadExactly(boot);
        var backup = new byte[12 * 512]; volume.ReadExactly(backup); Assert.Equal(boot, backup);
        Assert.Equal(2048UL, BinaryPrimitives.ReadUInt64LittleEndian(boot.AsSpan(64)));
        volume.Position = 0;
        using var fs = new DiscUtils.ExFat.ExFatFileSystem(volume);
        using (var file = fs.OpenFile("check.txt", FileMode.Create, FileAccess.Write)) file.Write("saved"u8);
        using var read = fs.OpenFile("check.txt", FileMode.Open, FileAccess.Read);
        var bytes = new byte[5]; read.ReadExactly(bytes); Assert.Equal("saved"u8.ToArray(), bytes);
    }

    [Fact]
    public void ApmIsReadableByIndependentPartitionReader()
    {
        using var disk = new SparseMemoryStream();
        DiskImageBuilder.Write(disk, new(64L << 20, "raw", "apm",
            [new(1L << 20, 8L << 20, "prodos", "FIRST", PartitionType: "Apple_PRODOS"),
             new(16L << 20, 16L << 20, "prodos", "SECOND", PartitionType: "Apple_PRODOS")]));
        var table = new DiscUtils.ApplePartitionMap.PartitionMap(disk);
        var volumes = table.Partitions.Where(p => p.FirstSector >= 2048).ToArray();
        Assert.Equal(2, volumes.Length);
        Assert.Equal(2048, volumes[0].FirstSector); Assert.Equal(32768, volumes[1].FirstSector);
        using var first = volumes[0].Open(); using var second = volumes[1].Open();
        first.Position = 1028; Assert.Equal(0xf5, first.ReadByte());
        second.Position = 1028; Assert.Equal(0xf6, second.ReadByte());
    }

    [Theory]
    [InlineData(16)] [InlineData(4096)] [InlineData(65535)]
    public void ProDosDirectoryAndBitmapAgreeOnEveryAllocatedBlock(int blocks)
    {
        using var volume = new MemoryStream(); volume.SetLength((long)blocks * 512);
        ProDosVolumeFormatter.Format(volume, "DATA");
        var bytes = volume.ToArray();
        var current = 2; var previous = 0; var directories = new HashSet<int>();
        while (current != 0)
        {
            Assert.True(directories.Add(current));
            Assert.Equal(previous, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(current * 512)));
            var next = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(current * 512 + 2));
            previous = current; current = next;
        }
        Assert.Equal(4, directories.Count);
        Assert.Equal(blocks, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(1028 + 0x25)));
        var bitmapStart = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(1028 + 0x23));
        var bitmapBlocks = (blocks + 4095) / 4096;
        for (var block = 0; block < bitmapBlocks * 4096; block++)
        {
            var free = (bytes[bitmapStart * 512 + block / 8] & (0x80 >> (block % 8))) != 0;
            Assert.Equal(block >= 6 + bitmapBlocks && block < blocks, free);
        }
    }

    [Fact]
    public void CpmErasesDirectoryAndPreservesReservedAndDataAreas()
    {
        using var volume = new MemoryStream(); volume.SetLength(8L << 20);
        volume.Position = 0; volume.WriteByte(42);
        volume.Position = 4096 + 512 * 32; volume.WriteByte(43);
        CpmVolumeFormatter.Format(volume, new(4096, 512, 4096));
        var bytes = volume.ToArray();
        Assert.Equal(42, bytes[0]); Assert.Equal(43, bytes[4096 + 512 * 32]);
        Assert.All(bytes[4096..(4096 + 512 * 32)], value => Assert.Equal(0xe5, value));
    }

    [Fact]
    public void InvalidFilesystemParametersDoNotWrite()
    {
        using var volume = new MemoryStream(); volume.SetLength(1L << 20); volume.WriteByte(42);
        Assert.Throws<ArgumentException>(() => ProDosVolumeFormatter.Format(volume, "bad label"));
        Assert.Throws<ArgumentException>(() => CpmVolumeFormatter.Format(volume, new(1024, 512)));
        volume.Position = 0; Assert.Equal(42, volume.ReadByte());
    }
}
