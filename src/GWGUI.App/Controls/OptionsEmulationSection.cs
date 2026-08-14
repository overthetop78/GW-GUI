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
    private readonly DataGrid _optionGrid = new() { AutoGenerateColumns = false, CanUserAddRows = true, CanUserDeleteRows = true };
    private readonly DataGrid _floppyGrid = new() { AutoGenerateColumns = false, CanUserAddRows = true, CanUserDeleteRows = true };
    private readonly CheckBox _multiDrive = new();
    private readonly TextBlock _cpuSummary = new() { TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _ramSummary = new() { TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _videoSummary = new() { TextWrapping = TextWrapping.Wrap };
    private readonly TextBox _mouseDevice = new();
    private readonly CheckBox _captureMouse = new() { IsChecked = true };
    private readonly TextBox[] _controllerDevices = Enumerable.Range(0, 4).Select(_ => new TextBox()).ToArray();
    private readonly ObservableCollection<KeyMappingItem> _keyboardMappings = [];
    private readonly DataGrid _keyboardGrid = new() { AutoGenerateColumns = false, CanUserAddRows = true, CanUserDeleteRows = true };
    private Guid _currentId;
    private bool _loading;
    private IReadOnlyList<AmigaMediaConfiguration>? _loadedMedia;

    public OptionsEmulationSection()
    {
        ConfigureGrids();
        _list.ItemsSource = _configurations;
        _list.DisplayMemberPath = nameof(ConfigurationItem.DisplayName);
        _list.SelectionChanged += ConfigurationSelected;
        _firmwareList.ItemsSource = _firmware;
        _firmwareList.DisplayMemberPath = nameof(FirmwareItem.DisplayName);
        _firmwareList.SelectionChanged += FirmwareSelected;
        _model.SelectionChanged += (_, _) => RefreshModelSummaries();
        _optionGrid.ItemsSource = _options;
        _floppyGrid.ItemsSource = _floppies;
        _keyboardGrid.ItemsSource = _keyboardMappings;
        _audio.Content = LocExtension.Get("Emulation.Audio");
        _multiDrive.Content = LocExtension.Get("Emulation.MultiDrive");
        _captureMouse.Content = LocExtension.Get("Emulation.CaptureMouse");

        var families = new TabControl { Margin = new Thickness(8) };
        families.Items.Add(new TabItem
        {
            Header = LocExtension.Get("Emulation.Configurations"),
            Content = BuildConfigurationCatalog()
        });
        families.Items.Add(new TabItem { Header = "Amiga", Content = BuildAmigaEditor() });
        Content = families;
        Loaded += async (_, _) => await ReloadAsync();
    }

    private UIElement BuildConfigurationCatalog()
    {
        var root = new Grid { Margin = new Thickness(12) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition());
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.Children.Add(new TextBlock
        {
            Text = LocExtension.Get("Emulation.ConfigurationsDescription", "Amiga"),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 10)
        });
        Grid.SetRow(_list, 1);
        root.Children.Add(_list);
        var actions = new WrapPanel { Margin = new Thickness(0, 10, 0, 0) };
        AddButton(actions, "Common.New", NewConfiguration);
        AddButton(actions, "Common.Delete", DeleteConfigurationAsync);
        AddButton(actions, "Common.Refresh", ReloadAsync);
        Grid.SetRow(actions, 2);
        root.Children.Add(actions);
        return root;
    }

    private UIElement BuildAmigaEditor()
    {
        var root = new Grid { Margin = new Thickness(8) };
        root.RowDefinitions.Add(new RowDefinition());
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var tabs = new TabControl();
        tabs.Items.Add(new TabItem { Header = LocExtension.Get("Emulation.GeneralTab"), Content = BuildGeneralTab() });
        tabs.Items.Add(new TabItem { Header = "CPU", Content = WrapSummary(_cpuSummary) });
        tabs.Items.Add(new TabItem { Header = "RAM", Content = WrapSummary(_ramSummary) });
        tabs.Items.Add(new TabItem { Header = "ROM", Content = BuildRomTab() });
        tabs.Items.Add(new TabItem { Header = LocExtension.Get("Emulation.VideoTab"), Content = WrapSummary(_videoSummary) });
        tabs.Items.Add(new TabItem { Header = LocExtension.Get("Emulation.Audio"), Content = BuildAudioTab() });
        tabs.Items.Add(new TabItem { Header = LocExtension.Get("Emulation.StorageTab"), Content = BuildStorageTab() });
        tabs.Items.Add(new TabItem { Header = LocExtension.Get("Emulation.KeyboardTab"), Content = BuildKeyboardTab() });
        tabs.Items.Add(new TabItem { Header = LocExtension.Get("Emulation.MouseTab"), Content = BuildMouseTab() });
        tabs.Items.Add(new TabItem { Header = LocExtension.Get("Emulation.ControllersTab"), Content = BuildControllersTab() });
        tabs.Items.Add(new TabItem { Header = LocExtension.Get("Emulation.AdvancedTab"), Content = BuildAdvancedTab() });
        tabs.Items.Add(new TabItem { Header = LocExtension.Get("Emulation.Core"), Content = new AmigaCoreManagementSection() });
        root.Children.Add(tabs);
        var actions = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 10, 0, 0) };
        AddButton(actions, "Common.Save", SaveConfigurationAsync);
        Grid.SetRow(actions, 1);
        root.Children.Add(actions);
        return root;
    }

    private UIElement BuildGeneralTab()
    {
        var form = CreateForm(1);
        AddField(form, 0, LocExtension.Get("Emulation.Model"), _model);
        return Wrap(form);
    }

    private UIElement BuildRomTab()
    {
        var root = new Grid { Margin = new Thickness(12) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition());
        var form = CreateForm(3);
        AddPathField(form, 0, "Kickstart", _kickstart, "ROM|*.rom;*.bin|All files|*.*");
        AddPathField(form, 1, LocExtension.Get("Emulation.ExtendedRom"), _extendedRom, "ROM|*.rom;*.bin|All files|*.*");
        AddPathField(form, 2, LocExtension.Get("Emulation.RomKey"), _romKey, "ROM key|*.key|All files|*.*");
        root.Children.Add(form);
        var firmware = new Grid { Margin = new Thickness(0, 12, 0, 0) };
        firmware.RowDefinitions.Add(new RowDefinition());
        firmware.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        firmware.Children.Add(_firmwareList);
        var actions = new WrapPanel { Margin = new Thickness(0, 8, 0, 0) };
        AddButton(actions, "Common.OpenFolder", OpenFirmwareFolder);
        Grid.SetRow(actions, 1);
        firmware.Children.Add(actions);
        Grid.SetRow(firmware, 1);
        root.Children.Add(firmware);
        return root;
    }

    private UIElement BuildAudioTab()
    {
        var panel = new StackPanel { Margin = new Thickness(16) };
        panel.Children.Add(_audio);
        return panel;
    }

    private UIElement BuildStorageTab()
    {
        var root = new Grid { Margin = new Thickness(12) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition());
        var form = CreateForm(1);
        AddPathField(form, 0, LocExtension.Get("Emulation.InitialDisk"), _disk,
            "Amiga media|*.adf;*.adz;*.dms;*.fdi;*.ipf;*.raw;*.hdf;*.hdz;*.lha;*.slave;*.info;*.cue;*.ccd;*.chd;*.nrg;*.mds;*.iso;*.uae;*.m3u;*.zip;*.7z|All files|*.*");
        root.Children.Add(form);
        var panel = new StackPanel { Margin = new Thickness(0, 12, 0, 0) };
        _floppyGrid.MinHeight = 180;
        panel.Children.Add(_floppyGrid);
        panel.Children.Add(_multiDrive);
        Grid.SetRow(panel, 1);
        root.Children.Add(panel);
        return root;
    }

    private UIElement BuildKeyboardTab()
    {
        _keyboardGrid.MinHeight = 280;
        return new Grid { Margin = new Thickness(12), Children = { _keyboardGrid } };
    }

    private UIElement BuildMouseTab()
    {
        var form = CreateForm(2);
        AddField(form, 0, LocExtension.Get("Emulation.DeviceId"), _mouseDevice);
        AddField(form, 1, LocExtension.Get("Emulation.CaptureMouse"), _captureMouse);
        return Wrap(form);
    }

    private UIElement BuildControllersTab()
    {
        var form = CreateForm(8);
        for (var port = 0; port < 4; port++)
        {
            AddField(form, port * 2, LocExtension.Get("Emulation.Controller", port + 1), _controllers[port]);
            AddField(form, port * 2 + 1, LocExtension.Get("Emulation.ControllerDevice", port + 1), _controllerDevices[port]);
        }
        return Wrap(form);
    }

    private UIElement BuildAdvancedTab()
    {
        var root = new Grid { Margin = new Thickness(12) };
        root.RowDefinitions.Add(new RowDefinition());
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _optionGrid.MinHeight = 260;
        root.Children.Add(_optionGrid);
        var actions = new WrapPanel { Margin = new Thickness(0, 8, 0, 0) };
        AddButton(actions, "Emulation.LoadOptions", LoadAvailableOptionsAsync);
        Grid.SetRow(actions, 1);
        root.Children.Add(actions);
        return root;
    }

    private static Grid CreateForm(int rows)
    {
        var form = new Grid { Margin = new Thickness(12) };
        form.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
        form.ColumnDefinitions.Add(new ColumnDefinition());
        form.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        for (var index = 0; index < rows; index++) form.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        return form;
    }

    private static UIElement Wrap(UIElement child) => new ScrollViewer
    {
        Content = child,
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto
    };

    private static UIElement WrapSummary(TextBlock summary)
    {
        summary.Margin = new Thickness(20);
        summary.FontSize = 16;
        return summary;
    }

    private void ConfigureGrids()
    {
        _optionGrid.Columns.Add(new DataGridTextColumn { Header = "Catégorie", Binding = new System.Windows.Data.Binding(nameof(OptionItem.Category)), IsReadOnly = true, Width = 130 });
        _optionGrid.Columns.Add(new DataGridTextColumn { Header = "Option", Binding = new System.Windows.Data.Binding(nameof(OptionItem.Name)), IsReadOnly = true, Width = 220 });
        _optionGrid.Columns.Add(new DataGridTextColumn { Header = "Clé", Binding = new System.Windows.Data.Binding(nameof(OptionItem.Key)), Width = 220 });
        _optionGrid.Columns.Add(new DataGridTextColumn { Header = "Valeur", Binding = new System.Windows.Data.Binding(nameof(OptionItem.Value)) { UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged }, Width = 150 });
        _optionGrid.Columns.Add(new DataGridTextColumn { Header = "Valeurs autorisées", Binding = new System.Windows.Data.Binding(nameof(OptionItem.AllowedValues)), IsReadOnly = true, Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
        _floppyGrid.Columns.Add(new DataGridTextColumn { Header = "Image", Binding = new System.Windows.Data.Binding(nameof(FloppyItem.Path)) { UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged }, Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
        _floppyGrid.Columns.Add(new DataGridTextColumn { Header = "Libellé", Binding = new System.Windows.Data.Binding(nameof(FloppyItem.Label)) { UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged }, Width = 150 });
        _floppyGrid.Columns.Add(new DataGridCheckBoxColumn { Header = "Lecture seule", Binding = new System.Windows.Data.Binding(nameof(FloppyItem.IsReadOnly)), Width = 100 });
        _keyboardGrid.Columns.Add(new DataGridTextColumn
        {
            Header = LocExtension.Get("Emulation.SystemKey", "Amiga"),
            Binding = new System.Windows.Data.Binding(nameof(KeyMappingItem.AmigaKey)) { UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged },
            Width = new DataGridLength(1, DataGridLengthUnitType.Star)
        });
        _keyboardGrid.Columns.Add(new DataGridComboBoxColumn
        {
            Header = LocExtension.Get("Emulation.HostKey"),
            ItemsSource = Enum.GetValues<GWGUI.Emulation.EmulationKey>(),
            SelectedItemBinding = new System.Windows.Data.Binding(nameof(KeyMappingItem.HostKey)) { UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged },
            Width = new DataGridLength(1, DataGridLengthUnitType.Star)
        });
    }

    private void RefreshModelSummaries()
    {
        if (_model.SelectedItem is not AmigaModel model)
        {
            _cpuSummary.Text = _ramSummary.Text = _videoSummary.Text = string.Empty;
            return;
        }
        _cpuSummary.Text = LocExtension.Get("Emulation.CpuSummary", model.Cpu);
        _ramSummary.Text = LocExtension.Get("Emulation.RamSummary", model.ChipMemoryKib,
            model.SlowMemoryKib, model.FastMemoryMib);
        _videoSummary.Text = LocExtension.Get("Emulation.VideoSummary", model.Chipset);
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
        _loadedMedia = configuration.Media;
        _disk.Text = configuration.Media?.FirstOrDefault()?.Path ?? configuration.InitialDiskPath ?? string.Empty;
        _audio.IsChecked = configuration.AudioEnabled;
        for (var port = 0; port < _controllers.Length; port++)
            _controllers[port].SelectedItem = configuration.Controllers?.ElementAtOrDefault(port) ?? AmigaControllerType.Automatic;
        _options.Clear();
        foreach (var option in configuration.Options ?? new Dictionary<string, string>())
            _options.Add(new OptionItem { Category = "Configuration", Key = option.Key, Name = option.Key, Value = option.Value });
        _floppies.Clear();
        foreach (var floppy in configuration.Floppies ?? [])
            _floppies.Add(new FloppyItem { Path = floppy.Path, Label = floppy.Label ?? string.Empty, IsReadOnly = floppy.IsReadOnly });
        _multiDrive.IsChecked = configuration.MountFloppiesInSeparateDrives;
        _keyboardMappings.Clear();
        foreach (var mapping in configuration.Input?.KeyboardMappings ?? new Dictionary<string, GWGUI.Emulation.EmulationKey>())
            _keyboardMappings.Add(new KeyMappingItem { AmigaKey = mapping.Key, HostKey = mapping.Value });
        _mouseDevice.Text = configuration.Input?.MouseDeviceId ?? string.Empty;
        _captureMouse.IsChecked = configuration.Input?.CaptureMouse ?? true;
        for (var port = 0; port < _controllerDevices.Length; port++)
            _controllerDevices[port].Text = configuration.Input?.ControllerBindings?
                .FirstOrDefault(binding => binding.Port == port)?.DeviceId ?? string.Empty;
        RefreshModelSummaries();
    }

    private async Task SaveConfigurationAsync()
    {
        if (_model.SelectedItem is not AmigaModel model) throw new InvalidOperationException(LocExtension.Get("Emulation.ModelRequired"));
        if (string.IsNullOrWhiteSpace(_kickstart.Text)) throw new InvalidOperationException(LocExtension.Get("Emulation.KickstartRequired"));
        ValidateOptionalFile(_kickstart.Text, required: true);
        ValidateOptionalFile(_extendedRom.Text);
        ValidateOptionalFile(_romKey.Text);
        ValidateOptionalMedia(_disk.Text);
        var options = _options.Where(item => !string.IsNullOrWhiteSpace(item.Key))
            .ToDictionary(item => item.Key.Trim(), item => item.Value?.Trim() ?? string.Empty, StringComparer.Ordinal);
        options["puae_model"] = model.Id;
        var floppies = _floppies.Where(item => !string.IsNullOrWhiteSpace(item.Path)).Select(item =>
        {
            ValidateOptionalFile(item.Path, required: true);
            return new AmigaFloppyConfiguration(Path.GetFullPath(item.Path), string.IsNullOrWhiteSpace(item.Label) ? null : item.Label.Trim(), item.IsReadOnly);
        }).ToArray();
        var initialPath = OptionalFullPath(_disk.Text);
        var preservedMedia = floppies.Length == 0 && _loadedMedia is { Count: > 0 }
            && string.Equals(Path.GetFullPath(_loadedMedia[0].Path), initialPath, StringComparison.OrdinalIgnoreCase)
                ? _loadedMedia : null;
        var keyboard = _keyboardMappings.Where(item => !string.IsNullOrWhiteSpace(item.AmigaKey))
            .ToDictionary(item => item.AmigaKey.Trim(), item => item.HostKey, StringComparer.OrdinalIgnoreCase);
        var controllerBindings = Enumerable.Range(0, 4).Select(port => new AmigaControllerBinding(port,
            (AmigaControllerType)(_controllers[port].SelectedItem ?? AmigaControllerType.Automatic),
            string.IsNullOrWhiteSpace(_controllerDevices[port].Text) ? null : _controllerDevices[port].Text.Trim())).ToArray();
        var input = new AmigaInputConfiguration(keyboard,
            string.IsNullOrWhiteSpace(_mouseDevice.Text) ? null : _mouseDevice.Text.Trim(),
            _captureMouse.IsChecked == true, controllerBindings);
        var configuration = new AmigaMachineConfiguration(model.Id, Path.GetFullPath(_kickstart.Text),
            initialPath, OptionalFullPath(_extendedRom.Text), OptionalFullPath(_romKey.Text),
            Options: options, Id: _currentId == Guid.Empty ? Guid.NewGuid() : _currentId,
            AudioEnabled: _audio.IsChecked == true,
            Controllers: _controllers.Select(combo => (AmigaControllerType)(combo.SelectedItem ?? AmigaControllerType.Automatic)).ToArray(),
            Input: input,
            Floppies: floppies.Length == 0 ? null : floppies,
            MountFloppiesInSeparateDrives: floppies.Length > 1 && _multiDrive.IsChecked == true,
            Media: preservedMedia);
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
                    Category = string.IsNullOrWhiteSpace(option.Category) ? "Avancé" : option.Category,
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

    private static void ValidateOptionalMedia(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        if (!File.Exists(value) && !Directory.Exists(value))
            throw new FileNotFoundException(LocExtension.Get("Emulation.FileMissing"), value);
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
        public string Category { get; set; } = string.Empty;
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

    public sealed class KeyMappingItem
    {
        public string AmigaKey { get; set; } = string.Empty;
        public GWGUI.Emulation.EmulationKey HostKey { get; set; }
    }
}
