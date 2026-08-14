using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Localization;
using GWGUI.App.Services;
using GWGUI.Emulation.Amiga;
using Microsoft.Win32;

namespace GWGUI.App.Controls;

public sealed class OptionsEmulationSection : UserControl
{
    private readonly AmigaConfigurationStore _store = new(StoragePaths.AmigaConfigurationsDirectory, StoragePaths.DataDirectory);
    private readonly ObservableCollection<ConfigurationItem> _configurations = [];
    private readonly ObservableCollection<FirmwareItem> _firmware = [];
    private readonly ObservableCollection<OptionItem> _options = [];
    private readonly ObservableCollection<FloppyItem> _floppies = [];
    private readonly ListBox _list = new() { MinWidth = 260 };
    private readonly ListBox _firmwareList = new() { MinWidth = 260 };
    private readonly ComboBox _model = new() { ItemsSource = AmigaModelCatalog.All, DisplayMemberPath = nameof(AmigaModel.DisplayName) };
    private readonly TextBox _kickstart = new();
    private readonly TextBox _extendedRom = new();
    private readonly TextBox _romKey = new();
    private readonly TextBox _disk = new();
    private readonly CheckBox _audio = new() { IsChecked = true };
    private readonly ComboBox[] _controllers = Enumerable.Range(0, 4).Select(_ => new ComboBox
    {
        ItemsSource = Enum.GetValues<AmigaControllerType>(),
        SelectedItem = AmigaControllerType.Automatic
    }).ToArray();
    private readonly DataGrid _optionGrid = new() { AutoGenerateColumns = true, CanUserAddRows = true, CanUserDeleteRows = true };
    private readonly DataGrid _floppyGrid = new() { AutoGenerateColumns = true, CanUserAddRows = true, CanUserDeleteRows = true };
    private readonly CheckBox _multiDrive = new();
    private Guid _currentId;
    private bool _loading;

