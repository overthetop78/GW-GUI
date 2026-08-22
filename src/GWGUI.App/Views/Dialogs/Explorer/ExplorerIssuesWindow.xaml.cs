using System.Windows;

namespace GWGUI.App.Views.Dialogs.Explorer;

public partial class ExplorerIssuesWindow : Window
{
    public ExplorerIssuesWindow(IReadOnlyList<string> issues)
    {
        InitializeComponent();
        IssuesList.ItemsSource = issues;
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
