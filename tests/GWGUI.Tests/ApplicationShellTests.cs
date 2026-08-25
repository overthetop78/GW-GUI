using GWGUI.App;
using GWGUI.App.Contracts.Rendering.Scp;
using GWGUI.App.Contracts.ViewModels.Conversion;
using GWGUI.App.Enums.Rendering.Scp;
using GWGUI.App.Enums.Services.Dialogs;
using GWGUI.App.Interfaces.Rendering.Scp;
using GWGUI.App.Functions.Rendering.Scp;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Presenters.Visualization;
using GWGUI.App.Rendering.Scp;
using GWGUI.App.Services.Logging;
using GWGUI.App.Services.Theming;
using GWGUI.App.Services.Visualization;
using GWGUI.App.Services.Windows;
using GWGUI.App.ViewModels.Main;
using GWGUI.App.ViewModels.Options;
using GWGUI.App.Views.Controls.Conversion;
using GWGUI.App.Views.Controls.Explorer;
using GWGUI.App.Views.Controls.Visualization;
using GWGUI.App.Views.Dialogs.Conversion;
using GWGUI.App.Views.Dialogs.Hardware;
using GWGUI.App.Views.Dialogs.Profiles;
using GWGUI.App.Views.Dialogs.Read;
using GWGUI.App.Views.Windows.Logs;
using GWGUI.App.Views.Windows.Options;
using GWGUI.App.Views.Windows.Shell;
using GWGUI.App.Views.Windows.Tools;
using GWGUI.Domain.Conversion;
using GWGUI.Domain.Formats;
using GWGUI.Domain.Profiles;
using GWGUI.Domain.Settings;
using GWGUI.Domain.Settings.Hardware;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Exploration.Scp;
using System.IO;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Exploration;
using GWGUI.Infrastructure.Processes;
using GWGUI.Infrastructure.Settings;
using GWGUI.Infrastructure.Hardware;
using SkiaSharp;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Threading;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace GWGUI.Tests;

public sealed class ApplicationShellTests : CoreTestBase
{
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
        var image = new ScpImage(new ScpHeader(0x24, 0, 1, 0, 0, ScpFlags.IndexAligned, ScpBitCellEncoding.Default16Bit, ScpHeadSelection.Side1, 0, 0), [track], true, 1024);
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
        var image = new ScpImage(new ScpHeader(0x24, 0, 1, 8, 8, ScpFlags.IndexAligned, ScpBitCellEncoding.Default16Bit, ScpHeadSelection.Side1, 0, 0), [track], true, 1024);
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
        static SKBitmap Render(DiskMediaCategory kind, int head)
        {
            var bitmap = new SKBitmap(256, 256);
            using var canvas = new SKCanvas(bitmap);
            new SkiaScpRenderer().Render(canvas, new ScpRenderRequest(null, head, null, 256, 256,
                new SKPoint(128, 128), 1, string.Empty, string.Empty, kind));
            return bitmap;
        }

        using var ddFront = Render(DiskMediaCategory.ThreeHalfDd, 0);
        using var ddBack = Render(DiskMediaCategory.ThreeHalfDd, 1);
        using var hdFront = Render(DiskMediaCategory.ThreeHalfHd, 0);

        Assert.True(ddFront.GetPixel(20, 40).Blue > ddFront.GetPixel(20, 40).Red);
        Assert.True(hdFront.GetPixel(20, 40).Red > hdFront.GetPixel(20, 40).Blue);
        Assert.NotEqual(ddFront.GetPixel(80, 15), ddBack.GetPixel(80, 15));
        Assert.NotEqual(ddFront.GetPixel(20, 236), hdFront.GetPixel(20, 236));
        Assert.Equal(110.08f, ScpMediaGeometryFunctions.FluxRadius(256, 256, 1, DiskMediaCategory.ThreeHalfDd), 2);
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
        var image = new ScpImage(new ScpHeader(0x24, 0, 1, 11, 11, ScpFlags.IndexAligned, ScpBitCellEncoding.Default16Bit, ScpHeadSelection.Side1, 0, 0), [track], true, 1024);

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
        var header = new ScpHeader(0x24, 0, 1, 4, 5, ScpFlags.IndexAligned, ScpBitCellEncoding.Default16Bit, ScpHeadSelection.Side1, 1, 0);
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
        WpfTestHost.Run(() =>
        {
                var app = Assert.IsType<GWGUI.App.App>(Application.Current);
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
                foreach (var automationId in new[] { "ReadExecuteButton", "WriteExecuteButton", "ConvertExecuteButton", "EraseExecuteButton", "CleanExecuteButton" })
                    Assert.Equal(automationId, AutomationProperties.GetAutomationId(Assert.IsType<System.Windows.Controls.Button>(window.FindName(automationId))));
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
                        && line.Contains("· IMG ·", StringComparison.Ordinal));
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
                var saveFailureDeadline = DateTime.UtcNow.AddSeconds(5);
                while (failingSaveDialogs.Requests.Count == 0 && DateTime.UtcNow < saveFailureDeadline)
                {
                    Dispatcher.CurrentDispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                    Thread.Sleep(1);
                }
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
                Assert.Contains("2", Assert.IsType<HardwareRow>(orphanList.Items[0]).ReaderLabel, StringComparison.Ordinal);
                typeof(OptionsWindow).GetMethod("MergeUnconfigured", hardwareFlags)!.Invoke(orphanWindow,
                    [new[] { new ControllerSettings { UsbId = "GW-ORPHAN", LastPort = "COM4", IsAvailable = true } }]);
                typeof(OptionsWindow).GetMethod("RefreshHardwareRows", hardwareFlags)!.Invoke(orphanWindow, null);
                var repairedRow = Assert.IsType<HardwareRow>(Assert.Single(orphanList.Items));
                Assert.Contains("1", repairedRow.ReaderLabel, StringComparison.Ordinal);
                Assert.NotNull(repairedRow.DriveId);
                Assert.True(repairedRow.Configured);
                orphanWindow.Close();
                var expectedDarkText = Assert.IsType<System.Windows.Media.SolidColorBrush>(app.Resources["TextBrush"]).Color;
                var expectedDarkAccent = Assert.IsType<System.Windows.Media.SolidColorBrush>(app.Resources["AccentBrush"]).Color;
                var expectedDarkControl = Assert.IsType<System.Windows.Media.SolidColorBrush>(app.Resources["ControlBrush"]).Color;
                Assert.Equal(expectedDarkText, Assert.IsType<System.Windows.Media.SolidColorBrush>(Assert.IsType<System.Windows.Controls.CheckBox>(optionsWindow.FindName("UseTagsCheck")).Foreground).Color);
                Assert.Equal(expectedDarkControl, Assert.IsType<System.Windows.Media.SolidColorBrush>(theme.Background).Color);
                Assert.Equal(expectedDarkAccent, Assert.IsType<System.Windows.Media.SolidColorBrush>(Assert.IsType<System.Windows.Controls.TabItem>(optionsNavigation.Items[0]).Foreground).Color);
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
        });
    }
}
