using GWGUI.MediaEngine.Visualization;
using System.ComponentModel;
using GWGUI.MediaEngine.Exploration.Results;
using System.IO;
using System.Net.Http;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Exploration;
using GWGUI.Infrastructure.Processes;
using GWGUI.Infrastructure.Settings;
using GWGUI.App.Localization;
using GWGUI.Infrastructure.HostTools;
using GWGUI.Infrastructure.Hardware;
using GWGUI.App.ViewModels;
using GWGUI.App.Services;
using GWGUI.App.Controls;
using GWGUI.App.Rendering;
using GWGUI.App.Services.PhysicalDiskWriting;
using GWGUI.App.Services.PhysicalDiskReading;

namespace GWGUI.App;

public partial class MainWindow : Window
{
    private ReadImageSection ReadImageBlock => ReadTabBlock.ImageBlock;
    private ProfileSection ReadProfileBlock => ReadTabBlock.ProfileBlock;
    private PathSection ReadFolderBlock => ReadTabBlock.FolderBlock;
    private ReadFileNameSection ReadFileNameBlock => ReadTabBlock.FileNameBlock;
    private ReadAdvancedSection ReadAdvancedBlock => ReadTabBlock.AdvancedBlock;
    private ReadCompletionBanner ReadCompletionBlock => ReadTabBlock.CompletionBlock;
    private Button ReadExecuteButton => ReadTabBlock.ExecuteActionButton;
    private WriteAdvancedSection WriteAdvancedBlock => WriteTabBlock.AdvancedBlock;
    private PathSection WriteSourceBlock => WriteTabBlock.SourceBlock;
    private ProfileSection WriteProfileBlock => WriteTabBlock.ProfileBlock;
    private WriteFormatSection WriteFormatBlock => WriteTabBlock.FormatBlock;
    private Button WriteExecuteButton => WriteTabBlock.ExecuteActionButton;
    private ConversionAdvancedSection ConvertAdvancedBlock => ConvertTabBlock.AdvancedBlock;
    private PathSection ConvertSourceBlock => ConvertTabBlock.SourceBlock;
    private ProfileSection ConvertProfileBlock => ConvertTabBlock.ProfileBlock;
    private ConversionOutputSection ConvertOutputBlock => ConvertTabBlock.OutputBlock;
    private ConversionFormatsSection ConvertFormatsBlock => ConvertTabBlock.FormatsBlock;
    private Button ConvertExecuteButton => ConvertTabBlock.ExecuteActionButton;
    private VisualizerHeaderSection VisualizerHeader => VisualizerTabBlock.Header;
    private ScpDiskView ScpSide0 => VisualizerTabBlock.FirstSide;
    private ScpDiskView ScpSide1 => VisualizerTabBlock.SecondSide;
    private Canvas ScpInspectorLayer => VisualizerTabBlock.InspectorCanvas;
    private ScpInspectorPanel ScpInspector => VisualizerTabBlock.Inspector;
    private VisualizerTrackOverview VisualizerTrackOverview => VisualizerTabBlock.Overview;
    private ListBox ToolsList => ToolsTabBlock.ToolsList;
    private Border ErasePanel => ToolsTabBlock.ErasePanel;
    private CheckBox EraseTracksEnabled => ToolsTabBlock.EraseTracksEnabled;
    private TextBox EraseTracksValue => ToolsTabBlock.EraseTracksValue;
    private CheckBox EraseRevsEnabled => ToolsTabBlock.EraseRevsEnabled;
    private TextBox EraseRevsValue => ToolsTabBlock.EraseRevsValue;
    private TextBox EraseExpertArguments => ToolsTabBlock.EraseExpertArguments;
    private Button EraseExecuteButton => ToolsTabBlock.EraseExecuteButton;
    private Border CleanPanel => ToolsTabBlock.CleanPanel;
    private CheckBox CleanCylindersEnabled => ToolsTabBlock.CleanCylindersEnabled;
    private TextBox CleanCylindersValue => ToolsTabBlock.CleanCylindersValue;
    private CheckBox CleanPassesEnabled => ToolsTabBlock.CleanPassesEnabled;
    private TextBox CleanPassesValue => ToolsTabBlock.CleanPassesValue;
    private CheckBox CleanLingerEnabled => ToolsTabBlock.CleanLingerEnabled;
    private TextBox CleanLingerValue => ToolsTabBlock.CleanLingerValue;
    private TextBox CleanExpertArguments => ToolsTabBlock.CleanExpertArguments;
    private Button CleanExecuteButton => ToolsTabBlock.CleanExecuteButton;
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
    private CheckBox ConvertTracksEnabled => ConvertAdvancedBlock.TracksEnabledCheckBox;
    private CheckBox ConvertDiskDefsEnabled => ConvertAdvancedBlock.DiskDefinitionsEnabled;
    private TextBox ConvertDiskDefsValue => ConvertAdvancedBlock.DiskDefinitionsValue;
    private TextBox ReadFolder => ReadFolderBlock.Input;
    private TextBox ReadFileName => ReadFileNameBlock.FileNameTextBox;
    private TextBox ReadExtensionText => ReadFileNameBlock.ExtensionTextBox;
    private CheckBox ReadRevsEnabled => ReadAdvancedBlock.RevsEnabledCheckBox;
    private CheckBox ReadAutoNumber => ReadAdvancedBlock.AutoNumberCheckBox;
    private ComboBox ReadSequenceKind => ReadAdvancedBlock.SequenceKindComboBox;
    private ComboBox ReadSequenceWidth => ReadAdvancedBlock.SequenceWidthComboBox;
    private TextBox ReadSequenceValue => ReadAdvancedBlock.SequenceValueTextBox;
    private TextBlock ReadNamePreview => ReadAdvancedBlock.NamePreviewTextBlock;
    private CheckBox ReadDiskDefsEnabled => ReadAdvancedBlock.DiskDefinitionsEnabled;
    private TextBox ReadDiskDefsValue => ReadAdvancedBlock.DiskDefinitionsValue;
    private ReadCompletionBanner OpenScpBanner => ReadCompletionBlock;
    private TextBlock OpenScpSummaryText => ReadCompletionBlock.SummaryTextBlock;
    private TextBox CommandPreview => TerminalBlock?.CommandTextBox!;
    private TextBox LogOutput => TerminalBlock?.OutputTextBox!;
    private TerminalSection ConsolePanel => TerminalBlock;
    private TextBlock ScpFileName => VisualizerHeader.FileNameText;
    private TextBlock ScpSummary => VisualizerHeader.SummaryText;
    private ComboBox ScpDecoderCombo => VisualizerHeader.DecoderCombo;
    private CheckBox LinkScpViews => VisualizerHeader.LinkZoomCheckBox;
    private TextBlock HardwareStatusText => StatusBarBlock.HardwareText;
    private ComboBox HardwareSelector => StatusBarBlock.HardwareChoices;
    private StatusBarItem ProfileStatusItem => StatusBarBlock.ProfileItem;
    private TextBlock ProfileStatusText => StatusBarBlock.ProfileText;
    private StatusBarItem OperationStatusItem => StatusBarBlock.OperationItem;
    private System.Windows.Shapes.Ellipse OperationStatusLight => StatusBarBlock.OperationLight;
    private TextBlock OperationStatusText => StatusBarBlock.OperationText;
    private StatusBarItem ProgressStatusItem => StatusBarBlock.ProgressItem;
    private Grid GlobalProgressPanel => StatusBarBlock.GlobalProgress;
    private ProgressBar OperationProgress => StatusBarBlock.ProgressBar;
    private TextBlock OperationProgressText => StatusBarBlock.ProgressText;
    private TrackProgressStrip Face0TrackProgress => StatusBarBlock.Face0Progress;
    private TrackProgressStrip Face1TrackProgress => StatusBarBlock.Face1Progress;
    private StatusBarItem HostToolsUpdateItem => StatusBarBlock.HostToolsItem;
    private Button HostToolsUpdateButton => StatusBarBlock.HostToolsButton;
    private readonly ISettingsStore _settingsStore;
    private readonly IGreaseweazleRunner _runner;
    private readonly IGwCommandBuilder _commandBuilder;
    private readonly IGwInstallationManager _hostTools;
    private readonly IHardwareRegistry _hardwareRegistry;
    private readonly StartupHardwareMonitor _startupHardwareMonitor;
    private AppSettings _settings = new();
    private bool UsesInternalPhysicalRead => _settings.Engines.PhysicalRead == OperationEngine.Internal;
    private bool UsesInternalPhysicalWrite => _settings.Engines.PhysicalWrite == OperationEngine.Internal;
    private bool UsesInternalConversion => _settings.Engines.Conversion == OperationEngine.Internal;
    private bool UsesInternalExplorerRead => _settings.Engines.ExplorerRead == OperationEngine.Internal;
    private readonly OperationRuntimeController _operation;
    private readonly IMessageDialogService _dialogs;
    private readonly IFileDialogService _fileDialogs;
    private readonly IBusinessDialogService _businessDialogs;
    private readonly IWindowNavigationService _navigation;
    private readonly ImageFormatWorkspace _formatWorkspace;
    private readonly DiskDefinitionsController _diskDefinitionsController;
    private readonly WindowPlacementController _windowPlacement = new();
    private IImageFormatCatalog _formatCatalog = null!;
    private readonly OperationProfileCollection _profiles = new();
    private readonly OperationProfileController _profileController;
    private ImageFormatDetector _formatDetector = null!;
    private DetectedImageFormat? _detectedWriteFormat;
    private readonly ConversionFormatPresenter _conversionFormatPresenter = new();
    private string? _conversionSourceExtension;
    private DetectedImageFormat? _conversionSourceDetection;
    private readonly FluxDecoderRegistry _fluxDecoders = new();
    private readonly ScpInspectorController _scpInspectorController;
    private readonly DiskImageWorkspaceController _diskImageWorkspace;
    private readonly OperationProgressController _progress;
    private readonly HardwareSelectionController _hardwareSelection;
    private readonly MaintenanceToolsController _maintenanceTools;
    private readonly string _logsDirectory;
    private readonly ConsoleLogSession _consoleLog;
    private readonly TerminalPanelController _terminalPanel;
    private readonly MainWindowViewModel _viewModel;
    private string? _lastInternalReadProgressLine;
    private GwFormatCapabilities _gwCapabilities = GwFormatCapabilities.Unknown;
    private bool _settingsSaveInProgress;
    private bool _closeAfterSettingsSave;
    private readonly bool _settingsProvidedAtStartup;

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
        ConnectToolsComponents();
        ConnectExplorerComponent();
        ConnectStatusBar();
        _dialogs = dialogs ?? new WpfMessageDialogService(this);
        _fileDialogs = fileDialogs ?? new WpfFileDialogService(this);
        _businessDialogs = businessDialogs ?? new WpfBusinessDialogService(this);
        _profileController = new OperationProfileController(
            _profiles,
            _businessDialogs,
            _dialogs,
            (key, arguments) => LocExtension.Get(key, arguments));
        _commandBuilder = commandBuilder ?? new GwCommandBuilder();
        _hostTools = hostTools ?? new GwInstallationManager(new HttpClient(), StoragePaths.HostToolsDirectory);
        var directory = StoragePaths.DataDirectory;
        _logsDirectory = StoragePaths.LogsDirectory;
        _consoleLog = new ConsoleLogSession(_logsDirectory, () => _settings.Logging);
        _terminalPanel = new TerminalPanelController(TerminalBlock, ConsoleRow, ConsoleSplitter, _settings);
        _runner = runner ?? new GreaseweazleRunner();
        _hardwareRegistry = hardwareRegistry ?? new GreaseweazleHardwareRegistry(new WindowsSerialDeviceDiscovery(), _runner, _commandBuilder);
        _navigation = navigation ?? new WpfWindowNavigationService(this, _hostTools, _runner, _commandBuilder);
        AmigaEmulationBlock.ConfigurationRequested += async (_, _) =>
        {
            _navigation.ShowOptions(_settings, OptionsSection.Emulation);
            await AmigaEmulationBlock.ReloadConfigurationsAsync();
        };
        _viewModel = new MainWindowViewModel(LocExtension.Get("Hardware.NotConfigured"), LocExtension.Get("Status.ReadyShort"));
        _progress = new OperationProgressController(_viewModel, Face0TrackProgress, Face1TrackProgress,
            (key, arguments) => LocExtension.Get(key, arguments));
        _operation = new OperationRuntimeController(
            Dispatcher,
            _viewModel,
            _progress,
            LogOutput,
            _consoleLog,
            (key, arguments) => LocExtension.Get(key, arguments));
        _hardwareSelection = new HardwareSelectionController(
            StatusBarBlock,
            _viewModel,
            () => _settings,
            _dialogs,
            enabled =>
            {
                ReadExecuteButton.IsEnabled = enabled;
                WriteExecuteButton.IsEnabled = enabled;
                EraseExecuteButton.IsEnabled = enabled;
                CleanExecuteButton.IsEnabled = enabled;
            },
            () => { UpdateReadCommand(); UpdateWriteCommand(); UpdateToolCommand(); },
            (key, arguments) => LocExtension.Get(key, arguments));
        _maintenanceTools = new MaintenanceToolsController(
            ToolsTabBlock,
            () => _settings,
            _commandBuilder,
            SelectedDeviceArgument,
            SelectedDriveArgument,
            () => MainTabs?.SelectedIndex == 5,
            command => CommandPreview.Text = command,
            (key, arguments) => LocExtension.Get(key, arguments));
        DataContext = _viewModel;
        _formatWorkspace = new ImageFormatWorkspace(key => LocExtension.Get(key));
        SynchronizeFormatWorkspace();
        _diskDefinitionsController = new DiskDefinitionsController(
            ReadAdvancedBlock, WriteAdvancedBlock, ConvertAdvancedBlock,
            () => _settings, _formatWorkspace, _fileDialogs, _dialogs,
            () => { SynchronizeFormatWorkspace(); RefreshFormatSelectors(); },
            path => { _viewModel.Read.DiskDefs.Value = path; _viewModel.Read.DiskDefs.Enabled = true; },
            path => { _viewModel.Write.DiskDefs.Value = path; _viewModel.Write.DiskDefs.Enabled = true; },
            path => { _viewModel.Conversion.DiskDefs.Value = path; _viewModel.Conversion.DiskDefs.Enabled = true; },
            UpdateReadCommand, UpdateWriteCommand, UpdateConvertCommand,
            (key, arguments) => LocExtension.Get(key, arguments));
        RefreshExplorerFormats();
        VisualizerHeader.SetFormats(_formatCatalog.Formats);
        VisualizerHeader.ClassificationSelector.ValueChanged += (_, _) => ApplyVisualizerClassification();
        var diskImageCancellation = new DiskImageCancellationScope();
        DiskImageWorkspaceController? diskImageWorkspace = null;
        _scpInspectorController = new ScpInspectorController(
            this,
            VisualizerTabBlock,
            _fluxDecoders,
            diskImageCancellation,
            cancellationToken => diskImageWorkspace!.PrepareViewsForInspectorAsync(cancellationToken),
            () => diskImageWorkspace!.HideProgressForInspector(),
            (key, arguments) => LocExtension.Get(key, arguments));
        _diskImageWorkspace = diskImageWorkspace = new DiskImageWorkspaceController(
            DiskExplorer,
            VisualizerTabBlock,
            _viewModel,
            Face0TrackProgress,
            Face1TrackProgress,
            () => _settings,
            () => _formatDetector,
            () => _gwCapabilities,
            _fileDialogs,
            _commandBuilder,
            _runner,
            _scpInspectorController,
            new ScpDocumentLoader(new ScpReader(), (key, arguments) => LocExtension.Get(key, arguments)),
            DiskImageExplorer.CreateDefault(),
            new SectorImageFluxVisualizer(),
            diskImageCancellation,
            () => _operation.IsRunning,
            ShowLoggedError,
            (key, arguments) => LocExtension.Get(key, arguments));
        VisualizerHeader.OpenButton.Click += OpenScp_Click;
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

