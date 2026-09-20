using GWGUI.App.Contracts.ViewModels.Visualization;
using GWGUI.App.Contracts.Rendering.Sectors;
using GWGUI.App.Contracts.Rendering.Scp;
using GWGUI.App.Enums.Rendering.Sectors;
using GWGUI.App.Enums.Rendering.Scp;
using GWGUI.App.Services.Theming;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Localization.Sources;
using GWGUI.App.Rendering.Sectors;
using GWGUI.App.Rendering.Scp;
using GWGUI.App.Views.Controls.Visualization;
using GWGUI.App.Views.Controls.Explorer;
using GWGUI.Infrastructure.Settings;
using GWGUI.MediaEngine.Enums;
using MediaSourceDescriptor = global::GWGUI.MediaEngine.Contracts.MediaSourceDescriptor;
using GWGUI.MediaEngine.Formats;
using GWGUI.MediaEngine.Constants;

using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Visualization;
using GWGUI.MediaEngine.Formats.Floppy.Scp;
using IMediaImageRepresentation = global::GWGUI.MediaEngine.Interfaces.IMediaImageRepresentation;
using SkiaSharp;
using GWGUI.Tests.Application.TestInfrastructure;
using GWGUI.Tests.Interface.ExplorerViews;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Reflection;
using System.Globalization;

namespace GWGUI.Tests.Interface.VisualizerViews;

[Collection("WPF")]
public sealed class MediaVisualizationLayoutTests(StaExecutionScenarios sta)
{
    [Theory]
    [InlineData(1280, 720)]
    [InlineData(2560, 1440)]
    public Task LayoutKeepsRenderingAndInspectorInsideAvailableSpace(double width, double height) => sta.Run(() =>
    {
        var section = new VisualizerTabSection();
        section.RegisterRepresentationView(MediaRepresentationKind.Sectors, new Grid());
        section.ShowDocument(Document(), Descriptor(0, 1));
        section.SetInspectorModel(0, Model("Face 0 selection"));
        section.SetInspectorModel(1, Model("Face 1 selection"));
        section.Measure(new Size(width, height));
        section.Arrange(new Rect(0, 0, width, height));

        var band = Assert.IsType<Border>(section.FindName("InspectorBand"));
        var first = Assert.IsType<MediaInspectorPanel>(section.FindName("Face0Inspector"));
        var second = Assert.IsType<MediaInspectorPanel>(section.FindName("Face1Inspector"));
        Assert.True(section.IsInspectorVisible);
        Assert.Equal(118, band.ActualHeight);
        Assert.True(first.ActualWidth > 0);
        Assert.True(second.ActualWidth > 0);
        Assert.Equal(2, Grid.GetRow(band));
        Assert.Equal(0, Grid.GetColumn(first));
        Assert.Equal(2, Grid.GetColumn(second));
        Assert.InRange(section.Header.ActualHeight, 1, 80);
        var overview = Assert.IsType<Border>(section.FindName("OverviewBar"));
        Assert.InRange(overview.ActualHeight, 1, 32);
        var trackOverview = Assert.IsType<VisualizerTrackOverview>(section.FindName("TrackOverview"));
        var progressRows = Assert.IsType<StackPanel>(trackOverview.FindName("ProgressRows"));
        Assert.Equal(2, progressRows.Children.Count);
        Assert.All(progressRows.Children.Cast<TrackProgressStrip>(), strip =>
        {
            Assert.InRange(strip.ActualHeight, 1, 12);
            var segments = Assert.IsType<ItemsControl>(strip.FindName("ProgressSegments"));
            Assert.InRange(segments.ActualHeight, 1, 8);
        });
        var visualizationArea = Assert.IsType<Grid>(section.FindName("VisualizationArea"));
        Assert.Empty(visualizationArea.ColumnDefinitions);

        section.SetInspectorModel(0, null);
        Assert.True(section.IsInspectorVisible);
        Assert.Equal(Visibility.Visible, band.Visibility);
        Assert.Null(first.Model);
        Assert.NotNull(second.Model);
    });

