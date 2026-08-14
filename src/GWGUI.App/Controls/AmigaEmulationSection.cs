using System.Diagnostics;
using System.IO;
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
    private readonly ComboBox _configuration = new() { MinWidth = 320, DisplayMemberPath = nameof(ConfigurationItem.DisplayName) };
    private readonly TextBox _kickstart = new() { MinWidth = 320 };
    private readonly TextBox _disk = new() { MinWidth = 320 };
    private readonly Button _start = new() { MinWidth = 110 };
    private readonly Button _firmwareFolder = new() { MinWidth = 130 };
    private readonly TabControl _machines = new();

    public AmigaEmulationSection()
    {
        AutomationProperties.SetName(_model, "Amiga model");
        AutomationProperties.SetName(_configuration, "Amiga configuration");
        AutomationProperties.SetName(_kickstart, "Amiga Kickstart");
        AutomationProperties.SetName(_disk, "Amiga disk image");
        AutomationProperties.SetName(_start, "Start Amiga");
        AutomationProperties.SetName(_firmwareFolder, "Open Amiga firmware folder");
        AutomationProperties.SetName(_machines, "Running Amiga machines");
        _model.ItemsSource = AmigaModelCatalog.All;
        _model.SelectedItem = AmigaModelCatalog.Get("A500");
        _configuration.SelectionChanged += ConfigurationSelected;
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
        setup.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        AddRow(setup, 0, LocExtension.Get("Emulation.Configuration"), _configuration, null);
        AddRow(setup, 1, LocExtension.Get("Emulation.Model"), _model, null);
        AddRow(setup, 2, "Kickstart", _kickstart, BrowseKickstart);
        AddRow(setup, 3, "ADF", _disk, BrowseDisk);
        Grid.SetColumn(_firmwareFolder, 3); Grid.SetRow(_firmwareFolder, 1); setup.Children.Add(_firmwareFolder);
        Grid.SetColumn(_start, 3); Grid.SetRow(_start, 3); setup.Children.Add(_start);
        root.Children.Add(setup);
        Grid.SetRow(_machines, 1); root.Children.Add(_machines);
        Content = root;
        Loaded += async (_, _) =>
        {
            await ReloadConfigurationsAsync();
            LoadDefaultFirmware();
        };
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

    private async Task ReloadConfigurationsAsync()
    {
        var configurations = await new AmigaConfigurationStore(StoragePaths.AmigaConfigurationsDirectory).LoadAllAsync();
        _configuration.ItemsSource = configurations.Select(configuration => new ConfigurationItem(configuration)).ToArray();
        _configuration.SelectedIndex = configurations.Count > 0 ? 0 : -1;
    }

    private void ConfigurationSelected(object sender, SelectionChangedEventArgs e)
    {
        if (_configuration.SelectedItem is not ConfigurationItem item) return;
        var configuration = item.Configuration;
        _model.SelectedItem = AmigaModelCatalog.All.FirstOrDefault(model => model.Id == configuration.Model) ?? AmigaModelCatalog.Get("A500");
        _kickstart.Text = configuration.KickstartPath;
        _disk.Text = configuration.InitialDiskPath ?? string.Empty;
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
            var corePath = await AmigaCoreProvider.EnsureAvailableAsync();
            var engine = new AmigaEngine(StoragePaths.AmigaSessionsDirectory, corePath, () => new WasapiAudioOutput(),
                configuration => Path.Combine(StoragePaths.AmigaConfigurationsDirectory,
                    configuration.Id.ToString("N"), "Saves"));
            var saved = (_configuration.SelectedItem as ConfigurationItem)?.Configuration;
            var options = new Dictionary<string, string>(saved?.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal)
            {
                ["puae_model"] = model.Id
            };
            var configuration = new AmigaMachineConfiguration(model.Id, _kickstart.Text,
                string.IsNullOrWhiteSpace(_disk.Text) ? null : _disk.Text,
                saved?.ExtendedRomPath, saved?.RomKeyPath, saved?.Core ?? AmigaCoreKind.External,
                options, saved?.Id ?? Guid.NewGuid(), saved?.AudioEnabled ?? true, saved?.Controllers, saved?.Input,
                saved?.Floppies, saved?.MountFloppiesInSeparateDrives ?? false);
            var machine = engine.CreateAmigaMachine(configuration);
            var view = new AmigaMachineView(machine);
            var tab = new TabItem { Header = model.DisplayName, Content = view };
            view.CloseRequested += async (_, _) => { await view.StopAsync(); _machines.Items.Remove(tab); };
            _machines.Items.Add(tab);
            _machines.SelectedItem = tab;
            try { await view.StartAsync(); }
            catch
            {
                await view.StopAsync();
                _machines.Items.Remove(tab);
                throw;
            }
        }
        catch (Exception error)
        {
            var logPath = ErrorLog.Write(error, "Amiga emulator");
            var detail = logPath is null ? LocExtension.Get("Common.Unknown") : LocExtension.Get("Error.LogSaved", logPath);
            MessageBox.Show(Window.GetWindow(this), LocExtension.Get("Error.Unexpected", detail), "Amiga", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally { _start.IsEnabled = true; }
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

    private sealed record ConfigurationItem(AmigaMachineConfiguration Configuration)
    {
        public string DisplayName => $"{Configuration.Model} · {Configuration.Id.ToString("N")[..8]}";
    }
}
