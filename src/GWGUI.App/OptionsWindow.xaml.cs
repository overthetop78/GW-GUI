using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
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
    private bool _initializingLanguage = true;
    public ObservableCollection<HardwareRow> Hardware { get; } = [];
    public ObservableCollection<ProfileOptionRow> Profiles { get; } = [];

    public OptionsWindow(AppSettings settings, IHardwareRegistry? hardwareRegistry = null, IGwInstallationManager? hostTools = null, OptionsSection section = OptionsSection.General)
    {
        InitializeComponent();
        _settings = settings;
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
        _initializingLanguage = false;
        ThemeCombo.SelectedIndex = (int)settings.Theme;
        TagPatternText.Text = settings.Conversion.TagPattern;
        RefreshHardwareRows();
        DrivesGrid.ItemsSource = Hardware;
        foreach (var operation in new[] { "Read", "Write", "Convert" }) Profiles.Add(new($"default-{operation.ToLowerInvariant()}", operation, LocExtension.Get("Profile.Default"), true));
        foreach (var profile in settings.Profiles) Profiles.Add(new(profile.Id, profile.Operation, profile.Name, false));
        ProfilesGrid.ItemsSource = Profiles;
        HostToolsStatus.Text = File.Exists(settings.GwExecutablePath) ? LocExtension.Get("HostTools.Detected", settings.GwExecutablePath!) : LocExtension.Get("HostTools.None");
        Navigation.SelectedIndex = (int)section;
    }

    private void Navigation_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (GeneralPage is null) return;
        var pages = new FrameworkElement[] { GeneralPage, ToolsPage, HardwarePage, ProfilesPage };
        for (var index = 0; index < pages.Length; index++) pages[index].Visibility = index == Navigation.SelectedIndex ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void Language_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_initializingLanguage || LanguageCombo.SelectedItem is not UiLanguage language ||
            string.Equals(_settings.Language, language.Code, StringComparison.OrdinalIgnoreCase)) return;

        _settings.Language = language.Code;
        if (Application.Current is App app) app.SetLanguage(language.Code);
        else LocalizationSource.Instance.Refresh();
        RefreshLocalizedContent();
        await new JsonSettingsStore(Path.Combine(StoragePaths.DataDirectory, "settings.json")).SaveAsync(_settings);
    }

    internal void RefreshLocalizedContent()
    {
        for (var index = 0; index < Profiles.Count; index++)
            if (Profiles[index].IsSystem) Profiles[index] = Profiles[index] with { Name = LocExtension.Get("Profile.Default") };
        ProfilesGrid.Items.Refresh();
        HostToolsStatus.Text = File.Exists(GwPathText.Text)
            ? LocExtension.Get("HostTools.Detected", GwPathText.Text)
            : LocExtension.Get("HostTools.None");
        TagPattern_Changed(this, new TextChangedEventArgs(TextBox.TextChangedEvent, UndoAction.None));
    }

    private void BrowseGw_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = LocExtension.Get("Options.ExecutableFilter") };
        if (dialog.ShowDialog(this) == true) SetGwPath(new(dialog.FileName, null, false));
    }

    private void DetectHostTools_Click(object sender, RoutedEventArgs e)
    {
        var found = _hostTools.Detect(GwPathText.Text).FirstOrDefault();
        if (found is null) { HostToolsStatus.Text = LocExtension.Get("HostTools.None"); return; }
        SetGwPath(found);
        HostToolsStatus.Text = LocExtension.Get("HostTools.Detected", found.ExecutablePath);
    }

    private async void CheckHostTools_Click(object sender, RoutedEventArgs e)
    {
        await WithHostToolsBusyAsync(async () =>
        {
            var release = await _hostTools.GetLatestReleaseAsync(); _availableVersion = release.Version; _lastHostToolsCheck = DateTimeOffset.UtcNow;
            HostToolsStatus.Text = LocExtension.Get("HostTools.Latest", release.Version);
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
        });
    }

    private void RollbackHostTools_Click(object sender, RoutedEventArgs e)
    {
        HostToolsSelection selection;
        try { selection = _hostTools.Rollback(GwPathText.Text, _previousGwPath); }
        catch (FileNotFoundException) { MessageBox.Show(this, LocExtension.Get("HostTools.NoPrevious"), LocExtension.Get("HostTools.Title")); return; }
        ApplySelection(selection);
        HostToolsStatus.Text = LocExtension.Get("HostTools.Detected", GwPathText.Text);
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

    private void BrowseImagesFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { Multiselect = false, Title = LocExtension.Get("Options.ImagesFolder") };
        if (dialog.ShowDialog(this) == true) ImagesFolderText.Text = dialog.FolderName;
    }

    private void TagPattern_Changed(object sender, TextChangedEventArgs e)
    {
        if (TagPatternPreview is null) return;
        var pattern = TagPatternText.Text;
        TagPatternPreview.Text = pattern.Contains("{tag}", StringComparison.OrdinalIgnoreCase)
            ? LocExtension.Get("Options.TagPatternPreview", "Disquette" + pattern.Replace("{tag}", "PC-720", StringComparison.OrdinalIgnoreCase) + ".ima")
            : LocExtension.Get("Options.TagPatternInvalid");
    }

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

    private void SaveHardwareRow_Click(object sender, RoutedEventArgs e)
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
    }

    private void ForgetHardwareRow_Click(object sender, RoutedEventArgs e)
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

    private void RenameProfile_Click(object sender, RoutedEventArgs e)
    {
        if (ProfilesGrid.SelectedItem is not ProfileOptionRow row) return;
        if (row.IsSystem) { MessageBox.Show(this, LocExtension.Get("Profile.SystemRename"), LocExtension.Get("Profile.Title"), MessageBoxButton.OK, MessageBoxImage.Information); return; }
        var dialog = new ProfileNameWindow(row.Name) { Owner = this };
        if (dialog.ShowDialog() != true) return;
        if (Profiles.Any(x => x.Operation == row.Operation && x.Id != row.Id && string.Equals(x.Name, dialog.ProfileName, StringComparison.CurrentCultureIgnoreCase))) { MessageBox.Show(this, LocExtension.Get("Profile.DuplicateName"), LocExtension.Get("Profile.Title"), MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        var index = Profiles.IndexOf(row); Profiles[index] = row with { Name = dialog.ProfileName };
    }

    private void DeleteProfile_Click(object sender, RoutedEventArgs e)
    {
        if (ProfilesGrid.SelectedItem is not ProfileOptionRow row) return;
        if (row.IsSystem) { MessageBox.Show(this, LocExtension.Get("Profile.SystemDelete"), LocExtension.Get("Profile.Title"), MessageBoxButton.OK, MessageBoxImage.Information); return; }
        if (MessageBox.Show(this, LocExtension.Get("Profile.DeleteConfirm", row.Name), LocExtension.Get("Profile.Title"), MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes) Profiles.Remove(row);
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!TagPatternText.Text.Contains("{tag}", StringComparison.OrdinalIgnoreCase)) { MessageBox.Show(this, LocExtension.Get("Options.TagPatternInvalid"), LocExtension.Get("Options.Title"), MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        _settings.DefaultImagesFolder = ImagesFolderText.Text.Trim();
        _settings.GwExecutablePath = string.IsNullOrWhiteSpace(GwPathText.Text) ? null : GwPathText.Text.Trim();
        _settings.PreviousGwExecutablePath = _previousGwPath;
        _settings.InstalledHostToolsVersion = _installedVersion;
        _settings.AvailableHostToolsVersion = _availableVersion;
        _settings.LastHostToolsCheckUtc = _lastHostToolsCheck;
        if (LanguageCombo.SelectedItem is UiLanguage language) _settings.Language = language.Code;
        _settings.Theme = (AppTheme)Math.Max(0, ThemeCombo.SelectedIndex);
        _settings.Conversion.TagPattern = TagPatternText.Text;
        _settings.Controllers = _controllers;
        _settings.UnconfiguredControllers = _unconfiguredControllers;
        _settings.Drives = _drives;
        var retained = Profiles.Where(x => !x.IsSystem).ToDictionary(x => x.Id);
        _settings.Profiles = _settings.Profiles.Where(x => retained.ContainsKey(x.Id)).Select(x => { x.Name = retained[x.Id].Name; return x; }).ToList();
        DialogResult = true;
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
