using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

public sealed class RecentFormatCodecTests
{
    [Fact]
    public async Task BbcDfsScpUsesZeroBasedFmSectorsAndExposesCatalogue()
    {
        var data = Enumerable.Range(0, 3).Select(_ => new byte[256]).ToArray();
        "BBC TEST"u8.CopyTo(data[0]);
        "FILE   "u8.CopyTo(data[0].AsSpan(8)); data[0][15] = (byte)'$';
        "VOL "u8.CopyTo(data[1]); data[1][5] = 8; data[1][6] = 1; data[1][7] = 0x90;
        data[1][12] = 3; data[1][15] = 2;
        data[2][0] = 1; data[2][1] = 2; data[2][2] = 3;
        var sectors = data.Select((bytes, number) => new TrackSector(number, bytes)).ToArray();
        var revolution = new FluxEncoderRegistry().Encode("iso.fm", new TrackEncodeRequest(0, 0, sectors)).Revolution;
        var decoded = new FluxDecoderRegistry().Decode("iso.fm", revolution);
        Assert.NotNull(decoded.Sectors);
        Assert.NotEmpty(decoded.Sectors);
        var image = await new BbcScpSectorImageReader(Fake(0, 0, revolution), new FluxDecoderRegistry())
            .ReadAsync("unused.scp", "acorn.dfs.ss");

        Assert.Equal("acorn.dfs.ss", image.FormatId);
        Assert.True(new FileSystemRegistry().TryRead(image, null, out var volume));
        Assert.Equal("Acorn DFS", volume.FileSystem);
        Assert.Contains(volume.Entries, entry => entry.Name == "FILE" && entry.Content!.SequenceEqual(new byte[] { 1, 2, 3 }));
    }

    [Fact]
    public async Task DecRx02ScpReassemblesPhysicalM2FmSectorsIntoRt11Block()
    {
        var first = Enumerable.Range(0, 256).Select(index => (byte)index).ToArray();
        var second = Enumerable.Range(0, 256).Select(index => (byte)(255 - index)).ToArray();
        var revolution = new FluxEncoderRegistry().Encode("dec.rx02", new TrackEncodeRequest(1, 0,
            [new TrackSector(1, first), new TrackSector(3, second)])).Revolution;
        var image = await new DecRx02ScpSectorImageReader(Fake(1, 0, revolution), new FluxDecoderRegistry())
            .ReadAsync("unused.scp");

        Assert.True(image.TryGetBlock(0, out var block));
        Assert.Equal(first.Concat(second), block.Data);
        Assert.True(block.IntegrityValid);
    }

    private static IScpReader Fake(int cylinder, int head, FluxRevolution revolution) => new FakeReader(new ScpImage(
        new ScpHeader(0, 0, 1, (byte)(cylinder * 2 + head), (byte)(cylinder * 2 + head), ScpFlags.None, ScpBitCellEncoding.Default16Bit, ScpHeadSelection.Both, 0, 0),
        [new ScpTrack((byte)(cylinder * 2 + head), cylinder, head, [new ScpRevolution(revolution, (uint)revolution.FluxIntervals.Count)])], true, 0));

    private sealed class FakeReader(ScpImage image) : IScpReader
    {
        public Task<ScpImage> ReadAsync(string path, CancellationToken cancellationToken = default) => Task.FromResult(image);
    }
}
