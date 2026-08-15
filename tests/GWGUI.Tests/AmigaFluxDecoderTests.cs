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

public sealed class AmigaFluxDecoderTests : CoreTestBase
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void AmigaMfmDecoderExtractsIdentityAndDecodesOddEvenData(bool corruptHeader, bool corruptData)
    {
        static byte Nibble(byte value, bool odd)
        {
            byte result = 0; var firstBit = odd ? 7 : 6; for (var index = 0; index < 4; index++) result |= (byte)(((value >> (firstBit - index * 2)) & 1) << (3 - index)); return result;
        }
        static byte[] EncodeOddEven(IReadOnlyList<byte> values)
        {
            var odd = new List<byte>(); var even = new List<byte>();
            for (var index = 0; index < values.Count; index += 2) { odd.Add((byte)((Nibble(values[index], true) << 4) | Nibble(values[index + 1], true))); even.Add((byte)((Nibble(values[index], false) << 4) | Nibble(values[index + 1], false))); }
            return odd.Concat(even).ToArray();
        }
        static (byte High, byte Low) Parity(IReadOnlyList<byte> encoded, bool split)
        {
            byte high = 0, low = 0;
            if (split) { var half = encoded.Count / 2; for (var index = 0; index < half; index += 2) { high ^= (byte)(encoded[index] ^ encoded[half + index]); low ^= (byte)(encoded[index + 1] ^ encoded[half + index + 1]); } }
            else for (var index = 0; index < encoded.Count; index += 4) { high ^= (byte)(encoded[index] ^ encoded[index + 2]); low ^= (byte)(encoded[index + 1] ^ encoded[index + 3]); }
            return (high, low);
        }
        const byte cylinder = 34; const byte head = 1; const byte sector = 7;
        byte[] info = [0xff, (byte)(cylinder << 1 | head), sector, 4]; var headerAndLabel = EncodeOddEven(info).Concat(new byte[16]).ToArray(); var headerParity = Parity(headerAndLabel, false);
        var data = Enumerable.Range(0, 512).Select(index => (byte)(index * 47 + 3)).ToArray(); var encodedData = EncodeOddEven(data); var dataParity = Parity(encodedData, true);
        if (corruptHeader) headerParity.High ^= 1; if (corruptData) dataParity.Low ^= 1;
        var encoded = headerAndLabel.Concat(new byte[] { 0,0,headerParity.High,headerParity.Low,0,0,dataParity.High,dataParity.Low }).Concat(encodedData).ToArray();
        var raw = string.Concat(Enumerable.Repeat("10", 50)) + Convert.ToString(0x44894489, 2).PadLeft(32, '0') + EncodeMfmBytesFromZero(encoded) + "001";
        var intervals = BitsToIntervals(raw, 40);

        var result = new AmigaMfmDecoder().Decode(new FluxRevolution(8_000_000, intervals));

        var decoded = Assert.Single(result.Sectors!); Assert.Equal(cylinder, decoded.Cylinder); Assert.Equal(head, decoded.Head); Assert.Equal(sector, decoded.Number); Assert.Equal(512, decoded.SizeBytes);
        Assert.Equal(!corruptHeader && !corruptData, decoded.IntegrityValid);
        Assert.Equal(data, result.DecodedBytes.Skip(4).Take(512));
        Assert.Equal("amiga.mfm", result.DecoderId);
        Assert.Equal("Amiga MFM", result.DisplayName);
        Assert.Equal(2, decoded.SizeCode);
        Assert.Equal(4d / 44d, result.Confidence, 10);
    }

    [Fact]
    public void AmigaMfmCodecRoundTripsOddEvenDataAndCalculatesBothParities()
    {
        byte[] values = [0x00, 0xff, 0x12, 0x34, 0x56, 0x78, 0x9a, 0xbc];
        var encoded = AmigaMfmCodec.EncodeOddEven(values);

        Assert.Equal(values, AmigaMfmCodec.DecodeOddEven(encoded));
        Assert.Equal(((byte)0, (byte)8), AmigaMfmCodec.CalculateParity(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }, 0, 8));
        Assert.Equal(((byte)0, (byte)8), AmigaMfmCodec.CalculateSplitParity(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }, 0, 8));
    }

    [Fact]
    public void AmigaMfmDecoderReportsUnavailableIntegrityWhenDataIsTruncated()
    {
        var encodedHeader = new byte[28]; encodedHeader[0] = 0xf0; encodedHeader[2] = 0xf0;
        var raw = string.Concat(Enumerable.Repeat("10", 50)) + Convert.ToString(0x44894489, 2).PadLeft(32, '0') + EncodeMfmBytesFromZero(encodedHeader) + "001";
        var intervals = BitsToIntervals(raw, 40); var result = new AmigaMfmDecoder().Decode(new FluxRevolution(8_000_000, intervals));
        Assert.Null(Assert.Single(result.Sectors!).IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.AmigaSync && structure.Description.Contains("unavailable", StringComparison.Ordinal));
    }
}
