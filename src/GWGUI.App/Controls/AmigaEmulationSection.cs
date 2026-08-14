using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using GWGUI.App.Localization;
using GWGUI.App.Services;
using GWGUI.Emulation.Amiga;
using Microsoft.Win32;

namespace GWGUI.App.Controls;

public sealed class AmigaEmulationSection : UserControl
{
    private readonly ComboBox _model = new() { Width = 230, DisplayMemberPath = nameof(AmigaModel.DisplayName) };
    private readonly TextBox _kickstart = new() { MinWidth = 320 };
    private readonly TextBox _disk = new() { MinWidth = 320 };
    private readonly Button _start = new() { MinWidth = 110 };
    private readonly Button _firmwareFolder = new() { MinWidth = 130 };
    private readonly TabControl _machines = new();
    private readonly HttpClient _httpClient = new();

    public AmigaEmulationSection()
    {
        AutomationProperties.SetName(_model, "Amiga model");
        AutomationProperties.SetName(_kickstart, "Amiga Kickstart");
        AutomationProperties.SetName(_disk, "Amiga disk image");
        AutomationProperties.SetName(_start, "Start Amiga");
        AutomationProperties.SetName(_firmwareFolder, "Open Amiga firmware folder");
        AutomationProperties.SetName(_machines, "Running Amiga machines");
        _model.ItemsSource = AmigaModelCatalog.All;
        _model.SelectedItem = AmigaModelCatalog.Get("A500");
        _start.Content = LocExtension.Get("Common.Execute");
        _firmwareFolder.Content = "Firmware";
        _start.Click += StartClick;
        _firmwareFolder.Click += OpenFirmwareFolder;

        var root = new Grid { Margin = new Thickness(16) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition());
        var setup = new Grid { Margin = new Thickness(0, 0, 0, 12) };
        setup.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        setup.ColumnDefinitions.Add(new ColumnDefinition());
        setup.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        setup.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        setup.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        setup.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        setup.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        AddRow(setup, 0, "Modèle", _model, null);
        AddRow(setup, 1, "Kickstart", _kickstart, BrowseKickstart);
        AddRow(setup, 2, "ADF", _disk, BrowseDisk);
        Grid.SetColumn(_firmwareFolder, 3); Grid.SetRow(_firmwareFolder, 0); setup.Children.Add(_firmwareFolder);
        Grid.SetColumn(_start, 3); Grid.SetRow(_start, 2); setup.Children.Add(_start);
        root.Children.Add(setup);
        Grid.SetRow(_machines, 1); root.Children.Add(_machines);
        Content = root;
        Loaded += (_, _) => LoadDefaultFirmware();
    }

    private static void AddRow(Grid grid, int row, string label, Control input, RoutedEventHandler? browse)
    {
        var text = new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 5, 12, 5) };
        Grid.SetRow(text, row); grid.Children.Add(text);
        input.Margin = new Thickness(0, 4, 8, 4); Grid.SetRow(input, row); Grid.SetColumn(input, 1); grid.Children.Add(input);
        if (browse is null) return;
        var button = new Button { Content = LocExtension.Get("Common.Browse"), MinWidth = 100, Margin = new Thickness(0, 4, 8, 4) };
        button.Click += browse; Grid.SetRow(button, row); Grid.SetColumn(button, 2); grid.Children.Add(button);
    }

    private void LoadDefaultFirmware()
    {
        Directory.CreateDirectory(StoragePaths.AmigaFirmwareDirectory);
        if (_kickstart.Text.Length > 0) return;
        var firmware = new AmigaFirmwareCatalog(StoragePaths.AmigaFirmwareDirectory).Scan()
            .FirstOrDefault(entry => entry.Type is AmigaFirmwareType.Kickstart or AmigaFirmwareType.Unknown);
        if (firmware is not null) _kickstart.Text = firmware.Path;
    }

    private void BrowseKickstart(object sender, RoutedEventArgs e) => Browse(_kickstart, "ROM|*.rom;*.bin|All files|*.*");
    private void BrowseDisk(object sender, RoutedEventArgs e) => Browse(_disk, "Amiga disk|*.adf;*.adz;*.ipf;*.dms;*.hdf;*.lha;*.iso;*.cue|All files|*.*");

    private static void Browse(TextBox target, string filter)
    {
        var dialog = new OpenFileDialog { Filter = filter, CheckFileExists = true };
        if (dialog.ShowDialog() == true) target.Text = dialog.FileName;
    }

    private async void StartClick(object sender, RoutedEventArgs e)
    {
        try
        {
            _start.IsEnabled = false;
            if (_model.SelectedItem is not AmigaModel model) return;
            if (!File.Exists(_kickstart.Text)) throw new FileNotFoundException("Kickstart", _kickstart.Text);
            if (_disk.Text.Length > 0 && !File.Exists(_disk.Text)) throw new FileNotFoundException("ADF", _disk.Text);
            var corePath = await EnsureCoreAsync();
            var engine = new AmigaEngine(StoragePaths.AmigaSessionsDirectory, corePath, () => new WasapiAudioOutput());
            var configuration = new AmigaMachineConfiguration(model.Id, _kickstart.Text,
                string.IsNullOrWhiteSpace(_disk.Text) ? null : _disk.Text, Id: Guid.NewGuid());
            var machine = engine.CreateAmigaMachine(configuration);
            var view = new AmigaMachineView(machine);
            var tab = new TabItem { Header = model.DisplayName, Content = view };
            view.CloseRequested += async (_, _) => { await view.StopAsync(); _machines.Items.Remove(tab); };
            _machines.Items.Add(tab);
            _machines.SelectedItem = tab;
            await view.StartAsync();
        }
        catch (Exception error)
        {
            var logPath = ErrorLog.Write(error, "Amiga emulator");
            var detail = logPath is null ? LocExtension.Get("Common.Unknown") : LocExtension.Get("Error.LogSaved", logPath);
            MessageBox.Show(Window.GetWindow(this), LocExtension.Get("Error.Unexpected", detail), "Amiga", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally { _start.IsEnabled = true; }
    }

    private async Task<string> EnsureCoreAsync()
    {
        var bundled = Path.Combine(AppContext.BaseDirectory, "Emulation", "puae_libretro.dll");
        if (File.Exists(bundled)) return bundled;
        var installer = new AmigaExternalCoreInstaller(_httpClient, StoragePaths.AmigaCoreDirectory);
        return installer.IsInstalled ? installer.LibraryPath : await installer.InstallAsync();
    }

    private void OpenFirmwareFolder(object sender, RoutedEventArgs e)
    {
        Directory.CreateDirectory(StoragePaths.AmigaFirmwareDirectory);
        Process.Start(new ProcessStartInfo(StoragePaths.AmigaFirmwareDirectory) { UseShellExecute = true });
    }

    public async Task StopAllAsync()
    {
        foreach (var view in _machines.Items.OfType<TabItem>().Select(item => item.Content).OfType<AmigaMachineView>().ToArray())
            await view.StopAsync();
    }
}
