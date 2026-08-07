using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GWGUI.Domain.Commands;
using GWGUI.Domain.Settings;
using GWGUI.Domain.Formats;
using GWGUI.Domain.Read;
using GWGUI.Domain.Naming;
using GWGUI.Domain.Profiles;
using GWGUI.Domain.Write;
using GWGUI.Domain.Conversion;
using GWGUI.Domain.Maintenance;
using GWGUI.Domain.Hardware;
using GWGUI.Domain.HostTools;
using GWGUI.Scp;
using GWGUI.Scp.Decoding;
using GWGUI.Scp.Images;
using GWGUI.Infrastructure.Processes;
using GWGUI.Infrastructure.Settings;
using GWGUI.App.Localization;
using GWGUI.Infrastructure.HostTools;
using GWGUI.Infrastructure.Hardware;
using GWGUI.App.ViewModels;
using GWGUI.App.Services;
using GWGUI.App.Controls;
using GWGUI.App.Rendering;

namespace GWGUI.App;

public partial class MainWindow : Window
{
    private RadioButton RawScpRadio => ReadImageBlock.RawScpRadio;
    private RadioButton KnownFormatRadio => ReadImageBlock.KnownFormatRadio;
    private Grid KnownFormatPanel => ReadImageBlock.KnownFormatPanel;
    private ComboBox ReadFamilyCombo => ReadImageBlock.FamilyCombo;
    private ComboBox ReadFormatCombo => ReadImageBlock.FormatCombo;
    private ComboBox ReadExtensionCombo => ReadImageBlock.ExtensionCombo;
    private ComboBox ReadProfileCombo => ReadProfileBlock.ProfileCombo;
    private ComboBox WriteProfileCombo => WriteProfileBlock.ProfileCombo;
    private TextBox WriteSourceText => WriteSourceBlock.Input;
    private TextBlock WriteDetectionText => WriteFormatBlock.DetectionText;
    private ComboBox WriteFormatCombo => WriteFormatBlock.FormatCombo;
    private CheckBox WriteNoVerify => WriteAdvancedBlock.NoVerifyCheckBox;
    private CheckBox WriteDiskDefsEnabled => WriteAdvancedBlock.DiskDefinitionsEnabled;
    private TextBox WriteDiskDefsValue => WriteAdvancedBlock.DiskDefinitionsValue;
    private ComboBox ConvertProfileCombo => ConvertProfileBlock.ProfileCombo;
    private TextBox ConvertSourceText => ConvertSourceBlock.Input;
    private TextBox ConvertOutputName => ConvertOutputBlock.OutputNameTextBox;
    private CheckBox ConvertTags => ConvertOutputBlock.TagsCheckBox;
    private TextBlock ConvertSourceInfo => ConvertOutputBlock.SourceInformation;
    private ItemsControl ConvertPinnedPanel => ConvertFormatsBlock.PinnedItems;
    private ItemsControl ConvertCommonPanel => ConvertFormatsBlock.CommonItems;
    private ItemsControl ConvertRarePanel => ConvertFormatsBlock.RareItems;
    private CheckBox ConvertTracksEnabled => ConvertAdvancedBlock.TracksEnabledCheckBox;
    private CheckBox ConvertDiskDefsEnabled => ConvertAdvancedBlock.DiskDefinitionsEnabled;
    private TextBox ConvertDiskDefsValue => ConvertAdvancedBlock.DiskDefinitionsValue;
    private TextBox ReadFolder => ReadFolderBlock.Input;
    private TextBox ReadFileName => ReadFileNameBlock.FileNameTextBox;
    private TextBox ReadExtensionText => ReadFileNameBlock.ExtensionTextBox;
    private TextBox CommandPreview => TerminalBlock?.CommandTextBox!;
    private TextBox LogOutput => TerminalBlock?.OutputTextBox!;
    private TerminalSection ConsolePanel => TerminalBlock;
    private TextBlock ScpFileName => VisualizerHeader.FileNameText;
    private TextBlock ScpSummary => VisualizerHeader.SummaryText;
    private ComboBox ScpDecoderCombo => VisualizerHeader.DecoderCombo;
    private CheckBox LinkScpViews => VisualizerHeader.LinkZoomCheckBox;
    private readonly ISettingsStore _settingsStore;
    private readonly IGreaseweazleRunner _runner;
    private readonly IGwCommandBuilder _commandBuilder;
    private readonly IGwInstallationManager _hostTools;
    private readonly IHardwareRegistry _hardwareRegistry;
    private readonly StartupHardwareMonitor _startupHardwareMonitor;
    private AppSettings _settings = new();
    private readonly OperationCoordinator _operation = new();
    private readonly OperationResultPresenter _operationResultPresenter = new();
    private readonly IMessageDialogService _dialogs;
    private readonly IFileDialogService _fileDialogs;
    private readonly IBusinessDialogService _businessDialogs;
    private readonly IWindowNavigationService _navigation;
    private IImageFormatCatalog _formatCatalog = null!;
    private IProfileStore<OperationProfile> _readProfiles = new InMemoryProfileStore(OperationKind.Read);
    private IProfileStore<OperationProfile> _writeProfiles = new InMemoryProfileStore(OperationKind.Write);
    private IProfileStore<OperationProfile> _convertProfiles = new InMemoryProfileStore(OperationKind.Convert);
    private ImageFormatDetector _formatDetector;
    private DetectedImageFormat? _detectedWriteFormat;
    private readonly ConversionFormatPresenter _conversionFormatPresenter = new();
    private string? _conversionSourceExtension;
    private DetectedImageFormat? _conversionSourceDetection;
    private ScpImage? _scpImage;
    private bool _syncingScpZoom;
    private readonly FluxDecoderRegistry _fluxDecoders = new();
    private readonly ScpInspectorPresenter _scpInspector;
    private readonly ScpDocumentLoader _scpLoader;
    private readonly DiskImageExplorer _diskImageExplorer = DiskImageExplorer.CreateDefault();
    private string? _lastScpPath;
    private ScpTrack? _selectedScpTrack;
    private readonly GwProgressTracker _progressTracker = new();
    private bool _trackProgressNeedsConfiguration;
    private readonly string _logsDirectory;
    private readonly ConsoleLogSession _consoleLog;
    private readonly MainWindowViewModel _viewModel;
    private GwFormatCapabilities _gwCapabilities = GwFormatCapabilities.Unknown;
    private bool _settingsSaveInProgress;
    private bool _closeAfterSettingsSave;
    private readonly bool _settingsProvidedAtStartup;
    private CancellationTokenSource? _scpCancellation;
    private CancellationTokenSource? _scpInspectorCancellation;
    private CancellationTokenSource? _explorerCancellation;
    private ScpInspectorWindow? _detachedScpInspector;
    private readonly Stopwatch _operationStopwatch = new();
    private readonly System.Windows.Threading.DispatcherTimer _operationTimer = new() { Interval = TimeSpan.FromSeconds(1) };

    public MainWindow() : this(null, null, null, null, null, null, null, null, null, null) { }

    public MainWindow(IMessageDialogService? dialogs, IFileDialogService? fileDialogs = null, IBusinessDialogService? businessDialogs = null, IWindowNavigationService? navigation = null, IGwCommandBuilder? commandBuilder = null, IGwInstallationManager? hostTools = null, IGreaseweazleRunner? runner = null, ISettingsStore? settingsStore = null, IHardwareRegistry? hardwareRegistry = null, AppSettings? initialSettings = null)
    {
        InitializeComponent();
        _settingsProvidedAtStartup = initialSettings is not null;
        _settings = initialSettings ?? new AppSettings();
        if (_settingsProvidedAtStartup) RestoreWindowPlacement();
        ConnectMainMenu();
        ConnectReadComponents();
        ConnectWriteComponents();
        ConnectConvertComponents();
        ConnectExplorerComponent();
        _dialogs = dialogs ?? new WpfMessageDialogService(this);
        _fileDialogs = fileDialogs ?? new WpfFileDialogService(this);
        _businessDialogs = businessDialogs ?? new WpfBusinessDialogService(this);
        _commandBuilder = commandBuilder ?? new GwCommandBuilder();
        _hostTools = hostTools ?? new GwInstallationManager(new HttpClient(), StoragePaths.HostToolsDirectory);
        var directory = StoragePaths.DataDirectory;
        _logsDirectory = StoragePaths.LogsDirectory;
        _consoleLog = new ConsoleLogSession(_logsDirectory, () => _settings.Logging);
        _runner = runner ?? new GreaseweazleRunner();
        _hardwareRegistry = hardwareRegistry ?? new GreaseweazleHardwareRegistry(new WindowsSerialDeviceDiscovery(), _runner, _commandBuilder);
        _navigation = navigation ?? new WpfWindowNavigationService(this, _hostTools, _runner, _commandBuilder);
        _viewModel = new MainWindowViewModel(LocExtension.Get("Hardware.NotConfigured"), LocExtension.Get("Status.ReadyShort"));
        _operationTimer.Tick += (_, _) => UpdateElapsedTime();
        DataContext = _viewModel;
        _formatCatalog = new BuiltInImageFormatCatalog(key => LocExtension.Get(key));
        _scpInspector = new ScpInspectorPresenter(_fluxDecoders, (key, arguments) => LocExtension.Get(key, arguments));
        _scpLoader = new ScpDocumentLoader(new ScpReader(), (key, arguments) => LocExtension.Get(key, arguments));
        ScpSide0.TrackSelected += ScpTrack_Selected; ScpSide1.TrackSelected += ScpTrack_Selected;
        ScpSide0.ZoomChanged += ScpZoom_Changed; ScpSide1.ZoomChanged += ScpZoom_Changed;
        VisualizerHeader.DecoderCombo.SelectionChanged += ScpDecoder_Changed;
        VisualizerHeader.ResetButton.Click += ResetScpViews_Click;
        VisualizerHeader.OpenButton.Click += OpenScp_Click;
        ScpInspector.CloseRequested += (_, _) => ScpInspector.Visibility = Visibility.Collapsed;
        ScpInspector.DetachRequested += (_, _) => DetachScpInspector();
        ScpInspector.DragRequested += (_, delta) => MoveScpInspector(delta.X, delta.Y);
        _formatDetector = new ImageFormatDetector(_formatCatalog);
        _settingsStore = settingsStore ?? new JsonSettingsStore(Path.Combine(directory, "settings.json"));
        _startupHardwareMonitor = new StartupHardwareMonitor(_hardwareRegistry, _settingsStore);
    }

    private void ConnectMainMenu()
    {
        ApplicationMenu.PreferencesRequested += Preferences_Click;
        ApplicationMenu.LogHistoryRequested += LogHistory_Click;
        ApplicationMenu.DocumentationRequested += Documentation_Click;
        ApplicationMenu.AboutRequested += About_Click;
        ApplicationMenu.ToolRequested += (sender, verb) => ToolCommand_Click(sender, new RoutedEventArgs());

        RegisterName("OptionsMenuItem", ApplicationMenu.OptionsMenuItem);
        RegisterName("HelpMenuItem", ApplicationMenu.HelpMenuItem);
        RegisterName("AlignMenuItem", ApplicationMenu.AlignMenuItem);
    }

    private void ConnectReadComponents()
    {
        RawScpRadio.Checked += ReadMode_Changed;
        KnownFormatRadio.Checked += ReadMode_Changed;
        ReadFamilyCombo.SelectionChanged += ReadFamily_Changed;
        ReadFormatCombo.SelectionChanged += ReadFormat_Changed;
        ReadExtensionCombo.SelectionChanged += ReadInput_Changed;
        ReadProfileCombo.SelectionChanged += ReadProfile_Changed;
        ReadProfileBlock.SaveButton.Click += SaveReadProfile_Click;
        ReadProfileBlock.ResetButton.Click += ResetReadProfile_Click;
        ReadFolderBlock.BrowseButton.Click += BrowseReadFolder_Click;
        ReadFileName.TextChanged += ReadInput_Changed;
        TerminalBlock.CopyButton.Click += CopyConsole_Click;

        RegisterName(nameof(RawScpRadio), RawScpRadio);
        RegisterName(nameof(KnownFormatRadio), KnownFormatRadio);
        RegisterName(nameof(KnownFormatPanel), KnownFormatPanel);
        RegisterName(nameof(ReadFamilyCombo), ReadFamilyCombo);
        RegisterName(nameof(ReadFormatCombo), ReadFormatCombo);
        RegisterName(nameof(ReadExtensionCombo), ReadExtensionCombo);
        RegisterName(nameof(ReadProfileCombo), ReadProfileCombo);
        RegisterName(nameof(ReadFolder), ReadFolder);
        RegisterName(nameof(ReadFileName), ReadFileName);
        RegisterName(nameof(ReadExtensionText), ReadExtensionText);
        RegisterName(nameof(CommandPreview), CommandPreview!);
        RegisterName(nameof(LogOutput), LogOutput!);
    }

