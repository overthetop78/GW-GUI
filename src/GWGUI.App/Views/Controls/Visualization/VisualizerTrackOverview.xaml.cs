using GWGUI.App.Contracts.Rendering.Scp;
using GWGUI.App.Enums.Rendering.Scp;
using GWGUI.App.Localization.Extensions;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Visualization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GWGUI.App.Views.Controls.Visualization;

public partial class VisualizerTrackOverview : UserControl
{
    private readonly Dictionary<int, TrackProgressStrip> _strips = [];

    public VisualizerTrackOverview() => InitializeComponent();

    public event Action<int, long>? ElementSelected;

    public void Configure(IReadOnlyDictionary<int, IReadOnlyList<int>> cylinders)
    {
        ArgumentNullException.ThrowIfNull(cylinders);
        var surfaces = cylinders.Keys.Order().ToArray();
        var elements = cylinders
            .SelectMany(pair => pair.Value.Select(cylinder => new MediaVisualizationElement(cylinder, pair.Key)))
            .ToArray();
        Configure(new MediaVisualizationDescriptor(
            MediaRepresentationKind.Flux,
            surfaces,
            MediaVisualizationProgressUnit.Track,
            MediaVisualizationDirection.Ascending,
            elements));
    }

    public void Configure(MediaVisualizationDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ProgressRows.Children.Clear();
        _strips.Clear();

        var surfaces = descriptor.Surfaces.Count > 0
            ? descriptor.Surfaces
            : descriptor.Elements.Where(element => element.Surface.HasValue).Select(element => element.Surface!.Value).Distinct().Order().ToArray();

        foreach (var surface in surfaces)
        {
            var positions = descriptor.Elements
                .Where(element => element.Surface == surface)
                .Select(element => element.Position)
                .Distinct()
                .Order()
                .ToArray();
            AddStrip(descriptor.ProgressUnit, surface, positions, SurfaceLabel(descriptor.RepresentationKind, surface));
        }

        var unassignedPositions = descriptor.Elements
            .Where(element => element.Surface is null)
            .Select(element => element.Position)
            .Distinct()
            .Order()
            .ToArray();
        if (unassignedPositions.Length > 0)
        {
            var unassignedSurface = surfaces.DefaultIfEmpty(-1).Max() + 1;
            AddStrip(descriptor.ProgressUnit, unassignedSurface, unassignedPositions, LocExtension.Get("Visual.All"));
        }

        Visibility = _strips.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void AddStrip(MediaVisualizationProgressUnit progressUnit, int surface, IReadOnlyList<long> positions, string label)
    {
        if (positions.Count == 0) return;
        var strip = new TrackProgressStrip();
        strip.Configure(progressUnit, surface, positions, label);
        strip.ElementSelected += HandleElementSelected;
        ProgressRows.Children.Add(strip);
        _strips[surface] = strip;
    }

    public void SelectElement(int surface, long position)
    {
        foreach (var pair in _strips)
            pair.Value.Select(pair.Key == surface ? position : long.MinValue);
    }

    public void MarkPrepared(ScpTrackPreparation preparation)
    {
        if (_strips.TryGetValue(preparation.Head, out var strip))
            strip.SetColor(preparation.Cylinder, ColorFor(preparation));
    }

    internal static Color ColorFor(ScpTrackPreparation preparation)
    {
        if (!preparation.HasFlux)
            return Color.FromRgb(255, 75, 96);

        var sectorCount = preparation.ValidSectors + preparation.InvalidSectors + preparation.UnverifiedSectors;
        if (sectorCount > 0)
        {
            var unreadableRatio = (preparation.InvalidSectors + preparation.UnverifiedSectors * .25) / sectorCount;
            if (unreadableRatio == 0) return Color.FromRgb(36, 179, 93);
            if (unreadableRatio <= .10) return Color.FromRgb(100, 201, 107);
            if (unreadableRatio <= .25) return Color.FromRgb(67, 220, 255);
            if (unreadableRatio <= .40) return Color.FromRgb(83, 173, 255);
            if (unreadableRatio <= .60) return Color.FromRgb(255, 205, 64);
            if (preparation.ValidSectors > 0) return Color.FromRgb(245, 158, 61);
            return Color.FromRgb(255, 75, 96);
        }

        return preparation.State switch
        {
            ScpTrackVisualState.ShortTransition => Color.FromRgb(143, 104, 255),
            ScpTrackVisualState.LongTransition => Color.FromRgb(83, 173, 255),
            ScpTrackVisualState.Header => Color.FromRgb(255, 205, 64),
            ScpTrackVisualState.DecodedData => Color.FromRgb(67, 220, 255),
            ScpTrackVisualState.Anomaly => Color.FromRgb(255, 75, 96),
            _ => Color.FromRgb(36, 179, 93)
        };
    }

    private void HandleElementSelected(int surface, long position)
    {
        SelectElement(surface, position);
        ElementSelected?.Invoke(surface, position);
    }

    private static string SurfaceLabel(MediaRepresentationKind representationKind, int surface) => representationKind switch
    {
        MediaRepresentationKind.Blocks => LocExtension.Get("Visual.Surface", surface),
        MediaRepresentationKind.OpticalTracks => LocExtension.Get("Visual.DiscFace", surface),
        MediaRepresentationKind.Sequential => LocExtension.Get("Visual.Channel", surface),
        _ => LocExtension.Get("Visual.Side", surface)
    };
}
