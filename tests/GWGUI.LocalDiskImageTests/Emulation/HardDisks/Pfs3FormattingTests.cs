using System.Buffers.Binary;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.HardDisks.FileSystems;
using Hst.Amiga.FileSystems.Pfs3;
using PfsFileMode = Hst.Amiga.FileSystems.FileMode;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class Pfs3FormattingTests
{
    [Theory]
    [InlineData(8L << 20)] [InlineData(6L << 30)] [InlineData(Pfs3VolumeFormatter.MaximumCapacity)]
    public async Task FormattedVolumesSupportDirectoriesFilesAndPersistentReopening(long capacity)
    {
        using var image = new SparseMemoryStream(); image.SetLength(capacity);
        Pfs3VolumeFormatter.Format(image, "Données");
        var payload = new byte[65536 + 71]; new Random(72).NextBytes(payload);
        await using (var fs = await Pfs3Volume.Mount(image, Pfs3VolumeFormatter.DescribeVolume(capacity)))
        {
            Assert.Equal("Données", fs.Name); Assert.Equal(capacity, fs.Size);
            Assert.True(fs.Free > 0 && fs.Free < capacity);
            await fs.CreateDirectory("Files"); await fs.ChangeDirectory("Files");
            await using var file = await fs.OpenFile("A long file name beyond thirty characters.bin", PfsFileMode.Write);
            await file.WriteAsync(payload);
        }
        Assert.True(image.CanWrite);
        await using (var fs = await Pfs3Volume.Mount(image, Pfs3VolumeFormatter.DescribeVolume(capacity)))
        {
            await fs.ChangeDirectory("Files");
            await using var file = await fs.OpenFile("A long file name beyond thirty characters.bin", PfsFileMode.Read);
            var actual = new byte[payload.Length]; await file.ReadExactlyAsync(actual); Assert.Equal(payload, actual);
        }
        image.Position = 0; var signature = new byte[4]; image.ReadExactly(signature);
        Assert.Equal(0x50465301U, BinaryPrimitives.ReadUInt32BigEndian(signature));
    }

    [Theory]
    [InlineData(null)] [InlineData("PDS\u0003")]
    public async Task RdbVolumeUsesPfsDosTypeWithoutOverwritingAdjacentPartitions(string? driverType)
    {
        using var disk = new SparseMemoryStream();
        DiskImageBuilder.Write(disk, new(32L << 20, "raw", "rdb",
            [new(1L << 20, 8L << 20, "pfs3", "DATA", PartitionType: driverType, PartitionName: "DH0"),
             new(10L << 20, 8L << 20, "fat16", "OTHER", PartitionType: "FAT\0")]));
        disk.Position = 512 + 192; var type = new byte[4]; disk.ReadExactly(type);
        Assert.Equal(driverType is null ? 0x50465303U : 0x50445303U, BinaryPrimitives.ReadUInt32BigEndian(type));
        using var volume = new SubStream(disk, Ownership.None, 1L << 20, 8L << 20);
        await using var fs = await Pfs3Volume.Mount(volume, Pfs3VolumeFormatter.DescribeVolume(volume.Length));
        Assert.Equal("DATA", fs.Name);
        using var other = new SubStream(disk, Ownership.None, 10L << 20, 8L << 20);
        using var fat = new DiscUtils.Fat.FatFileSystem(other); Assert.Equal("OTHER", fat.VolumeLabel.Trim());
    }

    [Fact]
    public void InvalidNamesAndUnimplementedLargeBlockModesAreRejectedBeforeWriting()
    {
        using var image = new MemoryStream();
        foreach (var label in new[] { "", new string('A', 32), "A/B", "A:B", "日本語" })
            Assert.Throws<ArgumentException>(() => DiskImageBuilder.Write(image,
                new(8L << 20, "raw", "none", [new(0, 8L << 20, "pfs3", label)])));
        Assert.Throws<ArgumentOutOfRangeException>(() => Pfs3VolumeFormatter.Validate(Pfs3VolumeFormatter.MaximumCapacity + 512, "DATA"));
        Assert.Throws<ArgumentOutOfRangeException>(() => Pfs3VolumeFormatter.Validate((8L << 20) - 512, "DATA"));
        Assert.Equal(0, image.Length);
    }
}
