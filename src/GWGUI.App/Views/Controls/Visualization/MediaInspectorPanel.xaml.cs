using GWGUI.App.Contracts.ViewModels.Visualization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace GWGUI.App.Views.Controls.Visualization;

public partial class MediaInspectorPanel : UserControl
{
    public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(
        nameof(Model), typeof(MediaInspectorModel), typeof(MediaInspectorPanel),
        new PropertyMetadata(null, ModelChanged));

    public static readonly DependencyProperty SurfaceTitleProperty = DependencyProperty.Register(
        nameof(SurfaceTitle), typeof(string), typeof(MediaInspectorPanel), new PropertyMetadata(string.Empty));

    public MediaInspectorPanel()
    {
        InitializeComponent();
    }

    public MediaInspectorModel? Model
    {
        get => (MediaInspectorModel?)GetValue(ModelProperty);
        set => SetValue(ModelProperty, value);
    }

    public string SurfaceTitle
    {
        get => (string)GetValue(SurfaceTitleProperty);
        set => SetValue(SurfaceTitleProperty, value);
    }

    private static void ModelChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
    {
        var panel = (MediaInspectorPanel)target;
        var model = args.NewValue as MediaInspectorModel;
        panel.SectionItems.ItemsSource = model?.Sections.Select(section => new SectionPresentation(
            section,
            section.Entries.Take(3).ToArray())).ToArray() ?? [];
        panel.SectionItems.Visibility = model?.Sections.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        panel.MoreInfoButton.Visibility = model?.Sections.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        panel.DetailsSections.ItemsSource = model?.Sections;
        if (model is null) panel.DetailsPopup.IsOpen = false;
        AutomationProperties.SetName(panel, model?.Title ?? panel.SurfaceTitle);
        AutomationProperties.SetHelpText(panel, model?.SelectedElement ?? string.Empty);
    }

    private void MoreInfoButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || Model is null) return;
        DetailsPopup.PlacementTarget = button;
        DetailsPopup.Placement = PlacementMode.Left;
        DetailsPopup.IsOpen = !DetailsPopup.IsOpen;
    }

    private sealed record SectionPresentation(
        MediaInspectorSection Section,
        IReadOnlyList<MediaInspectorEntry> PreviewEntries);
}