    private void ConnectWriteComponents()
    {
        WriteProfileCombo.SelectionChanged += WriteProfile_Changed;
        WriteProfileBlock.SaveButton.Click += SaveWriteProfile_Click;
        WriteProfileBlock.ResetButton.Click += ResetWriteProfile_Click;
        WriteSourceBlock.BrowseButton.Click += BrowseWriteSource_Click;
        WriteFormatBlock.ModifyButton.Click += ToggleWriteFormat_Click;
        WriteFormatBlock.VisualizeTracksButton.Click += VisualizeWriteSource_Click;
        WriteFormatCombo.SelectionChanged += WriteInput_Changed;
        WriteAdvancedBlock.InputChanged += WriteInput_Changed;
        WriteAdvancedBlock.FakeIndexChecked += WriteFakeIndex_Checked;
        WriteAdvancedBlock.HardSectorsChecked += WriteHardSectors_Checked;
        WriteAdvancedBlock.DenselChecked += WriteDensel_Checked;
        WriteAdvancedBlock.Tg43Checked += WriteTg43_Checked;
        WriteAdvancedBlock.BrowseDiskDefinitionsRequested += BrowseWriteDiskDefs_Click;
        RegisterName(nameof(WriteProfileCombo), WriteProfileCombo);
        RegisterName(nameof(WriteSourceText), WriteSourceText);
        RegisterName(nameof(WriteDetectionText), WriteDetectionText);
        RegisterName(nameof(WriteFormatCombo), WriteFormatCombo);
        RegisterName(nameof(WriteNoVerify), WriteNoVerify);
        RegisterName(nameof(WriteDiskDefsEnabled), WriteDiskDefsEnabled);
        RegisterName(nameof(WriteDiskDefsValue), WriteDiskDefsValue);
    }

    private void ConnectConvertComponents()
    {
        ConvertProfileCombo.SelectionChanged += ConvertProfile_Changed;
        ConvertProfileBlock.SaveButton.Click += SaveConvertProfile_Click;
        ConvertProfileBlock.ResetButton.Click += ResetConvertProfile_Click;
        ConvertSourceBlock.BrowseButton.Click += BrowseConvertSource_Click;
        ConvertSourceBlock.ActionButton.Click += VisualizeConvertSource_Click;
        ConvertOutputBlock.ValueChanged += ConvertInput_Changed;
        ConvertFormatsBlock.ValueChanged += ConversionSelectionChanged;
        ConvertAdvancedBlock.InputChanged += ConvertInput_Changed;
        ConvertAdvancedBlock.BrowseDiskDefinitionsRequested += BrowseConvertDiskDefs_Click;
        RegisterName(nameof(ConvertProfileCombo), ConvertProfileCombo);
        RegisterName(nameof(ConvertSourceText), ConvertSourceText);
        RegisterName(nameof(ConvertOutputName), ConvertOutputName);
        RegisterName(nameof(ConvertTags), ConvertTags);
        RegisterName(nameof(ConvertSourceInfo), ConvertSourceInfo);
        RegisterName(nameof(ConvertPinnedPanel), ConvertPinnedPanel);
        RegisterName(nameof(ConvertCommonPanel), ConvertCommonPanel);
        RegisterName(nameof(ConvertRarePanel), ConvertRarePanel);
        RegisterName(nameof(ConvertTracksEnabled), ConvertTracksEnabled);
        RegisterName(nameof(ConvertDiskDefsEnabled), ConvertDiskDefsEnabled);
        RegisterName(nameof(ConvertDiskDefsValue), ConvertDiskDefsValue);
    }

    private void ConnectExplorerComponent()
    {
        DiskExplorer.OpenRequested += OpenExplorerImage_Click;
        DiskExplorer.ReadDiskRequested += ReadDiskIntoExplorer_Click;
        DiskExplorer.FormatChanged += async (_, _) =>
        {
            if (!string.IsNullOrWhiteSpace(_explorerPath)) await LoadExplorerImageAsync(_explorerPath);
        };
    }

    private string? _explorerPath;

    private async void OpenExplorerImage_Click(object? sender, RoutedEventArgs e)
    {
        var path = _fileDialogs.OpenFile(new(LocExtension.Get("Common.DiskImageFilter"), ReadFolder.Text));
        if (path is not null) await LoadExplorerImageAsync(path);
    }

    private async void ReadDiskIntoExplorer_Click(object? sender, RoutedEventArgs e)
    {
        if (_operation.IsRunning) { ConfirmAndRequestStop(); return; }
        if (!EnsureSelectedHardwareAvailable()) return;
        if (string.IsNullOrWhiteSpace(_settings.GwExecutablePath) || !File.Exists(_settings.GwExecutablePath))
        {
            _dialogs.Show(LocExtension.Get("App.GwNotConfigured"), LocExtension.Get("App.Title"), icon: UserDialogIcon.Information);
            return;
        }

        var temporaryDirectory = Path.Combine(Path.GetTempPath(), "GW GUI");
        Directory.CreateDirectory(temporaryDirectory);
        var temporaryPath = Path.Combine(temporaryDirectory, $"explorer-{Guid.NewGuid():N}.scp");
        var command = _commandBuilder.BuildRead(new ReadRequest(_settings.GwExecutablePath, temporaryPath, ReadResultKind.RawScp, null, [], SelectedDeviceArgument(), SelectedDriveArgument()));
        try
        {
            DiskExplorer.SetReadDiskRunning(true);
            BeginProgress();
            await RenderPendingProgressAsync();
            LogOutput.Clear();
            await _consoleLog.BeginAsync("read", command.ToDisplayString());
            var outcome = await _operation.RunAsync(token => _runner.RunAsync(command, new Progress<GwOutputLine>(ReportOutput), token));
            await FlushPendingOutputAsync();
            ApplyOperationResult(_operationResultPresenter.Present(outcome));
            if (outcome.Result?.IsSuccess == true && File.Exists(temporaryPath)) await LoadExplorerImageAsync(temporaryPath);
        }
        catch (Exception exception)
        {
            _dialogs.Show(LocExtension.Get("Explorer.LoadFailed", exception.Message), LocExtension.Get("Tab.Explorer"), icon: UserDialogIcon.Error);
        }
        finally
        {
            EndProgress();
            DiskExplorer.SetReadDiskRunning(false);
            try { if (File.Exists(temporaryPath)) File.Delete(temporaryPath); } catch { }
        }
    }

