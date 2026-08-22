using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.Exploration.Scp;
using GWGUI.MediaEngine.Flux;
using System.IO;
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

public sealed class ClassicFluxDecoderTests : CoreTestBase
{
    [Fact]
    public void CommodoreGcrDecoderFindsSyncAndHeaderBlock()
    {
        const string headerByte08 = "01010" + "01001";
        var intervals = BitsToIntervals("111111111111" + headerByte08 + "1", 40);
        var result = new CommodoreGcrDecoder().Decode(new FluxRevolution(8_000_000, intervals));
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.CommodoreSync);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.CommodoreHeader);
        Assert.Contains((byte)0x08, result.DecodedBytes);
    }

    [Fact]
    public void DecoderRegistryExposesGcrFamilies()
    {
        var ids = new FluxDecoderRegistry().Decoders.Select(decoder => decoder.Id).ToHashSet();
        Assert.Contains("apple2.gcr", ids); Assert.Contains("commodore.gcr", ids); Assert.Contains("northstar.mfm", ids); Assert.Contains("heathkit.fm", ids);
    }

    [Fact]
    public void NorthstarDecoderRecognizesHardSectorBlockMark()
    {
        var raw = string.Concat(Enumerable.Repeat("10", 60)) + EncodeMfmBytesFromZero(0, 0, 0, 0, 0, 0, 0, 0xfb) + "001";
        var intervals = BitsToIntervals(raw, 40);
        var result = new NorthstarMfmDecoder().Decode(new FluxRevolution(8_000_000, intervals));
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatHeader);
    }

    [Fact]
    public void NorthstarDecoderExtractsSectorIdentityAndRotatingChecksum()
    {
        var data = Enumerable.Range(0, 512).Select(index => (byte)(index * 17)).ToArray();
        byte checksum = 0;
        foreach (var value in data) { checksum ^= value; checksum = (byte)((checksum >> 7) | (checksum << 1)); }
        var block = Enumerable.Repeat((byte)0, 7).Concat([(byte)0xfb, (byte)0x37]).Concat(data).Append(checksum).ToArray();
        var raw = string.Concat(Enumerable.Repeat("10", 60)) + EncodeMfmBytesFromZero(block) + "001";
        var intervals = BitsToIntervals(raw, 40);

        var result = new NorthstarMfmDecoder().Decode(new FluxRevolution(8_000_000, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(3, sector.Cylinder);
        Assert.Equal(7, sector.Number);
        Assert.Equal(512, sector.SizeBytes);
        Assert.True(sector.IntegrityValid);
        Assert.Equal(SectorIntegrityKind.Checksum, sector.IntegrityKind);
        Assert.Equal(data, result.DecodedBytes.TakeLast(512));
    }

    [Fact]
    public void NorthstarDecoderReportsUnavailableIntegrityForTruncatedBlock()
    {
        var partialData = Enumerable.Range(0, 32).Select(index => (byte)index).ToArray();
        var block = Enumerable.Repeat((byte)0, 7).Concat([(byte)0xfb, (byte)0x37]).Concat(partialData).ToArray();
        var raw = string.Concat(Enumerable.Repeat("10", 60)) + EncodeMfmBytesFromZero(block) + "001"; var intervals = BitsToIntervals(raw, 40);

        var result = new NorthstarMfmDecoder().Decode(new FluxRevolution(8_000_000, intervals));

        var sector = Assert.Single(result.Sectors!); Assert.Equal(3, sector.Cylinder); Assert.Equal(7, sector.Number); Assert.Null(sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Description.Contains("unavailable"));
    }

    [Fact]
    public void HeathkitDecoderRecognizesBitReversedFdHeaderMark()
    {
        var raw = EncodeFmBytes(0, 0, 0, 0xbf) + "001"; var intervals = BitsToIntervals(raw, 40);
        var result = new HeathkitFmDecoder().Decode(new FluxRevolution(8_000_000, intervals));
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatHeader);
    }

    [Fact]
    public void HeathkitDecoderExtractsBitReversedHeaderAndChecksum()
    {
        const byte volume = 2, cylinder = 12, sectorNumber = 5;
        byte checksum = 0;
        foreach (var value in new[] { volume, cylinder, sectorNumber }) { checksum ^= value; checksum = (byte)((checksum >> 7) | (checksum << 1)); }
        static byte Reverse(byte value) { byte result = 0; for (var bit = 0; bit < 8; bit++) result = (byte)((result << 1) | ((value >> bit) & 1)); return result; }
        var data = Enumerable.Range(0, 256).Select(index => (byte)(index * 9)).ToArray(); byte dataChecksum = 0;
        foreach (var value in data) { dataChecksum ^= value; dataChecksum = (byte)((dataChecksum >> 7) | (dataChecksum << 1)); }
        var raw = EncodeFmBytes(0, 0, 0, 0xbf, Reverse(volume), Reverse(cylinder), Reverse(sectorNumber), Reverse(checksum)) + string.Concat(Enumerable.Repeat("10", 20)) +
                  EncodeFmBytes(new byte[] { 0, 0, 0, 0xbf }.Concat(data.Select(Reverse)).Append(Reverse(dataChecksum)).ToArray()) + "001";
        var intervals = BitsToIntervals(raw, 40);

        var result = new HeathkitFmDecoder().Decode(new FluxRevolution(8_000_000, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(cylinder, sector.Cylinder);
        Assert.Equal(sectorNumber, sector.Number);
        Assert.Equal(256, sector.SizeBytes);
        Assert.True(sector.IntegrityValid);
        Assert.Equal(SectorIntegrityKind.Checksum, sector.IntegrityKind);
        Assert.Equal(data, result.DecodedBytes.TakeLast(256));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void HeathkitDecoderValidatesDataChecksum(bool corruptData)
    {
        const byte volume = 2, cylinder = 12, sectorNumber = 5;
        static byte Reverse(byte value) { byte result = 0; for (var bit = 0; bit < 8; bit++) result = (byte)((result << 1) | ((value >> bit) & 1)); return result; }
        byte headerChecksum = 0; foreach (var value in new[] { volume, cylinder, sectorNumber }) { headerChecksum ^= value; headerChecksum = (byte)((headerChecksum >> 7) | (headerChecksum << 1)); }
        var data = Enumerable.Range(0, 256).Select(index => (byte)(index * 7)).ToArray(); byte dataChecksum = 0; foreach (var value in data) { dataChecksum ^= value; dataChecksum = (byte)((dataChecksum >> 7) | (dataChecksum << 1)); } if (corruptData) dataChecksum++;
        var raw = EncodeFmBytes(0, 0, 0, 0xbf, Reverse(volume), Reverse(cylinder), Reverse(sectorNumber), Reverse(headerChecksum)) + string.Concat(Enumerable.Repeat("10", 20)) +
                  EncodeFmBytes(new byte[] { 0, 0, 0, 0xbf }.Concat(data.Select(Reverse)).Append(Reverse(dataChecksum)).ToArray()) + "001"; var intervals = BitsToIntervals(raw, 40);

        var result = new HeathkitFmDecoder().Decode(new FluxRevolution(8_000_000, intervals));

        Assert.Equal(!corruptData, Assert.Single(result.Sectors!).IntegrityValid);
    }

    [Fact]
    public void HeathkitDecoderReportsUnavailableIntegrityWithoutDataBlock()
    {
        const byte volume = 2, cylinder = 12, sectorNumber = 5;
        static byte Reverse(byte value) { byte result = 0; for (var bit = 0; bit < 8; bit++) result = (byte)((result << 1) | ((value >> bit) & 1)); return result; }
        byte checksum = 0; foreach (var value in new[] { volume, cylinder, sectorNumber }) { checksum ^= value; checksum = (byte)((checksum >> 7) | (checksum << 1)); }
        var raw = EncodeFmBytes(0, 0, 0, 0xbf, Reverse(volume), Reverse(cylinder), Reverse(sectorNumber), Reverse(checksum)) + "001"; var intervals = BitsToIntervals(raw, 40);

        var result = new HeathkitFmDecoder().Decode(new FluxRevolution(8_000_000, intervals));

        Assert.Null(Assert.Single(result.Sectors!).IntegrityValid);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MicralNDecoderExtractsIdentityDataAndCarryChecksum(bool corruptChecksum)
    {
        const byte cylinder = 17, sectorNumber = 29;
        var data = Enumerable.Range(0, 128).Select(index => (byte)(index * 11 + 7)).ToArray();
        static byte Update(byte checksum, byte value)
        {
            var carrySource = ((value ^ checksum) ^ 0xff) & ((value + checksum) ^ value);
            return (byte)(checksum + value + ((carrySource & 0x80) != 0 ? 1 : 0));
        }
        byte checksum = 0; foreach (var value in data) checksum = Update(checksum, value);
        if (corruptChecksum) checksum++;
        var raw = EncodeFmBytes(new byte[] { 0, 0, 0, 0xff, sectorNumber, cylinder }.Concat(data).Append(checksum).ToArray()) + "001";
        var intervals = BitsToIntervals(raw, 40);

        var result = new MicralNFmDecoder().Decode(new FluxRevolution(8_000_000, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(cylinder, sector.Cylinder);
        Assert.Equal(0, sector.Head);
        Assert.Equal(sectorNumber, sector.Number);
        Assert.Equal(128, sector.SizeBytes);
        Assert.Equal(!corruptChecksum, sector.IntegrityValid);
        Assert.Equal(SectorIntegrityKind.Checksum, sector.IntegrityKind);
        Assert.Equal(data, result.DecodedBytes);
        Assert.Contains(result.Structures, structure => structure.Description.Contains(corruptChecksum ? "invalid" : "valid", StringComparison.Ordinal));
    }

    [Fact]
    public void MicralNDecoderReportsUnavailableIntegrityForTruncatedBlock()
    {
        var raw = EncodeFmBytes(0, 0, 0, 0xff, 4, 2, 1, 2, 3) + "001";
        var intervals = BitsToIntervals(raw, 40);

        var result = new MicralNFmDecoder().Decode(new FluxRevolution(8_000_000, intervals));

        Assert.Empty(result.Sectors!);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatHeader && structure.Description.Contains("unavailable", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MembrainDecoderExtractsPackedIdentityAndNativeCrc(bool corruptCrc)
    {
        byte[] prefix = [0xa1, 0xfe, 0x04, 0xb9];
        var crc = TestCrc16(prefix, 0x8005, 0x0000);
        if (corruptCrc) crc ^= 1;
        var data = Enumerable.Range(0, 512).Select(index => (byte)(index * 7)).ToArray();
        var dataCrc = TestCrc16(new byte[] { 0xa1, 0xf8 }.Concat(data), 0x8005, 0x0000);
        var raw = Convert.ToString(0x44895554, 2).PadLeft(32, '0') + EncodeMfmBytesFromZero(0x04, 0xb9, (byte)(crc >> 8), (byte)crc) + "00000000" +
                  Convert.ToString(0x4489554a, 2).PadLeft(32, '0') + EncodeMfmBytesFromZero(data.Concat([(byte)(dataCrc >> 8), (byte)dataCrc]).ToArray()) + "001";
        var intervals = BitsToIntervals(raw, 40);

        var result = new MembrainMfmDecoder().Decode(new FluxRevolution(8_000_000, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(37, sector.Cylinder);
        Assert.Equal(1, sector.Head);
        Assert.Equal(9, sector.Number);
        Assert.Equal(512, sector.SizeBytes);
        Assert.Equal(!corruptCrc, sector.IntegrityValid);
        Assert.Equal(SectorIntegrityKind.Crc, sector.IntegrityKind);
        Assert.Equal(data, result.DecodedBytes.TakeLast(512));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MembrainDecoderValidatesDataCrc(bool corruptData)
    {
        byte[] header = [0xa1, 0xfe, 0x04, 0xb9]; var headerCrc = TestCrc16(header, 0x8005, 0x0000);
        var data = Enumerable.Range(0, 512).Select(index => (byte)(255 - index)).ToArray(); var dataCrc = TestCrc16(new byte[] { 0xa1, 0xf8 }.Concat(data), 0x8005, 0x0000);
        if (corruptData) dataCrc ^= 1;
        var raw = Convert.ToString(0x44895554, 2).PadLeft(32, '0') + EncodeMfmBytesFromZero(0x04, 0xb9, (byte)(headerCrc >> 8), (byte)headerCrc) + "00000000" +
                  Convert.ToString(0x4489554a, 2).PadLeft(32, '0') + EncodeMfmBytesFromZero(data.Concat([(byte)(dataCrc >> 8), (byte)dataCrc]).ToArray()) + "001";
        var intervals = BitsToIntervals(raw, 40);

        var result = new MembrainMfmDecoder().Decode(new FluxRevolution(8_000_000, intervals));

        Assert.Equal(!corruptData, Assert.Single(result.Sectors!).IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData && structure.Description.Contains(corruptData ? "invalid" : "valid"));
    }

    [Fact]
    public void MembrainDecoderReportsUnavailableIntegrityWithoutDataBlock()
    {
        byte[] header = [0xa1, 0xfe, 0x04, 0xb9]; var crc = TestCrc16(header, 0x8005, 0x0000);
        var raw = Convert.ToString(0x44895554, 2).PadLeft(32, '0') + EncodeMfmBytesFromZero(0x04, 0xb9, (byte)(crc >> 8), (byte)crc) + "001"; var intervals = BitsToIntervals(raw, 40);

        var result = new MembrainMfmDecoder().Decode(new FluxRevolution(8_000_000, intervals));

        Assert.Null(Assert.Single(result.Sectors!).IntegrityValid);
    }
}
