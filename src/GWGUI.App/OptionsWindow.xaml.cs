using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace GWGUI.App;

public partial class OptionsWindow : Window
{
    private readonly AppSettings _settings;
    private readonly List<ControllerSettings> _controllers;
    private readonly List<ControllerSettings> _unconfiguredControllers;
    private readonly List<DriveSettings> _drives;
    private readonly IGwInstallationManager _hostTools;
    private readonly IHardwareRegistry _hardwareRegistry;
    private string? _previousGwPath;
    private string? _installedVersion;
    private string? _availableVersion;
    private DateTimeOffset? _lastHostToolsCheck;
    private bool _initializing = true;
    private readonly ISettingsStore _settingsStore;
    private readonly SemaphoreSlim _saveLock = new(1, 1);
    private int _tagExampleIndex;
    private bool _closingAfterSave;
    private bool _closeInProgress;
    private bool _refreshingTagPresets;
    private ProfileOptionRow? _lastProfileClick;
    private DateTime _lastProfileClickAt;
    public ObservableCollection<HardwareRow> Hardware { get; } = [];
    public ObservableCollection<ProfileOptionRow> ReadProfiles { get; } = [];
    public ObservableCollection<ProfileOptionRow> WriteProfiles { get; } = [];
    public ObservableCollection<ProfileOptionRow> ConvertProfiles { get; } = [];
    private static readonly (string Key, string Pattern)[] TagPresetDefinitions =
    [
        ("Options.TagPresetFamily", "[{FAMILY}] "),
        ("Options.TagPresetFormat", "[{FORMAT}] "),
        ("Options.TagPresetFamilyFormat", "[{FAMILY}-{FORMAT}] "),
        ("Options.TagPresetFamilyExtension", "[{FAMILY}-{EXTENSION}] "),
        ("Options.TagPresetDetailed", "[{FAMILY}-{FORMAT}-{EXTENSION}] ")
    ];

    public OptionsWindow(AppSettings settings, IHardwareRegistry? hardwareRegistry = null, IGwInstallationManager? hostTools = null, OptionsSection section = OptionsSection.General, ISettingsStore? settingsStore = null)
    {
        InitializeComponent();
        _settings = settings;
        _settingsStore = settingsStore ?? new JsonSettingsStore(Path.Combine(StoragePaths.DataDirectory, "settings.json"));
        var managedRoot = StoragePaths.HostToolsDirectory;
        _hostTools = hostTools ?? new GwInstallationManager(new HttpClient(), managedRoot);
        _hardwareRegistry = hardwareRegistry ?? new GreaseweazleHardwareRegistry(new WindowsSerialDeviceDiscovery(), new GreaseweazleRunner());
        _previousGwPath = settings.PreviousGwExecutablePath; _installedVersion = settings.InstalledHostToolsVersion; _availableVersion = settings.AvailableHostToolsVersion; _lastHostToolsCheck = settings.LastHostToolsCheckUtc;
        _controllers = settings.Controllers.Select(x => new ControllerSettings
        {
            UsbId = x.UsbId,
            UsbSerialNumber = x.UsbSerialNumber,
            PnpDeviceId = x.PnpDeviceId,
            LastUsbLocation = x.LastUsbLocation,
            VendorId = x.VendorId,
            ProductId = x.ProductId,
            LastPort = x.LastPort,
            Model = x.Model,
            IsAvailable = x.IsAvailable
        }).ToList();
        _unconfiguredControllers = settings.UnconfiguredControllers.Select(CloneController).ToList();
        _drives = settings.Drives.Select(x => new DriveSettings { Id = x.Id, ControllerUsbId = x.ControllerUsbId, Selection = x.Selection, Size = x.Size, Density = x.Density, NominalRpm = x.NominalRpm }).ToList();
        AssignAllDriveSelections();
        ImagesFolderText.Text = settings.DefaultImagesFolder;
        GwPathText.Text = settings.GwExecutablePath;
        LanguageCombo.ItemsSource = UiLanguageCatalog.Available;
        LanguageCombo.SelectedItem = UiLanguageCatalog.Available.FirstOrDefault(language =>
            string.Equals(language.Code, settings.Language, StringComparison.OrdinalIgnoreCase))
            ?? UiLanguageCatalog.Available.First(language => language.Code == "en");
        ThemeCombo.SelectedIndex = (int)settings.Theme;
        UseTagsCheck.IsChecked = settings.Conversion.AddTags;
        TagPatternText.Text = settings.Conversion.TagPattern;
        RefreshTagPresets();
        RecentTagPatterns.ItemsSource = settings.Conversion.RecentCustomTagPatterns;
        RefreshHardwareRows();
        DrivesGrid.ItemsSource = Hardware;
        foreach (var profile in settings.Profiles)
            ProfilesFor(profile.Operation).Add(new(profile.Id, profile.Operation, profile.Name, false));
        ReadProfilesList.ItemsSource = ReadProfiles;
        WriteProfilesList.ItemsSource = WriteProfiles;
        ConvertProfilesList.ItemsSource = ConvertProfiles;
        HostToolsStatus.Text = File.Exists(settings.GwExecutablePath) ? LocExtension.Get("HostTools.Detected", settings.GwExecutablePath!) : LocExtension.Get("HostTools.None");
        Navigation.SelectedIndex = section == OptionsSection.Profiles ? 2 : section is OptionsSection.Hardware or OptionsSection.HostTools ? 1 : 0;
        _initializing = false;
        UpdateTagPreview();
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
        UpdateTagPreview();
        RefreshTagPresets();
    }