    private void ConnectStatusBar()
    {
        StatusBarBlock.HardwareSelectionChanged += HardwareSelector_Changed;
        StatusBarBlock.HostToolsUpdateRequested += Preferences_Click;
        StatusBarBlock.ToggleConsoleRequested += ToggleConsole_Click;
        RegisterName(nameof(HardwareStatusText), HardwareStatusText);
        RegisterName(nameof(HardwareSelector), HardwareSelector);
        RegisterName(nameof(OperationProgress), OperationProgress);
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
        ReadAdvancedBlock.InputChanged += ReadInput_Changed;
        ReadAdvancedBlock.FakeIndexChecked += ReadFakeIndex_Checked;
        ReadAdvancedBlock.HardSectorsChecked += ReadHardSectors_Checked;
        ReadAdvancedBlock.DenselChecked += ReadDensel_Checked;
        ReadAdvancedBlock.Tg43Checked += ReadTg43_Checked;
        ReadAdvancedBlock.SequenceKindChanged += ReadSequenceKind_Changed;
        ReadCompletionBlock.ExploreRequested += ExploreLastScp_Click;
        ReadCompletionBlock.VisualizeRequested += OpenLastScp_Click;
        ReadTabBlock.ExecuteRequested += ExecuteRead_Click;
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
        RegisterName(nameof(ReadRevsEnabled), ReadRevsEnabled);
        RegisterName(nameof(ReadExecuteButton), ReadExecuteButton);
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
        WriteTabBlock.ExecuteRequested += ExecuteWrite_Click;
        RegisterName(nameof(WriteProfileCombo), WriteProfileCombo);
        RegisterName(nameof(WriteSourceText), WriteSourceText);
        RegisterName(nameof(WriteDetectionText), WriteDetectionText);
        RegisterName(nameof(WriteFormatCombo), WriteFormatCombo);
        RegisterName(nameof(WriteNoVerify), WriteNoVerify);
        RegisterName(nameof(WriteDiskDefsEnabled), WriteDiskDefsEnabled);
        RegisterName(nameof(WriteDiskDefsValue), WriteDiskDefsValue);
        RegisterName(nameof(WriteExecuteButton), WriteExecuteButton);
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
        ConvertTabBlock.ExecuteRequested += ExecuteConvert_Click;
        ConvertTabBlock.MigrationRequested += OpenFileMigration_Click;
        RegisterName(nameof(ConvertProfileCombo), ConvertProfileCombo);
        RegisterName(nameof(ConvertSourceText), ConvertSourceText);
        RegisterName(nameof(ConvertOutputName), ConvertOutputName);
        RegisterName(nameof(ConvertTags), ConvertTags);
        RegisterName(nameof(ConvertSourceInfo), ConvertSourceInfo);
        RegisterName(nameof(ConvertTracksEnabled), ConvertTracksEnabled);
        RegisterName(nameof(ConvertDiskDefsEnabled), ConvertDiskDefsEnabled);
        RegisterName(nameof(ConvertDiskDefsValue), ConvertDiskDefsValue);
        RegisterName(nameof(ConvertFormatsBlock), ConvertFormatsBlock);
        RegisterName(nameof(ConvertExecuteButton), ConvertExecuteButton);
    }

