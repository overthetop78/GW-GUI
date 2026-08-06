using System.IO;
using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Localization;
using Microsoft.Win32;

namespace GWGUI.App;

public partial class LogHistoryWindow : Window
{
    public LogHistoryWindow(string directory)
    {
        InitializeComponent();
        FilesList.ItemsSource = Directory.Exists(directory)
            ? new DirectoryInfo(directory).GetFiles("*.log").Where(file => !file.Name.StartsWith("errors-", StringComparison.OrdinalIgnoreCase)).OrderByDescending(file => file.LastWriteTimeUtc).ToArray()
            : Array.Empty<FileInfo>();
        FilesList.SelectedIndex = FilesList.Items.Count > 0 ? 0 : -1;
    }

    private async void FilesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (FilesList.SelectedItem is not FileInfo file) { ContentText.Clear(); ExportButton.IsEnabled = false; return; }
        try { ContentText.Text = await File.ReadAllTextAsync(file.FullName); ExportButton.IsEnabled = true; }
        catch (IOException exception) { ContentText.Text = exception.Message; ExportButton.IsEnabled = false; }
    }

    private async void Export_Click(object sender, RoutedEventArgs e)
    {
        if (FilesList.SelectedItem is not FileInfo file) return;
        var dialog = new SaveFileDialog { Filter = LocExtension.Get("Logs.ExportFilter"), FileName = Path.ChangeExtension(file.Name, ".txt"), DefaultExt = ".txt" };
        if (dialog.ShowDialog(this) == true) await File.WriteAllTextAsync(dialog.FileName, ContentText.Text);
    }
}
