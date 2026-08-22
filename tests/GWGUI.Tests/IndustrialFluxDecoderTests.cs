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

public sealed class IndustrialFluxDecoderTests : CoreTestBase
{
    [Theory]
    [InlineData(512, false)]
    [InlineData(1024, true)]
    public void Aed6200pDecoderExtractsVariableSectorSizeAndHeaderCrc(int sectorSize, bool corruptCrc)
    {
        byte[] prefix = [0xc6, 12, (byte)sectorSize, 3, (byte)(sectorSize >> 8)];
        var crc = TestCrc16(prefix);
        if (corruptCrc) crc ^= 1;
        var data = Enumerable.Range(0, sectorSize).Select(index => (byte)(index * 11)).ToArray(); var dataCrc = TestCrc16(new byte[] { 0xc0 }.Concat(data));
        var raw = Convert.ToString(0x5094, 2).PadLeft(16, '0') + EncodeMfmBytesFromZero(12, (byte)sectorSize, 3, (byte)(sectorSize >> 8), (byte)(crc >> 8), (byte)crc) + "00000000" +
                  Convert.ToString(0x508a, 2).PadLeft(16, '0') + EncodeMfmBytesFromZero(data.Concat([(byte)(dataCrc >> 8), (byte)dataCrc]).ToArray()) + "001";
        var intervals = BitsToIntervals(raw, 40);

        var result = new Aed6200pMfmDecoder().Decode(new FluxRevolution(8_000_000, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(12, sector.Cylinder);
        Assert.Equal(0, sector.Head);
        Assert.Equal(3, sector.Number);
        Assert.Equal(sectorSize, sector.SizeBytes);
        Assert.Equal(!corruptCrc, sector.IntegrityValid);
        Assert.Equal(data, result.DecodedBytes.TakeLast(sectorSize));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Aed6200pDecoderValidatesVariableDataBlockCrc(bool corruptData)
    {
        const int sectorSize = 512; byte[] header = [0xc6, 12, 0, 3, 2]; var headerCrc = TestCrc16(header);
        var data = Enumerable.Range(0, sectorSize).Select(index => (byte)(index * 13)).ToArray(); var dataCrc = TestCrc16(new byte[] { 0xc3 }.Concat(data)); if (corruptData) dataCrc ^= 1;
        var raw = Convert.ToString(0x5094, 2).PadLeft(16, '0') + EncodeMfmBytesFromZero(12, 0, 3, 2, (byte)(headerCrc >> 8), (byte)headerCrc) + "00000000" +
                  Convert.ToString(0x5085, 2).PadLeft(16, '0') + EncodeMfmBytesFromZero(data.Concat([(byte)(dataCrc >> 8), (byte)dataCrc]).ToArray()) + "001"; var intervals = BitsToIntervals(raw, 40);

        var result = new Aed6200pMfmDecoder().Decode(new FluxRevolution(8_000_000, intervals));

        Assert.Equal(!corruptData, Assert.Single(result.Sectors!).IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData && structure.Description.Contains("C3"));
    }

    [Fact]
    public void Aed6200pDecoderReportsUnavailableIntegrityWithoutDataBlock()
    {
        byte[] header = [0xc6, 12, 0, 3, 2]; var crc = TestCrc16(header);
        var raw = Convert.ToString(0x5094, 2).PadLeft(16, '0') + EncodeMfmBytesFromZero(12, 0, 3, 2, (byte)(crc >> 8), (byte)crc) + "001"; var intervals = BitsToIntervals(raw, 40);

        var result = new Aed6200pMfmDecoder().Decode(new FluxRevolution(8_000_000, intervals));

        Assert.Null(Assert.Single(result.Sectors!).IntegrityValid);
    }

    [Theory]
    [InlineData(0xc0, "508A")]
    [InlineData(0xc1, "5089")]
    [InlineData(0xc2, "5084")]
    [InlineData(0xc3, "5085")]
    public void Aed6200pDecoderAcceptsEveryDataMark(byte dataMark, string encodedMark)
    {
        const int sectorSize = 128;
        byte[] header = [0xc6, 1, sectorSize, 2, 0];
        var headerCrc = TestCrc16(header);
        var data = Enumerable.Range(0, sectorSize).Select(index => (byte)index).ToArray();
        var dataCrc = TestCrc16(new[] { dataMark }.Concat(data));
        var raw = Convert.ToString(0x5094, 2).PadLeft(16, '0') + EncodeMfmBytesFromZero(1, sectorSize, 2, 0, (byte)(headerCrc >> 8), (byte)headerCrc) + "00000000" + Convert.ToString(Convert.ToUInt16(encodedMark, 16), 2).PadLeft(16, '0') + EncodeMfmBytesFromZero(data.Concat([(byte)(dataCrc >> 8), (byte)dataCrc]).ToArray()) + "001";

        var result = new Aed6200pMfmDecoder().Decode(new FluxRevolution(8_000_000, BitsToIntervals(raw, 40)));

        Assert.True(Assert.Single(result.Sectors).IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData && structure.Description.Contains(dataMark.ToString("X2")));
    }

    [Fact]
    public void Aed6200pDecoderReportsInvalidSizeAndTruncatedDataBlock()
    {
        byte[] invalidHeader = [0xc6, 1, 3, 2, 0];
        var invalidCrc = TestCrc16(invalidHeader);
        var invalidRaw = Convert.ToString(0x5094, 2).PadLeft(16, '0') + EncodeMfmBytesFromZero(1, 3, 2, 0, (byte)(invalidCrc >> 8), (byte)invalidCrc) + "001";
        var invalid = new Aed6200pMfmDecoder().Decode(new FluxRevolution(8_000_000, BitsToIntervals(invalidRaw, 40)));

        Assert.Equal(3, Assert.Single(invalid.Sectors).SizeBytes);
        Assert.Equal(0, Assert.Single(invalid.Sectors).SizeCode);

        byte[] truncatedHeader = [0xc6, 1, 128, 2, 0];
        var truncatedCrc = TestCrc16(truncatedHeader);
        var truncatedRaw = Convert.ToString(0x5094, 2).PadLeft(16, '0') + EncodeMfmBytesFromZero(1, 128, 2, 0, (byte)(truncatedCrc >> 8), (byte)truncatedCrc) + "00000000" + Convert.ToString(0x508a, 2).PadLeft(16, '0') + "001";
        var truncated = new Aed6200pMfmDecoder().Decode(new FluxRevolution(8_000_000, BitsToIntervals(truncatedRaw, 40)));

        Assert.Null(Assert.Single(truncated.Sectors).IntegrityValid);
        Assert.Contains(truncated.Structures, structure => structure.Kind == FluxStructureKind.FormatData && structure.Description.Contains("truncated"));
    }

    [Fact]
    public void Aed6200pDecoderReportsUnpairedDataMarkAndStandardConfidence()
    {
        var raw = Convert.ToString(0x5085, 2).PadLeft(16, '0') + "001";

        var result = new Aed6200pMfmDecoder().Decode(new FluxRevolution(8_000_000, BitsToIntervals(raw, 40)));

        Assert.Empty(result.Sectors);
        Assert.Single(result.Structures);
        Assert.Contains("Unpaired", result.Structures[0].Description);
        Assert.Equal(0.05, result.Confidence);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CenturionDecoderExtractsSectorIdentityAndXmodemHeaderCrc(bool corruptCrc)
    {
        byte[] identity = [17, 6];
        var crc = TestCrc16(identity, 0x1021, 0x0000);
        if (corruptCrc) crc ^= 1;
        var data = Enumerable.Range(0, 256).Select(index => (byte)(index * 5)).ToArray(); var dataCrc = TestCrc16(new byte[] { 1, 0 }.Concat(data), 0x1021, 0x0000);
        var raw = Convert.ToString(0x91224489, 2).PadLeft(32, '0') + EncodeMfmBytesFromZero(17, 6, (byte)(crc >> 8), (byte)crc) + string.Concat(Enumerable.Repeat("10", 200)) +
                  Convert.ToString(0xaaaaaaa9, 2).PadLeft(32, '0') + EncodeMfmBytesFromZero(new byte[] { 0, 1, 0 }.Concat(data).Concat([(byte)(dataCrc >> 8), (byte)dataCrc]).ToArray()) + "001";
        var intervals = BitsToIntervals(raw, 40);

        var result = new CenturionMfmDecoder().Decode(new FluxRevolution(8_000_000, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(17, sector.Cylinder);
        Assert.Equal(6, sector.Number);
        Assert.Equal(256, sector.SizeBytes);
        Assert.Equal(!corruptCrc, sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Description.Contains(corruptCrc ? "invalid" : "valid", StringComparison.Ordinal));
        Assert.Equal(data, result.DecodedBytes.TakeLast(256));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CenturionDecoderValidatesVariableDataBlockCrc(bool corruptData)
    {
        byte[] identity = [17, 6]; var headerCrc = TestCrc16(identity, 0x1021, 0x0000);
        var data = Enumerable.Range(0, 512).Select(index => (byte)(index * 3)).ToArray(); var dataCrc = TestCrc16(new byte[] { 2, 0 }.Concat(data), 0x1021, 0x0000); if (corruptData) dataCrc ^= 1;
        var raw = Convert.ToString(0x91224489, 2).PadLeft(32, '0') + EncodeMfmBytesFromZero(17, 6, (byte)(headerCrc >> 8), (byte)headerCrc) + string.Concat(Enumerable.Repeat("10", 200)) +
                  Convert.ToString(0xaaaaaaa9, 2).PadLeft(32, '0') + EncodeMfmBytesFromZero(new byte[] { 0, 2, 0 }.Concat(data).Concat([(byte)(dataCrc >> 8), (byte)dataCrc]).ToArray()) + "001"; var intervals = BitsToIntervals(raw, 40);

        var result = new CenturionMfmDecoder().Decode(new FluxRevolution(8_000_000, intervals));

        Assert.Equal(!corruptData, Assert.Single(result.Sectors!).IntegrityValid);
    }

    [Fact]
    public void CenturionDecoderReportsUnavailableIntegrityForUnsupportedKey()
    {
        byte[] identity = [17, 6]; var crc = TestCrc16(identity, 0x1021, 0x0000);
        var raw = Convert.ToString(0x91224489, 2).PadLeft(32, '0') + EncodeMfmBytesFromZero(17, 6, (byte)(crc >> 8), (byte)crc) + string.Concat(Enumerable.Repeat("10", 200)) +
                  Convert.ToString(0xaaaaaaa9, 2).PadLeft(32, '0') + EncodeMfmBytesFromZero(7, 1, 0) + "001"; var intervals = BitsToIntervals(raw, 40);

        var result = new CenturionMfmDecoder().Decode(new FluxRevolution(8_000_000, intervals));

        Assert.Null(Assert.Single(result.Sectors!).IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Description.Contains("unsupported key 7"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void QdMo5DecoderExtractsWideSectorNumberAndDataChecksum(bool corruptChecksum)
    {
        var data = Enumerable.Range(0, 128).Select(index => (byte)(index * 11)).ToArray();
        var checksum = (byte)(0x5a + data.Sum(value => value));
        if (corruptChecksum) checksum++;
        static string RawMark(string hexadecimal) => string.Concat(Convert.FromHexString(hexadecimal).Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        var headerMark = RawMark("A914A914A914A914A9144491");
        var dataMark = RawMark("A914A914A914A914A9149144");
        var headerTail = new byte[] { 0x12, 0x34 }.Concat(new byte[13]).ToArray();
        var raw = headerMark + EncodeMfmBytesFromZero(headerTail) + string.Concat(Enumerable.Repeat("10", 20)) + dataMark + EncodeMfmBytesFromZero(data.Append(checksum).ToArray()) + "001";
        var intervals = BitsToIntervals(raw, 40);

        var result = new QdMo5MfmDecoder().Decode(new FluxRevolution(8_000_000, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(0x1234, sector.Number);
        Assert.Equal(128, sector.SizeBytes);
        Assert.Equal(!corruptChecksum, sector.IntegrityValid);
        Assert.Equal(SectorIntegrityKind.Checksum, sector.IntegrityKind);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData && structure.Description.Contains(corruptChecksum ? "invalid" : "valid", StringComparison.Ordinal));
        Assert.Equal(data, result.DecodedBytes.TakeLast(128));
    }

    [Fact]
    public void QdMo5DecoderReportsUnavailableIntegrityForTruncatedData()
    {
        static string RawMark(string hexadecimal) => string.Concat(Convert.FromHexString(hexadecimal).Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        var headerMark = RawMark("A914A914A914A914A9144491"); var dataMark = RawMark("A914A914A914A914A9149144");
        var headerTail = new byte[] { 0x12, 0x34 }.Concat(new byte[13]).ToArray();
        var raw = headerMark + EncodeMfmBytesFromZero(headerTail) + string.Concat(Enumerable.Repeat("10", 20)) + dataMark + EncodeMfmBytesFromZero(Enumerable.Range(0, 12).Select(index => (byte)index).ToArray()) + "001";
        var intervals = BitsToIntervals(raw, 40);

        var result = new QdMo5MfmDecoder().Decode(new FluxRevolution(8_000_000, intervals));

        Assert.Null(Assert.Single(result.Sectors!).IntegrityValid);
    }

    [Fact]
    public void QdMo5DecoderReportsUnavailableIntegrityWhenDataBlockIsMissing()
    {
        static string RawMark(string hexadecimal) => string.Concat(Convert.FromHexString(hexadecimal).Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        var headerTail = new byte[] { 0x01, 0x02 }.Concat(new byte[13]).ToArray();
        var raw = RawMark("A914A914A914A914A9144491") + EncodeMfmBytesFromZero(headerTail) + "001";
        var intervals = BitsToIntervals(raw, 40);

        var result = new QdMo5MfmDecoder().Decode(new FluxRevolution(8_000_000, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(0x0102, sector.Number);
        Assert.Null(sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Description.Contains("unavailable", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EmuFmDecoderExtractsTrackIdentityAndValidatesLargeDataCrc(bool corruptDataCrc)
    {
        static byte Reverse(byte value) { byte result = 0; for (var bit = 0; bit < 8; bit++) result = (byte)((result << 1) | ((value >> bit) & 1)); return result; }
        static string EncodeEmuFm(IEnumerable<byte> values) => string.Concat(values.SelectMany(value => Enumerable.Range(0, 8).Select(bit => "01" + ((((value >> (7 - bit)) & 1) != 0) ? "01" : "00"))));
        byte track = 25, rawTrack = Reverse(track);
        var headerCrc = TestCrc16([rawTrack], 0x8005, 0x0000);
        var data = Enumerable.Range(0, 0xe00).Select(index => (byte)(index * 13)).ToArray();
        var dataCrc = TestCrc16(data, 0x8005, 0x0000);
        if (corruptDataCrc) dataCrc ^= 1;
        var marker = EncodeEmuFm([Reverse(0xfa), Reverse(0x96)]);
        var raw = marker + EncodeEmuFm([rawTrack, (byte)(headerCrc >> 8), (byte)headerCrc]) + new string('1', 64)
            + marker + EncodeEmuFm(data.Concat([(byte)(dataCrc >> 8), (byte)dataCrc])) + "1";
        var intervals = BitsToIntervals(raw, 40);

        var result = new EmuFmDecoder().Decode(new FluxRevolution(8_000_000, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(12, sector.Cylinder);
        Assert.Equal(1, sector.Head);
        Assert.Equal(1, sector.Number);
        Assert.Equal(0xe00, sector.SizeBytes);
        Assert.Equal(!corruptDataCrc, sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData && structure.Description.Contains(corruptDataCrc ? "invalid" : "valid", StringComparison.Ordinal));
        Assert.Equal(data, result.DecodedBytes.TakeLast(0xe00));
    }

    [Fact]
    public void EmuFmDecoderReportsUnavailableDataIntegrityWhenOnlyHeaderExists()
    {
        static byte Reverse(byte value) { byte result = 0; for (var bit = 0; bit < 8; bit++) result = (byte)((result << 1) | ((value >> bit) & 1)); return result; }
        static string EncodeEmuFm(IEnumerable<byte> values) => string.Concat(values.SelectMany(value => Enumerable.Range(0, 8).Select(bit => "01" + ((((value >> (7 - bit)) & 1) != 0) ? "01" : "00"))));
        var rawTrack = Reverse(8); var headerCrc = TestCrc16([rawTrack], 0x8005, 0x0000);
        var marker = EncodeEmuFm([Reverse(0xfa), Reverse(0x96)]);
        var raw = marker + EncodeEmuFm([rawTrack, (byte)(headerCrc >> 8), (byte)headerCrc]) + "1";
        var intervals = BitsToIntervals(raw, 40);

        var result = new EmuFmDecoder().Decode(new FluxRevolution(8_000_000, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(4, sector.Cylinder);
        Assert.Equal(0, sector.Head);
        Assert.Null(sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Description.Contains("unavailable", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(0xf8, false)]
    [InlineData(0xf9, false)]
    [InlineData(0xfa, false)]
    [InlineData(0xfb, true)]
    public void TycomFmDecoderExtractsIdentityDataMarkAndCrc(byte dataMark, bool corruptDataCrc)
    {
        static string EncodeTycomFm(IEnumerable<byte> values) => string.Concat(values.SelectMany(value => Enumerable.Range(0, 8).Select(bit => "01" + ((((value >> (7 - bit)) & 1) != 0) ? "01" : "00"))));
        static string RawMark(string hexadecimal) => string.Concat(Convert.FromHexString(hexadecimal).Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        const byte cylinder = 31, sectorNumber = 7;
        var headerCrc = TestCrc16([0xfe, cylinder, sectorNumber], 0x1021, 0xffff);
        var data = Enumerable.Range(0, 128).Select(index => (byte)(index * 19)).ToArray();
        var dataCrc = TestCrc16(new byte[] { dataMark }.Concat(data), 0x1021, 0xffff);
        if (corruptDataCrc) dataCrc ^= 1;
        var dataPattern = dataMark switch { 0xf8 => "55111444", 0xf9 => "55111445", 0xfa => "55111454", _ => "55111455" };
        var raw = RawMark("55111554") + EncodeTycomFm([cylinder, sectorNumber, (byte)(headerCrc >> 8), (byte)headerCrc]) + new string('1', 64)
            + RawMark(dataPattern) + EncodeTycomFm(data.Concat([(byte)(dataCrc >> 8), (byte)dataCrc])) + "1";
        var intervals = BitsToIntervals(raw, 40);

        var result = new TycomFmDecoder().Decode(new FluxRevolution(8_000_000, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(cylinder, sector.Cylinder);
        Assert.Equal(sectorNumber, sector.Number);
        Assert.Equal(128, sector.SizeBytes);
        Assert.Equal(!corruptDataCrc, sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData && structure.Description.Contains(dataMark.ToString("X2"), StringComparison.Ordinal));
        Assert.Equal(data, result.DecodedBytes.TakeLast(128));
    }

    [Fact]
    public void TycomFmDecoderReportsUnavailableDataIntegrityWhenOnlyHeaderExists()
    {
        static string EncodeTycomFm(IEnumerable<byte> values) => string.Concat(values.SelectMany(value => Enumerable.Range(0, 8).Select(bit => "01" + ((((value >> (7 - bit)) & 1) != 0) ? "01" : "00"))));
        static string RawMark(string hexadecimal) => string.Concat(Convert.FromHexString(hexadecimal).Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        var headerCrc = TestCrc16([0xfe, 4, 2], 0x1021, 0xffff);
        var raw = RawMark("55111554") + EncodeTycomFm([4, 2, (byte)(headerCrc >> 8), (byte)headerCrc]) + "1";
        var intervals = BitsToIntervals(raw, 40);

        var result = new TycomFmDecoder().Decode(new FluxRevolution(8_000_000, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(4, sector.Cylinder);
        Assert.Equal(2, sector.Number);
        Assert.Null(sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Description.Contains("unavailable", StringComparison.Ordinal));
    }

    [Fact]
    public void TycomFmDecoderRejectsCorruptedHeaderCrc()
    {
        static string EncodeTycomFm(IEnumerable<byte> values) => string.Concat(values.SelectMany(value => Enumerable.Range(0, 8).Select(bit => "01" + ((((value >> (7 - bit)) & 1) != 0) ? "01" : "00"))));
        static string RawMark(string hexadecimal) => string.Concat(Convert.FromHexString(hexadecimal).Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        var headerCrc = (ushort)(TestCrc16([0xfe, 9, 3], 0x1021, 0xffff) ^ 1);
        var raw = RawMark("55111554") + EncodeTycomFm([9, 3, (byte)(headerCrc >> 8), (byte)headerCrc]) + "1";
        var intervals = BitsToIntervals(raw, 40);

        var result = new TycomFmDecoder().Decode(new FluxRevolution(8_000_000, intervals));

        Assert.Empty(result.Sectors!);
        Assert.Contains(result.Structures, structure => structure.Description.Contains("header CRC invalid", StringComparison.Ordinal));
    }
}
