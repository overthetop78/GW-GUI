using GWGUI.App.Interfaces.Rendering.Scp;
using GWGUI.App.Views.Controls.Visualization;
using GWGUI.Tests.Application.TestInfrastructure;
using GWGUI.Tests.Interface.ExplorerViews;
using GWGUI.App.Services.DiskImages;
using GWGUI.App.Services.Visualization;
using GWGUI.App.ViewModels.Main;
using GWGUI.App.Views.Controls.Explorer;
using GWGUI.App.Interfaces.Services.Dialogs;
using GWGUI.Domain.Commands.Building;
using GWGUI.Domain.Commands.Execution;
using GWGUI.Domain.Settings;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Exploration;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.Visualization;
using System.Windows;
namespace GWGUI.Tests.Interface.VisualizerViews;
internal static class VisualizerDocumentScenarios
{
    public static async Task Classification()
    {
        using var workspace=new Workspace();
        var formats=new[] {new GWGUI.Domain.Formats.DiskFormat("test.first","family-a","format-a",[new(".a","a",true)]),new GWGUI.Domain.Formats.DiskFormat("test.second","family-b","format-b",[new(".b","b",true)])};
        workspace.Visualizer.Header.SetFormats(formats); workspace.Explorer.SetFormats(formats,null);
        var first=ExplorerDocumentScenarios.Document("FIRST"); var image=first.Image.WithFormatId("test.first"); var second=image.WithFormatId("test.second");
        var document=new ExploredDiskImage(first.SourcePath,image,first.Volume,first.Metadata,detectedFileSystems:[new("test.first","reader-a",image,first.Volume),new("test.second","reader-b",second,first.Volume)],scpImage:first.ScpImage);
        workspace.Explore=(_,_,_)=>Task.FromResult(document);
        await workspace.Controller.LoadExplorerAsync(first.SourcePath);
        var selector=workspace.Visualizer.Header.ClassificationSelector;
        Assert.Equal("test.first",selector.SelectedFormatId); Assert.Equal("family-a",selector.SelectedMachine);
        var machines=Assert.IsType<System.Windows.Controls.ComboBox>(selector.FindName("Machine"));
        Assert.Equal(2,machines.Items.Cast<GWGUI.App.Contracts.Storage.DiskMachineChoice>().Count(item=>item.IsDetected));
        selector.SetAutomaticDetection(false);
        workspace.Visualizer.Header.ApplyDetection("test.second",null); Assert.Equal("test.first",selector.SelectedFormatId);
        selector.SetAutomaticDetection(true);
        document=new ExploredDiskImage("unknown",image.WithFormatId("unknown"),first.Volume,first.Metadata,fileSystemRecognized:false,scpImage:first.ScpImage);
        await workspace.Controller.LoadExplorerAsync("unknown"); Assert.Null(selector.SelectedFormatId); Assert.Null(selector.SelectedMachine);
        Assert.All(machines.Items.Cast<GWGUI.App.Contracts.Storage.DiskMachineChoice>(),item=>Assert.False(item.IsDetected));
    }
    internal sealed class Workspace : IDisposable
    {
        public VisualizerTabSection Visualizer { get; } = new();
        public ExplorerSection Explorer { get; } = new();
        public MainWindowViewModel Model { get; } = new("hardware", "operation");
        public List<Exception> Errors { get; } = [];
        public DiskImageWorkspaceController Controller { get; }
        public Func<string, CancellationToken, Task<ScpImage>> Read { get; set; } = (_, _) => throw new InvalidOperationException("Unexpected SCP read");
        public Func<string, string?, CancellationToken, Task<ExploredDiskImage>> Explore { get; set; } = (_, _, _) => throw new InvalidOperationException("Unexpected exploration");
        public Workspace()
        {
            var scope = new DiskImageCancellationScope();
            var reader = ControlledDependencies.Simulate<IScpReader>((method, args) => method.Name == "ReadAsync"
                ? Read((string)args[0]!, (CancellationToken)args[1]!) : throw new InvalidOperationException(method.Name));
            Controller = new(Explorer, Visualizer, Model, new TrackProgressStrip(), new TrackProgressStrip(),
                () => new AppSettings(), () => throw new InvalidOperationException("Unexpected detection"),
                () => throw new InvalidOperationException("Unexpected capabilities"),
                ControlledDependencies.Reject<IFileDialogService>(), ControlledDependencies.Reject<IGwCommandBuilder>(),
                ControlledDependencies.Reject<IGreaseweazleRunner>(), InspectorSelectionScenarios.Controller(Visualizer, scope),
                new ScpDocumentLoader(reader, (key, _) => key), DiskImageExplorer.CreateDefault(), new SectorImageFluxVisualizer(),
                scope, () => false, (error, _, _, _) => Errors.Add(error), (key, _) => key,
                (path, format, token) => Explore(path, format, token));
        }
        public void Dispose() => Controller.Dispose();
    }

