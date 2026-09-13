using System.Buffers.Binary;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.HardDisks.FileSystems;
using Hst.Amiga.FileSystems.FastFileSystem;
using FsFileMode = Hst.Amiga.FileSystems.FileMode;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class LongNameDosTests
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task LongNamesCommentsAndFileDataSurviveReopening(bool fast)
    {
        const long capacity = 8L << 20;
        using var disk = new SparseMemoryStream(); disk.SetLength(capacity);
        LongNameDosVolumeFormatter.Format(disk, "Données", fast);
        var name = new string('A', 100) + ".bin";
        var directory = new string('D', 100);
        var comment = new string('C', 70);
        var payload = new byte[64000 + 27]; new Random(13).NextBytes(payload);
        await using (var fs = await FastFileSystemVolume.MountPartition(disk, LongNameDosVolumeFormatter.DescribeVolume(capacity, fast)))
        {
            Assert.Equal("Données", fs.Name);
            await fs.CreateDirectory(directory); await fs.ChangeDirectory(directory);
            await using (var file = await fs.OpenFile(name, FsFileMode.Write)) await file.WriteAsync(payload);
            await fs.SetComment(name, comment);
        }
        await using (var fs = await FastFileSystemVolume.MountPartition(disk, LongNameDosVolumeFormatter.DescribeVolume(capacity, fast)))
        {
            await fs.ChangeDirectory(directory);
            var entry = Assert.Single(await fs.ListEntries()); Assert.Equal(name, entry.Name); Assert.Equal(comment, entry.Comment);
            await using var file = await fs.OpenFile(name, FsFileMode.Read);
            var actual = new byte[payload.Length]; await file.ReadExactlyAsync(actual); Assert.Equal(payload, actual);
        }
        var boot = new byte[1024]; disk.Position = 0; disk.ReadExactly(boot);
        Assert.Equal(fast ? 7 : 6, boot[3]); Assert.Equal((uint)(capacity / 1024), BinaryPrimitives.ReadUInt32BigEndian(boot.AsSpan(8)));
        uint sum = 0;
        for (var i = 0; i < boot.Length; i += 4)
        {
            var next = unchecked(sum + BinaryPrimitives.ReadUInt32BigEndian(boot.AsSpan(i)));
            if (next < sum) next++; sum = next;
        }
        Assert.Equal(uint.MaxValue, sum);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task RdbDosTypeMatchesTheFormattedVolume(bool fast)
    {
        using var disk = new SparseMemoryStream();
        DiskImageBuilder.Write(disk, new(16L << 20, "raw", "rdb",
            [new(1L << 20, 8L << 20, fast ? "ffs-longnames" : "ofs-longnames", "DATA")]));
        disk.Position = 512 + 195; Assert.Equal(fast ? 7 : 6, disk.ReadByte());
        using var volume = new SubStream(disk, Ownership.None, 1L << 20, 8L << 20);
        await using var fs = await FastFileSystemVolume.MountPartition(volume, LongNameDosVolumeFormatter.DescribeVolume(volume.Length, fast));
        Assert.Equal("DATA", fs.Name);
    }

    [Fact]
    public void InvalidProfilesAreRejectedBeforeContainerOutput()
    {
        using var disk = new MemoryStream();
        Assert.Throws<ArgumentOutOfRangeException>(() => DiskImageBuilder.Write(disk,
            new(2L << 30, "raw", "none", [new(0, 2L << 30, "ffs-longnames", "DATA")])));
        Assert.Throws<ArgumentException>(() => DiskImageBuilder.Write(disk,
            new(8L << 20, "raw", "none", [new(0, 8L << 20, "ofs-longnames", "INVALID/NAME")])));
        Assert.Equal(0, disk.Length);
    }
}
