using GWGUI.Domain.Hardware;
using GWGUI.Domain.HostTools;
using GWGUI.Domain.Settings;
using GWGUI.Domain.Settings.Hardware;
using GWGUI.App.Enums.Services.Navigation;
using GWGUI.App.Functions.Options.Tags;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Options.Controllers;
using GWGUI.App.Options.States;
using GWGUI.App.Services.Logging;
using GWGUI.App.Services.Storage;
using GWGUI.App.ViewModels.Options;
using GWGUI.App.Views.Controls.Emulation.Machine;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using GWGUI.Infrastructure.Hardware;
using GWGUI.Infrastructure.Processes;
using GWGUI.Infrastructure.Settings;
using GWGUI.Infrastructure.HostTools;



namespace GWGUI.App.Views.Windows.Options;

public partial class OptionsWindow : Window
{
    private ScrollViewer GeneralScrollViewer => GeneralSection.Scroller;
    private TextBox ImagesFolderText => GeneralSection.ImagesFolder;
    private ComboBox LanguageCombo => GeneralSection.Languages;
    private ComboBox ThemeCombo => GeneralSection.Themes;
    private CheckBox UseTagsCheck => GeneralSection.UseTags;
    private ComboBox TagPresetCombo => GeneralSection.TagPresets;
    private TextBox TagPatternText => GeneralSection.TagPattern;
    private ListBox RecentTagPatterns => GeneralSection.RecentTagPatternsList;
    private ItemsControl TagVariablesList => GeneralSection.TagVariables;
    private TextBlock TagPatternPreview => GeneralSection.TagPreview;
    private ItemsControl LogOptionsList => LogsSection.OptionsList;
    private TextBlock LogsDirectoryText => LogsSection.DirectoryText;
    private Button ScanButton => HardwareSection.ScanAction;
    private Button AddDriveButton => HardwareSection.AddDriveAction;
    private ListBox DrivesGrid => HardwareSection.Drives;
    private TextBox GwPathText => HardwareSection.GwPath;
    private Button DownloadHostToolsButton => HardwareSection.DownloadAction;
    private ProgressBar HostToolsProgress => HardwareSection.DownloadProgress;
    private TextBlock HostToolsStatus => HardwareSection.HostToolsState;
    private ListBox ReadProfilesList => ProfilesSection.ReadProfiles;
    private ListBox WriteProfilesList => ProfilesSection.WriteProfiles;
    private ListBox ConvertProfilesList => ProfilesSection.ConvertProfiles;
    private readonly AppSettings _settings;
    private readonly HardwareOptionsState _hardwareState;
    private readonly ProfileOptionsState _profileState;
    private readonly ProfileOptionsController _profileOptionsController;
    private readonly GeneralOptionsController _generalOptionsController;
    private readonly TagOptionsController _tagOptionsController;
    private readonly LoggingOptionsController _loggingOptionsController;
    private readonly HardwareOptionsController _hardwareOptionsController;
    private readonly EngineOptionsController _engineOptionsController;
    private readonly List<ControllerSettings> _controllers;
    private readonly List<ControllerSettings> _unconfiguredControllers;
    private readonly List<DriveSettings> _drives;
    private readonly HostToolsOptionsController _hostToolsOptionsController;
    private readonly IHardwareRegistry _hardwareRegistry;
    private bool _initializing = true;
    private readonly ISettingsStore _settingsStore;
    private readonly SemaphoreSlim _saveLock = new(1, 1);
    private bool _closingAfterSave;
    private bool _closeInProgress;
    public ObservableCollection<HardwareRow> Hardware { get; } = [];
    public ObservableCollection<ProfileOptionRow> ReadProfiles => _profileOptionsController.Read;
    public ObservableCollection<ProfileOptionRow> WriteProfiles => _profileOptionsController.Write;
    public ObservableCollection<ProfileOptionRow> ConvertProfiles => _profileOptionsController.Convert;
    public ObservableCollection<LogOptionRow> LogOptions => _loggingOptionsController.Options;
    public OptionsWindow(AppSettings settings, IHardwareRegistry? hardwareRegistry = null, IGwInstallationManager? hostTools = null, OptionsSection section = OptionsSection.General, ISettingsStore? settingsStore = null)
    {
        InitializeComponent();
        ConnectSections();
        _settings = settings;
        _generalOptionsController = new GeneralOptionsController(
            this,
            GeneralSection,
            settings,
            () => _initializing,
            PersistSettingsAsync,
            RefreshLocalizedContent,
            (key, arguments) => LocExtension.Get(key, arguments));
        EmulationSection.Configure(settings, PersistSettingsAsync);
        _profileState = new ProfileOptionsState(settings.Profiles);
        _profileOptionsController = new ProfileOptionsController(
            this,
            ProfilesSection,
            _profileState,
            PersistSettingsAsync,
            (key, arguments) => LocExtension.Get(key, arguments));
        _tagOptionsController = new TagOptionsController(
            GeneralSection,
            settings,
            () => _initializing,
            PersistSettingsAsync,
            (key, arguments) => LocExtension.Get(key, arguments));
        _loggingOptionsController = new LoggingOptionsController(
            LogsSection,
            settings,
            () => _initializing,
            PersistSettingsAsync,
            key => LocExtension.Get(key),
            exception => ShowLoggedError(exception, "Opening Logs folder", "Error.Title", MessageBoxImage.Warning));
        _settingsStore = settingsStore ?? new JsonSettingsStore(Path.Combine(StoragePaths.DataDirectory, "settings.json"));
        var managedRoot = StoragePaths.HostToolsDirectory;
        var hostToolsManager = hostTools ?? new GwInstallationManager(new HttpClient(), managedRoot);
        _hostToolsOptionsController = new HostToolsOptionsController(
            this,
            HardwareSection,
            settings,
            hostToolsManager,
            PersistSettingsAsync,
            exception => ShowLoggedError(exception, "Managing Host Tools", "HostTools.Title", MessageBoxImage.Error),
            (key, arguments) => LocExtension.Get(key, arguments));
        _hardwareRegistry = hardwareRegistry ?? new GreaseweazleHardwareRegistry(new WindowsSerialDeviceDiscovery(), new GreaseweazleRunner());
        _hardwareState = new HardwareOptionsState(settings);
        _controllers = _hardwareState.Controllers;
        _unconfiguredControllers = _hardwareState.UnconfiguredControllers;
        _drives = _hardwareState.Drives;
        _hardwareOptionsController = new HardwareOptionsController(
            this,
            HardwareSection,
            _hardwareState,
            Hardware,
            _hardwareRegistry,
            () => _hostToolsOptionsController.CurrentPath,
            PersistSettingsAsync,
            ShowLoggedError);
        _engineOptionsController = new EngineOptionsController(
            EnginesSection,
            settings.Engines,
            () => _initializing,
            PersistSettingsAsync);
        _hardwareOptionsController.Initialize();
        Navigation.SelectedIndex = section switch
        {
            OptionsSection.Logs => 1,
            OptionsSection.Hardware or OptionsSection.HostTools => 2,
            OptionsSection.Engines => 3,
            OptionsSection.Profiles => 4,
            OptionsSection.Emulation => 5,
            _ => 0
        };
        _initializing = false;
    }

