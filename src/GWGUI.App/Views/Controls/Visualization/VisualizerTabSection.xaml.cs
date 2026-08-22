using System.Windows;
using System.Windows.Controls;

namespace GWGUI.App.Views.Controls.Visualization;

public partial class VisualizerTabSection : UserControl
{
    public VisualizerTabSection()
    {
        InitializeComponent();
        InspectorButton.Click += (_, e) => ToggleInspectorRequested?.Invoke(this, e);
    }

    public event RoutedEventHandler? ToggleInspectorRequested;

    public VisualizerHeaderSection Header => HeaderSection;
    public ScpDiskView FirstSide => Side0;
    public ScpDiskView SecondSide => Side1;
    public Canvas InspectorCanvas => InspectorLayer;
    public ScpInspectorPanel Inspector => InspectorPanel;
    public VisualizerTrackOverview Overview => TrackOverview;
}
