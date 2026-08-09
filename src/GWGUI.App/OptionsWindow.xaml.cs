using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using GWGUI.Domain.Commands;
using GWGUI.Domain.Hardware;
using GWGUI.Domain.Settings;
using GWGUI.Infrastructure.Hardware;
using GWGUI.Infrastructure.Processes;
using GWGUI.Infrastructure.Settings;
using GWGUI.App.Localization;
using GWGUI.Domain.HostTools;
using GWGUI.Infrastructure.HostTools;
using GWGUI.App.Services;
using GWGUI.App.Options;
using GWGUI.App.Controls;

namespace GWGUI.App;

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
    private readonly TagOptionsController _tagOptionsController;
    private readonly List<ControllerSettings> _controllers;
    private readonly List<ControllerSettings> _unconfiguredControllers;
    private readonly List<DriveSettings> _drives;
    private readonly HostToolsOptionsState _hostToolsState;
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
    public ObservableCollection<LogOptionRow> LogOptions { get; } = [];
    public OptionsWindow(AppSettings settings, IHardwareRegistry? hardwareRegistry = null, IGwInstallationManager? hostTools = null, OptionsSection section = OptionsSection.General, ISettingsStore? settingsStore = null)
    {
        InitializeComponent();
        ConnectSections();
        _settings = settings;
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
        _settingsStore = settingsStore ?? new JsonSettingsStore(Path.Combine(StoragePaths.DataDirectory, "settings.json"));
        var managedRoot = StoragePaths.HostToolsDirectory;
        var hostToolsManager = hostTools ?? new GwInstallationManager(new HttpClient(), managedRoot);
        _hostToolsState = new HostToolsOptionsState(settings, hostToolsManager);
        _hardwareRegistry = hardwareRegistry ?? new GreaseweazleHardwareRegistry(new WindowsSerialDeviceDiscovery(), new GreaseweazleRunner());
        _hardwareState = new HardwareOptionsState(settings);
        _controllers = _hardwareState.Controllers;
        _unconfiguredControllers = _hardwareState.UnconfiguredControllers;
        _drives = _hardwareState.Drives;
        ImagesFolderText.Text = settings.DefaultImagesFolder;
        GwPathText.Text = _hostToolsState.CurrentPath;
        LanguageCombo.ItemsSource = UiLanguageCatalog.Available;
        LanguageCombo.SelectedItem = UiLanguageCatalog.Available.FirstOrDefault(language =>
            string.Equals(language.Code, settings.Language, StringComparison.OrdinalIgnoreCase))
            ?? UiLanguageCatalog.Fallback;
        ThemeCombo.SelectedIndex = (int)settings.Theme;
        RefreshLogOptions();
        LogOptionsList.ItemsSource = LogOptions;
        LogsDirectoryText.Text = StoragePaths.LogsDirectory;
        RefreshHardwareRows();
        DrivesGrid.ItemsSource = Hardware;
        HostToolsStatus.Text = File.Exists(settings.GwExecutablePath) ? LocExtension.Get("HostTools.Detected", settings.GwExecutablePath!) : LocExtension.Get("HostTools.None");
        Navigation.SelectedIndex = section switch { OptionsSection.Logs => 1, OptionsSection.Hardware or OptionsSection.HostTools => 2, OptionsSection.Profiles => 3, _ => 0 };
        _initializing = false;
    }

    private void ConnectSections()
    {
        RegisterSectionNames();

        GeneralSection.LanguageChanged += Language_SelectionChanged;
        GeneralSection.ThemeChanged += Theme_SelectionChanged;
        GeneralSection.BrowseImagesFolderRequested += BrowseImagesFolder_Click;
        GeneralSection.AutoSaveTextEditingFinished += AutoSaveText_LostKeyboardFocus;

        LogsSection.LogRowChanged += LogRow_Changed;
        LogsSection.MaximumSizeEditingFinished += LogRowMaximumSize_LostKeyboardFocus;
        LogsSection.NumericTextEntered += NumericText_PreviewTextInput;
        LogsSection.OpenLogsFolderRequested += OpenLogsFolder_Click;

        HardwareSection.ScanRequested += ScanHardware_Click;
        HardwareSection.AddDriveRequested += AddDrive_Click;
        HardwareSection.SaveDriveRequested += SaveHardwareRow_Click;
        HardwareSection.ForgetDriveRequested += ForgetHardwareRow_Click;
        HardwareSection.AutoSaveTextEditingFinished += AutoSaveText_LostKeyboardFocus;
        HardwareSection.BrowseGwRequested += BrowseGw_Click;
        HardwareSection.DetectHostToolsRequested += DetectHostTools_Click;
        HardwareSection.CheckHostToolsRequested += CheckHostTools_Click;
        HardwareSection.DownloadHostToolsRequested += DownloadHostTools_Click;
        HardwareSection.RollbackHostToolsRequested += RollbackHostTools_Click;

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

    private async void Language_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_initializing || LanguageCombo.SelectedItem is not UiLanguage language ||
            string.Equals(_settings.Language, language.Code, StringComparison.OrdinalIgnoreCase)) return;

        _settings.Language = language.Code;
        if (Application.Current is App app) app.SetLanguage(language.Code);
        else LocalizationSource.Instance.Refresh();
        RefreshLocalizedContent();
        await PersistSettingsAsync();
    }

    internal void RefreshLocalizedContent()
    {
        HostToolsStatus.Text = File.Exists(GwPathText.Text)
            ? LocExtension.Get("HostTools.Detected", GwPathText.Text)
            : LocExtension.Get("HostTools.None");
        _tagOptionsController.RefreshLocalizedContent();
        RefreshLogOptions();
    }

    private async void Theme_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_initializing || ThemeCombo.SelectedIndex < 0) return;
        _settings.Theme = (AppTheme)ThemeCombo.SelectedIndex;
        if (Application.Current is App app) app.SetTheme(_settings.Theme);
        await PersistSettingsAsync();
    }

    private async void LogRow_Changed(object sender, RoutedEventArgs e)
    {
        if (_initializing) return;
        await PersistSettingsAsync();
    }

    private async void LogRowMaximumSize_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (sender is TextBox textBox && textBox.DataContext is LogOptionRow row && (!int.TryParse(textBox.Text, out var value) || value < 0))
            textBox.Text = row.Settings.MaximumKilobytes.ToString();
        await PersistSettingsAsync();
    }

    private void NumericText_PreviewTextInput(object sender, TextCompositionEventArgs e) => e.Handled = e.Text.Any(character => !char.IsDigit(character));

    private void OpenLogsFolder_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Directory.CreateDirectory(StoragePaths.LogsDirectory);
            Process.Start(new ProcessStartInfo(StoragePaths.LogsDirectory) { UseShellExecute = true });
        }
        catch (Exception exception)
        {
            ErrorLog.Write(exception, "Opening Logs folder");
            ShowLoggedError(exception, "Opening Logs folder", "Error.Title", MessageBoxImage.Warning);
        }
    }

    private async void BrowseGw_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = LocExtension.Get("Options.ExecutableFilter") };
        if (dialog.ShowDialog(this) == true) { SelectHostTools(new(dialog.FileName, null, false)); await PersistSettingsAsync(); }
    }

    private async void DetectHostTools_Click(object sender, RoutedEventArgs e)
    {
        var found = _hostToolsState.Detect(GwPathText.Text);
        if (found is null) { HostToolsStatus.Text = LocExtension.Get("HostTools.None"); return; }
        SelectHostTools(found);
        HostToolsStatus.Text = LocExtension.Get("HostTools.Detected", found.ExecutablePath);
        await PersistSettingsAsync();
    }

    private async void CheckHostTools_Click(object sender, RoutedEventArgs e)
    {
        await WithHostToolsBusyAsync(async () =>
        {
            var release = await _hostToolsState.CheckLatestAsync();
            HostToolsStatus.Text = LocExtension.Get("HostTools.Latest", release.Version);
            await PersistSettingsAsync();
        });
    }

    private async void DownloadHostTools_Click(object sender, RoutedEventArgs e)
    {
        await WithHostToolsBusyAsync(async () =>
        {
            var release = await _hostToolsState.CheckLatestAsync();
            if (MessageBox.Show(this, LocExtension.Get("HostTools.DownloadConfirm", release.Version), LocExtension.Get("HostTools.Title"), MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
            HostToolsProgress.Visibility = Visibility.Visible;
            var progress = new Progress<double>(value => HostToolsProgress.Value = value * 100);
            var installed = await _hostToolsState.InstallAsync(release, progress);
            GwPathText.Text = _hostToolsState.CurrentPath ?? "";
            HostToolsStatus.Text = LocExtension.Get("HostTools.Installed", installed.Version ?? release.Version);
            await PersistSettingsAsync();
        });
    }

    private async void RollbackHostTools_Click(object sender, RoutedEventArgs e)
    {
        try { _hostToolsState.Rollback(GwPathText.Text); }
        catch (FileNotFoundException) { MessageBox.Show(this, LocExtension.Get("HostTools.NoPrevious"), LocExtension.Get("HostTools.Title")); return; }
        GwPathText.Text = _hostToolsState.CurrentPath ?? "";
        HostToolsStatus.Text = LocExtension.Get("HostTools.Detected", GwPathText.Text);
        await PersistSettingsAsync();
    }

    private void SelectHostTools(HostToolsInstallation installation)
    {
        _hostToolsState.SetCurrentPath(GwPathText.Text);
        _hostToolsState.Select(installation);
        GwPathText.Text = _hostToolsState.CurrentPath ?? "";
    }

    private async Task WithHostToolsBusyAsync(Func<Task> action)
    {
        DownloadHostToolsButton.IsEnabled = false;
        try { await action(); }
        catch (Exception exception) { ShowLoggedError(exception, "Managing Host Tools", "HostTools.Title", MessageBoxImage.Error); }
        finally { DownloadHostToolsButton.IsEnabled = true; HostToolsProgress.Visibility = Visibility.Collapsed; }
    }

    private async void BrowseImagesFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { Multiselect = false, Title = LocExtension.Get("Options.ImagesFolder") };
        if (dialog.ShowDialog(this) == true) { ImagesFolderText.Text = dialog.FolderName; await PersistSettingsAsync(); }
    }

    internal static string RenderTagPattern(string pattern, string name, string family, string format, string extension, DateTime timestamp) =>
        TagPatternFormatter.Render(pattern, name, family, format, extension, timestamp);

    private async void AutoSaveText_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => await PersistSettingsAsync();

    private async void ScanHardware_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(GwPathText.Text) || !File.Exists(GwPathText.Text)) { MessageBox.Show(this, LocExtension.Get("Hardware.GwRequired"), LocExtension.Get("Hardware.ScanTitle")); return; }
        ScanButton.IsEnabled = false;
        try
        {
            var scanned = await _hardwareRegistry.ScanAsync(GwPathText.Text, _controllers);
            _controllers.Clear();
            _controllers.AddRange(scanned.ConfiguredControllers);
            MergeUnconfigured(scanned.UnconfiguredControllers);
            RefreshHardwareRows();
            await PersistSettingsAsync();
        }
        catch (Exception exception) { ShowLoggedError(exception, "Scanning hardware", "Hardware.ScanTitle", MessageBoxImage.Error); }
        finally { ScanButton.IsEnabled = true; }
    }

    private void AddDrive_Click(object sender, RoutedEventArgs e)
    {
        var selected = DrivesGrid.SelectedItem as HardwareRow;
        var controllerId = selected?.UsbId ?? (_controllers.Count == 1 ? _controllers[0].UsbId : null);
        if (controllerId is null) { MessageBox.Show(this, LocExtension.Get("Hardware.SelectController"), LocExtension.Get("Hardware.DriveDialogTitle")); return; }
        if (_hardwareState.HasMaximumDrives(controllerId)) { MessageBox.Show(this, LocExtension.Get("Hardware.MaximumDrives"), LocExtension.Get("Hardware.DriveDialogTitle")); return; }
        Hardware.Add(_hardwareState.CreateDraftRow(controllerId));
        DrivesGrid.SelectedItem = Hardware[^1];
    }

    private async void SaveHardwareRow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: HardwareRow row }) return;
        if (!_hardwareState.Save(row)) { MessageBox.Show(this, LocExtension.Get("Hardware.MaximumDrives"), LocExtension.Get("Hardware.DriveDialogTitle")); return; }
        RefreshHardwareRows();
        await PersistSettingsAsync();
    }

    private async void ForgetHardwareRow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: HardwareRow row }) return;
        var lastDrive = row.DriveId is not null && _drives.Count(item => item.ControllerUsbId == row.UsbId) == 1;
        var message = lastDrive ? LocExtension.Get("Hardware.ForgetLastConfirm") : LocExtension.Get("Hardware.ForgetConfirm");
        if (MessageBox.Show(this, message, LocExtension.Get("Hardware.Forget"), MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        RemoveHardwareRow(row);
        await PersistSettingsAsync();
    }

    private void RemoveHardwareRow(HardwareRow row)
    {
        // A row added with "Add drive" does not exist in the saved configuration yet.
        // Removing it must therefore affect only that visible row.
        if (row.DriveId is null && row.Configured)
        {
            Hardware.Remove(row);
            return;
        }
        _hardwareState.Remove(row);
        RefreshHardwareRows();
    }

    private void RefreshHardwareRows()
    {
        Hardware.Clear();
        foreach (var row in _hardwareState.CreateRows()) Hardware.Add(row);
    }

    internal void MergeUnconfigured(IReadOnlyList<ControllerSettings> detectedControllers)
    {
        _hardwareState.MergeUnconfigured(detectedControllers);
    }

    private void ApplyControlsToSettings()
    {
        _settings.DefaultImagesFolder = ImagesFolderText.Text.Trim();
        _hostToolsState.SetCurrentPath(GwPathText.Text);
        _hostToolsState.ApplyTo(_settings);
        if (LanguageCombo.SelectedItem is UiLanguage language) _settings.Language = language.Code;
        _settings.Theme = (AppTheme)Math.Max(0, ThemeCombo.SelectedIndex);
        _tagOptionsController.ApplyTo(_settings);
        _settings.Controllers = _controllers;
        _settings.UnconfiguredControllers = _unconfiguredControllers;
        _settings.Drives = _drives;
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

    private void RefreshLogOptions()
    {
        LogOptions.Clear();
        foreach (var definition in OptionsDefinitions.LogActions)
            LogOptions.Add(new(definition.Action, LocExtension.Get(definition.LabelKey), _settings.Logging.GetOrCreate(definition.Action)));
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
