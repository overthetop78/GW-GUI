using GWGUI.App.Contracts.Views.Visualization;
using GWGUI.App.Contracts.ViewModels.Visualization;
using GWGUI.App.Views.Controls.Visualization;
using MediaSourceDescriptor = global::GWGUI.MediaEngine.Contracts.MediaSourceDescriptor;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Contracts;

using IMediaImageRepresentation = global::GWGUI.MediaEngine.Interfaces.IMediaImageRepresentation;
using GWGUI.MediaEngine.Images.Visualization;
using GWGUI.Tests.Application.TestInfrastructure;
using System.Windows;
using System.Windows.Controls;

namespace GWGUI.Tests.Interface.VisualizerViews;

[Collection("WPF")]
public sealed class MediaVisualizationRoutingTests(StaExecutionScenarios sta)
{
    [Theory]
    [InlineData(MediaRepresentationKind.Flux)]
    [InlineData(MediaRepresentationKind.Sectors)]
    [InlineData(MediaRepresentationKind.Blocks)]
    [InlineData(MediaRepresentationKind.OpticalTracks)]
    [InlineData(MediaRepresentationKind.Sequential)]
    public Task RepresentationSelectsItsRegisteredView(MediaRepresentationKind kind) => sta.Run(() =>
    {
        var section = new VisualizerTabSection();
        var views = Enum.GetValues<MediaRepresentationKind>()
            .ToDictionary(value => value, _ => new RoutedView());
        foreach (var pair in views) section.RegisterRepresentationView(pair.Key, pair.Value);

        var document = Document(kind, "same-extension.image");
        var descriptor = new MediaVisualizationDescriptor(
            kind,
            [0],
            ProgressUnit(kind),
            MediaVisualizationDirection.Ascending,
            [new MediaVisualizationElement(0, 0)]);

        Assert.True(section.ShowDocument(document, descriptor, Inspector(kind)));
        Assert.Equal(kind, section.ActiveRepresentationKind);
        Assert.Same(document, section.CurrentDocument);
        Assert.Same(descriptor, section.CurrentDescriptor);
        Assert.All(views, pair => Assert.Equal(
            pair.Key == kind ? Visibility.Visible : Visibility.Collapsed,
            pair.Value.Visibility));
    });

    private static MediaImageDocument Document(MediaRepresentationKind kind, string path) => new(
        new MediaSourceDescriptor(path, []),
        "test.format",
        MediaKind.Unknown,
        new Representation(kind),
        [],
        [],
        new Dictionary<string, string>());

    private static MediaInspectorModel Inspector(MediaRepresentationKind kind) =>
        new(kind.ToString(), null, []);

    private static MediaVisualizationProgressUnit ProgressUnit(MediaRepresentationKind kind) => kind switch
    {
        MediaRepresentationKind.Flux => MediaVisualizationProgressUnit.Track,
        MediaRepresentationKind.Sectors => MediaVisualizationProgressUnit.Sector,
        MediaRepresentationKind.Blocks => MediaVisualizationProgressUnit.BlockRange,
        MediaRepresentationKind.OpticalTracks => MediaVisualizationProgressUnit.OpticalTrack,
        MediaRepresentationKind.Sequential => MediaVisualizationProgressUnit.Segment,
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    private sealed record Representation(MediaRepresentationKind RepresentationKind) : IMediaImageRepresentation
    {
        public long? LogicalLength => null;
        public bool SupportsRandomAccess => true;
        public bool SupportsSequentialAccess => true;
    }

    private sealed class RoutedView : ContentControl, IMediaVisualizationView
    {
        public event Action<int, long>? ElementSelected;
        public void SelectElement(int surface, long position) => ElementSelected?.Invoke(surface, position);
    }
}
