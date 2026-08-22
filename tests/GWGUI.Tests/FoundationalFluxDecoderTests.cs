using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Exploration.Scp;
using GWGUI.MediaEngine.Flux;
using System.IO;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Exploration;
using GWGUI.Infrastructure.Processes;
using GWGUI.Infrastructure.Settings;
using GWGUI.Infrastructure.Hardware;
using SkiaSharp;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Threading;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace GWGUI.Tests;

public sealed class FoundationalFluxDecoderTests : CoreTestBase
{
    [Fact]
    public void AmigaDecoderFindsTheDouble4489SyncWord()
    {
        var bits = Convert.ToString(0x4489, 2).PadLeft(16, '0') + Convert.ToString(0x4489, 2).PadLeft(16, '0');
        var intervals = new List<uint>(); var sinceTransition = 0;
        foreach (var bit in bits) { sinceTransition++; if (bit == '1') { intervals.Add((uint)(sinceTransition * 40)); sinceTransition = 0; } }
        var result = new AmigaMfmDecoder().Decode(new FluxRevolution(8_000_000, intervals));
        Assert.Contains(result.Structures, x => x.Kind == FluxStructureKind.AmigaSync);
        Assert.True(result.Confidence > 0);

        var singleWordBits = Convert.ToString(0x4489, 2).PadLeft(16, '0');
        var singleWordIntervals = BitsToIntervals(singleWordBits, 40);
        var singleWordResult = new AmigaMfmDecoder().Decode(new FluxRevolution(8_000_000, singleWordIntervals));
        Assert.DoesNotContain(singleWordResult.Structures, structure => structure.Kind == FluxStructureKind.AmigaSync);
    }

    [Fact]
    public void IsoMfmDecoderExtractsSectorIdentityAndDataCrc()
    {
        byte[] header = [0xa1, 0xa1, 0xa1, 0xfe, 0, 1, 2, 2]; var crc = TestCrc16(header);
        var data = Enumerable.Range(0, 512).Select(index => (byte)(index * 13)).ToArray(); var dataCrc = TestCrc16(new byte[] { 0xa1,0xa1,0xa1,0xfb }.Concat(data));
        var raw = Convert.ToString(0x4489, 2).PadLeft(16, '0') + Convert.ToString(0x4489, 2).PadLeft(16, '0') + Convert.ToString(0x4489, 2).PadLeft(16, '0') +
                  EncodeMfmBytes(0xfe, 0, 1, 2, 2, (byte)(crc >> 8), (byte)crc) + string.Concat(Enumerable.Repeat("10", 20)) +
                  Convert.ToString(0x44894489, 2).PadLeft(32, '0') + Convert.ToString(0x4489, 2).PadLeft(16, '0') + EncodeMfmBytes(new byte[] { 0xfb }.Concat(data).Concat([(byte)(dataCrc >> 8), (byte)dataCrc]).ToArray()) + "001";
        var intervals = BitsToIntervals(raw, 40);
        var result = new IsoMfmDecoder().Decode(new FluxRevolution(8_000_000, intervals));
        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(2, sector.Number); Assert.Equal(512, sector.SizeBytes); Assert.True(sector.IntegrityValid);
    }

    [Fact]
    public void IsoFmDecoderExtractsSingleDensitySectorData()
    {
        byte[] header = [0xfe, 3, 0, 7, 1]; var crc = TestCrc16(header);
        var data = Enumerable.Range(0, 256).Select(index => (byte)(index * 17)).ToArray(); var dataCrc = TestCrc16(new byte[] { 0xfb }.Concat(data));
        var raw = Convert.ToString(0xf57e, 2).PadLeft(16, '0') + EncodeFmBytes(3, 0, 7, 1, (byte)(crc >> 8), (byte)crc) + string.Concat(Enumerable.Repeat("10", 20)) + Convert.ToString(0xf56f, 2).PadLeft(16, '0') + EncodeFmBytes(data.Concat([(byte)(dataCrc >> 8), (byte)dataCrc]).ToArray()) + "001";
        var intervals = BitsToIntervals(raw, 40); var result = new IsoFmDecoder().Decode(new FluxRevolution(8_000_000, intervals));
        var sector = Assert.Single(result.Sectors!); Assert.Equal(7, sector.Number); Assert.Equal(256, sector.SizeBytes); Assert.True(sector.IntegrityValid);
    }

    [Theory]
    [InlineData((byte)0xfb, false)]
    [InlineData((byte)0xf8, true)]
    public void IsoMfmDecoderRecognizesDeletedDataAndCorruptedCrc(byte mark, bool corrupt)
    {
        byte[] header = [0xa1,0xa1,0xa1,0xfe,4,1,9,0]; var headerCrc = TestCrc16(header); var data = Enumerable.Range(0, 128).Select(index => (byte)(index * 19 + 1)).ToArray(); var dataCrc = TestCrc16(new byte[] { 0xa1,0xa1,0xa1,mark }.Concat(data)); if (corrupt) dataCrc++;
        var sync = string.Concat(Enumerable.Repeat(Convert.ToString(0x4489, 2).PadLeft(16, '0'), 3)); var raw = sync + EncodeMfmBytes(0xfe,4,1,9,0,(byte)(headerCrc >> 8),(byte)headerCrc) + string.Concat(Enumerable.Repeat("10", 20)) + sync + EncodeMfmBytes(new[] { mark }.Concat(data).Concat([(byte)(dataCrc >> 8),(byte)dataCrc]).ToArray()) + "001";
        var intervals = BitsToIntervals(raw, 40); var result = new IsoMfmDecoder().Decode(new FluxRevolution(8_000_000, intervals));
        Assert.Equal(!corrupt, Assert.Single(result.Sectors!).IntegrityValid); Assert.Contains(result.Structures, structure => structure.Kind == (mark == 0xf8 ? FluxStructureKind.DeletedDataAddressMark : FluxStructureKind.DataAddressMark));
    }