    public static async Task LateCompletion(bool failure)
    {
        using var workspace = new Workspace();
        var pending = new TaskCompletionSource<ScpImage>();
        var oldToken = default(CancellationToken);
        var latest = ExplorerDocumentScenarios.Document("latest").ScpImage!;
        workspace.Read = (path, token) => {
            if (path == "old.scp") { oldToken = token; return pending.Task; }
            Assert.Equal("latest.scp", path); return Task.FromResult(latest);
        };
        var old = workspace.Controller.LoadScpAsync("old.scp");
        Assert.Equal(Visibility.Visible, workspace.Model.ProgressVisibility);
        await workspace.Controller.LoadScpAsync("latest.scp");
        Assert.True(oldToken.IsCancellationRequested);
        if (failure) pending.SetException(new IOException("old read failed"));
        else pending.SetResult(ExplorerDocumentScenarios.Document("old").ScpImage!);
        await old;
        Assert.Equal("latest.scp", workspace.Visualizer.Header.FileNameText.Text);
        Assert.Equal("Visual.Summary", workspace.Visualizer.Header.SummaryText.Text);
        Assert.Equal(Visibility.Collapsed, workspace.Model.ProgressVisibility);
        Assert.Empty(workspace.Errors);
    }

    public static async Task CancelAndRetry()
    {
        using var workspace = new Workspace();
        workspace.Read = (_, _) => Task.FromException<ScpImage>(new InvalidDataException("invalid capture"));
        await workspace.Controller.LoadScpAsync("invalid.scp");
        Assert.IsType<InvalidDataException>(Assert.Single(workspace.Errors));
        Assert.Equal("Visual.Invalid", workspace.Visualizer.Header.SummaryText.Text);
        var pending = new TaskCompletionSource<ScpImage>();
        workspace.Read = (_, _) => pending.Task;
        var cancelled = workspace.Controller.LoadScpAsync("cancelled.scp");
        workspace.Controller.CancelAll();
        workspace.Controller.ClearVisualizer("empty");
        pending.SetResult(ExplorerDocumentScenarios.Document("cancelled").ScpImage!);
        await cancelled;
        Assert.Equal("empty", workspace.Visualizer.Header.FileNameText.Text);
        Assert.Equal("Visual.NoFile", workspace.Visualizer.Header.SummaryText.Text);
        workspace.Read = (_, _) => Task.FromResult(ExplorerDocumentScenarios.Document("retry").ScpImage!);
        await workspace.Controller.LoadScpAsync("retry.scp");
        Assert.Equal("retry.scp", workspace.Visualizer.Header.FileNameText.Text);
        Assert.Equal(Visibility.Collapsed, workspace.Model.ProgressVisibility);
        Assert.Single(workspace.Errors);
    }