    [Fact]
    public Task VisualizerStylesFollowLightAndDarkThemes() => sta.Run(() =>
    {
        var oldTheme = ThemeManager.IsDark ? AppTheme.Dark : AppTheme.Light;
        try
        {
            var surface = new Border();
            surface.Resources.MergedDictionaries.Add(System.Windows.Application.Current.Resources);
            surface.Style = (Style)surface.FindResource("VisualizerSurface");
            ThemeManager.Apply(AppTheme.Light);
            var light = Assert.IsType<SolidColorBrush>(surface.Background).Color;
            ThemeManager.Apply(AppTheme.Dark);
            var dark = Assert.IsType<SolidColorBrush>(surface.Background).Color;

            Assert.NotEqual(light, dark);
            Assert.Equal(Assert.IsType<SolidColorBrush>(System.Windows.Application.Current.Resources["CardBrush"]).Color, dark);
        }
        finally
        {
            ThemeManager.Apply(oldTheme);
        }
    });

    [Fact]
    public Task HeaderUsesColoredMediaIconsAndCompactLocalizedMetadata() => sta.Run(() =>
    {
        var localization = LocalizationSource.Instance;
        var previous = (localization.Culture, localization.UiCulture);
        try
        {
            var french = CultureInfo.GetCultureInfo("fr-FR");
            localization.SetCultures(french, french);
            var header = new VisualizerHeaderSection();
            header.SetFormats(new BuiltInImageFormatCatalog(key => LocExtension.Get(key)).Formats);
            var descriptor = Descriptor(0);

            header.ApplyDetection(DiskImageFormatIds.Atari90, null, [DiskImageFormatIds.Atari90], includeFlux: false);
            header.DisplayDocument(Document("disk.atr", MediaKind.Floppy, DiskImageFormatIds.Atari90), descriptor);
            header.DocumentIdentityControl.Measure(new Size(double.PositiveInfinity, 80));
            Assert.Equal("floppy-5.25", header.MediaIconKind);
            Assert.Equal(
                LocExtension.Get("Explorer.DetectedFormats", "Atari 8-bit (Atari 8-bit — 90 KiB)"),
                header.DocumentIdentityControl.SummaryText.Text);
            Assert.Null(header.DocumentIdentityControl.FindName("Badges"));
            Assert.InRange(header.DocumentIdentityControl.DesiredSize.Width, 1, 800);
            header.DisplayDocument(Document("drive.hdf", MediaKind.HardDisk), descriptor);
            Assert.Equal("hard-disk", header.MediaIconKind);
            header.DisplayDocument(Document("disc.iso", MediaKind.Optical), descriptor);
            Assert.Equal("optical", header.MediaIconKind);
            header.DisplayDocument(Document("program.cas", MediaKind.Tape), descriptor);
            Assert.Equal("cassette", header.MediaIconKind);
            header.DisplayDocument(Document("archive.tap", MediaKind.Tape), descriptor);
            Assert.Equal("tape", header.MediaIconKind);
            header.DisplayDocument(Document("game.crt", MediaKind.Unknown), descriptor);
            Assert.Equal("cartridge", header.MediaIconKind);
        }
        finally
        {
            localization.SetCultures(previous.Culture, previous.UiCulture);
        }
    });

    [Fact]
    public Task EstablishedFloppyFormatsUseTheirValidatedPng() => sta.Run(() =>
    {
        var identity = new GWGUI.App.Views.Controls.Common.MediaDocumentIdentity();
        var catalog = new BuiltInImageFormatCatalog();
        var expected = new Dictionary<string, string>
        {
            ["amstrad.cpc"] = "floppy-3",
            ["ibm.720"] = "floppy-3.5-dd",
            ["ibm.1440"] = "floppy-3.5-hd",
            ["ibm.2880"] = "floppy-3.5-ed",
            ["atari.90"] = "floppy-5.25",
            ["dec.rx02"] = "floppy-8"
        };

        foreach (var (formatId, iconKind) in expected)
        {
            var format = Assert.Single(catalog.Formats, candidate => candidate.Id == formatId);
            identity.Display("disk.img", string.Empty, MediaKind.Floppy, format);
            Assert.Equal(iconKind, identity.MediaIconKind);
        }

    });

