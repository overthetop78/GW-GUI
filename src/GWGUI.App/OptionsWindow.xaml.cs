using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using GWGUI.Domain.Commands;
using GWGUI.Domain.Hardware;
using GWGUI.Domain.Settings;
using GWGUI.Infrastructure.Hardware;
using GWGUI.Infrastructure.Processes;
using GWGUI.App.Localization;

namespace GWGUI.App;

public partial class OptionsWindow : Window
{
    private readonly AppSettings _settings;
    private readonly List<ControllerSettings> _controllers;
    private readonly List<DriveSettings> _drives;
    public ObservableCollection<HardwareRow> Hardware { get; } = [];
    public ObservableCollection<ProfileOptionRow> Profiles { get; } = [];

    public OptionsWindow(AppSettings settings)
    {
        InitializeComponent();
        _settings = settings;
        _controllers = settings.Controllers.Select(x => new ControllerSettings { UsbId = x.UsbId, LastPort = x.LastPort, Model = x.Model, IsAvailable = x.IsAvailable }).ToList();
        _drives = settings.Drives.Select(x => new DriveSettings { Id = x.Id, ControllerUsbId = x.ControllerUsbId, Selection = x.Selection, Size = x.Size, Density = x.Density, NominalRpm = x.NominalRpm }).ToList();
        ImagesFolderText.Text = settings.DefaultImagesFolder;
        GwPathText.Text = settings.GwExecutablePath;
        LanguageCombo.SelectedIndex = settings.Language == "en" ? 1 : 0;
        ThemeCombo.SelectedIndex = (int)settings.Theme;
        RefreshHardwareRows();
        DrivesGrid.ItemsSource = Hardware;
        foreach (var operation in new[] { "Read", "Write", "Convert" }) Profiles.Add(new($"default-{operation.ToLowerInvariant()}", operation, LocExtension.Get("Profile.Default"), true));
        foreach (var profile in settings.Profiles) Profiles.Add(new(profile.Id, profile.Operation, profile.Name, false));
        ProfilesGrid.ItemsSource = Profiles;
    }

    private void Navigation_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (GeneralPage is null) return;
        var pages = new FrameworkElement[] { GeneralPage, ToolsPage, HardwarePage, ProfilesPage };
        for (var index = 0; index < pages.Length; index++) pages[index].Visibility = index == Navigation.SelectedIndex ? Visibility.Visible : Visibility.Collapsed;
    }

    private void BrowseGw_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = LocExtension.Get("Options.ExecutableFilter") };
        if (dialog.ShowDialog(this) == true) GwPathText.Text = dialog.FileName;
    }

    private void BrowseImagesFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { Multiselect = false, Title = LocExtension.Get("Options.ImagesFolder") };
        if (dialog.ShowDialog(this) == true) ImagesFolderText.Text = dialog.FolderName;
    }

    private async void ScanHardware_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(GwPathText.Text) || !File.Exists(GwPathText.Text)) { MessageBox.Show(this, LocExtension.Get("Hardware.GwRequired"), LocExtension.Get("Hardware.ScanTitle")); return; }
        ScanButton.IsEnabled = false;
        try
        {
            foreach (var controller in _controllers) controller.IsAvailable = false;
            var discovery = new WindowsSerialDeviceDiscovery();
            foreach (var serial in discovery.FindSerialDevices())
            {
                var runner = new GreaseweazleRunner();
                var result = await runner.RunAsync(new GwCommand(GwPathText.Text, "info", ["--device", serial.Port]));
                if (!result.IsSuccess) continue;
                var parsed = GwInfoParser.Parse(string.Join(Environment.NewLine, result.Output.Select(x => x.Text)));
                var usbId = string.IsNullOrWhiteSpace(parsed.SerialNumber) ? serial.StableId : parsed.SerialNumber;
                var controller = _controllers.FirstOrDefault(x => x.UsbId == usbId);
                if (controller is null) { controller = new ControllerSettings { UsbId = usbId }; _controllers.Add(controller); }
                controller.LastPort = serial.Port;
                controller.Model = parsed.Model ?? serial.DisplayName;
                controller.IsAvailable = true;
            }
            RefreshHardwareRows();
        }
        catch (Exception exception) { MessageBox.Show(this, exception.Message, LocExtension.Get("Hardware.ScanTitle"), MessageBoxButton.OK, MessageBoxImage.Error); }
        finally { ScanButton.IsEnabled = true; }
    }

    private void AddDrive_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new DriveEditorWindow(_controllers) { Owner = this };
        if (dialog.ShowDialog() == true && dialog.Drive is not null) { _drives.Add(dialog.Drive); RefreshHardwareRows(); }
    }

    private void RemoveHardware_Click(object sender, RoutedEventArgs e)
    {
        if (DrivesGrid.SelectedItem is not HardwareRow row) return;
        if (row.DriveId is not null) _drives.RemoveAll(x => x.Id == row.DriveId);
        else if (MessageBox.Show(this, LocExtension.Get("Options.RemoveHardware"), LocExtension.Get("Options.HardwareTitle"), MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
        { _controllers.RemoveAll(x => x.UsbId == row.UsbId); _drives.RemoveAll(x => x.ControllerUsbId == row.UsbId); }
        RefreshHardwareRows();
    }

    private void RefreshHardwareRows()
    {
        Hardware.Clear();
        foreach (var controller in _controllers)
        {
            var drives = _drives.Where(x => x.ControllerUsbId == controller.UsbId).ToArray();
            if (drives.Length == 0) Hardware.Add(new(null, controller.LastPort, controller.UsbId, LocExtension.Get("Hardware.NoDrive"), controller.IsAvailable));
            foreach (var drive in drives) Hardware.Add(new(drive.Id, controller.LastPort, controller.UsbId, LocExtension.Get("Hardware.Description", drive.Size, drive.Density, drive.Selection, drive.NominalRpm is null ? "" : $" · {drive.NominalRpm} RPM"), controller.IsAvailable));
        }
    }

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
        _settings.DefaultImagesFolder = ImagesFolderText.Text.Trim();
        _settings.GwExecutablePath = string.IsNullOrWhiteSpace(GwPathText.Text) ? null : GwPathText.Text.Trim();
        _settings.Language = LanguageCombo.SelectedIndex == 1 ? "en" : "fr";
        _settings.Theme = (AppTheme)Math.Max(0, ThemeCombo.SelectedIndex);
        _settings.Controllers = _controllers;
        _settings.Drives = _drives;
        var retained = Profiles.Where(x => !x.IsSystem).ToDictionary(x => x.Id);
        _settings.Profiles = _settings.Profiles.Where(x => retained.ContainsKey(x.Id)).Select(x => { x.Name = retained[x.Id].Name; return x; }).ToList();
        DialogResult = true;
    }
}

public sealed record HardwareRow(string? DriveId, string Port, string UsbId, string Description, bool Available);
public sealed record ProfileOptionRow(string Id, string Operation, string Name, bool IsSystem)
{
    public string OperationLabel => Operation switch { "Read" => LocExtension.Get("Tab.Read"), "Write" => LocExtension.Get("Tab.Write"), "Convert" => LocExtension.Get("Tab.Convert"), _ => Operation };
}