    private void ConnectToolsComponents()
    {
        ToolsTabBlock.ToolSelectionChanged += ToolsList_SelectionChanged;
        ToolsTabBlock.InputChanged += ToolInput_Changed;
        ToolsTabBlock.EraseRequested += ExecuteErase_Click;
        ToolsTabBlock.CleanRequested += ExecuteClean_Click;
        RegisterName(nameof(EraseExecuteButton), EraseExecuteButton);
        RegisterName(nameof(CleanExecuteButton), CleanExecuteButton);
    }

    private void ConnectExplorerComponent()
    {
        DiskExplorer.OpenRequested += OpenExplorerImage_Click;
        DiskExplorer.ReadDiskRequested += ReadDiskIntoExplorer_Click;
        DiskExplorer.FormatChanged += async (_, _) =>
        {
            if (!string.IsNullOrWhiteSpace(_diskImageWorkspace.ExplorerPath)) await LoadExplorerImageAsync(_diskImageWorkspace.ExplorerPath, false);
        };
    }

    private async void OpenExplorerImage_Click(object? sender, RoutedEventArgs e)
    {
        var path = OpenDiskImageFromLastFolder();
        if (path is not null) await LoadImageInExplorerAndVisualizerAsync(path);
    }

    private async void ReadDiskIntoExplorer_Click(object? sender, RoutedEventArgs e)
    {
        if (_operation.IsRunning) { ConfirmAndRequestStop(); return; }
        if (!EnsureSelectedHardwareAvailable()) return;
        if (!UsesInternalExplorerRead && (string.IsNullOrWhiteSpace(_settings.GwExecutablePath) || !File.Exists(_settings.GwExecutablePath)))
        {
            _dialogs.Show(LocExtension.Get("App.GwNotConfigured"), LocExtension.Get("App.Title"), icon: UserDialogIcon.Information);
            return;
        }
        var selectedDrive = SelectedHardware()?.Label ?? LocExtension.Get("Hardware.NotConfigured");
        if (_dialogs.Show(
                LocExtension.Get("Explorer.ReadDiskConfirm", selectedDrive),
                LocExtension.Get("Explorer.ReadDiskConfirmTitle"),
                UserDialogButtons.YesNo,
                UserDialogIcon.Question) != UserDialogResult.Yes) return;

        var temporaryDirectory = Path.Combine(Path.GetTempPath(), "GW GUI");
        Directory.CreateDirectory(temporaryDirectory);
        var temporaryPath = Path.Combine(temporaryDirectory, $"explorer-{Guid.NewGuid():N}.scp");
        var command = UsesInternalExplorerRead
            ? null
            : _commandBuilder.BuildRead(new ReadRequest(_settings.GwExecutablePath!, temporaryPath, ReadResultKind.RawScp, null, [], SelectedDeviceArgument(), SelectedDriveArgument()));
        try
        {
            DiskExplorer.SetReadDiskRunning(true);
            BeginProgress();
            await RenderPendingProgressAsync();
            LogOutput.Clear();
            await _consoleLog.BeginAsync(
                UsesInternalExplorerRead ? "read-explorer-internal" : "read-explorer",
                UsesInternalExplorerRead ? LocExtension.Get("Read.InternalPreview", temporaryPath) : command!.ToDisplayString());
            var outcome = UsesInternalExplorerRead
                ? await ExecuteInternalExplorerReadAsync(temporaryPath)
                : await _operation.RunAsync(token => _runner.RunAsync(command!, new Progress<GwOutputLine>(ReportOutput), token));
            await FlushPendingOutputAsync();
            ApplyOperationResult(_operation.Present(outcome));
            if (outcome.Result?.IsSuccess == true && File.Exists(temporaryPath)) await LoadImageInExplorerAndVisualizerAsync(temporaryPath);
        }
        catch (Exception exception)
        {
            ShowLoggedError(exception, "Reading disk into Explorer", "Tab.Explorer", "Explorer.LoadFailed");
        }
        finally
        {
            EndProgress();
            DiskExplorer.SetReadDiskRunning(false);
            try { if (File.Exists(temporaryPath)) File.Delete(temporaryPath); } catch { }
        }
    }

