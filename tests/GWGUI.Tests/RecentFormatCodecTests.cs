using System.IO;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Reconstruction.Commodore;
using GWGUI.MediaEngine.Reconstruction.Dec;
using GWGUI.MediaEngine.Reconstruction.Iso;
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
        var image = await new IsoScpSectorImageReader(Fake(0, 0, revolution), new FluxDecoderRegistry()).ReadAsync("unused.scp", DiskImageFormatIds.AcornDfsSingleSided);

        Assert.Equal("acorn.dfs.ss", image.FormatId);
        Assert.True(new FileSystemRegistry().TryRead(image, null, out var match));
        Assert.NotNull(match);
        Assert.Equal(GWGUI.MediaEngine.FileSystems.Definitions.FileSystemIds.AcornDfs, match.Volume.FileSystemId);
        Assert.Contains(match.Volume.Entries, entry => entry.Name == "FILE" && entry.Content!.SequenceEqual(new byte[] { 1, 2, 3 }));
    }

    [Fact]
    public async Task AcornAdfsSelectionUsesTheGenericIsoPolicy()
    {
        var sectors = Enumerable.Range(1, 5).Select(number => new TrackSector(number, new byte[256])).ToArray();
        var revolution = new FluxEncoderRegistry().Encode("iso.fm", new TrackEncodeRequest(0, 0, sectors)).Revolution;

        var image = await new IsoScpSectorImageReader(Fake(0, 0, revolution), new FluxDecoderRegistry()).ReadAsync("unused.scp", DiskImageFormatIds.AcornAdfs800);

        Assert.Equal(DiskImageFormatIds.AcornAdfs800, image.FormatId);
    }

    [Fact]
    public async Task Commodore1581ScpMapsAValidPhysicalSector()
    {
        var data = Enumerable.Range(0, 512).Select(index => (byte)index).ToArray();
        var revolution = new FluxEncoderRegistry().Encode("iso.mfm", new TrackEncodeRequest(0, 0, [new TrackSector(1, data)])).Revolution;

        var image = await new CommodoreScpSectorImageReader(Fake(0, 0, revolution), new FluxDecoderRegistry()).ReadAsync("unused.scp", DiskImageFormatIds.Commodore1581);

        Assert.Equal(data.Take(256), image.GetBlock(20).ToArray());
        Assert.Equal(data.Skip(256), image.GetBlock(21).ToArray());
    }

    [Theory]
    [InlineData(80, 0)]
    [InlineData(0, 2)]
    public async Task Commodore1581ScpRejectsCandidatesOutsidePhysicalGeometry(int cylinder, int head)
    {
        var revolution = new FluxEncoderRegistry().Encode("iso.mfm", new TrackEncodeRequest(cylinder, Math.Min(head, 1), [new TrackSector(1, new byte[512])])).Revolution;

        await Assert.ThrowsAsync<InvalidDataException>(() => new CommodoreScpSectorImageReader(Fake(cylinder, head, revolution), new FluxDecoderRegistry()).ReadAsync("unused.scp", DiskImageFormatIds.Commodore1581));
    }

    [Theory]
    [InlineData(DiskImageFormatIds.IbmScan)]
    [InlineData(DiskImageFormatIds.Mac1440)]
    public async Task ExplicitIbmCompatibleSelectionUsesThePublicIsoReader(string formatId)
    {
        var sectors = Enumerable.Range(1, 9).Select(number => new TrackSector(number, new byte[512])).ToArray();
        var revolution = new FluxEncoderRegistry().Encode("iso.mfm", new TrackEncodeRequest(0, 0, sectors)).Revolution;

        var image = await new IsoScpSectorImageReader(Fake(0, 0, revolution), new FluxDecoderRegistry()).ReadAsync("unused.scp", formatId);

        Assert.StartsWith(DiskImageFormatIds.IbmPrefix, image.FormatId, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task IsoReaderSelectsTheDecoderProducingSectorDataAndRecordsTheRevolution()
    {
        var first = new FluxEncoderRegistry().Encode(FluxCodecIds.IsoFm, new TrackEncodeRequest(0, 0, [new TrackSector(1, Enumerable.Repeat((byte)1, 256).ToArray())])).Revolution;
        var second = new FluxEncoderRegistry().Encode(FluxCodecIds.IsoFm, new TrackEncodeRequest(0, 0, [new TrackSector(1, Enumerable.Repeat((byte)2, 256).ToArray())])).Revolution;
        var image = await new IsoScpSectorImageReader(Fake(0, 0, first, second), new FluxDecoderRegistry()).ReadAsync("unused.scp", "custom.iso");
        var block = Assert.Single(image.AvailableBlocks);
        Assert.Equal(1, block.Revolution);
        Assert.All(block.Data, value => Assert.Equal(1, value));
    }

    [Fact]
    public async Task IsoReaderSeparatesMismatchedInternalAddressIntoPhysicalCandidates()
    {
        var revolution = new FluxEncoderRegistry().Encode(FluxCodecIds.IsoMfm, new TrackEncodeRequest(2, 0, Enumerable.Range(1, 8).Select(number => new TrackSector(number, new byte[512])).ToArray())).Revolution;
        var image = await new IsoScpSectorImageReader(Fake(0, 0, revolution), new FluxDecoderRegistry()).ReadAsync("unused.scp", DiskImageFormatIds.UcsdIbmMfm);
        Assert.Equal(DiskImageFormatIds.UcsdIbmMfm, image.FormatId);
        Assert.NotEmpty(image.AvailableBlocks);
    }

    [Fact]
    public async Task IsoReaderRejectsRevolutionWithoutAddressedOrPhysicalCandidate()
    {
        var empty = new FluxRevolution(1, []);
        await Assert.ThrowsAsync<InvalidDataException>(() => new IsoScpSectorImageReader(Fake(0, 0, empty), new FluxDecoderRegistry()).ReadAsync("unused.scp", "custom.iso"));
    }

    [Fact]
    public void ExplicitIbmPolicyRejectsAnUnrelatedIdentifier()
    {
        var empty = new Dictionary<SectorAddress, List<IsoSectorCandidate>>();
        Assert.Throws<InvalidDataException>(() => new IbmPcIsoScpSectorImagePolicy(true).Build(DiskImageFormatIds.Atari90, new(empty, empty)));
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

    [Fact]
    public async Task DecRx02ScpRejectsAnUnpairedPhysicalSector()
    {
        var revolution = new FluxEncoderRegistry().Encode("dec.rx02", new TrackEncodeRequest(1, 0, [new TrackSector(1, new byte[256])])).Revolution;

        await Assert.ThrowsAsync<InvalidDataException>(() => new DecRx02ScpSectorImageReader(Fake(1, 0, revolution), new FluxDecoderRegistry()).ReadAsync("unused.scp"));
    }

    private static IScpReader Fake(int cylinder, int head, params FluxRevolution[] revolutions) => new FakeReader(new ScpImage(
        new ScpHeader(0, 0, 1, (byte)(cylinder * 2 + head), (byte)(cylinder * 2 + head), ScpFlags.None, ScpBitCellEncoding.Default16Bit, ScpHeadSelection.Both, 0, 0),
        [new ScpTrack((byte)(cylinder * 2 + head), cylinder, head, revolutions.Select(revolution => new ScpRevolution(revolution, (uint)revolution.FluxIntervals.Count)).ToArray())], true, 0));

    private sealed class FakeReader(ScpImage image) : IScpReader
    {
        public Task<ScpImage> ReadAsync(string path, CancellationToken cancellationToken = default) => Task.FromResult(image);
    }
}
