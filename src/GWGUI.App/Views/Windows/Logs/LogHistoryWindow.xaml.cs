using GWGUI.App.Localization.Extensions;
using GWGUI.App.Services.Logging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace GWGUI.App.Views.Windows.Logs;

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
        string content;
        var canExport = false;
        try { content = await File.ReadAllTextAsync(file.FullName).ConfigureAwait(false); canExport = true; }
        catch (IOException exception)
        {
            var path = ErrorLog.Write(exception, "Reading log history");
            var detail = path is null ? LocExtension.Get("Common.Unknown") : LocExtension.Get("Error.LogSaved", path);
            content = LocExtension.Get("Error.Unexpected", detail);
        }
        if (Dispatcher.HasShutdownStarted || Dispatcher.HasShutdownFinished) return;
        _ = Dispatcher.BeginInvoke(() => { if (Equals(FilesList.SelectedItem, file)) { ContentText.Text = content; ExportButton.IsEnabled = canExport; } });
    }

    private async void Export_Click(object sender, RoutedEventArgs e)
    {
        if (FilesList.SelectedItem is not FileInfo file) return;
        var dialog = new SaveFileDialog { Filter = LocExtension.Get("Logs.ExportFilter"), FileName = Path.ChangeExtension(file.Name, ".txt"), DefaultExt = ".txt" };
        if (dialog.ShowDialog(this) == true) await File.WriteAllTextAsync(dialog.FileName, ContentText.Text);
    }
}