    public OptionsEmulationSection()
    {
        var root = new Grid { Margin = new Thickness(12) };
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(280) });
        root.ColumnDefinitions.Add(new ColumnDefinition());

        var left = new Grid { Margin = new Thickness(0, 0, 12, 0) };
        left.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        left.RowDefinitions.Add(new RowDefinition());
        left.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        left.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        left.RowDefinitions.Add(new RowDefinition());
        var configurationsLabel = new TextBlock { Text = LocExtension.Get("Emulation.Configurations"), FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 6) };
        left.Children.Add(configurationsLabel);
        var leftButtons = new WrapPanel { Margin = new Thickness(0, 8, 0, 0) };
        AddButton(leftButtons, "Common.New", NewConfiguration);
        AddButton(leftButtons, "Common.Delete", DeleteConfigurationAsync);
        Grid.SetRow(leftButtons, 2);
        left.Children.Add(leftButtons);
        _list.ItemsSource = _configurations;
        _list.DisplayMemberPath = nameof(ConfigurationItem.DisplayName);
        _list.SelectionChanged += ConfigurationSelected;
        Grid.SetRow(_list, 1);
        left.Children.Add(_list);
        var firmwareLabel = new TextBlock { Text = LocExtension.Get("Emulation.Firmware"), FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 12, 0, 6) };
        Grid.SetRow(firmwareLabel, 3); left.Children.Add(firmwareLabel);
        _firmwareList.ItemsSource = _firmware;
        _firmwareList.DisplayMemberPath = nameof(FirmwareItem.DisplayName);
        _firmwareList.SelectionChanged += FirmwareSelected;
        Grid.SetRow(_firmwareList, 4); left.Children.Add(_firmwareList);
        root.Children.Add(left);

        var right = new Grid();
        right.RowDefinitions.Add(new RowDefinition());
        right.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        var form = new Grid { Margin = new Thickness(4) };
        form.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
        form.ColumnDefinitions.Add(new ColumnDefinition());
        form.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        for (var i = 0; i < 12; i++) form.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        AddField(form, 0, LocExtension.Get("Emulation.Model"), _model);
        AddPathField(form, 1, "Kickstart", _kickstart, "ROM|*.rom;*.bin|All files|*.*");
        AddPathField(form, 2, LocExtension.Get("Emulation.ExtendedRom"), _extendedRom, "ROM|*.rom;*.bin|All files|*.*");
        AddPathField(form, 3, LocExtension.Get("Emulation.RomKey"), _romKey, "ROM key|*.key|All files|*.*");
        AddPathField(form, 4, LocExtension.Get("Emulation.InitialDisk"), _disk, "Amiga disk|*.adf;*.adz;*.ipf;*.dms;*.hdf;*.lha;*.iso;*.cue|All files|*.*");
        _audio.Content = LocExtension.Get("Emulation.Audio");
        AddField(form, 5, LocExtension.Get("Emulation.Audio"), _audio);
        for (var port = 0; port < 4; port++) AddField(form, 6 + port, LocExtension.Get("Emulation.Controller", port + 1), _controllers[port]);
        var floppyLabel = new TextBlock { Text = LocExtension.Get("Emulation.Floppies"), Margin = new Thickness(0, 8, 12, 4) };
        Grid.SetRow(floppyLabel, 10); form.Children.Add(floppyLabel);
        var floppyPanel = new StackPanel { Margin = new Thickness(0, 8, 0, 4) };
        _floppyGrid.ItemsSource = _floppies;
        _floppyGrid.MinHeight = 120;
        floppyPanel.Children.Add(_floppyGrid);
        _multiDrive.Content = LocExtension.Get("Emulation.MultiDrive");
        floppyPanel.Children.Add(_multiDrive);
        Grid.SetRow(floppyPanel, 10); Grid.SetColumn(floppyPanel, 1); Grid.SetColumnSpan(floppyPanel, 2); form.Children.Add(floppyPanel);
        var optionsLabel = new TextBlock { Text = LocExtension.Get("Emulation.CoreOptions"), Margin = new Thickness(0, 8, 12, 4) };
        Grid.SetRow(optionsLabel, 11); form.Children.Add(optionsLabel);
        _optionGrid.ItemsSource = _options;
        _optionGrid.MinHeight = 180;
        _optionGrid.Margin = new Thickness(0, 8, 0, 4);
        Grid.SetRow(_optionGrid, 11); Grid.SetColumn(_optionGrid, 1); Grid.SetColumnSpan(_optionGrid, 2); form.Children.Add(_optionGrid);
        scroll.Content = form;
        right.Children.Add(scroll);

        var actions = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Right };
        AddButton(actions, "Common.Refresh", ReloadAsync);
        AddButton(actions, "Emulation.LoadOptions", LoadAvailableOptionsAsync);
        AddButton(actions, "Common.Save", SaveConfigurationAsync);
        AddButton(actions, "Common.OpenFolder", OpenFirmwareFolder);
        Grid.SetRow(actions, 1); right.Children.Add(actions);
        Grid.SetColumn(right, 1); root.Children.Add(right);
        Content = root;
        Loaded += async (_, _) => await ReloadAsync();
    }

    private static void AddButton(Panel panel, string resourceKey, Func<Task> action)
    {
        var button = new Button { Content = LocExtension.Get(resourceKey), MinWidth = 100 };
        button.Click += async (_, _) =>
        {
            try { button.IsEnabled = false; await action(); }
            catch (Exception error) { ShowError(button, error); }
            finally { button.IsEnabled = true; }
        };
        panel.Children.Add(button);
    }

    private static void AddField(Grid grid, int row, string label, Control control)
    {
        var text = new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 5, 12, 5) };
        Grid.SetRow(text, row); grid.Children.Add(text);
        control.Margin = new Thickness(0, 4, 0, 4);
        Grid.SetRow(control, row); Grid.SetColumn(control, 1); Grid.SetColumnSpan(control, 2); grid.Children.Add(control);
    }

    private static void AddPathField(Grid grid, int row, string label, TextBox textBox, string filter)
    {
        AddField(grid, row, label, textBox);
        Grid.SetColumnSpan(textBox, 1);
        var browse = new Button { Content = LocExtension.Get("Common.Browse"), MinWidth = 100 };
        browse.Click += (_, _) =>
        {
            var dialog = new OpenFileDialog { Filter = filter, CheckFileExists = true };
            if (dialog.ShowDialog() == true) textBox.Text = dialog.FileName;
        };
        Grid.SetRow(browse, row); Grid.SetColumn(browse, 2); grid.Children.Add(browse);
    }

    public async Task ReloadAsync()
    {
        if (_loading) return;
        _loading = true;
        try
        {
            var selectedId = _currentId;
            var loaded = await _store.LoadAllAsync();
            _configurations.Clear();
            foreach (var configuration in loaded) _configurations.Add(new ConfigurationItem(configuration));
            var selected = _configurations.FirstOrDefault(item => item.Configuration.Id == selectedId)
                ?? _configurations.FirstOrDefault();
            _list.SelectedItem = selected;
            if (selected is null) _ = NewConfiguration();
            else LoadEditor(selected.Configuration);
            _firmware.Clear();
            foreach (var entry in new AmigaFirmwareCatalog(StoragePaths.AmigaFirmwareDirectory).Scan())
                _firmware.Add(new FirmwareItem(entry));
        }
        finally { _loading = false; }
    }

    private Task NewConfiguration()
    {
        LoadEditor(AmigaMachineConfiguration.A500(string.Empty) with { Id = Guid.NewGuid() });
        _list.SelectedItem = null;
        return Task.CompletedTask;
    }

    private void ConfigurationSelected(object sender, SelectionChangedEventArgs e)
    {
        if (!_loading && _list.SelectedItem is ConfigurationItem item) LoadEditor(item.Configuration);
    }

    private void FirmwareSelected(object sender, SelectionChangedEventArgs e)
    {
        if (_firmwareList.SelectedItem is not FirmwareItem item) return;
        switch (item.Firmware.Type)
        {
            case AmigaFirmwareType.ExtendedRom: _extendedRom.Text = item.Firmware.Path; break;
            case AmigaFirmwareType.RomKey: _romKey.Text = item.Firmware.Path; break;
            default: _kickstart.Text = item.Firmware.Path; break;
        }
    }

    private void LoadEditor(AmigaMachineConfiguration configuration)
    {
        _currentId = configuration.Id;
        _model.SelectedItem = AmigaModelCatalog.All.First(model => model.Id == configuration.Model);
        _kickstart.Text = configuration.KickstartPath;
        _extendedRom.Text = configuration.ExtendedRomPath ?? string.Empty;
        _romKey.Text = configuration.RomKeyPath ?? string.Empty;
        _disk.Text = configuration.InitialDiskPath ?? string.Empty;
        _audio.IsChecked = configuration.AudioEnabled;
        for (var port = 0; port < _controllers.Length; port++)
            _controllers[port].SelectedItem = configuration.Controllers?.ElementAtOrDefault(port) ?? AmigaControllerType.Automatic;
        _options.Clear();
        foreach (var option in configuration.Options ?? new Dictionary<string, string>())
            _options.Add(new OptionItem { Key = option.Key, Value = option.Value });
        _floppies.Clear();
        foreach (var floppy in configuration.Floppies ?? [])
            _floppies.Add(new FloppyItem { Path = floppy.Path, Label = floppy.Label ?? string.Empty, IsReadOnly = floppy.IsReadOnly });
        _multiDrive.IsChecked = configuration.MountFloppiesInSeparateDrives;
    }

    private async Task SaveConfigurationAsync()
    {
        if (_model.SelectedItem is not AmigaModel model) throw new InvalidOperationException(LocExtension.Get("Emulation.ModelRequired"));
        if (string.IsNullOrWhiteSpace(_kickstart.Text)) throw new InvalidOperationException(LocExtension.Get("Emulation.KickstartRequired"));
        ValidateOptionalFile(_kickstart.Text, required: true);
        ValidateOptionalFile(_extendedRom.Text);
        ValidateOptionalFile(_romKey.Text);
        ValidateOptionalFile(_disk.Text);
        var options = _options.Where(item => !string.IsNullOrWhiteSpace(item.Key))
            .ToDictionary(item => item.Key.Trim(), item => item.Value?.Trim() ?? string.Empty, StringComparer.Ordinal);
        options["puae_model"] = model.Id;
        var floppies = _floppies.Where(item => !string.IsNullOrWhiteSpace(item.Path)).Select(item =>
        {
            ValidateOptionalFile(item.Path, required: true);
            return new AmigaFloppyConfiguration(Path.GetFullPath(item.Path), string.IsNullOrWhiteSpace(item.Label) ? null : item.Label.Trim(), item.IsReadOnly);
        }).ToArray();
        var configuration = new AmigaMachineConfiguration(model.Id, Path.GetFullPath(_kickstart.Text),
            OptionalFullPath(_disk.Text), OptionalFullPath(_extendedRom.Text), OptionalFullPath(_romKey.Text),
            Options: options, Id: _currentId == Guid.Empty ? Guid.NewGuid() : _currentId,
            AudioEnabled: _audio.IsChecked == true,
            Controllers: _controllers.Select(combo => (AmigaControllerType)(combo.SelectedItem ?? AmigaControllerType.Automatic)).ToArray(),
            Floppies: floppies.Length == 0 ? null : floppies,
            MountFloppiesInSeparateDrives: floppies.Length > 1 && _multiDrive.IsChecked == true);
        await _store.SaveAsync(configuration);
        _currentId = configuration.Id;
        await ReloadAsync();
    }

    private async Task LoadAvailableOptionsAsync()
    {
        if (_model.SelectedItem is not AmigaModel model) throw new InvalidOperationException(LocExtension.Get("Emulation.ModelRequired"));
        ValidateOptionalFile(_kickstart.Text, required: true);
        ValidateOptionalFile(_extendedRom.Text);
        ValidateOptionalFile(_romKey.Text);
        var configured = _options.Where(item => !string.IsNullOrWhiteSpace(item.Key))
            .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
        var corePath = await AmigaCoreProvider.EnsureAvailableAsync();
        var configuration = new AmigaMachineConfiguration(model.Id, Path.GetFullPath(_kickstart.Text),
            ExtendedRomPath: OptionalFullPath(_extendedRom.Text), RomKeyPath: OptionalFullPath(_romKey.Text),
            Options: new Dictionary<string, string> { ["puae_model"] = model.Id },
            Id: _currentId == Guid.Empty ? Guid.NewGuid() : _currentId, AudioEnabled: false);
        var engine = new AmigaEngine(StoragePaths.AmigaSessionsDirectory, corePath,
            hostExecutablePath: Environment.ProcessPath);
        await using var machine = engine.CreateAmigaMachine(configuration);
        await machine.StartAsync();
        try
        {
            var available = machine.AvailableOptions.ToArray();
            _options.Clear();
            foreach (var option in available)
                _options.Add(new OptionItem
                {
                    Key = option.Key,
                    Name = option.Name,
                    Value = configured.TryGetValue(option.Key, out var value) ? value : option.DefaultValue,
                    AllowedValues = string.Join(" | ", option.Values.Select(item => item.Value))
                });
        }
        finally { await machine.StopAsync(); }
    }

    private async Task DeleteConfigurationAsync()
    {
        if (_currentId == Guid.Empty || _list.SelectedItem is null) return;
        if (MessageBox.Show(Window.GetWindow(this), LocExtension.Get("Emulation.DeleteConfirm"), "Amiga",
            MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        _store.Delete(_currentId);
        _currentId = Guid.Empty;
        await ReloadAsync();
    }

    private static string? OptionalFullPath(string value) => string.IsNullOrWhiteSpace(value) ? null : Path.GetFullPath(value);
    private static void ValidateOptionalFile(string value, bool required = false)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            if (required) throw new FileNotFoundException(LocExtension.Get("Emulation.FileRequired"));
            return;
        }
        if (!File.Exists(value)) throw new FileNotFoundException(LocExtension.Get("Emulation.FileMissing"), value);
    }

    private Task OpenFirmwareFolder()
    {
        Directory.CreateDirectory(StoragePaths.AmigaFirmwareDirectory);
        Process.Start(new ProcessStartInfo(StoragePaths.AmigaFirmwareDirectory) { UseShellExecute = true });
        return Task.CompletedTask;
    }

    private static void ShowError(FrameworkElement owner, Exception error)
    {
        var path = ErrorLog.Write(error, "Amiga configuration");
        var detail = path is null ? LocExtension.Get("Common.Unknown") : LocExtension.Get("Error.LogSaved", path);
        MessageBox.Show(Window.GetWindow(owner), LocExtension.Get("Error.Unexpected", detail), "Amiga", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private sealed record ConfigurationItem(AmigaMachineConfiguration Configuration)
    {
        public string DisplayName => $"{Configuration.Model} · {Configuration.Id.ToString("N")[..8]} · {Path.GetFileName(Configuration.KickstartPath)}";
    }

    private sealed record FirmwareItem(AmigaFirmware Firmware)
    {
        public string DisplayName
        {
            get
            {
                var identity = Firmware.Version ?? LocExtension.Get("Common.Unknown");
                var models = Firmware.CompatibleModels.Count == 0 ? string.Empty : $" · {string.Join(", ", Firmware.CompatibleModels)}";
                return $"{Path.GetFileName(Firmware.Path)} · {identity} · {Firmware.Size:N0} B · MD5 {Firmware.Md5} · SHA-256 {Firmware.Sha256}{models}";
            }
        }
    }

    public sealed class OptionItem
    {
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string AllowedValues { get; set; } = string.Empty;
    }

    public sealed class FloppyItem
    {
        public string Path { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public bool IsReadOnly { get; set; }
    }
}