    [Theory]
    [InlineData((byte)0xfb, false)]
    [InlineData((byte)0xf8, true)]
    public void IsoFmDecoderRecognizesDeletedDataAndCorruptedCrc(byte mark, bool corrupt)
    {
        byte[] header = [0xfe,2,0,5,0]; var headerCrc = TestCrc16(header); var data = Enumerable.Range(0, 128).Select(index => (byte)(index * 23 + 2)).ToArray(); var dataCrc = TestCrc16(new[] { mark }.Concat(data)); if (corrupt) dataCrc++;
        var rawMark = mark == 0xfb ? 0xf56f : 0xf56a; var raw = Convert.ToString(0xf57e, 2).PadLeft(16, '0') + EncodeFmBytes(2,0,5,0,(byte)(headerCrc >> 8),(byte)headerCrc) + string.Concat(Enumerable.Repeat("10", 20)) + Convert.ToString(rawMark, 2).PadLeft(16, '0') + EncodeFmBytes(data.Concat([(byte)(dataCrc >> 8),(byte)dataCrc]).ToArray()) + "001";
        var intervals = BitsToIntervals(raw, 40); var result = new IsoFmDecoder().Decode(new FluxRevolution(8_000_000, intervals));
        Assert.Equal(!corrupt, Assert.Single(result.Sectors!).IntegrityValid); Assert.Contains(result.Structures, structure => structure.Kind == (mark == 0xf8 ? FluxStructureKind.DeletedDataAddressMark : FluxStructureKind.DataAddressMark));
    }

    [Fact]
    public void IsoDecodersReportUnavailableIntegrityWithoutDataField()
    {
        byte[] mfmHeader = [0xa1,0xa1,0xa1,0xfe,0,0,1,0]; var mfmCrc = TestCrc16(mfmHeader); var mfmRaw = string.Concat(Enumerable.Repeat(Convert.ToString(0x4489, 2).PadLeft(16, '0'), 3)) + EncodeMfmBytes(0xfe,0,0,1,0,(byte)(mfmCrc >> 8),(byte)mfmCrc) + "001";
        byte[] fmHeader = [0xfe,0,0,1,0]; var fmCrc = TestCrc16(fmHeader); var fmRaw = Convert.ToString(0xf57e, 2).PadLeft(16, '0') + EncodeFmBytes(0,0,1,0,(byte)(fmCrc >> 8),(byte)fmCrc) + "001";
        var mfmIntervals = BitsToIntervals(mfmRaw, 40); var fmIntervals = BitsToIntervals(fmRaw, 40);
        Assert.Null(Assert.Single(new IsoMfmDecoder().Decode(new FluxRevolution(8_000_000, mfmIntervals)).Sectors!).IntegrityValid);
        Assert.Null(Assert.Single(new IsoFmDecoder().Decode(new FluxRevolution(8_000_000, fmIntervals)).Sectors!).IntegrityValid);
    }

    [Fact]
    public void AppleIIGcrDecoderFindsAddressAndDataProloguesDespiteShortNoise()
    {
        var bits = Convert.ToString(0xD5AA96, 2).PadLeft(24, '0') + "0001000" + Convert.ToString(0xD5AAAD, 2).PadLeft(24, '0') + "1";
        var intervals = BitsToIntervals(bits, 40); intervals.Insert(0, 2);
        var result = new AppleIIGcrDecoder().Decode(new FluxRevolution(8_000_000, intervals));
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.AppleAddress);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.AppleData);
        Assert.Equal(40, result.EstimatedBitCellTicks);
    }

    [Fact]
    public void AdaptiveFluxClockFollowsGradualSpeedDrift()
    {
        var prologue = Convert.ToString(0xD5AA96, 2).PadLeft(24, '0');
        var bits = string.Concat(Enumerable.Repeat(prologue + "000", 10)) + "1";
        var intervals = new List<uint>(); var cells = 0; var transition = 0;
        foreach (var bit in bits)
        {
            cells++;
            if (bit != '1') continue;
            var cellTicks = 36d + Math.Min(8, transition * .25);
            intervals.Add((uint)Math.Round(cells * cellTicks)); cells = 0; transition++;
        }
        var result = new AppleIIGcrDecoder().Decode(new FluxRevolution(8_000_000, intervals));
        Assert.True(result.Structures.Count(structure => structure.Kind == FluxStructureKind.AppleAddress) >= 8);
        Assert.InRange(result.EstimatedBitCellTicks, 36, 44);
    }

    [Fact]
    public void RawFluxDecoderReportsShortNoiseAndLongDropout()
    {
        var intervals = Enumerable.Repeat(80u, 30).ToList(); intervals[8] = 5; intervals[20] = 900;
        var result = new RawFluxDecoder().Decode(new FluxRevolution(8_000_000, intervals));
        Assert.Equal(2, result.Structures.Count(structure => structure.Kind == FluxStructureKind.TimingAnomaly));
    }
}