    [Fact]
    public Task HeaderControlsKeepTheirCoordinatesBetweenFluxAndSectors() => sta.Run(() =>
    {
        var header = new VisualizerHeaderSection();
        header.SetFormats(new BuiltInImageFormatCatalog(key => key).Formats);
        header.ApplyDetection(DiskImageFormatIds.AmigaDos, null, [DiskImageFormatIds.AmigaDos]);
        header.ConfigureRevolutions(3);
        var fluxDescriptor = new MediaVisualizationDescriptor(
            MediaRepresentationKind.Flux,
            [0, 1],
            MediaVisualizationProgressUnit.Track,
            MediaVisualizationDirection.Ascending,
            [new MediaVisualizationElement(0, 0), new MediaVisualizationElement(0, 1)]);
        header.DisplayDocument(Document("capture.scp"), fluxDescriptor);
        header.Measure(new Size(1900, 80));
        header.Arrange(new Rect(0, 0, 1900, 80));
        Assert.IsType<GWGUI.App.Views.Controls.Common.CardSection>(header.HeaderCardControl);
        Assert.IsType<GWGUI.App.Views.Controls.Common.CardSection>(new ExplorerSection().HeaderCardControl);
        var names = new[] { "RepresentationChoice", "RevolutionControl", "Reset", "Open" };
        var fluxPositions = names.ToDictionary(name => name, name =>
            Assert.IsAssignableFrom<FrameworkElement>(header.FindName(name)).TranslatePoint(new Point(), header).X);

        header.DisplayDocument(Document("capture.scp"), Descriptor(0, 1));
        header.Measure(new Size(1900, 80));
        header.Arrange(new Rect(0, 0, 1900, 80));

        Assert.Equal(Visibility.Hidden, Assert.IsAssignableFrom<FrameworkElement>(header.FindName("RevolutionControl")).Visibility);
        Assert.All(names, name => Assert.Equal(
            fluxPositions[name],
            Assert.IsAssignableFrom<FrameworkElement>(header.FindName(name)).TranslatePoint(new Point(), header).X));
    });

    [Fact]
    public Task InspectorAndCommandsExposeAccessibleNames() => sta.Run(() =>
    {
        var section = new VisualizerTabSection();
        section.SetInspectorModel(Model());
        var inspector = Assert.IsType<MediaInspectorPanel>(section.FindName("Face0Inspector"));

        Assert.Equal("Selected sector", AutomationProperties.GetName(inspector));
        Assert.Null(section.FindName("InspectorButton"));
        Assert.Null(section.FindName("DetachInspectorButton"));
    });

    [Fact]
    public Task SectorViewShowsBothFacesAndExpandsASingleFace() => sta.Run(() =>
    {
        var view = new SectorMediaView();
        var side0 = Assert.IsType<Border>(view.FindName("Side0Panel"));
        var side1 = Assert.IsType<Border>(view.FindName("Side1Panel"));
        var twoFaces = new SectorMediaRenderModel("test.720", 512, [Surface(0), Surface(1)]);
        view.SetDocument(twoFaces, Descriptor(0, 1));
        Assert.Equal(Visibility.Visible, side0.Visibility);
        Assert.Equal(Visibility.Visible, side1.Visibility);
        Assert.Equal(1, Grid.GetColumnSpan(side0));
        Assert.Equal(1, Grid.GetColumnSpan(side1));

        view.SetDocument(new SectorMediaRenderModel("test.single", 512, [Surface(0)]), Descriptor(0));
        Assert.Equal(Visibility.Visible, side0.Visibility);
        Assert.Equal(Visibility.Collapsed, side1.Visibility);
        Assert.Equal(2, Grid.GetColumnSpan(side0));
    });

