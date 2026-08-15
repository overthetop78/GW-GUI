using System.IO;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.Domain.Commands;
using GWGUI.Domain.Profiles;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.Domain.Formats;
using GWGUI.Domain.Naming;
using GWGUI.Domain.Hardware;
using GWGUI.Domain.Conversion;
using GWGUI.Domain.Read;
using GWGUI.Domain.Write;
using GWGUI.Domain.Maintenance;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Exploration;
using GWGUI.Infrastructure.Processes;
using GWGUI.Infrastructure.Settings;
using GWGUI.Infrastructure.Hardware;
using GWGUI.Domain.Settings;
using GWGUI.App;
using GWGUI.App.Controls;
using GWGUI.App.ViewModels;
using GWGUI.App.Services;
using GWGUI.App.Rendering;
using GWGUI.App.Localization;
using SkiaSharp;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Threading;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace GWGUI.Tests;

public sealed class SpecializedFluxDecoderTests : CoreTestBase
{
    [Theory]
    [InlineData(0xf8, false)]
    [InlineData(0xf9, false)]
    [InlineData(0xfa, false)]
    [InlineData(0xfb, false)]
    [InlineData(0xfc, false)]
    [InlineData(0xfd, true)]
    public void DecRx02DecoderExtractsAllDataMarksAndFmOrM2FmCrc(byte dataMark, bool corruptDataCrc)
    {
        static string EncodeRxFm(IEnumerable<byte> values) => string.Concat(values.SelectMany(value => Enumerable.Range(0, 8).Select(bit => "01" + ((((value >> (7 - bit)) & 1) != 0) ? "01" : "00"))));
        static string RawMark(string hexadecimal) => string.Concat(Convert.FromHexString(hexadecimal).Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        static string EncodeM2Fm(byte[] values)
        {
            var bits = EncodeMfmBytesFromZero(values).ToCharArray(); const string normal = "00101010100", encoded = "01000100010"; var replacements = 0;
            for (var offset = 1; offset + normal.Length <= bits.Length; offset += 2)
            {
                var matches = true; for (var index = 0; index < normal.Length; index++) if (bits[offset + index] != normal[index]) { matches = false; break; }
                if (!matches) continue; for (var index = 0; index < encoded.Length; index++) bits[offset + index] = encoded[index]; replacements++; offset += normal.Length - 3;
            }
            Assert.True(replacements > 0, "The M²FM vector must exercise the DEC 11-bit substitution rule."); return new string(bits);
        }
        const byte cylinder = 22, head = 1, sectorNumber = 9;
        var sizeCode = dataMark is 0xf9 or 0xfd ? (byte)1 : (byte)0;
        var headerCrc = TestCrc16([0xfe, cylinder, head, sectorNumber, sizeCode], 0x1021, 0xffff);
        var m2fm = dataMark is 0xf9 or 0xfd; var size = m2fm ? 256 : 128;
        var data = Enumerable.Range(0, size).Select(index => (byte)(index * 23)).ToArray();
        var dataCrc = TestCrc16(new byte[] { dataMark }.Concat(data), 0x1021, 0xffff); if (corruptDataCrc) dataCrc ^= 1;
        var markPattern = dataMark switch { 0xf8 => "55111444", 0xf9 => "55111445", 0xfa => "55111454", 0xfb => "55111455", 0xfc => "55111544", _ => "55111545" };
        var payload = data.Concat([(byte)(dataCrc >> 8), (byte)dataCrc]).ToArray();
        var encodedPayload = m2fm ? "0" + EncodeM2Fm(payload) : EncodeRxFm(payload);
        var raw = RawMark("55111554") + EncodeRxFm([cylinder, head, sectorNumber, sizeCode, (byte)(headerCrc >> 8), (byte)headerCrc]) + new string('1', 64)
            + RawMark(markPattern) + encodedPayload + "1";
        var intervals = BitsToIntervals(raw, 40);

        var result = new DecRx02Decoder().Decode(new FluxRevolution(8_000_000, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(cylinder, sector.Cylinder); Assert.Equal(head, sector.Head); Assert.Equal(sectorNumber, sector.Number);
        Assert.Equal(size, sector.SizeBytes); Assert.Equal(!corruptDataCrc, sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData && structure.Description.Contains(dataMark.ToString("X2"), StringComparison.Ordinal) && structure.Description.Contains(m2fm ? "M²FM" : "FM", StringComparison.Ordinal));
        Assert.Equal(data, result.DecodedBytes.TakeLast(size));
    }

    [Fact]
    public void DecRx02DecoderReportsUnavailableDataAndRejectsBadHeaderCrc()
    {
        static string EncodeRxFm(IEnumerable<byte> values) => string.Concat(values.SelectMany(value => Enumerable.Range(0, 8).Select(bit => "01" + ((((value >> (7 - bit)) & 1) != 0) ? "01" : "00"))));
        static string RawMark(string hexadecimal) => string.Concat(Convert.FromHexString(hexadecimal).Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        var validCrc = TestCrc16([0xfe, 5, 0, 2, 0], 0x1021, 0xffff);
        var validBits = RawMark("55111554") + EncodeRxFm([5, 0, 2, 0, (byte)(validCrc >> 8), (byte)validCrc]) + "1";
        var invalidCrc = (ushort)(validCrc ^ 1);
        var invalidBits = RawMark("55111554") + EncodeRxFm([5, 0, 2, 0, (byte)(invalidCrc >> 8), (byte)invalidCrc]) + "1";

        var validIntervals = BitsToIntervals(validBits, 40); var invalidIntervals = BitsToIntervals(invalidBits, 40);
        var missing = new DecRx02Decoder().Decode(new FluxRevolution(8_000_000, validIntervals));
        var corrupt = new DecRx02Decoder().Decode(new FluxRevolution(8_000_000, invalidIntervals));

        Assert.Null(Assert.Single(missing.Sectors!).IntegrityValid);
        Assert.Contains(missing.Structures, structure => structure.Description.Contains("unavailable", StringComparison.Ordinal));
        Assert.Empty(corrupt.Sectors!);
        Assert.Contains(corrupt.Structures, structure => structure.Description.Contains("header CRC invalid", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ArburgDecoderValidatesFullFmDataTrackChecksum(bool corruptChecksum)
    {
        static byte Reverse(byte value) { byte result = 0; for (var bit = 0; bit < 8; bit++) result = (byte)((result << 1) | ((value >> bit) & 1)); return result; }
        static string EncodeArburgFm(IEnumerable<byte> values) => string.Concat(values.SelectMany(value => Enumerable.Range(0, 8).Select(bit => { var reversed = Reverse(value); return "01" + ((((reversed >> (7 - bit)) & 1) != 0) ? "01" : "00"); })));
        static string RawMark(string hexadecimal) => string.Concat(Convert.FromHexString(hexadecimal).Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        var data = Enumerable.Range(0, 0x9fe).Select(index => (byte)(index * 29)).ToArray(); ushort checksum = 0; foreach (var value in data) checksum += value;
        if (corruptChecksum) checksum++;
        var block = data.Concat([(byte)checksum, (byte)(checksum >> 8)]).ToArray();
        var raw = RawMark("4444444455555555") + EncodeArburgFm(block) + "1"; var intervals = BitsToIntervals(raw, 40);

        var result = new ArburgDecoder().Decode(new FluxRevolution(8_000_000, intervals));

        var sector = Assert.Single(result.Sectors!); Assert.Equal(0xa00, sector.SizeBytes); Assert.Equal(!corruptChecksum, sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData && structure.Description.Contains(corruptChecksum ? "invalid" : "valid", StringComparison.Ordinal));
        Assert.Equal(data, result.DecodedBytes.TakeLast(0x9fe));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ArburgDecoderValidatesFullVariableLengthSystemTrackChecksum(bool corruptChecksum)
    {
        static string EncodeSystem(IEnumerable<byte> values) => string.Concat(values.SelectMany(value => Enumerable.Range(0, 8).Select(bit => ((value >> bit) & 1) != 0 ? "001" : "01")));
        static string RawMark(string hexadecimal) => string.Concat(Convert.FromHexString(hexadecimal).Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        var data = Enumerable.Range(0, 0xefe).Select(index => (byte)(index * 31)).ToArray(); ushort checksum = 0; foreach (var value in data) checksum += value;
        if (corruptChecksum) checksum++;
        var block = data.Concat([(byte)checksum, (byte)(checksum >> 8)]).ToArray();
        var raw = RawMark("5555555555249249") + EncodeSystem(block) + "1"; var intervals = BitsToIntervals(raw, 40);

        var result = new ArburgDecoder().Decode(new FluxRevolution(8_000_000, intervals));

        var sector = Assert.Single(result.Sectors!); Assert.Equal(0xf00, sector.SizeBytes); Assert.Equal(!corruptChecksum, sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatHeader && structure.Description.Contains(corruptChecksum ? "invalid" : "valid", StringComparison.Ordinal));
        Assert.Equal(data, result.DecodedBytes.TakeLast(0xefe));
    }

    [Fact]
    public void ArburgDecoderReportsUnavailableIntegrityForTruncatedTrackBlocks()
    {
        static string RawMark(string hexadecimal) => string.Concat(Convert.FromHexString(hexadecimal).Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        static FluxDecodeResult Decode(string marker)
        {
            var intervals = BitsToIntervals(RawMark(marker) + "1", 40); return new ArburgDecoder().Decode(new FluxRevolution(8_000_000, intervals));
        }
        var data = Decode("4444444455555555"); var system = Decode("5555555555249249");
        Assert.Null(Assert.Single(data.Sectors!).IntegrityValid); Assert.Null(Assert.Single(system.Sectors!).IntegrityValid);
        Assert.All(data.Structures.Concat(system.Structures), structure => Assert.Contains("unavailable", structure.Description, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Victor9kDecoderExtractsIdentityAndValidatesHeaderAndDataChecksums(bool corruptHeader, bool corruptData)
    {
        static string EncodeGcr(IEnumerable<byte> values)
        {
            int[] table = [0x0a,0x0b,0x12,0x13,0x0e,0x0f,0x16,0x17,0x09,0x19,0x1a,0x1b,0x0d,0x1d,0x1e,0x15];
            return string.Concat(values.SelectMany(value => new[] { value >> 4, value & 15 }).Select(nibble => Convert.ToString(table[nibble], 2).PadLeft(5, '0')));
        }
        static string Block(string markerHex, IReadOnlyList<byte> values)
        {
            var marker = string.Concat(Convert.FromHexString(markerHex).Select(value => Convert.ToString(value, 2).PadLeft(8, '0'))); var bits = marker.ToList(); var encoded = EncodeGcr(values);
            while (bits.Count < 49 + encoded.Length * 2) bits.Add('0');
            for (var index = 0; index < encoded.Length; index++)
            {
                var position = 49 + index * 2;
                if (position < marker.Length) Assert.Equal(marker[position], encoded[index]);
                bits[position] = encoded[index];
            }
            return new(bits.ToArray());
        }
        const byte cylinder = 17; const byte sector = 6;
        var headerChecksum = (byte)(cylinder + sector + (corruptHeader ? 1 : 0));
        byte[] header = [0x06, cylinder, sector, headerChecksum, 0xa1, 0x1a];
        var data = Enumerable.Range(0, 512).Select(index => (byte)(index * 29 + 7)).ToArray(); ushort checksum = 0; foreach (var value in data) checksum += value;
        if (corruptData) checksum++;
        var dataBlock = new byte[] { 0x00 }.Concat(data).Concat([(byte)checksum, (byte)(checksum >> 8)]).ToArray();
        var raw = Block("5555555555551111", header) + new string('0', 20) + Block("5555555555551104", dataBlock) + "1";
        var intervals = BitsToIntervals(raw, 40);

        var result = new Victor9kGcrDecoder().Decode(new FluxRevolution(8_000_000, intervals));

        var decoded = Assert.Single(result.Sectors!); Assert.Equal(cylinder, decoded.Cylinder); Assert.Equal(sector, decoded.Number); Assert.Equal(512, decoded.SizeBytes);
        Assert.Equal(!corruptHeader && !corruptData, decoded.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData && structure.Description.Contains(corruptData ? "invalid" : "valid", StringComparison.Ordinal));
        Assert.Equal(data, result.DecodedBytes.TakeLast(512));
    }

    [Fact]
    public void Victor9kDecoderReportsUnavailableIntegrityForTruncatedSector()
    {
        var marker = string.Concat(Convert.FromHexString("5555555555551111").Select(value => Convert.ToString(value, 2).PadLeft(8, '0'))); var intervals = BitsToIntervals(marker + "1", 40);
        var result = new Victor9kGcrDecoder().Decode(new FluxRevolution(8_000_000, intervals));
        Assert.Null(Assert.Single(result.Sectors!).IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatHeader && structure.Description.Contains("unavailable", StringComparison.Ordinal));
    }
}
