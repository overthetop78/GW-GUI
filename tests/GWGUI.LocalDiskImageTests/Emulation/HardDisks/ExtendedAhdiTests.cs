using System.Buffers.Binary;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class ExtendedAhdiTests
{
    [Theory]
    [InlineData("ahdi", 0, 1)]
    [InlineData("ahdi", 3, 6)]
    [InlineData("icd", 12, 0)]
    public void AllPartitionsResolveToTheirData(string table, int primary, int logical)
    {
        var volumes = Enumerable.Range(0, primary + logical).Select(i =>
            new DiskVolumePlan((1L + 3 * i) << 20, 2L << 20, "fat12", $"VOL{i}",
                PartitionType: "GEM", AhdiLogical: i >= primary)).ToArray();
        using var disk = new SparseMemoryStream();
        DiskImageBuilder.Write(disk, new(64L << 20, "raw", table, volumes));
        var root = Read(disk, 0);
        var found = new List<(long Start, long Length)>();
        for (var i = 0; i < 4; i++)
        {
            var at = 0x1c6 + i * 12;
            if (root[at] == 0) continue;
            var start = Word(root, at + 4); var length = Word(root, at + 8);
            if (!root.AsSpan(at + 1, 3).SequenceEqual("XGM"u8)) { found.Add((start, length)); continue; }
            long current = start;
            var visited = new HashSet<long>();
            while (true)
            {
                Assert.True(visited.Add(current)); Assert.True(visited.Count <= logical);
                var aux = Read(disk, current);
                Assert.Equal(1, aux[0x1c6]);
                found.Add((current + Word(aux, 0x1ca), Word(aux, 0x1ce)));
                if (aux[0x1d2] == 0) break;
                Assert.Equal("XGM"u8.ToArray(), aux[0x1d3..0x1d6]);
                current = start + Word(aux, 0x1d6);
                Assert.InRange(current, (long)start, start + length - 1L);
            }
        }
        if (table == "icd")
            for (var i = 0; i < 8; i++)
            {
                var at = 0x156 + i * 12;
                if (root[at] != 0) found.Add((Word(root, at + 4), Word(root, at + 8)));
            }
        Assert.Equal(volumes.Length, found.Count);
        foreach (var volume in volumes)
        {
            Assert.Contains((volume.OffsetBytes / 512, volume.LengthBytes / 512), found);
            using var content = new SubStream(disk, Ownership.None, volume.OffsetBytes, volume.LengthBytes);
            using var fs = new DiscUtils.Fat.FatFileSystem(content);
            Assert.Equal(volume.Label, fs.VolumeLabel.Trim());
        }
    }

    [Fact]
    public void InvalidChainsAreRejectedBeforeWriting()
    {
        DiskVolumePlan L(long start) => new(start, 1024, AhdiLogical: true);
        DiskImagePlan[] invalid = [
            new(16384,"raw","ahdi",[L(512)]),
            new(16384,"raw","ahdi",[L(1024),L(2048)]),
            new(16384,"raw","ahdi",[L(1024),new(3072,512),L(4096)]),
            new(16384,"raw","ahdi",[new(512,512),new(1024,512),new(1536,512),new(2048,512),L(4096)]),
            new(16384,"raw","icd",[L(1024)]),
            new(16384,"raw","mbr",[L(1024)]),
            new(16384,"raw","icd",Enumerable.Range(1,13).Select(i=>new DiskVolumePlan(i*512,512)).ToArray()),
            new(16384,"raw","icd",[new(512,512,PartitionType:"XYZ")])];
        foreach (var plan in invalid)
        {
            using var disk = new MemoryStream(); disk.WriteByte(42);
            Assert.ThrowsAny<ArgumentException>(()=>DiskImageBuilder.Write(disk,plan));
            Assert.Equal(new byte[]{42},disk.ToArray());
        }
    }
    private static byte[] Read(Stream disk,long sector) { var data=new byte[512]; disk.Position=sector*512; disk.ReadExactly(data); return data; }
    private static uint Word(byte[] data,int offset)=>BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(offset));
}
