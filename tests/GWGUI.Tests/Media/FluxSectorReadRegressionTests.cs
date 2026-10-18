using GWGUI.MediaEngine.Images.Reading.Decoding;
using GWGUI.MediaEngine.Images.Reading.Decoding.Definitions;
using GWGUI.MediaEngine.Images.Writing.Encoding;

namespace GWGUI.Tests.Media;

public sealed class FluxSectorReadRegressionTests
{
    [Fact]
    public void DataGeneralPayloadSyncPatternsDoNotHideFollowingSectors()
    {
        var sectors = Enumerable.Range(0, 8).Select(number => new TrackSector(number,
            Enumerable.Range(0, 512).Select(index => (byte)(index + number)).ToArray())).ToArray();
        var encoded = new DataGeneralFmTrackEncoder().Encode(new TrackEncodeRequest(4, 0, sectors));
        var decoded = new DataGeneralFmDecoder().Decode(encoded.Revolution);
        Assert.Equal(8, decoded.Sectors.Count);
        foreach (var sector in decoded.Sectors)
        {
            Assert.True(sector.IntegrityValid);
            Assert.Equal(sectors[sector.Number].Data, sector.Data);
        }
    }

    [Fact]
    public void MicropolisCanPreserveTheCompleteRecordWhileKeepingPayloadModeByDefault()
    {
        var data = Enumerable.Range(0, 256).Select(index => (byte)index).ToArray();
        var encoded = new MicropolisMfmTrackEncoder().Encode(new TrackEncodeRequest(27, 0, [new TrackSector(7, data)]));
        var payload = Assert.Single(new MicropolisMfmDecoder().Decode(encoded.Revolution).Sectors);
        var record = Assert.Single(new MicropolisMfmDecoder(includeRecord: true).Decode(encoded.Revolution).Sectors);
        Assert.Equal(data, payload.Data);
        Assert.Equal(MicropolisMfmRecord.Create(27, 7, data).Bytes, record.Data);
        Assert.True(record.IntegrityValid);
    }
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void NativeNorthstarReadsTenHardwareSlotsWithoutInventingHeaderAddresses(bool fm)
    {
        var bits = new List<bool>();
        var expected = new List<byte[]>();
        var size = fm ? 256 : 512;
        var cellTicks = fm ? 160u : 80u;
        for (var sector = 0; sector < 10; sector++)
        {
            var data = Enumerable.Range(0, size).Select(index => (byte)(index + sector * 7)).ToArray();
            expected.Add(data);
            var bytes = Enumerable.Repeat((byte)0, fm ? 17 : 34)
                .Concat(Enumerable.Repeat((byte)0xfb, fm ? 1 : 2))
                .Concat(data).Append(GWGUI.MediaEngine.Functions.RotatingChecksumCalculator.Compute(data)).ToArray();
            var start = bits.Count;
            if (fm) TrackBitEncoding.Fm(bits, bytes);
            else TrackBitEncoding.Mfm(bits, bytes);
            var slotCells = 800000 / (int)cellTicks;
            while (bits.Count < start + slotCells) bits.Add((bits.Count - start) % 2 == 0);
        }
        var revolution = GWGUI.MediaEngine.Images.Models.Flux.FluxRevolutionFactory.Create(bits, cellTicks, 8000000);
        var actual = new NorthstarMfmDecoder().DecodeHardSectors(revolution, fm, 27, 1, 10);
        Assert.Equal(10, actual.Count);
        foreach (var sector in actual)
        {
            Assert.Equal(27, sector.Cylinder);
            Assert.Equal(1, sector.Head);
            Assert.True(sector.IntegrityValid);
            Assert.Equal(expected[sector.Number], sector.Data);
        }
    }
}
