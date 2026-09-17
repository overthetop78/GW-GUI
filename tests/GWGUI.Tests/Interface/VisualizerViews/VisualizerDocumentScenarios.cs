using GWGUI.App.Interfaces.Rendering.Scp;
using GWGUI.App.Contracts.Rendering.Scp;
using GWGUI.App.Enums.Rendering.Scp;
using GWGUI.App.Views.Controls.Visualization;
using GWGUI.Tests.Application.TestInfrastructure;
using GWGUI.Tests.Interface.ExplorerViews;
using GWGUI.App.Services.DiskImages;
using GWGUI.App.Services.Visualization;
using GWGUI.App.ViewModels.Main;
using GWGUI.App.ViewModels.Explorer;
using GWGUI.App.Views.Controls.Explorer;
using GWGUI.App.Interfaces.Services.Dialogs;
using GWGUI.App.Contracts.Services.Dialogs;
using GWGUI.Domain.Commands.Building;
using GWGUI.Domain.Commands.Execution;
using GWGUI.Domain.Settings;
using GWGUI.Domain.Enums;
using GWGUI.Domain.Formats;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Exploration;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.Composition;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Reading;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Representations.Sequential;
using GWGUI.MediaEngine.Representations.Sectors;
using GWGUI.MediaEngine.Visualization;
using GWGUI.MediaEngine.Enums;
using System.IO;
using System.Windows;
using GWGUI.MediaEngine.Formats.Floppy.Scp;

namespace GWGUI.Tests.Interface.VisualizerViews;
internal static class VisualizerDocumentScenarios
{
    public static void ReplacedCancellationTokenRemainsReadable()
    {
        using var scope = new DiskImageCancellationScope();
        var firstSource = scope.BeginVisualization();
        var firstToken = firstSource.Token;

        var secondSource = scope.BeginVisualization();

        Assert.True(firstToken.IsCancellationRequested);
        Assert.False(secondSource.IsCancellationRequested);
        Assert.True(scope.IsCurrentVisualization(secondSource));
    }

    public static async Task Classification()
    {
        using var workspace=new Workspace();
        var formats=new[] {new GWGUI.Domain.Formats.DiskFormat("test.first","family-a","format-a",[new(".a","a",true)]),new GWGUI.Domain.Formats.DiskFormat("test.second","family-b","format-b",[new(".b","b",true)])};
        workspace.Visualizer.Header.SetFormats(formats); workspace.Explorer.SetFormats(formats,null);
        var first=ExplorerDocumentScenarios.Document("FIRST"); var image=first.Image.WithFormatId("test.first"); var second=image.WithFormatId("test.second");
        var document=new ExploredDiskImage(first.SourcePath,image,first.Volume,first.Metadata,detectedFileSystems:[new("reader-a",image,first.Volume),new("reader-b",second,first.Volume)],scpImage:first.ScpImage);
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
        public TrackProgressStrip Face0Progress { get; } = new();
        public TrackProgressStrip Face1Progress { get; } = new();
        public List<Exception> Errors { get; } = [];
        public List<(string TitleKey, string MessageKey)> ErrorPresentations { get; } = [];
        public int MediaReadCount { get; private set; }
        public AppSettings Settings { get; }
        public DiskImageWorkspaceController Controller { get; }
        public Func<string, CancellationToken, Task<ScpImage>> Read { get; set; } = (_, _) => throw new InvalidOperationException("Unexpected SCP read");
        public Func<string, string?, CancellationToken, Task<ExploredDiskImage>> Explore { get; set; } = (_, _, _) => throw new InvalidOperationException("Unexpected exploration");
        public Workspace(
            AppSettings? settings = null,
            IFileDialogService? fileDialogs = null,
            bool withVisualizationProviders = false,
            bool withMediaServices = false,
            bool useDefaultExplorer = false,
            int sectorCylinders = 80,
            int sectorHeads = 1,
            IMediaImageReader? mediaImageReader = null)
        {
            Settings = settings ?? new AppSettings();
            var scope = new DiskImageCancellationScope();
            var reader = ControlledDependencies.Simulate<IScpReader>((method, args) => method.Name == "ReadAsync"
                ? Read((string)args[0]!, (CancellationToken)args[1]!) : throw new InvalidOperationException(method.Name));
            Controller = new(Explorer, Visualizer, Model, Face0Progress, Face1Progress,
                () => Settings, () => throw new InvalidOperationException("Unexpected detection"),
                () => throw new InvalidOperationException("Unexpected capabilities"),
                fileDialogs ?? ControlledDependencies.Reject<IFileDialogService>(), ControlledDependencies.Reject<IGwCommandBuilder>(),
                ControlledDependencies.Reject<IGreaseweazleRunner>(), InspectorSelectionScenarios.Controller(Visualizer, scope),
                new ScpDocumentLoader(reader, (key, _) => key), DiskImageExplorer.CreateDefault(),
                scope, () => false, (error, _, titleKey, messageKey) =>
                {
                    Errors.Add(error);
                    ErrorPresentations.Add((titleKey, messageKey));
                }, (key, _) => key,
                explore: useDefaultExplorer ? null : (path, format, token) => Explore(path, format, token),
                mediaReader: withMediaServices
                    ? new MediaImageReadingService(new MediaRecognitionRegistry([
                        mediaImageReader ?? new SectorDocumentReader(sectorCylinders, sectorHeads, () => MediaReadCount++)
                    ]))
                    : null,
                mediaExplorer: withMediaServices ? new MediaExplorer(new FileSystemRegistry([])) : null,
                visualizationProviders: withVisualizationProviders ? MediaVisualizationComposition.CreateDefault().Registry : null);
        }
        public void Dispose() => Controller.Dispose();
    }