    private void ConnectSections()
    {
        RegisterSectionNames();

        HardwareSection.ScanRequested += ScanHardware_Click;
        HardwareSection.AddDriveRequested += AddDrive_Click;
        HardwareSection.SaveDriveRequested += SaveHardwareRow_Click;
        HardwareSection.ForgetDriveRequested += ForgetHardwareRow_Click;
        HardwareSection.AutoSaveTextEditingFinished += AutoSaveText_LostKeyboardFocus;

    }

    private void RegisterSectionNames()
    {
        RegisterName(nameof(GeneralScrollViewer), GeneralScrollViewer);
        RegisterName(nameof(ImagesFolderText), ImagesFolderText);
        RegisterName(nameof(LanguageCombo), LanguageCombo);
        RegisterName(nameof(ThemeCombo), ThemeCombo);
        RegisterName(nameof(UseTagsCheck), UseTagsCheck);
        RegisterName(nameof(TagPresetCombo), TagPresetCombo);
        RegisterName(nameof(TagPatternText), TagPatternText);
        RegisterName(nameof(RecentTagPatterns), RecentTagPatterns);
        RegisterName(nameof(TagVariablesList), TagVariablesList);
        RegisterName(nameof(TagPatternPreview), TagPatternPreview);
        RegisterName(nameof(LogOptionsList), LogOptionsList);
        RegisterName(nameof(LogsDirectoryText), LogsDirectoryText);
        RegisterName(nameof(ScanButton), ScanButton);
        RegisterName(nameof(AddDriveButton), AddDriveButton);
        RegisterName(nameof(DrivesGrid), DrivesGrid);
        RegisterName(nameof(GwPathText), GwPathText);
        RegisterName(nameof(DownloadHostToolsButton), DownloadHostToolsButton);
        RegisterName(nameof(HostToolsProgress), HostToolsProgress);
        RegisterName(nameof(HostToolsStatus), HostToolsStatus);
        RegisterName(nameof(ReadProfilesList), ReadProfilesList);
        RegisterName(nameof(WriteProfilesList), WriteProfilesList);
        RegisterName(nameof(ConvertProfilesList), ConvertProfilesList);
    }

