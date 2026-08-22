using GWGUI.App.Contracts.Rendering.Scp;
using GWGUI.App.Enums.Rendering.Scp;
using GWGUI.App.Localization.Extensions;
using System.Windows.Controls;
using System.Windows.Media;

namespace GWGUI.App.Views.Controls.Visualization;

public partial class VisualizerTrackOverview : UserControl
{
    private readonly Dictionary<int, HashSet<int>> _preparedCylinders = [];

    public VisualizerTrackOverview() => InitializeComponent();

    public void Configure(IReadOnlyDictionary<int, IReadOnlyList<int>> cylinders)
    {
        Visibility = cylinders.Values.Any(items => items.Count > 0) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        _preparedCylinders.Clear();
        foreach (var head in cylinders.Keys)
            _preparedCylinders[head] = [];
        ConfigureFace(Face0, Face0Count, 0, cylinders.GetValueOrDefault(0) ?? []);
        ConfigureFace(Face1, Face1Count, 1, cylinders.GetValueOrDefault(1) ?? []);
    }

    public void MarkPrepared(ScpTrackPreparation preparation)
    {
        var strip = preparation.Head == 0 ? Face0 : Face1;
        var label = preparation.Head == 0 ? Face0Count : Face1Count;
        strip.SetColor(preparation.Cylinder, ColorFor(preparation));
        if (!_preparedCylinders.TryGetValue(preparation.Head, out var prepared))
            _preparedCylinders[preparation.Head] = prepared = [];
        prepared.Add(preparation.Cylinder);
        label.Text = $"{Math.Min(prepared.Count, strip.Segments.Count)} / {strip.Segments.Count}";
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

    private static void ConfigureFace(TrackProgressStrip strip, TextBlock label, int head, IReadOnlyList<int> cylinders)
    {
        strip.Visibility = cylinders.Count == 0 ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
        label.Visibility = strip.Visibility;
        strip.Configure(head, cylinders, LocExtension.Get("Visual.Side", head));
        label.Text = $"0 / {cylinders.Count}";
    }
}