    [Fact]
    public Task SectorViewKeepsCompleteGeometryWhileRevealingAnalyzedTracks() => sta.Run(() =>
    {
        var pending = new SectorMediaRenderModel("test.sectors", 512,
        [
            new SectorMediaSurface(0,
            [
                new SectorMediaTrack(0, [new SectorMediaElement(0, 0, 0, 1, 512, SectorMediaElementState.WithoutData)]),
                new SectorMediaTrack(1, [new SectorMediaElement(1, 1, 1, 1, 512, SectorMediaElementState.WithoutData)])
            ])
        ]);
        var analyzed = new SectorMediaTrack(0,
        [
            new SectorMediaElement(0, 0, 0, 1, 512, SectorMediaElementState.WithData)
        ]);
        var view = new SectorMediaView();

        view.SetDocument(pending, Descriptor(0));
        Assert.Equal(2, view.GeometryTrackCount);
        Assert.Equal(0, view.RevealedTrackCount);

        view.RevealTrack(0, analyzed);
        Assert.Equal(2, view.GeometryTrackCount);
        Assert.Equal(1, view.RevealedTrackCount);
    });

    [Fact]
    public Task OverviewTrackSelectionPublishesTheFirstSectorForItsInspector() => sta.Run(() =>
    {
        var first = new SectorMediaElement(17, 17, 4, 2, 512, SectorMediaElementState.WithData);
        var second = new SectorMediaElement(18, 18, 4, 3, 512, SectorMediaElementState.WithData);
        var view = new SectorMediaView();
        view.SetDocument(
            new SectorMediaRenderModel("test.720", 512,
            [
                new SectorMediaSurface(0, [new SectorMediaTrack(4, [first, second])]),
                new SectorMediaSurface(1, [new SectorMediaTrack(4, [second])])
            ]),
            Descriptor(0, 1));
        var selections = new List<(int Surface, SectorMediaElement? Sector)>();
        view.SectorSelected += (surface, sector) => selections.Add((surface, sector));

        view.SelectElement(0, 4);
        view.SelectElement(1, 4);

        Assert.Collection(
            selections,
            selection => { Assert.Equal(0, selection.Surface); Assert.Same(first, selection.Sector); },
            selection => { Assert.Equal(1, selection.Surface); Assert.Equal(4, selection.Sector?.Cylinder); Assert.Equal(3, selection.Sector?.Number); });
    });

    [Fact]
    public void UnrevealedSectorSupportIsDarkMagneticMedia()
    {
        var support = SkiaSectorMediaRenderer.UnrevealedColor;
        var boundary = SkiaSectorMediaRenderer.UnrevealedTrackBoundaryColor;

        Assert.Equal((byte)43, support.Red);
        Assert.Equal((byte)32, support.Green);
        Assert.Equal((byte)25, support.Blue);
        Assert.True(boundary.Red < support.Red);
        Assert.True(boundary.Green < support.Green);
        Assert.True(boundary.Blue < support.Blue);
        Assert.NotEqual(support, SkiaSectorMediaRenderer.ColorFor(SectorMediaElementState.WithoutData));
        Assert.NotEqual(support, SkiaSectorMediaRenderer.ColorFor(SectorMediaElementState.WithData));
    }

    [Fact]
    public void SectorHitTestUsesTheRenderedPixelCoordinates()
    {
        var sectors = Enumerable.Range(0, 4)
            .Select(number => new SectorMediaElement(number, number, 0, number, 512, SectorMediaElementState.WithData))
            .ToArray();
        var model = new SectorMediaRenderModel("test.720", 512,
            [new SectorMediaSurface(0, [new SectorMediaTrack(0, sectors)])]);
        var point = SectorMediaView.MapPointerToRender(new Point(300, 50), 600, 400, 1200, 800);

        var selected = new SkiaSectorMediaRenderer().HitTest(
            model, 0, MediaVisualizationDirection.Ascending, 1200, 800, point);

        Assert.NotNull(selected);
        Assert.Equal(0, selected.Number);
    }