    private async void Theme_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_initializing || ThemeCombo.SelectedIndex < 0) return;
        _settings.Theme = (AppTheme)ThemeCombo.SelectedIndex;
        if (Application.Current is App app) app.SetTheme(_settings.Theme);
        await PersistSettingsAsync();
    }

    private async void UseTags_Changed(object sender, RoutedEventArgs e)
    {
        if (_initializing) return;
        _settings.Conversion.AddTags = UseTagsCheck.IsChecked == true;
        await PersistSettingsAsync();
    }

    private async void BrowseGw_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = LocExtension.Get("Options.ExecutableFilter") };
        if (dialog.ShowDialog(this) == true) { SetGwPath(new(dialog.FileName, null, false)); await PersistSettingsAsync(); }
    }

    private async void DetectHostTools_Click(object sender, RoutedEventArgs e)
    {
        var found = _hostTools.Detect(GwPathText.Text).FirstOrDefault();
        if (found is null) { HostToolsStatus.Text = LocExtension.Get("HostTools.None"); return; }
        SetGwPath(found);
        HostToolsStatus.Text = LocExtension.Get("HostTools.Detected", found.ExecutablePath);
        await PersistSettingsAsync();
    }

    private async void CheckHostTools_Click(object sender, RoutedEventArgs e)
    {
        await WithHostToolsBusyAsync(async () =>
        {
            var release = await _hostTools.GetLatestReleaseAsync(); _availableVersion = release.Version; _lastHostToolsCheck = DateTimeOffset.UtcNow;
            HostToolsStatus.Text = LocExtension.Get("HostTools.Latest", release.Version);
            await PersistSettingsAsync();
        });
    }

    private async void DownloadHostTools_Click(object sender, RoutedEventArgs e)
    {
        await WithHostToolsBusyAsync(async () =>
        {
            var release = await _hostTools.GetLatestReleaseAsync(); _availableVersion = release.Version; _lastHostToolsCheck = DateTimeOffset.UtcNow;
            if (MessageBox.Show(this, LocExtension.Get("HostTools.DownloadConfirm", release.Version), LocExtension.Get("HostTools.Title"), MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
            HostToolsProgress.Visibility = Visibility.Visible;
            var progress = new Progress<double>(value => HostToolsProgress.Value = value * 100);
            var installed = await _hostTools.InstallAsync(release, progress);
            SetGwPath(installed);
            HostToolsStatus.Text = LocExtension.Get("HostTools.Installed", installed.Version ?? release.Version);
            await PersistSettingsAsync();
        });
    }

    private async void RollbackHostTools_Click(object sender, RoutedEventArgs e)
    {
        HostToolsSelection selection;
        try { selection = _hostTools.Rollback(GwPathText.Text, _previousGwPath); }
        catch (FileNotFoundException) { MessageBox.Show(this, LocExtension.Get("HostTools.NoPrevious"), LocExtension.Get("HostTools.Title")); return; }
        ApplySelection(selection);
        HostToolsStatus.Text = LocExtension.Get("HostTools.Detected", GwPathText.Text);
        await PersistSettingsAsync();
    }

    private void SetGwPath(HostToolsInstallation installation)
    {
        ApplySelection(_hostTools.Select(GwPathText.Text, _previousGwPath, installation));
    }

    private void ApplySelection(HostToolsSelection selection)
    {
        GwPathText.Text = selection.ExecutablePath ?? "";
        _previousGwPath = selection.PreviousExecutablePath;
        _installedVersion = selection.InstalledVersion;
    }

    private async Task WithHostToolsBusyAsync(Func<Task> action)
    {
        DownloadHostToolsButton.IsEnabled = false;
        try { await action(); }
        catch (Exception exception) { MessageBox.Show(this, exception.Message, LocExtension.Get("HostTools.Title"), MessageBoxButton.OK, MessageBoxImage.Error); }
        finally { DownloadHostToolsButton.IsEnabled = true; HostToolsProgress.Visibility = Visibility.Collapsed; }
    }

    private async void BrowseImagesFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { Multiselect = false, Title = LocExtension.Get("Options.ImagesFolder") };
        if (dialog.ShowDialog(this) == true) { ImagesFolderText.Text = dialog.FolderName; await PersistSettingsAsync(); }
    }

    private void TagPattern_Changed(object sender, TextChangedEventArgs e)
    {
        if (!_initializing && TagPresetCombo is not null && !TagPresetDefinitions.Any(item => string.Equals(item.Pattern, TagPatternText.Text, StringComparison.OrdinalIgnoreCase)))
            TagPresetCombo.SelectedItem = null;
        UpdateTagPreview();
    }

    private async void TagPreset_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_initializing || _refreshingTagPresets || TagPresetCombo.SelectedItem is not TagPresetOption preset) return;
        TagPatternText.Text = preset.Pattern;
        await PersistSettingsAsync();
    }

    private async void TagPattern_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (!TagPresetDefinitions.Any(item => string.Equals(item.Pattern, TagPatternText.Text, StringComparison.OrdinalIgnoreCase))) RememberCustomTagPattern(TagPatternText.Text);
        await PersistSettingsAsync();
    }

    private async void RecentTagPattern_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (RecentTagPatterns.SelectedItem is not string pattern) return;
        TagPatternText.Text = pattern;
        await PersistSettingsAsync();
    }

    private void NextTagExample_Click(object sender, RoutedEventArgs e) { _tagExampleIndex++; UpdateTagPreview(); }

    private void UpdateTagPreview()
    {
        if (TagPatternPreview is null || TagPatternText is null) return;
        var samples = new[]
        {
            ("Disquette", "PC", "720", "IMA"), ("Workbench", "AMIGA", "DD", "ADF"),
            ("Jeu", "ST", "720", "ST"), ("Archive", "PC", "1440", "IMG")
        };
        var sample = samples[_tagExampleIndex % samples.Length];
        var now = new DateTime(2026, 8, 6, 14, 35, 42);
        var rendered = RenderTagPattern(TagPatternText.Text, sample.Item1, sample.Item2, sample.Item3, sample.Item4, now);
        var fileName = TagPatternText.Text.Contains("{NAME}", StringComparison.OrdinalIgnoreCase) ? rendered : rendered + sample.Item1;
        TagPatternPreview.Text = LocExtension.Get("Options.TagPatternPreview", fileName + "." + sample.Item4.ToLowerInvariant());
    }

    internal static string RenderTagPattern(string pattern, string name, string family, string format, string extension, DateTime timestamp) => pattern
        .Replace("{TAG}", family + "-" + format, StringComparison.OrdinalIgnoreCase)
        .Replace("{NAME}", name, StringComparison.OrdinalIgnoreCase)
        .Replace("{FAMILY}", family, StringComparison.OrdinalIgnoreCase)
        .Replace("{FORMAT}", format, StringComparison.OrdinalIgnoreCase)
        .Replace("{EXTENSION}", extension, StringComparison.OrdinalIgnoreCase)
        .Replace("{DATE:YYYY-MM-DD}", timestamp.ToString("yyyy-MM-dd"), StringComparison.OrdinalIgnoreCase)
        .Replace("{DATE:YYYYMMDD}", timestamp.ToString("yyyyMMdd"), StringComparison.OrdinalIgnoreCase)
        .Replace("{DATE:DD-MM-YYYY}", timestamp.ToString("dd-MM-yyyy"), StringComparison.OrdinalIgnoreCase)
        .Replace("{TIME:HH-MM-SS}", timestamp.ToString("HH-mm-ss"), StringComparison.OrdinalIgnoreCase)
        .Replace("{TIME:HHMMSS}", timestamp.ToString("HHmmss"), StringComparison.OrdinalIgnoreCase)
        .Replace("{TIME:HH-MM}", timestamp.ToString("HH-mm"), StringComparison.OrdinalIgnoreCase);

    private void RememberCustomTagPattern(string pattern)
    {
        pattern = pattern.Trim();
        if (string.IsNullOrEmpty(pattern)) return;
        _settings.Conversion.RecentCustomTagPatterns.RemoveAll(item => string.Equals(item, pattern, StringComparison.OrdinalIgnoreCase));
        _settings.Conversion.RecentCustomTagPatterns.Insert(0, pattern);
        if (_settings.Conversion.RecentCustomTagPatterns.Count > 5) _settings.Conversion.RecentCustomTagPatterns.RemoveRange(5, _settings.Conversion.RecentCustomTagPatterns.Count - 5);
        RecentTagPatterns.ItemsSource = null;
        RecentTagPatterns.ItemsSource = _settings.Conversion.RecentCustomTagPatterns;
    }

    private void RefreshTagPresets()
    {
        if (TagPresetCombo is null || TagPatternText is null) return;
        var current = TagPatternText.Text;
        var presets = TagPresetDefinitions.Select(item => new TagPresetOption(LocExtension.Get(item.Key), item.Pattern)).ToArray();
        _refreshingTagPresets = true;
        try
        {
            TagPresetCombo.ItemsSource = presets;
            TagPresetCombo.SelectedItem = presets.FirstOrDefault(item => string.Equals(item.Pattern, current, StringComparison.OrdinalIgnoreCase));
        }
        finally { _refreshingTagPresets = false; }
    }

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
        catch (Exception exception) { MessageBox.Show(this, exception.Message, LocExtension.Get("Hardware.ScanTitle"), MessageBoxButton.OK, MessageBoxImage.Error); }
        finally { ScanButton.IsEnabled = true; }
    }

    private void AddDrive_Click(object sender, RoutedEventArgs e)
    {
        var selected = DrivesGrid.SelectedItem as HardwareRow;
        var controllerId = selected?.UsbId ?? (_controllers.Count == 1 ? _controllers[0].UsbId : null);
        if (controllerId is null) { MessageBox.Show(this, LocExtension.Get("Hardware.SelectController"), LocExtension.Get("Hardware.DriveDialogTitle")); return; }
        if (_drives.Count(drive => drive.ControllerUsbId == controllerId) >= 2) { MessageBox.Show(this, LocExtension.Get("Hardware.MaximumDrives"), LocExtension.Get("Hardware.DriveDialogTitle")); return; }
        Hardware.Add(CreateRow(null, controllerId, true));
        DrivesGrid.SelectedItem = Hardware[^1];
    }

    private async void SaveHardwareRow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: HardwareRow row }) return;
        var drive = row.DriveId is null ? null : _drives.FirstOrDefault(item => item.Id == row.DriveId);
        if (drive is null)
        {
            if (_drives.Count(item => item.ControllerUsbId == row.UsbId) >= 2) { MessageBox.Show(this, LocExtension.Get("Hardware.MaximumDrives"), LocExtension.Get("Hardware.DriveDialogTitle")); return; }
            var controller = _unconfiguredControllers.FirstOrDefault(item => item.UsbId == row.UsbId);
            if (controller is not null) { _unconfiguredControllers.Remove(controller); _controllers.Add(controller); }
            drive = new DriveSettings { ControllerUsbId = row.UsbId };
            _drives.Add(drive);
        }
        drive.Size = row.Size;
        drive.Density = row.Density;
        drive.NominalRpm = row.Rpm == HardwareChoices.UnknownSpeed ? null : int.Parse(row.Rpm.AsSpan(0, 3));
        AssignDriveSelections(row.UsbId);
        RefreshHardwareRows();
        await PersistSettingsAsync();
    }

    private async void ForgetHardwareRow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: HardwareRow row }) return;
        var lastDrive = row.DriveId is not null && _drives.Count(item => item.ControllerUsbId == row.UsbId) == 1;
        var message = lastDrive ? LocExtension.Get("Hardware.ForgetLastConfirm") : LocExtension.Get("Hardware.ForgetConfirm");
        if (MessageBox.Show(this, message, LocExtension.Get("Hardware.Forget"), MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        if (row.DriveId is not null)
        {
            _drives.RemoveAll(item => item.Id == row.DriveId);
            if (!_drives.Any(item => item.ControllerUsbId == row.UsbId)) _controllers.RemoveAll(item => item.UsbId == row.UsbId);
            else AssignDriveSelections(row.UsbId);
        }
        else
        {
            _unconfiguredControllers.RemoveAll(item => item.UsbId == row.UsbId);
            _controllers.RemoveAll(item => item.UsbId == row.UsbId);
        }
        RefreshHardwareRows();
        await PersistSettingsAsync();
    }

    private void RefreshHardwareRows()
    {
        Hardware.Clear();
        foreach (var controller in _controllers)
        {
            var drives = _drives.Where(x => x.ControllerUsbId == controller.UsbId).ToArray();
            if (drives.Length == 0) Hardware.Add(CreateRow(null, controller.UsbId, true));
            foreach (var drive in drives) Hardware.Add(CreateRow(drive, controller.UsbId, true));
        }
        foreach (var controller in _unconfiguredControllers)
            Hardware.Add(CreateRow(null, controller.UsbId, false));
    }

    private HardwareRow CreateRow(DriveSettings? drive, string controllerId, bool configured)
    {
        var controller = _controllers.Concat(_unconfiguredControllers).First(item => item.UsbId == controllerId);
        var index = drive is null ? _drives.Count(item => item.ControllerUsbId == controllerId) + 1
            : _drives.Where(item => item.ControllerUsbId == controllerId).ToList().IndexOf(drive) + 1;
        return new HardwareRow(drive?.Id, controller.LastPort, controllerId, LocExtension.Get("Hardware.ReaderNumber", index),
            drive?.Size ?? "3.5", drive?.Density ?? "Unknown",
            drive?.NominalRpm is int rpm ? $"{rpm} RPM" : HardwareChoices.UnknownSpeed,
            controller.IsAvailable, configured, LocExtension.Get(configured ? "Hardware.Configured" : "Hardware.NotConfiguredState"));
    }

    private void AssignAllDriveSelections()
    {
        foreach (var controllerId in _drives.Select(item => item.ControllerUsbId).Distinct(StringComparer.OrdinalIgnoreCase)) AssignDriveSelections(controllerId);
    }

    private void AssignDriveSelections(string controllerId)
    {
        HardwareRoutingPolicy.AssignAutomaticDriveSelections(_drives, controllerId);
    }

    private void MergeUnconfigured(IReadOnlyList<ControllerSettings> detectedControllers)
    {
        foreach (var controller in _unconfiguredControllers) controller.IsAvailable = false;
        foreach (var detected in detectedControllers)
        {
            var known = _unconfiguredControllers.FirstOrDefault(item => StartupHardwareMonitor.SameController(item, detected));
            if (known is null) _unconfiguredControllers.Add(detected);
            else
            {
                known.UsbSerialNumber = detected.UsbSerialNumber; known.PnpDeviceId = detected.PnpDeviceId;
                known.LastUsbLocation = detected.LastUsbLocation; known.VendorId = detected.VendorId; known.ProductId = detected.ProductId;
                known.LastPort = detected.LastPort; known.Model = detected.Model; known.IsAvailable = detected.IsAvailable;
            }
        }
    }

    private static ControllerSettings CloneController(ControllerSettings source) => new()
    {
        UsbId = source.UsbId, UsbSerialNumber = source.UsbSerialNumber, PnpDeviceId = source.PnpDeviceId,
        LastUsbLocation = source.LastUsbLocation, VendorId = source.VendorId, ProductId = source.ProductId,
        LastPort = source.LastPort, Model = source.Model, IsAvailable = source.IsAvailable
    };

    private async void RenameProfile_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedProfile(sender) is not ProfileOptionRow row) return;
        var dialog = new ProfileNameWindow(row.Name) { Owner = this };
        if (dialog.ShowDialog() != true) return;
        var profiles = ProfilesFor(row.Operation);
        if (profiles.Any(x => x.Id != row.Id && string.Equals(x.Name, dialog.ProfileName, StringComparison.CurrentCultureIgnoreCase))) { MessageBox.Show(this, LocExtension.Get("Profile.DuplicateName"), LocExtension.Get("Profile.Title"), MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        var index = profiles.IndexOf(row); profiles[index] = row with { Name = dialog.ProfileName };
        await PersistSettingsAsync();
    }

    private async void DeleteProfile_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedProfile(sender) is not ProfileOptionRow row) return;
        if (MessageBox.Show(this, LocExtension.Get("Profile.DeleteConfirm", row.Name), LocExtension.Get("Profile.Title"), MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        ProfilesFor(row.Operation).Remove(row);
        await PersistSettingsAsync();
    }

    private void ProfileList_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F2) { RenameProfile_Click(sender, new RoutedEventArgs()); e.Handled = true; }
        else if (e.Key == Key.Delete) { DeleteProfile_Click(sender, new RoutedEventArgs()); e.Handled = true; }
    }

    private void ProfileList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ListBox list || ItemsControl.ContainerFromElement(list, e.OriginalSource as DependencyObject) is not ListBoxItem item || item.DataContext is not ProfileOptionRow row) return;
        var now = DateTime.UtcNow;
        var delay = now - _lastProfileClickAt;
        if (Equals(list.SelectedItem, row) && _lastProfileClick == row && delay >= TimeSpan.FromMilliseconds(450) && delay <= TimeSpan.FromSeconds(1.5))
        {
            RenameProfile_Click(list, new RoutedEventArgs());
            _lastProfileClick = null;
            e.Handled = true;
            return;
        }
        _lastProfileClick = row;
        _lastProfileClickAt = now;
    }

    private void ProfileList_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is ListBox list && ItemsControl.ContainerFromElement(list, e.OriginalSource as DependencyObject) is ListBoxItem item)
            item.IsSelected = true;
    }

    private ProfileOptionRow? SelectedProfile(object sender)
    {
        if (sender is ListBox list) return list.SelectedItem as ProfileOptionRow;
        if (sender is MenuItem { Parent: ContextMenu context } && context.PlacementTarget is ListBox contextList) return contextList.SelectedItem as ProfileOptionRow;
        return ReadProfilesList.SelectedItem as ProfileOptionRow ?? WriteProfilesList.SelectedItem as ProfileOptionRow ?? ConvertProfilesList.SelectedItem as ProfileOptionRow;
    }

    private ObservableCollection<ProfileOptionRow> ProfilesFor(string operation) => operation switch
    {
        "Read" => ReadProfiles, "Write" => WriteProfiles, "Convert" => ConvertProfiles,
        _ => throw new ArgumentOutOfRangeException(nameof(operation))
    };

    private void ApplyControlsToSettings()
    {
        _settings.DefaultImagesFolder = ImagesFolderText.Text.Trim();
        _settings.GwExecutablePath = string.IsNullOrWhiteSpace(GwPathText.Text) ? null : GwPathText.Text.Trim();
        _settings.PreviousGwExecutablePath = _previousGwPath;
        _settings.InstalledHostToolsVersion = _installedVersion;
        _settings.AvailableHostToolsVersion = _availableVersion;
        _settings.LastHostToolsCheckUtc = _lastHostToolsCheck;
        if (LanguageCombo.SelectedItem is UiLanguage language) _settings.Language = language.Code;
        _settings.Theme = (AppTheme)Math.Max(0, ThemeCombo.SelectedIndex);
        _settings.Conversion.TagPattern = TagPatternText.Text;
        _settings.Conversion.AddTags = UseTagsCheck.IsChecked == true;
        _settings.Controllers = _controllers;
        _settings.UnconfiguredControllers = _unconfiguredControllers;
        _settings.Drives = _drives;
        var retained = ReadProfiles.Concat(WriteProfiles).Concat(ConvertProfiles).ToDictionary(x => x.Id);
        _settings.Profiles = _settings.Profiles.Where(x => retained.ContainsKey(x.Id)).Select(x => { x.Name = retained[x.Id].Name; return x; }).ToList();
    }

    private async Task PersistSettingsAsync()
    {
        if (_initializing) return;
        ApplyControlsToSettings();
        await _saveLock.WaitAsync().ConfigureAwait(false);
        try { await _settingsStore.SaveAsync(_settings).ConfigureAwait(false); }
        finally { _saveLock.Release(); }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private async void Window_Closing(object? sender, CancelEventArgs e)
    {
        if (_initializing || _closingAfterSave) return;
        e.Cancel = true;
        if (_closeInProgress) return;
        _closeInProgress = true;
        try { await PersistSettingsAsync(); }
        catch (Exception exception) { MessageBox.Show(this, exception.Message, LocExtension.Get("Options.Title"), MessageBoxButton.OK, MessageBoxImage.Warning); }
        finally
        {
            _closingAfterSave = true;
            _closeInProgress = false;
            Close();
        }
    }
}