    public static async Task UndetectedManualFormatRunsAnExplicitExploration()
    {
        using var workspace = new Workspace();
        var formats = new[]
        {
            new DiskFormat("test.detected", "detected-family", "detected-format", [new(".one", "one", true)]),
            new DiskFormat("test.manual", "manual-family", "manual-format", [new(".two", "two", true)])
        };
        workspace.Explorer.SetFormats(formats, null);
        workspace.Visualizer.Header.SetFormats(formats);
        var source = ExplorerDocumentScenarios.Document("EXPLICIT");
        var detectedImage = source.Image.WithFormatId("test.detected");
        var manualImage = source.Image.WithFormatId("test.manual");
        var detected = new ExploredDiskImage(source.SourcePath, detectedImage, source.Volume, source.Metadata,
            detectedFileSystems: [new("detected-reader", detectedImage, source.Volume)], scpImage: source.ScpImage);
        var manual = new ExploredDiskImage(source.SourcePath, manualImage, source.Volume, source.Metadata,
            detectedFileSystems: [new("manual-reader", manualImage, source.Volume)], scpImage: source.ScpImage);
        var requests = new List<string?>();
        workspace.Explore = (_, requestedFormat, _) =>
        {
            requests.Add(requestedFormat);
            return Task.FromResult(requestedFormat == "test.manual" ? manual : detected);
        };
        await workspace.Controller.LoadExplorerAsync(source.SourcePath);

        var automatic = ExplorerDocumentScenarios.Find<System.Windows.Controls.CheckBox>(workspace.Explorer, "AutomaticDetection");
        automatic.IsChecked = false;
        var selector = workspace.Explorer.ClassificationSelector;
        var machines = Assert.IsType<System.Windows.Controls.ComboBox>(selector.FindName("Machine"));
        machines.SelectedItem = machines.Items.Cast<GWGUI.App.Contracts.Storage.DiskMachineChoice>()
            .Single(item => item.DisplayName == "manual-family");

        await workspace.Controller.SelectExplorerRepresentationAsync();

        Assert.Equal([null, "test.manual"], requests);
        Assert.Equal("test.manual", workspace.Explorer.SelectedFormatId);
    }

    public static async Task NonScpFormatSelectionAttemptsTheChosenFormatAndReloadsTheInitialFormat()
    {
        using var workspace = new Workspace(
            withVisualizationProviders: true,
            withMediaServices: true,
            sectorCylinders: 1);
        var formats = new[]
        {
            new DiskFormat("test.sector", "initial-family", "initial-format", [new(".dll", "dll", true)]),
            new DiskFormat("test.incompatible", "other-family", "other-format", [new(".dll", "dll", true)])
        };
        workspace.Explorer.SetFormats(formats, null);
        workspace.Visualizer.Header.SetFormats(formats);
        var sourcePath = typeof(VisualizerDocumentScenarios).Assembly.Location;
        var source = ExplorerDocumentScenarios.Document("NON-SCP");
        var initial = new ExploredDiskImage(
            sourcePath,
            source.Image.WithFormatId("test.sector"),
            source.Volume,
            source.Metadata);
        var requests = new List<string?>();
        workspace.Explore = (_, formatId, _) =>
        {
            requests.Add(formatId);
            return formatId == "test.incompatible"
                ? Task.FromException<ExploredDiskImage>(new IOException("reader failure"))
                : Task.FromResult(initial);
        };

        await workspace.Controller.LoadAsync(sourcePath);
        var completedSelection = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        workspace.Visualizer.Header.ClassificationFormatChanged += async (_, formatId) =>
        {
            await workspace.Controller.SelectVisualizerRepresentationAsync(formatId);
            completedSelection.TrySetResult();
        };
        var selector = workspace.Visualizer.Header.ClassificationSelector;
        selector.SetAutomaticDetection(false);
        var machines = Assert.IsType<System.Windows.Controls.ComboBox>(selector.FindName("Machine"));
        machines.SelectedItem = machines.Items.Cast<GWGUI.App.Contracts.Storage.DiskMachineChoice>()
            .Single(item => item.DisplayName == "other-family");
        await completedSelection.Task;

        Assert.Equal("test.incompatible", selector.SelectedFormatId);
        Assert.IsType<DiskImageWorkspaceController.SelectedFormatUnsupportedException>(Assert.Single(workspace.Errors));
        Assert.Equal(
            ("Explorer.SelectedFormatUnsupportedTitle", "Explorer.SelectedFormatUnsupported"),
            Assert.Single(workspace.ErrorPresentations));
        Assert.Equal([null, "test.incompatible"], requests);

        completedSelection = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        machines.SelectedItem = machines.Items.Cast<GWGUI.App.Contracts.Storage.DiskMachineChoice>()
            .Single(item => item.DisplayName == "initial-family");
        await completedSelection.Task;

        Assert.Equal("test.sector", selector.SelectedFormatId);
        Assert.Equal("test.sector", workspace.Visualizer.CurrentDocument?.FormatId);
        Assert.Equal(MediaRepresentationKind.Sectors, workspace.Visualizer.ActiveRepresentationKind);
        Assert.Equal([null, "test.incompatible", "test.sector"], requests);
    }

