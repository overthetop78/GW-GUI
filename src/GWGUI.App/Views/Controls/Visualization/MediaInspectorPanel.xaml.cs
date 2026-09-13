using GWGUI.App.Contracts.ViewModels.Visualization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;

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
        panel.DataContext = model;
        AutomationProperties.SetName(panel, model?.Title ?? panel.SurfaceTitle);
        AutomationProperties.SetHelpText(panel, model?.SelectedElement ?? string.Empty);
    }
}
