using System.IO;
using GWGUI.Domain.Commands;
using GWGUI.Domain.Profiles;
using GWGUI.Scp;
using GWGUI.Scp.Containers.Scp;
using GWGUI.Domain.Formats;
using GWGUI.Domain.Naming;
using GWGUI.Domain.Hardware;
using GWGUI.Domain.Conversion;
using GWGUI.Domain.Read;
using GWGUI.Domain.Write;
using GWGUI.Domain.Maintenance;
using GWGUI.Scp.Decoding;
using GWGUI.Infrastructure.Processes;
using GWGUI.Infrastructure.Settings;
using GWGUI.Infrastructure.Hardware;
using GWGUI.Domain.Settings;
using GWGUI.App;
using GWGUI.App.Controls;
using GWGUI.App.ViewModels;
using GWGUI.App.Services;
using GWGUI.App.Rendering;
using GWGUI.App.Localization;
using SkiaSharp;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Threading;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace GWGUI.Tests;

public sealed class CoreTests
{
    private static string WindowsPowerShell => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "WindowsPowerShell", "v1.0", "powershell.exe");

    [Fact]
    public void ErrorLogWritesDiagnosticContextAndException()
    {
        var directory = Path.Combine(Path.GetTempPath(), "gwgui-error-log-" + Guid.NewGuid().ToString("N"));
        try
        {
            var path = ErrorLog.Write(new InvalidOperationException("diagnostic failure"), "unit-test", directory);
            Assert.NotNull(path);
            var content = File.ReadAllText(path!);
            Assert.Contains("Context: unit-test", content);
            Assert.Contains("InvalidOperationException", content);
            Assert.Contains("diagnostic failure", content);
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
    }

    [Fact]
    public async Task ScpRendererDrawsAHeadWithoutDependingOnWpfControls()
    {
        var revolution = new ScpRevolution(8_000_000, 2_000, Enumerable.Repeat<uint>(80, 2_000).ToArray());
        var track = new ScpTrack(0, 0, 0, [revolution]);
        var image = new ScpImage(new ScpHeader(0x24, 0, 1, 0, 0, ScpFlags.IndexAligned, 0, 2, 0, 0), [track], true, 1024);
        IScpRenderer renderer = new SkiaScpRenderer { DecoderId = "raw" };
        var preparations = new List<ScpTrackPreparation>();
        using var bitmap = new SKBitmap(256, 256);
        using var canvas = new SKCanvas(bitmap);

        await renderer.PrepareAsync(image, 0, new ImmediateProgress<ScpTrackPreparation>(preparations.Add));
        renderer.Render(canvas, new ScpRenderRequest(image, 0, track, 256, 256, new SKPoint(128, 128), 1, "No data", "Side 0"));

        Assert.NotEqual(SKColors.Transparent, bitmap.GetPixel(128, 20));
        Assert.NotEqual(new SKColor(7, 10, 14), bitmap.GetPixel(128, 20));
        var preparation = Assert.Single(preparations);
        Assert.Equal(0, preparation.Cylinder);
        Assert.Equal(0, preparation.Head);
        Assert.Equal(ScpTrackVisualState.NormalFlux, preparation.State);
        renderer.ClearCache();
    }

    [Fact]
    public async Task ScpRendererReportsAnomalyForTrackWithoutFlux()
    {
        var track = new ScpTrack(8, 4, 0, []);
        var image = new ScpImage(new ScpHeader(0x24, 0, 1, 8, 8, ScpFlags.IndexAligned, 0, 2, 0, 0), [track], true, 1024);
        IScpRenderer renderer = new SkiaScpRenderer();
        var preparations = new List<ScpTrackPreparation>();

        await renderer.PrepareAsync(image, 0, new ImmediateProgress<ScpTrackPreparation>(preparations.Add));

        var preparation = Assert.Single(preparations);
        Assert.Equal(4, preparation.Cylinder);
        Assert.Equal(ScpTrackVisualState.Anomaly, preparation.State);
    }

    [Fact]
    public void ScpRendererUsesDistinctFaithfulMediaColorsAndSides()
    {
        static SKBitmap Render(DiskMediaKind kind, int head)
        {
            var bitmap = new SKBitmap(256, 256);
            using var canvas = new SKCanvas(bitmap);
            new SkiaScpRenderer().Render(canvas, new ScpRenderRequest(null, head, null, 256, 256,
                new SKPoint(128, 128), 1, string.Empty, string.Empty, kind));
            return bitmap;
        }

        using var ddFront = Render(DiskMediaKind.ThreeHalfDd, 0);
        using var ddBack = Render(DiskMediaKind.ThreeHalfDd, 1);
        using var hdFront = Render(DiskMediaKind.ThreeHalfHd, 0);

        Assert.True(ddFront.GetPixel(20, 40).Blue > ddFront.GetPixel(20, 40).Red);
        Assert.True(hdFront.GetPixel(20, 40).Red > hdFront.GetPixel(20, 40).Blue);
        Assert.NotEqual(ddFront.GetPixel(80, 15), ddBack.GetPixel(80, 15));
        Assert.NotEqual(ddFront.GetPixel(20, 236), hdFront.GetPixel(20, 236));
        Assert.Equal(110.08f, ScpMediaGeometry.FluxRadius(256, 256, 1, DiskMediaKind.ThreeHalfDd), 2);
    }

    [Fact]
    public void VisualizerTrackClassificationDoesNotTurnTimingVariationIntoIntegrityFailure()
    {
        var timing = new FluxStructure(FluxStructureKind.TimingAnomaly, 10, 2, "timing");
        var validSector = new DecodedSector(0, 0, 1, 2, 512, true, 20);
        var invalidSector = validSector with { IntegrityValid = false };

        Assert.Equal(ScpTrackVisualState.NormalFlux, Classify(new FluxDecodeResult("test", "Test", 1, 40, [timing], [], [validSector])));
        Assert.Equal(ScpTrackVisualState.LongTransition, Classify(new FluxDecodeResult("test", "Test", 1, 40, [timing], [])));
        Assert.Equal(ScpTrackVisualState.Anomaly, Classify(new FluxDecodeResult("test", "Test", 1, 40, [], [], [invalidSector])));

        static ScpTrackVisualState Classify(FluxDecodeResult result)
        {
            var method = typeof(SkiaScpRenderer).GetMethod("Classify", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                ?? throw new MissingMethodException(typeof(SkiaScpRenderer).FullName, "Classify");
            return (ScpTrackVisualState)(method.Invoke(null, [result, 0, 0, 1])
                ?? throw new InvalidOperationException("Track classification returned no result."));
        }
    }

    [Fact]
    public void VisualizerTrackOverviewUsesProportionalQualityColors()
    {
        Assert.Equal(Color.FromRgb(36, 179, 93), TrackColor(new(0, 0, ScpTrackVisualState.NormalFlux, 11, 0, 0)));
        Assert.Equal(Color.FromRgb(100, 201, 107), TrackColor(new(0, 0, ScpTrackVisualState.Anomaly, 10, 1, 0)));
        Assert.Equal(Color.FromRgb(245, 158, 61), TrackColor(new(0, 0, ScpTrackVisualState.Anomaly, 1, 8, 0)));
        Assert.Equal(Color.FromRgb(255, 75, 96), TrackColor(new(0, 0, ScpTrackVisualState.Anomaly, 0, 11, 0)));

        static Color TrackColor(ScpTrackPreparation preparation)
        {
            var method = typeof(VisualizerTrackOverview).GetMethod("ColorFor", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                ?? throw new MissingMethodException(typeof(VisualizerTrackOverview).FullName, "ColorFor");
            return (Color)(method.Invoke(null, [preparation]) ?? throw new InvalidOperationException("Track color returned no result."));
        }
    }

    [Fact]
    public void ScpInspectorPresenterBuildsLocalizedTrackAnalysisOutsideWindow()
    {
        static string Localize(string key, object[] arguments) => arguments.Length == 0 ? key : $"{key}({string.Join(',', arguments)})";
        var presenter = new ScpInspectorPresenter(new FluxDecoderRegistry(), Localize);
        var revolution = new ScpRevolution(8_000_000, 4, [80, 80, 160, 80]);
        var track = new ScpTrack(11, 5, 1, [revolution]);
        var image = new ScpImage(new ScpHeader(0x24, 0, 1, 11, 11, ScpFlags.IndexAligned, 0, 2, 0, 0), [track], true, 1024);

        var text = presenter.Build(image, track, "raw");

        Assert.Contains("Visual.Track(1,5,11)", text);
        Assert.Contains("Visual.Revolution(1,4", text);
        Assert.Contains("Visual.DecoderName.raw", text);
        Assert.Contains("Visual.AnalysedRevolution(1)", text);
    }

    [Fact]
    public async Task ScpDocumentLoaderBuildsLocalizedModelThroughReaderContract()
    {
        static string Localize(string key, object[] arguments) => arguments.Length == 0 ? key : $"{key}({string.Join(',', arguments)})";
        var header = new ScpHeader(0x24, 0, 1, 4, 5, ScpFlags.IndexAligned, 0, 2, 1, 0);
        var image = new ScpImage(header, [new ScpTrack(4, 2, 0, []), new ScpTrack(5, 2, 1, [])], true, 1024);
        var reader = new StubScpReader(image); var loader = new ScpDocumentLoader(reader, Localize);

        var document = await loader.LoadAsync(@"F:\captures\demo.scp");

        Assert.Same(image, document.Image); Assert.Equal("demo.scp", document.FileName); Assert.True(document.Heads.SetEquals([0, 1]));
        Assert.Contains("Visual.Summary(2.4,2,1,50,Visual.ChecksumValid)", document.Summary);
        Assert.Equal(@"F:\captures\demo.scp", reader.Path);
    }

    [Fact]
    public void MainWindowStateViewModelPublishesSharedStatusChanges()
    {
        var model = new MainWindowViewModel("No hardware", "Ready");
        var changed = new List<string?>();
        model.PropertyChanged += (_, args) => changed.Add(args.PropertyName);

        model.HardwareText = "Drive 1";
        model.ProfileText = "Profile: Default";
        model.ProfileVisibility = Visibility.Visible;
        model.ProgressVisibility = Visibility.Visible;
        model.ProgressValue = 50;
        model.Face0ProgressVisibility = Visibility.Visible;
        model.Face0ProgressValue = 25;
        model.Face1ProgressVisibility = Visibility.Visible;
        model.Face1ProgressValue = 30;

        Assert.Equal("Drive 1", model.HardwareText);
        Assert.Equal(Visibility.Visible, model.ProfileVisibility);
        Assert.Equal(50, model.ProgressValue);
        Assert.Equal(25, model.Face0ProgressValue);
        Assert.Equal(30, model.Face1ProgressValue);
        Assert.Contains(nameof(model.HardwareText), changed);
        Assert.Contains(nameof(model.ProgressValue), changed);
        Assert.Contains(nameof(model.Face0ProgressValue), changed);
        Assert.Contains(nameof(model.Face1ProgressValue), changed);
    }

    [Fact]
    public void MainWindowXamlLoadsWithStatusBindingsAndAlignmentMenu()
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                var app = Application.Current as GWGUI.App.App ?? new GWGUI.App.App();
                app.InitializeComponent();
                ThemeManager.Apply(AppTheme.Light);
                var lightWindow = Assert.IsType<System.Windows.Media.SolidColorBrush>(app.Resources["WindowBrush"]).Color;
                var lightText = Assert.IsType<System.Windows.Media.SolidColorBrush>(app.Resources["TextBrush"]).Color;
                ThemeManager.Apply(AppTheme.Dark);
                var darkWindow = Assert.IsType<System.Windows.Media.SolidColorBrush>(app.Resources["WindowBrush"]).Color;
                var darkText = Assert.IsType<System.Windows.Media.SolidColorBrush>(app.Resources["TextBrush"]).Color;
                Assert.NotEqual(lightWindow, darkWindow);
                Assert.NotEqual(lightText, darkText);
                ThemeManager.Apply(AppTheme.System);
                Assert.IsType<System.Windows.Media.SolidColorBrush>(app.Resources["AccentBrush"]);
                var dialogs = new RecordingMessageDialogService();
                var files = new RecordingFileDialogService { FolderResult = @"F:\Images" };
                var business = new RecordingBusinessDialogService { ProfileNameResult = "Test profile" };
                var navigation = new RecordingWindowNavigationService();
                var settingsStore = new RecordingSettingsStore();
                var window = new MainWindow(dialogs, files, business, navigation, settingsStore: settingsStore);

                Assert.IsType<MainWindowViewModel>(window.DataContext);
                Assert.Equal("align", Assert.IsType<System.Windows.Controls.MenuItem>(window.FindName("AlignMenuItem")).Tag);
                Assert.Contains('_', Assert.IsType<string>(Assert.IsType<System.Windows.Controls.MenuItem>(window.FindName("OptionsMenuItem")).Header));
                Assert.Contains('_', Assert.IsType<string>(Assert.IsType<System.Windows.Controls.MenuItem>(window.FindName("HelpMenuItem")).Header));
                var hardwareText = Assert.IsType<System.Windows.Controls.TextBlock>(window.FindName("HardwareStatusText"));
                var progress = Assert.IsType<System.Windows.Controls.ProgressBar>(window.FindName("OperationProgress"));
                var readFileName = Assert.IsType<System.Windows.Controls.TextBox>(window.FindName("ReadFileName"));
                var readExtension = Assert.IsType<System.Windows.Controls.TextBox>(window.FindName("ReadExtensionText"));
                var readFamily = Assert.IsType<System.Windows.Controls.ComboBox>(window.FindName("ReadFamilyCombo"));
                var readFormat = Assert.IsType<System.Windows.Controls.ComboBox>(window.FindName("ReadFormatCombo"));
                var readImageExtension = Assert.IsType<System.Windows.Controls.ComboBox>(window.FindName("ReadExtensionCombo"));
                var readRevs = Assert.IsType<System.Windows.Controls.CheckBox>(window.FindName("ReadRevsEnabled"));
                var writeNoVerify = Assert.IsType<System.Windows.Controls.CheckBox>(window.FindName("WriteNoVerify"));
                var convertTags = Assert.IsType<System.Windows.Controls.CheckBox>(window.FindName("ConvertTags"));
                Assert.NotNull(BindingOperations.GetBindingExpression(hardwareText, System.Windows.Controls.TextBlock.TextProperty));
                Assert.NotNull(BindingOperations.GetBindingExpression(progress, System.Windows.Controls.Primitives.RangeBase.ValueProperty));
                Assert.Equal("Read.FileName", BindingOperations.GetBindingExpression(readFileName, System.Windows.Controls.TextBox.TextProperty)?.ParentBinding.Path.Path);
                Assert.Equal("Read.Revs.Enabled", BindingOperations.GetBindingExpression(readRevs, System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty)?.ParentBinding.Path.Path);
                Assert.Equal("Write.NoVerify.Enabled", BindingOperations.GetBindingExpression(writeNoVerify, System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty)?.ParentBinding.Path.Path);
                Assert.Equal("Conversion.AddTags", BindingOperations.GetBindingExpression(convertTags, System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty)?.ParentBinding.Path.Path);
                var writeSource = Assert.IsType<System.Windows.Controls.TextBox>(window.FindName("WriteSourceText"));
                var convertSource = Assert.IsType<System.Windows.Controls.TextBox>(window.FindName("ConvertSourceText"));
                var convertOutput = Assert.IsType<System.Windows.Controls.TextBox>(window.FindName("ConvertOutputName"));
                var commandPreview = Assert.IsType<System.Windows.Controls.TextBox>(window.FindName("CommandPreview"));
                var logOutput = Assert.IsType<System.Windows.Controls.TextBox>(window.FindName("LogOutput"));
                var mainTabs = Assert.IsType<System.Windows.Controls.TabControl>(window.FindName("MainTabs"));
                var readExecute = Assert.IsType<System.Windows.Controls.Button>(window.FindName("ReadExecuteButton"));
                foreach (var named in new FrameworkElement[] { readFileName, readExtension, writeSource, convertSource, convertOutput, commandPreview, logOutput, mainTabs })
                    Assert.False(string.IsNullOrWhiteSpace(AutomationProperties.GetName(named)));
                Assert.NotEqual(AutomationProperties.GetName(commandPreview), AutomationProperties.GetName(logOutput));
                Assert.NotNull(new System.Windows.Automation.Peers.TextBoxAutomationPeer(readFileName).GetPattern(PatternInterface.Value));
                Assert.NotNull(new System.Windows.Automation.Peers.TabControlAutomationPeer(mainTabs).GetPattern(PatternInterface.Selection));
                Assert.NotNull(new System.Windows.Automation.Peers.ButtonAutomationPeer(readExecute).GetPattern(PatternInterface.Invoke));
                Assert.True(readFileName.IsTabStop); Assert.True(readExecute.IsTabStop);
                Assert.All(mainTabs.Items.OfType<System.Windows.Controls.TabItem>(), tab => Assert.True(tab.Focusable));
                {
                    var settingsFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
                    var catalog = Assert.IsAssignableFrom<IImageFormatCatalog>(typeof(MainWindow).GetField("_formatCatalog", settingsFlags)!.GetValue(window));
                    readFamily.ItemsSource = catalog.Formats.Where(format => format.Family != "Raw").Select(format => format.Family).Distinct().Order().ToArray();
                    readFamily.SelectedIndex = 0;
                    var persisted = Assert.IsType<AppSettings>(typeof(MainWindow).GetField("_settings", settingsFlags)!.GetValue(window));
                    persisted.Read.UseKnownFormat = true;
                    persisted.Read.FormatId = "atarist.720";
                    persisted.Read.ImageExtension = ".msa";
                    typeof(MainWindow).GetMethod("RestoreReadSettings", settingsFlags)!.Invoke(window, null);
                    Assert.Equal("atarist.720", Assert.IsType<DiskFormat>(readFormat.SelectedItem).Id);
                    Assert.Equal(".msa", Assert.IsType<ImageExtension>(readImageExtension.SelectedItem).Extension);
                    readImageExtension.SelectedItem = Assert.IsType<DiskFormat>(readFormat.SelectedItem).Extensions.Single(extension => extension.Extension == ".st");
                    typeof(MainWindow).GetMethod("CaptureReadSettings", settingsFlags)!.Invoke(window, null);
                    Assert.Equal(".st", persisted.Read.ImageExtension);
                }
                var model = Assert.IsType<MainWindowViewModel>(window.DataContext);
                var orphanMainSettings = Assert.IsType<AppSettings>(typeof(MainWindow).GetField("_settings", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(window));
                orphanMainSettings.Controllers = [];
                orphanMainSettings.Drives = [new DriveSettings { ControllerUsbId = "GW-MISSING", Size = "3.5", Density = "HD" }];
                typeof(MainWindow).GetMethod("RefreshHardwareSelector", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.Invoke(window, null);
                Assert.Empty(Assert.IsType<System.Windows.Controls.ComboBox>(window.FindName("HardwareSelector")).Items);
                Assert.False(string.IsNullOrWhiteSpace(model.HardwareText));
                static System.Windows.Controls.CheckBox Probe(MainWindowViewModel dataContext, string path)
                {
                    var probe = new System.Windows.Controls.CheckBox { DataContext = dataContext };
                    BindingOperations.SetBinding(probe, System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty, new Binding(path) { Mode = BindingMode.TwoWay });
                    return probe;
                }
                var readProbe = Probe(model, "Read.Revs.Enabled"); var writeProbe = Probe(model, "Write.NoVerify.Enabled"); var convertProbe = Probe(model, "Conversion.AddTags");
                readProbe.IsChecked = true; writeProbe.IsChecked = true; convertProbe.IsChecked = true;
                Assert.True(model.Read.Revs.Enabled, "Read checkbox did not update its source");
                Assert.True(model.Write.NoVerify.Enabled, "Write checkbox did not update its source");
                Assert.True(model.Conversion.AddTags, "Conversion checkbox did not update its source");
                model.Read.Revs.Enabled = false; model.Write.NoVerify.Enabled = false; model.Conversion.AddTags = false;
                Assert.False(readProbe.IsChecked); Assert.False(writeProbe.IsChecked); Assert.False(convertProbe.IsChecked);
                var privateFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
                typeof(MainWindow).GetMethod("BuildConversionFormats", privateFlags)!.Invoke(window, [null, null]);
                var conversionFormats = Assert.IsType<ConversionFormatsSection>(window.FindName("ConvertFormatsBlock"));
                Assert.NotEmpty(conversionFormats.MachineChoices);
                Assert.All(conversionFormats.VisibleFormats, item => Assert.IsType<ConversionFormatPresentation>(item));
                model.Conversion.SetFormat("ibm.720", true, [".img"]);
                typeof(MainWindow).GetMethod("BuildConversionFormats", privateFlags)!.Invoke(window, [null, null]);
                Assert.Contains(conversionFormats.SelectedOutputLines,
                    line => line.Contains("IBM PC", StringComparison.CurrentCultureIgnoreCase)
                        && line.EndsWith("IMG", StringComparison.Ordinal));
                typeof(MainWindow).GetMethod("ShowAdvancedValidation", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                    .Invoke(window, [new ArgumentException("invalid value"), "Validation"]);
                var request = Assert.Single(dialogs.Requests);
                Assert.Equal("Validation", request.Title);
                Assert.Equal(UserDialogButtons.Ok, request.Buttons);
                Assert.Equal(UserDialogIcon.Warning, request.Icon);
                Assert.DoesNotContain("invalid value", request.Message, StringComparison.Ordinal);
                Assert.Contains(LocExtension.Get("Common.Unknown"), request.Message, StringComparison.CurrentCulture);
                typeof(MainWindow).GetMethod("BrowseReadFolder_Click", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                    .Invoke(window, [window, new RoutedEventArgs()]);
                Assert.Equal(@"F:\Images", model.Read.Folder);
                var folderRequest = Assert.Single(files.FolderRequests);
                Assert.False(string.IsNullOrWhiteSpace(folderRequest.Title));
                typeof(MainWindow).GetMethod("SaveReadProfile_Click", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                    .Invoke(window, [window, new RoutedEventArgs()]);
                Assert.Equal(1, business.ProfilePromptCount);
                var profileCombo = Assert.IsType<System.Windows.Controls.ComboBox>(window.FindName("ReadProfileCombo"));
                var savedReadProfile = Assert.Single(profileCombo.Items.Cast<OperationProfile>(), profile => profile.Name == "Test profile" && profile.Operation == OperationKind.Read);
                Assert.Equal("known", savedReadProfile.Values["result"]);
                Assert.Equal("atarist.720", savedReadProfile.Values["format"]);
                Assert.Equal(".st", savedReadProfile.Values["extension"]);
                Assert.Equal(@"F:\Images", savedReadProfile.Values["folder"]);

                var writeFormatCombo = Assert.IsType<System.Windows.Controls.ComboBox>(window.FindName("WriteFormatCombo"));
                var appCatalog = Assert.IsAssignableFrom<IImageFormatCatalog>(typeof(MainWindow).GetField("_formatCatalog", privateFlags)!.GetValue(window));
                var explorer = Assert.IsType<ExplorerSection>(window.FindName("DiskExplorer"));
                Assert.Null(explorer.FormatChoices[0].Id);
                Assert.Equal(appCatalog.Formats.Select(format => format.Id), explorer.FormatChoices.Skip(1).Select(format => format.Id));
                writeFormatCombo.ItemsSource = appCatalog.Formats.Where(format => format.Family != "Raw").ToArray();
                writeFormatCombo.SelectedItem = appCatalog.Formats.Single(format => format.Id == "amiga.amigados");
                typeof(MainWindow).GetMethod("SaveWriteProfile_Click", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                    .Invoke(window, [window, new RoutedEventArgs()]);
                Assert.Equal(2, business.ProfilePromptCount);
                var writeProfileCombo = Assert.IsType<System.Windows.Controls.ComboBox>(window.FindName("WriteProfileCombo"));
                var savedWriteProfile = Assert.Single(writeProfileCombo.Items.Cast<OperationProfile>(), profile => profile.Name == "Test profile" && profile.Operation == OperationKind.Write);
                Assert.Equal("amiga.amigados", savedWriteProfile.Values["format"]);
                var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
                typeof(MainWindow).GetMethod("About_Click", flags)!.Invoke(window, [window, new RoutedEventArgs()]);
                typeof(MainWindow).GetMethod("LogHistory_Click", flags)!.Invoke(window, [window, new RoutedEventArgs()]);
                typeof(MainWindow).GetMethod("Preferences_Click", flags)!.Invoke(window, [window, new RoutedEventArgs()]);
                var settings = Assert.IsType<AppSettings>(typeof(MainWindow).GetField("_settings", flags)!.GetValue(window));
                settings.GwExecutablePath = WindowsPowerShell;
                typeof(MainWindow).GetMethod("ToolCommand_Click", flags)!.Invoke(window, [new System.Windows.Controls.MenuItem { Tag = "rpm" }, new RoutedEventArgs()]);
                Assert.Equal(1, navigation.AboutCount);
                Assert.Single(navigation.LogDirectories);
                Assert.Single(navigation.OptionsSettings);
                Assert.Equal("rpm", Assert.Single(navigation.ToolRequests).Verb);

                settings.Controllers = [new() { UsbId = "GW-OFFLINE", LastPort = "COM8", Model = "Greaseweazle V4.1", IsAvailable = false }];
                settings.Drives = [new() { ControllerUsbId = "GW-OFFLINE", Selection = "A", Size = "3.5", Density = "HD" }];
                typeof(MainWindow).GetMethod("RefreshHardwareSelector", flags)!.Invoke(window, null);
                Assert.False(readExecute.IsEnabled);
                Assert.False(Assert.IsType<System.Windows.Controls.Button>(window.FindName("WriteExecuteButton")).IsEnabled);
                Assert.False(Assert.IsType<System.Windows.Controls.Button>(window.FindName("EraseExecuteButton")).IsEnabled);
                Assert.False(Assert.IsType<System.Windows.Controls.Button>(window.FindName("CleanExecuteButton")).IsEnabled);
                Assert.True(Assert.IsType<System.Windows.Controls.Button>(window.FindName("ConvertExecuteButton")).IsEnabled);
                var dialogCount = dialogs.Requests.Count;
                typeof(MainWindow).GetMethod("ToolCommand_Click", flags)!.Invoke(window, [new System.Windows.Controls.MenuItem { Tag = "rpm" }, new RoutedEventArgs()]);
                Assert.Single(navigation.ToolRequests);
                Assert.Equal(dialogCount + 1, dialogs.Requests.Count);
                Assert.Equal(UserDialogIcon.Warning, dialogs.Requests[^1].Icon);

                var busyDialogs = new RecordingMessageDialogService();
                var busyNavigation = new RecordingWindowNavigationService();
                var busyRunner = new BusyRunner();
                var busyWindow = new MainWindow(busyDialogs, navigation: busyNavigation, runner: busyRunner, settingsStore: new RecordingSettingsStore());
                var busySettings = Assert.IsType<AppSettings>(typeof(MainWindow).GetField("_settings", flags)!.GetValue(busyWindow));
                busySettings.GwExecutablePath = WindowsPowerShell;
                typeof(MainWindow).GetMethod("ToolCommand_Click", flags)!.Invoke(busyWindow, [new System.Windows.Controls.MenuItem { Tag = "rpm" }, new RoutedEventArgs()]);
                Assert.Empty(busyNavigation.ToolRequests);
                Assert.Contains("Greaseweazle", Assert.Single(busyDialogs.Requests).Message);
                var wpfNavigation = new WpfWindowNavigationService(busyWindow, runner: busyRunner);
                Assert.Same(busyRunner, typeof(WpfWindowNavigationService).GetField("_runner", flags)!.GetValue(wpfNavigation));
                busyWindow.Close();
                Dispatcher.CurrentDispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);

                var failingSaveDialogs = new RecordingMessageDialogService();
                var failingSaveWindow = new MainWindow(failingSaveDialogs, settingsStore: new FailingSettingsStore());
                failingSaveWindow.Close();
                Dispatcher.CurrentDispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                var saveFailure = Assert.Single(failingSaveDialogs.Requests);
                Assert.Equal(UserDialogIcon.Warning, saveFailure.Icon);
                Assert.DoesNotContain("test save failure", saveFailure.Message, StringComparison.Ordinal);
                Assert.Contains(LocExtension.Get("App.SettingsSaveFailed").Split('{')[0].Trim(), saveFailure.Message, StringComparison.CurrentCulture);

                var optionsSettings = new AppSettings
                {
                    Controllers = [new ControllerSettings { UsbId = "GW-UI-TEST", LastPort = "COM3", Model = "Greaseweazle", IsAvailable = true }]
                };
                var optionsWindow = new OptionsWindow(optionsSettings, settingsStore: new DelayedSettingsStore());
                var optionsNavigation = Assert.IsType<System.Windows.Controls.TabControl>(optionsWindow.FindName("Navigation"));
                var imagesFolder = Assert.IsType<System.Windows.Controls.TextBox>(optionsWindow.FindName("ImagesFolderText"));
                var language = Assert.IsType<System.Windows.Controls.ComboBox>(optionsWindow.FindName("LanguageCombo"));
                var theme = Assert.IsType<System.Windows.Controls.ComboBox>(optionsWindow.FindName("ThemeCombo"));
                var tagPattern = Assert.IsType<System.Windows.Controls.TextBox>(optionsWindow.FindName("TagPatternText"));
                var gwPath = Assert.IsType<System.Windows.Controls.TextBox>(optionsWindow.FindName("GwPathText"));
                var hostToolsProgress = Assert.IsType<System.Windows.Controls.ProgressBar>(optionsWindow.FindName("HostToolsProgress"));
                var drives = Assert.IsType<System.Windows.Controls.ListBox>(optionsWindow.FindName("DrivesGrid"));
                var profiles = Assert.IsType<System.Windows.Controls.ListBox>(optionsWindow.FindName("ReadProfilesList"));
                foreach (var named in new FrameworkElement[] { optionsNavigation, imagesFolder, language, theme, tagPattern, gwPath, hostToolsProgress, drives, profiles })
                    Assert.False(string.IsNullOrWhiteSpace(AutomationProperties.GetName(named)));
                Assert.NotNull(new TabControlAutomationPeer(optionsNavigation).GetPattern(PatternInterface.Selection));
                Assert.NotNull(new TextBoxAutomationPeer(imagesFolder).GetPattern(PatternInterface.Value));
                Assert.NotNull(new ComboBoxAutomationPeer(language).GetPattern(PatternInterface.Selection));
                ThemeManager.Apply(AppTheme.Dark);
                optionsWindow.Show();
                optionsWindow.UpdateLayout();
                Assert.Single(drives.Items);
                Assert.True(Assert.IsType<HardwareRow>(drives.Items[0]).Available);
                var hardwareFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
                var configuredHardware = new AppSettings
                {
                    Controllers = [new ControllerSettings { UsbId = "GW-ROW-TEST", LastPort = "COM3", IsAvailable = true }],
                    Drives = [new DriveSettings { ControllerUsbId = "GW-ROW-TEST", Size = "3.5", Density = "HD", NominalRpm = 300 }]
                };
                var rowWindow = new OptionsWindow(configuredHardware, settingsStore: new RecordingSettingsStore());
                var rowList = Assert.IsType<System.Windows.Controls.ListBox>(rowWindow.FindName("DrivesGrid"));
                rowList.SelectedIndex = 0;
                typeof(OptionsWindow).GetMethod("AddDrive_Click", hardwareFlags)!.Invoke(rowWindow, [rowWindow, new RoutedEventArgs()]);
                Assert.Equal(2, rowList.Items.Count);
                var temporaryRow = Assert.IsType<HardwareRow>(rowList.Items[1]);
                Assert.Null(temporaryRow.DriveId);
                typeof(OptionsWindow).GetMethod("RemoveHardwareRow", hardwareFlags)!.Invoke(rowWindow, [temporaryRow]);
                Assert.Single(rowList.Items);
                Assert.Single(Assert.IsType<List<DriveSettings>>(typeof(OptionsWindow).GetField("_drives", hardwareFlags)!.GetValue(rowWindow)));
                rowWindow.Close();

                var orphanSettings = new AppSettings
                {
                    UnconfiguredControllers = [new ControllerSettings { UsbId = "GW-ORPHAN", LastPort = "COM4", IsAvailable = true }],
                    Drives = [new DriveSettings { ControllerUsbId = "GW-ORPHAN", Size = "3.5", Density = "HD", NominalRpm = 300 }]
                };
                var orphanWindow = new OptionsWindow(orphanSettings, settingsStore: new RecordingSettingsStore());
                var orphanList = Assert.IsType<System.Windows.Controls.ListBox>(orphanWindow.FindName("DrivesGrid"));
                Assert.EndsWith("2", Assert.IsType<HardwareRow>(orphanList.Items[0]).ReaderLabel);
                typeof(OptionsWindow).GetMethod("MergeUnconfigured", hardwareFlags)!.Invoke(orphanWindow,
                    [new[] { new ControllerSettings { UsbId = "GW-ORPHAN", LastPort = "COM4", IsAvailable = true } }]);
                typeof(OptionsWindow).GetMethod("RefreshHardwareRows", hardwareFlags)!.Invoke(orphanWindow, null);
                var repairedRow = Assert.IsType<HardwareRow>(Assert.Single(orphanList.Items));
                Assert.EndsWith("1", repairedRow.ReaderLabel);
                Assert.NotNull(repairedRow.DriveId);
                Assert.True(repairedRow.Configured);
                orphanWindow.Close();
                var expectedDarkText = Assert.IsType<System.Windows.Media.SolidColorBrush>(app.Resources["TextBrush"]).Color;
                var expectedDarkControl = Assert.IsType<System.Windows.Media.SolidColorBrush>(app.Resources["ControlBrush"]).Color;
                Assert.Equal(expectedDarkText, Assert.IsType<System.Windows.Media.SolidColorBrush>(Assert.IsType<System.Windows.Controls.CheckBox>(optionsWindow.FindName("UseTagsCheck")).Foreground).Color);
                Assert.Equal(expectedDarkControl, Assert.IsType<System.Windows.Media.SolidColorBrush>(theme.Background).Color);
                Assert.Equal(expectedDarkText, Assert.IsType<System.Windows.Media.SolidColorBrush>(Assert.IsType<System.Windows.Controls.TabItem>(optionsNavigation.Items[0]).Foreground).Color);
                var generalScroller = Assert.IsType<System.Windows.Controls.ScrollViewer>(optionsWindow.FindName("GeneralScrollViewer"));
                var recentTags = Assert.IsType<System.Windows.Controls.ListBox>(optionsWindow.FindName("RecentTagPatterns"));
                Assert.True(generalScroller.ScrollableHeight <= 0, $"General page requires {generalScroller.ScrollableHeight} DIPs of scrolling at the normal window size.");
                var recentScroller = GetScrollViewer(recentTags);
                Assert.True(recentScroller.ScrollableHeight <= 0, $"Recent tag patterns require {recentScroller.ScrollableHeight} DIPs of scrolling.");
                optionsWindow.Close();
                for (var attempt = 0; attempt < 100 && optionsWindow.IsVisible; attempt++)
                {
                    Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.Background);
                    Thread.Sleep(10);
                }
                Assert.False(optionsWindow.IsVisible, "Options did not close after its asynchronous save.");
                ThemeManager.Apply(AppTheme.System);

                for (var cycle = 0; cycle < 4; cycle++)
                {
                    var repeatedOptions = new OptionsWindow(new AppSettings(), settingsStore: new DelayedSettingsStore());
                    repeatedOptions.Show();
                    var closeButton = Assert.IsType<System.Windows.Controls.Button>(repeatedOptions.FindName("CloseButton"));
                    closeButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                    for (var attempt = 0; attempt < 100 && repeatedOptions.IsVisible; attempt++)
                    {
                        Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.Background);
                        Thread.Sleep(10);
                    }
                    Assert.False(repeatedOptions.IsVisible, $"Options did not close on cycle {cycle + 1}.");
                }

                var hardwareWindow = new HardwareUnavailableWindow([new ControllerSettings { UsbId = "GW-TEST", LastPort = "COM3", Model = "Greaseweazle" }]);
                Assert.Single(Assert.IsType<System.Windows.Controls.ListBox>(hardwareWindow.FindName("MissingControllers")).Items);
                Assert.False(string.IsNullOrWhiteSpace(Assert.IsType<System.Windows.Controls.Button>(hardwareWindow.FindName("RetryButton")).Content?.ToString()));
                Assert.False(string.IsNullOrWhiteSpace(Assert.IsType<System.Windows.Controls.Button>(hardwareWindow.FindName("SettingsButton")).Content?.ToString()));
                Assert.False(string.IsNullOrWhiteSpace(Assert.IsType<System.Windows.Controls.Button>(hardwareWindow.FindName("ContinueButton")).Content?.ToString()));
                hardwareWindow.Close();

                foreach (var verb in new[] { "info", "bandwidth", "rpm", "seek", "pin", "reset", "delays", "update", "align" })
                {
                    var toolWindow = new GwToolWindow("gw.exe", verb, runner: new ScriptedRunner());
                    var rawOutput = Assert.IsType<System.Windows.Controls.TextBox>(toolWindow.FindName("RawOutput"));
                    var toolCommand = Assert.IsType<System.Windows.Controls.TextBox>(toolWindow.FindName("CommandText"));
                    Assert.False(string.IsNullOrWhiteSpace(AutomationProperties.GetName(rawOutput)));
                    Assert.False(string.IsNullOrWhiteSpace(AutomationProperties.GetName(toolCommand)));
                    var toolFields = Assert.IsType<Dictionary<string, System.Windows.Controls.TextBox>>(
                        typeof(GwToolWindow).GetField("_fields", privateFlags)!.GetValue(toolWindow));
                    Assert.All(toolFields.Values, field => Assert.False(string.IsNullOrWhiteSpace(AutomationProperties.GetName(field))));
                    toolWindow.Close();
                }

                var profileNameWindow = new ProfileNameWindow();
                Assert.False(string.IsNullOrWhiteSpace(AutomationProperties.GetName(
                    Assert.IsType<System.Windows.Controls.TextBox>(profileNameWindow.FindName("NameText")))));
                profileNameWindow.Close();

                var conflictWindow = new ConversionConflictWindow([new ConversionOutput("ibm.720", ".ima", @"F:\Images\disk.ima", true)]);
                Assert.False(string.IsNullOrWhiteSpace(AutomationProperties.GetName(
                    Assert.IsType<System.Windows.Controls.DataGrid>(conflictWindow.FindName("ConflictsGrid")))));
                conflictWindow.Close();

                var readConflictWindow = new ReadConflictWindow(@"F:\Images\disk.scp");
                foreach (var name in new[] { "OverwriteButton", "NextNumberButton", "EditNameButton" })
                {
                    var button = Assert.IsType<System.Windows.Controls.Button>(readConflictWindow.FindName(name));
                    Assert.False(string.IsNullOrWhiteSpace(AutomationProperties.GetName(button)));
                }
                readConflictWindow.Close();

                var logWindow = new LogHistoryWindow(Path.GetTempPath());
                var filesList = Assert.IsType<System.Windows.Controls.ListBox>(logWindow.FindName("FilesList"));
                var logContent = Assert.IsType<System.Windows.Controls.TextBox>(logWindow.FindName("ContentText"));
                Assert.False(string.IsNullOrWhiteSpace(AutomationProperties.GetName(filesList)));
                Assert.False(string.IsNullOrWhiteSpace(AutomationProperties.GetName(logContent)));
                logWindow.Close();

                window.Close();
                Dispatcher.CurrentDispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                Assert.Equal(1, settingsStore.SaveCount);
            }
            catch (Exception exception) { failure = exception; }
            finally { Dispatcher.CurrentDispatcher.InvokeShutdown(); }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(60)), "The WPF smoke test timed out.");
        if (failure is not null) throw failure;
    }

    [Fact]
    public async Task RunnerCapturesUnicodeStandardErrorAndExitCode()
    {
        var runner = new GreaseweazleRunner();
        var command = new GwCommand(WindowsPowerShell, "-NoProfile", ["-Command", "[Console]::OutputEncoding=[Text.Encoding]::UTF8; Write-Output 'café 漢字'; [Console]::Error.WriteLine('échec Ω'); exit 7"]);
        var result = await runner.RunAsync(command);
        Assert.Equal(7, result.ExitCode);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Output, line => line.Stream == GwOutputStream.Standard && line.Text.Contains("café 漢字"));
        Assert.Contains(result.Output, line => line.Stream == GwOutputStream.Error && line.Text.Contains("échec Ω"));
    }

    [Fact]
    public async Task BatchExecutorContinuesAfterFailuresAndKeepsAnExactSummary()
    {
        var runner = new ScriptedRunner(
            new GwExecutionResult(0, false, TimeSpan.Zero, []),
            new GwExecutionResult(2, false, TimeSpan.Zero, []),
            new GwExecutionResult(0, false, TimeSpan.Zero, []));
        var items = new[] { "one", "two", "three" }.Select(label => new GwBatchItem(label, new GwCommand("gw.exe", "convert", [label]))).ToArray();
        var started = new List<string>();

        var result = await new GwBatchExecutor(runner).RunAsync(items, itemStarting: item => started.Add(item.Label));

        Assert.False(result.WasCancelled);
        Assert.Equal(2, result.SuccessfulCount);
        Assert.Equal(["two"], result.FailedLabels);
        Assert.Equal(["one", "two", "three"], started);
        Assert.Equal(3, runner.Commands.Count);
    }

    [Fact]
    public async Task BatchExecutorStopsImmediatelyAfterACommandReportsCancellation()
    {
        var runner = new ScriptedRunner(
            new GwExecutionResult(0, false, TimeSpan.Zero, []),
            new GwExecutionResult(-1, true, TimeSpan.Zero, []),
            new GwExecutionResult(0, false, TimeSpan.Zero, []));
        var items = new[] { "one", "two", "three" }.Select(label => new GwBatchItem(label, new GwCommand("gw.exe", "convert", [label]))).ToArray();

        var result = await new GwBatchExecutor(runner).RunAsync(items);

        Assert.True(result.WasCancelled);
        Assert.Equal(1, result.SuccessfulCount);
        Assert.Empty(result.FailedLabels);
        Assert.Equal(2, runner.Commands.Count);
    }

    [Fact]
    public async Task OperationCoordinatorOwnsCancellationAndRejectsConcurrentWork()
    {
        var coordinator = new OperationCoordinator();
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var operation = coordinator.RunAsync(async token =>
        {
            started.SetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, token);
            return 1;
        });
        await started.Task;

        Assert.True(coordinator.IsRunning);
        var concurrent = await coordinator.RunAsync(_ => Task.FromResult(2));
        Assert.IsType<InvalidOperationException>(concurrent.Error);
        coordinator.RequestCancellation();
        var outcome = await operation;

        Assert.IsType<TaskCanceledException>(outcome.Error);
        Assert.False(coordinator.IsRunning);
    }

    [Fact]
    public async Task OperationCoordinatorReturnsSuccessAndFailureAsExplicitOutcomes()
    {
        var coordinator = new OperationCoordinator();

        var success = await coordinator.RunAsync(_ => Task.FromResult(42));
        var failure = await coordinator.RunAsync<int>(_ => throw new IOException("broken"));

        Assert.True(success.HasResult);
        Assert.Equal(42, success.Result);
        Assert.Null(success.Error);
        Assert.False(failure.HasResult);
        Assert.Equal("broken", Assert.IsType<IOException>(failure.Error).Message);
        Assert.False(coordinator.IsRunning);
    }

    [Fact]
    public void OperationResultPresenterDistinguishesSingleSuccessFailureAndCancellation()
    {
        var presenter = new OperationResultPresenter();
        static OperationOutcome<GwExecutionResult> Outcome(int code, bool cancelled = false) =>
            new(true, new GwExecutionResult(code, cancelled, TimeSpan.FromSeconds(2), []), null);

        var success = presenter.Present(Outcome(0));
        var failure = presenter.Present(Outcome(3));
        var cancelled = presenter.Present(Outcome(-1, true));

        Assert.Equal(OperationResultState.Success, success.State);
        Assert.Equal(OperationResultState.Error, failure.State);
        Assert.Equal(OperationResultState.Cancelled, cancelled.State);
        Assert.Equal(["Operation.Succeeded", "Operation.Finished"], success.Messages.Select(message => message.ResourceKey));
        Assert.Equal([0, "0:00:02"], success.Messages[1].Arguments);
        Assert.All(success.Messages, message => Assert.True(message.StartOnNewLine));
    }

    [Fact]
    public void OperationResultPresenterBuildsExactPartialBatchSummary()
    {
        var command = new GwCommand("gw.exe", "convert", []);
        var items = new[]
        {
            new GwBatchItemResult(new GwBatchItem("disk.ima", command), new GwExecutionResult(0, false, TimeSpan.Zero, [])),
            new GwBatchItemResult(new GwBatchItem("disk.img", command), new GwExecutionResult(2, false, TimeSpan.Zero, []))
        };

        var presentation = new OperationResultPresenter().Present(new OperationOutcome<GwBatchExecutionResult>(true, new(items, false), null));

        Assert.Equal(OperationResultState.Error, presentation.State);
        Assert.Collection(presentation.Messages,
            summary => { Assert.Equal("Conversion.Summary", summary.ResourceKey); Assert.Equal([1, 1], summary.Arguments); Assert.True(summary.StartOnNewLine); },
            failures => { Assert.Equal("Conversion.Failures", failures.ResourceKey); Assert.Equal(["disk.img"], failures.Arguments); Assert.False(failures.StartOnNewLine); });
    }

    [Fact]
    public void OperationResultPresenterTurnsThrownExceptionsIntoLocalizedErrors()
    {
        var presentation = new OperationResultPresenter().Present(new OperationOutcome<GwExecutionResult>(false, null, new IOException("broken")));

        Assert.Equal(OperationResultState.Error, presentation.State);
        var message = Assert.Single(presentation.Messages);
        Assert.Equal("Error.Unexpected", message.ResourceKey);
        var detail = Assert.IsType<string>(Assert.Single(message.Arguments));
        Assert.DoesNotContain("broken", detail, StringComparison.Ordinal);
        Assert.False(message.StartOnNewLine);
    }

    [Fact]
    public void ConversionConflictResolverAppliesSkipOverwriteAndNumberChoices()
    {
        var untouched = new ConversionOutput("ibm.720", ".ima", "plain.ima", true);
        var overwrite = new ConversionOutput("ibm.720", ".img", "replace.img", false);
        var skip = new ConversionOutput("atarist.720", ".st", "skip.st", true);
        var number = new ConversionOutput("amiga.amigados", ".adf", "number.adf", true);
        var conflicts = new[] { overwrite, skip, number };
        var decisions = new[]
        {
            new ConversionConflictDecision(overwrite, ConversionConflictChoice.Overwrite),
            new ConversionConflictDecision(skip, ConversionConflictChoice.Skip),
            new ConversionConflictDecision(number, ConversionConflictChoice.Number)
        };

        var resolved = ConversionConflictResolver.Apply([untouched, overwrite, skip, number], conflicts, decisions, path => "next-" + path);

        Assert.Equal([untouched, overwrite, number with { OutputPath = "next-number.adf" }], resolved);
    }

    [Fact]
    public async Task RunnerRejectsASecondConcurrentCommand()
    {
        var runner = new GreaseweazleRunner();
        using var cancellation = new CancellationTokenSource();
        var first = runner.RunAsync(new GwCommand(WindowsPowerShell, "-NoProfile", ["-Command", "Start-Sleep -Seconds 20"]), cancellationToken: cancellation.Token);
        Assert.True(SpinWait.SpinUntil(() => runner.IsRunning, TimeSpan.FromSeconds(2)));
        await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync(new GwCommand(WindowsPowerShell, "-NoProfile", ["-Command", "exit 0"])));
        cancellation.Cancel();
        Assert.True((await first).WasCancelled);
        Assert.False(runner.IsRunning);
    }

    [Fact]
    public async Task RunnerReassemblesAFragmentedUtf8Line()
    {
        var runner = new GreaseweazleRunner();
        var command = new GwCommand(WindowsPowerShell, "-NoProfile", ["-Command", "[Console]::OutputEncoding=[Text.Encoding]::UTF8; [Console]::Out.Write('frag'); Start-Sleep -Milliseconds 50; [Console]::Out.WriteLine('menté')"]);
        var result = await runner.RunAsync(command);
        Assert.True(result.IsSuccess);
        Assert.Contains(result.Output, line => line.Text == "fragmenté");
    }

    [Fact]
    public void ConversionCompatibilityUsesTheDetectedGeometryForSectorImages()
    {
        var catalog = new BuiltInImageFormatCatalog();
        var detection = new ImageFormatDetector(catalog).Detect("disk.ima", 737280);
        var outputs = ConversionSourceCompatibility.GetOutputs(catalog, ".ima", detection);
        Assert.Collection(outputs, output => Assert.Equal("ibm.720", output.Id));
    }

    [Fact]
    public void ConversionCompatibilityKeepsAllDecodableFormatsForRawFlux()
    {
        var catalog = new BuiltInImageFormatCatalog();
        var detection = new ImageFormatDetector(catalog).Detect("disk.scp", 1234);
        var outputs = ConversionSourceCompatibility.GetOutputs(catalog, ".scp", detection);
        Assert.Contains(outputs, output => output.Id == "amiga.amigados");
        Assert.Contains(outputs, output => output.Id == "atarist.720");
        Assert.Contains(outputs, output => output.Id == "ibm.720");
    }

    [Fact]
    public void ConversionFormatPresenterPinsSelectionsAndReturnsUncheckedItemsToTheirNaturalGroup()
    {
        var catalog = new BuiltInImageFormatCatalog();
        var rare = catalog.Formats.First(format => format.Id != "raw.scp" && !format.IsCommon);
        var selected = new HashSet<string> { "ibm.720", rare.Id };
        var extensions = new Dictionary<string, HashSet<string>> { ["ibm.720"] = [".img"] };
        var presenter = new ConversionFormatPresenter();

        var pinned = presenter.Build(catalog, null, null, selected, extensions);

        Assert.Equal(2, pinned.TakeWhile(item => item.Group == ConversionFormatGroup.Selected).Count());
        Assert.All(pinned.Take(2), item => Assert.True(item.IsSelected));
        Assert.True(pinned.Single(item => item.Format.Id == "ibm.720").ExplicitExtensions.SetEquals([".img"]));

        var unselected = presenter.Build(catalog, null, null, new HashSet<string>(), extensions);
        Assert.Equal(ConversionFormatGroup.Common, unselected.Single(item => item.Format.Id == "ibm.720").Group);
        Assert.Equal(ConversionFormatGroup.Rare, unselected.Single(item => item.Format.Id == rare.Id).Group);
        Assert.Equal(unselected.OrderBy(item => item.Group).ThenBy(item => item.Format.DisplayName, StringComparer.CurrentCulture), unselected);
    }

    [Fact]
    public void ConversionFormatPresenterDisablesSelectionsThatDoNotMatchDetectedSectorGeometry()
    {
        var catalog = new BuiltInImageFormatCatalog();
        var detection = new ImageFormatDetector(catalog).Detect("disk.ima", 737280);
        var selected = new HashSet<string> { "ibm.720", "atarist.720" };

        var items = new ConversionFormatPresenter().Build(catalog, ".ima", detection, selected, new Dictionary<string, HashSet<string>>());

        Assert.True(items.Single(item => item.Format.Id == "ibm.720").IsSelected);
        var incompatible = items.Single(item => item.Format.Id == "atarist.720");
        Assert.False(incompatible.IsCompatible);
        Assert.False(incompatible.IsSelected);
        Assert.NotEqual(ConversionFormatGroup.Selected, incompatible.Group);
    }

    [Fact]
    public void RawScpReadNeverAddsAStaleKnownFormat()
    {
        var command = ReadCommandBuilder.Build(new ReadRequest("gw.exe", "disk.scp", ReadResultKind.RawScp, "acorn.adfs.800", []));
        Assert.DoesNotContain("--format", command.Arguments);
        Assert.Equal(["disk.scp"], command.Arguments);
    }

    [Fact]
    public void KnownFormatReadRequiresAndAddsItsFormat()
    {
        var command = ReadCommandBuilder.Build(new ReadRequest("gw.exe", "disk.adf", ReadResultKind.KnownFormat, "amiga.amigados", []));
        Assert.Equal(["--format", "amiga.amigados", "disk.adf"], command.Arguments);
        Assert.Throws<ArgumentException>(() => ReadCommandBuilder.Build(new ReadRequest("gw.exe", "disk.adf", ReadResultKind.KnownFormat, null, [])));
    }

    [Fact]
    public void ADriveArgumentIsOnlyUsedWhenSeveralDrivesAreConfigured()
    {
        var first = new DriveSettings { ControllerUsbId = "GW-1", Selection = "A" };
        var second = new DriveSettings { ControllerUsbId = "GW-1", Selection = "B" };
        Assert.Null(HardwareRoutingPolicy.DriveArgument([first], first));
        Assert.Equal("B", HardwareRoutingPolicy.DriveArgument([first, second], second));
    }

    [Fact]
    public void OneDriveOnEachControllerDoesNotEmitAnUnnecessaryDriveArgument()
    {
        var first = new DriveSettings { ControllerUsbId = "GW-1", Selection = "A" };
        var second = new DriveSettings { ControllerUsbId = "GW-2", Selection = "B" };

        Assert.Null(HardwareRoutingPolicy.DriveArgument([first, second], first));
        Assert.Null(HardwareRoutingPolicy.DriveArgument([first, second], second));
    }

    [Fact]
    public void DeviceArgumentIsOnlyUsedWhenSeveralConfiguredControllersAreAvailable()
    {
        var firstController = new ControllerSettings { UsbId = "GW-1", LastPort = "COM3", IsAvailable = true };
        var secondController = new ControllerSettings { UsbId = "GW-2", LastPort = "COM5", IsAvailable = true };
        var firstDrive = new DriveSettings { ControllerUsbId = "GW-1", Selection = "A" };
        var secondDrive = new DriveSettings { ControllerUsbId = "GW-2", Selection = "A" };

        Assert.Null(HardwareRoutingPolicy.DeviceArgument([firstController], [firstDrive], firstDrive));
        Assert.Equal("COM5", HardwareRoutingPolicy.DeviceArgument([firstController, secondController], [firstDrive, secondDrive], secondDrive));

        secondController.IsAvailable = false;
        Assert.Null(HardwareRoutingPolicy.DeviceArgument([firstController, secondController], [firstDrive, secondDrive], firstDrive));
    }

    [Fact]
    public void AutomaticDriveSelectionAssignsHiddenAAndBPerController()
    {
        var first = new DriveSettings { ControllerUsbId = "GW-1", Selection = "legacy" };
        var second = new DriveSettings { ControllerUsbId = "GW-1", Selection = "legacy" };
        var other = new DriveSettings { ControllerUsbId = "GW-2", Selection = "legacy" };
        var drives = new List<DriveSettings> { first, second, other };

        HardwareRoutingPolicy.AssignAutomaticDriveSelections(drives, "GW-1");

        Assert.Equal("A", first.Selection);
        Assert.Equal("B", second.Selection);
        Assert.Equal("legacy", other.Selection);
    }

    [Theory]
    [InlineData("A", 0)]
    [InlineData("Z", 25)]
    [InlineData("AA", 26)]
    [InlineData("AB", 27)]
    public void AlphabeticSequenceInputParsesLikeItsDisplayedValue(string text, long expected)
    {
        Assert.True(SequenceFormatter.TryParse(text, SequenceKind.Alphabetic, out var value));
        Assert.Equal(expected, value);
        Assert.Equal(text, SequenceFormatter.Format(value, SequenceKind.Alphabetic, 1));
    }

    [Fact]
    public void RawContainerIdsAreNeverSentAsGwFormatArguments()
    {
        var write = WriteCommandBuilder.Build(new WriteRequest("gw.exe", "disk.scp", "raw.scp", []));
        Assert.Equal(["disk.scp"], write.Arguments);
        var convert = ConversionCommandBuilder.Build("gw.exe", "disk.scp", new ConversionOutput("raw.hfe", ".hfe", "disk.hfe", true));
        Assert.Equal(["disk.scp", "disk.hfe"], convert.Arguments);
        Assert.Equal("raw.gcr", GwFormatArgument.FromCatalogId("raw.gcr"));
    }

    [Fact]
    public void PortableMarkerMovesSettingsNextToTheApplication()
    {
        var directory = Path.Combine(Path.GetTempPath(), "gwgui-portable-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            Assert.Equal(Path.Combine("roaming", "GW GUI"), StoragePaths.ResolveDataDirectory(directory, "roaming"));
            File.WriteAllText(Path.Combine(directory, "portable.flag"), "");
            Assert.Equal(Path.Combine(directory, "Data"), StoragePaths.ResolveDataDirectory(directory, "roaming"));
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public void LegacyHostToolsFolderMovesToGreaseweazleFolderWithoutExtraNesting()
    {
        var directory = Path.Combine(Path.GetTempPath(), "gwgui-host-path-" + Guid.NewGuid().ToString("N"));
        var legacy = Path.Combine(directory, "host-tools");
        var preferred = Path.Combine(directory, "Greaseweazle");
        var executable = Path.Combine(legacy, "1.23", "gw.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(executable)!);
        File.WriteAllText(executable, "fake");
        try
        {
            StoragePaths.MigrateHostToolsDirectory(legacy, preferred);
            Assert.True(File.Exists(Path.Combine(preferred, "1.23", "gw.exe")));
            Assert.False(Directory.Exists(legacy));
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
    }

    [Fact]
    public async Task VersionOneSettingsMigrateFormatIdentifiersAndCollections()
    {
        var directory = Path.Combine(Path.GetTempPath(), "gwgui-settings-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "settings.json");
        try
        {
            await File.WriteAllTextAsync(path, """{"SchemaVersion":1,"Read":{"FormatId":"amiga.amigadoshd"},"Conversion":{"SelectedFormats":["amiga.amigadoshd"],"ExplicitExtensions":{"amiga.amigadoshd":[".adf"]}},"Profiles":[{"Operation":"Convert","Name":"HD","EnabledOptions":["format:amiga.amigadoshd"],"Values":{"extensions:amiga.amigadoshd":".adf"}}]}""");
            var settings = await new JsonSettingsStore(path).LoadAsync();

            Assert.Equal(SettingsMigrator.CurrentVersion, settings.SchemaVersion);
            Assert.Equal("amiga.amigados_hd", settings.Read.FormatId);
            Assert.Contains("amiga.amigados_hd", settings.Conversion.SelectedFormats);
            Assert.Contains("amiga.amigados_hd", settings.Conversion.ExplicitExtensions.Keys);
            Assert.Contains("format:amiga.amigados_hd", settings.Profiles[0].EnabledOptions);
            Assert.Contains("extensions:amiga.amigados_hd", settings.Profiles[0].Values.Keys);
            Assert.NotNull(settings.Write);
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public async Task InvalidSettingsRecoverFromLastBackupAndArePreserved()
    {
        var directory = Path.Combine(Path.GetTempPath(), "gwgui-settings-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "settings.json");
        try
        {
            var store = new JsonSettingsStore(path);
            await store.SaveAsync(new AppSettings { Language = "fr" });
            await store.SaveAsync(new AppSettings { Language = "en" });
            await File.WriteAllTextAsync(path, "{ invalid json");

            var recovered = await store.LoadAsync();

            Assert.Equal("fr", recovered.Language);
            Assert.Contains(Directory.GetFiles(directory), file => file.Contains(".invalid-", StringComparison.Ordinal));
            Assert.Contains("\"Language\": \"fr\"", await File.ReadAllTextAsync(path));
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public async Task LastDiskImageFolderIsPersistedIndependentlyFromReadDestination()
    {
        var directory = Path.Combine(Path.GetTempPath(), "gwgui-last-image-folder-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "settings.json");
        try
        {
            var store = new JsonSettingsStore(path);
            await store.SaveAsync(new AppSettings
            {
                DefaultImagesFolder = @"F:\Read destination",
                LastDiskImageFolder = @"F:\Disk images\Atari"
            });

            var restored = await store.LoadAsync();

            Assert.Equal(@"F:\Read destination", restored.DefaultImagesFolder);
            Assert.Equal(@"F:\Disk images\Atari", restored.LastDiskImageFolder);
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public async Task OperationLogWriterRotatesAndKeepsCommandAndOutput()
    {
        var directory = Path.Combine(Path.GetTempPath(), "gwgui-log-" + Guid.NewGuid().ToString("N"));
        try
        {
            var writer = new RotatingOperationLogWriter(directory, maximumBytes: 220, maximumFiles: 3);
            var command = new GwCommand("gw.exe", "read", ["disk.scp"]);
            for (var index = 0; index < 5; index++)
            {
                var line = new GwOutputLine(DateTimeOffset.UtcNow, GwOutputStream.Standard, $"T{index}.0: " + new string('x', 90));
                await writer.WriteAsync(command, new GwExecutionResult(0, false, TimeSpan.FromSeconds(1), [line]));
            }

            var files = Directory.GetFiles(directory, "operations*.log");
            Assert.Equal(3, files.Length);
            var current = await File.ReadAllTextAsync(Path.Combine(directory, "operations.log"));
            Assert.Contains("gw.exe read disk.scp", current);
            Assert.Contains("T4.0", current);
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
    }

    [Fact]
    public async Task ConsoleLogsUseOneFilePerActionAndTrimOldLines()
    {
        var directory = Path.Combine(Path.GetTempPath(), "gwgui-console-log-" + Guid.NewGuid().ToString("N"));
        var settings = new OperationLogSettings { Enabled = true, MaximumKilobytes = 1, KeepArchives = false };
        try
        {
            var logger = new ConsoleLogSession(directory, () => settings);
            await logger.BeginAsync("read", "gw.exe read disk.scp");
            for (var index = 0; index < 40; index++) await logger.AppendAsync($"T{index}.0: {new string('x', 80)}");

            var path = Path.Combine(directory, "read.log");
            Assert.True(File.Exists(path));
            Assert.True(new FileInfo(path).Length <= 1024);
            var text = await File.ReadAllTextAsync(path);
            Assert.Contains("T39.0", text);
            Assert.DoesNotContain("T0.0", text);
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
    }

    [Fact]
    public async Task ConsoleLogsCanArchiveWithTimestampAndBeDisabled()
    {
        var directory = Path.Combine(Path.GetTempPath(), "gwgui-console-archive-" + Guid.NewGuid().ToString("N"));
        var settings = new OperationLogSettings { Enabled = true, MaximumKilobytes = 1, KeepArchives = true };
        try
        {
            var logger = new ConsoleLogSession(directory, () => settings);
            await logger.BeginAsync("write", "gw.exe write disk.adf");
            for (var index = 0; index < 20; index++) await logger.AppendAsync(new string('x', 100));
            Assert.NotEmpty(Directory.GetFiles(directory, "write-*.log"));
            Assert.True(File.Exists(Path.Combine(directory, "write.log")));

            settings.Enabled = false;
            await logger.BeginAsync("convert", "gw.exe convert source.scp target.ima");
            await logger.AppendAsync("hidden");
            Assert.False(File.Exists(Path.Combine(directory, "convert.log")));
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
    }

    [Fact]
    public async Task ConsoleLogSettingsAreIndependentForEachAction()
    {
        var directory = Path.Combine(Path.GetTempPath(), "gwgui-action-logs-" + Guid.NewGuid().ToString("N"));
        var settings = new OperationLogSettings();
        settings.GetOrCreate("read").MaximumKilobytes = 2;
        settings.GetOrCreate("write").Enabled = false;
        try
        {
            var read = new ConsoleLogSession(directory, () => settings);
            await read.BeginAsync("read", "gw.exe read disk.scp");
            await read.AppendAsync("read output");
            var write = new ConsoleLogSession(directory, () => settings);
            await write.BeginAsync("write", "gw.exe write disk.scp");
            await write.AppendAsync("write output");

            Assert.True(File.Exists(Path.Combine(directory, "read.log")));
            Assert.False(File.Exists(Path.Combine(directory, "write.log")));
            Assert.Equal(2, settings.ForAction("read").MaximumKilobytes);
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
    }

    [Fact]
    public async Task CancelledReadOutputCleanerDeletesOnlyTheRequestedFile()
    {
        var directory = Path.Combine(Path.GetTempPath(), "gwgui-cancelled-read-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var incomplete = Path.Combine(directory, "incomplete.scp");
        var other = Path.Combine(directory, "keep.scp");
        try
        {
            await File.WriteAllTextAsync(incomplete, "partial");
            await File.WriteAllTextAsync(other, "keep");

            Assert.Null(CancelledOutputCleaner.TryDelete(incomplete));
            Assert.False(File.Exists(incomplete));
            Assert.True(File.Exists(other));
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
    }

    [Fact]
    public void GwHelpCapabilitiesAreParsedBySection()
    {
        const string help = """
            options:
              --format FORMAT

            FORMAT options:
              acorn.adfs.800  amiga.amigados  amiga.amigadoshd
              atarist.720     ibm.720         ibm.scan

            Supported file suffixes:
              .adf  .hfe  .ima  .img  .scp
            """;

        var capabilities = GwFormatCapabilitiesParser.ParseReadHelp(help);

        Assert.Contains("amiga.amigados", capabilities.FormatIds);
        Assert.Contains("ibm.scan", capabilities.FormatIds);
        Assert.DoesNotContain("--format", capabilities.FormatIds);
        Assert.Equal(6, capabilities.FormatIds.Count);
        Assert.Contains(".scp", capabilities.ImageExtensions);
        Assert.Equal(5, capabilities.ImageExtensions.Count);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("unrelated output")]
    public void MissingHelpSectionsReturnUnknownCapabilities(string? help)
    {
        Assert.False(GwFormatCapabilitiesParser.ParseReadHelp(help).IsKnown);
    }

    [Fact]
    public void RuntimeCapabilitiesFilterCuratedFormatsAndExtensions()
    {
        var capabilities = new GwFormatCapabilities(
            new HashSet<string>(["ibm.720"], StringComparer.OrdinalIgnoreCase),
            new HashSet<string>([".scp", ".img"], StringComparer.OrdinalIgnoreCase));

        var catalog = new CapabilityAwareImageFormatCatalog(new BuiltInImageFormatCatalog(), capabilities);

        Assert.Contains(catalog.Formats, format => format.Id == "raw.scp");
        var ibm = Assert.Single(catalog.Formats, format => format.Id == "ibm.720");
        Assert.Equal(".img", Assert.Single(ibm.Extensions).Extension);
        Assert.True(ibm.Extensions[0].IsDefault);
        Assert.DoesNotContain(catalog.Formats, format => format.Id == "atarist.720");
    }

    [Fact]
    public void ExplorerDetailsSwitchBetweenDiskAndCentralListItemInformation()
    {
        var child = new GWGUI.Scp.FileSystems.FileSystemEntry(
            "README.TXT", GWGUI.Scp.FileSystems.FileSystemEntryKind.File, 42,
            new DateTimeOffset(1993, 8, 20, 14, 37, 0, TimeSpan.Zero), "Test comment", 0, 1, true, [], [65, 66]);
        var folder = new GWGUI.Scp.FileSystems.FileSystemEntry(
            "DOCS", GWGUI.Scp.FileSystems.FileSystemEntryKind.Directory, 0, null, "", 0, 2, true, [child]);
        var volume = new GWGUI.Scp.FileSystems.FileSystemVolume(
            "TEST", "Atari TOS FAT12", 737280, 249 * 1024, null, null, [folder], ["warning"]);

        var image = new GWGUI.Scp.SectorImages.SectorImage("atarist.720", 512, 80, 2, 9, []);
        var diskDetails = ExplorerDetailsPresenter.ForDisk(new GWGUI.Scp.Images.ExploredDiskImage("test.st", image, volume));
        var fileDetails = ExplorerDetailsPresenter.ForItem(new ExplorerContentItem(child));
        var folderDetails = ExplorerDetailsPresenter.ForItem(new ExplorerContentItem(folder));

        Assert.Equal("TEST", diskDetails.Title);
        Assert.Equal(ExplorerIconKind.DiskImage, diskDetails.IconKind);
        Assert.Contains(diskDetails.Rows, row => row.Key == "Explorer.FileSystem" && row.Value == "Atari TOS FAT12");
        Assert.Contains(diskDetails.Rows, row => row.Key == "Explorer.System" && row.Value == "Atari ST");
        Assert.Contains(diskDetails.Rows, row => row.Key == "Explorer.Protection" && row.Value == "\u2014");
        Assert.Contains(diskDetails.Rows, row => row.Key == "Explorer.Entries" && row.Value == "2");
        Assert.Equal("README.TXT", fileDetails.Title);
        Assert.Equal(ExplorerIconKind.Text, fileDetails.IconKind);
        Assert.Contains(fileDetails.Rows, row => row.Key == "Explorer.Comment" && row.Value == "Test comment");
        Assert.Contains(folderDetails.Rows, row => row.Key == "Explorer.Entries" && row.Value == "1");
    }

    [Fact]
    public void VisualizationUsesGwOnlyForAdvertisedInputAndScpOutput()
    {
        var catalog = new BuiltInImageFormatCatalog();
        var detector = new ImageFormatDetector(catalog);
        var capabilities = new GwFormatCapabilities(
            new HashSet<string>(["atarist.720"], StringComparer.OrdinalIgnoreCase),
            new HashSet<string>([".st", ".scp"], StringComparer.OrdinalIgnoreCase));
        var st = detector.Detect("disk.st", 737280);
        var atr = detector.Detect("disk.atr", 92176);

        Assert.True(GwVisualizationPolicy.CanConvertToScp("disk.st", st, capabilities));
        Assert.False(GwVisualizationPolicy.CanConvertToScp("disk.atr", atr, capabilities));
        Assert.False(GwVisualizationPolicy.CanConvertToScp("disk.st", st, GwFormatCapabilities.Unknown));
        Assert.False(GwVisualizationPolicy.CanConvertToScp("disk.st", st,
            capabilities with { ImageExtensions = new HashSet<string>([".st"], StringComparer.OrdinalIgnoreCase) }));
        var dskCapabilities = capabilities with { ImageExtensions = new HashSet<string>([".dsk", ".edsk", ".scp"], StringComparer.OrdinalIgnoreCase) };
        Assert.True(GwVisualizationPolicy.CanConvertToScp("disk.dsk", detector.Detect("disk.dsk", 194816), dskCapabilities));
        Assert.True(GwVisualizationPolicy.CanConvertToScp("disk.EDSK", detector.Detect("disk.EDSK", 194816), dskCapabilities));
    }

    [Fact]
    public void AtariStHighDensityUsesTheCompatibleGwIbmGeometry()
    {
        var capabilities = new GwFormatCapabilities(
            new HashSet<string>(["ibm.1440"], StringComparer.OrdinalIgnoreCase),
            new HashSet<string>([".st", ".scp"], StringComparer.OrdinalIgnoreCase));
        var catalog = new CapabilityAwareImageFormatCatalog(new BuiltInImageFormatCatalog(), capabilities);
        var detection = new ImageFormatDetector(catalog).Detect("disk.st", 1474560);

        Assert.Equal("atarist.1440", detection.Format?.Id);
        Assert.Equal("ibm.1440", GwFormatArgument.FromCatalogId(detection.Format?.Id));
        Assert.True(GwVisualizationPolicy.CanConvertToScp("disk.st", detection, capabilities));
    }

    [Fact]
    public void AtariAtrVisualizationUsesOnlyNativeGwNinetyAndOneThirtyFormats()
    {
        var capabilities = new GwFormatCapabilities(
            new HashSet<string>(["atari.90", "atari.130"], StringComparer.OrdinalIgnoreCase),
            new HashSet<string>([".img", ".scp"], StringComparer.OrdinalIgnoreCase));
        var detector = new ImageFormatDetector(new BuiltInImageFormatCatalog());

        Assert.True(GwVisualizationPolicy.CanConvertToScp("disk.atr", detector.Detect("disk.atr", 92176), capabilities));
        Assert.True(GwVisualizationPolicy.CanConvertToScp("disk.atr", detector.Detect("disk.atr", 133136), capabilities));
        Assert.False(GwVisualizationPolicy.CanConvertToScp("disk.atr", detector.Detect("disk.atr", 183952), capabilities));
    }

    [Fact]
    public void RuntimeCapabilitiesKeepCuratedFormatsAndExposeUnknownDiskDefinitions()
    {
        var capabilities = new GwFormatCapabilities(
            new HashSet<string>(["ibm.720", "dec.rx02", "ensoniq.mirage"], StringComparer.OrdinalIgnoreCase),
            new HashSet<string>([".scp", ".img"], StringComparer.OrdinalIgnoreCase));

        var catalog = new CapabilityAwareImageFormatCatalog(new BuiltInImageFormatCatalog(), capabilities);

        var dec = Assert.Single(catalog.Formats, format => format.Id == "dec.rx02");
        Assert.Equal("DEC", dec.Family);
        Assert.Equal("DEC RX02 — 512 KiB", dec.DisplayName);
        Assert.False(dec.IsCommon);
        Assert.Equal(".img", Assert.Single(dec.Extensions).Extension);
        Assert.Equal("DEC-RX02", dec.Tag);
        Assert.Contains(".scp", dec.CompatibleSourceExtensions!);
        Assert.Contains(catalog.Formats, format => format.Id == "ensoniq.mirage");
    }

    [Fact]
    public void CustomDiskDefsReaderResolvesPrefixesAndImports()
    {
        var directory = Path.Combine(Path.GetTempPath(), "gwgui-diskdefs-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            File.WriteAllText(Path.Combine(directory, "child.cfg"), "disk format1\nend\n");
            File.WriteAllText(Path.Combine(directory, "root.cfg"), "disk local\nend\nimport vendor. \"child.cfg\"\n");

            var formats = DiskDefsFormatReader.Read(Path.Combine(directory, "root.cfg"));

            Assert.Equal(new HashSet<string>(["local", "vendor.format1"], StringComparer.OrdinalIgnoreCase), formats);
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public void CuratedCatalogContainsOfficialIbmAndAtariProfiles()
    {
        var catalog = new BuiltInImageFormatCatalog();
        string[] ibm = ["ibm.160", "ibm.180", "ibm.320", "ibm.360", "ibm.720", "ibm.800", "ibm.1200", "ibm.1440", "ibm.1680", "ibm.dmf", "ibm.2880", "ibm.scan"];
        string[] atari = ["atarist.180", "atarist.360", "atarist.400", "atarist.440", "atarist.720", "atarist.800", "atarist.810", "atarist.880"];

        Assert.All(ibm.Concat(atari), id => Assert.Contains(catalog.Formats, format => format.Id == id));
        Assert.Contains(catalog.Formats, format => format.Id == "amiga.amigados_hd");
        Assert.DoesNotContain(catalog.Formats, format => format.Id == "amiga.amigadoshd");
        Assert.All(catalog.Formats.Where(format => format.Family == "IBM PC"), format =>
            Assert.Equal(".ima", Assert.Single(format.Extensions, extension => extension.IsDefault).Extension));
    }

    [Fact]
    public void CatalogDisplayNamesAreProvidedByTheActiveLocalizer()
    {
        var catalog = new BuiltInImageFormatCatalog(key => "localized:" + key);
        var format = Assert.Single(catalog.Formats, item => item.Id == "ibm.720");
        Assert.Equal("localized:Format.ibm.720", format.DisplayName);
        Assert.Equal("localized:Extension.ima", format.Extensions[0].DisplayName);
    }

    [Fact]
    public void CuratedCatalogContainsEveryInternallyExplorableMachineFamily()
    {
        var catalog = new BuiltInImageFormatCatalog();
        string[] formats =
        [
            "amstrad.cpc", "amstrad.pcw",
            "acorn.dfs.ss", "acorn.dfs.ss80", "acorn.dfs.ds", "acorn.dfs.ds80",
            "epson.qx10.320", "epson.qx10.396", "epson.qx10.399", "epson.qx10.400", "epson.qx10.logo",
            "msx.1d", "msx.1dd", "msx.2d", "msx.2dd",
            "dec.rx02", "ucsd.ibm.mfm", "commodore900.coherent",
            "applelisa.office", "applelisa.macworks"
        ];

        Assert.All(formats, id => Assert.Contains(catalog.Formats, format => format.Id == id));
        Assert.Contains(catalog.Formats, format => format.Family == "Amstrad");
    }

    [Fact]
    public void AutomaticClassificationReplacesOrClearsThePreviousImageSelection()
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                var selector = new DiskClassificationSelector();
                selector.SetCatalog(new BuiltInImageFormatCatalog().Formats);

                selector.ApplyDetection("atarist.360", null);
                Assert.Equal("Atari ST", selector.SelectedMachine);
                Assert.Equal("atarist.360", selector.SelectedFormatId);

                selector.ApplyDetection("unknown", null);
                Assert.Null(selector.SelectedMachine);
                Assert.Null(selector.SelectedFormatId);
            }
            catch (Exception exception) { failure = exception; }
            finally { Dispatcher.CurrentDispatcher.InvokeShutdown(); }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(10)), "The automatic classification test timed out.");
        if (failure is not null) throw failure;
    }

    [Fact]
    public void DisplayCommandQuotesPathsWithSpaces()
    {
        var command = new GwCommand("C:\\GW Tools\\gw.exe", "read", ["F:\\Disquettes été\\Tilt n°117 漢字.scp"]);
        Assert.Equal("\"C:\\GW Tools\\gw.exe\" read \"F:\\Disquettes été\\Tilt n°117 漢字.scp\"", command.ToDisplayString());
        Assert.Equal("F:\\Disquettes été\\Tilt n°117 漢字.scp", command.Arguments[0]);
    }

    [Fact]
    public void DefaultProfileHasNoOptionalArguments()
    {
        var profile = OperationProfile.Default(OperationKind.Read);
        Assert.True(profile.IsSystem);
        Assert.Empty(profile.EnabledOptions);
        Assert.Empty(profile.Values);
    }

    [Fact]
    public void ScpHeaderReaderReadsCoreMetadata()
    {
        byte[] header = [(byte)'S', (byte)'C', (byte)'P', 0x24, 0, 5, 0, 83, 0, 0, 0, 0, 0, 0, 0, 0];
        var result = ScpHeaderReader.Read(header);
        Assert.Equal(84, result.TrackCount);
        Assert.Equal(5, result.Revolutions);
        Assert.Equal(0, result.Heads);
    }

    [Fact]
    public void ScpReaderReadsTrackRevolutionAndBigEndianFluxOverflow()
    {
        var data = new byte[0x2b0 + 16 + 6];
        data[0] = (byte)'S'; data[1] = (byte)'C'; data[2] = (byte)'P'; data[3] = 0x25; data[5] = 1; data[6] = 0; data[7] = 0;
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x10, 4), 0x2b0);
        data[0x2b0] = (byte)'T'; data[0x2b1] = (byte)'R'; data[0x2b2] = (byte)'K'; data[0x2b3] = 0;
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x2b4, 4), 8_000_000);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x2b8, 4), 3);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x2bc, 4), 16);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(0x2c0, 2), 100);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(0x2c2, 2), 0);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(0x2c4, 2), 50);
        uint checksum = 0; foreach (var value in data.AsSpan(0x10)) checksum = unchecked(checksum + value);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x0c, 4), checksum);
        var image = new ScpReader().Read(data);
        Assert.True(image.ChecksumValid);
        Assert.Equal([100u, 65_586u], image.Tracks[0].Revolutions[0].FluxIntervals);
        Assert.Equal(300d, image.Tracks[0].Revolutions[0].Rpm(image.Header.ResolutionNanoseconds), 3);
    }

    [Theory]
    [InlineData(0, "A")]
    [InlineData(25, "Z")]
    [InlineData(26, "AA")]
    [InlineData(27, "AB")]
    public void AlphabeticSequenceContinuesAfterZ(long value, string expected) =>
        Assert.Equal(expected, SequenceFormatter.Format(value, SequenceKind.Alphabetic, 1));

    [Fact]
    public void ExplicitExtensionCanReplaceImplicitImaDefault()
    {
        var format = new BuiltInImageFormatCatalog().Formats.Single(x => x.Id == "ibm.720");
        Assert.Equal(".ima", format.Extensions.Single(x => x.IsDefault).Extension);
        Assert.Contains(format.Extensions, x => x.Extension == ".img");
    }

    [Fact]
    public void ScpCanBeDecodedIntoAllKnownOutputFamilies()
    {
        var outputs = new BuiltInImageFormatCatalog().GetCompatibleOutputs(".scp");
        Assert.Contains(outputs, x => x.Id == "amiga.amigados");
        Assert.Contains(outputs, x => x.Id == "atarist.720");
        Assert.Contains(outputs, x => x.Id == "ibm.720");
    }

    [Fact]
    public void DeviceInfoSurvivesAnUnrelatedNetworkFailure()
    {
        const string output = "Host Tools: 1.23\nCOM3\nModel: Greaseweazle V4.1\nMCU: AT32F403A\nFirmware: 1.6\nSerial: GW0CF19C9E7592000007E0941B\nUSB: Full Speed (12 Mbit/s)\nError contacting github";
        var info = GwInfoParser.Parse(output);
        Assert.Equal("COM3", info.Port);
        Assert.Equal("Greaseweazle V4.1", info.Model);
        Assert.Equal("GW0CF19C9E7592000007E0941B", info.SerialNumber);
        Assert.True(info.HasNetworkWarning);
    }

    [Fact]
    public void DeviceInfoReadsCurrentIndentedPortLine()
    {
        var info = GwInfoParser.Parse("Host Tools: 1.23\nDevice:\n  Port:      COM12\n  Model:     Greaseweazle V4.1\n  Serial:    GW123");
        Assert.Equal("COM12", info.Port);
        Assert.Equal("Greaseweazle V4.1", info.Model);
        Assert.Equal("GW123", info.SerialNumber);
    }

    [Fact]
    public async Task HardwareRegistryKeepsDisconnectedControllersAndMergesScannedUsbIdentity()
    {
        var output = new[]
        {
            new GwOutputLine(DateTimeOffset.UtcNow, GwOutputStream.Standard, "Model: Greaseweazle V4.1"),
            new GwOutputLine(DateTimeOffset.UtcNow, GwOutputStream.Standard, "Serial: GW-NEW-123")
        };
        var runner = new ScriptedRunner(new GwExecutionResult(0, false, TimeSpan.FromMilliseconds(10), output));
        IHardwareRegistry registry = new GreaseweazleHardwareRegistry(
            new StaticSerialDeviceDiscovery([new SerialDevice("COM9", "USB\\VID_1209&PID_4D69", "Greaseweazle serial device", 0x1209, 0x4d69)]),
            runner);
        var configured = new[]
        {
            new ControllerSettings { UsbId = "GW-OLD-001", LastPort = "COM3", Model = "Greaseweazle F7", IsAvailable = true }
        };

        var scanned = await registry.ScanAsync("gw.exe", configured);

        var disconnected = Assert.Single(scanned.ConfiguredControllers, controller => controller.UsbId == "GW-OLD-001");
        Assert.False(disconnected.IsAvailable);
        Assert.Equal("COM3", disconnected.LastPort);
        var discovered = Assert.Single(scanned.UnconfiguredControllers, controller => controller.UsbId == "GW-NEW-123");
        Assert.True(discovered.IsAvailable);
        Assert.Equal("COM9", discovered.LastPort);
        Assert.Equal("Greaseweazle V4.1", discovered.Model);
        var command = Assert.Single(runner.Commands);
        Assert.Equal("info", command.Verb);
        Assert.Equal(["--device", "COM9"], command.Arguments);
    }

    [Fact]
    public async Task HardwareRegistryTracksMultipleControllersAcrossDisconnectPortChangeAndReconnect()
    {
        var discovery = new MutableSerialDeviceDiscovery(
        [
            new("COM3", "PNP-A", "Greaseweazle A", 0x1209, 0x4d69),
            new("COM4", "PNP-B", "Greaseweazle B", 0x1209, 0x4d69)
        ]);
        var runner = new DeviceInfoRunner(new Dictionary<string, (string Serial, string Model)>
        {
            ["COM3"] = ("GW-A", "Greaseweazle V4.1"),
            ["COM4"] = ("GW-B", "Greaseweazle F7"),
            ["COM7"] = ("GW-B", "Greaseweazle F7"),
            ["COM9"] = ("GW-A", "Greaseweazle V4.1")
        });
        IHardwareRegistry registry = new GreaseweazleHardwareRegistry(discovery, runner);

        var initialScan = await registry.ScanAsync("gw.exe", []);
        var initial = initialScan.UnconfiguredControllers;
        Assert.Equal(2, initial.Count);
        Assert.All(initial, controller => Assert.True(controller.IsAvailable));

        discovery.Devices = [new("COM7", "PNP-B", "Greaseweazle B", 0x1209, 0x4d69)];
        var disconnected = (await registry.ScanAsync("gw.exe", initial)).ConfiguredControllers;
        var controllerA = Assert.Single(disconnected, controller => controller.UsbId == "GW-A");
        Assert.False(controllerA.IsAvailable);
        Assert.Equal("COM3", controllerA.LastPort);
        var controllerB = Assert.Single(disconnected, controller => controller.UsbId == "GW-B");
        Assert.True(controllerB.IsAvailable);
        Assert.Equal("COM7", controllerB.LastPort);

        discovery.Devices =
        [
            new("COM9", "PNP-A", "Greaseweazle A", 0x1209, 0x4d69),
            new("COM7", "PNP-B", "Greaseweazle B", 0x1209, 0x4d69)
        ];
        var reconnected = (await registry.ScanAsync("gw.exe", disconnected)).ConfiguredControllers;
        Assert.Equal(2, reconnected.Count);
        Assert.All(reconnected, controller => Assert.True(controller.IsAvailable));
        Assert.Equal("COM9", reconnected.Single(controller => controller.UsbId == "GW-A").LastPort);
        Assert.Equal("COM7", reconnected.Single(controller => controller.UsbId == "GW-B").LastPort);
    }

    [Fact]
    public async Task HardwareRegistryDoesNotProbeUnrelatedSerialPorts()
    {
        var runner = new ScriptedRunner(new GwExecutionResult(0, false, TimeSpan.Zero, []));
        IHardwareRegistry registry = new GreaseweazleHardwareRegistry(
            new StaticSerialDeviceDiscovery([new SerialDevice("COM6", "USB\\VID_2341&PID_0043\\ARDUINO", "Arduino", 0x2341, 0x0043)]),
            runner);

        var scanned = await registry.ScanAsync("gw.exe", []);

        Assert.Empty(scanned.ConfiguredControllers);
        Assert.Empty(scanned.UnconfiguredControllers);
        Assert.Empty(runner.Commands);
    }

    [Fact]
    public async Task StartupHardwareMonitorPersistsAvailabilityWithoutChangingConfiguration()
    {
        var controller = new ControllerSettings { UsbId = "GW-ONE", LastPort = "COM3", Model = "Greaseweazle V4.1", IsAvailable = true };
        var drive = new DriveSettings { ControllerUsbId = "GW-ONE", Selection = "A", Size = "3.5", Density = "HD" };
        var settings = new AppSettings { GwExecutablePath = WindowsPowerShell, Controllers = [controller], Drives = [drive] };
        var scanned = new ControllerSettings { UsbId = "GW-ONE", LastPort = "COM3", Model = controller.Model, IsAvailable = false };
        var store = new RecordingSettingsStore();
        var monitor = new StartupHardwareMonitor(new StaticHardwareRegistry([scanned]), store);

        var result = await monitor.CheckAsync(settings);

        Assert.True(result.Performed);
        Assert.Same(scanned, Assert.Single(result.MissingControllers));
        Assert.Same(drive, Assert.Single(settings.Drives));
        Assert.Equal("GW-ONE", settings.Drives[0].ControllerUsbId);
        Assert.Equal(1, store.SaveCount);
    }

    [Fact]
    public async Task StartupHardwareMonitorUpdatesPortSilentlyWhenControllerIsFound()
    {
        var settings = new AppSettings
        {
            GwExecutablePath = WindowsPowerShell,
            Controllers = [new() { UsbId = "GW-ONE", LastPort = "COM3", IsAvailable = true }]
        };
        var found = new ControllerSettings { UsbId = "GW-ONE", LastPort = "COM5", IsAvailable = true };
        var store = new RecordingSettingsStore();
        var monitor = new StartupHardwareMonitor(new StaticHardwareRegistry([found]), store);

        var result = await monitor.CheckAsync(settings);

        Assert.True(result.Performed);
        Assert.Empty(result.MissingControllers);
        Assert.Equal("COM5", Assert.Single(settings.Controllers).LastPort);
        Assert.Equal(1, store.SaveCount);
    }

    [Fact]
    public async Task StartupHardwareMonitorMarksConfiguredControllersUnavailableWithoutHostTools()
    {
        var settings = new AppSettings
        {
            GwExecutablePath = @"Z:\missing\gw.exe",
            Controllers = [new() { UsbId = "GW-ONE", LastPort = "COM3", IsAvailable = true }]
        };
        var store = new RecordingSettingsStore();
        var monitor = new StartupHardwareMonitor(new StaticHardwareRegistry([]), store);

        var result = await monitor.CheckAsync(settings);

        Assert.True(result.Performed);
        Assert.False(Assert.Single(settings.Controllers).IsAvailable);
        Assert.Single(result.MissingControllers);
        Assert.Equal(1, store.SaveCount);
    }

    [Fact]
    public async Task StartupHardwareMonitorReportsNewControllerWithoutConfiguringIt()
    {
        var settings = new AppSettings { GwExecutablePath = WindowsPowerShell };
        var detected = new ControllerSettings { UsbId = "GW-NEW", UsbSerialNumber = "GW-NEW", LastPort = "COM7", IsAvailable = true };
        var store = new RecordingSettingsStore();
        var monitor = new StartupHardwareMonitor(new StaticHardwareRegistry([], [detected]), store);

        var result = await monitor.CheckAsync(settings);

        Assert.Same(detected, Assert.Single(result.NewControllers));
        Assert.Empty(settings.Controllers);
        Assert.Empty(settings.UnconfiguredControllers);
        Assert.Equal(1, store.SaveCount);
    }

    [Fact]
    public async Task StartupHardwareMonitorRemembersDeclinedControllerAndDoesNotAskAgain()
    {
        var remembered = new ControllerSettings { UsbId = "GW-IGNORED", UsbSerialNumber = "GW-IGNORED", LastPort = "COM4", IsAvailable = false };
        var detected = new ControllerSettings { UsbId = "GW-IGNORED", UsbSerialNumber = "GW-IGNORED", LastPort = "COM9", IsAvailable = true };
        var settings = new AppSettings { GwExecutablePath = WindowsPowerShell, UnconfiguredControllers = [remembered] };
        var monitor = new StartupHardwareMonitor(new StaticHardwareRegistry([], [detected]), new RecordingSettingsStore());

        var result = await monitor.CheckAsync(settings);

        Assert.Empty(result.NewControllers);
        var retained = Assert.Single(settings.UnconfiguredControllers);
        Assert.Equal("COM9", retained.LastPort);
        Assert.True(retained.IsAvailable);
        Assert.Empty(settings.Controllers);
    }

    [Fact]
    public void PhysicalGreaseweazleDiscoveryFindsConnectedControllerWhenEnabled()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("GWGUI_TEST_PHYSICAL_DISCOVERY"), "1", StringComparison.Ordinal))
            return;

        var devices = new WindowsSerialDeviceDiscovery().FindSerialDevices();
        var controller = Assert.Single(devices, GreaseweazleDeviceMatcher.IsCandidate);
        Assert.Matches("^COM[0-9]+$", controller.Port);
        Assert.False(string.IsNullOrWhiteSpace(controller.StableId));
        Assert.True(controller.VendorId == 0x1209 || controller.UsbSerialNumber?.StartsWith("GW", StringComparison.OrdinalIgnoreCase) == true);
    }

    [Fact]
    public void WindowPlacementRejectsAWindowOutsideAllScreens()
    {
        var settings = new GWGUI.Domain.Settings.WindowPlacementSettings { Width = 1400, Height = 800, Left = 9000, Top = 9000 };
        var result = GWGUI.Domain.Settings.WindowPlacementPolicy.Normalize(settings, 1280, 720, 0, 0, 3840, 2160);
        Assert.Null(result.Left);
        Assert.Null(result.Top);
    }

    [Fact]
    public void WindowPlacementKeepsAVisibleSecondaryScreenPosition()
    {
        var settings = new GWGUI.Domain.Settings.WindowPlacementSettings { Width = 1400, Height = 800, Left = -1500, Top = 120 };
        var result = GWGUI.Domain.Settings.WindowPlacementPolicy.Normalize(settings, 1280, 720, -1920, 0, 5760, 2160);
        Assert.Equal(-1500, result.Left);
        Assert.Equal(120, result.Top);
    }

    [Fact]
    public void WindowPlacementClampsTheWholeWindowInsideTheVirtualDesktop()
    {
        var settings = new GWGUI.Domain.Settings.WindowPlacementSettings { Width = 1360, Height = 820, Left = 1200, Top = 700 };
        var result = GWGUI.Domain.Settings.WindowPlacementPolicy.Normalize(settings, 1280, 720, 0, 0, 2048, 1152);
        Assert.Equal(688, result.Left);
        Assert.Equal(332, result.Top);
    }

    [Theory]
    [InlineData(-2560, 0, 2560, 1440, 1.25, -1900, 100, -1900, 100, 1360, 820)]
    [InlineData(1920, 0, 2560, 1440, 1.25, 1600, 80, 1600, 80, 1360, 820)]
    [InlineData(0, -2160, 3840, 2160, 1.5, 100, -1300, 100, -1300, 1360, 820)]
    [InlineData(1920, 0, 1920, 1080, 1.0, 3400, 700, 2480, 260, 1360, 820)]
    [InlineData(0, 0, 1920, 1080, 1.5, 300, 200, 0, 0, 1280, 720)]
    public void WindowPlacementUsesTheActualMonitorWorkAreaAtDifferentDpi(
        double leftPixels, double topPixels, double widthPixels, double heightPixels, double scale,
        double savedLeft, double savedTop, double expectedLeft, double expectedTop, double expectedWidth, double expectedHeight)
    {
        var workLeft = leftPixels / scale;
        var workTop = topPixels / scale;
        var workWidth = widthPixels / scale;
        var workHeight = heightPixels / scale;
        var result = WindowPlacementPolicy.ConstrainToWorkArea(new(1360, 820, savedLeft, savedTop), workLeft, workTop, workWidth, workHeight);

        Assert.Equal(expectedLeft, result.Left);
        Assert.Equal(expectedTop, result.Top);
        Assert.Equal(expectedWidth, result.Width);
        Assert.Equal(expectedHeight, result.Height);
    }

    [Fact]
    public void GwProgressCountsUniqueTracksAndIgnoresRetries()
    {
        var tracker = new GwProgressTracker();
        Assert.Null(tracker.Accept("Reading c=0-79:h=0-1 revs=3"));
        var first = tracker.Accept("T0.0: Raw Flux");
        var retry = tracker.Accept("T0.0: Retry #1.1");
        var second = tracker.Accept("T0.1: Raw Flux");
        Assert.Equal(160, first!.TotalTracks);
        Assert.Equal(GwTrackState.Success, first.State);
        Assert.Equal(1, retry!.CompletedTracks);
        Assert.Equal(GwTrackState.Retry, retry.State);
        Assert.Equal(2, second!.CompletedTracks);
        Assert.Equal(80, second.TotalOnHead);
        Assert.Equal(1, second.CompletedOnHead);
        Assert.True(second.Head0Expected);
        Assert.True(second.Head1Expected);
        Assert.Equal(Enumerable.Range(0, 80), second.Cylinders);
        Assert.Equal(1, second.NextCylinder);
        Assert.Equal(0, second.NextHead);
    }

    [Fact]
    public void GwProgressUnderstandsSteppedAndCommaSeparatedTrackSets()
    {
        var tracker = new GwProgressTracker();
        tracker.Accept("Writing c=0-39/2,41:h=0");
        var progress = tracker.Accept("T0.0: Writing Track");
        Assert.Equal(21, progress!.TotalTracks);
    }

    [Fact]
    public void GwProgressUsesConvertGeometryForPerSideSegments()
    {
        var tracker = new GwProgressTracker();
        Assert.Null(tracker.Accept("Converting c=0-79:h=0-1 -> c=0-79:h=0-1"));

        var progress = tracker.Accept("T24.0: Raw Flux (120817 flux in 599.49ms)");

        Assert.NotNull(progress);
        Assert.Equal(80, progress.TotalOnHead);
        Assert.Equal(160, progress.TotalTracks);
        Assert.True(progress.Head0Expected);
        Assert.True(progress.Head1Expected);
    }

    [Fact]
    public void GwProgressUsesEraseGeometryForPerSideSegments()
    {
        var tracker = new GwProgressTracker();
        Assert.Null(tracker.Accept("Erasing c=0-79:h=0-1, revs=3"));

        var progress = tracker.Accept("T0.1: Erasing Track");

        Assert.NotNull(progress);
        Assert.Equal(80, progress.TotalOnHead);
        Assert.Equal(160, progress.TotalTracks);
        Assert.True(progress.Head0Expected);
        Assert.True(progress.Head1Expected);
    }

    [Fact]
    public async Task ScpCaptureInfoReadsFinalMetadataWithoutDecodingFlux()
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-scp-summary-{Guid.NewGuid():N}.scp");
        try
        {
            var data = BuildSingleTrackScp([100, 120, 140]);
            await File.WriteAllBytesAsync(path, data);
            var info = await ScpCaptureInfoReader.ReadAsync(path);

            Assert.Equal(1, info.CapturedTracks);
            Assert.Equal(0, info.MissingTracks);
            Assert.Equal(1, info.Cylinders);
            Assert.Equal(1, info.Sides);
            Assert.Equal(1, info.Header.Revolutions);
            Assert.True(info.ChecksumValid);
            Assert.Equal(data.Length, info.FileSize);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void DefaultProfileCannotBeRenamedOrDeleted()
    {
        IProfileStore<OperationProfile> store = new InMemoryProfileStore(OperationKind.Read);
        var profile = store.GetAll().Single();
        Assert.Throws<InvalidOperationException>(() => store.Rename(profile.Id, "Autre"));
        Assert.Throws<InvalidOperationException>(() => store.Delete(profile.Id));
    }

    [Fact]
    public void SavingUnderAnotherNameCreatesTheExpectedCopy()
    {
        IProfileStore<OperationProfile> store = new InMemoryProfileStore(OperationKind.Read);
        store.Save(new OperationProfile("p1", OperationKind.Read, "Disquettes récalcitrantes", new Dictionary<string, string>(), new HashSet<string> { "retries" }));
        store.Save(new OperationProfile("p2", OperationKind.Read, "Disquettes Acorn", new Dictionary<string, string>(), new HashSet<string> { "retries" }));
        Assert.Equal(3, store.GetAll().Count);
    }

    [Fact]
    public void ProfileStoreRejectsProfilesFromAnotherTab()
    {
        IProfileStore<OperationProfile> readProfiles = new InMemoryProfileStore(OperationKind.Read);
        var writeProfile = new OperationProfile("write-1", OperationKind.Write, "Même nom autorisé ailleurs", new Dictionary<string, string>(), new HashSet<string>());

        Assert.Throws<ArgumentException>(() => readProfiles.Save(writeProfile));
        Assert.Throws<ArgumentException>(() => new InMemoryProfileStore(OperationKind.Read, [writeProfile]));
        Assert.Single(readProfiles.GetAll());
    }

    [Fact]
    public void NoExplicitExtensionUsesImaWithoutCheckingIt()
    {
        var planner = new ConversionPlanner(new BuiltInImageFormatCatalog());
        var outputs = planner.Plan("disk.scp", "out", "disk", [new ConversionSelection("ibm.720", new HashSet<string>())], false);
        Assert.Single(outputs);
        Assert.Equal(".ima", outputs[0].Extension);
        Assert.True(outputs[0].UsesImplicitExtension);
    }

    [Fact]
    public void ExplicitImgReplacesImplicitImaAndBothCanBeRequested()
    {
        var planner = new ConversionPlanner(new BuiltInImageFormatCatalog());
        var imgOnly = planner.Plan("disk.scp", "out", "disk", [new ConversionSelection("ibm.720", new HashSet<string> { ".img" })], false);
        Assert.Equal([".img"], imgOnly.Select(x => x.Extension));
        var both = planner.Plan("disk.scp", "out", "disk", [new ConversionSelection("ibm.720", new HashSet<string> { ".ima", ".img" })], false);
        Assert.Equal(2, both.Count);
    }

    [Fact]
    public void DefaultReadAddsNoOptionalGwArguments()
    {
        var command = ReadCommandBuilder.Build(new ReadRequest("gw.exe", "disk.scp", ReadResultKind.RawScp, null, []));
        Assert.Equal(["disk.scp"], command.Arguments);
    }

    [Fact]
    public void OnlyEnabledReadOptionsAreEmitted()
    {
        var command = ReadCommandBuilder.Build(new ReadRequest("gw.exe", "disk.scp", ReadResultKind.RawScp, null,
            [new EnabledOption("--revs", "5"), new EnabledOption("--tracks", "c=0-79:h=0-1")], "COM3", null));
        Assert.Equal(["--device", "COM3", "--revs", "5", "--tracks", "c=0-79:h=0-1", "disk.scp"], command.Arguments);
    }

    [Fact]
    public void ExpertArgumentsPreserveQuotedValues()
    {
        var command = ReadCommandBuilder.Build(new ReadRequest("gw.exe", "disk.scp", ReadResultKind.RawScp, null, [], ExpertArguments: "--fake-index 300 --tracks \"c=0-79:h=0-1\""));
        Assert.Equal(["--fake-index", "300", "--tracks", "c=0-79:h=0-1", "disk.scp"], command.Arguments);
    }

    [Fact]
    public void NextNameSkipsExistingSequences()
    {
        var occupied = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { Path.Combine("out", "Disk 01.scp"), Path.Combine("out", "Disk 02.scp") };
        var result = OutputConflictResolver.FindNextAvailable("out", "Disk", ".scp", SequenceKind.Numeric, 2, 1, occupied.Contains);
        Assert.Equal(Path.Combine("out", "Disk 03.scp"), result);
    }

    [Theory]
    [InlineData("disk.adf", 901120, "amiga.amigados")]
    [InlineData("disk.adf", 819200, "acorn.adfs.800")]
    [InlineData("disk.adf", 820224, "acorn.adfs.800")]
    [InlineData("disk.adf", 1802240, "amiga.amigados_hd")]
    [InlineData("disk.st", 368640, "atarist.360")]
    [InlineData("disk.st", 901120, "atarist.880")]
    [InlineData("disk.ima", 163840, "ibm.160")]
    [InlineData("disk.ima", 1228800, "ibm.1200")]
    [InlineData("disk.ima", 1474560, "ibm.1440")]
    [InlineData("disk.img", 1720320, "ibm.1680")]
    [InlineData("disk.img", 2949120, "ibm.2880")]
    public void WriteDetectorUsesContainerSizeToResolveAmbiguity(string name, long length, string formatId)
    {
        var result = new ImageFormatDetector(new BuiltInImageFormatCatalog()).Detect(name, length);
        Assert.Equal(formatId, result.Format?.Id);
        Assert.False(result.RequiresUserChoice);
    }

    [Fact]
    public void UnknownImgGeometryRequiresExplicitChoice()
    {
        var result = new ImageFormatDetector(new BuiltInImageFormatCatalog()).Detect("disk.img", 12345);
        Assert.True(result.RequiresUserChoice);
    }

    [Fact]
    public void WriteVerificationIsEnabledUnlessNoVerifyWasExplicitlySelected()
    {
        var normal = WriteCommandBuilder.Build(new WriteRequest("gw.exe", "disk.adf", "amiga.amigados", []));
        Assert.DoesNotContain("--no-verify", normal.Arguments);
        var unsafeCommand = WriteCommandBuilder.Build(new WriteRequest("gw.exe", "disk.adf", "amiga.amigados", [], DisableVerify: true));
        Assert.Contains("--no-verify", unsafeCommand.Arguments);
    }

    [Fact]
    public void AdvancedReadOptionsRemainSeparateCommandArguments()
    {
        EnabledOption[] options = [new("--seek-retries", "2"), new("--fake-index", "300rpm"), new("--adjust-speed", "360rpm"), new("--pll", "period=5:phase=60"), new("--reverse"), new("--densel", "L")];
        var command = ReadCommandBuilder.Build(new ReadRequest("gw.exe", "disk.scp", ReadResultKind.RawScp, null, options));
        Assert.Equal(["--seek-retries", "2", "--fake-index", "300rpm", "--adjust-speed", "360rpm", "--pll", "period=5:phase=60", "--reverse", "--densel", "L", "disk.scp"], command.Arguments);
    }

    [Fact]
    public void AdvancedWriteOptionsRemainSeparateCommandArguments()
    {
        EnabledOption[] options = [new("--tracks", "c=0-79:h=0-1"), new("--pre-erase"), new("--precomp", "type=mfm:40=125"), new("--hard-sectors"), new("--gen-tg43")];
        var command = WriteCommandBuilder.Build(new WriteRequest("gw.exe", "disk.adf", "amiga.amigados", options));
        Assert.Equal(["--format", "amiga.amigados", "--tracks", "c=0-79:h=0-1", "--pre-erase", "--precomp", "type=mfm:40=125", "--hard-sectors", "--gen-tg43", "disk.adf"], command.Arguments);
    }

    [Fact]
    public void AdvancedConversionOptionsRemainSeparateCommandArguments()
    {
        var output = new ConversionOutput("ibm.720", ".ima", "out/disk.ima", true);
        EnabledOption[] options = [new("--tracks", "c=0-79:h=0-1"), new("--out-tracks", "c=0-39:h=0"), new("--adjust-speed", "300rpm"), new("--pll", "period=5:phase=60"), new("--reverse")];
        var command = ConversionCommandBuilder.Build("gw.exe", "source.scp", output, options);
        Assert.Equal(["--format", "ibm.720", "--tracks", "c=0-79:h=0-1", "--out-tracks", "c=0-39:h=0", "--adjust-speed", "300rpm", "--pll", "period=5:phase=60", "--reverse", "source.scp", "out/disk.ima"], command.Arguments);
    }

    [Theory]
    [InlineData("atarist.810", "disk.st")]
    [InlineData("amstrad.cpc", "disk.dsk")]
    [InlineData("amstrad.pcw", "disk.dsk")]
    public void CommandsUseTheBundledDiskDefinition(string formatId, string outputPath)
    {
        var read = ReadCommandBuilder.Build(new ReadRequest("gw.exe", outputPath, ReadResultKind.KnownFormat, formatId, []));
        var write = WriteCommandBuilder.Build(new WriteRequest("gw.exe", outputPath, formatId, []));
        var convert = ConversionCommandBuilder.Build("gw.exe", "source.scp", new(formatId, Path.GetExtension(outputPath), outputPath, true));

        foreach (var command in new[] { read, write, convert })
        {
            Assert.Contains("--diskdefs", command.Arguments);
            Assert.Contains(BuiltInDiskDefinitions.FilePath, command.Arguments);
            Assert.Contains(formatId, command.Arguments);
        }
    }

    [Fact]
    public void CustomDiskDefinitionOverridesTheBundledDefinition()
    {
        EnabledOption[] options = [new("--diskdefs", "custom.cfg")];
        var command = WriteCommandBuilder.Build(new WriteRequest("gw.exe", "disk.st", "atarist.810", options));

        Assert.Equal(1, command.Arguments.Count(argument => argument == "--diskdefs"));
        Assert.Contains("custom.cfg", command.Arguments);
        Assert.DoesNotContain(BuiltInDiskDefinitions.FilePath, command.Arguments);
    }

    [Theory]
    [InlineData("--revs", "0")]
    [InlineData("--retries", "-1")]
    [InlineData("--tracks", "")]
    [InlineData("--densel", "X")]
    public void InvalidStructuredOptionValuesAreRejected(string argument, string value)
    {
        Assert.Throws<ArgumentException>(() => ReadCommandBuilder.Build(new ReadRequest("gw.exe", "disk.scp", ReadResultKind.RawScp, null, [new EnabledOption(argument, value)])));
    }

    [Fact]
    public void MutuallyExclusiveStructuredOptionsAreRejected()
    {
        Assert.Throws<ArgumentException>(() => ReadCommandBuilder.Build(new ReadRequest("gw.exe", "disk.scp", ReadResultKind.RawScp, null, [new EnabledOption("--fake-index", "300rpm"), new EnabledOption("--hard-sectors")])));
        Assert.Throws<ArgumentException>(() => WriteCommandBuilder.Build(new WriteRequest("gw.exe", "disk.adf", null, [new EnabledOption("--densel", "H"), new EnabledOption("--gen-tg43")])));
    }

    [Fact]
    public void ConversionCommandUsesSelectedFormatAndSeparatePaths()
    {
        var output = new ConversionOutput("atarist.720", ".st", "out/disk.st", true);
        var command = ConversionCommandBuilder.Build("gw.exe", "source.scp", output);
        Assert.Equal(["--format", "atarist.720", "source.scp", "out/disk.st"], command.Arguments);
    }

    [Fact]
    public void ConversionTagsPreventSameExtensionOutputsFromColliding()
    {
        var planner = new ConversionPlanner(new BuiltInImageFormatCatalog());
        var outputs = planner.Plan("disk.scp", "out", "disk", [new ConversionSelection("amiga.amigados", new HashSet<string>()), new ConversionSelection("acorn.adfs.800", new HashSet<string>())], true);
        Assert.Equal(2, outputs.Select(x => x.OutputPath).Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void ConversionTagsAreStableAndIndependentFromTranslatedLabels()
    {
        var catalog = new BuiltInImageFormatCatalog(key => "translated:" + key);
        var output = Assert.Single(new ConversionPlanner(catalog).Plan("disk.scp", "out", "disk", [new ConversionSelection("ibm.720", new HashSet<string>())], true));
        Assert.Equal(Path.Combine("out", "[PC-720] disk.ima"), output.OutputPath);
    }

    [Fact]
    public void MaintenanceDefaultsDoNotEmitOptionalArguments()
    {
        Assert.Empty(MaintenanceCommandBuilder.Erase(new EraseRequest("gw.exe", [])).Arguments);
        Assert.Empty(MaintenanceCommandBuilder.Clean(new CleanRequest("gw.exe")).Arguments);
    }

    [Fact]
    public void CentralCommandBuilderCoversEveryApplicationOperation()
    {
        IGwCommandBuilder builder = new GwCommandBuilder();

        Assert.Equal("read", builder.BuildRead(new("gw.exe", "disk.scp", ReadResultKind.RawScp, null, [])).Verb);
        Assert.Equal("write", builder.BuildWrite(new("gw.exe", "disk.adf", "amiga.amigados", [])).Verb);
        Assert.Equal("convert", builder.BuildConversion("gw.exe", "disk.scp", new("ibm.720", ".ima", "disk.ima", true)).Verb);
        Assert.Equal("erase", builder.BuildErase(new("gw.exe", [])).Verb);
        Assert.Equal("clean", builder.BuildClean(new("gw.exe")).Verb);
        Assert.Equal("rpm", builder.BuildTool(new("gw.exe", "rpm", new Dictionary<string, string> { ["nr"] = "1" }, new HashSet<string>())).Verb);
        Assert.Equal(["--device", "COM9", "--bootloader"], builder.BuildInfo(new("gw.exe", "COM9", true)).Arguments);
    }

    [Fact]
    public void CleaningOptionsAreMappedExplicitly()
    {
        var command = MaintenanceCommandBuilder.Clean(new CleanRequest("gw.exe", 80, 3, 100));
        Assert.Equal(["--cylinders", "80", "--passes", "3", "--linger", "100"], command.Arguments);
    }

    [Fact]
    public void DiagnosticToolCommandsAreValidatedAndRouted()
    {
        var rpm = ToolCommandBuilder.Build(new("gw.exe", "rpm", new Dictionary<string, string> { ["nr"] = "3" }, new HashSet<string>(), "COM7", "B"));
        Assert.Equal(["--nr", "3", "--device", "COM7", "--drive", "B"], rpm.Arguments);
        var pin = ToolCommandBuilder.Build(new("gw.exe", "pin", new Dictionary<string, string> { ["pin"] = "26" }, new HashSet<string> { "set", "high" }, "COM7"));
        Assert.Equal(["set", "26", "H", "--device", "COM7"], pin.Arguments);
        Assert.Throws<ArgumentOutOfRangeException>(() => ToolCommandBuilder.Build(new("gw.exe", "pin", new Dictionary<string, string> { ["pin"] = "12" }, new HashSet<string>())));
        Assert.Throws<ArgumentOutOfRangeException>(() => ToolCommandBuilder.Build(new("gw.exe", "rpm", new Dictionary<string, string> { ["nr"] = "0" }, new HashSet<string>())));
    }

    [Fact]
    public void DelayToolCommandIncludesOnlyEnabledNonNegativeValues()
    {
        var values = new Dictionary<string, string> { ["select"] = "10", ["step"] = "3000" };
        var command = ToolCommandBuilder.Build(new("gw.exe", "delays", values, new HashSet<string> { "step" }));
        Assert.Equal(["--step", "3000"], command.Arguments);
        Assert.Throws<ArgumentOutOfRangeException>(() => ToolCommandBuilder.Build(new("gw.exe", "delays", new Dictionary<string, string> { ["step"] = "-1" }, new HashSet<string> { "step" })));
    }

    [Fact]
    public void AlignCommandCoversRequiredAndAdvancedOptions()
    {
        var values = new Dictionary<string, string>
        {
            ["tracks"] = "c=40:h=0-1", ["revs"] = "3", ["reads"] = "10",
            ["format"] = "ibm.720", ["adjust-speed"] = "300rpm", ["densel"] = "H"
        };
        var enabled = new HashSet<string> { "format", "adjust-speed", "densel", "reverse" };
        var command = ToolCommandBuilder.Build(new("gw.exe", "align", values, enabled, "COM7", "B"));

        Assert.Equal("align", command.Verb);
        Assert.Equal(["--tracks", "c=40:h=0-1", "--revs", "3", "--reads", "10", "--format", "ibm.720", "--adjust-speed", "300rpm", "--densel", "H", "--reverse", "--device", "COM7", "--drive", "B"], command.Arguments);
    }

    [Fact]
    public void AlignCommandRejectsInvalidOrExclusiveOptions()
    {
        var values = new Dictionary<string, string> { ["tracks"] = "c=40:h=0", ["revs"] = "3", ["reads"] = "10", ["fake-index"] = "300rpm" };
        Assert.Throws<ArgumentException>(() => ToolCommandBuilder.Build(new("gw.exe", "align", values, new HashSet<string> { "fake-index", "hard-sectors" })));
        Assert.Throws<ArgumentException>(() => ToolCommandBuilder.Build(new("gw.exe", "align", new Dictionary<string, string> { ["tracks"] = "", ["revs"] = "3", ["reads"] = "10" }, new HashSet<string>())));
        Assert.Throws<ArgumentOutOfRangeException>(() => ToolCommandBuilder.Build(new("gw.exe", "align", new Dictionary<string, string> { ["tracks"] = "c=40:h=0", ["revs"] = "0", ["reads"] = "10" }, new HashSet<string>())));
    }

    [Theory]
    [InlineData("c=0-79:h=0-1")]
    [InlineData("c=0-39/2,41:h=0:step=2:hswap:h0.off=+1")]
    [InlineData("c=0-79:h=0-1:step=1/2:h1.off=-2")]
    public void TrackSpecificationsFollowGreaseweazleGrammar(string value) => GwOptionValidator.ValidateTrackSpec(value);

    [Theory]
    [InlineData("c=79-0:h=0-1")]
    [InlineData("c=0-79:h=2")]
    [InlineData("c=0-79:h=0-1:step=0")]
    [InlineData("c=0-79")]
    [InlineData("c=0-79:h=0-1:unknown=1")]
    public void InvalidTrackSpecificationsAreRejected(string value) => Assert.Throws<ArgumentException>(() => GwOptionValidator.ValidateTrackSpec(value));

    [Fact]
    public void PllPrecompensationAndSpeedSpecificationsAreValidated()
    {
        GwOptionValidator.ValidatePllSpec("period=5:phase=60:lowpass=1.5");
        GwOptionValidator.ValidatePrecompSpec("type=mfm:40=125:60=150");
        foreach (var speed in new[] { "300rpm", "200ms", "40000000scp", ".5ms", "300" }) GwOptionValidator.ValidateSpeed(speed);
        Assert.Throws<ArgumentException>(() => GwOptionValidator.ValidatePllSpec("period=five:phase=60"));
        Assert.Throws<ArgumentException>(() => GwOptionValidator.ValidatePllSpec("period=5:jitter=2"));
        Assert.Throws<ArgumentException>(() => GwOptionValidator.ValidatePrecompSpec("type=wrong:40=125"));
        Assert.Throws<ArgumentException>(() => GwOptionValidator.ValidatePrecompSpec("type=mfm"));
        Assert.Throws<ArgumentException>(() => GwOptionValidator.ValidateSpeed("300xyz"));
    }

    [Fact]
    public void AmigaDecoderFindsTheDouble4489SyncWord()
    {
        var bits = Convert.ToString(0x4489, 2).PadLeft(16, '0') + Convert.ToString(0x4489, 2).PadLeft(16, '0');
        var intervals = new List<uint>(); var sinceTransition = 0;
        foreach (var bit in bits) { sinceTransition++; if (bit == '1') { intervals.Add((uint)(sinceTransition * 40)); sinceTransition = 0; } }
        var result = new AmigaMfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));
        Assert.Contains(result.Structures, x => x.Kind == FluxStructureKind.AmigaSync);
        Assert.True(result.Confidence > 0);
    }

    [Fact]
    public void IsoMfmDecoderExtractsSectorIdentityAndDataCrc()
    {
        byte[] header = [0xa1, 0xa1, 0xa1, 0xfe, 0, 1, 2, 2]; var crc = TestCrc16(header);
        var data = Enumerable.Range(0, 512).Select(index => (byte)(index * 13)).ToArray(); var dataCrc = TestCrc16(new byte[] { 0xa1,0xa1,0xa1,0xfb }.Concat(data));
        var raw = Convert.ToString(0x4489, 2).PadLeft(16, '0') + Convert.ToString(0x4489, 2).PadLeft(16, '0') + Convert.ToString(0x4489, 2).PadLeft(16, '0') +
                  EncodeMfmBytes(0xfe, 0, 1, 2, 2, (byte)(crc >> 8), (byte)crc) + string.Concat(Enumerable.Repeat("10", 20)) +
                  Convert.ToString(0x44894489, 2).PadLeft(32, '0') + Convert.ToString(0x4489, 2).PadLeft(16, '0') + EncodeMfmBytes(new byte[] { 0xfb }.Concat(data).Concat([(byte)(dataCrc >> 8), (byte)dataCrc]).ToArray()) + "001";
        var intervals = BitsToIntervals(raw, 40);
        var result = new IsoMfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));
        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(2, sector.Number); Assert.Equal(512, sector.SizeBytes); Assert.True(sector.IntegrityValid);
    }

    [Fact]
    public void IsoFmDecoderExtractsSingleDensitySectorData()
    {
        byte[] header = [0xfe, 3, 0, 7, 1]; var crc = TestCrc16(header);
        var data = Enumerable.Range(0, 256).Select(index => (byte)(index * 17)).ToArray(); var dataCrc = TestCrc16(new byte[] { 0xfb }.Concat(data));
        var raw = Convert.ToString(0xf57e, 2).PadLeft(16, '0') + EncodeFmBytes(3, 0, 7, 1, (byte)(crc >> 8), (byte)crc) + string.Concat(Enumerable.Repeat("10", 20)) + Convert.ToString(0xf56f, 2).PadLeft(16, '0') + EncodeFmBytes(data.Concat([(byte)(dataCrc >> 8), (byte)dataCrc]).ToArray()) + "001";
        var intervals = BitsToIntervals(raw, 40); var result = new IsoFmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));
        var sector = Assert.Single(result.Sectors!); Assert.Equal(7, sector.Number); Assert.Equal(256, sector.SizeBytes); Assert.True(sector.IntegrityValid);
    }

    [Theory]
    [InlineData((byte)0xfb, false)]
    [InlineData((byte)0xf8, true)]
    public void IsoMfmDecoderRecognizesDeletedDataAndCorruptedCrc(byte mark, bool corrupt)
    {
        byte[] header = [0xa1,0xa1,0xa1,0xfe,4,1,9,0]; var headerCrc = TestCrc16(header); var data = Enumerable.Range(0, 128).Select(index => (byte)(index * 19 + 1)).ToArray(); var dataCrc = TestCrc16(new byte[] { 0xa1,0xa1,0xa1,mark }.Concat(data)); if (corrupt) dataCrc++;
        var sync = string.Concat(Enumerable.Repeat(Convert.ToString(0x4489, 2).PadLeft(16, '0'), 3)); var raw = sync + EncodeMfmBytes(0xfe,4,1,9,0,(byte)(headerCrc >> 8),(byte)headerCrc) + string.Concat(Enumerable.Repeat("10", 20)) + sync + EncodeMfmBytes(new[] { mark }.Concat(data).Concat([(byte)(dataCrc >> 8),(byte)dataCrc]).ToArray()) + "001";
        var intervals = BitsToIntervals(raw, 40); var result = new IsoMfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));
        Assert.Equal(!corrupt, Assert.Single(result.Sectors!).IntegrityValid); Assert.Contains(result.Structures, structure => structure.Kind == (mark == 0xf8 ? FluxStructureKind.DeletedDataAddressMark : FluxStructureKind.DataAddressMark));
    }

    [Theory]
    [InlineData((byte)0xfb, false)]
    [InlineData((byte)0xf8, true)]
    public void IsoFmDecoderRecognizesDeletedDataAndCorruptedCrc(byte mark, bool corrupt)
    {
        byte[] header = [0xfe,2,0,5,0]; var headerCrc = TestCrc16(header); var data = Enumerable.Range(0, 128).Select(index => (byte)(index * 23 + 2)).ToArray(); var dataCrc = TestCrc16(new[] { mark }.Concat(data)); if (corrupt) dataCrc++;
        var rawMark = mark == 0xfb ? 0xf56f : 0xf56a; var raw = Convert.ToString(0xf57e, 2).PadLeft(16, '0') + EncodeFmBytes(2,0,5,0,(byte)(headerCrc >> 8),(byte)headerCrc) + string.Concat(Enumerable.Repeat("10", 20)) + Convert.ToString(rawMark, 2).PadLeft(16, '0') + EncodeFmBytes(data.Concat([(byte)(dataCrc >> 8),(byte)dataCrc]).ToArray()) + "001";
        var intervals = BitsToIntervals(raw, 40); var result = new IsoFmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));
        Assert.Equal(!corrupt, Assert.Single(result.Sectors!).IntegrityValid); Assert.Contains(result.Structures, structure => structure.Kind == (mark == 0xf8 ? FluxStructureKind.DeletedDataAddressMark : FluxStructureKind.DataAddressMark));
    }

    [Fact]
    public void IsoDecodersReportUnavailableIntegrityWithoutDataField()
    {
        byte[] mfmHeader = [0xa1,0xa1,0xa1,0xfe,0,0,1,0]; var mfmCrc = TestCrc16(mfmHeader); var mfmRaw = string.Concat(Enumerable.Repeat(Convert.ToString(0x4489, 2).PadLeft(16, '0'), 3)) + EncodeMfmBytes(0xfe,0,0,1,0,(byte)(mfmCrc >> 8),(byte)mfmCrc) + "001";
        byte[] fmHeader = [0xfe,0,0,1,0]; var fmCrc = TestCrc16(fmHeader); var fmRaw = Convert.ToString(0xf57e, 2).PadLeft(16, '0') + EncodeFmBytes(0,0,1,0,(byte)(fmCrc >> 8),(byte)fmCrc) + "001";
        var mfmIntervals = BitsToIntervals(mfmRaw, 40); var fmIntervals = BitsToIntervals(fmRaw, 40);
        Assert.Null(Assert.Single(new IsoMfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)mfmIntervals.Count, mfmIntervals)).Sectors!).IntegrityValid);
        Assert.Null(Assert.Single(new IsoFmDecoder().Decode(new ScpRevolution(8_000_000, (uint)fmIntervals.Count, fmIntervals)).Sectors!).IntegrityValid);
    }

    [Fact]
    public void AppleGcrDecoderFindsAddressAndDataProloguesDespiteShortNoise()
    {
        var bits = Convert.ToString(0xD5AA96, 2).PadLeft(24, '0') + "0001000" + Convert.ToString(0xD5AAAD, 2).PadLeft(24, '0') + "1";
        var intervals = BitsToIntervals(bits, 40); intervals.Insert(0, 2);
        var result = new AppleGcrDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.AppleAddress);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.AppleData);
        Assert.Equal(40, result.EstimatedBitCellTicks);
    }

    [Fact]
    public void AdaptiveFluxClockFollowsGradualSpeedDrift()
    {
        var prologue = Convert.ToString(0xD5AA96, 2).PadLeft(24, '0');
        var bits = string.Concat(Enumerable.Repeat(prologue + "000", 10)) + "1";
        var intervals = new List<uint>(); var cells = 0; var transition = 0;
        foreach (var bit in bits)
        {
            cells++;
            if (bit != '1') continue;
            var cellTicks = 36d + Math.Min(8, transition * .25);
            intervals.Add((uint)Math.Round(cells * cellTicks)); cells = 0; transition++;
        }
        var result = new AppleGcrDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));
        Assert.True(result.Structures.Count(structure => structure.Kind == FluxStructureKind.AppleAddress) >= 8);
        Assert.InRange(result.EstimatedBitCellTicks, 36, 44);
    }

    [Fact]
    public void RawFluxDecoderReportsShortNoiseAndLongDropout()
    {
        var intervals = Enumerable.Repeat(80u, 30).ToList(); intervals[8] = 5; intervals[20] = 900;
        var result = new RawFluxDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));
        Assert.Equal(2, result.Structures.Count(structure => structure.Kind == FluxStructureKind.TimingAnomaly));
    }

    [Fact]
    public void CommodoreGcrDecoderFindsSyncAndHeaderBlock()
    {
        const string headerByte08 = "01010" + "01001";
        var intervals = BitsToIntervals("111111111111" + headerByte08 + "1", 40);
        var result = new CommodoreGcrDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.CommodoreSync);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.CommodoreHeader);
        Assert.Contains((byte)0x08, result.DecodedBytes);
    }

    [Fact]
    public void DecoderRegistryExposesGcrFamilies()
    {
        var ids = new FluxDecoderRegistry().Decoders.Select(decoder => decoder.Id).ToHashSet();
        Assert.Contains("apple2.gcr", ids); Assert.Contains("commodore.gcr", ids); Assert.Contains("northstar.mfm", ids); Assert.Contains("heathkit.fm", ids);
    }

    [Fact]
    public void NorthstarDecoderRecognizesHardSectorBlockMark()
    {
        var raw = string.Concat(Enumerable.Repeat("10", 60)) + EncodeMfmBytesFromZero(0, 0, 0, 0, 0, 0, 0, 0xfb) + "001";
        var intervals = BitsToIntervals(raw, 40);
        var result = new NorthstarMfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatHeader);
    }

    [Fact]
    public void NorthstarDecoderExtractsSectorIdentityAndRotatingChecksum()
    {
        var data = Enumerable.Range(0, 512).Select(index => (byte)(index * 17)).ToArray();
        byte checksum = 0;
        foreach (var value in data) { checksum ^= value; checksum = (byte)((checksum >> 7) | (checksum << 1)); }
        var block = Enumerable.Repeat((byte)0, 7).Concat([(byte)0xfb, (byte)0x37]).Concat(data).Append(checksum).ToArray();
        var raw = string.Concat(Enumerable.Repeat("10", 60)) + EncodeMfmBytesFromZero(block) + "001";
        var intervals = BitsToIntervals(raw, 40);

        var result = new NorthstarMfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(3, sector.Cylinder);
        Assert.Equal(7, sector.Number);
        Assert.Equal(512, sector.SizeBytes);
        Assert.True(sector.IntegrityValid);
        Assert.Equal(SectorIntegrityKind.Checksum, sector.IntegrityKind);
        Assert.Equal(data, result.DecodedBytes.TakeLast(512));
    }

    [Fact]
    public void NorthstarDecoderReportsUnavailableIntegrityForTruncatedBlock()
    {
        var partialData = Enumerable.Range(0, 32).Select(index => (byte)index).ToArray();
        var block = Enumerable.Repeat((byte)0, 7).Concat([(byte)0xfb, (byte)0x37]).Concat(partialData).ToArray();
        var raw = string.Concat(Enumerable.Repeat("10", 60)) + EncodeMfmBytesFromZero(block) + "001"; var intervals = BitsToIntervals(raw, 40);

        var result = new NorthstarMfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var sector = Assert.Single(result.Sectors!); Assert.Equal(3, sector.Cylinder); Assert.Equal(7, sector.Number); Assert.Null(sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Description.Contains("unavailable"));
    }

    [Fact]
    public void HeathkitDecoderRecognizesBitReversedFdHeaderMark()
    {
        var raw = EncodeFmBytes(0, 0, 0, 0xbf) + "001"; var intervals = BitsToIntervals(raw, 40);
        var result = new HeathkitFmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatHeader);
    }

    [Fact]
    public void HeathkitDecoderExtractsBitReversedHeaderAndChecksum()
    {
        const byte volume = 2, cylinder = 12, sectorNumber = 5;
        byte checksum = 0;
        foreach (var value in new[] { volume, cylinder, sectorNumber }) { checksum ^= value; checksum = (byte)((checksum >> 7) | (checksum << 1)); }
        static byte Reverse(byte value) { byte result = 0; for (var bit = 0; bit < 8; bit++) result = (byte)((result << 1) | ((value >> bit) & 1)); return result; }
        var data = Enumerable.Range(0, 256).Select(index => (byte)(index * 9)).ToArray(); byte dataChecksum = 0;
        foreach (var value in data) { dataChecksum ^= value; dataChecksum = (byte)((dataChecksum >> 7) | (dataChecksum << 1)); }
        var raw = EncodeFmBytes(0, 0, 0, 0xbf, Reverse(volume), Reverse(cylinder), Reverse(sectorNumber), Reverse(checksum)) + string.Concat(Enumerable.Repeat("10", 20)) +
                  EncodeFmBytes(new byte[] { 0, 0, 0, 0xbf }.Concat(data.Select(Reverse)).Append(Reverse(dataChecksum)).ToArray()) + "001";
        var intervals = BitsToIntervals(raw, 40);

        var result = new HeathkitFmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(cylinder, sector.Cylinder);
        Assert.Equal(sectorNumber, sector.Number);
        Assert.Equal(256, sector.SizeBytes);
        Assert.True(sector.IntegrityValid);
        Assert.Equal(SectorIntegrityKind.Checksum, sector.IntegrityKind);
        Assert.Equal(data, result.DecodedBytes.TakeLast(256));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void HeathkitDecoderValidatesDataChecksum(bool corruptData)
    {
        const byte volume = 2, cylinder = 12, sectorNumber = 5;
        static byte Reverse(byte value) { byte result = 0; for (var bit = 0; bit < 8; bit++) result = (byte)((result << 1) | ((value >> bit) & 1)); return result; }
        byte headerChecksum = 0; foreach (var value in new[] { volume, cylinder, sectorNumber }) { headerChecksum ^= value; headerChecksum = (byte)((headerChecksum >> 7) | (headerChecksum << 1)); }
        var data = Enumerable.Range(0, 256).Select(index => (byte)(index * 7)).ToArray(); byte dataChecksum = 0; foreach (var value in data) { dataChecksum ^= value; dataChecksum = (byte)((dataChecksum >> 7) | (dataChecksum << 1)); } if (corruptData) dataChecksum++;
        var raw = EncodeFmBytes(0, 0, 0, 0xbf, Reverse(volume), Reverse(cylinder), Reverse(sectorNumber), Reverse(headerChecksum)) + string.Concat(Enumerable.Repeat("10", 20)) +
                  EncodeFmBytes(new byte[] { 0, 0, 0, 0xbf }.Concat(data.Select(Reverse)).Append(Reverse(dataChecksum)).ToArray()) + "001"; var intervals = BitsToIntervals(raw, 40);

        var result = new HeathkitFmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        Assert.Equal(!corruptData, Assert.Single(result.Sectors!).IntegrityValid);
    }

    [Fact]
    public void HeathkitDecoderReportsUnavailableIntegrityWithoutDataBlock()
    {
        const byte volume = 2, cylinder = 12, sectorNumber = 5;
        static byte Reverse(byte value) { byte result = 0; for (var bit = 0; bit < 8; bit++) result = (byte)((result << 1) | ((value >> bit) & 1)); return result; }
        byte checksum = 0; foreach (var value in new[] { volume, cylinder, sectorNumber }) { checksum ^= value; checksum = (byte)((checksum >> 7) | (checksum << 1)); }
        var raw = EncodeFmBytes(0, 0, 0, 0xbf, Reverse(volume), Reverse(cylinder), Reverse(sectorNumber), Reverse(checksum)) + "001"; var intervals = BitsToIntervals(raw, 40);

        var result = new HeathkitFmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        Assert.Null(Assert.Single(result.Sectors!).IntegrityValid);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MicralNDecoderExtractsIdentityDataAndCarryChecksum(bool corruptChecksum)
    {
        const byte cylinder = 17, sectorNumber = 29;
        var data = Enumerable.Range(0, 128).Select(index => (byte)(index * 11 + 7)).ToArray();
        static byte Update(byte checksum, byte value)
        {
            var carrySource = ((value ^ checksum) ^ 0xff) & ((value + checksum) ^ value);
            return (byte)(checksum + value + ((carrySource & 0x80) != 0 ? 1 : 0));
        }
        byte checksum = 0; foreach (var value in data) checksum = Update(checksum, value);
        if (corruptChecksum) checksum++;
        var raw = EncodeFmBytes(new byte[] { 0, 0, 0, 0xff, sectorNumber, cylinder }.Concat(data).Append(checksum).ToArray()) + "001";
        var intervals = BitsToIntervals(raw, 40);

        var result = new MicralNFmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(cylinder, sector.Cylinder);
        Assert.Equal(0, sector.Head);
        Assert.Equal(sectorNumber, sector.Number);
        Assert.Equal(128, sector.SizeBytes);
        Assert.Equal(!corruptChecksum, sector.IntegrityValid);
        Assert.Equal(SectorIntegrityKind.Checksum, sector.IntegrityKind);
        Assert.Equal(data, result.DecodedBytes);
        Assert.Contains(result.Structures, structure => structure.Description.Contains(corruptChecksum ? "invalid" : "valid", StringComparison.Ordinal));
    }

    [Fact]
    public void MicralNDecoderReportsUnavailableIntegrityForTruncatedBlock()
    {
        var raw = EncodeFmBytes(0, 0, 0, 0xff, 4, 2, 1, 2, 3) + "001";
        var intervals = BitsToIntervals(raw, 40);

        var result = new MicralNFmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        Assert.Empty(result.Sectors!);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatHeader && structure.Description.Contains("unavailable", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MembrainDecoderExtractsPackedIdentityAndNativeCrc(bool corruptCrc)
    {
        byte[] prefix = [0xa1, 0xfe, 0x04, 0xb9];
        var crc = TestCrc16(prefix, 0x8005, 0x0000);
        if (corruptCrc) crc ^= 1;
        var data = Enumerable.Range(0, 512).Select(index => (byte)(index * 7)).ToArray();
        var dataCrc = TestCrc16(new byte[] { 0xa1, 0xf8 }.Concat(data), 0x8005, 0x0000);
        var raw = Convert.ToString(0x44895554, 2).PadLeft(32, '0') + EncodeMfmBytesFromZero(0x04, 0xb9, (byte)(crc >> 8), (byte)crc) + "00000000" +
                  Convert.ToString(0x4489554a, 2).PadLeft(32, '0') + EncodeMfmBytesFromZero(data.Concat([(byte)(dataCrc >> 8), (byte)dataCrc]).ToArray()) + "001";
        var intervals = BitsToIntervals(raw, 40);

        var result = new MembrainMfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(37, sector.Cylinder);
        Assert.Equal(1, sector.Head);
        Assert.Equal(9, sector.Number);
        Assert.Equal(512, sector.SizeBytes);
        Assert.Equal(!corruptCrc, sector.IntegrityValid);
        Assert.Equal(SectorIntegrityKind.Crc, sector.IntegrityKind);
        Assert.Equal(data, result.DecodedBytes.TakeLast(512));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MembrainDecoderValidatesDataCrc(bool corruptData)
    {
        byte[] header = [0xa1, 0xfe, 0x04, 0xb9]; var headerCrc = TestCrc16(header, 0x8005, 0x0000);
        var data = Enumerable.Range(0, 512).Select(index => (byte)(255 - index)).ToArray(); var dataCrc = TestCrc16(new byte[] { 0xa1, 0xf8 }.Concat(data), 0x8005, 0x0000);
        if (corruptData) dataCrc ^= 1;
        var raw = Convert.ToString(0x44895554, 2).PadLeft(32, '0') + EncodeMfmBytesFromZero(0x04, 0xb9, (byte)(headerCrc >> 8), (byte)headerCrc) + "00000000" +
                  Convert.ToString(0x4489554a, 2).PadLeft(32, '0') + EncodeMfmBytesFromZero(data.Concat([(byte)(dataCrc >> 8), (byte)dataCrc]).ToArray()) + "001";
        var intervals = BitsToIntervals(raw, 40);

        var result = new MembrainMfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        Assert.Equal(!corruptData, Assert.Single(result.Sectors!).IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData && structure.Description.Contains(corruptData ? "invalid" : "valid"));
    }

    [Fact]
    public void MembrainDecoderReportsUnavailableIntegrityWithoutDataBlock()
    {
        byte[] header = [0xa1, 0xfe, 0x04, 0xb9]; var crc = TestCrc16(header, 0x8005, 0x0000);
        var raw = Convert.ToString(0x44895554, 2).PadLeft(32, '0') + EncodeMfmBytesFromZero(0x04, 0xb9, (byte)(crc >> 8), (byte)crc) + "001"; var intervals = BitsToIntervals(raw, 40);

        var result = new MembrainMfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        Assert.Null(Assert.Single(result.Sectors!).IntegrityValid);
    }

    [Theory]
    [InlineData(512, false)]
    [InlineData(1024, true)]
    public void Aed6200pDecoderExtractsVariableSectorSizeAndHeaderCrc(int sectorSize, bool corruptCrc)
    {
        byte[] prefix = [0xc6, 12, (byte)sectorSize, 3, (byte)(sectorSize >> 8)];
        var crc = TestCrc16(prefix);
        if (corruptCrc) crc ^= 1;
        var data = Enumerable.Range(0, sectorSize).Select(index => (byte)(index * 11)).ToArray(); var dataCrc = TestCrc16(new byte[] { 0xc0 }.Concat(data));
        var raw = Convert.ToString(0x5094, 2).PadLeft(16, '0') + EncodeMfmBytesFromZero(12, (byte)sectorSize, 3, (byte)(sectorSize >> 8), (byte)(crc >> 8), (byte)crc) + "00000000" +
                  Convert.ToString(0x508a, 2).PadLeft(16, '0') + EncodeMfmBytesFromZero(data.Concat([(byte)(dataCrc >> 8), (byte)dataCrc]).ToArray()) + "001";
        var intervals = BitsToIntervals(raw, 40);

        var result = new Aed6200pMfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(12, sector.Cylinder);
        Assert.Equal(0, sector.Head);
        Assert.Equal(3, sector.Number);
        Assert.Equal(sectorSize, sector.SizeBytes);
        Assert.Equal(!corruptCrc, sector.IntegrityValid);
        Assert.Equal(data, result.DecodedBytes.TakeLast(sectorSize));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Aed6200pDecoderValidatesVariableDataBlockCrc(bool corruptData)
    {
        const int sectorSize = 512; byte[] header = [0xc6, 12, 0, 3, 2]; var headerCrc = TestCrc16(header);
        var data = Enumerable.Range(0, sectorSize).Select(index => (byte)(index * 13)).ToArray(); var dataCrc = TestCrc16(new byte[] { 0xc3 }.Concat(data)); if (corruptData) dataCrc ^= 1;
        var raw = Convert.ToString(0x5094, 2).PadLeft(16, '0') + EncodeMfmBytesFromZero(12, 0, 3, 2, (byte)(headerCrc >> 8), (byte)headerCrc) + "00000000" +
                  Convert.ToString(0x5085, 2).PadLeft(16, '0') + EncodeMfmBytesFromZero(data.Concat([(byte)(dataCrc >> 8), (byte)dataCrc]).ToArray()) + "001"; var intervals = BitsToIntervals(raw, 40);

        var result = new Aed6200pMfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        Assert.Equal(!corruptData, Assert.Single(result.Sectors!).IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData && structure.Description.Contains("C3"));
    }

    [Fact]
    public void Aed6200pDecoderReportsUnavailableIntegrityWithoutDataBlock()
    {
        byte[] header = [0xc6, 12, 0, 3, 2]; var crc = TestCrc16(header);
        var raw = Convert.ToString(0x5094, 2).PadLeft(16, '0') + EncodeMfmBytesFromZero(12, 0, 3, 2, (byte)(crc >> 8), (byte)crc) + "001"; var intervals = BitsToIntervals(raw, 40);

        var result = new Aed6200pMfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        Assert.Null(Assert.Single(result.Sectors!).IntegrityValid);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CenturionDecoderExtractsSectorIdentityAndXmodemHeaderCrc(bool corruptCrc)
    {
        byte[] identity = [17, 6];
        var crc = TestCrc16(identity, 0x1021, 0x0000);
        if (corruptCrc) crc ^= 1;
        var data = Enumerable.Range(0, 256).Select(index => (byte)(index * 5)).ToArray(); var dataCrc = TestCrc16(new byte[] { 1, 0 }.Concat(data), 0x1021, 0x0000);
        var raw = Convert.ToString(0x91224489, 2).PadLeft(32, '0') + EncodeMfmBytesFromZero(17, 6, (byte)(crc >> 8), (byte)crc) + string.Concat(Enumerable.Repeat("10", 200)) +
                  Convert.ToString(0xaaaaaaa9, 2).PadLeft(32, '0') + EncodeMfmBytesFromZero(new byte[] { 0, 1, 0 }.Concat(data).Concat([(byte)(dataCrc >> 8), (byte)dataCrc]).ToArray()) + "001";
        var intervals = BitsToIntervals(raw, 40);

        var result = new CenturionMfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(17, sector.Cylinder);
        Assert.Equal(6, sector.Number);
        Assert.Equal(256, sector.SizeBytes);
        Assert.Equal(!corruptCrc, sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Description.Contains(corruptCrc ? "invalid" : "valid", StringComparison.Ordinal));
        Assert.Equal(data, result.DecodedBytes.TakeLast(256));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CenturionDecoderValidatesVariableDataBlockCrc(bool corruptData)
    {
        byte[] identity = [17, 6]; var headerCrc = TestCrc16(identity, 0x1021, 0x0000);
        var data = Enumerable.Range(0, 512).Select(index => (byte)(index * 3)).ToArray(); var dataCrc = TestCrc16(new byte[] { 2, 0 }.Concat(data), 0x1021, 0x0000); if (corruptData) dataCrc ^= 1;
        var raw = Convert.ToString(0x91224489, 2).PadLeft(32, '0') + EncodeMfmBytesFromZero(17, 6, (byte)(headerCrc >> 8), (byte)headerCrc) + string.Concat(Enumerable.Repeat("10", 200)) +
                  Convert.ToString(0xaaaaaaa9, 2).PadLeft(32, '0') + EncodeMfmBytesFromZero(new byte[] { 0, 2, 0 }.Concat(data).Concat([(byte)(dataCrc >> 8), (byte)dataCrc]).ToArray()) + "001"; var intervals = BitsToIntervals(raw, 40);

        var result = new CenturionMfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        Assert.Equal(!corruptData, Assert.Single(result.Sectors!).IntegrityValid);
    }

    [Fact]
    public void CenturionDecoderReportsUnavailableIntegrityForUnsupportedKey()
    {
        byte[] identity = [17, 6]; var crc = TestCrc16(identity, 0x1021, 0x0000);
        var raw = Convert.ToString(0x91224489, 2).PadLeft(32, '0') + EncodeMfmBytesFromZero(17, 6, (byte)(crc >> 8), (byte)crc) + string.Concat(Enumerable.Repeat("10", 200)) +
                  Convert.ToString(0xaaaaaaa9, 2).PadLeft(32, '0') + EncodeMfmBytesFromZero(7, 1, 0) + "001"; var intervals = BitsToIntervals(raw, 40);

        var result = new CenturionMfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        Assert.Null(Assert.Single(result.Sectors!).IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Description.Contains("unsupported key 7"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void QdMo5DecoderExtractsWideSectorNumberAndDataChecksum(bool corruptChecksum)
    {
        var data = Enumerable.Range(0, 128).Select(index => (byte)(index * 11)).ToArray();
        var checksum = (byte)(0x5a + data.Sum(value => value));
        if (corruptChecksum) checksum++;
        static string RawMark(string hexadecimal) => string.Concat(Convert.FromHexString(hexadecimal).Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        var headerMark = RawMark("A914A914A914A914A9144491");
        var dataMark = RawMark("A914A914A914A914A9149144");
        var headerTail = new byte[] { 0x12, 0x34 }.Concat(new byte[13]).ToArray();
        var raw = headerMark + EncodeMfmBytesFromZero(headerTail) + string.Concat(Enumerable.Repeat("10", 20)) + dataMark + EncodeMfmBytesFromZero(data.Append(checksum).ToArray()) + "001";
        var intervals = BitsToIntervals(raw, 40);

        var result = new QdMo5MfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(0x1234, sector.Number);
        Assert.Equal(128, sector.SizeBytes);
        Assert.Equal(!corruptChecksum, sector.IntegrityValid);
        Assert.Equal(SectorIntegrityKind.Checksum, sector.IntegrityKind);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData && structure.Description.Contains(corruptChecksum ? "invalid" : "valid", StringComparison.Ordinal));
        Assert.Equal(data, result.DecodedBytes.TakeLast(128));
    }

    [Fact]
    public void QdMo5DecoderReportsUnavailableIntegrityForTruncatedData()
    {
        static string RawMark(string hexadecimal) => string.Concat(Convert.FromHexString(hexadecimal).Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        var headerMark = RawMark("A914A914A914A914A9144491"); var dataMark = RawMark("A914A914A914A914A9149144");
        var headerTail = new byte[] { 0x12, 0x34 }.Concat(new byte[13]).ToArray();
        var raw = headerMark + EncodeMfmBytesFromZero(headerTail) + string.Concat(Enumerable.Repeat("10", 20)) + dataMark + EncodeMfmBytesFromZero(Enumerable.Range(0, 12).Select(index => (byte)index).ToArray()) + "001";
        var intervals = BitsToIntervals(raw, 40);

        var result = new QdMo5MfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        Assert.Null(Assert.Single(result.Sectors!).IntegrityValid);
    }

    [Fact]
    public void QdMo5DecoderReportsUnavailableIntegrityWhenDataBlockIsMissing()
    {
        static string RawMark(string hexadecimal) => string.Concat(Convert.FromHexString(hexadecimal).Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        var headerTail = new byte[] { 0x01, 0x02 }.Concat(new byte[13]).ToArray();
        var raw = RawMark("A914A914A914A914A9144491") + EncodeMfmBytesFromZero(headerTail) + "001";
        var intervals = BitsToIntervals(raw, 40);

        var result = new QdMo5MfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(0x0102, sector.Number);
        Assert.Null(sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Description.Contains("unavailable", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EmuFmDecoderExtractsTrackIdentityAndValidatesLargeDataCrc(bool corruptDataCrc)
    {
        static byte Reverse(byte value) { byte result = 0; for (var bit = 0; bit < 8; bit++) result = (byte)((result << 1) | ((value >> bit) & 1)); return result; }
        static string EncodeEmuFm(IEnumerable<byte> values) => string.Concat(values.SelectMany(value => Enumerable.Range(0, 8).Select(bit => "01" + ((((value >> (7 - bit)) & 1) != 0) ? "01" : "00"))));
        byte track = 25, rawTrack = Reverse(track);
        var headerCrc = TestCrc16([rawTrack], 0x8005, 0x0000);
        var data = Enumerable.Range(0, 0xe00).Select(index => (byte)(index * 13)).ToArray();
        var dataCrc = TestCrc16(data, 0x8005, 0x0000);
        if (corruptDataCrc) dataCrc ^= 1;
        var marker = EncodeEmuFm([Reverse(0xfa), Reverse(0x96)]);
        var raw = marker + EncodeEmuFm([rawTrack, (byte)(headerCrc >> 8), (byte)headerCrc]) + new string('1', 64)
            + marker + EncodeEmuFm(data.Concat([(byte)(dataCrc >> 8), (byte)dataCrc])) + "1";
        var intervals = BitsToIntervals(raw, 40);

        var result = new EmuFmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(12, sector.Cylinder);
        Assert.Equal(1, sector.Head);
        Assert.Equal(1, sector.Number);
        Assert.Equal(0xe00, sector.SizeBytes);
        Assert.Equal(!corruptDataCrc, sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData && structure.Description.Contains(corruptDataCrc ? "invalid" : "valid", StringComparison.Ordinal));
        Assert.Equal(data, result.DecodedBytes.TakeLast(0xe00));
    }

    [Fact]
    public void EmuFmDecoderReportsUnavailableDataIntegrityWhenOnlyHeaderExists()
    {
        static byte Reverse(byte value) { byte result = 0; for (var bit = 0; bit < 8; bit++) result = (byte)((result << 1) | ((value >> bit) & 1)); return result; }
        static string EncodeEmuFm(IEnumerable<byte> values) => string.Concat(values.SelectMany(value => Enumerable.Range(0, 8).Select(bit => "01" + ((((value >> (7 - bit)) & 1) != 0) ? "01" : "00"))));
        var rawTrack = Reverse(8); var headerCrc = TestCrc16([rawTrack], 0x8005, 0x0000);
        var marker = EncodeEmuFm([Reverse(0xfa), Reverse(0x96)]);
        var raw = marker + EncodeEmuFm([rawTrack, (byte)(headerCrc >> 8), (byte)headerCrc]) + "1";
        var intervals = BitsToIntervals(raw, 40);

        var result = new EmuFmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(4, sector.Cylinder);
        Assert.Equal(0, sector.Head);
        Assert.Null(sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Description.Contains("unavailable", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(0xf8, false)]
    [InlineData(0xf9, false)]
    [InlineData(0xfa, false)]
    [InlineData(0xfb, true)]
    public void TycomFmDecoderExtractsIdentityDataMarkAndCrc(byte dataMark, bool corruptDataCrc)
    {
        static string EncodeTycomFm(IEnumerable<byte> values) => string.Concat(values.SelectMany(value => Enumerable.Range(0, 8).Select(bit => "01" + ((((value >> (7 - bit)) & 1) != 0) ? "01" : "00"))));
        static string RawMark(string hexadecimal) => string.Concat(Convert.FromHexString(hexadecimal).Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        const byte cylinder = 31, sectorNumber = 7;
        var headerCrc = TestCrc16([0xfe, cylinder, sectorNumber], 0x1021, 0xffff);
        var data = Enumerable.Range(0, 128).Select(index => (byte)(index * 19)).ToArray();
        var dataCrc = TestCrc16(new byte[] { dataMark }.Concat(data), 0x1021, 0xffff);
        if (corruptDataCrc) dataCrc ^= 1;
        var dataPattern = dataMark switch { 0xf8 => "55111444", 0xf9 => "55111445", 0xfa => "55111454", _ => "55111455" };
        var raw = RawMark("55111554") + EncodeTycomFm([cylinder, sectorNumber, (byte)(headerCrc >> 8), (byte)headerCrc]) + new string('1', 64)
            + RawMark(dataPattern) + EncodeTycomFm(data.Concat([(byte)(dataCrc >> 8), (byte)dataCrc])) + "1";
        var intervals = BitsToIntervals(raw, 40);

        var result = new TycomFmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(cylinder, sector.Cylinder);
        Assert.Equal(sectorNumber, sector.Number);
        Assert.Equal(128, sector.SizeBytes);
        Assert.Equal(!corruptDataCrc, sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData && structure.Description.Contains(dataMark.ToString("X2"), StringComparison.Ordinal));
        Assert.Equal(data, result.DecodedBytes.TakeLast(128));
    }

    [Fact]
    public void TycomFmDecoderReportsUnavailableDataIntegrityWhenOnlyHeaderExists()
    {
        static string EncodeTycomFm(IEnumerable<byte> values) => string.Concat(values.SelectMany(value => Enumerable.Range(0, 8).Select(bit => "01" + ((((value >> (7 - bit)) & 1) != 0) ? "01" : "00"))));
        static string RawMark(string hexadecimal) => string.Concat(Convert.FromHexString(hexadecimal).Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        var headerCrc = TestCrc16([0xfe, 4, 2], 0x1021, 0xffff);
        var raw = RawMark("55111554") + EncodeTycomFm([4, 2, (byte)(headerCrc >> 8), (byte)headerCrc]) + "1";
        var intervals = BitsToIntervals(raw, 40);

        var result = new TycomFmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(4, sector.Cylinder);
        Assert.Equal(2, sector.Number);
        Assert.Null(sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Description.Contains("unavailable", StringComparison.Ordinal));
    }

    [Fact]
    public void TycomFmDecoderRejectsCorruptedHeaderCrc()
    {
        static string EncodeTycomFm(IEnumerable<byte> values) => string.Concat(values.SelectMany(value => Enumerable.Range(0, 8).Select(bit => "01" + ((((value >> (7 - bit)) & 1) != 0) ? "01" : "00"))));
        static string RawMark(string hexadecimal) => string.Concat(Convert.FromHexString(hexadecimal).Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        var headerCrc = (ushort)(TestCrc16([0xfe, 9, 3], 0x1021, 0xffff) ^ 1);
        var raw = RawMark("55111554") + EncodeTycomFm([9, 3, (byte)(headerCrc >> 8), (byte)headerCrc]) + "1";
        var intervals = BitsToIntervals(raw, 40);

        var result = new TycomFmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        Assert.Empty(result.Sectors!);
        Assert.Contains(result.Structures, structure => structure.Description.Contains("header CRC invalid", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(0xf8, false)]
    [InlineData(0xf9, false)]
    [InlineData(0xfa, false)]
    [InlineData(0xfb, false)]
    [InlineData(0xfc, false)]
    [InlineData(0xfd, true)]
    public void DecRx02DecoderExtractsAllDataMarksAndFmOrM2FmCrc(byte dataMark, bool corruptDataCrc)
    {
        static string EncodeRxFm(IEnumerable<byte> values) => string.Concat(values.SelectMany(value => Enumerable.Range(0, 8).Select(bit => "01" + ((((value >> (7 - bit)) & 1) != 0) ? "01" : "00"))));
        static string RawMark(string hexadecimal) => string.Concat(Convert.FromHexString(hexadecimal).Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        static string EncodeM2Fm(byte[] values)
        {
            var bits = EncodeMfmBytesFromZero(values).ToCharArray(); const string normal = "00101010100", encoded = "01000100010"; var replacements = 0;
            for (var offset = 1; offset + normal.Length <= bits.Length; offset += 2)
            {
                var matches = true; for (var index = 0; index < normal.Length; index++) if (bits[offset + index] != normal[index]) { matches = false; break; }
                if (!matches) continue; for (var index = 0; index < encoded.Length; index++) bits[offset + index] = encoded[index]; replacements++; offset += normal.Length - 3;
            }
            Assert.True(replacements > 0, "The M²FM vector must exercise the DEC 11-bit substitution rule."); return new string(bits);
        }
        const byte cylinder = 22, head = 1, sectorNumber = 9, sizeCode = 0;
        var headerCrc = TestCrc16([0xfe, cylinder, head, sectorNumber, sizeCode], 0x1021, 0xffff);
        var m2fm = dataMark is 0xf9 or 0xfd; var size = m2fm ? 256 : 128;
        var data = Enumerable.Range(0, size).Select(index => (byte)(index * 23)).ToArray();
        var dataCrc = TestCrc16(new byte[] { dataMark }.Concat(data), 0x1021, 0xffff); if (corruptDataCrc) dataCrc ^= 1;
        var markPattern = dataMark switch { 0xf8 => "55111444", 0xf9 => "55111445", 0xfa => "55111454", 0xfb => "55111455", 0xfc => "55111544", _ => "55111545" };
        var payload = data.Concat([(byte)(dataCrc >> 8), (byte)dataCrc]).ToArray();
        var encodedPayload = m2fm ? "0" + EncodeM2Fm(payload) : EncodeRxFm(payload);
        var raw = RawMark("55111554") + EncodeRxFm([cylinder, head, sectorNumber, sizeCode, (byte)(headerCrc >> 8), (byte)headerCrc]) + new string('1', 64)
            + RawMark(markPattern) + encodedPayload + "1";
        var intervals = BitsToIntervals(raw, 40);

        var result = new DecRx02Decoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(cylinder, sector.Cylinder); Assert.Equal(head, sector.Head); Assert.Equal(sectorNumber, sector.Number);
        Assert.Equal(size, sector.SizeBytes); Assert.Equal(!corruptDataCrc, sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData && structure.Description.Contains(dataMark.ToString("X2"), StringComparison.Ordinal) && structure.Description.Contains(m2fm ? "M²FM" : "FM", StringComparison.Ordinal));
        Assert.Equal(data, result.DecodedBytes.TakeLast(size));
    }

    [Fact]
    public void DecRx02DecoderReportsUnavailableDataAndRejectsBadHeaderCrc()
    {
        static string EncodeRxFm(IEnumerable<byte> values) => string.Concat(values.SelectMany(value => Enumerable.Range(0, 8).Select(bit => "01" + ((((value >> (7 - bit)) & 1) != 0) ? "01" : "00"))));
        static string RawMark(string hexadecimal) => string.Concat(Convert.FromHexString(hexadecimal).Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        var validCrc = TestCrc16([0xfe, 5, 0, 2, 0], 0x1021, 0xffff);
        var validBits = RawMark("55111554") + EncodeRxFm([5, 0, 2, 0, (byte)(validCrc >> 8), (byte)validCrc]) + "1";
        var invalidCrc = (ushort)(validCrc ^ 1);
        var invalidBits = RawMark("55111554") + EncodeRxFm([5, 0, 2, 0, (byte)(invalidCrc >> 8), (byte)invalidCrc]) + "1";

        var validIntervals = BitsToIntervals(validBits, 40); var invalidIntervals = BitsToIntervals(invalidBits, 40);
        var missing = new DecRx02Decoder().Decode(new ScpRevolution(8_000_000, (uint)validIntervals.Count, validIntervals));
        var corrupt = new DecRx02Decoder().Decode(new ScpRevolution(8_000_000, (uint)invalidIntervals.Count, invalidIntervals));

        Assert.Null(Assert.Single(missing.Sectors!).IntegrityValid);
        Assert.Contains(missing.Structures, structure => structure.Description.Contains("unavailable", StringComparison.Ordinal));
        Assert.Empty(corrupt.Sectors!);
        Assert.Contains(corrupt.Structures, structure => structure.Description.Contains("header CRC invalid", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ArburgDecoderValidatesFullFmDataTrackChecksum(bool corruptChecksum)
    {
        static byte Reverse(byte value) { byte result = 0; for (var bit = 0; bit < 8; bit++) result = (byte)((result << 1) | ((value >> bit) & 1)); return result; }
        static string EncodeArburgFm(IEnumerable<byte> values) => string.Concat(values.SelectMany(value => Enumerable.Range(0, 8).Select(bit => { var reversed = Reverse(value); return "01" + ((((reversed >> (7 - bit)) & 1) != 0) ? "01" : "00"); })));
        static string RawMark(string hexadecimal) => string.Concat(Convert.FromHexString(hexadecimal).Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        var data = Enumerable.Range(0, 0x9fe).Select(index => (byte)(index * 29)).ToArray(); ushort checksum = 0; foreach (var value in data) checksum += value;
        if (corruptChecksum) checksum++;
        var block = data.Concat([(byte)checksum, (byte)(checksum >> 8)]).ToArray();
        var raw = RawMark("4444444455555555") + EncodeArburgFm(block) + "1"; var intervals = BitsToIntervals(raw, 40);

        var result = new ArburgDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var sector = Assert.Single(result.Sectors!); Assert.Equal(0xa00, sector.SizeBytes); Assert.Equal(!corruptChecksum, sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData && structure.Description.Contains(corruptChecksum ? "invalid" : "valid", StringComparison.Ordinal));
        Assert.Equal(data, result.DecodedBytes.TakeLast(0x9fe));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ArburgDecoderValidatesFullVariableLengthSystemTrackChecksum(bool corruptChecksum)
    {
        static string EncodeSystem(IEnumerable<byte> values) => string.Concat(values.SelectMany(value => Enumerable.Range(0, 8).Select(bit => ((value >> bit) & 1) != 0 ? "001" : "01")));
        static string RawMark(string hexadecimal) => string.Concat(Convert.FromHexString(hexadecimal).Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        var data = Enumerable.Range(0, 0xefe).Select(index => (byte)(index * 31)).ToArray(); ushort checksum = 0; foreach (var value in data) checksum += value;
        if (corruptChecksum) checksum++;
        var block = data.Concat([(byte)checksum, (byte)(checksum >> 8)]).ToArray();
        var raw = RawMark("5555555555249249") + EncodeSystem(block) + "1"; var intervals = BitsToIntervals(raw, 40);

        var result = new ArburgDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var sector = Assert.Single(result.Sectors!); Assert.Equal(0xf00, sector.SizeBytes); Assert.Equal(!corruptChecksum, sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatHeader && structure.Description.Contains(corruptChecksum ? "invalid" : "valid", StringComparison.Ordinal));
        Assert.Equal(data, result.DecodedBytes.TakeLast(0xefe));
    }

    [Fact]
    public void ArburgDecoderReportsUnavailableIntegrityForTruncatedTrackBlocks()
    {
        static string RawMark(string hexadecimal) => string.Concat(Convert.FromHexString(hexadecimal).Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        static FluxDecodeResult Decode(string marker)
        {
            var intervals = BitsToIntervals(RawMark(marker) + "1", 40); return new ArburgDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));
        }
        var data = Decode("4444444455555555"); var system = Decode("5555555555249249");
        Assert.Null(Assert.Single(data.Sectors!).IntegrityValid); Assert.Null(Assert.Single(system.Sectors!).IntegrityValid);
        Assert.All(data.Structures.Concat(system.Structures), structure => Assert.Contains("unavailable", structure.Description, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Victor9kDecoderExtractsIdentityAndValidatesHeaderAndDataChecksums(bool corruptHeader, bool corruptData)
    {
        static string EncodeGcr(IEnumerable<byte> values)
        {
            int[] table = [0x0a,0x0b,0x12,0x13,0x0e,0x0f,0x16,0x17,0x09,0x19,0x1a,0x1b,0x0d,0x1d,0x1e,0x15];
            return string.Concat(values.SelectMany(value => new[] { value >> 4, value & 15 }).Select(nibble => Convert.ToString(table[nibble], 2).PadLeft(5, '0')));
        }
        static string Block(string markerHex, IReadOnlyList<byte> values)
        {
            var marker = string.Concat(Convert.FromHexString(markerHex).Select(value => Convert.ToString(value, 2).PadLeft(8, '0'))); var bits = marker.ToList(); var encoded = EncodeGcr(values);
            while (bits.Count < 49 + encoded.Length * 2) bits.Add('0');
            for (var index = 0; index < encoded.Length; index++)
            {
                var position = 49 + index * 2;
                if (position < marker.Length) Assert.Equal(marker[position], encoded[index]);
                bits[position] = encoded[index];
            }
            return new(bits.ToArray());
        }
        const byte cylinder = 17; const byte sector = 6;
        var headerChecksum = (byte)(cylinder + sector + (corruptHeader ? 1 : 0));
        byte[] header = [0x06, cylinder, sector, headerChecksum, 0xa1, 0x1a];
        var data = Enumerable.Range(0, 512).Select(index => (byte)(index * 29 + 7)).ToArray(); ushort checksum = 0; foreach (var value in data) checksum += value;
        if (corruptData) checksum++;
        var dataBlock = new byte[] { 0x00 }.Concat(data).Concat([(byte)checksum, (byte)(checksum >> 8)]).ToArray();
        var raw = Block("5555555555551111", header) + new string('0', 20) + Block("5555555555551104", dataBlock) + "1";
        var intervals = BitsToIntervals(raw, 40);

        var result = new Victor9kGcrDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var decoded = Assert.Single(result.Sectors!); Assert.Equal(cylinder, decoded.Cylinder); Assert.Equal(sector, decoded.Number); Assert.Equal(512, decoded.SizeBytes);
        Assert.Equal(!corruptHeader && !corruptData, decoded.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData && structure.Description.Contains(corruptData ? "invalid" : "valid", StringComparison.Ordinal));
        Assert.Equal(data, result.DecodedBytes.TakeLast(512));
    }

    [Fact]
    public void Victor9kDecoderReportsUnavailableIntegrityForTruncatedSector()
    {
        var marker = string.Concat(Convert.FromHexString("5555555555551111").Select(value => Convert.ToString(value, 2).PadLeft(8, '0'))); var intervals = BitsToIntervals(marker + "1", 40);
        var result = new Victor9kGcrDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));
        Assert.Null(Assert.Single(result.Sectors!).IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatHeader && structure.Description.Contains("unavailable", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void AppleGcrDecoderExtractsAddressAndDecodesSixAndTwoData(bool corruptAddress, bool corruptData)
    {
        byte[] table = [0x96,0x97,0x9a,0x9b,0x9d,0x9e,0x9f,0xa6,0xa7,0xab,0xac,0xad,0xae,0xaf,0xb2,0xb3,0xb4,0xb5,0xb6,0xb7,0xb9,0xba,0xbb,0xbc,0xbd,0xbe,0xbf,0xcb,0xcd,0xce,0xcf,0xd3,0xd6,0xd7,0xd9,0xda,0xdb,0xdc,0xdd,0xde,0xdf,0xe5,0xe6,0xe7,0xe9,0xea,0xeb,0xec,0xed,0xee,0xef,0xf2,0xf3,0xf4,0xf5,0xf6,0xf7,0xf9,0xfa,0xfb,0xfc,0xfd,0xfe,0xff];
        static IEnumerable<byte> FourAndFour(byte value) => [(byte)((value >> 1) | 0xaa), (byte)(value | 0xaa)];
        static string Bits(IEnumerable<byte> values) => string.Concat(values.Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        static byte[] EncodeData(byte[] source, IReadOnlyList<byte> translation, bool corrupt)
        {
            var buffer = new byte[300]; source.CopyTo(buffer, 0); var encoded = new List<byte>(343); byte checksum = 0;
            for (var index = 0; index < 86; index++)
            {
                var value = (byte)(((buffer[index] & 1) << 1) | ((buffer[index] & 2) >> 1) | ((buffer[index + 86] & 1) << 3) | ((buffer[index + 86] & 2) << 1) | ((buffer[index + 172] & 1) << 5) | ((buffer[index + 172] & 2) << 3));
                encoded.Add(translation[value ^ checksum]); checksum = value;
            }
            for (var index = 0; index < 256; index++) { var value = (byte)(source[index] >> 2); encoded.Add(translation[value ^ checksum]); checksum = value; }
            encoded.Add(translation[(checksum + (corrupt ? 1 : 0)) & 0x3f]); return encoded.ToArray();
        }
        const byte volume = 254; const byte track = 19; const byte sector = 11;
        var addressChecksum = (byte)(volume ^ track ^ sector ^ (corruptAddress ? 1 : 0));
        var address = FourAndFour(volume).Concat(FourAndFour(track)).Concat(FourAndFour(sector)).Concat(FourAndFour(addressChecksum));
        var data = Enumerable.Range(0, 256).Select(index => (byte)(index * 37 + 9)).ToArray();
        var calibration = new string('1', 100);
        var raw = calibration + Bits([0xd5,0xaa,0x96]) + Bits(address) + Bits([0xde,0xaa,0xeb,0xff,0xff,0xff]) + Bits([0xd5,0xaa,0xad]) + Bits(EncodeData(data, table, corruptData)) + Bits([0xde,0xaa,0xeb]) + "1";
        var intervals = BitsToIntervals(raw, 40);

        var result = new AppleGcrDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var decoded = Assert.Single(result.Sectors!); Assert.Equal(track, decoded.Cylinder); Assert.Equal(sector, decoded.Number); Assert.Equal(256, decoded.SizeBytes);
        Assert.Equal(!corruptAddress && !corruptData, decoded.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.AppleData && structure.Description.Contains(corruptData ? "invalid" : "valid", StringComparison.Ordinal));
        if (!corruptData) Assert.Equal(data, result.DecodedBytes.Skip(4).Take(256));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void AppleMacGcrDecoderExtractsAddressTagsDataAndChecksums(bool corruptHeader, bool corruptData)
    {
        byte[] table = [0x96,0x97,0x9a,0x9b,0x9d,0x9e,0x9f,0xa6,0xa7,0xab,0xac,0xad,0xae,0xaf,0xb2,0xb3,0xb4,0xb5,0xb6,0xb7,0xb9,0xba,0xbb,0xbc,0xbd,0xbe,0xbf,0xcb,0xcd,0xce,0xcf,0xd3,0xd6,0xd7,0xd9,0xda,0xdb,0xdc,0xdd,0xde,0xdf,0xe5,0xe6,0xe7,0xe9,0xea,0xeb,0xec,0xed,0xee,0xef,0xf2,0xf3,0xf4,0xf5,0xf6,0xf7,0xf9,0xfa,0xfb,0xfc,0xfd,0xfe,0xff];
        static string Bits(IEnumerable<byte> values) => string.Concat(values.Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        static byte[] EncodeData(byte[] source, IReadOnlyList<byte> translation, bool corrupt)
        {
            var b1 = new byte[175]; var b2 = new byte[175]; var b3 = new byte[175]; uint c1 = 0, c2 = 0, c3 = 0; var position = 0;
            for (var index = 0; ; index++)
            {
                c1 = (c1 & 0xff) << 1; if ((c1 & 0x100) != 0) c1++;
                var value = source[position++]; b1[index] = (byte)(value ^ c1); c3 += value; if ((c1 & 0x100) != 0) { c3++; c1 &= 0xff; }
                value = source[position++]; b2[index] = (byte)(value ^ c3); c2 += value; if (c3 > 0xff) { c2++; c3 &= 0xff; }
                if (position == source.Length) break;
                value = source[position++]; b3[index] = (byte)(value ^ c2); c1 += value; if (c2 > 0xff) { c1++; c2 &= 0xff; }
            }
            var symbols = new List<byte>(704) { 0 };
            for (var index = 0; index <= 174; index++)
            {
                var w4 = (byte)(((b1[index] >> 2) & 48) | ((b2[index] >> 4) & 12) | ((b3[index] >> 6) & 3));
                symbols.Add(w4); symbols.Add((byte)(b1[index] & 0x3f)); symbols.Add((byte)(b2[index] & 0x3f)); if (index != 174) symbols.Add((byte)(b3[index] & 0x3f));
            }
            var c4 = (byte)(((c1 & 0xc0) >> 6) | ((c2 & 0xc0) >> 4) | ((c3 & 0xc0) >> 2));
            symbols.Add(c4); symbols.Add((byte)(c3 & 0x3f)); symbols.Add((byte)(c2 & 0x3f)); symbols.Add((byte)(c1 & 0x3f));
            if (corrupt) symbols[^1] ^= 1;
            return symbols.Select(value => translation[value]).ToArray();
        }
        const byte cylinder = 198, head = 1, sectorNumber = 7, format = 0x12;
        var header = new byte[] { (byte)(cylinder & 0x3f), sectorNumber, (byte)(((cylinder >> 6) & 3) | (head << 5)), format };
        var headerChecksum = (byte)(header.Aggregate(0, (checksum, value) => checksum ^ value) & 0x3f); if (corruptHeader) headerChecksum ^= 1;
        var payload = Enumerable.Range(0, 512).Select(index => (byte)(index * 19 + 3)).ToArray();
        var tagged = Enumerable.Range(0, 12).Select(index => (byte)(0xa0 + index)).Concat(payload).ToArray();
        var raw = new string('1', 100) + Bits([0xd5, 0xaa, 0x96]) + Bits(header.Append(headerChecksum).Select(value => table[value])) + new string('0', 32)
            + Bits([0xd5, 0xaa, 0xad]) + Bits(EncodeData(tagged, table, corruptData)) + "1";
        var intervals = BitsToIntervals(raw, 40);

        var result = new AppleMacGcrDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var decoded = Assert.Single(result.Sectors!);
        Assert.Equal(cylinder, decoded.Cylinder); Assert.Equal(head, decoded.Head); Assert.Equal(sectorNumber, decoded.Number); Assert.Equal(512, decoded.SizeBytes);
        Assert.Equal(!corruptHeader && !corruptData, decoded.IntegrityValid);
        if (!corruptHeader) Assert.Equal(payload, result.DecodedBytes.TakeLast(512));
    }

    [Fact]
    public void AppleMacGcrDecoderReportsUnavailableIntegrityForTruncatedData()
    {
        byte[] table = [0x96,0x97,0x9a,0x9b,0x9d,0x9e,0x9f,0xa6,0xa7,0xab,0xac,0xad,0xae,0xaf,0xb2,0xb3,0xb4,0xb5,0xb6,0xb7,0xb9,0xba,0xbb,0xbc,0xbd,0xbe,0xbf,0xcb,0xcd,0xce,0xcf,0xd3,0xd6,0xd7,0xd9,0xda,0xdb,0xdc,0xdd,0xde,0xdf,0xe5,0xe6,0xe7,0xe9,0xea,0xeb,0xec,0xed,0xee,0xef,0xf2,0xf3,0xf4,0xf5,0xf6,0xf7,0xf9,0xfa,0xfb,0xfc,0xfd,0xfe,0xff];
        static string Bits(IEnumerable<byte> values) => string.Concat(values.Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        byte[] header = [3, 4, 0, 0x12]; var checksum = (byte)(header.Aggregate(0, (value, item) => value ^ item) & 0x3f);
        var raw = new string('1', 100) + Bits([0xd5, 0xaa, 0x96]) + Bits(header.Append(checksum).Select(value => table[value])) + new string('0', 32)
            + Bits([0xd5, 0xaa, 0xad]) + Bits(Enumerable.Repeat((byte)0xff, 650)) + "1";
        var intervals = BitsToIntervals(raw, 40);
        var result = new AppleMacGcrDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));
        Assert.Null(Assert.Single(result.Sectors!).IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.AppleData && structure.Description.Contains("unavailable", StringComparison.Ordinal));
    }

    [Fact]
    public void AppleGcrDecoderReportsUnavailableIntegrityWhenDataBlockIsMissing()
    {
        var calibration = new string('1', 100); var mark = string.Concat(Convert.FromHexString("D5AA96").Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        var address = string.Concat(Enumerable.Repeat("10101010", 8)); var epilogue = string.Concat(Convert.FromHexString("DEAAEB").Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        var intervals = BitsToIntervals(calibration + mark + address + epilogue + "0001", 40); var result = new AppleGcrDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));
        Assert.Null(Assert.Single(result.Sectors!).IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.AppleAddress && structure.Description.Contains("unavailable", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void CommodoreGcrDecoderExtractsTrackSectorAndValidatesData(bool corruptHeader, bool corruptData)
    {
        int[] table = [0x0a,0x0b,0x12,0x13,0x0e,0x0f,0x16,0x17,0x09,0x19,0x1a,0x1b,0x0d,0x1d,0x1e,0x15];
        string Encode(IEnumerable<byte> values) => string.Concat(values.SelectMany(value => new[] { value >> 4, value & 15 }).Select(nibble => Convert.ToString(table[nibble], 2).PadLeft(5, '0')));
        const byte track = 23; const byte sector = 8; const byte id2 = 0xa1; const byte id1 = 0x1a;
        var headerChecksum = (byte)(sector ^ track ^ id2 ^ id1 ^ (corruptHeader ? 1 : 0));
        byte[] header = [0x08, headerChecksum, sector, track, id2, id1];
        var data = Enumerable.Range(0, 256).Select(index => (byte)(index * 43 + 5)).ToArray(); byte checksum = 0; foreach (var value in data) checksum ^= value;
        if (corruptData) checksum ^= 1;
        var dataBlock = new byte[] { 0x07 }.Concat(data).Append(checksum).ToArray();
        var raw = new string('1', 100) + "000" + new string('1', 20) + Encode(header) + "000000" + new string('1', 20) + Encode(dataBlock) + "0001";
        var intervals = BitsToIntervals(raw, 40);

        var result = new CommodoreGcrDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var decoded = Assert.Single(result.Sectors!); Assert.Equal(track, decoded.Cylinder); Assert.Equal(sector, decoded.Number); Assert.Equal(256, decoded.SizeBytes);
        Assert.Equal(!corruptHeader && !corruptData, decoded.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData && structure.Description.Contains(corruptData ? "invalid" : "valid", StringComparison.Ordinal));
        Assert.Equal(data, result.DecodedBytes.Skip(7).Take(256));
    }

    [Fact]
    public void CommodoreGcrDecoderReportsUnavailableIntegrityWhenDataIsMissing()
    {
        int[] table = [0x0a,0x0b,0x12,0x13,0x0e,0x0f,0x16,0x17,0x09,0x19,0x1a,0x1b,0x0d,0x1d,0x1e,0x15];
        string Encode(IEnumerable<byte> values) => string.Concat(values.SelectMany(value => new[] { value >> 4, value & 15 }).Select(nibble => Convert.ToString(table[nibble], 2).PadLeft(5, '0')));
        byte[] header = [0x08, 0x03, 0x02, 0x01, 0xa1, 0xa1]; var raw = new string('1', 100) + "000" + new string('1', 20) + Encode(header) + "0001";
        var intervals = BitsToIntervals(raw, 40); var result = new CommodoreGcrDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));
        Assert.Null(Assert.Single(result.Sectors!).IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.CommodoreHeader && structure.Description.Contains("unavailable", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void AmigaMfmDecoderExtractsIdentityAndDecodesOddEvenData(bool corruptHeader, bool corruptData)
    {
        static byte Nibble(byte value, bool odd)
        {
            byte result = 0; var firstBit = odd ? 7 : 6; for (var index = 0; index < 4; index++) result |= (byte)(((value >> (firstBit - index * 2)) & 1) << (3 - index)); return result;
        }
        static byte[] EncodeOddEven(IReadOnlyList<byte> values)
        {
            var odd = new List<byte>(); var even = new List<byte>();
            for (var index = 0; index < values.Count; index += 2) { odd.Add((byte)((Nibble(values[index], true) << 4) | Nibble(values[index + 1], true))); even.Add((byte)((Nibble(values[index], false) << 4) | Nibble(values[index + 1], false))); }
            return odd.Concat(even).ToArray();
        }
        static (byte High, byte Low) Parity(IReadOnlyList<byte> encoded, bool split)
        {
            byte high = 0, low = 0;
            if (split) { var half = encoded.Count / 2; for (var index = 0; index < half; index += 2) { high ^= (byte)(encoded[index] ^ encoded[half + index]); low ^= (byte)(encoded[index + 1] ^ encoded[half + index + 1]); } }
            else for (var index = 0; index < encoded.Count; index += 4) { high ^= (byte)(encoded[index] ^ encoded[index + 2]); low ^= (byte)(encoded[index + 1] ^ encoded[index + 3]); }
            return (high, low);
        }
        const byte cylinder = 34; const byte head = 1; const byte sector = 7;
        byte[] info = [0xff, (byte)(cylinder << 1 | head), sector, 4]; var headerAndLabel = EncodeOddEven(info).Concat(new byte[16]).ToArray(); var headerParity = Parity(headerAndLabel, false);
        var data = Enumerable.Range(0, 512).Select(index => (byte)(index * 47 + 3)).ToArray(); var encodedData = EncodeOddEven(data); var dataParity = Parity(encodedData, true);
        if (corruptHeader) headerParity.High ^= 1; if (corruptData) dataParity.Low ^= 1;
        var encoded = headerAndLabel.Concat(new byte[] { 0,0,headerParity.High,headerParity.Low,0,0,dataParity.High,dataParity.Low }).Concat(encodedData).ToArray();
        var raw = string.Concat(Enumerable.Repeat("10", 50)) + Convert.ToString(0x44894489, 2).PadLeft(32, '0') + EncodeMfmBytesFromZero(encoded) + "001";
        var intervals = BitsToIntervals(raw, 40);

        var result = new AmigaMfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var decoded = Assert.Single(result.Sectors!); Assert.Equal(cylinder, decoded.Cylinder); Assert.Equal(head, decoded.Head); Assert.Equal(sector, decoded.Number); Assert.Equal(512, decoded.SizeBytes);
        Assert.Equal(!corruptHeader && !corruptData, decoded.IntegrityValid);
        Assert.Equal(data, result.DecodedBytes.Skip(4).Take(512));
    }

    [Fact]
    public void AmigaMfmDecoderReportsUnavailableIntegrityWhenDataIsTruncated()
    {
        var encodedHeader = new byte[28]; encodedHeader[0] = 0xf0; encodedHeader[2] = 0xf0;
        var raw = string.Concat(Enumerable.Repeat("10", 50)) + Convert.ToString(0x44894489, 2).PadLeft(32, '0') + EncodeMfmBytesFromZero(encodedHeader) + "001";
        var intervals = BitsToIntervals(raw, 40); var result = new AmigaMfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));
        Assert.Null(Assert.Single(result.Sectors!).IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.AmigaSync && structure.Description.Contains("unavailable", StringComparison.Ordinal));
    }

    [Fact]
    public void NativeChecksumDecodersReportCorruptedBlocks()
    {
        var northstarData = new byte[512];
        var northstarBlock = Enumerable.Repeat((byte)0, 7)
            .Concat([(byte)0xfb, (byte)0x21])
            .Concat(northstarData)
            .Append((byte)0x01)
            .ToArray();
        var northstarIntervals = BitsToIntervals(EncodeMfmBytesFromZero(northstarBlock) + "001", 40);
        var northstar = new NorthstarMfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)northstarIntervals.Count, northstarIntervals));

        static byte Reverse(byte value) { byte result = 0; for (var bit = 0; bit < 8; bit++) result = (byte)((result << 1) | ((value >> bit) & 1)); return result; }
        var heathkitBits = EncodeFmBytes(0, 0, 0, 0xbf, Reverse(1), Reverse(2), Reverse(3), Reverse(0xff)) + "001";
        var heathkitIntervals = BitsToIntervals(heathkitBits, 40);
        var heathkit = new HeathkitFmDecoder().Decode(new ScpRevolution(8_000_000, (uint)heathkitIntervals.Count, heathkitIntervals));

        Assert.False(Assert.Single(northstar.Sectors!).IntegrityValid);
        Assert.False(Assert.Single(heathkit.Sectors!).IntegrityValid);
    }

    [Theory]
    [InlineData("membrain.mfm", "44895554", FluxStructureKind.FormatHeader)]
    [InlineData("aed6200p.mfm", "5094", FluxStructureKind.FormatHeader)]
    [InlineData("qdmo5.mfm", "A914A914A914A914A9144491", FluxStructureKind.FormatHeader)]
    [InlineData("centurion.mfm", "91224489", FluxStructureKind.FormatHeader)]
    [InlineData("emu.fm", "4545555545545445", FluxStructureKind.FormatHeader)]
    [InlineData("arburg", "5555555555249249", FluxStructureKind.FormatHeader)]
    [InlineData("victor9k.gcr", "5555555555551111", FluxStructureKind.FormatHeader)]
    [InlineData("tycom.fm", "55111444", FluxStructureKind.FormatData)]
    [InlineData("dec.rx02", "55111545", FluxStructureKind.FormatData)]
    public async Task SignatureMfmDecodersRecognizeTheirNativeMarks(string decoderId, string hexadecimal, FluxStructureKind expectedKind)
    {
        var mark = string.Concat(Convert.FromHexString(hexadecimal).Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        var calibration = decoderId is "emu.fm" or "tycom.fm" or "dec.rx02" or "arburg" or "victor9k.gcr" ? "" : string.Concat(Enumerable.Repeat("10", 50));
        var bits = calibration + string.Concat(Enumerable.Repeat(mark + "000", 4)) + "001";
        var intervals = BitsToIntervals(bits, 40);
        var image = new ScpReader().Read(BuildSingleTrackScp(intervals));
        var track = Assert.Single(image.Tracks);
        var result = new FluxDecoderRegistry().Decode(decoderId, Assert.Single(track.Revolutions));
        Assert.Contains(result.Structures, structure => structure.Kind == expectedKind);

        static string Localize(string key, object[] arguments) => arguments.Length == 0 ? key : $"{key}({string.Join(',', arguments)})";
        var inspection = new ScpInspectorPresenter(new FluxDecoderRegistry(), Localize).Build(image, track, decoderId);
        Assert.Contains("Visual.StructureKind." + expectedKind, inspection);

        using var bitmap = new SKBitmap(320, 320);
        using var canvas = new SKCanvas(bitmap);
        IScpRenderer renderer = new SkiaScpRenderer { DecoderId = decoderId };
        await renderer.PrepareAsync(image, 0);
        renderer.Render(canvas, new ScpRenderRequest(image, 0, track, 320, 320, new SKPoint(160, 160), 1, "No data", "Side 0"));
        var overlay = expectedKind == FluxStructureKind.FormatData ? new SKColor(67, 220, 255) : new SKColor(255, 205, 64);
        Assert.Contains(Enumerable.Range(0, bitmap.Height).SelectMany(y => Enumerable.Range(0, bitmap.Width).Select(x => bitmap.GetPixel(x, y))), color => color == overlay);
        renderer.ClearCache();
    }

    [Fact]
    public void DecoderRegistrySelectsMostConvincingRevolution()
    {
        var weak = new ScpRevolution(8_000_000, 2, [40u, 40u]);
        var prologues = string.Concat(Enumerable.Repeat(Convert.ToString(0xD5AA96, 2).PadLeft(24, '0') + "1", 8));
        var strongIntervals = BitsToIntervals(prologues, 40);
        var strong = new ScpRevolution(8_000_000, (uint)strongIntervals.Count, strongIntervals);
        var best = new FluxDecoderRegistry().DecodeBest([weak, strong], "apple2.gcr");
        Assert.NotNull(best); Assert.Equal(1, best.Value.RevolutionIndex); Assert.Equal("apple2.gcr", best.Value.Result.DecoderId);
    }

    [Fact]
    public void AutomaticDecoderRejectsInvalidOnlyFalseRecognitionInFavorOfRawFlux()
    {
        var invalid = new FluxDecodeResult("false.fm", "False FM", 1, 40, [new(FluxStructureKind.DeletedDataAddressMark, 0, 16, "false")], [], [new(0, 0, 1, 2, 512, false, 0)]);
        var raw = new FluxDecodeResult("raw", "Raw", .05, 40, [], []);
        var valid = invalid with { DecoderId = "valid.mfm", Sectors = [new(0, 0, 1, 2, 512, true, 0)] };

        Assert.True(AutomaticScore(raw) > AutomaticScore(invalid));
        Assert.True(AutomaticScore(valid) > AutomaticScore(raw));

        static double AutomaticScore(FluxDecodeResult result)
        {
            var method = typeof(FluxDecoderRegistry).GetMethod("AutomaticScore", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                ?? throw new MissingMethodException(typeof(FluxDecoderRegistry).FullName, "AutomaticScore");
            return (double)(method.Invoke(null, [result]) ?? throw new InvalidOperationException("Automatic decoder score returned no result."));
        }
    }

    [Fact]
    public void ConversionTagPatternIsAppliedWithoutForcingBrackets()
    {
        var planner = new ConversionPlanner(new BuiltInImageFormatCatalog());
        var output = Assert.Single(planner.Plan("disk.scp", "out", "disk", [new ConversionSelection("ibm.720", new HashSet<string>())], true, "TAG-{tag} "));
        Assert.Equal("TAG-PC-720 disk.ima", Path.GetFileName(output.OutputPath));
        Assert.Throws<ArgumentException>(() => planner.Plan("disk.scp", "out", "disk", [new ConversionSelection("ibm.720", new HashSet<string>())], true, "_format"));
    }

    [Fact]
    public void ConversionTagVariablesProduceDeterministicFilenameSafeNames()
    {
        var planner = new ConversionPlanner(new BuiltInImageFormatCatalog());
        var familyFormat = Assert.Single(planner.Plan("disk.scp", "out", "disk", [new ConversionSelection("ibm.720", new HashSet<string>())], true, "[{FAMILY}-{FORMAT}-{EXTENSION}] "));
        Assert.Equal("[PC-720-IMA] disk.ima", Path.GetFileName(familyFormat.OutputPath));

        var format = new DiskFormat("ibm.720", "IBM PC", "IBM PC 720", [new ImageExtension(".ima", "IMA", true)], Tag: "PC-720");
        var rendered = ConversionPlanner.FormatTag("{NAME}_{DATE:YYYY-MM-DD}_{TIME:HH-MM-SS}_{TAG}", format, ".ima", "disk", new DateTime(2026, 8, 6, 14, 35, 42));
        Assert.Equal("disk_2026-08-06_14-35-42_PC-720", rendered);
    }

    [Fact]
    public void ReadViewModelBuildsNumericAndAlphabeticTargetsAndAdvancesOnlyWhenRequested()
    {
        var model = new GWGUI.App.ViewModels.ReadOperationViewModel
        {
            Folder = Path.Combine("images", "magazines"),
            FileName = "Tilt",
            AutoNumber = true,
            SequenceWidthIndex = 1,
            SequenceValue = "9"
        };

        Assert.Equal(Path.Combine("images", "magazines", "Tilt 09.scp"), model.BuildTarget(".scp", "Exemple"));
        Assert.True(model.TryAdvanceSequence());
        Assert.Equal("10", model.SequenceValue);

        model.SequenceKindIndex = 1;
        model.SequenceValue = "Z";
        Assert.Equal(Path.Combine("images", "magazines", "Tilt AZ.scp"), model.BuildTarget(".scp", "Exemple"));
        Assert.True(model.TryAdvanceSequence());
        Assert.Equal("AA", model.SequenceValue);
    }

    [Fact]
    public void ReadViewModelUsesExampleWithoutMutatingAnEmptyName()
    {
        var model = new GWGUI.App.ViewModels.ReadOperationViewModel { Folder = "images", FileName = "   " };
        Assert.Equal(Path.Combine("images", "Exemple.scp"), model.BuildTarget(".scp", "Exemple"));
        Assert.False(model.TryAdvanceSequence());
        Assert.Equal("   ", model.FileName);
    }

    [Fact]
    public void ReadViewModelDefaultProfileRemovesEveryOptionalGwArgument()
    {
        var model = new ReadOperationViewModel();
        model.ApplyOptions(new HashSet<string> { "revs", "tracks", "reverse", "diskdefs" }, new Dictionary<string, string>
        {
            ["revs"] = "3", ["tracks"] = "c=0-39:h=0", ["diskdefs"] = "custom.cfg", ["expert"] = "--raw"
        });
        Assert.Equal(4, model.BuildOptions().Count);

        model.ApplyOptions(new HashSet<string>(), new Dictionary<string, string>());

        Assert.Empty(model.BuildOptions());
        Assert.Empty(model.CaptureEnabledOptions());
        Assert.Equal("", model.ExpertArguments);
    }

    [Fact]
    public void ReadViewModelMapsProfileValuesAndEnforcesExclusiveOptions()
    {
        var model = new ReadOperationViewModel();
        model.ApplyOptions(new HashSet<string> { "retries", "densel" }, new Dictionary<string, string> { ["retries"] = "7", ["densel"] = "L" });
        Assert.Equal([new EnabledOption("--retries", "7"), new EnabledOption("--densel", "L")], model.BuildOptions());

        model.EnableTg43();
        model.EnableHardSectors();
        model.EnableFakeIndex();

        Assert.False(model.Densel.Enabled);
        Assert.True(model.Tg43.Enabled);
        Assert.False(model.HardSectors.Enabled);
        Assert.True(model.FakeIndex.Enabled);
        Assert.Contains("gen-tg43", model.CaptureEnabledOptions());
    }

    [Fact]
    public void WriteViewModelDefaultProfileRestoresVerificationAndClearsOptionalArguments()
    {
        var model = new WriteOperationViewModel();
        model.ApplyOptions(new HashSet<string> { "no-verify", "retries", "pre-erase" }, new Dictionary<string, string> { ["retries"] = "4", ["expert"] = "--raw" });
        Assert.True(model.DisableVerification);
        Assert.Equal([new EnabledOption("--retries", "4"), new EnabledOption("--pre-erase")], model.BuildOptions());

        model.ApplyOptions(new HashSet<string>(), new Dictionary<string, string>());

        Assert.False(model.DisableVerification);
        Assert.Empty(model.BuildOptions());
        Assert.Empty(model.CaptureEnabledOptions());
        Assert.Equal("", model.ExpertArguments);
    }

    [Fact]
    public void WriteViewModelRoundTripsProfilesAndEnforcesHardwareExclusions()
    {
        var model = new WriteOperationViewModel();
        model.ApplyOptions(new HashSet<string> { "tracks", "densel", "diskdefs" }, new Dictionary<string, string>
        {
            ["tracks"] = "c=0-39:h=0", ["densel"] = "L", ["diskdefs"] = "formats.cfg", ["expert"] = "--foo bar"
        });
        model.EnableTg43();
        model.EnableHardSectors();
        model.EnableFakeIndex();

        Assert.Equal("L", model.Densel.Value);
        Assert.False(model.Densel.Enabled);
        Assert.True(model.Tg43.Enabled);
        Assert.False(model.HardSectors.Enabled);
        Assert.True(model.FakeIndex.Enabled);
        Assert.Equal("--foo bar", model.CaptureValues()["expert"]);
        Assert.Contains("diskdefs", model.CaptureEnabledOptions());
    }

    [Fact]
    public void ConversionViewModelDefaultProfileClearsFormatsTagsAndOptionalArguments()
    {
        var model = new ConversionOperationViewModel();
        model.ApplyProfile(new HashSet<string> { "tags", "tracks", "format:ibm.720" }, new Dictionary<string, string>
        {
            ["tracks"] = "c=0-39:h=0", ["extensions:ibm.720"] = ".ima,.img", ["expert"] = "--raw"
        });
        Assert.True(model.AddTags);
        Assert.Contains("ibm.720", model.SelectedFormats);

        model.ApplyProfile(new HashSet<string>(), new Dictionary<string, string>());

        Assert.False(model.AddTags);
        Assert.Empty(model.SelectedFormats);
        Assert.Empty(model.ExplicitExtensions);
        Assert.Empty(model.BuildOptions());
        Assert.Equal("", model.ExpertArguments);
    }

    [Fact]
    public void ConversionViewModelRoundTripsMultipleFormatsAndExplicitExtensions()
    {
        var catalog = new BuiltInImageFormatCatalog();
        var model = new ConversionOperationViewModel { AddTags = true };
        model.Tracks.Enabled = true;
        model.Tracks.Value = "c=0-79:h=0-1";
        model.SetFormat("ibm.720", true, [".ima", ".img"]);
        model.SetFormat("atarist.720", true, []);

        var enabled = model.CaptureProfileEnabled();
        var values = model.CaptureProfileValues();
        var restored = new ConversionOperationViewModel();
        restored.ApplyProfile(enabled, values);
        var selections = restored.BuildSelections(catalog.Formats).ToArray();

        Assert.True(restored.AddTags);
        Assert.Equal(2, selections.Length);
        Assert.True(selections.Single(x => x.FormatId == "ibm.720").ExplicitExtensions.SetEquals([".ima", ".img"]));
        Assert.Empty(selections.Single(x => x.FormatId == "atarist.720").ExplicitExtensions);
        Assert.Equal([new EnabledOption("--tracks", "c=0-79:h=0-1")], restored.BuildOptions());
    }

    private sealed class ScriptedRunner(params GwExecutionResult[] results) : IGreaseweazleRunner
    {
        private readonly Queue<GwExecutionResult> _results = new(results);
        public List<GwCommand> Commands { get; } = [];
        public bool IsRunning { get; private set; }
        public Task<GwExecutionResult> RunAsync(GwCommand command, IProgress<GwOutputLine>? output = null, CancellationToken cancellationToken = default)
        {
            Commands.Add(command); IsRunning = true;
            try { return Task.FromResult(_results.Dequeue()); }
            finally { IsRunning = false; }
        }
    }

    private sealed class BusyRunner : IGreaseweazleRunner
    {
        public bool IsRunning => true;
        public Task<GwExecutionResult> RunAsync(GwCommand command, IProgress<GwOutputLine>? output = null, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("The shared runner is busy.");
    }

    private sealed class StubScpReader(ScpImage image) : IScpReader
    {
        public string? Path { get; private set; }
        public Task<ScpImage> ReadAsync(string path, CancellationToken cancellationToken = default) { Path = path; return Task.FromResult(image); }
    }

    private sealed class StaticSerialDeviceDiscovery(IReadOnlyList<SerialDevice> devices) : ISerialDeviceDiscovery
    {
        public IReadOnlyList<SerialDevice> FindSerialDevices() => devices;
    }

    private sealed class StaticHardwareRegistry(IReadOnlyList<ControllerSettings> controllers, IReadOnlyList<ControllerSettings>? unconfigured = null) : IHardwareRegistry
    {
        public Task<HardwareScanResult> ScanAsync(string executable, IReadOnlyList<ControllerSettings> configuredControllers, CancellationToken cancellationToken = default) =>
            Task.FromResult(new HardwareScanResult(controllers, unconfigured ?? []));
    }

    private sealed class MutableSerialDeviceDiscovery(IReadOnlyList<SerialDevice> devices) : ISerialDeviceDiscovery
    {
        public IReadOnlyList<SerialDevice> Devices { get; set; } = devices;
        public IReadOnlyList<SerialDevice> FindSerialDevices() => Devices;
    }

    private sealed class DeviceInfoRunner(IReadOnlyDictionary<string, (string Serial, string Model)> devices) : IGreaseweazleRunner
    {
        public bool IsRunning { get; private set; }

        public Task<GwExecutionResult> RunAsync(GwCommand command, IProgress<GwOutputLine>? output = null, CancellationToken cancellationToken = default)
        {
            IsRunning = true;
            try
            {
                var deviceIndex = command.Arguments.ToList().IndexOf("--device");
                var port = deviceIndex >= 0 && deviceIndex + 1 < command.Arguments.Count ? command.Arguments[deviceIndex + 1] : "";
                if (!devices.TryGetValue(port, out var device)) return Task.FromResult(new GwExecutionResult(1, false, TimeSpan.Zero, []));
                GwOutputLine[] lines =
                [
                    new(DateTimeOffset.UtcNow, GwOutputStream.Standard, $"Model: {device.Model}"),
                    new(DateTimeOffset.UtcNow, GwOutputStream.Standard, $"Serial: {device.Serial}")
                ];
                return Task.FromResult(new GwExecutionResult(0, false, TimeSpan.Zero, lines));
            }
            finally { IsRunning = false; }
        }
    }

    private sealed class RecordingMessageDialogService(UserDialogResult result = UserDialogResult.Ok) : IMessageDialogService
    {
        public List<(string Message, string Title, UserDialogButtons Buttons, UserDialogIcon Icon)> Requests { get; } = [];
        public UserDialogResult Show(string message, string title, UserDialogButtons buttons = UserDialogButtons.Ok, UserDialogIcon icon = UserDialogIcon.None)
        {
            Requests.Add((message, title, buttons, icon));
            return result;
        }
    }

    private sealed class RecordingFileDialogService : IFileDialogService
    {
        public string? OpenResult { get; set; }
        public string? SaveResult { get; set; }
        public string? FolderResult { get; set; }
        public List<OpenFileRequest> OpenRequests { get; } = [];
        public List<SaveFileRequest> SaveRequests { get; } = [];
        public List<SelectFolderRequest> FolderRequests { get; } = [];
        public string? OpenFile(OpenFileRequest request) { OpenRequests.Add(request); return OpenResult; }
        public string? SaveFile(SaveFileRequest request) { SaveRequests.Add(request); return SaveResult; }
        public string? SelectFolder(SelectFolderRequest request) { FolderRequests.Add(request); return FolderResult; }
    }

    private sealed class RecordingSettingsStore : ISettingsStore
    {
        public int SaveCount { get; private set; }
        public Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default) => Task.FromResult(new AppSettings());
        public Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default) { SaveCount++; return Task.CompletedTask; }
    }

    private sealed class DelayedSettingsStore : ISettingsStore
    {
        public Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default) => Task.FromResult(new AppSettings());
        public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default) => await Task.Delay(20, cancellationToken);
    }

    private sealed class FailingSettingsStore : ISettingsStore
    {
        public Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default) => Task.FromResult(new AppSettings());
        public Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default) => Task.FromException(new IOException("test save failure"));
    }

    private sealed class RecordingBusinessDialogService : IBusinessDialogService
    {
        public string? ProfileNameResult { get; set; }
        public int ProfilePromptCount { get; private set; }
        public IReadOnlyList<ConversionConflictDecision>? ConflictResult { get; set; }
        public ReadConflictChoice? ReadConflictResult { get; set; }
        public MissingHardwareChoice MissingHardwareResult { get; set; } = MissingHardwareChoice.Continue;
        public List<IReadOnlyList<ControllerSettings>> MissingHardwareRequests { get; } = [];
        public string? PromptProfileName(string? initialName = null) { ProfilePromptCount++; return ProfileNameResult; }
        public ReadConflictChoice? ResolveReadConflict(string outputPath) => ReadConflictResult;
        public IReadOnlyList<ConversionConflictDecision>? ResolveConversionConflicts(IReadOnlyList<ConversionOutput> outputs) => ConflictResult;
        public MissingHardwareChoice ResolveMissingHardware(IReadOnlyList<ControllerSettings> controllers)
        {
            MissingHardwareRequests.Add(controllers);
            return MissingHardwareResult;
        }
    }

    private sealed class RecordingWindowNavigationService : IWindowNavigationService
    {
        public bool OptionsResult { get; set; }
        public int AboutCount { get; private set; }
        public List<AppSettings> OptionsSettings { get; } = [];
        public List<string> LogDirectories { get; } = [];
        public List<GwToolWindowRequest> ToolRequests { get; } = [];
        public List<OptionsSection> OptionsSections { get; } = [];
        public bool ShowOptions(AppSettings settings, OptionsSection section = OptionsSection.General) { OptionsSettings.Add(settings); OptionsSections.Add(section); return OptionsResult; }
        public void ShowLogHistory(string logsDirectory) => LogDirectories.Add(logsDirectory);
        public void ShowAbout() => AboutCount++;
        public void ShowGwTool(GwToolWindowRequest request) => ToolRequests.Add(request);
    }

    private sealed class ImmediateProgress<T>(Action<T> report) : IProgress<T>
    {
        public void Report(T value) => report(value);
    }

    private static System.Windows.Controls.ScrollViewer GetScrollViewer(System.Windows.DependencyObject parent)
    {
        for (var index = 0; index < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, index);
            if (child is System.Windows.Controls.ScrollViewer scrollViewer) return scrollViewer;
            var nested = GetScrollViewerOrDefault(child);
            if (nested is not null) return nested;
        }

        throw new InvalidOperationException("No ScrollViewer found in the visual tree.");
    }

    private static System.Windows.Controls.ScrollViewer? GetScrollViewerOrDefault(System.Windows.DependencyObject parent)
    {
        for (var index = 0; index < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, index);
            if (child is System.Windows.Controls.ScrollViewer scrollViewer) return scrollViewer;
            var nested = GetScrollViewerOrDefault(child);
            if (nested is not null) return nested;
        }

        return null;
    }

    private static string EncodeMfmBytes(params byte[] values) { var result = new System.Text.StringBuilder(); var previous = 1; foreach (var value in values) for (var bit = 7; bit >= 0; bit--) { var data = (value >> bit) & 1; var clock = previous == 0 && data == 0 ? 1 : 0; result.Append(clock).Append(data); previous = data; } return result.ToString(); }
    private static byte[] BuildSingleTrackScp(IReadOnlyList<uint> intervals)
    {
        if (intervals.Any(value => value is 0 or > ushort.MaxValue)) throw new ArgumentOutOfRangeException(nameof(intervals));
        var data = new byte[0x2c0 + intervals.Count * 2];
        data[0] = (byte)'S'; data[1] = (byte)'C'; data[2] = (byte)'P'; data[3] = 0x25; data[5] = 1; data[6] = 0; data[7] = 0; data[8] = (byte)ScpFlags.IndexAligned;
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x10, 4), 0x2b0);
        data[0x2b0] = (byte)'T'; data[0x2b1] = (byte)'R'; data[0x2b2] = (byte)'K';
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x2b4, 4), intervals.Aggregate(0u, (sum, value) => checked(sum + value)));
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x2b8, 4), (uint)intervals.Count);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x2bc, 4), 16);
        for (var index = 0; index < intervals.Count; index++) System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(0x2c0 + index * 2, 2), (ushort)intervals[index]);
        uint checksum = 0; foreach (var value in data.AsSpan(0x10)) checksum = unchecked(checksum + value);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x0c, 4), checksum);
        return data;
    }
    private static string EncodeMfmBytesFromZero(params byte[] values) { var result = new System.Text.StringBuilder(); var previous = 0; foreach (var value in values) for (var bit = 7; bit >= 0; bit--) { var data = (value >> bit) & 1; var clock = previous == 0 && data == 0 ? 1 : 0; result.Append(clock).Append(data); previous = data; } return result.ToString(); }
    private static string EncodeFmBytes(params byte[] values) => string.Concat(values.SelectMany(value => Enumerable.Range(0, 8).Select(bit => "1" + (((value >> (7 - bit)) & 1) != 0 ? "1" : "0"))));
    private static List<uint> BitsToIntervals(string bits, uint cellTicks) { var result = new List<uint>(); var cells = 0; foreach (var bit in bits) { cells++; if (bit == '1') { result.Add((uint)cells * cellTicks); cells = 0; } } return result; }
    private static ushort TestCrc16(IEnumerable<byte> values) => TestCrc16(values, 0x1021, 0xffff);
    private static ushort TestCrc16(IEnumerable<byte> values, ushort polynomial, ushort initial) { var crc = initial; foreach (var value in values) { crc ^= (ushort)(value << 8); for (var bit = 0; bit < 8; bit++) crc = (ushort)((crc & 0x8000) != 0 ? (crc << 1) ^ polynomial : crc << 1); } return crc; }
}
