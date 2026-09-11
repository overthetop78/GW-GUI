using GWGUI.App.Contracts.ViewModels.Visualization;
using GWGUI.App.Contracts.Views.Visualization;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Visualization;
using System.Windows;
using System.Windows.Controls;

namespace GWGUI.App.Views.Controls.Visualization;

public partial class VisualizerTabSection : UserControl
{
    private readonly Dictionary<MediaRepresentationKind, FrameworkElement> representationViews = [];

    public VisualizerTabSection()
    {
        InitializeComponent();
        TrackOverview.ElementSelected += HandleOverviewElementSelected;
        Side0.ElementSelected += HandleViewElementSelected;
        Side1.ElementSelected += HandleViewElementSelected;
        RegisterRepresentationView(MediaRepresentationKind.Flux, FluxViewHost);
        InspectorButton.Click += (_, e) => ToggleInspectorRequested?.Invoke(this, e);
        DetachInspectorButton.Click += (_, e) => DetachInspectorRequested?.Invoke(this, e);
    }

    public event RoutedEventHandler? ToggleInspectorRequested;
    public event RoutedEventHandler? DetachInspectorRequested;

    public VisualizerHeaderSection Header => HeaderSection;
    public ScpDiskView FirstSide => Side0;
    public ScpDiskView SecondSide => Side1;
    public MediaInspectorPanel CommonInspector => MediaInspector;
    public VisualizerTrackOverview Overview => TrackOverview;

    public MediaRepresentationKind? ActiveRepresentationKind { get; private set; }
    public MediaImageDocument? CurrentDocument { get; private set; }
    public MediaVisualizationDescriptor? CurrentDescriptor { get; private set; }
    public bool IsInspectorVisible => InspectorHost.Visibility == Visibility.Visible;

    public void RegisterRepresentationView(MediaRepresentationKind representationKind, FrameworkElement view)
    {
        ArgumentNullException.ThrowIfNull(view);
        if (representationViews.TryGetValue(representationKind, out var previous) && !ReferenceEquals(previous, view))
        {
            if (previous is IMediaVisualizationView previousVisualizationView)
                previousVisualizationView.ElementSelected -= HandleViewElementSelected;
            MediaViewHost.Children.Remove(previous);
        }

        representationViews[representationKind] = view;
        if (view is IMediaVisualizationView visualizationView)
        {
            visualizationView.ElementSelected -= HandleViewElementSelected;
            visualizationView.ElementSelected += HandleViewElementSelected;
        }
        if (!MediaViewHost.Children.Contains(view)) MediaViewHost.Children.Add(view);
        if (CurrentDocument?.Representation.RepresentationKind == representationKind)
        {
            ActivateRepresentationView(representationKind, view);
            return;
        }

        if (ActiveRepresentationKind is null) ActiveRepresentationKind = representationKind;
        view.Visibility = ActiveRepresentationKind == representationKind ? Visibility.Visible : Visibility.Collapsed;
    }

    public bool ShowDocument(
        MediaImageDocument document,
        MediaVisualizationDescriptor descriptor,
        MediaInspectorModel? inspectorModel = null)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(descriptor);
        var representationKind = document.Representation.RepresentationKind;
        if (descriptor.RepresentationKind != representationKind)
            throw new ArgumentException("The visualization descriptor must describe the document representation.", nameof(descriptor));

        CurrentDocument = document;
        CurrentDescriptor = descriptor;
        HeaderSection.DisplayDocument(document, descriptor);
        Legend.Configure(descriptor);
        TrackOverview.Configure(descriptor);
        SetInspectorModel(inspectorModel);

        if (!representationViews.TryGetValue(representationKind, out var selectedView)) return false;
        ActivateRepresentationView(representationKind, selectedView);
        return true;
    }

    public void SetInspectorModel(MediaInspectorModel? inspectorModel)
    {
        MediaInspector.Model = inspectorModel;
        var showInspector = inspectorModel is not null;
        InspectorHost.Visibility = showInspector ? Visibility.Visible : Visibility.Collapsed;
        InspectorSplitter.Visibility = showInspector ? Visibility.Visible : Visibility.Collapsed;
        InspectorSplitterColumn.Width = showInspector ? new GridLength(6) : new GridLength(0);
        InspectorColumn.Width = showInspector ? new GridLength(340) : new GridLength(0);
    }

    private void ActivateRepresentationView(MediaRepresentationKind representationKind, FrameworkElement selectedView)
    {
        foreach (var view in representationViews.Values)
            view.Visibility = ReferenceEquals(view, selectedView) ? Visibility.Visible : Visibility.Collapsed;
        ActiveRepresentationKind = representationKind;
    }

    private void HandleOverviewElementSelected(int surface, long position)
    {
        if (ActiveRepresentationKind == MediaRepresentationKind.Flux)
        {
            (surface == 0 ? Side0 : Side1).SelectElement(surface, position);
            return;
        }

        if (ActiveRepresentationKind is { } representationKind
            && representationViews.GetValueOrDefault(representationKind) is IMediaVisualizationView view)
            view.SelectElement(surface, position);
    }

    private void HandleViewElementSelected(int surface, long position) =>
        TrackOverview.SelectElement(surface, position);
}