    internal void RefreshLocalizedContent()
    {
        _hostToolsOptionsController.RefreshLocalizedContent();
        _tagOptionsController.RefreshLocalizedContent();
        _loggingOptionsController.RefreshLocalizedContent();
        _hardwareOptionsController.RefreshRows();
        EmulationSection.RefreshLocalizedContent();
    }

    internal static string RenderTagPattern(string pattern, string name, string family, string format, string extension, DateTime timestamp) =>
        TagPatternFormatter.Render(pattern, name, family, format, extension, timestamp);

    private async void AutoSaveText_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => await PersistSettingsAsync();

    private async void ScanHardware_Click(object sender, RoutedEventArgs e) => await _hardwareOptionsController.ScanAsync();

    private void AddDrive_Click(object sender, RoutedEventArgs e) => _hardwareOptionsController.AddDrive();

    private async void SaveHardwareRow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: HardwareRow row }) return;
        await _hardwareOptionsController.SaveAsync(row);
    }

    private async void ForgetHardwareRow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: HardwareRow row }) return;
        await _hardwareOptionsController.ForgetAsync(row);
    }

    private void RemoveHardwareRow(HardwareRow row) => _hardwareOptionsController.Remove(row);

    private void RefreshHardwareRows() => _hardwareOptionsController.RefreshRows();

    internal void MergeUnconfigured(IReadOnlyList<ControllerSettings> detectedControllers)
    {
        _hardwareOptionsController.MergeUnconfigured(detectedControllers);
    }

    private void ApplyControlsToSettings()
    {
        _generalOptionsController.ApplyTo(_settings);
        _hostToolsOptionsController.ApplyTo(_settings);
        _tagOptionsController.ApplyTo(_settings);
        _hardwareOptionsController.ApplyTo(_settings);
        _engineOptionsController.ApplyTo(_settings);
        _profileOptionsController.ApplyTo(_settings);
    }

    private async Task PersistSettingsAsync()
    {
        if (_initializing) return;
        ApplyControlsToSettings();
        await _saveLock.WaitAsync().ConfigureAwait(false);
        try { await _settingsStore.SaveAsync(_settings).ConfigureAwait(false); }
        finally { _saveLock.Release(); }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => BeginClose();

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        if (_initializing || _closingAfterSave) return;
        e.Cancel = true;
        BeginClose();
    }

    private void ShowLoggedError(Exception exception, string context, string titleKey, MessageBoxImage icon)
    {
        var path = ErrorLog.Write(exception, context);
        var detail = path is null ? LocExtension.Get("Common.Unknown") : LocExtension.Get("Error.LogSaved", path);
        MessageBox.Show(this, LocExtension.Get("Error.Unexpected", detail), LocExtension.Get(titleKey), MessageBoxButton.OK, icon);
    }

    private void BeginClose()
    {
        if (_closeInProgress) return;
        _closeInProgress = true;
        _ = SaveAndCloseAsync();
    }

    private async Task SaveAndCloseAsync()
    {
        try { await PersistSettingsAsync().ConfigureAwait(false); }
        catch (Exception exception)
        {
            var path = ErrorLog.Write(exception, "Saving Options while closing");
            var detail = path is null ? LocExtension.Get("Common.Unknown") : LocExtension.Get("Error.LogSaved", path);
            try
            {
                if (!Dispatcher.HasShutdownStarted && !Dispatcher.HasShutdownFinished)
                    await Dispatcher.InvokeAsync(() => { if (IsLoaded) MessageBox.Show(this, LocExtension.Get("Error.SaveFailed", detail), LocExtension.Get("Error.Title"), MessageBoxButton.OK, MessageBoxImage.Warning); }).Task.ConfigureAwait(false);
            }
            catch (Exception dialogException) { ErrorLog.Write(dialogException, "Displaying Options save error"); }
        }
        try
        {
            if (!Dispatcher.HasShutdownStarted && !Dispatcher.HasShutdownFinished)
                await Dispatcher.InvokeAsync(() =>
                {
                    if (_closingAfterSave) return;
                    _closingAfterSave = true;
                    _closeInProgress = false;
                    Close();
                }).Task.ConfigureAwait(false);
            else
            {
                _closingAfterSave = true;
                _closeInProgress = false;
            }
        }
        catch (Exception closeException)
        {
            _closingAfterSave = true;
            _closeInProgress = false;
            ErrorLog.Write(closeException, "Closing Options after save");
        }
    }
}
