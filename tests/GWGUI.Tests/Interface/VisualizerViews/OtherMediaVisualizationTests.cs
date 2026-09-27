using GWGUI.App.Contracts.Rendering.Blocks;
using GWGUI.App.Contracts.Rendering.Optical;
using GWGUI.App.Contracts.Rendering.Sequential;
using GWGUI.App.Enums.Rendering.Blocks;
using GWGUI.App.Enums.Rendering.Optical;
using GWGUI.App.Enums.Rendering.Sequential;
using GWGUI.App.Presenters.Visualization;
using GWGUI.App.Rendering.Blocks;
using GWGUI.App.Rendering.Optical;
using GWGUI.App.Rendering.Sequential;
using MediaSourceDescriptor = global::GWGUI.MediaEngine.Contracts.MediaSourceDescriptor;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Images.Models.Optical;
using GWGUI.MediaEngine.Images.Models.Blocks;
using GWGUI.MediaEngine.Images.Models.Sequential;
using GWGUI.MediaEngine.Images.Visualization.Providers;
using GWGUI.MediaFileSystems.Contracts;
using SkiaSharp;
using RenderSequentialMediaSegment = GWGUI.App.Contracts.Rendering.Sequential.SequentialMediaSegment;

namespace GWGUI.Tests.Interface.VisualizerViews;

public sealed class OtherMediaVisualizationTests
{
    [Fact]
    public void AmstradCartridgeUsesItsThirtyTwoDeclaredBanksAsCartridgeBlocks()
    {
        const int bankSize = 16 * 1024;
        var ranges = Enumerable.Range(0, 32)
            .Select(bank => ((long)bank * bankSize, (long)bankSize))
            .ToArray();
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["mediaRole"] = "cartridge",
            ["bankSize"] = bankSize.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["bankCount"] = "32"
        };
        foreach (var bank in Enumerable.Range(0, 32)) metadata[$"bank.{bank}.chunkId"] = $"cb{bank:D2}";
        var representation = new BlockMediaImageRepresentation(32L * bankSize, ranges);
        var document = new MediaImageDocument(
            new MediaSourceDescriptor("test.cpr", []),
            "amstrad.cpr",
            MediaKind.Cartridge,
            representation,
            [new MediaVolumeDescriptor(0, 32L * bankSize, GWGUI.MediaEngine.Constants.MediaVolumeOrigins.DirectVolume)],
            [],
            metadata);

        var model = new BlockMediaInspectorPresenter((key, _) => key).BuildRenderModel(document);
        var descriptor = new BlockMediaVisualizationProvider().CreateDescriptor(document);

        Assert.Equal(BlockMediaShape.Cartridge, model.Shape);
        Assert.Equal(32, model.Ranges.Count);
        Assert.Equal("cb00", model.Ranges[0].Label);
        Assert.Equal("cb31", model.Ranges[^1].Label);
        Assert.All(model.Ranges, range => Assert.Equal(bankSize, range.Length));
        Assert.Equal(32, descriptor.Elements.Count);
        Assert.NotNull(new SkiaBlockMediaRenderer().HitTest(
            model, 800, 600, new SKPoint(240, 170)));

