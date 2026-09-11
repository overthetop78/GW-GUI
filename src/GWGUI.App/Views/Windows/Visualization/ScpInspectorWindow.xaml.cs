using GWGUI.App.Contracts.ViewModels.Visualization;
using System.Windows;

namespace GWGUI.App.Views.Windows.Visualization;

public partial class ScpInspectorWindow : Window
{
    public ScpInspectorWindow()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => Inspector.Model = DataContext as MediaInspectorModel;
    }

    public event EventHandler? AttachRequested;

    private void AttachButton_Click(object sender, RoutedEventArgs e)
    {
        AttachRequested?.Invoke(this, EventArgs.Empty);
        Close();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
