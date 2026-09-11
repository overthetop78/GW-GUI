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
using GWGUI.Domain.Contracts;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Representations.Optical;
using SkiaSharp;
using RenderSequentialMediaSegment = GWGUI.App.Contracts.Rendering.Sequential.SequentialMediaSegment;

namespace GWGUI.Tests.Interface.VisualizerViews;

public sealed class OtherMediaVisualizationTests
{
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
            0, 2, TimeSpan.Zero, TimeSpan.FromSeconds(10), SequentialSegmentKind.Signal,
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
}