        var inspector = new BlockMediaInspectorPresenter((key, _) => key)
            .BuildInspectorModel(model, model.Ranges[21]);
        var entries = inspector.Sections.SelectMany(section => section.Entries).ToArray();
        Assert.Equal("cb21", inspector.SelectedElement);
        Assert.DoesNotContain(entries, entry => entry.Value == "LBA" || entry.Unit == "LBA");
        Assert.DoesNotContain(entries, entry => entry.Unit == "Visual.BlocksUnit");
        Assert.Contains(entries, entry => entry.Label == "Visual.CapacityLabel" && entry.Value.Contains("512"));
        Assert.Contains(entries, entry => entry.Label == "Visual.SizeLabel" && entry.Value.Contains("16"));
    }

    [Fact]
    public void RawAmstradRomUsesBankNamesWithoutLbaAddressing()
    {
        const int bankSize = 16 * 1024;
        var document = new MediaImageDocument(
            new MediaSourceDescriptor("test.rom", []),
            "amstrad.rom",
            MediaKind.Cartridge,
            new BlockMediaImageRepresentation(
                bankSize,
                [(0L, (long)bankSize)]),
            [new MediaVolumeDescriptor(0, bankSize, GWGUI.MediaEngine.Constants.MediaVolumeOrigins.DirectVolume)],
            [],
            new Dictionary<string, string>
            {
                ["bankSize"] = bankSize.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["bankCount"] = "1"
            });
        var presenter = new BlockMediaInspectorPresenter((key, _) => key);
        var model = presenter.BuildRenderModel(document);
        var inspector = presenter.BuildInspectorModel(model, Assert.Single(model.Ranges));

        Assert.Equal("bank00", inspector.SelectedElement);
        Assert.DoesNotContain(inspector.Sections.SelectMany(section => section.Entries),
            entry => entry.Value == "LBA" || entry.Unit == "LBA");
    }

    [Fact]
    public void BlockSelectionKeepsSixtyFourBitAddresses()
    {
        const long length = (long)int.MaxValue * 8;
        var range = new BlockMediaRange(0, length, BlockMediaRangeState.Allocated);
        var model = new BlockMediaRenderModel(length, [range]);

        var selected = new SkiaBlockMediaRenderer().HitTest(model, 800, 800, new SKPoint(700, 400));
        var inspector = new BlockMediaInspectorPresenter((key, _) => key).BuildInspectorModel(model, selected);

        Assert.Same(range, selected);
        Assert.Equal(length, selected!.Length);
        Assert.Contains(inspector.Sections.SelectMany(section => section.Entries),
            entry => entry.Label == "Visual.LengthLabel" && entry.Value == length.ToString("N0"));
    }

    [Fact]
    public void OpticalSelectionDoesNotInventMissingFaceOrLayer()
    {
        var track = new OpticalMediaTrack(1, 1, 0, 10_000, OpticalTrackKind.Data);
        var model = new OpticalMediaRenderModel(10_000, null, null, [track], []);
        var selected = new SkiaOpticalMediaRenderer().HitTest(
            model, null, null, null, 800, 800, new SKPoint(700, 400));
        var representation = new OpticalMediaImageRepresentation(10_000, [(1, 1, 0L, 10_000L)]);
        var document = new MediaImageDocument(
            new MediaSourceDescriptor("memory.iso", []), "test.optical", MediaKind.Optical,
            representation, [], [], new Dictionary<string, string>());
        var inspector = new OpticalMediaInspectorPresenter((key, _) => key)
            .BuildInspectorModel(document, model, selected);
        var entries = inspector.Sections.SelectMany(section => section.Entries).ToArray();

        Assert.Same(track, selected);
        Assert.DoesNotContain(entries, entry => entry.Label == "Visual.SideLabel");
        Assert.DoesNotContain(entries, entry => entry.Label == "Visual.LayerLabel");
        Assert.DoesNotContain(entries, entry => entry.Label == "Visual.FaceCountLabel");
        Assert.DoesNotContain(entries, entry => entry.Label == "Visual.LayerCountLabel");
    }

    [Fact]
    public void SequentialSelectionUsesLaneAndTime()
    {
        var segment = new RenderSequentialMediaSegment(
            0, 2, TimeSpan.Zero, TimeSpan.FromSeconds(10), GWGUI.App.Enums.Rendering.Sequential.SequentialSegmentKind.Signal,
            ChannelNumber: 2);
        var model = new SequentialMediaRenderModel(null, TimeSpan.FromSeconds(10), [segment]);

        var selected = new SkiaSequentialMediaRenderer().HitTest(
            model, 1, 1_000, 200, new SKPoint(500, 100));
        var inspector = new SequentialMediaInspectorPresenter((key, _) => key)
            .BuildInspectorModel(model, selected);

        Assert.Same(segment, selected);
        Assert.Contains(inspector.Sections.SelectMany(section => section.Entries),
            entry => entry.Label == "Visual.ChannelLabel" && entry.Value == "2");
        Assert.DoesNotContain(inspector.Sections.SelectMany(section => section.Entries),
            entry => entry.Label == "Visual.CapacityLabel");
    }

    [Fact]
    public void UntimedSequentialBlocksUseTheirStoredLengthsForTheTapeLayout()
    {
        var first = new RenderSequentialMediaSegment(
            0, 0, TimeSpan.Zero, TimeSpan.Zero,
            GWGUI.App.Enums.Rendering.Sequential.SequentialSegmentKind.Signal,
            StoredLength: 25);
        var second = new RenderSequentialMediaSegment(
            1, 0, TimeSpan.Zero, TimeSpan.Zero,
            GWGUI.App.Enums.Rendering.Sequential.SequentialSegmentKind.DecodedBlock,
            StoredLength: 75);
        var model = new SequentialMediaRenderModel(100, null, [first, second]);
        using var bitmap = new SKBitmap(1_000, 200);
        using var canvas = new SKCanvas(bitmap);
        var renderer = new SkiaSequentialMediaRenderer();

        renderer.Render(canvas, model, null, 2, 1, bitmap.Width, bitmap.Height);

        Assert.Same(first, renderer.HitTest(model, 1, bitmap.Width, bitmap.Height, new SKPoint(150, 100)));
        Assert.Same(second, renderer.HitTest(model, 1, bitmap.Width, bitmap.Height, new SKPoint(700, 100)));
        Assert.NotEqual(bitmap.GetPixel(150, 100), bitmap.GetPixel(700, 100));
    }

    [Fact]
    public void AtariCasInspectorShowsCassetteAndChunkMetadata()
    {
        var sourceSegment = new GWGUI.MediaEngine.Contracts.SequentialMediaSegment(
            0,
            GWGUI.MediaEngine.Enums.SequentialSegmentKind.DataBlock,
            132,
            TimeSpan.Zero,
            TimeSpan.FromSeconds(2),
            metadata: new Dictionary<string, string>
            {
                ["chunkId"] = "data",
                ["baudRate"] = "600",
                ["auxiliary"] = "260"
            });
        var representation = new SequentialMediaImageRepresentation(132, TimeSpan.FromSeconds(2), [sourceSegment]);
        var document = new MediaImageDocument(
            new MediaSourceDescriptor("test.cas", []),
            "tape.atari-cas",
            MediaKind.Tape,
            representation,
            [],
            [],
            new Dictionary<string, string>
            {
                ["internalName"] = "TEST",
                ["baudRates"] = "600",
                ["chunkCount"] = "3",
                ["chunkTypes"] = "FUJI, baud, data",
                ["dataChunkCount"] = "1",
                ["fskChunkCount"] = "0"
            });
        var presenter = new SequentialMediaInspectorPresenter((key, _) => key);
        var model = presenter.BuildRenderModel(document);
        var inspector = presenter.BuildInspectorModel(model, Assert.Single(model.Segments));
        var entries = inspector.Sections.SelectMany(section => section.Entries).ToArray();

        Assert.Equal("Visual.CassetteInspectorTitle", inspector.Title);
        Assert.Contains(entries, entry => entry.Label == "Explorer.BaudRates" && entry.Value == "600");
        Assert.Contains(entries, entry => entry.Label == "Explorer.ChunkType" && entry.Value == "data");
        Assert.Contains(entries, entry => entry.Label == "Explorer.BaudRate" && entry.Value == "600");
        Assert.Contains(entries, entry => entry.Label == "Explorer.DelayBeforeBlock" && entry.Value == "260");
    }
}
