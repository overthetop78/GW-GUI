using GWGUI.App.Presenters.Visualization;
using GWGUI.App.Contracts.Progress;
using GWGUI.App.ViewModels.Visualization;
using GWGUI.App.Views.Controls.Visualization;
using GWGUI.Domain.Contracts;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Interfaces.Visualization;
using GWGUI.MediaEngine.Representations.Flux;
using GWGUI.MediaEngine.Representations.Sectors;
using GWGUI.MediaEngine.Visualization.Providers;
using GWGUI.Tests.Application.TestInfrastructure;

namespace GWGUI.Tests.Interface.VisualizerViews;

[Collection("WPF")]
public sealed class FloppyVisualizationTests(StaExecutionScenarios sta)
{
    [Fact]
    public void FluxAndSectorDocumentsKeepDifferentInformation()
    {
        var fluxTrack = new ProtectedTrack(
            3,
            1,
            null,
            [],
            [],
            [],
            [new TrackFluxRevolution(25, new FluxRevolution(1_000, [10, 20, 30]))]);
        var fluxDocument = Document(new FluxMediaImageRepresentation(new ProtectedTrackImage([fluxTrack], false)));
        var flux = new FluxMediaVisualizationProvider().CreateDescriptor(fluxDocument);

        var blocks = new[]
        {
            new SectorBlock(0, new SectorAddress(0, 0, 1), new byte[512], true),
            new SectorBlock(1, new SectorAddress(0, 0, 2), new byte[512], false)
        };
        var image = new SectorImage("test.sectors", 512, 1, 1, 3, blocks);
        var sectorDocument = Document(new SectorMediaImageRepresentation(image));
        var sectors = new SectorMediaVisualizationProvider().CreateDescriptor(sectorDocument);
        var sectorModel = new SectorMediaInspectorPresenter((key, _) => key).BuildRenderModel(image);

        Assert.Equal(MediaRepresentationKind.Flux, flux.RepresentationKind);
        Assert.Equal(MediaVisualizationDirection.SourceDefined, flux.Direction);
        Assert.Equal(3, Assert.Single(flux.Elements).Position);
        Assert.Equal(MediaRepresentationKind.Sectors, sectors.RepresentationKind);
        Assert.Equal(MediaVisualizationDirection.Ascending, sectors.Direction);
        Assert.Equal(3, Assert.Single(Assert.Single(sectorModel.Surfaces).Tracks).Sectors.Count);
        Assert.Equal(3, fluxTrack.Revolutions[0].Flux.FluxIntervals.Count);
    }

    [Fact]
    public Task ProgressAndSelectionUseDeclaredTrackPositions() => sta.Run(() =>
    {
        var strip = new TrackProgressStrip();
        strip.Configure(MediaVisualizationProgressUnit.Track, 1, [0L, 7L, 42L], "Side 1");

        strip.SetState(7, TrackSegmentState.Success);
        strip.Select(42);

        Assert.Equal(3, strip.Total);
        Assert.Equal(1, strip.Completed);
        Assert.Equal(TrackSegmentState.Success, strip.Segments.Single(item => item.Position == 7).State);
        Assert.True(strip.Segments.Single(item => item.Position == 42).IsSelected);
        Assert.False(strip.Segments.Single(item => item.Position == 0).IsSelected);
    });

    private static MediaImageDocument Document(GWGUI.MediaEngine.Interfaces.IMediaImageRepresentation representation) => new(
        new MediaSourceDescriptor("memory.image", []),
        "test.format",
        MediaKind.Floppy,
        representation,
        [],
        [],
        new Dictionary<string, string>());
}