    [Fact]
    public Task FluxTrackDefaultsToTheSynthesisAndAllowsOneRevolution() => sta.Run(() =>
    {
        var source = ExplorerDocumentScenarios.Document("flux").ScpImage!;
        var track = new ScpTrack(0, 12, 0,
        [
            new ScpRevolution(8_000_000, 2, [80, 160]),
            new ScpRevolution(8_000_000, 2, [90, 150])
        ]);
        var image = new ScpImage(source.Header, [track], true, source.FileSize);
        var section = new VisualizerTabSection();
        section.FirstSide.SetImage(image, 0);
        section.SecondSide.SetImage(image, 1);
        section.Header.ConfigureRevolutions(2);

        Assert.Null(section.FirstSide.CreateRenderRequest(800, 600).SelectedRevolutionIndex);
        Assert.Null(section.SecondSide.CreateRenderRequest(800, 600).SelectedRevolutionIndex);
        var selector = section.Header.RevolutionCombo;
        Assert.Equal(Visibility.Visible, selector.Parent is FrameworkElement parent ? parent.Visibility : Visibility.Collapsed);
        Assert.Equal(3, selector.Items.Count);
        Assert.Equal(0, selector.SelectedIndex);

        selector.SelectedIndex = 2;
        Assert.Equal(1, section.FirstSide.CreateRenderRequest(800, 600).SelectedRevolutionIndex);
        Assert.Equal(1, section.SecondSide.CreateRenderRequest(800, 600).SelectedRevolutionIndex);
    });

    [Fact]
    public async Task PreparedFluxTracksStayHiddenUntilExplicitlyRevealed()
    {
        var source = ExplorerDocumentScenarios.Document("flux").ScpImage!;
        var track = new ScpTrack(0, 0, 0,
        [
            new ScpRevolution(8_000_000, 3, [80, 120, 160])
        ]);
        var image = new ScpImage(source.Header, [track], true, source.FileSize);
        var renderer = new SkiaScpRenderer();

        await renderer.PrepareAsync(image, 0);
        Assert.Equal(0, renderer.RevealedTrackCount);

        renderer.RevealTrack(track.Cylinder);
        Assert.Equal(1, renderer.RevealedTrackCount);
    }

    [Fact]
    public async Task SelectedFluxRevolutionChangesEveryVisibleTrackWithoutSelectingIt()
    {
        var source = ExplorerDocumentScenarios.Document("flux").ScpImage!;
        var track = new ScpTrack(0, 0, 0,
        [
            new ScpRevolution(8_000_000, 5, [100, 100, 100, 100, 100]),
            new ScpRevolution(8_000_000, 5, [10, 100, 10, 100, 100]),
            new ScpRevolution(8_000_000, 5, [100, 100, 100, 100, 1000])
        ]);
        var image = new ScpImage(source.Header, [track], true, source.FileSize);
        var renderer = new SkiaScpRenderer();
        await renderer.PrepareAsync(image, 0);
        renderer.RevealTrack(0);

        SKBitmap Render(int revolution)
        {
            var bitmap = new SKBitmap(320, 320);
            using var canvas = new SKCanvas(bitmap);
            renderer.Render(canvas, new ScpRenderRequest(image, 0, null, revolution, 320, 320,
                new SKPoint(160, 160), 1, "No data", "Side 0", DiskMediaCategory.ThreeHalfDd));
            return bitmap;
        }

        using var first = Render(0);
        using var second = Render(1);
        using var third = Render(2);
        static int Difference(SKBitmap left, SKBitmap right)
        {
            var count = 0;
            for (var y = 0; y < left.Height; y++)
            for (var x = 0; x < left.Width; x++)
                if (left.GetPixel(x, y) != right.GetPixel(x, y)) count++;
            return count;
        }

        Assert.True(Difference(first, second) > 20);
        Assert.True(Difference(first, third) > 20);
        Assert.True(Difference(second, third) > 20);
    }

    [Fact]
    public async Task FluxSynthesisRatesAgreementBetweenRevolutionsIndependentlyOfTheSelectedDecoder()
    {
        var source = ExplorerDocumentScenarios.Document("flux").ScpImage!;
        static uint[] Flux(int count) => Enumerable.Range(0, count)
            .Select(index => (uint)(index % 3 switch { 0 => 80, 1 => 120, _ => 160 }))
            .ToArray();
        var track = new ScpTrack(0, 0, 0,
        [
            new ScpRevolution(8_000_000, 720, Flux(720)),
            new ScpRevolution(8_020_000, 718, Flux(718)),
            new ScpRevolution(7_990_000, 722, Flux(722))
        ]);
        var image = new ScpImage(source.Header, [track], true, source.FileSize);
        var reports = new List<ScpTrackPreparation>();

        await new SkiaScpRenderer { DecoderId = "amiga.mfm" }
            .PrepareAsync(image, 0, new InlineProgress<ScpTrackPreparation>(reports.Add));

        var result = Assert.Single(reports);
        Assert.True(result.Quality > .95, $"Expected readable revolutions, observed quality {result.Quality:P1}.");
    }