    public static async Task ReplacedAnalysis(bool combinedLoad)
    {
        using var workspace = new Workspace();
        var pending = new TaskCompletionSource<ExploredDiskImage>();
        var latest = ExplorerDocumentScenarios.Document("latest");
        workspace.Explore = (path, _, _) => path == "old.scp" ? pending.Task : Task.FromResult(latest);
        var reads = new List<string>();
        workspace.Read = (path, _) => { reads.Add(path); return Task.FromResult(latest.ScpImage!); };
        var old = combinedLoad ? workspace.Controller.LoadAsync("old.scp") : workspace.Controller.LoadVisualizerAsync("old.scp");
        if (combinedLoad) await workspace.Controller.LoadAsync("latest.scp");
        else await workspace.Controller.LoadVisualizerAsync("latest.scp");
        pending.SetResult(ExplorerDocumentScenarios.Document("old"));
        await old;
        Assert.Equal(new[] { "latest.scp" }, reads);
        Assert.Equal("latest.scp", workspace.Visualizer.Header.FileNameText.Text);
        Assert.Same(latest, workspace.Controller.LastReadImage);
        Assert.Empty(workspace.Errors);
    }

    public static async Task Heads(int mask)
    {
        using var workspace = new Workspace();
        var tracks = Enumerable.Range(0, 2).Where(head => (mask & (1 << head)) != 0)
            .Select(head => new ScpTrack((byte)head, 0, head, [])).ToArray();
        var image = new ScpImage(ExplorerDocumentScenarios.Document("heads").ScpImage!.Header, tracks, true, 688);
        workspace.Read = (_, _) => Task.FromResult(image);
        await workspace.Controller.LoadScpAsync("heads.scp");
        Assert.Equal((mask & 1) != 0 ? Visibility.Visible : Visibility.Collapsed, workspace.Visualizer.FirstSide.Visibility);
        Assert.Equal((mask & 2) != 0 ? Visibility.Visible : Visibility.Collapsed, workspace.Visualizer.SecondSide.Visibility);
        Assert.Equal(mask == 1 ? 2 : 1, System.Windows.Controls.Grid.GetColumnSpan(workspace.Visualizer.FirstSide));
        Assert.Equal(mask == 2 ? 2 : 1, System.Windows.Controls.Grid.GetColumnSpan(workspace.Visualizer.SecondSide));
        Assert.Equal(mask == 2 ? 0 : 1, System.Windows.Controls.Grid.GetColumn(workspace.Visualizer.SecondSide));
        Assert.Equal("heads.scp", workspace.Visualizer.Header.FileNameText.Text);
        Assert.Equal(Visibility.Collapsed, workspace.Model.ProgressVisibility);
        Assert.Empty(workspace.Errors);
        workspace.Controller.ClearVisualizer("empty");
        Assert.Null(workspace.Visualizer.FirstSide.SelectedTrack); Assert.Null(workspace.Visualizer.SecondSide.SelectedTrack);
        Assert.Equal("Visual.NoFile", workspace.Visualizer.Header.SummaryText.Text);
    }
    public static void Image()
    {
        var clears = 0; var prepares = 0; string? decoder = null;
        var image = ExplorerDocumentScenarios.Document("flux").ScpImage!;
        var renderer = ControlledDependencies.Simulate<IScpRenderer>((method, args) => {
            switch (method.Name) {
                case "ClearCache": clears++; return null;
                case "PrepareAsync": Assert.Same(image, args[0]); Assert.Equal(1, args[1]); prepares++; return Task.CompletedTask;
                case "set_DecoderId": decoder = (string?)args[0]; return null;
                default: throw new InvalidOperationException(method.Name);
            }
        });
        var view = new ScpDiskView(renderer);
        view.PrepareAsync().GetAwaiter().GetResult(); Assert.Equal(0, prepares);
        view.SetZoom(3); view.SetImage(image, 1);
        Assert.Equal(1, view.Zoom); Assert.Null(view.SelectedTrack); Assert.Equal(1, clears);
        view.PrepareAsync().GetAwaiter().GetResult(); Assert.Equal(1, prepares);
        view.SetDecoder("synthetic-decoder"); Assert.Equal("synthetic-decoder", decoder);
        view.SetImage(null, 0); view.PrepareAsync().GetAwaiter().GetResult();
        Assert.Equal(2, clears); Assert.Equal(1, prepares);
    }
}