    private async Task<OperationOutcome<GwExecutionResult>> ExecuteInternalExplorerReadAsync(string temporaryPath)
    {
        var hardware = SelectedHardware();
        if (hardware is null) return new(false, null, new InvalidOperationException(LocExtension.Get("Hardware.NotConfigured")));
        var selection = GreaseweazleDriveSelectionPolicy.Resolve(hardware.Drive.Selection);
        var options = new PhysicalDiskReadOptions(
            hardware.Port,
            selection.BusType,
            selection.Unit,
            PhysicalDiskTrackSelectionParser.Parse("c=0-79:h=0-1"),
            ScpCaptureDiskTypePolicy.Resolve(hardware.Drive.Density));
        var stopwatch = Stopwatch.StartNew();
        _lastInternalReadProgressLine = null;
        return await _operation.RunAsync(async token =>
        {
            var progress = new Progress<PhysicalDiskReadOperationProgress>(ReportInternalReadProgress);
            await InternalPhysicalDiskReader.CreateDefault().ReadAsync(options, temporaryPath, progress, token);
            return new GwExecutionResult(0, false, stopwatch.Elapsed, []);
        });
    }

    private async Task<ExploredDiskImage?> LoadExplorerImageAsync(string path, bool newImage = true)
        => await _diskImageWorkspace.LoadExplorerAsync(path, newImage);

    private async void OpenScp_Click(object sender, RoutedEventArgs e)
    {
        var path = OpenDiskImageFromLastFolder();
        if (path is null) return;
        await LoadImageInExplorerAndVisualizerAsync(path);
    }

    private string? OpenDiskImageFromLastFolder()
        => _diskImageWorkspace.SelectImage();

    private async Task LoadImageInExplorerAndVisualizerAsync(string path, string? displayFileName = null)
        => await _diskImageWorkspace.LoadAsync(path, displayFileName);

    private async Task LoadVisualizerImageAsync(string path, string? displayFileName = null, ExploredDiskImage? exploredImage = null)
        => await _diskImageWorkspace.LoadVisualizerAsync(path, displayFileName, exploredImage);

    private void ShowAdvancedValidation(Exception exception, string title)
    {
        _diskDefinitionsController.ShowInvalid(exception, title);
    }

    private async void OpenLastScp_Click(object sender, RoutedEventArgs e)
    {
        if (_diskImageWorkspace.LastCapturedPath is null) return;
        MainTabs.SelectedIndex = 3;
        await LoadImageInExplorerAndVisualizerAsync(_diskImageWorkspace.LastCapturedPath);
    }

    private async void ExploreLastScp_Click(object sender, RoutedEventArgs e)
    {
        if (_diskImageWorkspace.LastCapturedPath is null) return;
        MainTabs.SelectedIndex = 4;
        await LoadImageInExplorerAndVisualizerAsync(_diskImageWorkspace.LastCapturedPath);
    }

    private async Task LoadScpAsync(string path, string? displayFileName = null)
        => await _diskImageWorkspace.LoadScpAsync(path, displayFileName);

