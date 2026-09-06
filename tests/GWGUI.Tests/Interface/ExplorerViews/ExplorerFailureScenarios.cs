using GWGUI.App.Views.Controls.Explorer;
using System.Windows;
using System.Windows.Controls;
namespace GWGUI.Tests.Interface.ExplorerViews;
internal static class ExplorerFailureScenarios
{
    public static async Task LoadingAndRetry(bool staleFailure)
    {
        using var workspace = new GWGUI.Tests.Interface.VisualizerViews.VisualizerDocumentScenarios.Workspace();
        var pending = new TaskCompletionSource<GWGUI.MediaEngine.Exploration.Results.ExploredDiskImage>();
        var oldToken = default(CancellationToken);
        var latest = ExplorerDocumentScenarios.Document("latest");
        workspace.Explore = (path, _, token) => {
            if (path == "old.scp") { oldToken = token; return pending.Task; }
            Assert.Equal("latest.scp", path); return Task.FromResult(latest);
        };
        var old = workspace.Controller.LoadExplorerAsync("old.scp");
        Assert.Equal(Visibility.Visible, ExplorerDocumentScenarios.Find<Border>(workspace.Explorer, "LoadingOverlay").Visibility);
        Assert.Empty(ExplorerDocumentScenarios.Find<ListView>(workspace.Explorer, "ContentsList").Items);
        Assert.Same(latest, await workspace.Controller.LoadExplorerAsync("latest.scp"));
        Assert.True(oldToken.IsCancellationRequested);
        if (staleFailure) pending.SetException(new IOException("obsolete failure"));
        else pending.SetResult(ExplorerDocumentScenarios.Document("old"));
        Assert.Null(await old);
        Assert.Same(latest, workspace.Controller.LastReadImage);
        Assert.Equal("latest", ExplorerDocumentScenarios.Find<TextBlock>(workspace.Explorer, "VolumeNameText").Text);
        Assert.Empty(workspace.Errors);
        workspace.Explore = (_, _, _) => Task.FromException<GWGUI.MediaEngine.Exploration.Results.ExploredDiskImage>(new IOException("current failure"));
        Assert.Null(await workspace.Controller.LoadExplorerAsync("failure.scp"));
        Assert.IsType<IOException>(Assert.Single(workspace.Errors));
        Assert.Empty(ExplorerDocumentScenarios.Find<ListView>(workspace.Explorer, "ContentsList").Items);
        Assert.Equal(Visibility.Collapsed, ExplorerDocumentScenarios.Find<Border>(workspace.Explorer, "LoadingOverlay").Visibility);
        workspace.Explore = (_, _, _) => Task.FromResult(latest);
        Assert.Same(latest, await workspace.Controller.LoadExplorerAsync("retry.scp"));
        Assert.Equal("latest", ExplorerDocumentScenarios.Find<TextBlock>(workspace.Explorer, "VolumeNameText").Text);
    }
    public static void Clear()
    {
        var section = new ExplorerSection();
        section.Display(ExplorerDocumentScenarios.Document("OLD"));
        section.SetLoading(true);
        Assert.Equal(Visibility.Visible, ExplorerDocumentScenarios.Find<Border>(section, "LoadingOverlay").Visibility);
        section.Clear("virtual/new.scp");
        Assert.Empty(ExplorerDocumentScenarios.Find<ListBox>(section, "FolderList").Items);
        Assert.Empty(ExplorerDocumentScenarios.Find<ListView>(section, "ContentsList").Items);
        Assert.Equal("virtual/new.scp", ExplorerDocumentScenarios.Find<TextBox>(section, "PathText").Text);
        section.SetLoading(false);
        Assert.Equal(Visibility.Collapsed, ExplorerDocumentScenarios.Find<Border>(section, "LoadingOverlay").Visibility);
        section.Display(ExplorerDocumentScenarios.Document("NEW"));
        Assert.Equal("NEW", ExplorerDocumentScenarios.Find<TextBlock>(section, "VolumeNameText").Text);
    }
}