    private async Task LoadExplorerImageAsync(string path)
    {
        _explorerCancellation?.Cancel();
        _explorerCancellation?.Dispose();
        var cancellation = _explorerCancellation = new CancellationTokenSource();
        _explorerPath = path;
        DiskExplorer.Clear(path);
        DiskExplorer.SetLoading(true);
        try
        {
            var document = await _diskImageExplorer.ExploreAsync(path, DiskExplorer.SelectedFormatId, cancellation.Token);
            if (!cancellation.IsCancellationRequested) DiskExplorer.Display(document);
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
        catch (Exception exception)
        {
            _dialogs.Show(LocExtension.Get("Explorer.LoadFailed", exception.Message), LocExtension.Get("Tab.Explorer"), icon: UserDialogIcon.Error);
        }
        finally
        {
            if (ReferenceEquals(_explorerCancellation, cancellation)) DiskExplorer.SetLoading(false);
        }
    }

    private async void OpenScp_Click(object sender, RoutedEventArgs e)
    {
        var path = _fileDialogs.OpenFile(new(LocExtension.Get("Common.DiskImageFilter"), ReadFolder.Text));
        if (path is null) return;
        await OpenDiskImageAsync(path);
    }

    private async Task OpenDiskImageAsync(string path)
    {
        if (Path.GetExtension(path).Equals(".scp", StringComparison.OrdinalIgnoreCase)) { await LoadScpAsync(path); return; }
        if (_operation.IsRunning) return;
        if (string.IsNullOrWhiteSpace(_settings.GwExecutablePath) || !File.Exists(_settings.GwExecutablePath)) { _dialogs.Show(LocExtension.Get("App.GwNotConfigured"), LocExtension.Get("App.Title")); return; }
        var detection = _formatDetector.Detect(path, new FileInfo(path).Length);
        if (detection.Format is not { } format) { _dialogs.Show(LocExtension.Get("Write.VisualizeFormatRequired"), LocExtension.Get("Visual.Title")); return; }
        var temporaryPath = Path.Combine(Path.GetTempPath(), $"gwgui-visual-{Guid.NewGuid():N}.scp");
        try
        {
            var command = _commandBuilder.BuildConversion(_settings.GwExecutablePath, path, new ConversionOutput(format.Id, ".scp", temporaryPath, false));
            BeginProgress(); await RenderPendingProgressAsync(); LogOutput.Clear(); await _consoleLog.BeginAsync("convert", command.ToDisplayString());
            var outcome = await _operation.RunAsync(token => _runner.RunAsync(command, new Progress<GwOutputLine>(ReportOutput), token));
            await FlushPendingOutputAsync(); ApplyOperationResult(_operationResultPresenter.Present(outcome)); EndProgress();
            if (outcome.Result?.IsSuccess != true || !File.Exists(temporaryPath)) return;
            await LoadScpAsync(temporaryPath, Path.GetFileName(path));
        }
        finally { try { if (File.Exists(temporaryPath)) File.Delete(temporaryPath); } catch { } }
    }

    private async void OpenLastScp_Click(object sender, RoutedEventArgs e)
    {
        if (_lastScpPath is null) return; MainTabs.SelectedIndex = 3; await LoadScpAsync(_lastScpPath);
    }

    private async void ExploreLastScp_Click(object sender, RoutedEventArgs e)
    {
        if (_lastScpPath is null) return;
        MainTabs.SelectedIndex = 4;
        await LoadExplorerImageAsync(_lastScpPath);
    }

    private async Task LoadScpAsync(string path, string? displayFileName = null)
    {
        var cancellation = ReplaceScpCancellation();
        try
        {
            ShowScpProgress(LocExtension.Get("Visual.Loading"), 0, true);
            ScpSummary.Text = LocExtension.Get("Visual.Loading");
            var document = await _scpLoader.LoadAsync(path, cancellation.Token); _scpImage = document.Image;
            ScpFileName.Text = displayFileName ?? document.FileName;
            var heads = document.Heads;
            ScpSummary.Text = document.Summary;
            ScpSide0.SetImage(_scpImage, 0); ScpSide1.SetImage(_scpImage, 1);
            _selectedScpTrack = null;
            ScpSide0.Visibility = heads.Contains(0) ? Visibility.Visible : Visibility.Collapsed; ScpSide1.Visibility = heads.Contains(1) ? Visibility.Visible : Visibility.Collapsed;
            Grid.SetColumn(ScpSide0, 0); Grid.SetColumnSpan(ScpSide0, heads.Count == 1 && heads.Contains(0) ? 2 : 1);
            Grid.SetColumn(ScpSide1, heads.Count == 1 && heads.Contains(1) ? 0 : 1); Grid.SetColumnSpan(ScpSide1, heads.Count == 1 && heads.Contains(1) ? 2 : 1);
            ScpInspector.DataContext = null;
            if (_detachedScpInspector is not null) _detachedScpInspector.DataContext = null;
            await PrepareScpViewsAsync(cancellation.Token);
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
        catch (Exception exception) { _scpImage = null; ScpSummary.Text = LocExtension.Get("Visual.Invalid"); _dialogs.Show(exception.Message, LocExtension.Get("Visual.Title"), icon: UserDialogIcon.Error); }
        finally { if (ReferenceEquals(_scpCancellation, cancellation)) HideScpProgress(); }
    }

    private void ScpTrack_Selected(object? sender, ScpTrack? track)
    {
        _selectedScpTrack = track;
        UpdateScpInspector();
    }

    private async void ScpDecoder_Changed(object sender, SelectionChangedEventArgs e)
    {
        var decoderId = (ScpDecoderCombo.SelectedItem as ScpDecoderChoice)?.Id;
        ScpSide0?.SetDecoder(decoderId); ScpSide1?.SetDecoder(decoderId);
        if (_scpImage is null) { UpdateScpInspector(); return; }
        var cancellation = ReplaceScpCancellation();
        try { await PrepareScpViewsAsync(cancellation.Token); UpdateScpInspector(); }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
        finally { if (ReferenceEquals(_scpCancellation, cancellation)) HideScpProgress(); }
    }

    private CancellationTokenSource ReplaceScpCancellation()
    {
        _scpCancellation?.Cancel();
        _scpCancellation?.Dispose();
        return _scpCancellation = new CancellationTokenSource();
    }

    private async Task PrepareScpViewsAsync(CancellationToken cancellationToken)
    {
        if (_scpImage is null) return;
        var heads = _scpImage.Tracks.Select(track => track.Head).Distinct().Order().ToArray();
        var total = Math.Max(1, _scpImage.Tracks.Count);
        var completedByHead = heads.ToDictionary(head => head, _ => 0);
        var cylindersByHead = heads.ToDictionary(head => head, head => (IReadOnlyList<int>)_scpImage.Tracks.Where(track => track.Head == head).OrderBy(track => track.Cylinder).Select(track => track.Cylinder).ToArray());
        VisualizerTrackOverview.Configure(cylindersByHead);
        Face0TrackProgress.Configure(0, cylindersByHead.GetValueOrDefault(0) ?? [], LocExtension.Get("Visual.Side", 0));
        Face1TrackProgress.Configure(1, cylindersByHead.GetValueOrDefault(1) ?? [], LocExtension.Get("Visual.Side", 1));
        _viewModel.ProgressVisibility = Visibility.Visible;
        _viewModel.GlobalProgressVisibility = Visibility.Collapsed;
        _viewModel.Face0ProgressVisibility = heads.Contains(0) ? Visibility.Visible : Visibility.Collapsed;
        _viewModel.Face1ProgressVisibility = heads.Contains(1) ? Visibility.Visible : Visibility.Collapsed;
        _viewModel.ProgressText = LocExtension.Get("Visual.AnalysingTrack", 0, total);
        var preparations = heads.Select(head =>
        {
            var view = head == 0 ? ScpSide0 : ScpSide1;
            var strip = head == 0 ? Face0TrackProgress : Face1TrackProgress;
            var cylinders = cylindersByHead[head];
            var progress = new Progress<ScpTrackPreparation>(preparation =>
            {
                if (cancellationToken.IsCancellationRequested) return;
                var value = ++completedByHead[head];
                var current = Math.Min(total, completedByHead.Values.Sum());
                for (var index = 0; index < Math.Min(value, cylinders.Count); index++) strip.SetState(cylinders[index], TrackSegmentState.Success);
                if (value < cylinders.Count) strip.SetActive(cylinders[value]); else strip.ClearActive();
                VisualizerTrackOverview.MarkPrepared(preparation);
                _viewModel.ProgressText = LocExtension.Get("Visual.AnalysingTrack", current, total);
                view.RefreshPreparedTracks();
            });
            return view.PrepareAsync(progress, cancellationToken);
        });
        await Task.WhenAll(preparations);
    }

    private void ShowScpProgress(string text, double value, bool indeterminate)
    {
        _viewModel.ProgressText = text;
        _viewModel.ProgressValue = value;
        _viewModel.ProgressIndeterminate = indeterminate;
        _viewModel.ProgressVisibility = Visibility.Visible;
        _viewModel.GlobalProgressVisibility = Visibility.Visible;
        _viewModel.Face0ProgressVisibility = Visibility.Collapsed;
        _viewModel.Face1ProgressVisibility = Visibility.Collapsed;
        _viewModel.Face0ProgressValue = 0;
        _viewModel.Face1ProgressValue = 0;
        Face0TrackProgress.Reset();
        Face1TrackProgress.Reset();
    }

    private void HideScpProgress()
    {
        if (_operation.IsRunning) return;
        _viewModel.ProgressVisibility = Visibility.Collapsed;
        _viewModel.ProgressIndeterminate = false;
    }

    private void UpdateScpInspector()
    {
        var track = _selectedScpTrack;
        if (track is null || _scpImage is null) return;
        _ = UpdateScpInspectorAsync(track);
    }

    private async Task UpdateScpInspectorAsync(ScpTrack track)
    {
        var image = _scpImage;
        if (image is null) return;
        _scpInspectorCancellation?.Cancel();
        _scpInspectorCancellation?.Dispose();
        var cancellation = _scpInspectorCancellation = new CancellationTokenSource();
        var decoderId = (ScpDecoderCombo.SelectedItem as ScpDecoderChoice)?.Id;
        try
        {
            var model = await Task.Run(() => _scpInspector.BuildModel(image, track, decoderId), cancellation.Token);
            if (!cancellation.IsCancellationRequested && ReferenceEquals(_scpInspectorCancellation, cancellation))
            {
                ScpInspector.DataContext = model;
                ScpInspector.Visibility = _detachedScpInspector is null ? Visibility.Visible : Visibility.Collapsed;
                if (_detachedScpInspector is null) PositionScpInspector();
                if (_detachedScpInspector is not null) _detachedScpInspector.DataContext = model;
            }
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
    }

    private void ScpZoom_Changed(object? sender, float zoom)
    {
        if (_syncingScpZoom || LinkScpViews.IsChecked != true) return;
        _syncingScpZoom = true; try { (ReferenceEquals(sender, ScpSide0) ? ScpSide1 : ScpSide0).SetZoom(zoom); } finally { _syncingScpZoom = false; }
    }

    private void ResetScpViews_Click(object sender, RoutedEventArgs e) { ScpSide0.ResetView(); ScpSide1.ResetView(); }
    private void ToggleScpInspector_Click(object sender, RoutedEventArgs e) { if (_detachedScpInspector is not null) { _detachedScpInspector.Activate(); return; } ScpInspector.Visibility = ScpInspector.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible; }
    private void MoveScpInspector(double x, double y)
    {
        var left = Math.Clamp(Canvas.GetLeft(ScpInspector) + x, 0, Math.Max(0, ScpInspectorLayer.ActualWidth - ScpInspector.ActualWidth));
        var top = Math.Clamp(Canvas.GetTop(ScpInspector) + y, 0, Math.Max(0, ScpInspectorLayer.ActualHeight - ScpInspector.ActualHeight));
        Canvas.SetLeft(ScpInspector, left); Canvas.SetTop(ScpInspector, top);
    }
    private void PositionScpInspector()
    {
        ScpInspector.Width = Math.Max(320, Math.Min(390, ScpInspectorLayer.ActualWidth - 12));
        ScpInspector.Height = Math.Max(280, Math.Min(410, ScpInspectorLayer.ActualHeight - 12));
        var currentLeft = Canvas.GetLeft(ScpInspector);
        var currentTop = Canvas.GetTop(ScpInspector);
        var left = double.IsNaN(currentLeft) ? Math.Max(12, ScpInspectorLayer.ActualWidth - ScpInspector.Width - 20) : Math.Min(currentLeft, Math.Max(0, ScpInspectorLayer.ActualWidth - ScpInspector.Width));
        var top = double.IsNaN(currentTop) ? 18 : Math.Min(currentTop, Math.Max(0, ScpInspectorLayer.ActualHeight - ScpInspector.Height));
        Canvas.SetLeft(ScpInspector, left); Canvas.SetTop(ScpInspector, top);
    }
    private void DetachScpInspector()
    {
        if (_detachedScpInspector is not null) return;
        ScpInspector.Visibility = Visibility.Collapsed;
        var window = _detachedScpInspector = new ScpInspectorWindow { Owner = this, DataContext = ScpInspector.DataContext };
        window.AttachRequested += (_, _) => ScpInspector.Visibility = Visibility.Visible;
        window.Closed += (_, _) => _detachedScpInspector = null;
        window.Show();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (!_settingsProvidedAtStartup) _settings = await _settingsStore.LoadAsync();
        if (!string.IsNullOrWhiteSpace(_settings.GwExecutablePath))
            _gwCapabilities = await new GwFormatCapabilityReader().ReadAsync(_settings.GwExecutablePath);
        LoadConfiguredDiskDefs();
        RebuildFormatCatalog();
        ScpDecoderCombo.ItemsSource = new[] { new ScpDecoderChoice(null, LocExtension.Get("Visual.Automatic")) }.Concat(_fluxDecoders.Decoders.Select(x => new ScpDecoderChoice(x.Id, DecoderName(x.Id)))).ToArray();
        ScpDecoderCombo.SelectedIndex = 0;
        LoadProfileStores();
        if (!_settingsProvidedAtStartup)
        {
            RestoreWindowPlacement();
            ConstrainToCurrentWorkArea();
        }
        _viewModel.Read.Folder = _settings.DefaultImagesFolder;
        ReadFamilyCombo.ItemsSource = _formatCatalog.Formats.Where(x => x.Family != "Raw").Select(x => x.Family).Distinct().Order().ToArray();
        ReadFamilyCombo.SelectedIndex = 0;
        RefreshReadProfiles();
        RefreshWriteProfiles();
        RefreshConvertProfiles();
        RestoreReadSettings();
        RestoreWriteSettings();
        RestoreConversionSettings();
        BuildConversionFormats(null);
        RefreshHardwareSelector();
        await VerifyConfiguredHardwareAsync();
        SetConsoleVisibility(_settings.ConsoleExpanded);
        UpdateReadCommand();
        UpdateProfileStatus();
        _ = CheckHostToolsUpdateAsync();
    }

    private async Task VerifyConfiguredHardwareAsync()
    {
        if (_settings.Controllers.Count == 0 || string.IsNullOrWhiteSpace(_settings.GwExecutablePath) || !File.Exists(_settings.GwExecutablePath))
            return;

        while (true)
        {
            StartupHardwareCheckResult check;
            try { check = await _startupHardwareMonitor.CheckAsync(_settings); }
            catch (Exception exception)
            {
                foreach (var controller in _settings.Controllers) controller.IsAvailable = false;
                RefreshHardwareSelector();
                await _settingsStore.SaveAsync(_settings);
                _dialogs.Show(LocExtension.Get("Hardware.StartupCheckFailed", exception.Message), LocExtension.Get("Hardware.StartupTitle"), icon: UserDialogIcon.Warning);
                check = new(true, _settings.Controllers.ToArray(), []);
            }
            if (!check.Performed) return;
            RefreshHardwareSelector();
            foreach (var controller in check.NewControllers)
            {
                var configure = _dialogs.Show(
                    LocExtension.Get("Hardware.NewDetected", controller.Model, controller.LastPort),
                    LocExtension.Get("Hardware.NewDetectedTitle"), UserDialogButtons.YesNo, UserDialogIcon.Question) == UserDialogResult.Yes;
                if (configure)
                {
                    _settings.UnconfiguredControllers.Add(controller);
                    await _settingsStore.SaveAsync(_settings);
                    _navigation.ShowOptions(_settings, OptionsSection.Hardware);
                }
                else
                {
                    _settings.UnconfiguredControllers.Add(controller);
                }
                await _settingsStore.SaveAsync(_settings);
                RefreshHardwareSelector();
            }
            var missing = check.MissingControllers;
            if (missing.Count == 0) return;

            switch (_businessDialogs.ResolveMissingHardware(missing))
            {
                case MissingHardwareChoice.Retry:
                    continue;
                case MissingHardwareChoice.OpenSettings:
                    CaptureProfiles();
                    if (_navigation.ShowOptions(_settings))
                    {
                        LoadProfileStores();
                        RefreshReadProfiles(); RefreshWriteProfiles(); RefreshConvertProfiles();
                        _viewModel.Read.Folder = _settings.DefaultImagesFolder;
                        RefreshHardwareSelector();
                        ((App)Application.Current).SetTheme(_settings.Theme);
                        await _settingsStore.SaveAsync(_settings);
                    }
                    return;
                default:
                    return;
            }
        }
    }

    private void MainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MainTabs?.SelectedIndex == 1) UpdateWriteCommand();
        else if (MainTabs?.SelectedIndex == 0) UpdateReadCommand();
        else if (MainTabs?.SelectedIndex == 2) UpdateConvertCommand();
        else if (MainTabs?.SelectedIndex == 5) UpdateToolCommand();
        UpdateProfileStatus();
    }

    private void RefreshWriteProfiles(string? selectedId = null)
    {
        var items = LocalizedProfiles(OperationKind.Write);
        WriteProfileCombo.ItemsSource = items;
        WriteProfileCombo.SelectedItem = items.FirstOrDefault(x => x.Id == selectedId) ?? items[0];
    }

    private void RefreshConvertProfiles(string? selectedId = null)
    {
        var items = LocalizedProfiles(OperationKind.Convert);
        ConvertProfileCombo.ItemsSource = items;
        ConvertProfileCombo.SelectedItem = items.FirstOrDefault(x => x.Id == selectedId) ?? items[0];
    }

    private void BrowseWriteSource_Click(object sender, RoutedEventArgs e)
    {
        var path = _fileDialogs.OpenFile(new(LocExtension.Get("Common.DiskImageFilter"), ReadFolder.Text));
        if (path is null) return;
        _viewModel.Write.SourcePath = path;
        _detectedWriteFormat = _formatDetector.Detect(path, new FileInfo(path).Length);
        WriteDetectionText.Text = $"{_detectedWriteFormat.Format?.DisplayName ?? LocExtension.Get("Detection.Ambiguous")} — {LocExtension.Get(_detectedWriteFormat.ExplanationKey)}";
        WriteFormatCombo.ItemsSource = _detectedWriteFormat.Candidates.Count > 0 ? _detectedWriteFormat.Candidates : _formatCatalog.Formats;
        WriteFormatCombo.SelectedItem = _detectedWriteFormat.Format;
        WriteFormatCombo.Visibility = _detectedWriteFormat.RequiresUserChoice ? Visibility.Visible : Visibility.Collapsed;
        WriteFormatBlock.VisualizeTracksButton.IsEnabled = true;
        UpdateWriteCommand();
    }

    private async void VisualizeWriteSource_Click(object sender, RoutedEventArgs e)
    {
        var source = _viewModel.Write.SourcePath;
        if (string.IsNullOrWhiteSpace(source) || !File.Exists(source)) return;
        if (Path.GetExtension(source).Equals(".scp", StringComparison.OrdinalIgnoreCase))
        {
            MainTabs.SelectedIndex = 3;
            await LoadScpAsync(source);
            return;
        }
        if (_operation.IsRunning) return;
        if (string.IsNullOrWhiteSpace(_settings.GwExecutablePath) || !File.Exists(_settings.GwExecutablePath))
        {
            _dialogs.Show(LocExtension.Get("App.GwNotConfigured"), LocExtension.Get("App.Title"));
            return;
        }

        var format = (WriteFormatCombo.SelectedItem as DiskFormat) ?? _detectedWriteFormat?.Format;
        if (format is null)
        {
            _dialogs.Show(LocExtension.Get("Write.VisualizeFormatRequired"), LocExtension.Get("Write.Title"));
            return;
        }

        var temporaryPath = Path.Combine(Path.GetTempPath(), $"gwgui-write-{Guid.NewGuid():N}.scp");
        try
        {
            var output = new ConversionOutput(format.Id, ".scp", temporaryPath, false);
            var command = _commandBuilder.BuildConversion(_settings.GwExecutablePath, source, output);
            BeginProgress();
            await RenderPendingProgressAsync();
            LogOutput.Clear();
            await _consoleLog.BeginAsync("convert", command.ToDisplayString());
            var outcome = await _operation.RunAsync(token => _runner.RunAsync(command, new Progress<GwOutputLine>(ReportOutput), token));
            await FlushPendingOutputAsync();
            ApplyOperationResult(_operationResultPresenter.Present(outcome));
            EndProgress();
            if (outcome.Result?.IsSuccess != true || !File.Exists(temporaryPath)) return;
            MainTabs.SelectedIndex = 3;
            await LoadScpAsync(temporaryPath);
        }
        finally
        {
            try { if (File.Exists(temporaryPath)) File.Delete(temporaryPath); } catch { }
        }
    }

    private void ToggleWriteFormat_Click(object sender, RoutedEventArgs e)
    {
        if (WriteFormatCombo.ItemsSource is null) WriteFormatCombo.ItemsSource = _formatCatalog.Formats;
        WriteFormatCombo.Visibility = WriteFormatCombo.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
    }

    private void WriteInput_Changed(object sender, RoutedEventArgs e) => UpdateWriteCommand();

    private GwCommand BuildWriteCommand()
    {
        return _commandBuilder.BuildWrite(new WriteRequest(_settings.GwExecutablePath ?? "gw.exe", _viewModel.Write.SourcePath,
            (WriteFormatCombo?.SelectedItem as DiskFormat)?.Id ?? _detectedWriteFormat?.Format?.Id, _viewModel.Write.BuildOptions(),
            _viewModel.Write.DisableVerification, SelectedDeviceArgument(), SelectedDriveArgument(), _viewModel.Write.ExpertArguments));
    }

    private void UpdateWriteCommand()
    {
        if (CommandPreview is null || WriteSourceText is null || MainTabs?.SelectedIndex != 1) return;
        try { CommandPreview.Text = BuildWriteCommand().ToDisplayString(); }
        catch (ArgumentException exception) { CommandPreview.Text = $"⚠ {exception.Message}"; }
    }

    private async void ExecuteWrite_Click(object sender, RoutedEventArgs e)
    {
        if (_operation.IsRunning) { ConfirmAndRequestStop(); return; }
        if (!EnsureSelectedHardwareAvailable()) return;
        if (!ValidateDiskDefs(WriteDiskDefsEnabled, WriteDiskDefsValue, LocExtension.Get("Write.Title"))) return;
        if (!File.Exists(WriteSourceText.Text)) { _dialogs.Show(LocExtension.Get("Write.SelectSource"), LocExtension.Get("Write.Title"), icon: UserDialogIcon.Information); return; }
        var selected = WriteFormatCombo.SelectedItem as DiskFormat ?? _detectedWriteFormat?.Format;
        if (selected is null || (_detectedWriteFormat?.RequiresUserChoice == true && WriteFormatCombo.SelectedItem is null))
        { _dialogs.Show(LocExtension.Get("Write.Ambiguous"), LocExtension.Get("Write.Title"), icon: UserDialogIcon.Warning); WriteFormatCombo.Visibility = Visibility.Visible; return; }
        if (string.IsNullOrWhiteSpace(_settings.GwExecutablePath) || !File.Exists(_settings.GwExecutablePath)) { _dialogs.Show(LocExtension.Get("App.GwNotConfigured"), LocExtension.Get("App.Title"), icon: UserDialogIcon.Information); return; }
        GwCommand command;
        try { command = BuildWriteCommand(); }
        catch (ArgumentException exception) { ShowAdvancedValidation(exception, LocExtension.Get("Write.Title")); return; }
        var warning = LocExtension.Get(_viewModel.Write.DisableVerification ? "Write.VerifyOff" : "Write.VerifyOn");
        var confirmation = LocExtension.Get("Write.Confirm", Path.GetFileName(WriteSourceText.Text), selected.DisplayName, SelectedHardware()?.Label ?? LocExtension.Get("Hardware.NotConfigured"), warning);
        if (_dialogs.Show(confirmation, LocExtension.Get("Write.ConfirmTitle"), UserDialogButtons.OkCancel, UserDialogIcon.Warning) != UserDialogResult.Ok) return;
        WriteExecuteButton.Content = LocExtension.Get("Common.Stop"); BeginProgress(); await RenderPendingProgressAsync(); LogOutput.Clear(); await _consoleLog.BeginAsync("write", command.ToDisplayString());
        var output = new Progress<GwOutputLine>(ReportOutput);
        var outcome = await _operation.RunAsync(token => _runner.RunAsync(command, output, token));
        await FlushPendingOutputAsync();
        ApplyOperationResult(_operationResultPresenter.Present(outcome));
        EndProgress(); WriteExecuteButton.Content = LocExtension.Get("Common.Execute");
    }

    private void WriteProfile_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (WriteProfileCombo.SelectedItem is not OperationProfile profile || WriteNoVerify is null) return;
        _viewModel.Write.ApplyOptions(profile.EnabledOptions, profile.Values);
        ApplyWriteProfileFormat(profile);
        UpdateWriteCommand();
        UpdateProfileStatus();
    }

