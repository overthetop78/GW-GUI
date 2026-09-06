using GWGUI.App.Views.Controls.Explorer;
using GWGUI.App.ViewModels.Explorer;
using System.Windows.Controls;
namespace GWGUI.Tests.Interface.ExplorerViews;
internal static class ExplorerTreeScenarios
{
    public static void Select()
    {
        var section = new ExplorerSection();
        section.Clear(); section.Display(ExplorerDocumentScenarios.Document("VOLUME"));
        var folders = ExplorerDocumentScenarios.Find<ListBox>(section, "FolderList");
        var contents = ExplorerDocumentScenarios.Find<ListView>(section, "ContentsList");
        var details = ExplorerDocumentScenarios.Find<ExplorerDetailsPanel>(section, "DetailsPanel");
        Assert.Equal(2, folders.Items.Count);
        folders.SelectedIndex = 1;
        var file = Assert.IsType<ExplorerContentItem>(Assert.Single(contents.Items.Cast<object>()));
        Assert.Equal("FILE.BIN", file.Entry.Name); Assert.Equal(2, file.Entry.Size);
        contents.SelectedItem = file;
        Assert.Same(file, contents.SelectedItem);
        Assert.False(details.IsShowingDisk); Assert.Equal("FILE.BIN", details.DisplayedTitle);
        Assert.Equal("note", Assert.IsType<TextBlock>(details.FindName("DetailValue4")).Text);
        Assert.Equal(file.SizeText, Assert.IsType<TextBlock>(details.FindName("DetailValue2")).Text);
        Assert.Equal(file.ModifiedText, Assert.IsType<TextBlock>(details.FindName("DetailValue3")).Text);
        folders.SelectedIndex = 0;
        Assert.Null(contents.SelectedItem);
        Assert.Equal("DIR", Assert.IsType<ExplorerContentItem>(Assert.Single(contents.Items.Cast<object>())).Entry.Name);
        Assert.True(details.IsShowingDisk); Assert.Equal("VOLUME", details.DisplayedTitle);
        Assert.Equal("virtual/VOLUME.scp", ExplorerDocumentScenarios.Find<TextBox>(section, "PathText").Text);
        var empty = new GWGUI.MediaEngine.FileSystems.FileSystemEntry("EMPTY", GWGUI.MediaEngine.FileSystems.FileSystemEntryKind.Directory, 0, null, "", 0, 0, true, []);
        var source = ExplorerDocumentScenarios.Document("EMPTY-VOLUME", false);
        var document = new GWGUI.MediaEngine.Exploration.Results.ExploredDiskImage(source.SourcePath, source.Image,
            new("EMPTY-VOLUME", "synthetic", 512, 256, null, null, [empty], []), source.Metadata, scpImage: source.ScpImage);
        section.Clear(); section.Display(document);
        folders.SelectedIndex = 1;
        Assert.Empty(contents.Items); Assert.Null(contents.SelectedItem); Assert.True(details.IsShowingDisk);
        folders.SelectedIndex = 0;
        Assert.Equal("EMPTY", Assert.IsType<ExplorerContentItem>(Assert.Single(contents.Items.Cast<object>())).Entry.Name);
    }
}