    [Fact]
    public Task FluxAndSectorViewsUseConsistentHeadersAndLinkedZoom() => sta.Run(() =>
    {
        var flux = new ScpDiskView();
        var fluxCanvas = Assert.IsType<SkiaSharp.Views.WPF.SKElement>(flux.FindName("Canvas"));
        var fluxHeader = Assert.IsType<Border>(flux.FindName("SurfaceHeader"));
        Assert.Equal(1, Grid.GetRow(fluxCanvas));
        Assert.Null(flux.FindName("RevolutionSelector"));
        Assert.EndsWith("—", Assert.IsType<TextBlock>(flux.FindName("SelectionLabel")).Text);

        var sectors = new SectorMediaView();
        sectors.SetDocument(new SectorMediaRenderModel("test.720", 512, [Surface(0), Surface(1)]), Descriptor(0, 1));
        var sectorHeader = Assert.IsType<Border>(sectors.FindName("Side0Header"));
        Assert.Contains("—", Assert.IsType<TextBlock>(sectors.FindName("Selection0Label")).Text);
        flux.Measure(new Size(900, 600));
        flux.Arrange(new Rect(0, 0, 900, 600));
        sectors.Measure(new Size(900, 600));
        sectors.Arrange(new Rect(0, 0, 900, 600));
        Assert.Equal(fluxHeader.ActualHeight, sectorHeader.ActualHeight);
        var setZoom = typeof(SectorMediaView).GetMethod("SetZoom", BindingFlags.Instance | BindingFlags.NonPublic)!;
        setZoom.Invoke(sectors, [0, 1.5f]);
        Assert.Equal("150 %", Assert.IsType<Button>(sectors.FindName("ResetZoom0Button")).Content);
        Assert.Equal("150 %", Assert.IsType<Button>(sectors.FindName("ResetZoom1Button")).Content);

        sectors.LinkZoom = false;
        setZoom.Invoke(sectors, [0, 2f]);
        Assert.Equal("200 %", Assert.IsType<Button>(sectors.FindName("ResetZoom0Button")).Content);
        Assert.Equal("150 %", Assert.IsType<Button>(sectors.FindName("ResetZoom1Button")).Content);
    });

    [Fact]
    public void FluxQualityAndSectorStatesUseDistinctColors()
    {
        Assert.Equal(new SKColor(190, 55, 62), SkiaScpRenderer.QualityColor(0));
        Assert.Equal(new SKColor(211, 113, 48), SkiaScpRenderer.QualityColor(.3));
        Assert.Equal(new SKColor(132, 118, 57), SkiaScpRenderer.QualityColor(.6));
        Assert.Equal(new SKColor(63, 116, 72), SkiaScpRenderer.QualityColor(.8));
        Assert.Equal(new SKColor(47, 166, 91), SkiaScpRenderer.QualityColor(1));

        var colors = Enum.GetValues<SectorMediaElementState>()
            .Select(SkiaSectorMediaRenderer.ColorFor)
            .Distinct()
            .ToArray();
        Assert.Equal(Enum.GetValues<SectorMediaElementState>().Length, colors.Length);
    }

    [Fact]
    public void FluxBackgroundNoLongerDrawsAFloppyShell()
    {
        using var bitmap = new SKBitmap(320, 220);
        using var canvas = new SKCanvas(bitmap);
        new SkiaScpRenderer().Render(canvas, new ScpRenderRequest(
            null, 0, null, null, bitmap.Width, bitmap.Height, new SKPoint(160, 110), 1,
            "No data", "Side 0", DiskMediaCategory.ThreeHalfDd));

        var corner = bitmap.GetPixel(8, 8);
        Assert.True(corner.Red > 225 && corner.Green > 230 && corner.Blue > 235);
    }