    private void ApplyWriteProfileFormat(OperationProfile profile)
    {
        if (profile.Values.TryGetValue("format", out var formatId) && _formatCatalog.Formats.FirstOrDefault(format => format.Id == formatId) is { } format)
        {
            WriteFormatCombo.ItemsSource = _formatCatalog.Formats.Where(item => item.Family != "Raw").ToArray();
            WriteFormatCombo.SelectedItem = format;
            WriteFormatCombo.Visibility = Visibility.Visible;
            return;
        }
        if (_detectedWriteFormat is not null)
        {
            WriteFormatCombo.ItemsSource = _detectedWriteFormat.Candidates.Count > 0 ? _detectedWriteFormat.Candidates : _formatCatalog.Formats;
            WriteFormatCombo.SelectedItem = _detectedWriteFormat.Format;
            WriteFormatCombo.Visibility = _detectedWriteFormat.RequiresUserChoice ? Visibility.Visible : Visibility.Collapsed;
        }
        else
        {
            WriteFormatCombo.SelectedItem = null;
            WriteFormatCombo.Visibility = Visibility.Collapsed;
        }
    }

    private void ResetWriteProfile_Click(object sender, RoutedEventArgs e) { if (WriteProfileCombo.SelectedItem is OperationProfile profile) { WriteProfileCombo.SelectedItem = null; WriteProfileCombo.SelectedItem = profile; } }

    private void SaveWriteProfile_Click(object sender, RoutedEventArgs e)
    {
        var profileName = _businessDialogs.PromptProfileName(); if (profileName is null) return;
        var enabled = _viewModel.Write.CaptureEnabledOptions();
        var values = _viewModel.Write.CaptureValues();
        if (WriteFormatCombo.SelectedItem is DiskFormat format) values["format"] = format.Id;
        var profile = new OperationProfile(Guid.NewGuid().ToString("N"), OperationKind.Write, profileName, values, enabled);
        try { profile = _writeProfiles.Save(profile); } catch (InvalidOperationException) { if (_dialogs.Show(LocExtension.Get("Profile.Replace"), LocExtension.Get("Profile.Title"), UserDialogButtons.YesNo) != UserDialogResult.Yes) return; profile = _writeProfiles.Save(profile, true); }
        RefreshWriteProfiles(profile.Id);
    }

    private void BuildConversionFormats(string? sourceExtension, DetectedImageFormat? detection = null)
    {
        if (ConvertCommonPanel is null) return;
        _conversionSourceExtension = sourceExtension;
        _conversionSourceDetection = detection;
        var items = _conversionFormatPresenter.Build(_formatCatalog, sourceExtension, detection, _viewModel.Conversion.SelectedFormats, _viewModel.Conversion.ExplicitExtensions);
        foreach (var item in items)
        {
            if (!item.IsCompatible && _viewModel.Conversion.SelectedFormats.Contains(item.Format.Id))
                _viewModel.Conversion.SetFormat(item.Format.Id, false, item.ExplicitExtensions);
        }
        ConvertPinnedPanel.ItemsSource = items.Where(item => item.Group == ConversionFormatGroup.Selected);
        ConvertCommonPanel.ItemsSource = items.Where(item => item.Group == ConversionFormatGroup.Common);
        ConvertRarePanel.ItemsSource = items.Where(item => item.Group == ConversionFormatGroup.Rare);
    }

