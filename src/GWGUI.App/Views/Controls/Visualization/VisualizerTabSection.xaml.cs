using GWGUI.App.Contracts.ViewModels.Visualization;
using GWGUI.App.Contracts.Views.Visualization;
using GWGUI.App.Localization.Extensions;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Visualization;
using System.Windows;
using System.Windows.Controls;

namespace GWGUI.App.Views.Controls.Visualization;

public partial class VisualizerTabSection : UserControl
{
    private readonly Dictionary<MediaRepresentationKind, FrameworkElement> representationViews = [];
    private readonly Dictionary<MediaRepresentationKind, (MediaImageDocument Document, MediaVisualizationDescriptor Descriptor)> representationDocuments = [];

    public VisualizerTabSection()
    {
        InitializeComponent();
        TrackOverview.ElementSelected += HandleOverviewElementSelected;
        Side0.ElementSelected += HandleViewElementSelected;
        Side1.ElementSelected += HandleViewElementSelected;
        RegisterRepresentationView(MediaRepresentationKind.Flux, FluxViewHost);
        HeaderSection.LinkZoomCheckBox.Checked += LinkZoomChanged;
        HeaderSection.LinkZoomCheckBox.Unchecked += LinkZoomChanged;
        HeaderSection.RevolutionChoiceChanged += revolution =>
        {
            Side0.SetRevolution(revolution);
            Side1.SetRevolution(revolution);
        };
        Face0Inspector.SurfaceTitle = LocExtension.Get("Visual.Side", 0);
        Face1Inspector.SurfaceTitle = LocExtension.Get("Visual.Side", 1);
        HeaderSection.OpenButton.Click += (_, args) => OpenRequested?.Invoke(this, args);
        EmptyOpenButton.Click += (_, args) => OpenRequested?.Invoke(this, args);
    }

    public event RoutedEventHandler? OpenRequested;

    public VisualizerHeaderSection Header => HeaderSection;
    public ScpDiskView FirstSide => Side0;
    public ScpDiskView SecondSide => Side1;
    public MediaInspectorPanel CommonInspector => Face0Inspector;
    public MediaInspectorPanel FirstInspector => Face0Inspector;
    public MediaInspectorPanel SecondInspector => Face1Inspector;
    public VisualizerTrackOverview Overview => TrackOverview;

    public void SetRecognitionProgress(bool visible, string stage, string detail, double value, bool indeterminate = false)
    {
        RecognitionCard.SetProgress(stage, detail, value, indeterminate);
        RecognitionOverlay.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
    }

    public MediaRepresentationKind? ActiveRepresentationKind { get; private set; }
    public MediaImageDocument? CurrentDocument { get; private set; }
    public MediaVisualizationDescriptor? CurrentDescriptor { get; private set; }
    public bool IsInspectorVisible => InspectorBand.Visibility == Visibility.Visible;
    internal Button EmptyOpenImageButton => EmptyOpenButton;
    internal bool RecognitionProgressVisible => RecognitionOverlay.Visibility == Visibility.Visible;
    internal string RecognitionStage => RecognitionCard.Stage;
    internal string RecognitionDetail => RecognitionCard.Detail;
    internal double RecognitionValue => RecognitionCard.Value;

    public void ClearDocumentForLoading()
    {
        CurrentDocument = null;
        CurrentDescriptor = null;
        ActiveRepresentationKind = null;
        representationDocuments.Clear();
        foreach (var view in representationViews.Values) view.Visibility = Visibility.Collapsed;
        MediaSurface.Visibility = Visibility.Collapsed;
        EmptyState.Visibility = Visibility.Collapsed;
        InspectorBand.Visibility = Visibility.Collapsed;
        OverviewBar.Visibility = Visibility.Collapsed;
        ClearInspectorModels();
    }

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
        if (view is SectorMediaView sectorMediaView)
            sectorMediaView.LinkZoom = HeaderSection.LinkZoomCheckBox.IsChecked == true;
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

        if (CurrentDocument is not null
            && !string.Equals(CurrentDocument.Source.PrimaryPath, document.Source.PrimaryPath, StringComparison.OrdinalIgnoreCase))
            representationDocuments.Clear();
        CurrentDocument = document;
        CurrentDescriptor = descriptor;
        EmptyState.Visibility = Visibility.Collapsed;
        MediaSurface.Visibility = Visibility.Visible;
        InspectorBand.Visibility = Visibility.Visible;
        OverviewBar.Visibility = Visibility.Visible;
        representationDocuments[representationKind] = (document, descriptor);
        HeaderSection.DisplayDocument(document, descriptor);
        Legend.Configure(descriptor);
        TrackOverview.Configure(descriptor);
        ConfigureInspectorSurfaces(descriptor);
        SetInspectorModel(0, inspectorModel);

        if (!representationViews.TryGetValue(representationKind, out var selectedView)) return false;
        ActivateRepresentationView(representationKind, selectedView);
        return true;
    }

    public bool ShowRepresentation(MediaRepresentationKind representationKind)
    {
        if (!representationDocuments.TryGetValue(representationKind, out var entry)) return false;
        return ShowDocument(entry.Document, entry.Descriptor);
    }

    public void SetInspectorModel(MediaInspectorModel? inspectorModel)
    {
        SetInspectorModel(0, inspectorModel);
    }

    public void SetInspectorModel(int surface, MediaInspectorModel? inspectorModel)
    {
        (surface == 1 ? Face1Inspector : Face0Inspector).Model = inspectorModel;
    }

    public void ClearInspectorModels()
    {
        Face0Inspector.Model = null;
        Face1Inspector.Model = null;
    }

    private void ConfigureInspectorSurfaces(MediaVisualizationDescriptor descriptor)
    {
        ClearInspectorModels();
        Face0Inspector.SurfaceTitle = descriptor.RepresentationKind == MediaRepresentationKind.Sequential
            ? LocExtension.Get("Explorer.Cassette")
            : LocExtension.Get("Visual.Side", 0);
        Face1Inspector.SurfaceTitle = LocExtension.Get("Visual.Side", 1);
        var hasFace0 = descriptor.Surfaces.Contains(0);
        var hasFace1 = descriptor.Surfaces.Contains(1);
        Face0Inspector.Visibility = hasFace0 || !hasFace1 ? Visibility.Visible : Visibility.Collapsed;
        Face1Inspector.Visibility = hasFace1 ? Visibility.Visible : Visibility.Collapsed;
        Grid.SetColumn(Face0Inspector, 0);
        Grid.SetColumnSpan(Face0Inspector, hasFace1 ? 1 : 3);
        Grid.SetColumn(Face1Inspector, hasFace0 ? 2 : 0);
        Grid.SetColumnSpan(Face1Inspector, hasFace0 ? 1 : 3);
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

    private void LinkZoomChanged(object sender, RoutedEventArgs e)
    {
        var linked = HeaderSection.LinkZoomCheckBox.IsChecked == true;
        foreach (var sectorView in representationViews.Values.OfType<SectorMediaView>())
            sectorView.LinkZoom = linked;
    }
}