    private void ApplyVisualizerClassification()
    {
        _diskImageWorkspace.ApplyClassification();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (!_settingsProvidedAtStartup) _settings = await _settingsStore.LoadAsync();
        if (!string.IsNullOrWhiteSpace(_settings.GwExecutablePath))
            _formatWorkspace.SetCapabilities(await new GwFormatCapabilityReader().ReadAsync(_settings.GwExecutablePath));
        SynchronizeFormatWorkspace();
        _diskDefinitionsController.LoadConfigured();
        RebuildFormatCatalog();
        RefreshExplorerFormats();
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
                var path = ErrorLog.Write(exception, "Checking configured hardware at startup");
                var detail = path is null ? LocExtension.Get("Common.Unknown") : LocExtension.Get("Error.LogSaved", path);
                _dialogs.Show(LocExtension.Get("Hardware.StartupCheckFailed", detail), LocExtension.Get("Hardware.StartupTitle"), icon: UserDialogIcon.Warning);
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
        => _profileController.Refresh(WriteProfileCombo, OperationKind.Write, selectedId);

    private void RefreshConvertProfiles(string? selectedId = null)
        => _profileController.Refresh(ConvertProfileCombo, OperationKind.Convert, selectedId);

    private async void BrowseWriteSource_Click(object sender, RoutedEventArgs e)
    {
        var path = _fileDialogs.OpenFile(new(LocExtension.Get("Common.DiskImageFilter"), ReadFolder.Text));
        if (path is null) return;
        _viewModel.Write.SourcePath = path;
        _detectedWriteFormat = _formatDetector.Detect(path, new FileInfo(path).Length);
        WriteDetectionText.Text = $"{_detectedWriteFormat.Format?.DisplayName ?? LocExtension.Get("Detection.Ambiguous")} â€” {LocExtension.Get(_detectedWriteFormat.ExplanationKey)}";
        WriteFormatCombo.ItemsSource = _detectedWriteFormat.Candidates.Count > 0 ? _detectedWriteFormat.Candidates : _formatCatalog.Formats;
        WriteFormatCombo.SelectedItem = _detectedWriteFormat.Format;
        WriteFormatCombo.Visibility = _detectedWriteFormat.RequiresUserChoice ? Visibility.Visible : Visibility.Collapsed;
        WriteFormatBlock.VisualizeTracksButton.IsEnabled = true;
        try
        {
            await _diskImageWorkspace.AnalyzeAsync(path);
        }
        catch (Exception exception) when (exception is InvalidDataException or NotSupportedException)
        {
            AppendAnalysisFailure(exception, $"Analyzing write source: {path}");
        }
        UpdateWriteCommand();
    }

    private async void VisualizeWriteSource_Click(object sender, RoutedEventArgs e)
    {
        var source = _viewModel.Write.SourcePath;
        if (string.IsNullOrWhiteSpace(source) || !File.Exists(source)) return;
        if (Path.GetExtension(source).Equals(".scp", StringComparison.OrdinalIgnoreCase))
        {
            MainTabs.SelectedIndex = 3;
            await LoadImageInExplorerAndVisualizerAsync(source);
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
            ApplyOperationResult(_operation.Present(outcome));
            EndProgress();
            if (outcome.Result?.IsSuccess != true || !File.Exists(temporaryPath)) return;
            MainTabs.SelectedIndex = 3;
            await Task.WhenAll(LoadScpAsync(temporaryPath, Path.GetFileName(source)), LoadExplorerImageAsync(source));
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
        if (UsesInternalPhysicalWrite)
        {
            CommandPreview.Text = LocExtension.Get("Write.InternalPreview", WriteSourceText.Text);
            return;
        }
        try { CommandPreview.Text = BuildWriteCommand().ToDisplayString(); }
        catch (ArgumentException) { CommandPreview.Text = $"âš  {LocExtension.Get("Advanced.Invalid", LocExtension.Get("Common.Unknown"))}"; }
    }

    private async void ExecuteWrite_Click(object sender, RoutedEventArgs e)
    {
        if (_operation.IsRunning) { ConfirmAndRequestStop(); return; }
        if (!EnsureSelectedHardwareAvailable()) return;
        if (!_diskDefinitionsController.Validate(WriteDiskDefsEnabled, WriteDiskDefsValue, LocExtension.Get("Write.Title"))) return;
        if (!File.Exists(WriteSourceText.Text)) { _dialogs.Show(LocExtension.Get("Write.SelectSource"), LocExtension.Get("Write.Title"), icon: UserDialogIcon.Information); return; }
        var selected = WriteFormatCombo.SelectedItem as DiskFormat ?? _detectedWriteFormat?.Format;
        if (selected is null || (_detectedWriteFormat?.RequiresUserChoice == true && WriteFormatCombo.SelectedItem is null))
        { _dialogs.Show(LocExtension.Get("Write.Ambiguous"), LocExtension.Get("Write.Title"), icon: UserDialogIcon.Warning); WriteFormatCombo.Visibility = Visibility.Visible; return; }
        if (UsesInternalPhysicalWrite)
        {
            await ExecuteInternalWriteAsync(selected);
            return;
        }
        if (string.IsNullOrWhiteSpace(_settings.GwExecutablePath) || !File.Exists(_settings.GwExecutablePath)) { _dialogs.Show(LocExtension.Get("App.GwNotConfigured"), LocExtension.Get("App.Title"), icon: UserDialogIcon.Information); return; }
        GwCommand command;
        try { command = BuildWriteCommand(); }
        catch (ArgumentException) { _diskDefinitionsController.ShowInvalid(LocExtension.Get("Write.Title")); return; }
        var warning = LocExtension.Get(_viewModel.Write.DisableVerification ? "Write.VerifyOff" : "Write.VerifyOn");
        var confirmation = LocExtension.Get("Write.Confirm", Path.GetFileName(WriteSourceText.Text), selected.DisplayName, SelectedHardware()?.Label ?? LocExtension.Get("Hardware.NotConfigured"), warning);
        if (_dialogs.Show(confirmation, LocExtension.Get("Write.ConfirmTitle"), UserDialogButtons.OkCancel, UserDialogIcon.Warning) != UserDialogResult.Ok) return;
        WriteExecuteButton.Content = LocExtension.Get("Common.Stop"); BeginProgress(); await RenderPendingProgressAsync(); LogOutput.Clear(); await _consoleLog.BeginAsync("write", command.ToDisplayString());
        var output = new Progress<GwOutputLine>(ReportOutput);
        var outcome = await _operation.RunAsync(token => _runner.RunAsync(command, output, token));
        await FlushPendingOutputAsync();
        ApplyOperationResult(_operation.Present(outcome));
        EndProgress(); WriteExecuteButton.Content = LocExtension.Get("Common.Execute");
    }

    private void OpenFileMigration_Click(object sender, RoutedEventArgs e)
    {
        var sourcePath = File.Exists(ConvertSourceText.Text) ? ConvertSourceText.Text : null;
        new FileMigrationWindow(sourcePath) { Owner = this }.ShowDialog();
    }

    private async Task ExecuteInternalWriteAsync(DiskFormat selected)
    {
        var hardware = SelectedHardware();
        if (hardware is null)
        {
            _dialogs.Show(LocExtension.Get("Hardware.NotConfigured"), LocExtension.Get("Write.Title"), icon: UserDialogIcon.Warning);
            return;
        }
        if (!_viewModel.Write.DisableVerification)
        {
            _dialogs.Show(LocExtension.Get("Write.InternalVerificationUnavailable"), LocExtension.Get("Write.Title"), icon: UserDialogIcon.Warning);
            return;
        }
        if (HasUnsupportedInternalWriteOptions())
        {
            _dialogs.Show(LocExtension.Get("Write.InternalUnsupportedOptions"), LocExtension.Get("Write.Title"), icon: UserDialogIcon.Warning);
            return;
        }

        var warning = LocExtension.Get(_viewModel.Write.DisableVerification ? "Write.VerifyOff" : "Write.VerifyOn");
        var confirmation = LocExtension.Get("Write.Confirm", Path.GetFileName(WriteSourceText.Text), selected.DisplayName, hardware.Label, warning);
        if (_dialogs.Show(confirmation, LocExtension.Get("Write.ConfirmTitle"), UserDialogButtons.OkCancel, UserDialogIcon.Warning) != UserDialogResult.Ok) return;

        WriteExecuteButton.Content = LocExtension.Get("Common.Stop");
        BeginProgress();
        await RenderPendingProgressAsync();
        LogOutput.Clear();
        await _consoleLog.BeginAsync("write-internal", LocExtension.Get("Write.InternalPreview", WriteSourceText.Text));
        var stopwatch = Stopwatch.StartNew();
        var outcome = await _operation.RunAsync(async token =>
        {
            var writer = InternalPhysicalDiskWriter.CreateDefault();
            var options = CreateInternalWriteOptions(hardware);
            var progress = new Progress<PhysicalTrackWriteProgress>(_progress.Accept);
            var request = new InternalPhysicalDiskWriteRequest(WriteSourceText.Text, selected.Id, options);
            var result = await writer.WriteAsync(request, progress, token);
            var lines = result.Failures.Select(failure => new GwOutputLine(
                DateTimeOffset.Now,
                GwOutputStream.Error,
                LocExtension.Get("Write.InternalFailure", failure.Cylinder?.ToString() ?? "-", failure.Head?.ToString() ?? "-", LocExtension.Get("Write.InternalFailureReason")))).ToArray();
            foreach (var line in lines) ReportOutput(line);
            return new GwExecutionResult(result.IsSuccess ? 0 : 1, result.Cancelled, stopwatch.Elapsed, lines);
        });
        await FlushPendingOutputAsync();
        ApplyOperationResult(_operation.Present(outcome));
        EndProgress();
        WriteExecuteButton.Content = LocExtension.Get("Common.Execute");
    }

    private PhysicalDiskWriteOptions CreateInternalWriteOptions(HardwareChoice hardware)
    {
        var selection = GreaseweazleDriveSelectionPolicy.Resolve(hardware.Drive.Selection);
        return new(hardware.Port, selection.BusType, selection.Unit, Verify: false);
    }

    private bool HasUnsupportedInternalWriteOptions() =>
        _viewModel.Write.EraseEmpty.Enabled ||
        _viewModel.Write.Retries.Enabled ||
        _viewModel.Write.Tracks.Enabled ||
        _viewModel.Write.PreErase.Enabled ||
        _viewModel.Write.FakeIndex.Enabled ||
        _viewModel.Write.HardSectors.Enabled ||
        _viewModel.Write.Precomp.Enabled ||
        _viewModel.Write.Reverse.Enabled ||
        _viewModel.Write.Densel.Enabled ||
        _viewModel.Write.Tg43.Enabled ||
        _viewModel.Write.DiskDefs.Enabled ||
        !string.IsNullOrWhiteSpace(_viewModel.Write.ExpertArguments);

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
        var enabled = _viewModel.Write.CaptureEnabledOptions();
        var values = _viewModel.Write.CaptureValues();
        if (WriteFormatCombo.SelectedItem is DiskFormat format) values["format"] = format.Id;
        var profile = _profileController.Save(OperationKind.Write, name =>
            new OperationProfile(Guid.NewGuid().ToString("N"), OperationKind.Write, name, values, enabled));
        if (profile is null) return;
        RefreshWriteProfiles(profile.Id);
    }

    private void BuildConversionFormats(string? sourceExtension, DetectedImageFormat? detection = null)
    {
        if (ConvertFormatsBlock is null) return;
        _conversionSourceExtension = sourceExtension;
        _conversionSourceDetection = detection;
        var items = _conversionFormatPresenter.Build(_formatCatalog, sourceExtension, detection, _viewModel.Conversion.SelectedFormats, _viewModel.Conversion.ExplicitExtensions);
        foreach (var item in items)
        {
            if (!item.IsCompatible && _viewModel.Conversion.SelectedFormats.Contains(item.Format.Id))
                _viewModel.Conversion.SetFormat(item.Format.Id, false, item.ExplicitExtensions);
        }
        ConvertFormatsBlock.SetItems(items, sourceExtension);
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
        var enabled = _viewModel.Conversion.CaptureProfileEnabled();
        var values = _viewModel.Conversion.CaptureProfileValues();
        var profile = _profileController.Save(OperationKind.Convert, name =>
            new OperationProfile(Guid.NewGuid().ToString("N"), OperationKind.Convert, name, values, enabled));
        if (profile is null) return;
        RefreshConvertProfiles(profile.Id);
    }

    private void ConversionSelectionChanged(object? sender, EventArgs e)
    {
        if (sender is not ConversionFormatControl control) return;
        _viewModel.Conversion.SetFormat(control.Format.Id, control.IsSelected, control.ExplicitExtensions);
        BuildConversionFormats(_conversionSourceExtension, _conversionSourceDetection);
        UpdateConvertCommand();
    }

    private async void BrowseConvertSource_Click(object sender, RoutedEventArgs e)
    {
        var path = _fileDialogs.OpenFile(new(LocExtension.Get("Common.DiskImageFilter"), ReadFolder.Text));
        if (path is null) return;
        _viewModel.Conversion.SourcePath = path; _viewModel.Conversion.OutputName = Path.GetFileNameWithoutExtension(path);
        var detection = _formatDetector.Detect(path, new FileInfo(path).Length);
        ConvertSourceInfo.Text = detection.Format?.DisplayName ?? LocExtension.Get("Conversion.SourceAmbiguous");
        ConvertSourceBlock.ActionButton.Visibility = Path.GetExtension(path).Equals(".scp", StringComparison.OrdinalIgnoreCase)
            ? Visibility.Visible
            : Visibility.Collapsed;
        try
        {
            await _diskImageWorkspace.AnalyzeAsync(path);
        }
        catch (Exception exception) when (exception is InvalidDataException or NotSupportedException)
        {
            AppendAnalysisFailure(exception, $"Analyzing conversion source: {path}");
        }
        BuildConversionFormats(Path.GetExtension(path), detection); UpdateConvertCommand();
    }

    private async void VisualizeConvertSource_Click(object sender, RoutedEventArgs e)
    {
        var source = _viewModel.Conversion.SourcePath;
        if (string.IsNullOrWhiteSpace(source) || !File.Exists(source) ||
            !Path.GetExtension(source).Equals(".scp", StringComparison.OrdinalIgnoreCase)) return;
        MainTabs.SelectedIndex = 3;
        await LoadImageInExplorerAndVisualizerAsync(source);
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
            if (UsesInternalConversion && !ConversionBatchExecutor.IsInternal(_viewModel.Conversion.SourcePath, outputs[0]))
            {
                CommandPreview.Text = LocExtension.Get("Conversion.EngineInternalUnavailable", outputs[0].OutputPath);
                return;
            }
            var first = UsesInternalConversion
                ? new GwCommand("GW GUI", "encode", ["--codec", outputs[0].FormatId, _viewModel.Conversion.SourcePath, outputs[0].OutputPath])
                : _commandBuilder.BuildConversion(_settings.GwExecutablePath ?? "gw.exe", _viewModel.Conversion.SourcePath, outputs[0], GetConvertOptions(), _viewModel.Conversion.ExpertArguments);
            CommandPreview.Text = first.ToDisplayString() + (outputs.Count > 1 ? LocExtension.Get("Conversion.More", outputs.Count - 1) : "");
        }
        catch (Exception exception) { ErrorLog.Write(exception, "Building conversion preview"); CommandPreview.Text = $"âš  {LocExtension.Get("Advanced.Invalid", LocExtension.Get("Common.Unknown"))}"; }
    }

