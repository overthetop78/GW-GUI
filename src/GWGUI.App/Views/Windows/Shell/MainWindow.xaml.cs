using GWGUI.Domain.Commands.Building;
using GWGUI.Domain.Commands.Execution;
using GWGUI.Domain.Formats;
using GWGUI.Domain.Formats.Detection;
using GWGUI.Domain.Hardware;
using GWGUI.Domain.HostTools;
using GWGUI.Domain.Profiles;
using GWGUI.Domain.Settings;
using GWGUI.App.Contracts.Services.Hardware;
using GWGUI.App.Contracts.Visualization;
using GWGUI.App.Controllers.MainWindow;
using GWGUI.App.Dictionaries.Options;
using GWGUI.App.Enums.Services.Dialogs;
using GWGUI.App.Interfaces.Services.Dialogs;
using GWGUI.App.Interfaces.Services.Navigation;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Functions.Localization;
using GWGUI.App.Presenters.Conversion;
using GWGUI.App.Services.Dialogs;
using GWGUI.App.Services.DiskImages;
using GWGUI.App.Services.Documentation;
using GWGUI.App.Services.Hardware;
using GWGUI.App.Services.Logging;
using GWGUI.App.Services.Maintenance;
using GWGUI.App.Services.Operations;
using GWGUI.App.Services.Profiles;
using GWGUI.App.Services.Storage;
using GWGUI.App.Services.Terminal;
using GWGUI.App.Services.Visualization;
using GWGUI.App.Services.Windows;
using GWGUI.App.ViewModels.Main;
using GWGUI.App.Views.Controls.Common;
using GWGUI.App.Views.Controls.Conversion;
using GWGUI.App.Views.Controls.Options;
using GWGUI.App.Views.Controls.Read;
using GWGUI.App.Views.Controls.Visualization;
using GWGUI.App.Views.Controls.Write;
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
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Exploration;
using GWGUI.Infrastructure.Processes;
using GWGUI.Infrastructure.Settings;
using GWGUI.Infrastructure.HostTools;
using GWGUI.Infrastructure.Hardware;



