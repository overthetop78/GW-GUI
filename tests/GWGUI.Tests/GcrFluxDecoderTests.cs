using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.Exploration.Scp;
using GWGUI.MediaEngine.Flux;
using System.IO;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Containers.Scp;
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

public sealed class GcrFluxDecoderTests : CoreTestBase
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void AppleIIGcrDecoderExtractsAddressAndDecodesSixAndTwoData(bool corruptAddress, bool corruptData)
    {
        byte[] table = [0x96,0x97,0x9a,0x9b,0x9d,0x9e,0x9f,0xa6,0xa7,0xab,0xac,0xad,0xae,0xaf,0xb2,0xb3,0xb4,0xb5,0xb6,0xb7,0xb9,0xba,0xbb,0xbc,0xbd,0xbe,0xbf,0xcb,0xcd,0xce,0xcf,0xd3,0xd6,0xd7,0xd9,0xda,0xdb,0xdc,0xdd,0xde,0xdf,0xe5,0xe6,0xe7,0xe9,0xea,0xeb,0xec,0xed,0xee,0xef,0xf2,0xf3,0xf4,0xf5,0xf6,0xf7,0xf9,0xfa,0xfb,0xfc,0xfd,0xfe,0xff];
        static IEnumerable<byte> FourAndFour(byte value) => [(byte)((value >> 1) | 0xaa), (byte)(value | 0xaa)];
        static string Bits(IEnumerable<byte> values) => string.Concat(values.Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        static byte[] EncodeData(byte[] source, IReadOnlyList<byte> translation, bool corrupt)
        {
            var buffer = new byte[300]; source.CopyTo(buffer, 0); var encoded = new List<byte>(343); byte checksum = 0;
            for (var index = 0; index < 86; index++)
            {
                var value = (byte)(((buffer[index] & 1) << 1) | ((buffer[index] & 2) >> 1) | ((buffer[index + 86] & 1) << 3) | ((buffer[index + 86] & 2) << 1) | ((buffer[index + 172] & 1) << 5) | ((buffer[index + 172] & 2) << 3));
                encoded.Add(translation[value ^ checksum]); checksum = value;
            }
            for (var index = 0; index < 256; index++) { var value = (byte)(source[index] >> 2); encoded.Add(translation[value ^ checksum]); checksum = value; }
            encoded.Add(translation[(checksum + (corrupt ? 1 : 0)) & 0x3f]); return encoded.ToArray();
        }
        const byte volume = 254; const byte track = 19; const byte sector = 11;
        var addressChecksum = (byte)(volume ^ track ^ sector ^ (corruptAddress ? 1 : 0));
        var address = FourAndFour(volume).Concat(FourAndFour(track)).Concat(FourAndFour(sector)).Concat(FourAndFour(addressChecksum));
        var data = Enumerable.Range(0, 256).Select(index => (byte)(index * 37 + 9)).ToArray();
        var calibration = new string('1', 100);
        var raw = calibration + Bits([0xd5,0xaa,0x96]) + Bits(address) + Bits([0xde,0xaa,0xeb,0xff,0xff,0xff]) + Bits([0xd5,0xaa,0xad]) + Bits(EncodeData(data, table, corruptData)) + Bits([0xde,0xaa,0xeb]) + "1";
        var intervals = BitsToIntervals(raw, 40);

        var result = new AppleIIGcrDecoder().Decode(new FluxRevolution(8_000_000, intervals));

        var decoded = Assert.Single(result.Sectors!); Assert.Equal(track, decoded.Cylinder); Assert.Equal(sector, decoded.Number); Assert.Equal(256, decoded.SizeBytes);
        Assert.Equal(!corruptAddress && !corruptData, decoded.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.AppleData && structure.Description.Contains(corruptData ? "invalid" : "valid", StringComparison.Ordinal));
        if (!corruptData) Assert.Equal(data, result.DecodedBytes.Skip(4).Take(256));
        Assert.Equal("apple2.gcr", result.DecoderId);
        Assert.Equal("Apple II GCR", result.DisplayName);
        Assert.Equal(1, decoded.SizeCode);
        Assert.Equal((result.Sectors.Count * 2d + result.Structures.Count) / 32d, result.Confidence, 10);
    }

    [Fact]
    public void AppleIIGcrCodecHandlesValidInvalidUnknownAndTruncatedBlocks()
    {
        static bool[] Bits(IEnumerable<byte> values) => values.SelectMany(value => Enumerable.Range(0, 8).Select(bit => (value & (1 << (7 - bit))) != 0)).ToArray();
        var source = Enumerable.Range(0, 256).Select(index => (byte)(index * 29 + 7)).ToArray();

        var sixAndTwo = AppleIIGcrCodec.EncodeSixAndTwo(source);
        var validSixAndTwo = AppleIIGcrCodec.TryDecodeSixAndTwo(Bits(sixAndTwo), 0);
        Assert.True(validSixAndTwo?.Valid);
        Assert.Equal(source, validSixAndTwo?.Data);
        var invalidSixAndTwo = sixAndTwo.ToArray();
        invalidSixAndTwo[^1] = AppleIIGcrFormat.SixAndTwoTable[(AppleIIGcrFormat.InverseSixAndTwoTable[invalidSixAndTwo[^1]] + 1) % AppleIIGcrFormat.SixAndTwoTable.Count];
        Assert.False(AppleIIGcrCodec.TryDecodeSixAndTwo(Bits(invalidSixAndTwo), 0)?.Valid);
        var unknownSixAndTwo = sixAndTwo.ToArray();
        unknownSixAndTwo[0] = 0x80;
        Assert.Null(AppleIIGcrCodec.TryDecodeSixAndTwo(Bits(unknownSixAndTwo), 0));
        Assert.Null(AppleIIGcrCodec.TryDecodeSixAndTwo(Bits(sixAndTwo.SkipLast(1)), 0));

        var fiveAndThree = AppleIIGcrCodec.EncodeFiveAndThree(source);
        var validFiveAndThree = AppleIIGcrCodec.TryDecodeFiveAndThree(Bits(fiveAndThree), 0);
        Assert.True(validFiveAndThree?.Valid);
        Assert.Equal(source, validFiveAndThree?.Data);
        var invalidFiveAndThree = fiveAndThree.ToArray();
        invalidFiveAndThree[^1] = AppleIIGcrFormat.FiveAndThreeTable[(AppleIIGcrFormat.InverseFiveAndThreeTable[invalidFiveAndThree[^1]] + 1) % AppleIIGcrFormat.FiveAndThreeTable.Count];
        Assert.False(AppleIIGcrCodec.TryDecodeFiveAndThree(Bits(invalidFiveAndThree), 0)?.Valid);
        var unknownFiveAndThree = fiveAndThree.ToArray();
        unknownFiveAndThree[0] = 0x80;
        Assert.Null(AppleIIGcrCodec.TryDecodeFiveAndThree(Bits(unknownFiveAndThree), 0));
        Assert.Null(AppleIIGcrCodec.TryDecodeFiveAndThree(Bits(fiveAndThree.SkipLast(1)), 0));
    }

    [Fact]
    public void AppleIIGcrDecoderReportsAnUnpairedDataPrologue()
    {
        var bits = Convert.FromHexString("D5AAAD").SelectMany(value => Enumerable.Range(0, 8).Select(bit => (value & (1 << (7 - bit))) != 0)).ToArray();
        var result = new AppleIIGcrDecoder().DecodeBits(bits);

        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.AppleData && structure.Description.Contains("Unpaired", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void AppleMacGcrDecoderExtractsAddressTagsDataAndChecksums(bool corruptHeader, bool corruptData)
    {
        byte[] table = [0x96,0x97,0x9a,0x9b,0x9d,0x9e,0x9f,0xa6,0xa7,0xab,0xac,0xad,0xae,0xaf,0xb2,0xb3,0xb4,0xb5,0xb6,0xb7,0xb9,0xba,0xbb,0xbc,0xbd,0xbe,0xbf,0xcb,0xcd,0xce,0xcf,0xd3,0xd6,0xd7,0xd9,0xda,0xdb,0xdc,0xdd,0xde,0xdf,0xe5,0xe6,0xe7,0xe9,0xea,0xeb,0xec,0xed,0xee,0xef,0xf2,0xf3,0xf4,0xf5,0xf6,0xf7,0xf9,0xfa,0xfb,0xfc,0xfd,0xfe,0xff];
        static string Bits(IEnumerable<byte> values) => string.Concat(values.Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        static byte[] EncodeData(byte[] source, IReadOnlyList<byte> translation, bool corrupt)
        {
            var b1 = new byte[175]; var b2 = new byte[175]; var b3 = new byte[175]; uint c1 = 0, c2 = 0, c3 = 0; var position = 0;
            for (var index = 0; ; index++)
            {
                c1 = (c1 & 0xff) << 1; if ((c1 & 0x100) != 0) c1++;
                var value = source[position++]; b1[index] = (byte)(value ^ c1); c3 += value; if ((c1 & 0x100) != 0) { c3++; c1 &= 0xff; }
                value = source[position++]; b2[index] = (byte)(value ^ c3); c2 += value; if (c3 > 0xff) { c2++; c3 &= 0xff; }
                if (position == source.Length) break;
                value = source[position++]; b3[index] = (byte)(value ^ c2); c1 += value; if (c2 > 0xff) { c1++; c2 &= 0xff; }
            }
            var symbols = new List<byte>(704) { 0 };
            for (var index = 0; index <= 174; index++)
            {
                var w4 = (byte)(((b1[index] >> 2) & 48) | ((b2[index] >> 4) & 12) | ((b3[index] >> 6) & 3));
                symbols.Add(w4); symbols.Add((byte)(b1[index] & 0x3f)); symbols.Add((byte)(b2[index] & 0x3f)); if (index != 174) symbols.Add((byte)(b3[index] & 0x3f));
            }
            var c4 = (byte)(((c1 & 0xc0) >> 6) | ((c2 & 0xc0) >> 4) | ((c3 & 0xc0) >> 2));
            symbols.Add(c4); symbols.Add((byte)(c3 & 0x3f)); symbols.Add((byte)(c2 & 0x3f)); symbols.Add((byte)(c1 & 0x3f));
            if (corrupt) symbols[^1] ^= 1;
            return symbols.Select(value => translation[value]).ToArray();
        }
        const byte cylinder = 198, head = 1, sectorNumber = 7, format = 0x12;
        var header = new byte[] { (byte)(cylinder & 0x3f), sectorNumber, (byte)(((cylinder >> 6) & 3) | (head << 5)), format };
        var headerChecksum = (byte)(header.Aggregate(0, (checksum, value) => checksum ^ value) & 0x3f); if (corruptHeader) headerChecksum ^= 1;
        var payload = Enumerable.Range(0, 512).Select(index => (byte)(index * 19 + 3)).ToArray();
        var tagged = Enumerable.Range(0, 12).Select(index => (byte)(0xa0 + index)).Concat(payload).ToArray();
        var raw = new string('1', 100) + Bits([0xd5, 0xaa, 0x96]) + Bits(header.Append(headerChecksum).Select(value => table[value])) + new string('0', 32)
            + Bits([0xd5, 0xaa, 0xad]) + Bits(EncodeData(tagged, table, corruptData)) + "1";
        var intervals = BitsToIntervals(raw, 40);

        var result = new AppleMacGcrDecoder().Decode(new FluxRevolution(8_000_000, intervals));

        var decoded = Assert.Single(result.Sectors!);
        Assert.Equal(cylinder, decoded.Cylinder); Assert.Equal(head, decoded.Head); Assert.Equal(sectorNumber, decoded.Number); Assert.Equal(format, decoded.FormatCode); Assert.Equal(512, decoded.SizeBytes);
        Assert.Equal(!corruptHeader && !corruptData, decoded.IntegrityValid);
        if (!corruptHeader) Assert.Equal(Enumerable.Range(0, 12).Select(index => (byte)(0xa0 + index)), decoded.Tag);
        Assert.Equal("applemac.gcr", result.DecoderId);
        Assert.Equal("Apple Macintosh GCR", result.DisplayName);
        Assert.Equal((result.Sectors.Count * 2d + result.Structures.Count) / 24d, result.Confidence, 10);
        if (!corruptHeader) Assert.Equal(payload, result.DecodedBytes.TakeLast(512));
    }

    [Fact]
    public void AppleIwmDecoderHandlesMissingInvalidAndUnpairedDataAndExplicitBitCell()
    {
        static bool[] Bits(IEnumerable<byte> values) => values.SelectMany(value => Enumerable.Range(0, 8).Select(bit => (value & (1 << (7 - bit))) != 0)).ToArray();
        var table = AppleIIGcrFormat.SixAndTwoTable;
        byte[] header = [3, 4, 0, AppleIwmGcrFormat.DefaultFormat];
        var checksum = (byte)(header.Aggregate(0, (value, item) => value ^ item) & AppleIwmGcrFormat.SixBitMask);
        var addressBits = Bits(AppleIwmGcrFormat.AddressMark.Concat(header.Append(checksum).Select(value => table[value])));

        var missing = new AppleMacGcrDecoder().DecodeBits(addressBits);
        Assert.Null(Assert.Single(missing.Sectors).IntegrityValid);

        var invalidSymbols = Enumerable.Repeat((byte)0xff, AppleIwmGcrFormat.DataSymbolCount).ToArray();
        invalidSymbols[0] = 0x80;
        var invalid = new AppleMacGcrDecoder().DecodeBits(addressBits.Concat(Bits(AppleIwmGcrFormat.DataMark.Concat(invalidSymbols))).ToArray());
        Assert.Contains(invalid.Structures, structure => structure.Kind == FluxStructureKind.AppleData && structure.Description.Contains("unavailable", StringComparison.Ordinal));

        var unpaired = new AppleMacGcrDecoder().DecodeBits(Bits(AppleIwmGcrFormat.DataMark));
        Assert.Contains(unpaired.Structures, structure => structure.Kind == FluxStructureKind.AppleData && structure.Description.Contains("Unpaired", StringComparison.Ordinal));

        var sector = new TrackSector(0, Enumerable.Range(0, AppleIwmGcrFormat.SectorByteCount).Select(index => (byte)index).ToArray());
        var encoded = new AppleMacGcrTrackEncoder().Encode(new(0, 0, [sector], BitCellTicks: 40));
        var fixedCell = new AppleMacGcrDecoder().DecodeAtBitCell(encoded.Revolution, 40);
        Assert.Equal(40, fixedCell.EstimatedBitCellTicks);
        Assert.Equal(sector.Data, Assert.Single(fixedCell.Sectors).Data);
    }

    [Fact]
    public void AppleMacGcrDecoderReportsUnavailableIntegrityForTruncatedData()
    {
        byte[] table = [0x96,0x97,0x9a,0x9b,0x9d,0x9e,0x9f,0xa6,0xa7,0xab,0xac,0xad,0xae,0xaf,0xb2,0xb3,0xb4,0xb5,0xb6,0xb7,0xb9,0xba,0xbb,0xbc,0xbd,0xbe,0xbf,0xcb,0xcd,0xce,0xcf,0xd3,0xd6,0xd7,0xd9,0xda,0xdb,0xdc,0xdd,0xde,0xdf,0xe5,0xe6,0xe7,0xe9,0xea,0xeb,0xec,0xed,0xee,0xef,0xf2,0xf3,0xf4,0xf5,0xf6,0xf7,0xf9,0xfa,0xfb,0xfc,0xfd,0xfe,0xff];
        static string Bits(IEnumerable<byte> values) => string.Concat(values.Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        byte[] header = [3, 4, 0, 0x12]; var checksum = (byte)(header.Aggregate(0, (value, item) => value ^ item) & 0x3f);
        var raw = new string('1', 100) + Bits([0xd5, 0xaa, 0x96]) + Bits(header.Append(checksum).Select(value => table[value])) + new string('0', 32)
            + Bits([0xd5, 0xaa, 0xad]) + Bits(Enumerable.Repeat((byte)0xff, 650)) + "1";
        var intervals = BitsToIntervals(raw, 40);
        var result = new AppleMacGcrDecoder().Decode(new FluxRevolution(8_000_000, intervals));
        Assert.Null(Assert.Single(result.Sectors!).IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.AppleData && structure.Description.Contains("unavailable", StringComparison.Ordinal));
    }

    [Fact]
    public void AppleIIGcrDecoderReportsUnavailableIntegrityWhenDataBlockIsMissing()
    {
        var calibration = new string('1', 100); var mark = string.Concat(Convert.FromHexString("D5AA96").Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        var address = string.Concat(Enumerable.Repeat("10101010", 8)); var epilogue = string.Concat(Convert.FromHexString("DEAAEB").Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        var intervals = BitsToIntervals(calibration + mark + address + epilogue + "0001", 40); var result = new AppleIIGcrDecoder().Decode(new FluxRevolution(8_000_000, intervals));
        Assert.Null(Assert.Single(result.Sectors!).IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.AppleAddress && structure.Description.Contains("unavailable", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void CommodoreGcrDecoderExtractsTrackSectorAndValidatesData(bool corruptHeader, bool corruptData)
    {
        int[] table = [0x0a,0x0b,0x12,0x13,0x0e,0x0f,0x16,0x17,0x09,0x19,0x1a,0x1b,0x0d,0x1d,0x1e,0x15];
        string Encode(IEnumerable<byte> values) => string.Concat(values.SelectMany(value => new[] { value >> 4, value & 15 }).Select(nibble => Convert.ToString(table[nibble], 2).PadLeft(5, '0')));
        const byte track = 23; const byte sector = 8; const byte id2 = 0xa1; const byte id1 = 0x1a;
        var headerChecksum = (byte)(sector ^ track ^ id2 ^ id1 ^ (corruptHeader ? 1 : 0));
        byte[] header = [0x08, headerChecksum, sector, track, id2, id1];
        var data = Enumerable.Range(0, 256).Select(index => (byte)(index * 43 + 5)).ToArray(); byte checksum = 0; foreach (var value in data) checksum ^= value;
        if (corruptData) checksum ^= 1;
        var dataBlock = new byte[] { 0x07 }.Concat(data).Append(checksum).ToArray();
        var raw = new string('1', 100) + "000" + new string('1', 20) + Encode(header) + "000000" + new string('1', 20) + Encode(dataBlock) + "0001";
        var intervals = BitsToIntervals(raw, 40);

        var result = new CommodoreGcrDecoder().Decode(new FluxRevolution(8_000_000, intervals));

        var decoded = Assert.Single(result.Sectors!); Assert.Equal(track, decoded.Cylinder); Assert.Equal(sector, decoded.Number); Assert.Equal(256, decoded.SizeBytes);
        Assert.Equal(!corruptHeader && !corruptData, decoded.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData && structure.Description.Contains(corruptData ? "invalid" : "valid", StringComparison.Ordinal));
        Assert.Equal(data, result.DecodedBytes.Skip(7).Take(256));
    }

    [Fact]
    public void CommodoreGcrDecoderReportsUnavailableIntegrityWhenDataIsMissing()
    {
        int[] table = [0x0a,0x0b,0x12,0x13,0x0e,0x0f,0x16,0x17,0x09,0x19,0x1a,0x1b,0x0d,0x1d,0x1e,0x15];
        string Encode(IEnumerable<byte> values) => string.Concat(values.SelectMany(value => new[] { value >> 4, value & 15 }).Select(nibble => Convert.ToString(table[nibble], 2).PadLeft(5, '0')));
        byte[] header = [0x08, 0x03, 0x02, 0x01, 0xa1, 0xa1]; var raw = new string('1', 100) + "000" + new string('1', 20) + Encode(header) + "0001";
        var intervals = BitsToIntervals(raw, 40); var result = new CommodoreGcrDecoder().Decode(new FluxRevolution(8_000_000, intervals));
        Assert.Null(Assert.Single(result.Sectors!).IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.CommodoreHeader && structure.Description.Contains("unavailable", StringComparison.Ordinal));
    }
}