    private void ConvertProfile_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (ConvertProfileCombo.SelectedItem is not OperationProfile profile || ConvertTracksEnabled is null) return;
        ApplyConvertProfile(profile);
        UpdateProfileStatus();
    }

    private void ApplyConvertProfile(OperationProfile profile)
    {
        _viewModel.Conversion.ApplyProfile(profile.EnabledOptions, profile.Values);
        BuildConversionFormats(_conversionSourceExtension, _conversionSourceDetection);
        UpdateConvertCommand();
    }

    private void ResetConvertProfile_Click(object sender, RoutedEventArgs e) { if (ConvertProfileCombo.SelectedItem is OperationProfile profile) ApplyConvertProfile(profile); }

    private void SaveConvertProfile_Click(object sender, RoutedEventArgs e)
    {
        var profileName = _businessDialogs.PromptProfileName(); if (profileName is null) return;
        var enabled = _viewModel.Conversion.CaptureProfileEnabled();
        var values = _viewModel.Conversion.CaptureProfileValues();
        var profile = new OperationProfile(Guid.NewGuid().ToString("N"), OperationKind.Convert, profileName, values, enabled);
        try { profile = _convertProfiles.Save(profile); } catch (InvalidOperationException) { if (_dialogs.Show(LocExtension.Get("Profile.Replace"), LocExtension.Get("Profile.Title"), UserDialogButtons.YesNo) != UserDialogResult.Yes) return; profile = _convertProfiles.Save(profile, true); }
        RefreshConvertProfiles(profile.Id);
    }

    private void ConversionSelectionChanged(object? sender, EventArgs e)
    {
        if (sender is not ConversionFormatControl control) return;
        _viewModel.Conversion.SetFormat(control.Format.Id, control.IsSelected, control.ExplicitExtensions);
        BuildConversionFormats(_conversionSourceExtension, _conversionSourceDetection);
        UpdateConvertCommand();
    }

    private void BrowseConvertSource_Click(object sender, RoutedEventArgs e)
    {
        var path = _fileDialogs.OpenFile(new(LocExtension.Get("Common.DiskImageFilter"), ReadFolder.Text));
        if (path is null) return;
        _viewModel.Conversion.SourcePath = path; _viewModel.Conversion.OutputName = Path.GetFileNameWithoutExtension(path);
        var detection = _formatDetector.Detect(path, new FileInfo(path).Length);
        ConvertSourceInfo.Text = detection.Format?.DisplayName ?? LocExtension.Get("Conversion.SourceAmbiguous");
        ConvertSourceBlock.ActionButton.Visibility = Path.GetExtension(path).Equals(".scp", StringComparison.OrdinalIgnoreCase)
            ? Visibility.Visible
            : Visibility.Collapsed;
        BuildConversionFormats(Path.GetExtension(path), detection); UpdateConvertCommand();
    }

    private async void VisualizeConvertSource_Click(object sender, RoutedEventArgs e)
    {
        var source = _viewModel.Conversion.SourcePath;
        if (string.IsNullOrWhiteSpace(source) || !File.Exists(source) ||
            !Path.GetExtension(source).Equals(".scp", StringComparison.OrdinalIgnoreCase)) return;
        MainTabs.SelectedIndex = 3;
        await LoadScpAsync(source);
    }

    private void ConvertInput_Changed(object sender, RoutedEventArgs e) => UpdateConvertCommand();

    private IReadOnlyList<ConversionOutput> PlanConversions()
    {
        if (string.IsNullOrWhiteSpace(_viewModel.Conversion.SourcePath)) return [];
        return new ConversionPlanner(_formatCatalog).Plan(_viewModel.Conversion.SourcePath, ReadFolder.Text, _viewModel.Conversion.OutputName.Trim(), _viewModel.Conversion.BuildSelections(_formatCatalog.Formats), _viewModel.Conversion.AddTags, _settings.Conversion.TagPattern);
    }

    private EnabledOption[] GetConvertOptions()
    {
        return _viewModel.Conversion.BuildOptions().ToArray();
    }

    private void UpdateConvertCommand()
    {
        if (CommandPreview is null || ConvertSourceText is null || MainTabs?.SelectedIndex != 2) return;
        try
        {
            var outputs = PlanConversions();
            if (outputs.Count == 0) { CommandPreview.Text = LocExtension.Get("Conversion.SelectOutput"); return; }
            var first = _commandBuilder.BuildConversion(_settings.GwExecutablePath ?? "gw.exe", _viewModel.Conversion.SourcePath, outputs[0], GetConvertOptions(), _viewModel.Conversion.ExpertArguments);
            CommandPreview.Text = first.ToDisplayString() + (outputs.Count > 1 ? LocExtension.Get("Conversion.More", outputs.Count - 1) : "");
        }
        catch (Exception exception) { CommandPreview.Text = $"⚠ {exception.Message}"; }
    }

    private async void ExecuteConvert_Click(object sender, RoutedEventArgs e)
    {
        if (_operation.IsRunning) { ConfirmAndRequestStop(); return; }
        if (!ValidateDiskDefs(ConvertDiskDefsEnabled, ConvertDiskDefsValue, LocExtension.Get("Conversion.Title"))) return;
        if (!File.Exists(ConvertSourceText.Text)) { _dialogs.Show(LocExtension.Get("Conversion.SourceRequired"), LocExtension.Get("Conversion.Title")); return; }
        if (string.IsNullOrWhiteSpace(ConvertOutputName.Text)) { _dialogs.Show(LocExtension.Get("Conversion.NameRequired"), LocExtension.Get("Conversion.Title")); return; }
        if (string.IsNullOrWhiteSpace(_settings.GwExecutablePath) || !File.Exists(_settings.GwExecutablePath)) { _dialogs.Show(LocExtension.Get("App.GwNotConfigured"), LocExtension.Get("App.Title")); return; }
        IReadOnlyList<ConversionOutput> outputs;
        try { outputs = PlanConversions(); GwOptionValidator.Validate(GetConvertOptions()); } catch (Exception exception) { ShowAdvancedValidation(exception, LocExtension.Get("Conversion.Title")); return; }
        if (outputs.Count == 0) { _dialogs.Show(LocExtension.Get("Conversion.CheckOutput"), LocExtension.Get("Conversion.Title")); return; }
        var existing = outputs.Where(x => File.Exists(x.OutputPath)).ToArray();
        if (existing.Length > 0)
        {
            var decisions = _businessDialogs.ResolveConversionConflicts(existing); if (decisions is null) return;
            outputs = ConversionConflictResolver.Apply(outputs, existing, decisions, NumberedPath);
        }
        ConvertExecuteButton.Content = LocExtension.Get("Common.Stop"); BeginProgress(); await RenderPendingProgressAsync(); LogOutput.Clear(); await _consoleLog.BeginAsync("convert", CommandPreview.Text);
        var progress = new Progress<GwOutputLine>(ReportOutput);
        var outcome = await _operation.RunAsync(token =>
        {
            var items = outputs.Select(planned => new GwBatchItem(Path.GetFileName(planned.OutputPath), _commandBuilder.BuildConversion(_settings.GwExecutablePath, _viewModel.Conversion.SourcePath, planned, GetConvertOptions(), _viewModel.Conversion.ExpertArguments))).ToArray();
            return new GwBatchExecutor(_runner).RunAsync(items, progress, item => Dispatcher.Invoke(() =>
            {
                BeginProgress();
                AppendConsoleText($"{Environment.NewLine}→ {item.Label}{Environment.NewLine}");
            }, System.Windows.Threading.DispatcherPriority.ContextIdle), token);
        });
        await FlushPendingOutputAsync();
        ApplyOperationResult(_operationResultPresenter.Present(outcome));
        EndProgress(); ConvertExecuteButton.Content = LocExtension.Get("Common.Execute");
    }

    private static string NumberedPath(string path)
    {
        var folder = Path.GetDirectoryName(path)!; var name = Path.GetFileNameWithoutExtension(path); var extension = Path.GetExtension(path);
        for (var number = 1; number < int.MaxValue; number++) { var candidate = Path.Combine(folder, $"{name} ({number}){extension}"); if (!File.Exists(candidate)) return candidate; }
        throw new IOException(LocExtension.Get("Conversion.NoAvailableOutputName"));
    }

    private void CaptureConversionSettings()
    {
        _settings.Conversion.AddTags = _viewModel.Conversion.AddTags;
        _settings.Conversion.SelectedFormats = _viewModel.Conversion.SelectedFormats.ToHashSet();
        _settings.Conversion.ExplicitExtensions = _viewModel.Conversion.ExplicitExtensions.ToDictionary(x => x.Key, x => x.Value.ToHashSet());
        _settings.Conversion.EnabledOptions = _viewModel.Conversion.CaptureEnabledOptions();
        _settings.Conversion.OptionValues = _viewModel.Conversion.CaptureValues();
    }

    private void RestoreConversionSettings()
    {
        _viewModel.Conversion.ApplySettings(_settings.Conversion.AddTags, _settings.Conversion.SelectedFormats, _settings.Conversion.ExplicitExtensions, _settings.Conversion.EnabledOptions, _settings.Conversion.OptionValues);
    }

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        _scpCancellation?.Cancel();
        _scpInspectorCancellation?.Cancel();
        if (_closeAfterSettingsSave) return;
        e.Cancel = true;
        if (_settingsSaveInProgress) return;

        if (_operation.IsRunning)
        {
            var answer = _dialogs.Show(LocExtension.Get("App.OperationRunningClose"), LocExtension.Get("App.Title"), UserDialogButtons.YesNo, UserDialogIcon.Warning);
            if (answer != UserDialogResult.Yes) return;
            _operation.RequestCancellation();
        }

        CaptureWindowSettings();
        CaptureReadSettings();
        CaptureWriteSettings();
        CaptureProfiles();
        CaptureConversionSettings();
        _settingsSaveInProgress = true;
        _ = SaveSettingsAndCloseAsync();
    }

    private async Task SaveSettingsAndCloseAsync()
    {
        Exception? failure = null;
        try
        {
            await _settingsStore.SaveAsync(_settings).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            failure = exception;
        }

        if (Dispatcher.HasShutdownStarted || Dispatcher.HasShutdownFinished) return;
        try
        {
            await Dispatcher.InvokeAsync(() =>
            {
                if (failure is not null)
                    _dialogs.Show(LocExtension.Get("App.SettingsSaveFailed", failure.Message), LocExtension.Get("App.Title"), icon: UserDialogIcon.Warning);
                _settingsSaveInProgress = false;
                _closeAfterSettingsSave = true;
                Close();
            }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }
        catch (TaskCanceledException) when (Dispatcher.HasShutdownStarted || Dispatcher.HasShutdownFinished) { }
    }

    private void RefreshReadProfiles(string? selectedId = null)
    {
        var items = LocalizedProfiles(OperationKind.Read);
        ReadProfileCombo.ItemsSource = items;
        ReadProfileCombo.SelectedItem = items.FirstOrDefault(x => x.Id == selectedId) ?? items[0];
    }

    private IReadOnlyList<OperationProfile> LocalizedProfiles(OperationKind operation) =>
        ProfileStore(operation).GetAll().Select(profile => profile.IsSystem ? profile with { Name = LocExtension.Get("Profile.Default") } : profile).ToArray();

    private void ReadProfile_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (ReadProfileCombo.SelectedItem is not OperationProfile profile || ReadRevsEnabled is null) return;
        ApplyReadProfile(profile);
        UpdateProfileStatus();
    }

    private void ResetReadProfile_Click(object sender, RoutedEventArgs e)
    {
        if (ReadProfileCombo.SelectedItem is OperationProfile profile) ApplyReadProfile(profile);
    }

    private void ApplyReadProfile(OperationProfile profile)
    {
        _viewModel.Read.ApplyOptions(profile.EnabledOptions, profile.Values);
        if (profile.IsSystem)
        {
            RawScpRadio.IsChecked = true;
        }
        else
        {
            if (profile.Values.GetValueOrDefault("result") == "raw") RawScpRadio.IsChecked = true;
            else if (profile.Values.GetValueOrDefault("result") == "known") KnownFormatRadio.IsChecked = true;
            SelectReadFormat(profile.Values.GetValueOrDefault("format"), profile.Values.GetValueOrDefault("extension"));
            if (profile.Values.TryGetValue("folder", out var folder) && !string.IsNullOrWhiteSpace(folder)) _viewModel.Read.Folder = folder;
        }
        UpdateReadCommand();
    }

    private void SaveReadProfile_Click(object sender, RoutedEventArgs e)
    {
        var profileName = _businessDialogs.PromptProfileName();
        if (profileName is null) return;
        var enabled = _viewModel.Read.CaptureEnabledOptions();
        var values = _viewModel.Read.CaptureValues();
        values["result"] = RawScpRadio.IsChecked == true ? "raw" : "known";
        if (ReadFormatCombo.SelectedItem is DiskFormat format) values["format"] = format.Id;
        if (ReadExtensionCombo.SelectedItem is ImageExtension extension) values["extension"] = extension.Extension;
        if (!string.IsNullOrWhiteSpace(_viewModel.Read.Folder)) values["folder"] = _viewModel.Read.Folder;
        var profile = new OperationProfile(Guid.NewGuid().ToString("N"), OperationKind.Read, profileName, values, enabled);
        try { profile = _readProfiles.Save(profile); }
        catch (InvalidOperationException)
        {
            if (_dialogs.Show(LocExtension.Get("Profile.Replace"), LocExtension.Get("Profile.Title"), UserDialogButtons.YesNo, UserDialogIcon.Question) != UserDialogResult.Yes) return;
            profile = _readProfiles.Save(profile, true);
        }
        RefreshReadProfiles(profile.Id);
    }

    private void CaptureProfiles()
    {
        _settings.Profiles = Enum.GetValues<OperationKind>().SelectMany(operation => ProfileStore(operation).GetAll()).Where(x => !x.IsSystem)
            .Select(x => new ProfileSettings { Id = x.Id, Operation = x.Operation.ToString(), Name = x.Name, Values = x.Values.ToDictionary(), EnabledOptions = x.EnabledOptions.ToHashSet() }).ToList();
    }

    private IProfileStore<OperationProfile> ProfileStore(OperationKind operation) => operation switch
    {
        OperationKind.Read => _readProfiles,
        OperationKind.Write => _writeProfiles,
        OperationKind.Convert => _convertProfiles,
        _ => throw new ArgumentOutOfRangeException(nameof(operation))
    };

    private void LoadProfileStores()
    {
        var profiles = _settings.Profiles.Select(ToProfile).ToArray();
        _readProfiles = new InMemoryProfileStore(OperationKind.Read, profiles.Where(profile => profile.Operation == OperationKind.Read));
        _writeProfiles = new InMemoryProfileStore(OperationKind.Write, profiles.Where(profile => profile.Operation == OperationKind.Write));
        _convertProfiles = new InMemoryProfileStore(OperationKind.Convert, profiles.Where(profile => profile.Operation == OperationKind.Convert));
    }

    private static OperationProfile ToProfile(ProfileSettings value) => new(value.Id, Enum.TryParse<OperationKind>(value.Operation, out var operation) ? operation : OperationKind.Read, value.Name, value.Values, value.EnabledOptions);

    private void ToggleConsole_Click(object sender, RoutedEventArgs e) => SetConsoleVisibility(ConsolePanel.Visibility != Visibility.Visible);

    private void LogHistory_Click(object sender, RoutedEventArgs e) => _navigation.ShowLogHistory(_logsDirectory);
    private void About_Click(object sender, RoutedEventArgs e) => _navigation.ShowAbout();
    private void Documentation_Click(object sender, RoutedEventArgs e)
    {
        var language = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "fr" ? "fr" : "en";
        var path = Path.Combine(AppContext.BaseDirectory, "Documentation", $"user-guide.{language}.md");
        if (File.Exists(path)) Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
    }

    private async void ExportConsole_Click(object sender, RoutedEventArgs e)
    {
        var path = _fileDialogs.SaveFile(new(LocExtension.Get("Logs.ExportFilter"), $"gw-gui-{DateTime.Now:yyyyMMdd-HHmmss}.txt", ".txt"));
        if (path is null) return;
        await File.WriteAllTextAsync(path, CommandPreview.Text + Environment.NewLine + Environment.NewLine + LogOutput.Text);
    }

    private void CopyConsole_Click(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(CommandPreview.Text + Environment.NewLine + Environment.NewLine + LogOutput.Text);
    }

    private void SetConsoleVisibility(bool visible)
    {
        if (!visible && ConsolePanel.Visibility == Visibility.Visible && ConsoleRow.ActualHeight >= 100)
            _settings.ConsoleHeight = ConsoleRow.ActualHeight;
        ConsolePanel.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        ConsoleSplitter.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        ConsoleRow.Height = visible ? new GridLength(Math.Max(100, _settings.ConsoleHeight)) : new GridLength(0);
    }

    private void ReadInput_Changed(object sender, RoutedEventArgs e) => UpdateReadCommand();

    private void ReadMode_Changed(object sender, RoutedEventArgs e)
    {
        if (KnownFormatPanel is null) return;
        KnownFormatPanel.Visibility = KnownFormatRadio.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        UpdateReadExtension();
        UpdateReadCommand();
    }

    private void ReadFamily_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (ReadFormatCombo is null || ReadFamilyCombo.SelectedItem is not string family) return;
        ReadFormatCombo.ItemsSource = _formatCatalog.Formats.Where(x => x.Family == family).ToArray();
        ReadFormatCombo.SelectedIndex = 0;
    }

    private void ReadFormat_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (ReadExtensionCombo is null) return;
        ReadExtensionCombo.ItemsSource = (ReadFormatCombo.SelectedItem as DiskFormat)?.Extensions;
        var extensions = ReadExtensionCombo.ItemsSource as IReadOnlyList<ImageExtension>;
        ReadExtensionCombo.SelectedIndex = extensions is null ? -1 : Math.Max(0, extensions.ToList().FindIndex(x => x.IsDefault));
        UpdateReadExtension();
        UpdateReadCommand();
    }

    private void UpdateReadExtension()
    {
        if (ReadExtensionText is null) return;
        ReadExtensionText.Text = RawScpRadio?.IsChecked == true ? LocExtension.Get("Read.RawScp") : (ReadExtensionCombo?.SelectedItem as ImageExtension)?.DisplayName ?? LocExtension.Get("Read.ChooseType");
    }

    private void UpdateReadCommand()
    {
        if (CommandPreview is null || ReadFileName is null || ReadFolder is null) return;
        var extension = GetReadExtension();
        var target = GetReadTarget(extension);
        if (ReadNamePreview is not null) ReadNamePreview.Text = Path.GetFileName(target);
        try { CommandPreview.Text = BuildReadCommand(target).ToDisplayString(); }
        catch (ArgumentException exception) { CommandPreview.Text = $"⚠ {exception.Message}"; }
    }

    private string GetReadTarget(string extension)
        => _viewModel.Read.BuildTarget(extension, "Exemple");

    private string GetReadExtension() => RawScpRadio?.IsChecked == true ? ".scp" : (ReadExtensionCombo?.SelectedItem as ImageExtension)?.Extension ?? "";

    private GwCommand BuildReadCommand(string target)
    {
        return _commandBuilder.BuildRead(new ReadRequest(
            _settings.GwExecutablePath ?? "gw.exe", target,
            RawScpRadio?.IsChecked == true ? ReadResultKind.RawScp : ReadResultKind.KnownFormat,
            (ReadFormatCombo?.SelectedItem as DiskFormat)?.Id, _viewModel.Read.BuildOptions(),
            SelectedDeviceArgument(), SelectedDriveArgument(), _viewModel.Read.ExpertArguments));
    }

    private static string SelectedText(ComboBox combo) => (combo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty;

    private void ReadFakeIndex_Checked(object sender, RoutedEventArgs e) { _viewModel.Read.EnableFakeIndex(); ReadInput_Changed(sender, e); }
    private void ReadSequenceKind_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (ReadSequenceValue is null) return;
        var targetKind = ReadSequenceKind.SelectedIndex == 1 ? SequenceKind.Alphabetic : SequenceKind.Numeric;
        var sourceKind = targetKind == SequenceKind.Alphabetic ? SequenceKind.Numeric : SequenceKind.Alphabetic;
        if (SequenceFormatter.TryParse(ReadSequenceValue.Text, sourceKind, out var value))
            _viewModel.Read.SequenceValue = targetKind == SequenceKind.Numeric
                ? (value + 1).ToString()
                : SequenceFormatter.Format(Math.Max(0, value - 1), targetKind, 1);
        UpdateReadCommand();
    }
    private void ReadHardSectors_Checked(object sender, RoutedEventArgs e) { _viewModel.Read.EnableHardSectors(); ReadInput_Changed(sender, e); }
    private void ReadDensel_Checked(object sender, RoutedEventArgs e) { _viewModel.Read.EnableDensel(); ReadInput_Changed(sender, e); }
    private void ReadTg43_Checked(object sender, RoutedEventArgs e) { _viewModel.Read.EnableTg43(); ReadInput_Changed(sender, e); }
    private void WriteFakeIndex_Checked(object sender, RoutedEventArgs e) { _viewModel.Write.EnableFakeIndex(); WriteInput_Changed(sender, e); }
    private void WriteHardSectors_Checked(object sender, RoutedEventArgs e) { _viewModel.Write.EnableHardSectors(); WriteInput_Changed(sender, e); }
    private void WriteDensel_Checked(object sender, RoutedEventArgs e) { _viewModel.Write.EnableDensel(); WriteInput_Changed(sender, e); }
    private void WriteTg43_Checked(object sender, RoutedEventArgs e) { _viewModel.Write.EnableTg43(); WriteInput_Changed(sender, e); }

    private void BrowseReadDiskDefs_Click(object sender, RoutedEventArgs e) => BrowseDiskDefs(ReadDiskDefsValue, ReadDiskDefsEnabled, UpdateReadCommand);
    private void BrowseWriteDiskDefs_Click(object sender, RoutedEventArgs e) => BrowseDiskDefs(WriteDiskDefsValue, WriteDiskDefsEnabled, UpdateWriteCommand);
    private void BrowseConvertDiskDefs_Click(object sender, RoutedEventArgs e) => BrowseDiskDefs(ConvertDiskDefsValue, ConvertDiskDefsEnabled, UpdateConvertCommand);

    private void BrowseDiskDefs(TextBox target, CheckBox enabled, Action refresh)
    {
        var path = _fileDialogs.OpenFile(new(LocExtension.Get("Advanced.DiskDefsFilter"), FileName: target.Text));
        if (path is null) return;
        try { AddDiskDefs(path); }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
        {
            ShowAdvancedValidation(exception, LocExtension.Get("Advanced.DiskDefs"));
            return;
        }
        if (ReferenceEquals(target, ReadDiskDefsValue)) { _viewModel.Read.DiskDefs.Value = path; _viewModel.Read.DiskDefs.Enabled = true; }
        else if (ReferenceEquals(target, WriteDiskDefsValue)) { _viewModel.Write.DiskDefs.Value = path; _viewModel.Write.DiskDefs.Enabled = true; }
        else if (ReferenceEquals(target, ConvertDiskDefsValue)) { _viewModel.Conversion.DiskDefs.Value = path; _viewModel.Conversion.DiskDefs.Enabled = true; }
        else { target.Text = path; enabled.IsChecked = true; }
        RefreshFormatSelectors();
        refresh();
    }

    private void LoadConfiguredDiskDefs()
    {
        var paths = new[]
        {
            _settings.Read.OptionValues.GetValueOrDefault("diskdefs"),
            _settings.Write.OptionValues.GetValueOrDefault("diskdefs"),
            _settings.Conversion.OptionValues.GetValueOrDefault("diskdefs")
        }.Concat(_settings.Profiles.Select(profile => profile.Values.GetValueOrDefault("diskdefs")));
        foreach (var path in paths.Where(path => !string.IsNullOrWhiteSpace(path) && File.Exists(path)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            try { AddDiskDefs(path!); }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException) { /* Validation is shown when the profile is executed. */ }
        }
    }

    private void AddDiskDefs(string path)
    {
        var discovered = DiskDefsFormatReader.Read(path);
        var ids = _gwCapabilities.FormatIds.Concat(discovered).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var extensions = _gwCapabilities.ImageExtensions.Count > 0
            ? _gwCapabilities.ImageExtensions
            : new HashSet<string>([".scp", ".img", ".ima", ".hfe"], StringComparer.OrdinalIgnoreCase);
        _gwCapabilities = new GwFormatCapabilities(ids, extensions);
        RebuildFormatCatalog();
    }

    private void RebuildFormatCatalog()
    {
        _formatCatalog = new CapabilityAwareImageFormatCatalog(new BuiltInImageFormatCatalog(key => LocExtension.Get(key)), _gwCapabilities);
        _formatDetector = new ImageFormatDetector(_formatCatalog);
    }

    private void RefreshFormatSelectors()
    {
        var selectedReadId = (ReadFormatCombo.SelectedItem as DiskFormat)?.Id;
        var selectedFamily = (ReadFormatCombo.SelectedItem as DiskFormat)?.Family ?? ReadFamilyCombo.SelectedItem as string;
        var families = _formatCatalog.Formats.Where(format => format.Family != "Raw").Select(format => format.Family).Distinct().Order().ToArray();
        ReadFamilyCombo.ItemsSource = families;
        ReadFamilyCombo.SelectedItem = selectedFamily is not null && families.Contains(selectedFamily) ? selectedFamily : families.FirstOrDefault();
        if (selectedReadId is not null)
            ReadFormatCombo.SelectedItem = _formatCatalog.Formats.FirstOrDefault(format => format.Id == selectedReadId);

        if (WriteFormatCombo.ItemsSource is not null)
        {
            var selectedWriteId = (WriteFormatCombo.SelectedItem as DiskFormat)?.Id;
            WriteFormatCombo.ItemsSource = _formatCatalog.Formats.Where(format => format.Family != "Raw").ToArray();
            WriteFormatCombo.SelectedItem = _formatCatalog.Formats.FirstOrDefault(format => format.Id == selectedWriteId);
        }

        DetectedImageFormat? detection = null;
        var sourceExtension = string.IsNullOrWhiteSpace(ConvertSourceText.Text) ? null : Path.GetExtension(ConvertSourceText.Text);
        if (File.Exists(ConvertSourceText.Text)) detection = _formatDetector.Detect(ConvertSourceText.Text, new FileInfo(ConvertSourceText.Text).Length);
        BuildConversionFormats(sourceExtension, detection);
    }

    private bool ValidateDiskDefs(CheckBox enabled, TextBox path, string title)
    {
        if (enabled.IsChecked != true || File.Exists(path.Text)) return true;
        _dialogs.Show(LocExtension.Get("Advanced.DiskDefsMissing"), title, icon: UserDialogIcon.Warning);
        return false;
    }

    private void ShowAdvancedValidation(Exception exception, string title) =>
        _dialogs.Show(LocExtension.Get("Advanced.Invalid", exception.Message), title, icon: UserDialogIcon.Warning);

    private void CopyReadName_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(ReadFileName.Text)) Clipboard.SetText(ReadFileName.Text);
    }

    private void BrowseReadFolder_Click(object sender, RoutedEventArgs e)
    {
        var path = _fileDialogs.SelectFolder(new(LocExtension.Get("Read.DestinationFolder"), ReadFolder.Text));
        if (path is not null) { _viewModel.Read.Folder = path; UpdateReadCommand(); }
    }

    private async void ExecuteRead_Click(object sender, RoutedEventArgs e)
    {
        if (_operation.IsRunning) { ConfirmAndRequestStop(); return; }
        if (!EnsureSelectedHardwareAvailable()) return;
        if (!ValidateDiskDefs(ReadDiskDefsEnabled, ReadDiskDefsValue, LocExtension.Get("Read.Title"))) return;
        if (string.IsNullOrWhiteSpace(ReadFileName.Text))
        {
            _dialogs.Show(LocExtension.Get("Read.NameRequired"), LocExtension.Get("Read.Title"), icon: UserDialogIcon.Information);
            return;
        }
        if (string.IsNullOrWhiteSpace(_settings.GwExecutablePath) || !File.Exists(_settings.GwExecutablePath))
        {
            _dialogs.Show(LocExtension.Get("App.GwNotConfigured"), LocExtension.Get("App.Title"), icon: UserDialogIcon.Information);
            return;
        }

        var extension = GetReadExtension();
        if (string.IsNullOrWhiteSpace(extension)) { _dialogs.Show(LocExtension.Get("Read.TypeRequired"), LocExtension.Get("Read.Title"), icon: UserDialogIcon.Information); return; }
        var target = GetReadTarget(extension);
        if (File.Exists(target))
        {
            var choice = _businessDialogs.ResolveReadConflict(target);
            if (choice is null or ReadConflictChoice.EditName) { ReadFileName.Focus(); ReadFileName.SelectAll(); return; }
            if (choice == ReadConflictChoice.UseNextNumber)
            {
                if (ReadAutoNumber.IsChecked != true) ReadAutoNumber.IsChecked = true;
                var kind = ReadSequenceKind.SelectedIndex == 1 ? SequenceKind.Alphabetic : SequenceKind.Numeric;
                if (!SequenceFormatter.TryParse(ReadSequenceValue.Text, kind, out var next)) next = kind == SequenceKind.Alphabetic ? 0 : 1;
                var available = OutputConflictResolver.FindNextAvailableWithValue(ReadFolder.Text, ReadFileName.Text.Trim(), extension, kind, ReadSequenceWidth.SelectedIndex + 1, next);
                target = available.Path;
                _viewModel.Read.SequenceValue = kind == SequenceKind.Numeric ? available.Value.ToString() : SequenceFormatter.Format(available.Value, kind, 1);
            }
        }
        GwCommand command;
        try { command = BuildReadCommand(target); }
        catch (ArgumentException exception) { ShowAdvancedValidation(exception, LocExtension.Get("Read.Title")); return; }
        ReadExecuteButton.Content = LocExtension.Get("Common.Stop");
        BeginProgress();
        await RenderPendingProgressAsync();
        LogOutput.Clear();
        await _consoleLog.BeginAsync("read", command.ToDisplayString());
        var output = new Progress<GwOutputLine>(ReportOutput);
        var outcome = await _operation.RunAsync(token => _runner.RunAsync(command, output, token));
        await FlushPendingOutputAsync();
        ApplyOperationResult(_operationResultPresenter.Present(outcome));
        if (outcome.Result is { } result)
        {
            if (result.WasCancelled)
            {
                var deletionError = CancelledOutputCleaner.TryDelete(target);
                if (deletionError is null) AppendConsoleText(Environment.NewLine + LocExtension.Get("Read.CancelledFileDeleted", target) + Environment.NewLine);
                else
                {
                    AppendConsoleText(Environment.NewLine + LocExtension.Get("Read.CancelledFileDeleteFailed", target, deletionError.Message) + Environment.NewLine);
                    _dialogs.Show(LocExtension.Get("Read.CancelledFileDeleteFailed", target, deletionError.Message), LocExtension.Get("Read.Title"), icon: UserDialogIcon.Warning);
                }
            }
            if (result.IsSuccess && extension.Equals(".scp", StringComparison.OrdinalIgnoreCase))
            {
                _lastScpPath = target;
                OpenScpBanner.Visibility = Visibility.Visible;
                await AppendScpCaptureSummaryAsync(target);
            }
            var sequenceKind = ReadSequenceKind.SelectedIndex == 1 ? SequenceKind.Alphabetic : SequenceKind.Numeric;
            if (result.IsSuccess) _viewModel.Read.TryAdvanceSequence();
        }
        EndProgress(); ReadExecuteButton.Content = LocExtension.Get("Common.Execute");
    }

    private void RestoreReadSettings()
    {
        KnownFormatRadio.IsChecked = _settings.Read.UseKnownFormat;
        RawScpRadio.IsChecked = !_settings.Read.UseKnownFormat;
        SelectReadFormat(_settings.Read.FormatId, _settings.Read.ImageExtension);
        _viewModel.Read.AutoNumber = _settings.Read.AutoNumber;
        _viewModel.Read.SequenceKindIndex = _settings.Read.SequenceKind == "Alphabetic" ? 1 : 0;
        _viewModel.Read.SequenceWidthIndex = Math.Clamp(_settings.Read.SequenceWidth - 1, 0, 2);
        _viewModel.Read.SequenceValue = _settings.Read.SequenceKind == "Alphabetic" ? SequenceFormatter.Format(_settings.Read.NextSequence, SequenceKind.Alphabetic, 1) : _settings.Read.NextSequence.ToString();
        _viewModel.Read.ApplyOptions(_settings.Read.EnabledOptions, _settings.Read.OptionValues);
    }

    private void SelectReadFormat(string? formatId, string? imageExtension)
    {
        var format = _formatCatalog.Formats.FirstOrDefault(item => item.Id == formatId);
        if (format is null) return;
        ReadFamilyCombo.SelectedItem = format.Family;
        ReadFormatCombo.SelectedItem = format;
        var extension = format.Extensions.FirstOrDefault(item => item.Extension.Equals(imageExtension, StringComparison.OrdinalIgnoreCase));
        if (extension is not null) ReadExtensionCombo.SelectedItem = extension;
    }

    private void CaptureReadSettings()
    {
        _settings.Read.UseKnownFormat = KnownFormatRadio.IsChecked == true;
        _settings.Read.FormatId = (ReadFormatCombo.SelectedItem as DiskFormat)?.Id;
        _settings.Read.ImageExtension = (ReadExtensionCombo.SelectedItem as ImageExtension)?.Extension;
        _settings.Read.AutoNumber = _viewModel.Read.AutoNumber;
        _settings.Read.SequenceKind = _viewModel.Read.SequenceKind == SequenceKind.Alphabetic ? "Alphabetic" : "Numeric";
        _settings.Read.SequenceWidth = _viewModel.Read.SequenceWidthIndex + 1;
        if (SequenceFormatter.TryParse(_viewModel.Read.SequenceValue, _viewModel.Read.SequenceKind, out var sequence)) _settings.Read.NextSequence = sequence;
        _settings.Read.EnabledOptions = _viewModel.Read.CaptureEnabledOptions();
        _settings.Read.OptionValues = _viewModel.Read.CaptureValues();
    }

    private void RestoreWriteSettings()
    {
        _viewModel.Write.ApplyOptions(_settings.Write.EnabledOptions, _settings.Write.OptionValues);
    }

    private void CaptureWriteSettings()
    {
        _settings.Write.EnabledOptions = _viewModel.Write.CaptureEnabledOptions();
        _settings.Write.OptionValues = _viewModel.Write.CaptureValues();
    }

    private async void Preferences_Click(object sender, RoutedEventArgs e)
    {
        CaptureProfiles();
        if (_navigation.ShowOptions(_settings))
        {
            CaptureReadSettings();
            CaptureWriteSettings();
            CaptureConversionSettings();
            CaptureWindowSettings();
            LoadProfileStores();
            RefreshReadProfiles(); RefreshWriteProfiles(); RefreshConvertProfiles();
            _viewModel.Read.Folder = _settings.DefaultImagesFolder;
            RefreshHardwareSelector();
            var app = (App)Application.Current;
            app.SetTheme(_settings.Theme);
            await _settingsStore.SaveAsync(_settings);
            UpdateReadCommand();
        }
    }

    internal void RefreshLocalizedContent()
    {
        var readProfile = (ReadProfileCombo.SelectedItem as OperationProfile)?.Id;
        var writeProfile = (WriteProfileCombo.SelectedItem as OperationProfile)?.Id;
        var convertProfile = (ConvertProfileCombo.SelectedItem as OperationProfile)?.Id;
        RebuildFormatCatalog();
        RefreshReadProfiles(readProfile);
        RefreshWriteProfiles(writeProfile);
        RefreshConvertProfiles(convertProfile);
        DiskExplorer.RefreshFormats(DiskExplorer.SelectedFormatId);
        RefreshHardwareSelector();
        UpdateReadExtension();
        UpdateProfileStatus();
        UpdateReadCommand();
        UpdateWriteCommand();
        UpdateConvertCommand();
        UpdateToolCommand();
        if (!_operation.IsRunning) SetOperationState("Status.ReadyShort", Color.FromRgb(136, 136, 136));
        ShowHostToolsUpdateIfNeeded();
    }

    private void CaptureWindowSettings()
    {
        _settings.Window.Width = RestoreBounds.Width;
        _settings.Window.Height = RestoreBounds.Height;
        _settings.Window.Left = RestoreBounds.Left;
        _settings.Window.Top = RestoreBounds.Top;
        _settings.Window.Maximized = WindowState == WindowState.Maximized;
        _settings.ConsoleExpanded = ConsolePanel.Visibility == Visibility.Visible;
        if (_settings.ConsoleExpanded) _settings.ConsoleHeight = ConsoleRow.ActualHeight;
    }

    private void RestoreWindowPlacement()
    {
        var placement = WindowPlacementPolicy.Normalize(_settings.Window, MinWidth, MinHeight,
            SystemParameters.VirtualScreenLeft, SystemParameters.VirtualScreenTop, SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight);
        Width = placement.Width;
        Height = placement.Height;
        if (placement.Left is double left && placement.Top is double top)
        {
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = left;
            Top = top;
        }
        if (_settings.Window.Maximized) WindowState = WindowState.Maximized;
    }

    private void ConstrainToCurrentWorkArea()
    {
        if (WindowState == WindowState.Maximized) return;
        var area = MonitorWorkArea.Get(this);
        var placement = WindowPlacementPolicy.ConstrainToWorkArea(new(Width, Height, Left, Top), area.Left, area.Top, area.Width, area.Height);
        Width = placement.Width;
        Height = placement.Height;
        Left = placement.Left!.Value;
        Top = placement.Top!.Value;
    }

    private void ToolsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ErasePanel is null) return;
        ErasePanel.Visibility = ToolsList.SelectedIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
        CleanPanel.Visibility = ToolsList.SelectedIndex == 1 ? Visibility.Visible : Visibility.Collapsed;
        UpdateToolCommand();
    }

    private void ToolInput_Changed(object sender, RoutedEventArgs e) => UpdateToolCommand();

    private GwCommand BuildEraseCommand()
    {
        var options = new List<EnabledOption>();
        if (EraseTracksEnabled.IsChecked == true) options.Add(new("--tracks", EraseTracksValue.Text.Trim()));
        if (EraseRevsEnabled.IsChecked == true) options.Add(new("--revs", EraseRevsValue.Text.Trim()));
        return _commandBuilder.BuildErase(new EraseRequest(_settings.GwExecutablePath ?? "gw.exe", options, SelectedDeviceArgument(), SelectedDriveArgument(), EraseExpertArguments.Text));
    }

    private GwCommand BuildCleanCommand() => _commandBuilder.BuildClean(new CleanRequest(_settings.GwExecutablePath ?? "gw.exe",
        CleanCylindersEnabled.IsChecked == true && int.TryParse(CleanCylindersValue.Text, out var cylinders) ? cylinders : null,
        CleanPassesEnabled.IsChecked == true && int.TryParse(CleanPassesValue.Text, out var passes) ? passes : null,
        CleanLingerEnabled.IsChecked == true && int.TryParse(CleanLingerValue.Text, out var linger) ? linger : null,
        SelectedDeviceArgument(), SelectedDriveArgument(), CleanExpertArguments.Text));

    private void RefreshHardwareSelector()
    {
        if (HardwareSelector is null) return;
        var previousId = (HardwareSelector.SelectedItem as HardwareChoice)?.Drive.Id;
        var choices = _settings.Drives.Select(drive =>
        {
            var controller = _settings.Controllers.FirstOrDefault(item => item.UsbId == drive.ControllerUsbId);
            if (controller is null) return null;
            var number = _settings.Drives.Where(item => item.ControllerUsbId == drive.ControllerUsbId).ToList().IndexOf(drive) + 1;
            var label = LocExtension.Get("Hardware.DriveChoice", number, drive.Size, drive.Density, controller.LastPort);
            return new HardwareChoice(drive, controller.LastPort, controller.IsAvailable,
                label + (controller.IsAvailable ? "" : $" ({LocExtension.Get("Hardware.Disconnected")})"));
        }).Where(choice => choice is not null).Cast<HardwareChoice>().ToArray();
        HardwareSelector.ItemsSource = choices;
        HardwareSelector.SelectedItem = choices.FirstOrDefault(x => x.Drive.Id == previousId) ?? choices.FirstOrDefault();
        var selectionRequired = choices.Length > 1;
        HardwareSelectorItem.Visibility = Visibility.Collapsed;
        HardwareSelector.Visibility = selectionRequired ? Visibility.Visible : Visibility.Collapsed;
        HardwareStatusText.Visibility = selectionRequired ? Visibility.Collapsed : Visibility.Visible;
        UpdateHardwareStatus();
    }

    private HardwareChoice? SelectedHardware() => HardwareSelector?.SelectedItem as HardwareChoice;
    private string? SelectedDeviceArgument() => HardwareRoutingPolicy.DeviceArgument(_settings.Controllers, _settings.Drives, SelectedHardware()?.Drive);
    private string? SelectedDriveArgument() => HardwareRoutingPolicy.DriveArgument(_settings.Drives, SelectedHardware()?.Drive);
    private void HardwareSelector_Changed(object sender, SelectionChangedEventArgs e) { UpdateHardwareStatus(); UpdateReadCommand(); UpdateWriteCommand(); UpdateToolCommand(); }
    private void UpdateHardwareStatus()
    {
        var selected = SelectedHardware();
        var enabled = selected is not { Available: false };
        _viewModel.HardwareText = selected is null ? LocExtension.Get("Hardware.NotConfigured") : selected.Label;
        _viewModel.HardwareBrush = new SolidColorBrush(selected?.Available == true ? Color.FromRgb(63, 171, 91) : Color.FromRgb(136, 136, 136));
        if (ReadExecuteButton is not null) ReadExecuteButton.IsEnabled = enabled;
        if (WriteExecuteButton is not null) WriteExecuteButton.IsEnabled = enabled;
        if (EraseExecuteButton is not null) EraseExecuteButton.IsEnabled = enabled;
        if (CleanExecuteButton is not null) CleanExecuteButton.IsEnabled = enabled;
    }

    private bool EnsureSelectedHardwareAvailable()
    {
        if (SelectedHardware() is not { Available: false }) return true;
        _dialogs.Show(LocExtension.Get("Hardware.SelectedDisconnected"), LocExtension.Get("Menu.Hardware"), icon: UserDialogIcon.Warning);
        return false;
    }

    private void UpdateToolCommand()
    {
        if (CommandPreview is null || ToolsList is null || MainTabs?.SelectedIndex != 5) return;
        try { CommandPreview.Text = (ToolsList.SelectedIndex == 0 ? BuildEraseCommand() : BuildCleanCommand()).ToDisplayString(); }
        catch (Exception exception) { CommandPreview.Text = $"⚠ {exception.Message}"; }
    }

    private async void ExecuteErase_Click(object sender, RoutedEventArgs e)
    {
        if (_operation.IsRunning) { ConfirmAndRequestStop(); return; }
        if (!EnsureSelectedHardwareAvailable()) return;
        if (_dialogs.Show(LocExtension.Get("Maintenance.EraseConfirm"), LocExtension.Get("Maintenance.EraseTitle"), UserDialogButtons.OkCancel, UserDialogIcon.Warning) != UserDialogResult.Ok) return;
        await ExecuteMaintenanceAsync(BuildEraseCommand(), EraseExecuteButton);
    }

    private async void ExecuteClean_Click(object sender, RoutedEventArgs e)
    {
        if (_operation.IsRunning) { ConfirmAndRequestStop(); return; }
        if (!EnsureSelectedHardwareAvailable()) return;
        if (_dialogs.Show(LocExtension.Get("Maintenance.CleanConfirm"), LocExtension.Get("Maintenance.CleanTitle"), UserDialogButtons.OkCancel, UserDialogIcon.Warning) != UserDialogResult.Ok) return;
        await ExecuteMaintenanceAsync(BuildCleanCommand(), CleanExecuteButton);
    }

    private async Task ExecuteMaintenanceAsync(GwCommand command, Button button)
    {
        if (_operation.IsRunning) { ConfirmAndRequestStop(); return; }
        if (!EnsureSelectedHardwareAvailable()) return;
        if (string.IsNullOrWhiteSpace(_settings.GwExecutablePath) || !File.Exists(_settings.GwExecutablePath)) { _dialogs.Show(LocExtension.Get("App.GwNotConfigured"), LocExtension.Get("App.Title")); return; }
        button.Content = LocExtension.Get("Common.Stop"); BeginProgress(); await RenderPendingProgressAsync(); LogOutput.Clear(); await _consoleLog.BeginAsync(command.Verb, command.ToDisplayString());
        var progress = new Progress<GwOutputLine>(ReportOutput);
        var outcome = await _operation.RunAsync(token => _runner.RunAsync(command, progress, token));
        await FlushPendingOutputAsync();
        ApplyOperationResult(_operationResultPresenter.Present(outcome));
        EndProgress(); button.Content = LocExtension.Get("Common.Execute");
    }

    private void ConfirmAndRequestStop()
    {
        if (_dialogs.Show(LocExtension.Get("Operation.StopConfirm"), LocExtension.Get("Operation.StopTitle"), UserDialogButtons.YesNo, UserDialogIcon.Warning) == UserDialogResult.Yes)
            _operation.RequestCancellation();
    }

    private void BeginProgress()
    {
        if (!_operationStopwatch.IsRunning)
        {
            _operationStopwatch.Restart();
            _operationTimer.Start();
            _viewModel.TimerVisibility = Visibility.Visible;
            UpdateElapsedTime();
        }
        _progressTracker.Reset();
        Face0TrackProgress.ResetToPending();
        Face1TrackProgress.ResetToPending();
        _trackProgressNeedsConfiguration = true;
        SetOperationState("Status.Running", Color.FromRgb(45, 125, 210));
        _viewModel.ProgressVisibility = Visibility.Visible;
        _viewModel.ProgressIndeterminate = true;
        _viewModel.ProgressValue = 0;
        _viewModel.ProgressText = "";
    }

    private async Task RenderPendingProgressAsync() =>
        await Dispatcher.InvokeAsync(static () => { }, System.Windows.Threading.DispatcherPriority.Render);

    private void ReportOutput(GwOutputLine line)
    {
        AppendConsoleText(line.Text + Environment.NewLine);
        var progress = _progressTracker.Accept(line.Text);
        if (progress is null) return;
        if (progress.TotalOnHead is int totalOnHead)
        {
            _viewModel.GlobalProgressVisibility = Visibility.Collapsed;
            _viewModel.Face0ProgressVisibility = progress.Head0Expected ? Visibility.Visible : Visibility.Collapsed;
            _viewModel.Face1ProgressVisibility = progress.Head1Expected ? Visibility.Visible : Visibility.Collapsed;
            if ((_trackProgressNeedsConfiguration || Face0TrackProgress.Segments.Count == 0) && progress.Head0Expected)
                Face0TrackProgress.Configure(0, progress.Cylinders, LocExtension.Get("Visual.Side", 0));
            if ((_trackProgressNeedsConfiguration || Face1TrackProgress.Segments.Count == 0) && progress.Head1Expected)
                Face1TrackProgress.Configure(1, progress.Cylinders, LocExtension.Get("Visual.Side", 1));
            _trackProgressNeedsConfiguration = false;
            var text = LocExtension.Get("Status.FaceTrackProgress", progress.Head, progress.Cylinder, progress.CompletedOnHead, totalOnHead);
            var segmentState = progress.State switch
            {
                GwTrackState.Retry => Controls.TrackSegmentState.Retry,
                GwTrackState.Failed => Controls.TrackSegmentState.Failed,
                _ => Controls.TrackSegmentState.Success
            };
            (progress.Head == 0 ? Face0TrackProgress : Face1TrackProgress).SetState(progress.Cylinder, segmentState);
            if (progress.State == GwTrackState.Retry)
            {
                Face0TrackProgress.ClearActive();
                Face1TrackProgress.ClearActive();
            }
            else if (progress.NextCylinder is int nextCylinder && progress.NextHead is int nextHead)
                (nextHead == 0 ? Face0TrackProgress : Face1TrackProgress).SetActive(nextCylinder);
            if (progress.Head == 0)
            {
                _viewModel.Face0ProgressValue = progress.HeadFraction.GetValueOrDefault() * 100;
                _viewModel.Face0ProgressText = text;
            }
            else
            {
                _viewModel.Face1ProgressValue = progress.HeadFraction.GetValueOrDefault() * 100;
                _viewModel.Face1ProgressText = text;
            }
            return;
        }
        if (progress.TotalTracks is int total)
        {
            _viewModel.ProgressIndeterminate = false;
            _viewModel.ProgressValue = progress.Fraction.GetValueOrDefault() * 100;
            _viewModel.ProgressText = LocExtension.Get("Status.TrackProgress", progress.Cylinder, progress.Head, progress.CompletedTracks, total);
        }
        else _viewModel.ProgressText = LocExtension.Get("Status.TrackUnknown", progress.Cylinder, progress.Head, progress.CompletedTracks);
    }

    private async Task FlushPendingOutputAsync() =>
        await Dispatcher.InvokeAsync(static () => { }, System.Windows.Threading.DispatcherPriority.ContextIdle);

    private async Task AppendScpCaptureSummaryAsync(string path)
    {
        try
        {
            var info = await ScpCaptureInfoReader.ReadAsync(path);
            var checksum = LocExtension.Get(info.ChecksumValid ? "Visual.ChecksumValid" : "Visual.ChecksumInvalid");
            AppendConsoleText(Environment.NewLine + LocExtension.Get("Read.ScpSummaryTitle") + Environment.NewLine);
            AppendConsoleText(LocExtension.Get("Read.ScpTracksSummary", info.CapturedTracks, info.MissingTracks, info.Cylinders, info.Sides) + Environment.NewLine);
            AppendConsoleText(LocExtension.Get("Read.ScpTechnicalSummary", info.Header.Revolutions, info.Header.ResolutionNanoseconds, info.FileSize, checksum) + Environment.NewLine);
            AppendConsoleText(LocExtension.Get("Read.ScpOutputFile", path) + Environment.NewLine);
            OpenScpSummaryText.Text = LocExtension.Get("Read.ScpBannerSummary", info.CapturedTracks, info.MissingTracks, info.Cylinders, info.Sides, info.Header.Revolutions, info.FileSize, checksum);
            LogOutput.ScrollToEnd();
        }
        catch (Exception exception)
        {
            OpenScpSummaryText.Text = LocExtension.Get("Read.ScpSummaryUnavailable", exception.Message);
            AppendConsoleText(Environment.NewLine + LocExtension.Get("Read.ScpSummaryUnavailable", exception.Message) + Environment.NewLine);
        }
    }

    private void EndProgress()
    {
        _operationStopwatch.Stop();
        _operationTimer.Stop();
        UpdateElapsedTime();
        _viewModel.TimerVisibility = Visibility.Collapsed;
        _viewModel.ProgressIndeterminate = false;
        _viewModel.ProgressValue = 100;
        _viewModel.ProgressVisibility = Visibility.Collapsed;
        _viewModel.GlobalProgressVisibility = Visibility.Visible;
        _viewModel.Face0ProgressVisibility = Visibility.Collapsed;
        _viewModel.Face1ProgressVisibility = Visibility.Collapsed;
    }

    private void UpdateElapsedTime()
    {
        var elapsed = _operationStopwatch.Elapsed;
        _viewModel.ElapsedText = $"{(int)elapsed.TotalHours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}";
    }

    private void ApplyOperationResult(OperationResultPresentation presentation)
    {
        switch (presentation.State)
        {
            case OperationResultState.Success: SetOperationState("Status.Success", Color.FromRgb(63, 171, 91)); break;
            case OperationResultState.Cancelled: SetOperationState("Status.Cancelled", Color.FromRgb(220, 148, 45)); break;
            default: SetOperationState("Status.Error", Color.FromRgb(210, 66, 66)); break;
        }
        foreach (var message in presentation.Messages)
        {
            if (message.StartOnNewLine) AppendConsoleText(Environment.NewLine);
            AppendConsoleText(LocExtension.Get(message.ResourceKey, message.Arguments.ToArray()));
        }
    }

    private void AppendConsoleText(string text)
    {
        LogOutput.AppendText(text);
        LogOutput.ScrollToEnd();
        _ = _consoleLog.AppendTextAsync(text);
    }
    private void SetOperationState(string resourceKey, Color color)
    {
        _viewModel.OperationText = LocExtension.Get(resourceKey);
        _viewModel.OperationBrush = new SolidColorBrush(color);
    }

    private void UpdateProfileStatus()
    {
        if (ProfileStatusItem is null || MainTabs is null) return;
        string? name = MainTabs.SelectedIndex switch
        {
            0 => (ReadProfileCombo?.SelectedItem as OperationProfile)?.Name,
            1 => (WriteProfileCombo?.SelectedItem as OperationProfile)?.Name,
            2 => (ConvertProfileCombo?.SelectedItem as OperationProfile)?.Name,
            _ => null
        };
        _viewModel.ProfileVisibility = name is null ? Visibility.Collapsed : Visibility.Visible;
        if (name is not null) _viewModel.ProfileText = LocExtension.Get("Status.Profile", name);
    }

    private async Task CheckHostToolsUpdateAsync()
    {
        if (_settings.LastHostToolsCheckUtc is DateTimeOffset checkedAt && DateTimeOffset.UtcNow - checkedAt < TimeSpan.FromDays(1))
        {
            ShowHostToolsUpdateIfNeeded(); return;
        }
        try
        {
            var release = await _hostTools.GetLatestReleaseAsync();
            _settings.AvailableHostToolsVersion = release.Version; _settings.LastHostToolsCheckUtc = DateTimeOffset.UtcNow;
            ShowHostToolsUpdateIfNeeded();
        }
        catch { /* Update checks are intentionally silent. */ }
    }

    private void ShowHostToolsUpdateIfNeeded()
    {
        var available = _settings.AvailableHostToolsVersion;
        var installed = _settings.InstalledHostToolsVersion;
        var newer = Version.TryParse(available, out var availableVersion) && (!Version.TryParse(installed, out var installedVersion) || availableVersion > installedVersion);
        _viewModel.HostToolsUpdateVisibility = newer ? Visibility.Visible : Visibility.Collapsed;
        if (newer) _viewModel.HostToolsUpdateText = LocExtension.Get("HostTools.UpdateAvailable", available!);
    }

    private static string DecoderName(string id) => LocExtension.Get("Visual.DecoderName." + id);
    private void ToolCommand_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { Tag: string verb }) return;
        if (_runner.IsRunning)
        {
            _dialogs.Show(LocExtension.Get("Operation.Busy"), LocExtension.Get("App.Title"), icon: UserDialogIcon.Information);
            return;
        }
        if (!EnsureSelectedHardwareAvailable()) return;
        if (string.IsNullOrWhiteSpace(_settings.GwExecutablePath) || !File.Exists(_settings.GwExecutablePath))
        {
            _dialogs.Show(LocExtension.Get("App.GwNotConfigured"), LocExtension.Get("App.Title"), icon: UserDialogIcon.Information);
            return;
        }
        _navigation.ShowGwTool(new(_settings.GwExecutablePath, verb, SelectedDeviceArgument(), SelectedDriveArgument(), _logsDirectory, _settings.Logging));
    }
}

public sealed record HardwareChoice(DriveSettings Drive, string Port, bool Available, string Label);
