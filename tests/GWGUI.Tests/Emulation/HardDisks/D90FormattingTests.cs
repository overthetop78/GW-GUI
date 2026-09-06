using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class D90FormattingTests
{
    [Theory]
    [InlineData(4, 19441)] [InlineData(6, 29162)]
    public void BamChainCoversDiskAndProtectsEveryMetadataSector(int heads, int expectedFree)
    {
        var capacity = 153L * heads * 32 * 256;
        using var disk = new SparseMemoryStream();
        DiskImageBuilder.Write(disk, new(capacity, "raw", "none", [new(0, capacity, "d90-dos", "TEST", SectorBytes: 256)]));
        byte[] Sector(int track, int sector)
        { var bytes = new byte[256]; disk.Position = (track * heads * 32L + sector) * 256; disk.ReadExactly(bytes); return bytes; }
        var config = Sector(0, 0); var header = Sector(config[6], config[7]);
        Assert.Equal("TEST"u8.ToArray(), header[6..10]); Assert.Equal("ID"u8.ToArray(), header[24..26]);
        Assert.Equal("3A"u8.ToArray(), header[27..29]);
        Assert.Equal(config[4..6], header[..2]); Assert.All(Sector(0, 1), b => Assert.Equal(0xff, b));
        var directory = Sector(header[0], header[1]); Assert.Equal(0, directory[0]); Assert.Equal(0xff, directory[1]);
        Assert.All(directory[2..], b => Assert.Equal(0, b));
        var metadata = new HashSet<int> { 0, 1, config[4] * heads * 32 + config[5], config[6] * heads * 32 + config[7] };
        var free = new HashSet<int>(); var seen = new HashSet<int>();
        var track = (int)config[8]; var sector = (int)config[9]; var previous = 0xffff; var covered = 0;
        while (track != 0xff || sector != 0xff)
        {
            Assert.True(seen.Add(track * heads * 32 + sector)); metadata.Add(track * heads * 32 + sector);
            var bam = Sector(track, sector); Assert.Equal(previous, bam[2] * 256 + bam[3]);
            Assert.Equal(covered, bam[4]); Assert.InRange((int)bam[5], covered + 1, 153);
            var start = 6 + 250 % (5 * heads);
            for (var t = bam[4]; t < bam[5]; t++)
            for (var h = 0; h < heads; h++)
            {
                var at = start + ((t - bam[4]) * heads + h) * 5; var freeCount = 0;
                for (var s = 0; s < 32; s++)
                    if ((bam[at + 1 + s / 8] & (1 << (s % 8))) != 0)
                    { Assert.True(free.Add((t * heads + h) * 32 + s)); freeCount++; }
                Assert.Equal(freeCount, bam[at]);
            }
            covered = bam[5]; previous = track * 256 + sector; track = bam[0]; sector = bam[1];
        }
        Assert.Equal(153, covered); Assert.Empty(metadata.Intersect(free));
        Assert.Equal(153 * heads * 32, metadata.Count + free.Count);
        Assert.Equal(expectedFree, free.Count(block => block >= heads * 32));
    }

    [Theory]
    [InlineData(5013504, "lower", 256)] [InlineData(5013504, "TEST", 512)] [InlineData(5014016, "TEST", 256)]
    public void InvalidProfilesWriteNothing(long capacity, string label, int sector)
    {
        using var disk = new MemoryStream();
        Assert.ThrowsAny<ArgumentException>(() => DiskImageBuilder.Write(disk,
            new(capacity, "raw", "none", [new(0, capacity, "d90-dos", label, SectorBytes: sector)])));
        Assert.Equal(0, disk.Length);
    }
}
