using GWGUI.App.Contracts.Rendering.Scp;
using GWGUI.App.Contracts.Rendering.Sectors;
using GWGUI.App.Enums.Rendering.Sectors;
using GWGUI.App.Enums.Rendering.Scp;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Rendering.Scp;
using GWGUI.App.Rendering.Sectors;
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

    public void MarkSectors(SectorMediaRenderModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        foreach (var surface in model.Surfaces)
        {
            if (!_strips.TryGetValue(surface.Index, out var strip)) continue;
            foreach (var track in surface.Tracks)
                strip.SetColor(track.Cylinder, SectorColor(track.Sectors));
        }
    }

    public void MarkSectorTrack(int surface, SectorMediaTrack track)
    {
        ArgumentNullException.ThrowIfNull(track);
        if (_strips.TryGetValue(surface, out var strip))
            strip.SetColor(track.Cylinder, SectorColor(track.Sectors));
    }

    private static Color SectorColor(IReadOnlyList<SectorMediaElement> sectors)
    {
        var state = sectors.Count == 0
            ? SectorMediaElementState.WithoutData
            : sectors.Any(sector => sector.State == SectorMediaElementState.Dead)
                ? SectorMediaElementState.Dead
                : sectors.Any(sector => sector.State == SectorMediaElementState.Degraded)
                    ? SectorMediaElementState.Degraded
                    : sectors.Any(sector => sector.State == SectorMediaElementState.WithData)
                        ? SectorMediaElementState.WithData
                        : SectorMediaElementState.WithoutData;
        var color = SkiaSectorMediaRenderer.ColorFor(state);
        return Color.FromRgb(color.Red, color.Green, color.Blue);
    }

    internal static Color ColorFor(ScpTrackPreparation preparation)
    {
        if (!preparation.HasFlux)
            return Color.FromRgb(190, 55, 62);
        var color = SkiaScpRenderer.QualityColor(preparation.Quality);
        return Color.FromRgb(color.Red, color.Green, color.Blue);
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