    private sealed class SectorDocumentReader(int cylinders, int heads, Action onRead) : IMediaImageReader
    {
        public IReadOnlySet<string> FormatIds { get; } = new HashSet<string> { "test.sector" };
        public IReadOnlySet<string> Extensions { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".img", ".scp" };
        public IReadOnlyList<ReadOnlyMemory<byte>> Signatures { get; } = [];
        public IReadOnlySet<string> AssociatedFileExtensions { get; } = new HashSet<string>();
        public IReadOnlySet<MediaKind> MediaKinds { get; } = new HashSet<MediaKind> { MediaKind.Floppy };
        public IReadOnlySet<MediaRepresentationKind> RepresentationKinds { get; } =
            new HashSet<MediaRepresentationKind> { MediaRepresentationKind.Sectors };

        public bool SupportsFormatId(string formatId) => FormatIds.Contains(formatId);

        public ValueTask<bool> CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken) =>
            ValueTask.FromResult(true);

        public Task<MediaImageDocument> ReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            onRead();
            var blocks = Enumerable.Range(0, cylinders * heads)
                .Select(index => new SectorBlock(
                    index,
                    new SectorAddress(index / heads, index % heads, 1),
                    new byte[512],
                    true))
                .ToArray();
            var image = new SectorImage("test.sector", 512, cylinders, heads, 1, blocks);
            return Task.FromResult(new MediaImageDocument(
                context.Source,
                "test.sector",
                MediaKind.Floppy,
                new SectorMediaImageRepresentation(image),
                [],
                [],
                new Dictionary<string, string>()));
        }
    }

    private sealed class SequentialDocumentReader : IMediaImageReader
    {
        public IReadOnlySet<string> FormatIds { get; } = new HashSet<string> { "test.tape" };
        public IReadOnlySet<string> Extensions { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".tap" };
        public IReadOnlyList<ReadOnlyMemory<byte>> Signatures { get; } = [];
        public IReadOnlySet<string> AssociatedFileExtensions { get; } = new HashSet<string>();
        public IReadOnlySet<MediaKind> MediaKinds { get; } = new HashSet<MediaKind> { MediaKind.Tape };
        public IReadOnlySet<MediaRepresentationKind> RepresentationKinds { get; } =
            new HashSet<MediaRepresentationKind> { MediaRepresentationKind.Sequential };

        public bool SupportsFormatId(string formatId) => FormatIds.Contains(formatId);

        public ValueTask<bool> CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken) =>
            ValueTask.FromResult(true);

        public Task<MediaImageDocument> ReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var segments = new[]
            {
                new SequentialMediaSegment(0, SequentialSegmentKind.DataBlock, length: 8, faceNumber: 0, trackNumber: 0, channelNumber: 0),
                new SequentialMediaSegment(8, SequentialSegmentKind.Silence, length: 4, faceNumber: 0, trackNumber: 0, channelNumber: 0)
            };
            return Task.FromResult(new MediaImageDocument(
                context.Source,
                "test.tape",
                MediaKind.Tape,
                new SequentialMediaImageRepresentation(12, segments: segments),
                [],
                [],
                new Dictionary<string, string>()));
        }
    }

    public static async Task CassettePresentationUsesRecognizedMediaKindRatherThanExtension()
    {
        var existingNonTapePath = typeof(VisualizerDocumentScenarios).Assembly.Location;
        using (var tapeWorkspace = new Workspace(
                   withVisualizationProviders: true,
                   withMediaServices: true,
                   useDefaultExplorer: true,
                   mediaImageReader: new SequentialDocumentReader()))
        {
            await tapeWorkspace.Controller.LoadAsync(existingNonTapePath);

            Assert.Equal(MediaKind.Tape, tapeWorkspace.Visualizer.CurrentDocument?.MediaKind);
            Assert.Equal("Explorer.LoadingTapeReading", tapeWorkspace.Explorer.LoadingStage);
            Assert.Equal(100, tapeWorkspace.Explorer.LoadingValue);
            Assert.Empty(tapeWorkspace.Errors);
        }

        var falseCassettePath = Path.Combine(Path.GetTempPath(), $"gwgui-false-cassette-{Guid.NewGuid():N}.cas");
        try
        {
            File.WriteAllBytes(falseCassettePath, [0]);
            using var floppyWorkspace = new Workspace(
                withVisualizationProviders: true,
                withMediaServices: true,
                useDefaultExplorer: true,
                sectorCylinders: 1);

            await floppyWorkspace.Controller.LoadAsync(falseCassettePath);

            Assert.Equal(MediaKind.Floppy, floppyWorkspace.Visualizer.CurrentDocument?.MediaKind);
            Assert.NotEqual("Explorer.LoadingTapeReading", floppyWorkspace.Explorer.LoadingStage);
            Assert.Empty(floppyWorkspace.Errors);
        }
        finally
        {
            File.Delete(falseCassettePath);
        }
    }

    public static async Task SharedOpeningReadsMediaOnce()
    {
        using var workspace = new Workspace(
            withVisualizationProviders: true,
            withMediaServices: true,
            useDefaultExplorer: true,
            sectorCylinders: 1);
        var sourcePath = typeof(VisualizerDocumentScenarios).Assembly.Location;

        await workspace.Controller.LoadAsync(sourcePath);

        Assert.Equal(1, workspace.MediaReadCount);
        Assert.Equal(sourcePath, workspace.Controller.ExplorerPath);
        Assert.Equal(sourcePath, workspace.Visualizer.CurrentDocument?.Source.PrimaryPath);
        Assert.Equal(MediaRepresentationKind.Sectors, workspace.Visualizer.ActiveRepresentationKind);
    }

    public static void SharedNonScpProgressStaysInLoadingPanelsUntilTrackProgress()
    {
        using var workspace = new Workspace();
        workspace.Explorer.SetLoading(true);
        workspace.Explorer.SetLoadingProgress("Reading media", "disk.img", 58);
        workspace.Controller.ReportSharedProgress("Reading media", "disk.img", 58, false);

        Assert.Equal("Reading media", workspace.Explorer.LoadingStage);
        Assert.Equal("disk.img", workspace.Explorer.LoadingDetail);
        Assert.Equal(58, workspace.Explorer.LoadingValue);
        Assert.Equal(Visibility.Collapsed, workspace.Model.ProgressVisibility);
        Assert.Equal(Visibility.Collapsed, workspace.Model.GlobalProgressVisibility);
        Assert.Equal(58, workspace.Model.ProgressValue);
        Assert.Empty(workspace.Model.ProgressText);
        Assert.Equal("Tab.Read", workspace.Model.OperationText);
        Assert.True(workspace.Visualizer.RecognitionProgressVisible);
        Assert.Equal(58, workspace.Visualizer.RecognitionValue);
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
        Assert.True(workspace.Visualizer.RecognitionProgressVisible);
        await workspace.Controller.LoadScpAsync("latest.scp");
        Assert.True(oldToken.IsCancellationRequested);
        if (failure) pending.SetException(new IOException("old read failed"));
        else pending.SetResult(ExplorerDocumentScenarios.Document("old").ScpImage!);
        await old;
        Assert.Equal("latest.scp", workspace.Visualizer.Header.FileNameText.Text);
        Assert.Equal("Visual.Summary", workspace.Visualizer.Header.SummaryText.Text);
        Assert.Equal(Visibility.Collapsed, workspace.Model.ProgressVisibility);
        Assert.False(workspace.Visualizer.RecognitionProgressVisible);
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
        var latestSource = ExplorerDocumentScenarios.Document("latest");
        var latestScp = new ScpImage(latestSource.ScpImage!.Header,
        [
            new ScpTrack(0, 0, 0, [new ScpRevolution(8_000_000, 3, [80, 120, 160])])
        ], true, latestSource.ScpImage.FileSize);
        var latest = new ExploredDiskImage(
            latestSource.SourcePath,
            latestSource.Image,
            latestSource.Volume,
            latestSource.Metadata,
            scpImage: latestScp);
        workspace.Explore = async (path, _, cancellationToken) =>
        {
            if (path != "old.scp") return latest;
            using var registration = cancellationToken.Register(() => pending.TrySetCanceled(cancellationToken));
            return await pending.Task;
        };
        workspace.Read = (_, _) => throw new InvalidOperationException("The shared pipeline must not reopen the SCP source.");
        if (!combinedLoad) Assert.Null(workspace.Controller.ExplorerPath);
        var old = combinedLoad ? workspace.Controller.LoadAsync("old.scp") : workspace.Controller.LoadVisualizerAsync("old.scp");
        if (combinedLoad) await workspace.Controller.LoadAsync("latest.scp");
        else await workspace.Controller.LoadVisualizerAsync("latest.scp");
        pending.TrySetResult(ExplorerDocumentScenarios.Document("old"));
        await old;
        Assert.Equal("latest.scp", workspace.Visualizer.Header.FileNameText.Text);
        if (!combinedLoad) Assert.Null(workspace.Controller.ExplorerPath);
        Assert.Same(latest, workspace.Controller.LastReadImage);
        Assert.Empty(workspace.Errors);
    }

    public static async Task SharedLoadsAreSerializedAndCoalesced()
    {
        using var workspace = new Workspace();
        var firstStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var events = new System.Collections.Concurrent.ConcurrentQueue<string>();
        var latestSource = ExplorerDocumentScenarios.Document("latest-serialized");
        var latestScp = new ScpImage(latestSource.ScpImage!.Header,
        [
            new ScpTrack(0, 0, 0, [new ScpRevolution(8_000_000, 3, [80, 120, 160])])
        ], true, latestSource.ScpImage.FileSize);
        var latest = new ExploredDiskImage(
            latestSource.SourcePath,
            latestSource.Image,
            latestSource.Volume,
            latestSource.Metadata,
            scpImage: latestScp);
        workspace.Explore = async (path, _, cancellationToken) =>
        {
            events.Enqueue($"start:{path}");
            if (path == "first.scp")
            {
                firstStarted.TrySetResult(true);
                try
                {
                    await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                }
                finally
                {
                    events.Enqueue("stop:first.scp");
                }
            }
            return latest;
        };
        workspace.Read = (_, _) => throw new InvalidOperationException("The shared pipeline must not reopen the SCP source.");

        var first = workspace.Controller.LoadAsync("first.scp");
        await firstStarted.Task;
        var intermediate = workspace.Controller.LoadAsync("intermediate.scp");
        var last = workspace.Controller.LoadAsync("latest.scp");

        await Task.WhenAll(first, intermediate, last);

        Assert.Equal(
            ["start:first.scp", "stop:first.scp", "start:latest.scp"],
            events.ToArray());
        Assert.Same(latest, workspace.Controller.LastReadImage);
        Assert.Equal("latest.scp", workspace.Visualizer.Header.FileNameText.Text);
        Assert.Empty(workspace.Errors);
    }

    public static async Task ReplacedSectorPresentationDoesNotEscapeCancellation()
    {
        using var workspace = new Workspace(withVisualizationProviders: true, withMediaServices: true, sectorCylinders: 4);
        var sourcePath = typeof(VisualizerDocumentScenarios).Assembly.Location;
        workspace.Explore = (path, _, _) =>
        {
            var source = ExplorerDocumentScenarios.Document(Path.GetFileNameWithoutExtension(path));
            return Task.FromResult(new ExploredDiskImage(
                path,
                source.Image.WithFormatId("test.sector"),
                source.Volume,
                source.Metadata));
        };

        var replaced = workspace.Controller.LoadAsync(sourcePath);
        await Task.Delay(125);
        await workspace.Controller.LoadAsync(sourcePath);
        await replaced;

        Assert.Equal(sourcePath, workspace.Controller.ExplorerPath);
        Assert.Equal(sourcePath, Assert.IsType<ExploredDiskImage>(workspace.Controller.LastReadImage).SourcePath);
        Assert.Empty(workspace.Errors);
    }

    public static async Task SectorStatusProgressUsesMediaGeometry(int cylinders, int heads)
    {
        using var workspace = new Workspace(
            withVisualizationProviders: true,
            withMediaServices: true,
            sectorCylinders: cylinders,
            sectorHeads: heads);
        var sourcePath = typeof(VisualizerDocumentScenarios).Assembly.Location;
        workspace.Explore = (path, _, _) =>
        {
            var source = ExplorerDocumentScenarios.Document(Path.GetFileNameWithoutExtension(path));
            return Task.FromResult(new ExploredDiskImage(
                path,
                source.Image.WithFormatId("test.sector"),
                source.Volume,
                source.Metadata));
        };

        var loading = workspace.Controller.LoadAsync(sourcePath);
        for (var attempt = 0; attempt < 20 &&
             (workspace.Face0Progress.Total != cylinders || workspace.Face0Progress.Completed == 0); attempt++)
            await Task.Delay(25);

        Assert.Equal(cylinders, workspace.Face0Progress.Total);
        Assert.Equal(heads == 2 ? cylinders : 0, workspace.Face1Progress.Total);
        Assert.True(workspace.Face0Progress.Completed + workspace.Face1Progress.Completed > 0);
        Assert.Equal(Visibility.Collapsed, workspace.Model.GlobalProgressVisibility);
        Assert.Equal(Visibility.Visible, workspace.Model.Face0ProgressVisibility);
        Assert.Equal(heads == 2 ? Visibility.Visible : Visibility.Collapsed, workspace.Model.Face1ProgressVisibility);
        Assert.Equal("Tab.Read", workspace.Model.OperationText);
        Assert.Empty(workspace.Model.ProgressText);

        workspace.Controller.CancelAll();
        await loading;
        Assert.Equal(Visibility.Collapsed, workspace.Model.ProgressVisibility);
        Assert.Empty(workspace.Errors);
    }

    public static void ScpMultiFormatSelector()
    {
        var header = new VisualizerHeaderSection();
        header.SetFormats(new BuiltInImageFormatCatalog(key => key).Formats);
        header.ApplyDetection(null, null, []);

        var selector = Assert.IsType<System.Windows.Controls.ComboBox>(header.FindName("RepresentationChoice"));
        Assert.Equal(Visibility.Collapsed, selector.Visibility);
        Assert.Null(Assert.Single(selector.Items.Cast<ExplorerFormatChoice>()).Id);

        header.ApplyDetection(
            DiskImageFormatIds.AmigaDos,
            null,
            [DiskImageFormatIds.AmigaDos, DiskImageFormatIds.Ibm720, DiskImageFormatIds.AtariSt720]);

        Assert.Equal(
            new string?[] { null, DiskImageFormatIds.AmigaDos, DiskImageFormatIds.Ibm720, DiskImageFormatIds.AtariSt720 },
            selector.Items.Cast<ExplorerFormatChoice>().Select(choice => choice.Id));
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

    public static async Task ProgressiveTrackPresentationUsesMinimumInterval()
    {
        using var workspace = new Workspace();
        var source = ExplorerDocumentScenarios.Document("paced").ScpImage!;
        var tracks = Enumerable.Range(0, 4)
            .SelectMany(cylinder => Enumerable.Range(0, 2)
                .Select(head => new ScpTrack((byte)(cylinder * 2 + head), cylinder, head, [])))
            .ToArray();
        var image = new ScpImage(source.Header, tracks, true, source.FileSize);
        workspace.Read = (_, _) => Task.FromResult(image);
        var elapsed = System.Diagnostics.Stopwatch.StartNew();

        await workspace.Controller.LoadScpAsync("paced.scp");

        elapsed.Stop();
        Assert.True(elapsed.Elapsed >= TimeSpan.FromMilliseconds(180),
            $"The eight tracks were presented in only {elapsed.Elapsed.TotalMilliseconds:N0} ms.");
        Assert.Equal(
            Enumerable.Range(0, 4).SelectMany(cylinder => new[] { (Head: 0, Cylinder: cylinder), (Head: 1, Cylinder: cylinder) }),
            workspace.Controller.ScpTrackPresentationOrder);
        Assert.Equal(Visibility.Collapsed, workspace.Model.ProgressVisibility);
        Assert.Empty(workspace.Errors);
    }

    public static void SectorTrackPresentationDeductsAnalysisTime()
    {
        Assert.Equal(
            TimeSpan.FromMilliseconds(75),
            DiskImageWorkspaceController.RemainingSectorTrackPresentationDelay(TimeSpan.FromMilliseconds(25)));
        Assert.Equal(
            TimeSpan.Zero,
            DiskImageWorkspaceController.RemainingSectorTrackPresentationDelay(TimeSpan.FromMilliseconds(125)));
    }

    public static async Task SelectingScpSectorFormatRevealsEveryTrack()
    {
        using var workspace = new Workspace(withVisualizationProviders: true);
        var source = ExplorerDocumentScenarios.Document("multi");
        var scpTrack = new ScpTrack(0, 0, 0,
        [
            new ScpRevolution(8_000_000, 3, [80, 120, 160])
        ]);
        var scpImage = new ScpImage(source.ScpImage!.Header, [scpTrack], true, source.ScpImage.FileSize);
        var blocks = Enumerable.Range(0, 4)
            .Select(index => new SectorBlock(
                index,
                new SectorAddress(index / 2, index % 2, 0),
                Enumerable.Repeat((byte)(index + 1), 512).ToArray(),
                true))
            .ToArray();
        var sectorImage = new SectorImage("amiga.amigados", 512, 2, 2, 1, blocks);
        var explored = new ExploredDiskImage(
            "multi.scp",
            sectorImage,
            source.Volume,
            source.Metadata,
            detectedSectorImages: [sectorImage],
            scpImage: scpImage);
        workspace.Read = (_, _) => Task.FromResult(scpImage);

        await workspace.Controller.LoadVisualizerAsync("multi.scp", exploredImage: explored);
        var firstFluxPresentation = workspace.Controller.ScpTrackPresentationOrder.ToArray();
        await workspace.Controller.SelectVisualizerRepresentationAsync("amiga.amigados");

        Assert.Equal(4, workspace.Controller.SectorRevealedTrackCount);
        Assert.Equal(MediaRepresentationKind.Sectors, workspace.Visualizer.ActiveRepresentationKind);

        var previousSelection = workspace.Controller.SelectVisualizerRepresentationAsync("amiga.amigados");
        await workspace.Controller.SelectVisualizerRepresentationAsync(null);
        await previousSelection;
        Assert.Equal(MediaRepresentationKind.Flux, workspace.Visualizer.ActiveRepresentationKind);
        Assert.Equal(firstFluxPresentation, workspace.Controller.ScpTrackPresentationOrder);
        Assert.Single(workspace.Controller.ScpTrackPresentationOrder);
        Assert.Equal(Visibility.Collapsed, workspace.Model.ProgressVisibility);
    }

    public static async Task ExplorerAndVisualizerSynchronizeFormatsForTheSameImage()
    {
        using var workspace = new Workspace(withVisualizationProviders: true);
        var source = ExplorerDocumentScenarios.Document("shared");
        var scpImage = new ScpImage(source.ScpImage!.Header,
        [
            new ScpTrack(0, 0, 0, [new ScpRevolution(8_000_000, 3, [80, 120, 160])])
        ], true, source.ScpImage.FileSize);
        var blocks = Enumerable.Range(0, 4)
            .Select(index => new SectorBlock(
                index,
                new SectorAddress(index / 2, index % 2, 0),
                new byte[512],
                true))
            .ToArray();
        var amiga = new SectorImage(DiskImageFormatIds.AmigaDos, 512, 2, 2, 1, blocks);
        var ibm = new SectorImage(DiskImageFormatIds.Ibm720, 512, 2, 2, 1, blocks);
        var explored = new ExploredDiskImage(
            "shared.scp", amiga, source.Volume, source.Metadata,
            detectedFileSystems: [new("amiga", amiga, source.Volume)],
            detectedSectorImages: [ibm], scpImage: scpImage);
        workspace.Visualizer.Header.SetFormats(new BuiltInImageFormatCatalog(key => key).Formats);
        workspace.Explorer.SetFormats(new BuiltInImageFormatCatalog(key => key).Formats, null);
        workspace.Explore = (_, _, _) => Task.FromResult(explored);
        workspace.Read = (_, _) => Task.FromResult(scpImage);

        await workspace.Controller.LoadAsync("shared.scp");

        Assert.Equal("shared.scp", ExplorerDocumentScenarios.Find<System.Windows.Controls.TextBox>(workspace.Explorer, "PathText").Text);
        Assert.Equal(
            "DIR",
            Assert.IsType<ExplorerContentItem>(Assert.Single(
                ExplorerDocumentScenarios.Find<System.Windows.Controls.ListView>(workspace.Explorer, "ContentsList").Items.Cast<object>())).Entry.Name);
        Assert.Equal("shared.scp", workspace.Visualizer.Header.FileNameText.Text);
        Assert.Equal(MediaRepresentationKind.Flux, workspace.Visualizer.ActiveRepresentationKind);

        await workspace.Controller.SelectVisualizerRepresentationAsync(DiskImageFormatIds.Ibm720);

        Assert.Equal(DiskImageFormatIds.Ibm720, workspace.Explorer.SelectedFormatId);
        Assert.Equal(DiskImageFormatIds.Ibm720, workspace.Visualizer.Header.SelectedRepresentationId);
        Assert.Equal(MediaRepresentationKind.Sectors, workspace.Visualizer.ActiveRepresentationKind);

        workspace.Explorer.SelectDetectedFormat(DiskImageFormatIds.AmigaDos);
        await workspace.Controller.SelectExplorerRepresentationAsync();

        Assert.Equal(DiskImageFormatIds.AmigaDos, workspace.Explorer.SelectedFormatId);
        Assert.Equal(DiskImageFormatIds.AmigaDos, workspace.Visualizer.Header.SelectedRepresentationId);
        Assert.Equal(MediaRepresentationKind.Sectors, workspace.Visualizer.ActiveRepresentationKind);
    }

    public static void BothVisualizerOpenButtonsUseTheSameAction()
    {
        var visualizer = new VisualizerTabSection();
        var requests = 0;
        visualizer.OpenRequested += (_, _) => requests++;

        visualizer.Header.OpenButton.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
        visualizer.EmptyOpenImageButton.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));

        Assert.Equal(2, requests);
    }

    public static async Task SharedScpLoadClearsThenBuildsFluxDuringRecognition()
    {
        using var workspace = new Workspace(withVisualizationProviders: true);
        var first = ExplorerDocumentScenarios.Document("first");
        var firstScp = new ScpImage(first.ScpImage!.Header,
        [
            new ScpTrack(0, 0, 0, [new ScpRevolution(8_000_000, 3, [80, 120, 160])])
        ], true, first.ScpImage.FileSize);
        first = new ExploredDiskImage(
            first.SourcePath,
            first.Image,
            first.Volume,
            first.Metadata,
            scpImage: firstScp);
        workspace.Read = (_, _) => throw new InvalidOperationException("The shared pipeline must not reopen the SCP source.");
        workspace.Explore = (_, _, _) => Task.FromResult(first);
        await workspace.Controller.LoadAsync("first.scp");
        Assert.NotNull(workspace.Visualizer.CurrentDocument);

        var pendingRecognition = new TaskCompletionSource<ExploredDiskImage>();
        workspace.Explore = (_, _, _) => pendingRecognition.Task;

        var loading = workspace.Controller.LoadAsync("second.scp");
        Assert.Null(workspace.Visualizer.CurrentDocument);
        Assert.True(workspace.Visualizer.RecognitionProgressVisible);
        Assert.Equal("Tab.Read", workspace.Model.OperationText);
        Assert.Empty(workspace.Model.ProgressText);

        var second = ExplorerDocumentScenarios.Document("second");
        var secondScp = new ScpImage(second.ScpImage!.Header,
        [
            new ScpTrack(0, 0, 0, [new ScpRevolution(8_000_000, 3, [90, 130, 170])])
        ], true, second.ScpImage.FileSize);
        second = new ExploredDiskImage(
            second.SourcePath,
            second.Image,
            second.Volume,
            second.Metadata,
            scpImage: secondScp);
        await Task.Delay(100);

        Assert.Null(workspace.Visualizer.CurrentDocument);
        Assert.False(loading.IsCompleted);
        Assert.True(workspace.Visualizer.RecognitionProgressVisible);

        pendingRecognition.SetResult(second);
        await loading;

        Assert.Equal("second.scp", workspace.Visualizer.Header.FileNameText.Text);
        Assert.Equal(Visibility.Collapsed, workspace.Model.ProgressVisibility);
        Assert.Equal("Status.ReadyShort", workspace.Model.OperationText);
        Assert.Empty(workspace.Errors);
    }

    public static void ImageFoldersRemainIndependent()
    {
        var visualizerFolder = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        var explorerFolder = Path.GetTempPath();
        var requests = new List<OpenFileRequest>();
        var selectedPaths = new Queue<string>(
        [
            Path.Combine(visualizerFolder, "visualizer.scp"),
            Path.Combine(explorerFolder, "explorer.adf")
        ]);
        var dialogs = ControlledDependencies.Simulate<IFileDialogService>((method, args) =>
        {
            Assert.Equal(nameof(IFileDialogService.OpenFile), method.Name);
            requests.Add((OpenFileRequest)args[0]!);
            return selectedPaths.Dequeue();
        });
        var settings = new AppSettings
        {
            LastVisualizerImageFolder = visualizerFolder,
            LastExplorerImageFolder = explorerFolder
        };
        using var workspace = new Workspace(settings, dialogs);

        workspace.Controller.SelectVisualizerImage();
        workspace.Controller.SelectExplorerImage();

        Assert.Equal(visualizerFolder, requests[0].InitialDirectory);
        Assert.Equal(explorerFolder, requests[1].InitialDirectory);
        Assert.Equal(visualizerFolder, settings.LastVisualizerImageFolder);
        Assert.Equal(Path.TrimEndingDirectorySeparator(explorerFolder), settings.LastExplorerImageFolder);
    }

    public static void Image()
    {
        var clears = 0; var prepares = 0; string? decoder = null;
        var preparedCylinders = new List<int>();
        var image = ExplorerDocumentScenarios.Document("flux").ScpImage!;
        var renderer = ControlledDependencies.Simulate<IScpRenderer>((method, args) => {
            switch (method.Name) {
                case "ClearCache": clears++; return null;
                case "RevealTrack": return null;
                case "PrepareAsync":
                    Assert.Same(image, args[0]);
                    Assert.Equal(1, args[1]);
                    prepares++;
                    var progress = (IProgress<ScpTrackPreparation>?)args[2];
                    progress?.Report(new ScpTrackPreparation(4, 1, ScpTrackVisualState.NormalFlux));
                    progress?.Report(new ScpTrackPreparation(7, 1, ScpTrackVisualState.NormalFlux));
                    return Task.CompletedTask;
                case "set_DecoderId": decoder = (string?)args[0]; return null;
                default: throw new InvalidOperationException(method.Name);
            }
        });
        var view = new ScpDiskView(renderer);
        view.PrepareAsync().GetAwaiter().GetResult(); Assert.Equal(0, prepares);
        view.SetZoom(3); view.SetImage(image, 1);
        Assert.Equal(1, view.Zoom); Assert.Null(view.SelectedTrack); Assert.Equal(1, clears);
        view.PrepareAsync(new ImmediateProgress<ScpTrackPreparation>(item => preparedCylinders.Add(item.Cylinder))).GetAwaiter().GetResult();
        Assert.Equal(1, prepares);
        Assert.Equal([4, 7], preparedCylinders);
        view.SetDecoder("synthetic-decoder"); Assert.Equal("synthetic-decoder", decoder);
        view.SetImage(null, 0); view.PrepareAsync().GetAwaiter().GetResult();
        Assert.Equal(2, clears); Assert.Equal(1, prepares);
    }

    private sealed class ImmediateProgress<T>(Action<T> report) : IProgress<T>
    {
        public void Report(T value) => report(value);
    }
}