    private async void ExecuteConvert_Click(object sender, RoutedEventArgs e)
    {
        if (_operation.IsRunning) { ConfirmAndRequestStop(); return; }
        if (!_diskDefinitionsController.Validate(ConvertDiskDefsEnabled, ConvertDiskDefsValue, LocExtension.Get("Conversion.Title"))) return;
        if (!File.Exists(ConvertSourceText.Text)) { _dialogs.Show(LocExtension.Get("Conversion.SourceRequired"), LocExtension.Get("Conversion.Title")); return; }
        if (string.IsNullOrWhiteSpace(ConvertOutputName.Text)) { _dialogs.Show(LocExtension.Get("Conversion.NameRequired"), LocExtension.Get("Conversion.Title")); return; }
        IReadOnlyList<ConversionOutput> outputs;
        try { outputs = PlanConversions(); GwOptionValidator.Validate(GetConvertOptions()); } catch (Exception) { _diskDefinitionsController.ShowInvalid(LocExtension.Get("Conversion.Title")); return; }
        if (outputs.Count == 0) { _dialogs.Show(LocExtension.Get("Conversion.CheckOutput"), LocExtension.Get("Conversion.Title")); return; }
        if (UsesInternalConversion && outputs.Any(output => !ConversionBatchExecutor.IsInternal(_viewModel.Conversion.SourcePath, output)))
        {
            _dialogs.Show(LocExtension.Get("Conversion.EngineInternalUnavailable", outputs.First(output => !ConversionBatchExecutor.IsInternal(_viewModel.Conversion.SourcePath, output)).OutputPath), LocExtension.Get("Conversion.Title"));
            return;
        }
        if (!UsesInternalConversion &&
            (string.IsNullOrWhiteSpace(_settings.GwExecutablePath) || !File.Exists(_settings.GwExecutablePath)))
        { _dialogs.Show(LocExtension.Get("App.GwNotConfigured"), LocExtension.Get("App.Title")); return; }
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
            var items = outputs.Select(planned => (Output: planned, Command: _commandBuilder.BuildConversion(_settings.GwExecutablePath ?? "gw.exe", _viewModel.Conversion.SourcePath, planned, GetConvertOptions(), _viewModel.Conversion.ExpertArguments))).ToArray();
            return new ConversionBatchExecutor(_runner).RunAsync(_viewModel.Conversion.SourcePath, items, progress, item => Dispatcher.Invoke(() =>
            {
                BeginProgress();
                AppendConsoleText($"{Environment.NewLine}â†’ {item.Label}{Environment.NewLine}");
            }, System.Windows.Threading.DispatcherPriority.ContextIdle), token, _settings.Engines.Conversion);
        });
        await FlushPendingOutputAsync();
        ApplyOperationResult(_operation.Present(outcome));
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
        _diskImageWorkspace.CancelAll();
        if (_closeAfterSettingsSave)
        {
            _diskImageWorkspace.Dispose();
            return;
        }
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

        var stopEmulation = await Dispatcher.InvokeAsync(() => AmigaEmulationBlock.StopAllAsync());
        await stopEmulation.ConfigureAwait(false);

        await _operation.WaitForCompletionAsync().ConfigureAwait(false);

        if (Dispatcher.HasShutdownStarted || Dispatcher.HasShutdownFinished) return;
        try
        {
            await Dispatcher.InvokeAsync(() =>
            {
                if (failure is not null)
                {
                    var logPath = ErrorLog.Write(failure, "Saving application settings");
                    var detail = logPath is null ? LocExtension.Get("Common.Unknown") : LocExtension.Get("Error.LogSaved", logPath);
                    _dialogs.Show(LocExtension.Get("App.SettingsSaveFailed", detail), LocExtension.Get("App.Title"), icon: UserDialogIcon.Warning);
                }
                _settingsSaveInProgress = false;
                _closeAfterSettingsSave = true;
                Close();
            }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }
        catch (TaskCanceledException) when (Dispatcher.HasShutdownStarted || Dispatcher.HasShutdownFinished) { }
    }

    private void RefreshReadProfiles(string? selectedId = null)
        => _profileController.Refresh(ReadProfileCombo, OperationKind.Read, selectedId);

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
        var enabled = _viewModel.Read.CaptureEnabledOptions();
        var values = _viewModel.Read.CaptureValues();
        values["result"] = RawScpRadio.IsChecked == true ? "raw" : "known";
        if (ReadFormatCombo.SelectedItem is DiskFormat format) values["format"] = format.Id;
        if (ReadExtensionCombo.SelectedItem is ImageExtension extension) values["extension"] = extension.Extension;
        if (!string.IsNullOrWhiteSpace(_viewModel.Read.Folder)) values["folder"] = _viewModel.Read.Folder;
        var profile = _profileController.Save(OperationKind.Read, name =>
            new OperationProfile(Guid.NewGuid().ToString("N"), OperationKind.Read, name, values, enabled));
        if (profile is null) return;
        RefreshReadProfiles(profile.Id);
    }

    private void CaptureProfiles()
    {
        _settings.Profiles = _profiles.Capture();
    }

    private void LoadProfileStores()
    {
        _profiles.Reset(_settings.Profiles);
    }

    private void ToggleConsole_Click(object sender, RoutedEventArgs e) => _terminalPanel.Toggle();

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
        await _terminalPanel.ExportAsync(path);
    }

    private void CopyConsole_Click(object sender, RoutedEventArgs e)
    {
        _terminalPanel.CopyToClipboard();
    }

    private void SetConsoleVisibility(bool visible) => _terminalPanel.SetVisibility(visible);

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
        if (UsesInternalPhysicalRead)
        {
            CommandPreview.Text = LocExtension.Get("Read.InternalPreview", target);
            return;
        }
        try { CommandPreview.Text = BuildReadCommand(target).ToDisplayString(); }
        catch (ArgumentException) { CommandPreview.Text = $"âš  {LocExtension.Get("Advanced.Invalid", LocExtension.Get("Common.Unknown"))}"; }
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

    private void RebuildFormatCatalog()
    {
        _formatWorkspace.SetCapabilities(_gwCapabilities);
        SynchronizeFormatWorkspace();
    }

    private void SynchronizeFormatWorkspace()
    {
        _gwCapabilities = _formatWorkspace.Capabilities;
        _formatCatalog = _formatWorkspace.Catalog;
        _formatDetector = _formatWorkspace.Detector;
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
        if (!_diskDefinitionsController.Validate(ReadDiskDefsEnabled, ReadDiskDefsValue, LocExtension.Get("Read.Title"))) return;
        if (string.IsNullOrWhiteSpace(ReadFileName.Text))
        {
            _dialogs.Show(LocExtension.Get("Read.NameRequired"), LocExtension.Get("Read.Title"), icon: UserDialogIcon.Information);
            return;
        }
        if (!UsesInternalPhysicalRead && (string.IsNullOrWhiteSpace(_settings.GwExecutablePath) || !File.Exists(_settings.GwExecutablePath)))
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
        if (UsesInternalPhysicalRead)
        {
            var hardware = SelectedHardware();
            if (hardware is null)
            {
                _dialogs.Show(LocExtension.Get("Hardware.NotConfigured"), LocExtension.Get("Read.Title"), icon: UserDialogIcon.Warning);
                return;
            }
            if (RawScpRadio.IsChecked != true)
            {
                _dialogs.Show(LocExtension.Get("Read.InternalRawScpOnly"), LocExtension.Get("Read.Title"), icon: UserDialogIcon.Warning);
                return;
            }
            if (HasUnsupportedInternalReadOptions())
            {
                _dialogs.Show(LocExtension.Get("Read.InternalUnsupportedOptions"), LocExtension.Get("Read.Title"), icon: UserDialogIcon.Warning);
                return;
            }
            PhysicalDiskReadOptions options;
            try { options = CreateInternalReadOptions(hardware); }
            catch (Exception exception) when (exception is ArgumentException or OverflowException)
            {
                _dialogs.Show(LocExtension.Get("Read.InternalInvalidOptions"), LocExtension.Get("Read.Title"), icon: UserDialogIcon.Warning);
                return;
            }
            await ExecuteInternalReadAsync(options, target);
            return;
        }
        GwCommand command;
        try { command = BuildReadCommand(target); }
        catch (ArgumentException) { _diskDefinitionsController.ShowInvalid(LocExtension.Get("Read.Title")); return; }
        ReadExecuteButton.Content = LocExtension.Get("Common.Stop");
        BeginProgress();
        await RenderPendingProgressAsync();
        LogOutput.Clear();
        await _consoleLog.BeginAsync("read", command.ToDisplayString());
        var output = new Progress<GwOutputLine>(ReportOutput);
        var outcome = await _operation.RunAsync(token => _runner.RunAsync(command, output, token));
        await FlushPendingOutputAsync();
        ApplyOperationResult(_operation.Present(outcome));
        if (outcome.Result is { } result)
        {
            if (result.WasCancelled)
            {
                var deletionError = CancelledOutputCleaner.TryDelete(target);
                if (deletionError is null) AppendConsoleText(Environment.NewLine + LocExtension.Get("Read.CancelledFileDeleted", target) + Environment.NewLine);
                else
                {
                    var logPath = ErrorLog.Write(deletionError, "Deleting cancelled read output");
                    var detail = logPath is null ? LocExtension.Get("Common.Unknown") : LocExtension.Get("Error.LogSaved", logPath);
                    AppendConsoleText(Environment.NewLine + LocExtension.Get("Read.CancelledFileDeleteFailed", target, detail) + Environment.NewLine);
                    _dialogs.Show(LocExtension.Get("Read.CancelledFileDeleteFailed", target, detail), LocExtension.Get("Read.Title"), icon: UserDialogIcon.Warning);
                }
            }
            if (result.IsSuccess && extension.Equals(".scp", StringComparison.OrdinalIgnoreCase))
            {
                _diskImageWorkspace.LastCapturedPath = target;
                OpenScpBanner.Visibility = Visibility.Visible;
                await AppendScpCaptureSummaryAsync(target);
            }
            if (result.IsSuccess && File.Exists(target))
            {
                await AnalyzeCompletedReadAsync(target);
            }
            var sequenceKind = ReadSequenceKind.SelectedIndex == 1 ? SequenceKind.Alphabetic : SequenceKind.Numeric;
            if (result.IsSuccess) _viewModel.Read.TryAdvanceSequence();
        }
        EndProgress(); ReadExecuteButton.Content = LocExtension.Get("Common.Execute");
    }

    private async Task ExecuteInternalReadAsync(PhysicalDiskReadOptions options, string target)
    {
        ReadExecuteButton.Content = LocExtension.Get("Common.Stop");
        BeginProgress();
        await RenderPendingProgressAsync();
        LogOutput.Clear();
        await _consoleLog.BeginAsync("read-internal", LocExtension.Get("Read.InternalPreview", target));
        var stopwatch = Stopwatch.StartNew();
        PhysicalDiskReadResult? capture = null;
        _lastInternalReadProgressLine = null;
        var outcome = await _operation.RunAsync(async token =>
        {
            var reader = InternalPhysicalDiskReader.CreateDefault();
            var progress = new Progress<PhysicalDiskReadOperationProgress>(ReportInternalReadProgress);
            capture = await reader.ReadAsync(options, target, progress, token);
            return new GwExecutionResult(0, false, stopwatch.Elapsed, []);
        });
        await FlushPendingOutputAsync();
        ApplyOperationResult(_operation.Present(outcome));
        if (outcome.Result is { WasCancelled: true })
        {
            var deletionError = CancelledOutputCleaner.TryDelete(target);
            if (deletionError is null) AppendConsoleText(Environment.NewLine + LocExtension.Get("Read.CancelledFileDeleted", target) + Environment.NewLine);
            else
            {
                var logPath = ErrorLog.Write(deletionError, "Deleting cancelled internal read output");
                var detail = logPath is null ? LocExtension.Get("Common.Unknown") : LocExtension.Get("Error.LogSaved", logPath);
                AppendConsoleText(Environment.NewLine + LocExtension.Get("Read.CancelledFileDeleteFailed", target, detail) + Environment.NewLine);
            }
        }
        if (outcome.Result?.IsSuccess == true && capture is not null)
        {
            _diskImageWorkspace.RememberReadImage(capture.Document);
            _diskImageWorkspace.LastCapturedPath = target;
            OpenScpBanner.Visibility = Visibility.Visible;
            await AppendScpCaptureSummaryAsync(target);
            _viewModel.Read.TryAdvanceSequence();
        }
        EndProgress();
        ReadExecuteButton.Content = LocExtension.Get("Common.Execute");
    }

    private void ReportInternalReadProgress(PhysicalDiskReadOperationProgress progress)
    {
        _progress.Accept(progress);
        string line;
        if (progress.Cylinder is int cylinder && progress.Head is int head)
        {
            line = LocExtension.Get(
                "Status.TrackProgress",
                cylinder,
                head,
                progress.CompletedTracks,
                progress.TotalTracks);
        }
        else
        {
            line = LocExtension.Get("Status.Running");
        }
        if (string.Equals(line, _lastInternalReadProgressLine, StringComparison.Ordinal))
        {
            return;
        }

        _lastInternalReadProgressLine = line;
        AppendConsoleText(line + Environment.NewLine);
    }

    private async Task AnalyzeCompletedReadAsync(string path)
    {
        try
        {
            await _diskImageWorkspace.AnalyzeAsync(path);
        }
        catch (Exception exception) when (exception is InvalidDataException or NotSupportedException)
        {
            AppendAnalysisFailure(exception, $"Analyzing completed disk read: {path}");
        }
    }

    private void AppendAnalysisFailure(Exception exception, string context)
    {
        var logPath = ErrorLog.Write(exception, context);
        var detail = logPath is null
            ? LocExtension.Get("Common.Unknown")
            : LocExtension.Get("Error.LogSaved", logPath);
        AppendConsoleText(Environment.NewLine);
        AppendConsoleText(LocExtension.Get("Error.Unexpected", detail));
        AppendConsoleText(Environment.NewLine);
    }

    private PhysicalDiskReadOptions CreateInternalReadOptions(HardwareChoice hardware)
    {
        var selection = GreaseweazleDriveSelectionPolicy.Resolve(hardware.Drive.Selection);
        var tracks = PhysicalDiskTrackSelectionParser.Parse(_viewModel.Read.Tracks.Enabled ? _viewModel.Read.Tracks.Value : "c=0-79:h=0-1");
        var revolutions = _viewModel.Read.Revs.Enabled ? int.Parse(_viewModel.Read.Revs.Value) : PhysicalDiskReadDefaults.Revolutions;
        var retries = _viewModel.Read.Retries.Enabled ? int.Parse(_viewModel.Read.Retries.Value) : PhysicalDiskReadDefaults.FluxOverflowRetries;
        var seekRetries = _viewModel.Read.SeekRetries.Enabled ? int.Parse(_viewModel.Read.SeekRetries.Value) : PhysicalDiskReadDefaults.SeekRetries;
        TimeSpan? fakeIndex = _viewModel.Read.FakeIndex.Enabled ? PhysicalDiskIndexPeriodParser.Parse(_viewModel.Read.FakeIndex.Value) : null;
        return new(hardware.Port, selection.BusType, selection.Unit, tracks, ScpCaptureDiskTypePolicy.Resolve(hardware.Drive.Density), revolutions, retries, seekRetries, fakeIndex, _viewModel.Read.HardSectors.Enabled);
    }

    private bool HasUnsupportedInternalReadOptions() =>
        _viewModel.Read.AdjustSpeed.Enabled ||
        _viewModel.Read.Pll.Enabled ||
        _viewModel.Read.Reverse.Enabled ||
        _viewModel.Read.Densel.Enabled ||
        _viewModel.Read.Tg43.Enabled ||
        _viewModel.Read.DiskDefs.Enabled ||
        !string.IsNullOrWhiteSpace(_viewModel.Read.ExpertArguments);

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
            UpdateWriteCommand();
            UpdateConvertCommand();
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
        RefreshExplorerFormats();
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

    private void RefreshExplorerFormats()
    {
        var selectedId = DiskExplorer.SelectedFormatId;
        DiskExplorer.SetFormats(_formatCatalog.Formats, selectedId);
        VisualizerHeader.SetFormats(_formatCatalog.Formats);
    }

    private void ShowLoggedError(Exception exception, string context, string titleKey, string messageKey = "Error.Unexpected")
    {
        var path = ErrorLog.Write(exception, context);
        var detail = path is null ? LocExtension.Get("Common.Unknown") : LocExtension.Get("Error.LogSaved", path);
        _dialogs.Show(LocExtension.Get(messageKey, detail), LocExtension.Get(titleKey), icon: UserDialogIcon.Error);
    }

    private void CaptureWindowSettings()
    {
        _windowPlacement.Capture(
            this,
            _settings,
            _terminalPanel.IsVisible,
            _terminalPanel.ActualHeight);
    }

    private void RestoreWindowPlacement() => _windowPlacement.Restore(this, _settings.Window);

    private void ConstrainToCurrentWorkArea() => _windowPlacement.ConstrainToCurrentWorkArea(this);

    private void ToolsList_SelectionChanged(object sender, SelectionChangedEventArgs e) => _maintenanceTools.UpdateSelection();

    private void ToolInput_Changed(object sender, RoutedEventArgs e) => UpdateToolCommand();

    private GwCommand BuildEraseCommand() => _maintenanceTools.BuildErase();

    private GwCommand BuildCleanCommand() => _maintenanceTools.BuildClean();

    private void RefreshHardwareSelector() => _hardwareSelection.Refresh();
    private HardwareChoice? SelectedHardware() => _hardwareSelection.Selected;
    private string? SelectedDeviceArgument() => _hardwareSelection.DeviceArgument();
    private string? SelectedDriveArgument() => _hardwareSelection.DriveArgument();
    private void HardwareSelector_Changed(object sender, SelectionChangedEventArgs e) => _hardwareSelection.OnSelectionChanged();
    private bool EnsureSelectedHardwareAvailable() => _hardwareSelection.EnsureAvailable();

    private void UpdateToolCommand() => _maintenanceTools.UpdatePreview();

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
        ApplyOperationResult(_operation.Present(outcome));
        EndProgress(); button.Content = LocExtension.Get("Common.Execute");
    }

    private void ConfirmAndRequestStop()
    {
        if (_dialogs.Show(LocExtension.Get("Operation.StopConfirm"), LocExtension.Get("Operation.StopTitle"), UserDialogButtons.YesNo, UserDialogIcon.Warning) == UserDialogResult.Yes)
            _operation.RequestCancellation();
    }

    private void BeginProgress() => _operation.Begin();

    private Task RenderPendingProgressAsync() => _operation.RenderPendingAsync();

    private void ReportOutput(GwOutputLine line) => _operation.Report(line);

    private Task FlushPendingOutputAsync() => _operation.FlushPendingAsync();

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
            var logPath = ErrorLog.Write(exception, "Reading SCP summary");
            var detail = logPath is null ? LocExtension.Get("Common.Unknown") : LocExtension.Get("Error.LogSaved", logPath);
            OpenScpSummaryText.Text = LocExtension.Get("Read.ScpSummaryUnavailable", detail);
            AppendConsoleText(Environment.NewLine + LocExtension.Get("Read.ScpSummaryUnavailable", detail) + Environment.NewLine);
        }
    }

    private void EndProgress() => _operation.End();

    private void ApplyOperationResult(OperationResultPresentation presentation) => _operation.Apply(presentation);

    private void AppendConsoleText(string text) => _operation.AppendText(text);
    private void SetOperationState(string resourceKey, Color color) => _operation.SetState(resourceKey, color);

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
