using GWGUI.App.Contracts.ViewModels.Visualization;
using System.Windows.Automation;
using System.Windows.Controls;

namespace GWGUI.App.Views.Controls.Visualization;

public partial class MediaInspectorPanel : UserControl
{
    private MediaInspectorModel? model;

    public MediaInspectorPanel()
    {
        InitializeComponent();
    }

    public MediaInspectorModel? Model
    {
        get => model;
        set
        {
            model = value;
            DataContext = value;
            AutomationProperties.SetName(this, value?.Title ?? string.Empty);
            AutomationProperties.SetHelpText(this, value?.SelectedElement ?? string.Empty);
        }
    }
}