    [Fact]
    public Task OverviewImmediatelyUsesFluxAndSectorQualityColors() => sta.Run(() =>
    {
        var overview = new VisualizerTrackOverview();
        overview.Configure(new Dictionary<int, IReadOnlyList<int>> { [0] = [0, 1] });
        overview.MarkPrepared(new ScpTrackPreparation(0, 0, ScpTrackVisualState.NormalFlux, HasFlux: true, Quality: 1));
        overview.MarkPrepared(new ScpTrackPreparation(1, 0, ScpTrackVisualState.Anomaly, HasFlux: true, Quality: .3));

        var rows = Assert.IsType<StackPanel>(overview.FindName("ProgressRows"));
        var fluxStrip = Assert.IsType<TrackProgressStrip>(Assert.Single(rows.Children));
        Assert.Equal(Color.FromRgb(47, 166, 91), Assert.IsType<SolidColorBrush>(fluxStrip.Segments[0].Brush).Color);
        Assert.Equal(Color.FromRgb(211, 113, 48), Assert.IsType<SolidColorBrush>(fluxStrip.Segments[1].Brush).Color);

        var sectorModel = new SectorMediaRenderModel("test.720", 512,
        [
            new SectorMediaSurface(0,
            [
                new SectorMediaTrack(0, [new SectorMediaElement(0, 0, 0, 1, 512, SectorMediaElementState.WithData)]),
                new SectorMediaTrack(1, [new SectorMediaElement(1, 1, 1, 1, 512, SectorMediaElementState.Dead)]),
                new SectorMediaTrack(2, [new SectorMediaElement(2, 2, 2, 1, 512, SectorMediaElementState.WithoutData)])
            ])
        ]);
        overview.Configure(new MediaVisualizationDescriptor(
            MediaRepresentationKind.Sectors,
            [0],
            MediaVisualizationProgressUnit.Track,
            MediaVisualizationDirection.Ascending,
            [new MediaVisualizationElement(0, 0), new MediaVisualizationElement(1, 0), new MediaVisualizationElement(2, 0)]));
        overview.MarkSectors(sectorModel);
        var sectorStrip = Assert.IsType<TrackProgressStrip>(Assert.Single(rows.Children));
        Assert.Equal(Color.FromRgb(45, 176, 100), Assert.IsType<SolidColorBrush>(sectorStrip.Segments[0].Brush).Color);
        Assert.Equal(Color.FromRgb(207, 67, 67), Assert.IsType<SolidColorBrush>(sectorStrip.Segments[1].Brush).Color);
        Assert.Equal(Color.FromRgb(74, 83, 94), Assert.IsType<SolidColorBrush>(sectorStrip.Segments[2].Brush).Color);
    });

    private static SectorMediaSurface Surface(int index) => new(index,
        [new SectorMediaTrack(0, [new SectorMediaElement(index, index, 0, 1, 512, SectorMediaElementState.WithData)])]);

    private static MediaVisualizationDescriptor Descriptor(params int[] surfaces) => new(
        MediaRepresentationKind.Sectors,
        surfaces,
        MediaVisualizationProgressUnit.Sector,
        MediaVisualizationDirection.Ascending,
        surfaces.Select(surface => new MediaVisualizationElement(0, surface)).ToArray());

    private static MediaInspectorModel Model(string selectedElement = "Track 0 · Sector 1") => new(
        "Selected sector",
        selectedElement,
        [new MediaInspectorSection("Summary", "i", [new MediaInspectorEntry("State", "Available")])]);

    private static MediaImageDocument Document(
        string path = "layout.img",
        MediaKind mediaKind = MediaKind.Floppy,
        string formatId = "test.720") => new(
        new MediaSourceDescriptor(path, []), formatId, mediaKind,
        new TestSectorRepresentation(), [], [], new Dictionary<string, string>());

    private sealed class TestSectorRepresentation : IMediaImageRepresentation
    {
        public MediaRepresentationKind RepresentationKind => MediaRepresentationKind.Sectors;
        public long? LogicalLength => 720 * 1024;
        public bool SupportsRandomAccess => true;
        public bool SupportsSequentialAccess => true;
    }

    private sealed class InlineProgress<T>(Action<T> report) : IProgress<T>
    {
        public void Report(T value) => report(value);
    }
}