public sealed class HardwareRow(string? driveId, string port, string usbId, string readerLabel, string size, string density, string rpm, bool available, bool configured, string configurationState)
{
    public string? DriveId { get; } = driveId;
    public string Port { get; } = port;
    public string UsbId { get; } = usbId;
    public string ReaderLabel { get; } = readerLabel;
    public string Size { get; set; } = size;
    public string Density { get; set; } = density;
    public string Rpm { get; set; } = rpm;
    public bool Available { get; } = available;
    public bool Configured { get; } = configured;
    public string ConfigurationState { get; } = configurationState;
}

public static class HardwareChoices
{
    public const string UnknownSpeed = "—";
    public static IReadOnlyList<string> Sizes { get; } = ["3", "3.5", "5.25", "8"];
    public static IReadOnlyList<string> Densities { get; } = ["Unknown", "DD", "HD", "ED"];
    public static IReadOnlyList<string> Speeds { get; } = [UnknownSpeed, "300 RPM", "360 RPM"];
}
public sealed record ProfileOptionRow(string Id, string Operation, string Name, bool IsSystem)
{
    public string OperationLabel => Operation switch { "Read" => LocExtension.Get("Tab.Read"), "Write" => LocExtension.Get("Tab.Write"), "Convert" => LocExtension.Get("Tab.Convert"), _ => Operation };
}
public sealed record TagPresetOption(string Label, string Pattern);
