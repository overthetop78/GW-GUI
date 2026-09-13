using System.Buffers.Binary;
using DiscUtils;
using DiscUtils.Partitions;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.HardDisks.Partitioning;
using DiskImageBuilder = GWGUI.Emulation.HardDisks.DiskImageBuilder;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class ChsMbrTests
{
    [Fact]
    public void PrimaryAndLogicalPartitionsShareAccurateAbsoluteChsAndBootGeometry()
    {
        const long track = 63 * 512;
        var volumes = Enumerable.Range(0, 4).Select(i => new DiskVolumePlan((1 + i * 160) * track,
            100 * track, "fat12", $"VOL{i}", Active: i == 0, MbrLogical: i > 0)).ToArray();
        using var image = new SparseMemoryStream();
        DiskImageBuilder.Write(image, new(32L << 20, "raw", "mbr-chs", volumes));
        var table = new BiosPartitionTable(image, new Geometry(66, 16, 63, 512));
        Assert.Equal(4, table.Count);
        foreach (var volume in volumes)
        {
            var partition = Assert.Single(table.Partitions, p => p.FirstSector == volume.OffsetBytes / 512);
            using var content = partition.Open();
            var boot = new byte[512]; content.ReadExactly(boot);
            Assert.Equal(63, BinaryPrimitives.ReadUInt16LittleEndian(boot.AsSpan(24)));
            Assert.Equal(16, BinaryPrimitives.ReadUInt16LittleEndian(boot.AsSpan(26)));
            Assert.Equal((uint)(volume.OffsetBytes / 512), BinaryPrimitives.ReadUInt32LittleEndian(boot.AsSpan(28)));
            using var fs = new DiscUtils.Fat.FatFileSystem(content);
            Assert.Equal(volume.Label, fs.VolumeLabel.Trim());
        }
        var root = Read(image, 0);
        CheckEntry(root.AsSpan(446), volumes[0].OffsetBytes / 512, volumes[0].LengthBytes / 512, 0, 16, 63);
        Assert.Equal(0x80, root[446]); Assert.Equal(5, root[466]);
        var firstEbr = volumes[1].OffsetBytes / 512 - 63;
        for (var i = 1; i < volumes.Length; i++)
        {
            var ebr = volumes[i].OffsetBytes / 512 - 63;
            var sector = Read(image, ebr);
            CheckEntry(sector.AsSpan(446), volumes[i].OffsetBytes / 512, volumes[i].LengthBytes / 512, ebr, 16, 63);
            if (i + 1 < volumes.Length)
            {
                var next = volumes[i + 1]; Assert.Equal(5, sector[466]);
                CheckEntry(sector.AsSpan(462), next.OffsetBytes / 512 - 63, next.LengthBytes / 512 + 63, firstEbr, 16, 63);
            }
            else Assert.All(sector[462..510], value => Assert.Equal(0, value));
        }
    }

    [Theory]
    [InlineData(1, 1)] [InlineData(16, 63)] [InlineData(255, 63)]
    public void FinalAddressUsesAllTenCylinderBitsWithoutSaturation(int heads, int sectors)
    {
        var geometry = new DiskChsGeometry(heads, sectors);
        var volume = new DiskVolumePlan(geometry.MaximumBytes - sectors * 512L, sectors * 512L);
        using var image = new SparseMemoryStream(); image.SetLength(geometry.MaximumBytes);
        ChsMbrPartitionWriter.Write(image, [volume], geometry);
        var root = Read(image, 0);
        CheckEntry(root.AsSpan(446), volume.OffsetBytes / 512, sectors, 0, heads, sectors);
        Assert.Equal(255, root[453]); Assert.Equal(0xc0 | sectors, root[452]);
        Assert.Throws<ArgumentOutOfRangeException>(() => ChsMbrPartitionWriter.Validate(geometry.MaximumBytes + 512, [], geometry));
    }

    [Theory]
    [InlineData("fat", 16)] [InlineData("fat16", 16)] [InlineData("fat32", 64)] [InlineData("ntfs", 16)]
    public void BootParametersFollowTheTableForEachBiosFilesystem(string filesystem, int mebibytes)
    {
        const long start = 63 * 512;
        using var image = new SparseMemoryStream();
        var volume = new DiskVolumePlan(start, (long)mebibytes << 20, filesystem, "DATA", MbrType: filesystem == "fat" ? (byte)6 : null);
        DiskImageBuilder.Write(image, new(128L << 20, "raw", "mbr-chs", [volume]));
        var boot = Read(image, 63);
        Assert.Equal(63, BinaryPrimitives.ReadUInt16LittleEndian(boot.AsSpan(24)));
        Assert.Equal(16, BinaryPrimitives.ReadUInt16LittleEndian(boot.AsSpan(26)));
        if (filesystem == "fat32")
        {
            Assert.Equal(0x0b, Read(image, 0)[450]);
            Assert.Equal(boot, Read(image, 69));
        }
    }

    [Fact]
    public void InvalidGeometryTypesAndReservedTrackCollisionsLeaveDiskUntouched()
    {
        const long track = 63 * 512;
        DiskVolumePlan[][] invalid =
        [
            [new(512, 1024)], [new(track, 1024, MbrLogical: true)],
            [new(track, track), new(2 * track, track, MbrLogical: true)],
            [new(2 * track, track, MbrLogical: true), new(4 * track, track), new(7 * track, track, MbrLogical: true)],
            [new(track, 1024, MbrType: 0x0e)], [new(track, 1024, BiosGeometry: new(8, 17))],
            [new(2 * track, track, MbrLogical: true, Active: true)]
        ];
        foreach (var volumes in invalid)
        {
            using var image = new MemoryStream();
            Assert.ThrowsAny<ArgumentException>(() => DiskImageBuilder.Write(image, new(8L << 20, "raw", "mbr-chs", volumes)));
            Assert.Equal(0, image.Length);
        }
        Assert.Throws<ArgumentOutOfRangeException>(() => new DiskChsGeometry(256, 63));
        Assert.Throws<ArgumentOutOfRangeException>(() => new DiskChsGeometry(16, 64));
    }

    private static byte[] Read(Stream disk, long sector)
    {
        var result = new byte[512]; disk.Position = sector * 512; disk.ReadExactly(result); return result;
    }

    private static void CheckEntry(ReadOnlySpan<byte> entry, long start, long length, long relativeTo, int heads, int sectors)
    {
        long Decode(ReadOnlySpan<byte> bytes) => (((bytes[1] & 0xc0) << 2) | bytes[2]) * (long)heads * sectors +
            bytes[0] * sectors + (bytes[1] & 63) - 1;
        Assert.Equal(start, Decode(entry[1..])); Assert.Equal(start + length - 1, Decode(entry[5..]));
        Assert.Equal((uint)(start - relativeTo), BinaryPrimitives.ReadUInt32LittleEndian(entry[8..]));
        Assert.Equal((uint)length, BinaryPrimitives.ReadUInt32LittleEndian(entry[12..]));
    }
}
