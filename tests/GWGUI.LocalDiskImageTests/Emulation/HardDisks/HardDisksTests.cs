using System.Buffers.Binary;
using DiscUtils;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.HardDisks.Containers;
using GWGUI.Emulation.HardDisks.FileSystems;
using GWGUI.Emulation.HardDisks.Partitioning;
using GWGUI.Emulation.Atari.Contracts;
using GWGUI.Emulation.Atari.Enums;
using GWGUI.Emulation.Atari.Functions;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class HardDisksTests
{
    [Theory]
    [InlineData(DiskContainerKind.Vhd, false)]
    [InlineData(DiskContainerKind.Vhdx, false)]
    [InlineData(DiskContainerKind.Vdi, false)]
    [InlineData(DiskContainerKind.Vmdk, false)]
    [InlineData(DiskContainerKind.Vhd, true)]
    [InlineData(DiskContainerKind.Vhdx, true)]
    [InlineData(DiskContainerKind.Vdi, true)]
    public void ContainerRoundTrip(DiskContainerKind kind, bool fixedSize)
    {
        using var image = new MemoryStream();
        const long capacity = 40L * 1024 * 1024;
        DiskContainerWriter.Write(image, capacity, kind, content =>
        {
            content.Position = 512; content.Write("first"u8);
            content.Position = capacity - 512; content.Write("last"u8);
        }, fixedSize);
        image.Position = 0;
        using VirtualDisk disk = kind switch
        {
            DiskContainerKind.Vhd => new DiscUtils.Vhd.Disk(image, Ownership.None),
            DiskContainerKind.Vhdx => new DiscUtils.Vhdx.Disk(image, Ownership.None),
            DiskContainerKind.Vdi => new DiscUtils.Vdi.Disk(image, Ownership.None),
            _ => new DiscUtils.Vmdk.Disk(image, Ownership.None)
        };
        Assert.Equal(capacity, disk.Capacity);
        disk.Content.Position = 512; var first = new byte[5]; disk.Content.ReadExactly(first); Assert.Equal("first"u8.ToArray(), first);
        disk.Content.Position = capacity - 512; var last = new byte[4]; disk.Content.ReadExactly(last); Assert.Equal("last"u8.ToArray(), last);
        disk.Content.Position = 2048; Assert.Equal(0, disk.Content.ReadByte());
    }

    [Fact]
    public void GzipPreservesInitializedAndEmptySectors()
    {
        using var image = new MemoryStream();
        GzipImageWriter.Write(image, 4096, content => { content.Position = 512; content.WriteByte(42); });
        image.Position = 0;
        using var gzip = new System.IO.Compression.GZipStream(image, System.IO.Compression.CompressionMode.Decompress);
        using var expanded = new MemoryStream(); gzip.CopyTo(expanded);
        var expected = new byte[4096]; expected[512] = 42;
        Assert.Equal(expected, expanded.ToArray());
    }

    [Fact]
    public void Qcow2TablesResolveWrittenSectorAndRefcounts()
    {
        using var image = new MemoryStream();
        Qcow2ImageWriter.Write(image, 40L * 1024 * 1024, content => { content.Position = 123 * 512; content.Write("payload"u8); });
        var bytes = image.ToArray();
        Assert.Equal(0x514649fbu, BinaryPrimitives.ReadUInt32BigEndian(bytes));
        var l1 = (long)BinaryPrimitives.ReadUInt64BigEndian(bytes.AsSpan(40));
        var l2 = (long)(BinaryPrimitives.ReadUInt64BigEndian(bytes.AsSpan((int)l1)) & ~(1UL << 63));
        var data = (long)(BinaryPrimitives.ReadUInt64BigEndian(bytes.AsSpan((int)l2)) & ~(1UL << 63));
        Assert.Equal("payload"u8.ToArray(), bytes.AsSpan((int)data + 123 * 512, 7).ToArray());
        var refs = (int)BinaryPrimitives.ReadUInt64BigEndian(bytes.AsSpan(48));
        var block = (int)BinaryPrimitives.ReadUInt64BigEndian(bytes.AsSpan(refs));
        for (var i = 0; i < bytes.Length / 65536; i++)
            Assert.Equal(1, BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(block + i * 2)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PcPartitionAndFilesystemCanBeReopened(bool gpt)
    {
        using var image = new SparseMemoryStream(); image.SetLength(80L * 1024 * 1024);
        var partition = gpt ? GptPartitionWriter.Create(image, DiscUtils.Partitions.WellKnownPartitionType.WindowsFat)
            : MbrPartitionWriter.Create(image, DiscUtils.Partitions.WellKnownPartitionType.WindowsFat);
        using var volume = partition.Open();
        FatVolumeFormatter.Format(volume, "TEST", partition.FirstSector);
        volume.Position = 0;
        using var fs = new DiscUtils.Fat.FatFileSystem(volume);
        using (var file = fs.OpenFile("CHECK.TXT", FileMode.Create, FileAccess.Write)) file.Write("saved"u8);
        using var read = fs.OpenFile("CHECK.TXT", FileMode.Open, FileAccess.Read);
        Assert.Equal(5, read.Length);
    }

    [Fact]
    public void NtfsCanCreateAndReadFile()
    {
        using var volume = new SparseMemoryStream(); volume.SetLength(64L * 1024 * 1024);
        NtfsVolumeFormatter.Format(volume, "TEST"); volume.Position = 0;
        using var fs = new DiscUtils.Ntfs.NtfsFileSystem(volume);
        using (var file = fs.OpenFile("check", FileMode.Create, FileAccess.Write)) file.WriteByte(42);
        using var read = fs.OpenFile("check", FileMode.Open, FileAccess.Read); Assert.Equal(42, read.ReadByte());
    }

    [Fact]
    public void AtariAhdiFatHasPartitionAndValidClusterGeometry()
    {
        using var disk = new MemoryStream(); disk.SetLength(40L * 1024 * 1024);
        HardDiskPreparationWriter.Prepare(disk, HardDiskPreparation.AtariAhdiFat16);
        var bytes = disk.GetBuffer(); Assert.Equal("BGM"u8.ToArray(), bytes.AsSpan(0x1c7, 3).ToArray());
        Assert.Equal(1u, BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(0x1ca)));
        var boot = bytes.AsSpan(512);
        Assert.Equal(1024, BinaryPrimitives.ReadUInt16LittleEndian(boot[11..])); Assert.Equal(2, boot[13]);
        var fatOffset = 512 + BinaryPrimitives.ReadUInt16LittleEndian(boot[11..]);
        Assert.Equal(0xfff8, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(fatOffset)));
    }

    [Theory]
    [InlineData(20)]
    [InlineData(40)]
    public void AtariIdePreparationStoresWordSwappedAhdiAndFat(int mib)
    {
        using var disk = new MemoryStream(); disk.SetLength((long)mib * 1024 * 1024);
        HardDiskPreparationWriter.Prepare(disk, HardDiskPreparation.AtariAhdiFat16, formatId: "atari-ide");
        var bytes = disk.GetBuffer();

        var partitionType = mib < 32 ? "GEM" : "BGM";
        Assert.Equal(new byte[] { (byte)partitionType[0], 1, (byte)partitionType[2], (byte)partitionType[1] },
            bytes.AsSpan(0x1c6, 4).ToArray());
        Assert.Equal(new byte[] { 0, 0, 1, 0 }, bytes.AsSpan(0x1ca, 4).ToArray());
        Assert.Equal(new byte[] { 0x1c, 0x60 }, bytes.AsSpan(512, 2).ToArray());
        Assert.Equal(new byte[] { (byte)'W', (byte)'G', (byte)'U', (byte)'G' }, bytes.AsSpan(514, 4).ToArray());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AmigaVolumeHasValidRootAndFreeSpaceBitmap(bool ffs)
    {
        using var volume = new MemoryStream(); volume.SetLength(80L * 1024 * 1024);
        AmigaDosVolumeFormatter.Format(volume, "TEST", ffs);
        var bytes = volume.GetBuffer(); Assert.Equal("DOS"u8.ToArray(), bytes.AsSpan(0,3).ToArray()); Assert.Equal(ffs ? 1 : 0, bytes[3]);
        var rootIndex = (int)BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(8));
        var root = bytes.AsSpan(rootIndex * 512,512); Assert.Equal(0u, Sum(root));
        Assert.NotEqual(0u,BinaryPrimitives.ReadUInt32BigEndian(root[416..])); // >25 bitmap blocks
        var bitmapIndex = (int)BinaryPrimitives.ReadUInt32BigEndian(root[316..]);
        Assert.Equal(0u, Sum(bytes.AsSpan(bitmapIndex * 512,512)));
        Assert.Equal(uint.MaxValue, BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(bitmapIndex*512+4)));
    }

    [Fact]
    public void RdbChecksumsAndPartitionBounds()
    {
        using var disk = new MemoryStream(); disk.SetLength(40L*1024*1024);
        using var volume = AmigaRdbPartitionWriter.Create(disk);
        AmigaDosVolumeFormatter.Format(volume,"TEST",true);
        var bytes = disk.GetBuffer(); Assert.Equal("RDSK"u8.ToArray(),bytes.AsSpan(0,4).ToArray());
        Assert.Equal(0u, Sum(bytes.AsSpan(0,256))); Assert.Equal(0u, Sum(bytes.AsSpan(512,256)));
        Assert.Equal("DOS"u8.ToArray(),bytes.AsSpan(16384,3).ToArray());
        Assert.Equal(disk.Length-16384,volume.Length);
    }

    [Fact]
    public void AtariHddInterfaceAndRemovalPersist()
    {
        var original = new AtariMachineConfiguration(AtariMachineModel.St);
        var settings = AtariStorageSettingsFunctions.Describe(original);
        settings = settings with { ConfiguredSlots = [EmulationMediaSlot.HardDisk0],
            MountedMedia = [new("test.ide",EmulationMediaSlot.HardDisk0,EmulationMediaType.HardDisk,false,true)],
            DeviceSettings = [new(EmulationMediaSlot.HardDisk0,InterfaceId:"ide")] };
        var mounted = AtariStorageSettingsFunctions.Apply(original,settings);
        Assert.Equal(AtariStorageBus.Ide,Assert.Single(mounted.Media).StorageBus);
        Assert.Equal("Ide",mounted.Options["storage.interface.HardDisk0"]);
        var removed = AtariStorageSettingsFunctions.Apply(mounted,settings with {MountedMedia=[]}); Assert.Empty(removed.Media);
        Assert.Throws<ArgumentException>(()=>AtariStorageSettingsFunctions.Apply(original,settings with
        {MountedMedia=[new("wrong.hdf",EmulationMediaSlot.HardDisk0,EmulationMediaType.HardDisk,false,true)]}));
    }

    private static uint Sum(ReadOnlySpan<byte> bytes)
    { uint sum=0; for(var i=0;i<bytes.Length;i+=4)sum=unchecked(sum+BinaryPrimitives.ReadUInt32BigEndian(bytes[i..])); return sum; }

    [Theory]
    [InlineData(20)] [InlineData(40)] [InlineData(256)] [InlineData(512)] [InlineData(1024)]
    public void AtariFatGeometryFitsEverySupportedCapacity(int mib)
    {
        using var disk = new SparseMemoryStream(); disk.SetLength((long)mib*1024*1024);
        using var volume = AtariAhdiPartitionWriter.Create(disk); AtariFat16VolumeFormatter.Format(volume);
        volume.Position=0; var boot = new byte[512]; volume.ReadExactly(boot);
        var size=BinaryPrimitives.ReadUInt16LittleEndian(boot.AsSpan(11));
        var count=BinaryPrimitives.ReadUInt16LittleEndian(boot.AsSpan(19));
        var fat=BinaryPrimitives.ReadUInt16LittleEndian(boot.AsSpan(22));
        var root=(512*32+size-1)/size; var clusters=(count-1-2*fat-root)/2;
        Assert.InRange(clusters,4085,32765); Assert.True((long)count*size<=volume.Length);
        Assert.True((clusters+2)*2<=fat*size); Assert.True(root>0);
    }

    [Fact]
    public void ReferenceScanFindsRelativeAndCaseVariantPathsAndRejectsInvalidJson()
    {
        Assert.True(HardDiskConfigurationReferences.ContainsPath("{\"media\":[{\"path\":\"disks/test.ide\"}]}",
            @"C:\config\DISKS\TEST.IDE", @"C:\config"));
        Assert.False(HardDiskConfigurationReferences.ContainsPath("{\"media\":[]}",@"C:\config\disk.ide",@"C:\config"));
        Assert.ThrowsAny<System.Text.Json.JsonException>(()=>HardDiskConfigurationReferences.ContainsPath("broken",@"C:\disk.ide",@"C:\config"));
    }
}
