using GWGUI.App.Presenters.Visualization;
using GWGUI.App.Contracts.Progress;
using GWGUI.App.ViewModels.Visualization;
using GWGUI.App.Views.Controls.Visualization;
using MediaSourceDescriptor = global::GWGUI.MediaEngine.Contracts.MediaSourceDescriptor;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Interfaces.Visualization;
using GWGUI.MediaEngine.Representations.Flux;
using GWGUI.MediaEngine.Representations.Sectors;
using GWGUI.MediaEngine.Visualization.Providers;
using GWGUI.Tests.Application.TestInfrastructure;
using GWGUI.App.Enums.Rendering.Sectors;

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

    [Fact]
    public void SectorStatesAnalyzePayloadIntegritySizeAndPhysicalAddresses()
    {
        var data = new byte[512];
        data[0] = 1;
        var image = new SectorImage("test.sectors", 512, 1, 1, 8,
        [
            new SectorBlock(0, new SectorAddress(0, 0, 1), data, true),
            new SectorBlock(1, new SectorAddress(0, 0, 2), new byte[512], true),
            new SectorBlock(2, new SectorAddress(0, 0, 3), [], true),
            new SectorBlock(3, new SectorAddress(0, 0, 4), new byte[512], null, DiagnosticCode: 7),
            new SectorBlock(4, new SectorAddress(0, 0, 5), new byte[512], false),
            new SectorBlock(5, new SectorAddress(0, 0, 6), new byte[256], true),
            new SectorBlock(6, new SectorAddress(1, 0, 7), Enumerable.Repeat((byte)0xE5, 512).ToArray(), true),
            new SectorBlock(7, new SectorAddress(0, 0, 7), Enumerable.Repeat((byte)0xFF, 512).ToArray(), true)
        ]);

        var presenter = new SectorMediaInspectorPresenter((key, _) => key);
        var sectors = Assert.Single(Assert.Single(presenter.BuildRenderModel(image).Surfaces).Tracks).Sectors;
        Assert.Equal(
            [
                SectorMediaElementState.WithData,
                SectorMediaElementState.WithData,
                SectorMediaElementState.WithoutData,
                SectorMediaElementState.Degraded,
                SectorMediaElementState.Dead,
                SectorMediaElementState.Degraded,
                SectorMediaElementState.Degraded,
                SectorMediaElementState.Degraded
            ],
            sectors.Select(sector => sector.State));
        Assert.Equal(
            [
                "Visual.SectorWithData",
                "Visual.SectorWithData",
                "Visual.SectorWithoutData",
                "Visual.SectorDegraded",
                "Visual.SectorDead",
                "Visual.SectorDegraded",
                "Visual.SectorDegraded",
                "Visual.SectorDegraded"
            ],
            sectors.Select(sector => presenter.BuildInspectorModel(0, sector).Sections[0].Entries[^1].Value));
    }

    [Theory]
    [InlineData(0x00)]
    [InlineData(0xE5)]
    [InlineData(0xF6)]
    [InlineData(0xFF)]
    public void FilledPayloadIsStillData(byte fill)
    {
        var image = new SectorImage("test.sectors", 512, 1, 1, 1,
        [
            new SectorBlock(0, new SectorAddress(0, 0, 1), Enumerable.Repeat(fill, 512).ToArray(), true)
        ]);

        var presenter = new SectorMediaInspectorPresenter((key, _) => key);
        var sector = Assert.Single(Assert.Single(Assert.Single(presenter.BuildRenderModel(image).Surfaces).Tracks).Sectors);

        Assert.Equal(SectorMediaElementState.WithData, sector.State);
    }

    private static MediaImageDocument Document(GWGUI.MediaEngine.Interfaces.IMediaImageRepresentation representation) => new(
        new MediaSourceDescriptor("memory.image", []),
        "test.format",
        MediaKind.Floppy,
        representation,
        [],
        [],
        new Dictionary<string, string>());
}
