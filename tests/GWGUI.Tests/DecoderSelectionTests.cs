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

public sealed class DecoderSelectionTests : CoreTestBase
{
    [Fact]
    public void NativeChecksumDecodersReportCorruptedBlocks()
    {
        var northstarData = new byte[512];
        var northstarBlock = Enumerable.Repeat((byte)0, 7)
            .Concat([(byte)0xfb, (byte)0x21])
            .Concat(northstarData)
            .Append((byte)0x01)
            .ToArray();
        var northstarIntervals = BitsToIntervals(EncodeMfmBytesFromZero(northstarBlock) + "001", 40);
        var northstar = new NorthstarMfmDecoder().Decode(new FluxRevolution(8_000_000, northstarIntervals));

        static byte Reverse(byte value) { byte result = 0; for (var bit = 0; bit < 8; bit++) result = (byte)((result << 1) | ((value >> bit) & 1)); return result; }
        var heathkitBits = EncodeFmBytes(0, 0, 0, 0xbf, Reverse(1), Reverse(2), Reverse(3), Reverse(0xff)) + "001";
        var heathkitIntervals = BitsToIntervals(heathkitBits, 40);
        var heathkit = new HeathkitFmDecoder().Decode(new FluxRevolution(8_000_000, heathkitIntervals));

        Assert.False(Assert.Single(northstar.Sectors!).IntegrityValid);
        Assert.False(Assert.Single(heathkit.Sectors!).IntegrityValid);
    }

    [Theory]
    [InlineData("membrain.mfm", "44895554", FluxStructureKind.FormatHeader)]
    [InlineData("aed6200p.mfm", "5094", FluxStructureKind.FormatHeader)]
    [InlineData("centurion.mfm", "91224489", FluxStructureKind.FormatHeader)]
    [InlineData("emu.fm", "4545555545545445", FluxStructureKind.FormatHeader)]
    [InlineData("arburg", "5555555555249249", FluxStructureKind.FormatHeader)]
    [InlineData("victor9k.gcr", "5555555555551111", FluxStructureKind.FormatHeader)]
    [InlineData("tycom.fm", "55111444", FluxStructureKind.FormatData)]
    [InlineData("dec.rx02", "55111545", FluxStructureKind.FormatData)]
    public async Task SignatureMfmDecodersRecognizeTheirNativeMarks(string decoderId, string hexadecimal, FluxStructureKind expectedKind)
    {
        var mark = string.Concat(Convert.FromHexString(hexadecimal).Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        var calibration = decoderId is "emu.fm" or "tycom.fm" or "dec.rx02" or "arburg" or "victor9k.gcr" ? "" : string.Concat(Enumerable.Repeat("10", 50));
        var bits = calibration + string.Concat(Enumerable.Repeat(mark + "000", 4)) + "001";
        var intervals = BitsToIntervals(bits, 40);
        var image = new ScpReader().Read(BuildSingleTrackScp(intervals));
        var track = Assert.Single(image.Tracks);
        var result = new FluxDecoderRegistry().Decode(decoderId, Assert.Single(track.Revolutions).Flux);
        Assert.Contains(result.Structures, structure => structure.Kind == expectedKind);

        static string Localize(string key, object[] arguments) => arguments.Length == 0 ? key : $"{key}({string.Join(',', arguments)})";
        var inspection = new ScpInspectorPresenter(new FluxDecoderRegistry(), Localize).Build(image, track, decoderId);
        Assert.Contains("Visual.StructureKind." + expectedKind, inspection);

        using var bitmap = new SKBitmap(320, 320);
        using var canvas = new SKCanvas(bitmap);
        IScpRenderer renderer = new SkiaScpRenderer { DecoderId = decoderId };
        await renderer.PrepareAsync(image, 0);
        renderer.Render(canvas, new ScpRenderRequest(image, 0, track, 320, 320, new SKPoint(160, 160), 1, "No data", "Side 0"));
        var overlay = expectedKind == FluxStructureKind.FormatData ? new SKColor(67, 220, 255) : new SKColor(255, 205, 64);
        Assert.Contains(Enumerable.Range(0, bitmap.Height).SelectMany(y => Enumerable.Range(0, bitmap.Width).Select(x => bitmap.GetPixel(x, y))), color => color == overlay);
        renderer.ClearCache();
    }

    [Fact]
    public void DecoderRegistrySelectsMostConvincingRevolution()
    {
        var weak = new FluxRevolution(8_000_000, [40u, 40u]);
        var sectors = Enumerable.Range(0, AppleIIGcrFormat.SixAndTwoSectorsPerTrack).Select(number => new TrackSector(number, new byte[AppleIIGcrFormat.SectorSize])).ToArray();
        var strong = new AppleIIGcrTrackEncoder().Encode(new(0, 0, sectors)).Revolution;
        var best = new FluxDecoderRegistry().DecodeBest([weak, strong], "apple2.gcr");
        Assert.NotNull(best); Assert.Equal(1, best.RevolutionIndex); Assert.Equal("apple2.gcr", best.Result.DecoderId);
    }

    [Fact]
    public void AutomaticDecoderRejectsInvalidOnlyFalseRecognitionInFavorOfRawFlux()
    {
        var invalid = new FluxDecodeResult("false.fm", "False FM", 1, 40, [new(FluxStructureKind.DeletedDataAddressMark, 0, 16, "false")], [], [new(0, 0, 1, 2, 512, false, 0)]);
        var raw = new FluxDecodeResult("raw", "Raw", .05, 40, [], []);
        var valid = new FluxDecodeResult("valid.mfm", invalid.DisplayName, invalid.Confidence, invalid.EstimatedBitCellTicks, invalid.Structures, invalid.DecodedBytes, [new(0, 0, 1, 2, 512, true, 0)]);

        Assert.True(AutomaticScore(raw) > AutomaticScore(invalid));
        Assert.True(AutomaticScore(valid) > AutomaticScore(raw));

        static double AutomaticScore(FluxDecodeResult result)
        {
            return FluxDecoderScoring.Calculate(result);
        }
    }
}
