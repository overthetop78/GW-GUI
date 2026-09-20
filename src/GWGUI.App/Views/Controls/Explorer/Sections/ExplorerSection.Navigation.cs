using GWGUI.MediaEngine.Images.Formats;
using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Enums.Explorer;
using GWGUI.App.Functions.Explorer;
using GWGUI.App.Functions.Storage;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Presenters.Explorer;
using GWGUI.App.ViewModels.Explorer;
using GWGUI.App.Views.Dialogs.Explorer;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.Images.Models.Optical;
using GWGUI.MediaEngine.Images.Models.Sequential;
using GWGUI.MediaEngine.Images.Reading.Recognition;

namespace GWGUI.App.Views.Controls.Explorer;

public partial class ExplorerSection
{
    public static int CountEntries(IEnumerable<FileSystemEntry> entries) => ExplorerIssueBuilder.CountEntries(entries);

    private void RefreshVisibleFolders(ExplorerFolderItem? selected = null)
    {
        if (_rootFolder is null) return;
        _visibleFolders.Clear();
        foreach (var item in ExplorerTreeNavigator.Flatten(_rootFolder)) _visibleFolders.Add(item);
        if (selected is not null) FolderList.SelectedItem = selected;
    }

    private void ShowContents(IEnumerable<FileSystemEntry> entries)
    {
        var family = _document is not null
            ? ExplorerFileIconClassifier.FamilyFor(_document)
            : _mediaDocument is not null
                ? ExplorerFileIconClassifier.FamilyFor(_mediaDocument.Document.FormatId, _mediaVolume?.FileSystem?.FileSystemId)
                : ExplorerFileSystemFamily.Unknown;
        ContentsList.ItemsSource = entries
            .OrderBy(entry => entry.Kind != FileSystemEntryKind.Directory)
            .ThenBy(entry => entry.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(entry => new ExplorerContentItem(entry, family)).ToArray();
        ContentsList.SelectedItem = null;
        if (_document is not null) DetailsPanel.ShowDisk(_document, CurrentSystem(_document));
        else if (_mediaDocument is not null && _mediaVolume is not null)
            DetailsPanel.ShowMedia(_mediaDocument, _mediaVolume, CurrentSystem(_mediaDocument));
    }

    private void ContentsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_document is null && _mediaDocument is null) return;
        if (ContentsList.SelectedItem is ExplorerContentItem item)
        {
            if (_document is not null) DetailsPanel.ShowItem(_document, item);
            else DetailsPanel.ShowItem(item);
        }
        else if (_document is not null) DetailsPanel.ShowDisk(_document, CurrentSystem(_document));
        else if (_mediaDocument is not null && _mediaVolume is not null)
            DetailsPanel.ShowMedia(_mediaDocument, _mediaVolume, CurrentSystem(_mediaDocument));
    }

    private void FolderToggle_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: ExplorerFolderItem item } || item.Children.Count == 0) return;
        item.IsExpanded = !item.IsExpanded;
        RefreshVisibleFolders(item);
        ShowContents(item.Entry?.Children ?? _rootEntries);
        e.Handled = true;
    }

    private void FolderList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (FolderList.SelectedItem is ExplorerFolderItem item) ShowContents(item.Entry?.Children ?? _rootEntries);
    }

    private void FolderList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var element = e.OriginalSource as DependencyObject;
        while (element is not null && element is not ListBoxItem)
            element = VisualTreeHelper.GetParent(element);
        if (element is not ListBoxItem { DataContext: ExplorerFolderItem item }) return;
        ContentsList.SelectedItem = null;
        ShowContents(item.Entry?.Children ?? _rootEntries);
    }

    private void ContentsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (ContentsList.SelectedItem is not ExplorerContentItem { Entry.Kind: FileSystemEntryKind.Directory } selected || _rootFolder is null) return;
        var folder = ExplorerTreeNavigator.Find(_rootFolder, selected.Entry);
        if (folder is null) return;
        ExplorerTreeNavigator.ExpandPathTo(_rootFolder, folder);
        RefreshVisibleFolders(folder);
        ShowContents(folder.Entry!.Children);
    }

    private void WarningsButton_Click(object sender, RoutedEventArgs e)
    {
        var issues = _document is not null
            ? BuildIssues(_document)
            : _mediaDocument is not null && _mediaVolume is not null
                ? ExplorerIssueBuilder.Build(_mediaDocument, _mediaVolume)
                : [];
        if (issues.Count == 0) return;
        new ExplorerIssuesWindow(issues) { Owner = Window.GetWindow(this) }.ShowDialog();
    }

    public static IReadOnlyList<string> BuildIssues(ExploredDiskImage document) => ExplorerIssueBuilder.Build(document);
}