namespace GWGUI.App.Views.Windows.Shell;

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
    private TextBox CommandPreview => TerminalBlock?.CommandTextBox!;
    private TextBox LogOutput => TerminalBlock?.OutputTextBox!;
    private ComboBox ScpDecoderCombo => VisualizerHeader.DecoderCombo;
    private TextBlock HardwareStatusText => StatusBarBlock.HardwareText;
    private ComboBox HardwareSelector => StatusBarBlock.HardwareChoices;
    private StatusBarItem ProfileStatusItem => StatusBarBlock.ProfileItem;
    private ProgressBar OperationProgress => StatusBarBlock.ProgressBar;
    private TrackProgressStrip Face0TrackProgress => StatusBarBlock.Face0Progress;
    private TrackProgressStrip Face1TrackProgress => StatusBarBlock.Face1Progress;
    private readonly ISettingsStore _settingsStore;
    private readonly IGreaseweazleRunner _runner;
    private readonly IGwCommandBuilder _commandBuilder;
    private readonly IGwInstallationManager _hostTools;
    private readonly IHardwareRegistry _hardwareRegistry;
    private readonly StartupHardwareMonitor _startupHardwareMonitor;
    private readonly HostToolsUpdateController _hostToolsUpdate;
    private readonly ReadTabController _readTab;
    private readonly WriteTabController _writeTab;
    private readonly ConversionTabController _conversionTab;
    private readonly MainWindowLifecycleController _lifecycle;
    private AppSettings _settings = new();
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
    private readonly ConversionFormatPresenter _conversionFormatPresenter = new();
    private readonly FluxDecoderRegistry _fluxDecoders = new();
    private readonly ScpInspectorController _scpInspectorController;
    private readonly DiskImageWorkspaceController _diskImageWorkspace;
    private readonly ExplorerReadController _explorerRead;
    private readonly OperationProgressController _progress;
    private readonly HardwareSelectionController _hardwareSelection;
    private readonly MaintenanceToolsController _maintenanceTools;
    private readonly string _logsDirectory;
    private readonly ConsoleLogSession _consoleLog;
    private readonly TerminalPanelController _terminalPanel;
    private readonly MainWindowViewModel _viewModel;
    private GwFormatCapabilities _gwCapabilities = GwFormatCapabilities.Unknown;
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
        _viewModel = new MainWindowViewModel(LocExtension.Get("Hardware.NotConfigured"), LocExtension.Get("Status.ReadyShort"));
        _hostToolsUpdate = new HostToolsUpdateController(_hostTools, _settings, _viewModel);
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
            (key, arguments) => LocExtension.Get(key, arguments),
            _operation, _dialogs, EnsureSelectedHardwareAvailable, ConfirmAndRequestStop,
            _consoleLog, _runner, LogOutput);
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
        VisualizerHeader.ClassificationSelector.ValueChanged += (_, _) => _diskImageWorkspace.ApplyClassification();
        _readTab = new ReadTabController(
            ReadTabBlock,
            _viewModel,
            _profileController,
            () => _formatCatalog,
            () => _settings,
            _commandBuilder,
            _fileDialogs,
            _businessDialogs,
            _dialogs,
            _diskDefinitionsController,
            _operation,
            _progress,
            _consoleLog,
            _runner,
            _diskImageWorkspace,
            CommandPreview,
            LogOutput,
            SelectedDeviceArgument,
            SelectedDriveArgument,
            EnsureSelectedHardwareAvailable,
            SelectedHardware,
            ConfirmAndRequestStop,
            UpdateProfileStatus,
            UpdateReadCommand);
        _writeTab = new WriteTabController(
            WriteTabBlock, _viewModel, _profileController, () => _formatCatalog, () => _formatDetector,
            () => _settings, _commandBuilder, _fileDialogs, _dialogs, _diskDefinitionsController,
            _operation, _progress, _consoleLog, _runner, _diskImageWorkspace, ReadFolder,
            CommandPreview, LogOutput, () => MainTabs?.SelectedIndex ?? -1, index => MainTabs.SelectedIndex = index,
            SelectedDeviceArgument, SelectedDriveArgument, EnsureSelectedHardwareAvailable, SelectedHardware,
            ConfirmAndRequestStop, path => _diskImageWorkspace.LoadAsync(path), _diskImageWorkspace.LoadScpAsync,
            async path => { await _diskImageWorkspace.LoadExplorerAsync(path); }, AppendAnalysisFailure, UpdateProfileStatus);
        _conversionTab = new ConversionTabController(
            this, ConvertTabBlock, _viewModel, _profileController, _conversionFormatPresenter,
            () => _formatCatalog, () => _formatDetector, () => _settings, _commandBuilder, _runner,
            _fileDialogs, _businessDialogs, _dialogs, _diskDefinitionsController, _operation, _consoleLog,
            _diskImageWorkspace, ReadFolder, CommandPreview, LogOutput, () => MainTabs?.SelectedIndex ?? -1,
            index => MainTabs.SelectedIndex = index, path => _diskImageWorkspace.LoadAsync(path),
            ConfirmAndRequestStop, AppendAnalysisFailure, UpdateProfileStatus, Dispatcher);
        _explorerRead = new ExplorerReadController(
            DiskExplorer,
            LogOutput,
            _settings,
            _dialogs,
            _hardwareSelection,
            _operation,
            _progress,
            _commandBuilder,
            _runner,
            _consoleLog,
            path => _diskImageWorkspace.LoadAsync(path),
            ShowLoggedError);
        VisualizerHeader.OpenButton.Click += async (_, _) =>
        {
            var path = _diskImageWorkspace.SelectImage();
            if (path is not null) await _diskImageWorkspace.LoadAsync(path);
        };
        _settingsStore = settingsStore ?? new JsonSettingsStore(Path.Combine(directory, "settings.json"));
        _startupHardwareMonitor = new StartupHardwareMonitor(_hardwareRegistry, _settingsStore);
        _lifecycle = new MainWindowLifecycleController(
            this, () => _settings, value => _settings = value, _settingsProvidedAtStartup,
            _settingsStore, _startupHardwareMonitor, _dialogs, _businessDialogs, _navigation,
            _operation, _diskImageWorkspace, _viewModel, value => EmulationBlock.Configure(value),
            () => EmulationBlock.StopAllAsync(),
            async () =>
            {
                if (!string.IsNullOrWhiteSpace(_settings.GwExecutablePath))
                    _formatWorkspace.SetCapabilities(await new GwFormatCapabilityReader().ReadAsync(_settings.GwExecutablePath));
            },
            () =>
            {
                SynchronizeFormatWorkspace(); _diskDefinitionsController.LoadConfigured(); RebuildFormatCatalog(); RefreshExplorerFormats();
                ScpDecoderCombo.ItemsSource = new[] { new ScpDecoderChoice(null, LocExtension.Get("Visual.Automatic")) }
                    .Concat(_fluxDecoders.Decoders.Select(x => new ScpDecoderChoice(x.Id, DecoderName(x.Id)))).ToArray();
                ScpDecoderCombo.SelectedIndex = 0;
            },
            () =>
            {
                ReadFamilyCombo.ItemsSource = _formatCatalog.Formats.Where(x => x.Family != "Raw").Select(x => x.Family).Distinct().Order().ToArray();
                ReadFamilyCombo.SelectedIndex = 0;
            },
            () => BuildConversionFormats(null), LoadProfileStores, RestoreWindowPlacement, ConstrainToCurrentWorkArea,
            () => RefreshReadProfiles(), () => RefreshWriteProfiles(), () => RefreshConvertProfiles(),
            RestoreReadSettings, RestoreWriteSettings, RestoreConversionSettings, RefreshHardwareSelector,
            _terminalPanel.SetVisibility, UpdateReadCommand, UpdateWriteCommand, UpdateConvertCommand,
            UpdateProfileStatus, CheckHostToolsUpdateAsync,
            CaptureWindowSettings, CaptureReadSettings, CaptureWriteSettings, CaptureProfiles,
            CaptureConversionSettings, () => ((App)Application.Current).SetTheme(_settings.Theme));
    }

    private void ConnectMainMenu()
    {
        ApplicationMenu.PreferencesRequested += Preferences_Click;
        ApplicationMenu.LogHistoryRequested += (_, _) => _navigation.ShowLogHistory(_logsDirectory);
        ApplicationMenu.DocumentationRequested += Documentation_Click;
        ApplicationMenu.AboutRequested += (_, _) => _navigation.ShowAbout();
        ApplicationMenu.ToolRequested += (sender, verb) => ToolCommand_Click(sender, new RoutedEventArgs());

        RegisterName("OptionsMenuItem", ApplicationMenu.OptionsMenuItem);
        RegisterName("HelpMenuItem", ApplicationMenu.HelpMenuItem);
        RegisterName("AlignMenuItem", ApplicationMenu.AlignMenuItem);
    }

    private void ConnectStatusBar()
    {
        StatusBarBlock.HardwareSelectionChanged += HardwareSelector_Changed;
        StatusBarBlock.HostToolsUpdateRequested += Preferences_Click;
        StatusBarBlock.ToggleConsoleRequested += (_, _) => _terminalPanel.Toggle();
        RegisterName(nameof(HardwareStatusText), HardwareStatusText);
        RegisterName(nameof(HardwareSelector), HardwareSelector);
        RegisterName(nameof(OperationProgress), OperationProgress);
    }

    private void ConnectReadComponents()
    {
        RawScpRadio.Checked += (_, _) => _readTab.ModeChanged();
        KnownFormatRadio.Checked += (_, _) => _readTab.ModeChanged();
        ReadFamilyCombo.SelectionChanged += (_, _) => _readTab.FamilyChanged();
        ReadFormatCombo.SelectionChanged += (_, _) => _readTab.FormatChanged();
        ReadExtensionCombo.SelectionChanged += (_, _) => _readTab.InputChanged();
        ReadProfileCombo.SelectionChanged += (_, _) => _readTab.ProfileChanged();
        ReadProfileBlock.SaveButton.Click += (_, _) => _readTab.SaveProfile();
        ReadProfileBlock.ResetButton.Click += (_, _) => _readTab.ResetProfile();
        ReadFolderBlock.BrowseButton.Click += (_, _) => _readTab.BrowseFolder();
        ReadFileName.TextChanged += ReadInput_Changed;
        ReadAdvancedBlock.InputChanged += ReadInput_Changed;
        ReadAdvancedBlock.FakeIndexChecked += (_, _) => _readTab.EnableFakeIndex();
        ReadAdvancedBlock.HardSectorsChecked += (_, _) => _readTab.EnableHardSectors();
        ReadAdvancedBlock.DenselChecked += (_, _) => _readTab.EnableDensel();
        ReadAdvancedBlock.Tg43Checked += (_, _) => _readTab.EnableTg43();
        ReadAdvancedBlock.SequenceKindChanged += (_, _) => _readTab.ChangeSequenceKind();
        ReadCompletionBlock.ExploreRequested += async (_, _) =>
        {
            if (_diskImageWorkspace.LastCapturedPath is null) return;
            MainTabs.SelectedIndex = 4;
            await _diskImageWorkspace.LoadAsync(_diskImageWorkspace.LastCapturedPath);
        };
        ReadCompletionBlock.VisualizeRequested += async (_, _) =>
        {
            if (_diskImageWorkspace.LastCapturedPath is null) return;
            MainTabs.SelectedIndex = 3;
            await _diskImageWorkspace.LoadAsync(_diskImageWorkspace.LastCapturedPath);
        };
        ReadTabBlock.ExecuteRequested += ExecuteRead_Click;
        TerminalBlock.CopyButton.Click += (_, _) => _terminalPanel.CopyToClipboard();

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
        WriteProfileCombo.SelectionChanged += (_, _) => _writeTab.ProfileChanged();
        WriteProfileBlock.SaveButton.Click += (_, _) => _writeTab.SaveProfile();
        WriteProfileBlock.ResetButton.Click += (_, _) => _writeTab.ResetProfile();
        WriteSourceBlock.BrowseButton.Click += async (_, _) => await _writeTab.BrowseSourceAsync();
        WriteFormatBlock.ModifyButton.Click += (_, _) => _writeTab.ToggleFormat();
        WriteFormatBlock.VisualizeTracksButton.Click += async (_, _) => await _writeTab.VisualizeSourceAsync();
        WriteFormatCombo.SelectionChanged += WriteInput_Changed;
        WriteAdvancedBlock.InputChanged += WriteInput_Changed;
        WriteAdvancedBlock.FakeIndexChecked += (_, _) => _writeTab.EnableFakeIndex();
        WriteAdvancedBlock.HardSectorsChecked += (_, _) => _writeTab.EnableHardSectors();
        WriteAdvancedBlock.DenselChecked += (_, _) => _writeTab.EnableDensel();
        WriteAdvancedBlock.Tg43Checked += (_, _) => _writeTab.EnableTg43();
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
        ConvertProfileCombo.SelectionChanged += (_, _) => _conversionTab.ProfileChanged();
        ConvertProfileBlock.SaveButton.Click += (_, _) => _conversionTab.SaveProfile();
        ConvertProfileBlock.ResetButton.Click += (_, _) => _conversionTab.ResetProfile();
        ConvertSourceBlock.BrowseButton.Click += async (_, _) => await _conversionTab.BrowseSourceAsync();
        ConvertSourceBlock.ActionButton.Click += async (_, _) => await _conversionTab.VisualizeSourceAsync();
        ConvertOutputBlock.ValueChanged += (_, _) => _conversionTab.UpdateCommand();
        ConvertFormatsBlock.ValueChanged += (sender, _) => _conversionTab.SelectionChanged(sender);
        ConvertAdvancedBlock.InputChanged += (_, _) => _conversionTab.UpdateCommand();
        ConvertTabBlock.ExecuteRequested += ExecuteConvert_Click;
        ConvertTabBlock.MigrationRequested += (_, _) => _conversionTab.OpenMigration();
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
        ToolsTabBlock.ToolSelectionChanged += (_, _) => _maintenanceTools.UpdateSelection();
        ToolsTabBlock.InputChanged += (_, _) => _maintenanceTools.UpdatePreview();
        ToolsTabBlock.EraseRequested += async (_, _) => await _maintenanceTools.ExecuteEraseAsync();
        ToolsTabBlock.CleanRequested += async (_, _) => await _maintenanceTools.ExecuteCleanAsync();
        RegisterName(nameof(EraseExecuteButton), EraseExecuteButton);
        RegisterName(nameof(CleanExecuteButton), CleanExecuteButton);
    }

    private void ConnectExplorerComponent()
    {
        DiskExplorer.OpenRequested += async (_, _) =>
        {
            var path = _diskImageWorkspace.SelectImage();
            if (path is not null) await _diskImageWorkspace.LoadAsync(path);
        };
        DiskExplorer.ReadDiskRequested += async (_, _) => await _explorerRead.ExecuteAsync();
        DiskExplorer.FormatChanged += async (_, _) =>
        {
            if (!string.IsNullOrWhiteSpace(_diskImageWorkspace.ExplorerPath)) await _diskImageWorkspace.LoadExplorerAsync(_diskImageWorkspace.ExplorerPath, false);
        };
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e) => await _lifecycle.LoadAsync();

    private void MainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MainTabs?.SelectedIndex == 1) UpdateWriteCommand();
        else if (MainTabs?.SelectedIndex == 0) UpdateReadCommand();
        else if (MainTabs?.SelectedIndex == 2) UpdateConvertCommand();
        else if (MainTabs?.SelectedIndex == 5) UpdateToolCommand();
        UpdateProfileStatus();
    }

    private void RefreshWriteProfiles(string? selectedId = null)
        => _writeTab.RefreshProfiles(selectedId);

    private void RefreshConvertProfiles(string? selectedId = null)
        => _conversionTab.RefreshProfiles(selectedId);

    private void WriteInput_Changed(object sender, RoutedEventArgs e) => _writeTab.UpdateCommand();

    private void UpdateWriteCommand() => _writeTab.UpdateCommand();

    private async void ExecuteWrite_Click(object sender, RoutedEventArgs e)
        => await _writeTab.ExecuteAsync();

    private void BuildConversionFormats(string? sourceExtension, DetectedImageFormat? detection = null)
        => _conversionTab.BuildFormats(sourceExtension, detection);

    private void UpdateConvertCommand() => _conversionTab.UpdateCommand();

    private async void ExecuteConvert_Click(object sender, RoutedEventArgs e)
        => await _conversionTab.ExecuteAsync();

    private void CaptureConversionSettings() => _conversionTab.CaptureSettings();

    private void RestoreConversionSettings() => _conversionTab.RestoreSettings();

    private void Window_Closing(object? sender, CancelEventArgs e) => _lifecycle.Closing(e);

    private void RefreshReadProfiles(string? selectedId = null)
        => _readTab.RefreshProfiles(selectedId);

    private void CaptureProfiles()
    {
        _settings.Profiles = _profiles.Capture();
    }

    private void LoadProfileStores()
    {
        _profiles.Reset(_settings.Profiles);
    }

    private void Documentation_Click(object sender, RoutedEventArgs e)
    {
        var language = System.Globalization.CultureInfo.CurrentUICulture.Name;
        var path = UserGuideLocator.Find(AppContext.BaseDirectory, language);
        if (path is not null) Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
    }

    private void ReadInput_Changed(object sender, RoutedEventArgs e) => _readTab.InputChanged();

    private void UpdateReadExtension() => _readTab.UpdateExtension();

    private void UpdateReadCommand() => _readTab.UpdateCommand();

    private static string SelectedText(ComboBox combo) => (combo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty;


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

    private async void ExecuteRead_Click(object sender, RoutedEventArgs e)
        => await _readTab.ExecuteAsync();

    private void AppendAnalysisFailure(Exception exception, string context)
    {
        ErrorLog.Write(exception, context);
        var detail = ExceptionDescriptionFunctions.Describe(exception);
        _operation.AppendText(Environment.NewLine);
        _operation.AppendText(LocExtension.Get("Error.Unexpected", detail));
        _operation.AppendText(Environment.NewLine);
    }

    private void ShowAdvancedValidation(Exception exception, string title)
    {
        _diskDefinitionsController.ShowInvalid(exception, title);
    }

    private void BrowseReadFolder_Click(object sender, RoutedEventArgs e) => _readTab.BrowseFolder();

    private void SaveReadProfile_Click(object sender, RoutedEventArgs e) => _readTab.SaveProfile();

    private void SaveWriteProfile_Click(object sender, RoutedEventArgs e) => _writeTab.SaveProfile();

    private void LogHistory_Click(object sender, RoutedEventArgs e) => _navigation.ShowLogHistory(_logsDirectory);

    private void About_Click(object sender, RoutedEventArgs e) => _navigation.ShowAbout();

    private void RestoreReadSettings() => _readTab.RestoreSettings();

    private void CaptureReadSettings() => _readTab.CaptureSettings();

    private void RestoreWriteSettings()
        => _writeTab.RestoreSettings();

    private void CaptureWriteSettings()
        => _writeTab.CaptureSettings();

    private async void Preferences_Click(object sender, RoutedEventArgs e)
        => await _lifecycle.ShowPreferencesAsync();

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
        if (!_operation.IsRunning) _operation.SetState("Status.ReadyShort", Color.FromRgb(136, 136, 136));
        ShowHostToolsUpdateIfNeeded();
        EmulationBlock.RefreshLocalizedContent();
    }

    private void RefreshExplorerFormats()
    {
        var selectedId = DiskExplorer.SelectedFormatId;
        DiskExplorer.SetFormats(_formatCatalog.Formats, selectedId);
        VisualizerHeader.SetFormats(_formatCatalog.Formats);
    }

    private void ShowLoggedError(Exception exception, string context, string titleKey, string messageKey = "Error.Unexpected")
    {
        ErrorLog.Write(exception, context);
        var detail = ExceptionDescriptionFunctions.Describe(exception);
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

    private void RefreshHardwareSelector() => _hardwareSelection.Refresh();
    private HardwareChoice? SelectedHardware() => _hardwareSelection.Selected;
    private string? SelectedDeviceArgument() => _hardwareSelection.DeviceArgument();
    private string? SelectedDriveArgument() => _hardwareSelection.DriveArgument();
    private void HardwareSelector_Changed(object sender, SelectionChangedEventArgs e) => _hardwareSelection.OnSelectionChanged();
    private bool EnsureSelectedHardwareAvailable() => _hardwareSelection.EnsureAvailable();

    private void UpdateToolCommand() => _maintenanceTools.UpdatePreview();

    private void ConfirmAndRequestStop()
    {
        if (_dialogs.Show(LocExtension.Get("Operation.StopConfirm"), LocExtension.Get("Operation.StopTitle"), UserDialogButtons.YesNo, UserDialogIcon.Warning) == UserDialogResult.Yes)
            _operation.RequestCancellation();
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

    private Task CheckHostToolsUpdateAsync() => _hostToolsUpdate.CheckAsync();

    private void ShowHostToolsUpdateIfNeeded() => _hostToolsUpdate.Refresh();

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
