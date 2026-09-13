using System.Buffers.Binary;
using DiscUtils.Fat;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.HardDisks.Partitioning;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class LegacyDisklabelTests
{
    [Theory]
    [InlineData("sun-vtoc", 7)]
    [InlineData("sgi", 14)]
    public void AllDataSlotsRetainTheirFormattedVolumes(string table, int count)
    {
        const long size = 2L << 20;
        var volumes = Enumerable.Range(0, count).Select(i => new DiskVolumePlan((i + 1) * size, size,
            "fat12", $"DATA{i}", PartitionType: "131")).ToArray();
        using var disk = new SparseMemoryStream();
        DiskImageBuilder.Write(disk, new((count + 1) * size, "raw", table, volumes));
        var label = new byte[512]; disk.Position = 0; disk.ReadExactly(label);
        var slices = new List<(long Start, long Size)>();
        if (table == "sun-vtoc")
        {
            Assert.Equal(0xdabe, U16(label, 508)); Assert.Equal(0x600ddeeeu, U32(label, 188));
            Assert.Equal(1u, U32(label, 128)); Assert.Equal(8, U16(label, 140));
            ushort sum = 0; for (var i = 0; i < 512; i += 2) sum ^= U16(label, i); Assert.Equal(0, sum);
            var cylinder = U16(label, 436) * U16(label, 438) * 512L;
            for (var i = 0; i < 8; i++)
            {
                if (U16(label, 142 + i * 4) == 5) { Assert.Equal(disk.Length / 512, U32(label, 448 + i * 8)); continue; }
                slices.Add((U32(label, 444 + i * 8) * cylinder, U32(label, 448 + i * 8) * 512L));
            }
        }
        else
        {
            Assert.Equal(0x0be5a941u, U32(label, 0)); Assert.Equal(512, U16(label, 40));
            uint sum = 0; for (var i = 0; i < 512; i += 4) sum = unchecked(sum + U32(label, i)); Assert.Equal(0u, sum);
            for (var i = 0; i < 16; i++)
            {
                var type = U32(label, 320 + i * 12);
                if (type is 0 or 6) { Assert.Equal(0u, U32(label, 316 + i * 12)); continue; }
                slices.Add((U32(label, 316 + i * 12) * 512L, U32(label, 312 + i * 12) * 512L));
            }
        }
        Assert.Equal(count, slices.Count);
        for (var i = 0; i < slices.Count; i++)
        {
            Assert.Equal(volumes[i].OffsetBytes, slices[i].Start); Assert.Equal(size, slices[i].Size);
            using var view = new SubStream(disk, Ownership.None, slices[i].Start, slices[i].Size);
            using var fs = new FatFileSystem(view); Assert.Equal($"DATA{i}", fs.VolumeLabel.Trim());
        }
    }

    [Theory]
    [InlineData("sun-vtoc")]
    [InlineData("sgi")]
    public void MetadataCollisionsAndUnknownTypesLeaveDestinationUntouched(string table)
    {
        using var disk = new MemoryStream();
        Assert.Throws<ArgumentException>(() => DiskImageBuilder.Write(disk, new(32L << 20, "raw", table,
            [new(512, 2L << 20, "fat12", "DATA", PartitionType: "131")])));
        Assert.Throws<ArgumentException>(() => DiskImageBuilder.Write(disk, new(32L << 20, "raw", table,
            [new(4L << 20, 2L << 20, "fat12", "DATA", PartitionType: "wrong")])));
        Assert.Equal(0, disk.Length);
    }

    [Fact]
    public void ExplicitGeometryIsAppliedAndOverflowRejected()
    {
        var registry = DiskFormatRegistry.CreateDefault();
        registry.Register(SunDisklabelWriter.Describe("sun-custom", new(4, 17)));
        var cylinder = 4L * 17 * 512;
        using var disk = new MemoryStream();
        DiskImageBuilder.Write(disk, new(cylinder * 100, "raw", "sun-custom", [new(cylinder, cylinder * 99, Label: "")] ), registry);
        var label = disk.ToArray(); Assert.Equal(4, U16(label, 436)); Assert.Equal(17, U16(label, 438));
        Assert.Equal(100, U16(label, 432));
        Assert.Throws<ArgumentException>(() => SunDisklabelWriter.Validate(cylinder * 65536, [], new(4, 17)));
        Assert.Throws<ArgumentException>(() => SgiDisklabelWriter.Validate(cylinder * 65536, [], new(4, 17)));
    }
    [Theory]
    [InlineData(null)] [InlineData(2)]
    public void FiveSunBackupsOccupyOnlyTheSelectedTrackOfTheLastAlternateCylinder(int? head)
    {
        var geometry = new LegacyDiskGeometry(4, 17);
        var capacity = geometry.CylinderBytes * 100;
        var registry = DiskFormatRegistry.CreateDefault();
        registry.Register(SunDisklabelWriter.Describe("sun-backup-custom", geometry, new(2, head)));
        using var disk = new SparseMemoryStream();
        DiskImageBuilder.Write(disk, new(capacity, "raw", "sun-backup-custom",
            [new(geometry.CylinderBytes, geometry.CylinderBytes * 97, "fat12", "DATA", PartitionType: "131")]), registry);
        var label = new byte[512]; disk.Position = 0; disk.ReadExactly(label);
        Assert.Equal(100, U16(label, 422)); Assert.Equal(98, U16(label, 432)); Assert.Equal(2, U16(label, 434));
        Assert.Equal((uint)(geometry.CylinderBytes * 98 / 512), U32(label, 464));
        var backupTrack = (99L * 4 + (head ?? 3)) * 17;
        for (var sector = 0; sector < 17; sector++)
        {
            var copy = new byte[512]; disk.Position = (backupTrack + sector) * 512; disk.ReadExactly(copy);
            if (sector is 1 or 3 or 5 or 7 or 9)
            {
                Assert.Equal(label, copy);
                ushort checksum = 0; for (var i = 0; i < 512; i += 2) checksum ^= U16(copy, i);
                Assert.Equal(0, checksum);
            }
            else Assert.All(copy, value => Assert.Equal(0, value));
        }
        using var volume = new SubStream(disk, Ownership.None, geometry.CylinderBytes, geometry.CylinderBytes * 97);
        using var fs = new FatFileSystem(volume); Assert.Equal("DATA", fs.VolumeLabel.Trim());
    }

    [Fact]
    public void SunBackupGeometryAndCollisionsAreValidatedBeforeWriting()
    {
        var geometry = new LegacyDiskGeometry(); var cylinder = geometry.CylinderBytes;
        Assert.Throws<ArgumentException>(() => SunDisklabelWriter.Validate(cylinder * 100,
            [new(cylinder, 98 * cylinder)], geometry, new()));
        Assert.Throws<ArgumentException>(() => SunDisklabelWriter.Validate(cylinder * 100, [], geometry, new(0)));
        Assert.Throws<ArgumentException>(() => SunDisklabelWriter.Validate(cylinder * 100, [], geometry, new(2, 16)));
        Assert.Throws<ArgumentException>(() => SunDisklabelWriter.Validate(16L * 9 * 512 * 100, [], new(16, 9), new()));
        using var disk = new MemoryStream();
        Assert.Throws<ArgumentException>(() => DiskImageBuilder.Write(disk, new(cylinder * 3, "raw", "sun-vtoc-backup", [])));
        Assert.Equal(0, disk.Length);
    }

    private static ushort U16(byte[] data, int at) => BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(at));
    private static uint U32(byte[] data, int at) => BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(at));
}
