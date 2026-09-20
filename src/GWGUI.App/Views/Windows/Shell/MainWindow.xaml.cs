using GWGUI.Infrastructure.Commands.Building;
using GWGUI.Infrastructure.Commands.Execution;
using GWGUI.MediaEngine.Images.Formats;
using GWGUI.MediaEngine.Images.Formats.Detection;
using ImageFormatWorkspace = GWGUI.MediaEngine.Images.Formats.ImageFormatWorkspace;
using GWGUI.Infrastructure.Hardware;
using GWGUI.Infrastructure.HostTools;
using GWGUI.App.Profiles;
using GWGUI.Infrastructure.Settings;
using GWGUI.App.Contracts.Services.Hardware;
using GWGUI.App.Contracts.Dialogs;
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
using GWGUI.App.Services.Emulation;
using GWGUI.App.Services.Hardware;
using GWGUI.App.Services.Logging;
using GWGUI.App.Services.Maintenance;
using GWGUI.App.Services.Operations;
using GWGUI.App.Services.Profiles;
using GWGUI.App.Services.Storage;
using GWGUI.App.Services.Terminal;
using GWGUI.App.Services.Visualization;
using GWGUI.App.Services.Windows;
using GWGUI.App.Services.Updates;
using GWGUI.App.ViewModels.Main;
using GWGUI.App.Views.Controls.Common;
using GWGUI.App.Views.Controls.Conversion;
using GWGUI.App.Views.Controls.Options;
using GWGUI.App.Views.Controls.Read;
using GWGUI.App.Views.Controls.Visualization;
using GWGUI.App.Views.Controls.Write;
using GWGUI.App.Views.Dialogs.Common;
using GWGUI.MediaEngine.Images.Visualization;
using System.ComponentModel;
using GWGUI.MediaEngine.Contracts.Explorer;
using System.IO;
using System.Net.Http;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Composition;
using GWGUI.MediaEngine.Images.Reading.Decoding;
using GWGUI.MediaEngine.Images.Reading;
using GWGUI.Infrastructure.Processes;
using GWGUI.App.Constants.Views.Shell;
namespace GWGUI.App.Views.Windows.Shell;

public partial class MainWindow : Window
{
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
    private readonly PendingModuleInstallationStore _pendingModuleInstallations;
    private readonly ImageFormatWorkspace _formatWorkspace;
    private readonly DiskDefinitionsController _diskDefinitionsController;
    private readonly WindowPlacementController _windowPlacement = new();
    private IImageFormatCatalog _formatCatalog = null!;
    private readonly OperationProfileCollection _profiles = new();
    private readonly OperationProfileController _profileController;
    private ImageFormatDetector _formatDetector = null!;
    private readonly ConversionFormatPresenter _conversionFormatPresenter = new();
    private readonly FluxDecoderRegistry _fluxDecoders = new();
    private readonly MediaEngineComposition _mediaEngine;
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
    private readonly Action<string> _openDocumentation;
    private readonly Action<Exception, string> _writeError;

    public MainWindow() : this(null, null, null, null, null, null, null, null, null, null) { }

    public MainWindow(IMessageDialogService? dialogs, IFileDialogService? fileDialogs = null, IBusinessDialogService? businessDialogs = null, IWindowNavigationService? navigation = null, IGwCommandBuilder? commandBuilder = null, IGwInstallationManager? hostTools = null, IGreaseweazleRunner? runner = null, ISettingsStore? settingsStore = null, IHardwareRegistry? hardwareRegistry = null, AppSettings? initialSettings = null, Action<string>? openDocumentation = null, Action<Exception, string>? logError = null, string? dataDirectory = null)
    {
        _openDocumentation = openDocumentation ?? (url => Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }));
        _writeError = logError ?? ((error, context) => ErrorLog.Write(error, context));
        InitializeComponent();
        _mediaEngine = MediaEngineComposition.CreateDefault();
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
        var directory = dataDirectory ?? StoragePaths.DataDirectory;
        _logsDirectory = Path.Combine(directory, MainWindowConstants.LogsDirectoryName);
        _consoleLog = new ConsoleLogSession(_logsDirectory, () => _settings.Logging);
        _terminalPanel = new TerminalPanelController(TerminalBlock, ConsoleRow, ConsoleSplitter, _settings);
        _runner = runner ?? new GreaseweazleRunner();
        _hardwareRegistry = hardwareRegistry ?? new GreaseweazleHardwareRegistry(new WindowsSerialDeviceDiscovery(), _runner, _commandBuilder);
        _pendingModuleInstallations = new PendingModuleInstallationStore();
        _navigation = navigation ?? new WpfWindowNavigationService(this, _hostTools, _runner, _commandBuilder,
            _pendingModuleInstallations);
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
            () => MainTabs?.SelectedIndex == MainWindowConstants.ToolsTabIndex,
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
            new ScpDocumentLoader(_mediaEngine.Recognition.ScpReader, (key, arguments) => LocExtension.Get(key, arguments)),
            DiskImageExplorer.CreateDefault(),
            diskImageCancellation,
            () => _operation.IsRunning,
            ShowLoggedError,
            (key, arguments) => LocExtension.Get(key, arguments),
            mediaReader: _mediaEngine.ReadingService,
            mediaExplorer: _mediaEngine.Explorer,
            visualizationProviders: _mediaEngine.Visualization.Registry);
        VisualizerHeader.ClassificationSelector.ValueChanged += (_, _) => _diskImageWorkspace.ApplyClassification();
        VisualizerHeader.ClassificationFormatChanged += async (_, formatId) =>
            await _diskImageWorkspace.SelectVisualizerRepresentationAsync(formatId);
        VisualizerHeader.RepresentationChoiceChanged += async (_, formatId) =>
            await _diskImageWorkspace.SelectVisualizerRepresentationAsync(formatId);
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
            _mediaEngine.ReadingService, _mediaEngine.ConversionService, _mediaEngine.SequentialConversionService,
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
        VisualizerTabBlock.OpenRequested += async (_, _) =>
        {
            var path = _diskImageWorkspace.SelectVisualizerImage();
            if (path is not null) await _diskImageWorkspace.LoadAsync(path);
        };
        _settingsStore = settingsStore ?? new JsonSettingsStore(
            Path.Combine(directory, MainWindowConstants.SettingsFileName));
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
        ReadFamilyCombo.ItemsSource = _formatCatalog.Formats
            .Where(x => x.Family != MainWindowConstants.RawFormatFamily)
            .Select(x => x.Family).Distinct().Order().ToArray();
                ReadFamilyCombo.SelectedIndex = 0;
            },
            () => BuildConversionFormats(null), LoadProfileStores, RestoreWindowPlacement, ConstrainToCurrentWorkArea,
            () => RefreshReadProfiles(), () => RefreshWriteProfiles(), () => RefreshConvertProfiles(),
            RestoreReadSettings, RestoreWriteSettings, RestoreConversionSettings, RefreshHardwareSelector,
            _terminalPanel.SetVisibility, UpdateReadCommand, UpdateWriteCommand, UpdateConvertCommand,
            UpdateProfileStatus, CheckHostToolsUpdateAsync,
            CaptureWindowSettings, CaptureReadSettings, CaptureWriteSettings, CaptureProfiles,
            CaptureConversionSettings, () => ((App)Application.Current).SetTheme(_settings.Theme),
            _pendingModuleInstallations.Clear);
    }

}
