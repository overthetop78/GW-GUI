using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using GWGUI.App.Localization;
using GWGUI.App.Services;
using GWGUI.Domain.Settings;
using GWGUI.Emulation.Amiga;
using Microsoft.Win32;

namespace GWGUI.App.Controls;

public sealed class OptionsEmulationSection : UserControl
{
    private static readonly string[] ControllerButtons =
        ["B", "Y", "Select", "Start", "Up", "Down", "Left", "Right", "A", "X", "L", "R", "L2", "R2", "L3", "R3"];
    private static readonly string[] ControllerSources = ControllerButtons
        .Concat(new[] { "Mouse:Left", "Mouse:Right", "Mouse:Middle" })
        .Concat(Enum.GetValues<GWGUI.Emulation.EmulationKey>()
            .Where(key => key != GWGUI.Emulation.EmulationKey.Unknown)
            .Select(key => $"Keyboard:{key}"))
        .ToArray();
    private readonly AmigaConfigurationStore _store = new(StoragePaths.AmigaConfigurationsDirectory, StoragePaths.DataDirectory);
    private readonly ObservableCollection<ConfigurationItem> _configurations = [];
    private readonly ObservableCollection<FirmwareItem> _firmware = [];
    private readonly ObservableCollection<OptionItem> _options = [];
    private readonly ObservableCollection<MediaItem> _media = [];
    private readonly ListBox _list = new() { MinWidth = 260 };
    private readonly ListBox _firmwareList = new() { MinWidth = 260 };
    private readonly ComboBox _model = new() { ItemsSource = AmigaModelCatalog.All, DisplayMemberPath = nameof(AmigaModel.DisplayName) };
    private readonly TextBox _kickstart = new();
    private readonly TextBox _extendedRom = new();
    private readonly TextBox _romKey = new();
    private readonly CheckBox _audio = new() { IsChecked = true };
    private readonly ComboBox _cpuModel = new();
    private readonly ComboBox _fpuModel = new();
    private readonly ComboBox _cpuCompatibility = new();
    private readonly ComboBox _cpuFrequency = new();
    private readonly TextBlock _cpuNominalFrequency = new() { VerticalAlignment = VerticalAlignment.Center };
    private readonly TextBlock _cpuModelHint = new() { VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap };
    private readonly ComboBox _chipMemory = new() { ItemsSource = new[] { "auto", "1", "2", "3", "4" } };
    private readonly ComboBox _slowMemory = new() { ItemsSource = new[] { "auto", "0", "2", "4", "6", "7" } };
    private readonly ComboBox _fastMemory = new() { ItemsSource = new[] { "auto", "0", "1", "2", "4", "8" } };
    private readonly ComboBox _z3Memory = new() { ItemsSource = new[] { "auto", "0", "1", "2", "4", "8", "16", "32", "64", "128", "256", "512" } };
    private readonly ComboBox _videoStandard = new() { ItemsSource = new[] { "PAL", "NTSC" } };
    private readonly TextBlock _chipset = new() { VerticalAlignment = VerticalAlignment.Center };
    private readonly ComboBox _videoResolution = new() { ItemsSource = new[] { "auto", "auto-lores", "auto-superhires", "lores", "hires", "superhires" } };
    private readonly ComboBox _videoAspect = new() { ItemsSource = new[] { "auto", "PAL", "NTSC", "1:1" } };
    private readonly ComboBox _cropVideo = new() { ItemsSource = new[] { "disabled", "minimum", "smaller", "small", "medium", "large", "larger", "maximum", "auto" } };
    private readonly ComboBox _videoLineMode = new();
    private readonly ComboBox _videoHzChange = new();
    private readonly ComboBox _videoFrameskip = new();
    private readonly ComboBox _videoColors = new();
    private readonly ComboBox _videoGamma = new();
    private readonly ComboBox _immediateBlits = new();
    private readonly ComboBox _collisionLevel = new();
    private readonly CheckBox _flickerFixer = new();
    private readonly ComboBox _audioOutput = new() { DisplayMemberPath = nameof(AudioOutputDevice.Name) };
    private readonly ComboBox _audioLatency = new() { ItemsSource = new[] { 20, 35, 50, 75, 100, 150, 250 } };
    private readonly ComboBox _audioInterpolation = new() { ItemsSource = new[] { "none", "anti", "sinc", "rh", "crux" } };
    private readonly ComboBox _audioFilter = new() { ItemsSource = new[] { "emulated", "off", "on" } };
    private readonly ComboBox _audioFilterType = new();
    private readonly ComboBox _floppySound = new();
    private readonly ComboBox _floppySoundType = new();
    private readonly CheckBox _muteEmptyFloppy = new();
    private readonly ComboBox _cdAudioVolume = new();
    private readonly Slider _stereoSeparation = new() { Minimum = 0, Maximum = 100, TickFrequency = 10, IsSnapToTickEnabled = true };
    private readonly ComboBox[] _controllers = Enumerable.Range(0, 4).Select(_ => new ComboBox
    {
    }).ToArray();
    private readonly StackPanel _mediaRows = new();
    private readonly ComboBox _floppyDriveCount = new() { ItemsSource = new[] { 0, 1, 2, 3, 4 }, SelectedItem = 1 };
    private readonly ComboBox _hardDriveCount = new() { ItemsSource = new[] { 0, 1 }, SelectedItem = 0 };
    private readonly CheckBox _cdDrive = new();
    private readonly CheckBox _multiDrive = new();
    private readonly ComboBox _floppySpeed = new();
    private readonly CheckBox _floppyWriteProtection = new();
    private readonly CheckBox _floppyWriteRedirect = new();
    private readonly ComboBox _cdSpeed = new();
    private readonly TextBox _mouseDevice = new();
    private readonly CheckBox _captureMouse = new() { IsChecked = true };
    private readonly ComboBox _physicalMouse = new();
    private readonly ComboBox _mouseSpeed = new();
    private readonly ComboBox _analogMouse = new();
    private readonly ComboBox _analogMouseDeadzone = new();
    private readonly ComboBox _analogMouseSpeed = new();
    private readonly ComboBox _releaseMouseKey = new() { ItemsSource = Enum.GetValues<GWGUI.Emulation.EmulationKey>().Where(key => key != GWGUI.Emulation.EmulationKey.Unknown) };
    private readonly ComboBox[] _mouseActions = Enumerable.Range(0, 3).Select(_ => new ComboBox()).ToArray();
    private readonly ComboBox[] _controllerDevices = Enumerable.Range(0, 4).Select(_ => new ComboBox { DisplayMemberPath = nameof(GameControllerDevice.Name) }).ToArray();
    private readonly ObservableCollection<ControllerMappingItem>[] _controllerMappings = Enumerable.Range(0, 4)
        .Select(_ => new ObservableCollection<ControllerMappingItem>()).ToArray();
    private readonly ObservableCollection<KeyMappingItem> _keyboardMappings = [];
    private readonly CheckBox _keyboardPassThrough = new();
    private readonly CheckBox _turboFire = new();
    private readonly ComboBox _turboButton = new();
    private readonly ComboBox _turboPulse = new();
    private readonly ComboBox _joyPortOrder = new();
    private readonly DataGrid _keyboardGrid = new() { AutoGenerateColumns = false, CanUserAddRows = true, CanUserDeleteRows = true };
    private readonly TextBox _storageBaseFolder = new();
    private readonly TextBox _captureFolder = new();
    private readonly TextBox _stateFolder = new();
    private readonly TextBlock _detectedDevices = new() { TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _detectedControllers = new() { TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center };
    private readonly TextBlock _storageTree = new() { LineHeight = 24 };
    private readonly TabControl _familyTabs = new() { Margin = new Thickness(8) };
    private AppSettings? _appSettings;
    private Func<Task>? _persistAppSettings;
    private Guid _currentId;
    private bool _loading;

    public OptionsEmulationSection()
    {
        ConfigureGrids();
        _list.ItemsSource = _configurations;
        _list.DisplayMemberPath = nameof(ConfigurationItem.DisplayName);
        _list.SelectionChanged += ConfigurationSelected;
        _firmwareList.ItemsSource = _firmware;
        _firmwareList.DisplayMemberPath = nameof(FirmwareItem.DisplayName);
        _firmwareList.SelectionChanged += FirmwareSelected;
        _model.SelectionChanged += (_, _) => ApplyModelDefaults();
        _cpuModel.SelectionChanged += (_, _) =>
        {
            ConfigureFpuChoices();
            ConfigureCpuFrequencyChoices();
            UpdateCpuModelSummary();
        };
        _cpuCompatibility.SelectionChanged += (_, _) => ConfigureCpuFrequencyChoices();
        _videoStandard.SelectionChanged += (_, _) =>
        {
            ConfigureCpuModelChoices();
            ConfigureCpuFrequencyChoices();
        };
        _floppyDriveCount.SelectionChanged += (_, _) => RefreshMediaRows();
        _hardDriveCount.SelectionChanged += (_, _) => RefreshMediaRows();
        _cdDrive.Checked += (_, _) => RefreshMediaRows();
        _cdDrive.Unchecked += (_, _) => RefreshMediaRows();
        _keyboardGrid.ItemsSource = _keyboardMappings;
        _multiDrive.Content = LocExtension.Get("Emulation.MultiDrive");
        ConfigureOptionChoices();
        var controllerChoices = new[]
        {
            new LocalizedChoice<AmigaControllerType>(AmigaControllerType.Automatic, LocExtension.Get("Visual.Automatic")),
            new(AmigaControllerType.RetroPad, "RetroPad"), new(AmigaControllerType.Cd32Pad, "CD32 Pad"),
            new(AmigaControllerType.AnalogJoystick, "Analog Joystick"), new(AmigaControllerType.Joystick, "Joystick"),
            new(AmigaControllerType.Keyboard, LocExtension.Get("Emulation.KeyboardTab")),
            new(AmigaControllerType.None, LocExtension.Get("HostTools.None"))
        };
        foreach (var controller in _controllers)
        {
            controller.ItemsSource = controllerChoices;
            controller.SelectedIndex = 0;
        }
        foreach (var action in _mouseActions) action.ItemsSource = ControllerSources;

        _familyTabs.Items.Add(new TabItem
        {
            Header = LocExtension.Get("Emulation.GeneralTab"),
            Content = BuildGeneralEmulationSettings()
        });
        _familyTabs.Items.Add(new TabItem
        {
            Header = LocExtension.Get("Emulation.Configurations"),
            Content = BuildConfigurationCatalog()
        });
        _familyTabs.Items.Add(new TabItem { Header = "Amiga", Content = BuildAmigaEditor() });
        Content = _familyTabs;
        Loaded += async (_, _) => await ReloadAsync();
    }

    public void Configure(AppSettings settings, Func<Task> persistSettings)
    {
        _appSettings = settings;
        _persistAppSettings = persistSettings;
        _storageBaseFolder.Text = settings.EmulationStorageFolder;
        _captureFolder.Text = settings.EmulationCaptureFolder;
        _stateFolder.Text = settings.EmulationStateFolder;
        StoragePaths.ConfigureEmulationStorageDirectory(settings.EmulationStorageFolder);
        EnsureStorageFolders();
    }

    private UIElement BuildGeneralEmulationSettings()
    {
        var root = new StackPanel { Margin = new Thickness(14) };
        var defaults = new StackPanel { Margin = new Thickness(8, 6, 8, 8) };
        defaults.Children.Add(BuildPathRow(LocExtension.Get("Emulation.StorageBaseFolder"), _storageBaseFolder,
            BrowseStorageBaseFolderAsync, OpenStorageBaseFolderAsync));
        defaults.Children.Add(BuildPathRow(LocExtension.Get("Emulation.CaptureFolder"), _captureFolder, BrowseCaptureFolderAsync));
        defaults.Children.Add(BuildPathRow(LocExtension.Get("Emulation.StateFolder"), _stateFolder, BrowseStateFolderAsync));
        root.Children.Add(Card(defaults, LocExtension.Get("Emulation.DefaultFolders")));
        var save = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 10, 0, 0) };
        AddButton(save, "Common.Save", SaveGeneralSettingsAsync);
        root.Children.Add(save);
        return new ScrollViewer { Content = root, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
    }

    private static Grid BuildPathRow(string label, TextBox textBox, Func<Task> browse, Func<Task>? open = null)
    {
        var row = new Grid { Margin = new Thickness(10, 6, 10, 2) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(320) });
        row.ColumnDefinitions.Add(new ColumnDefinition());
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        if (open is not null) row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        row.Children.Add(new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) });
        Grid.SetColumn(textBox, 1); row.Children.Add(textBox);
        var browseButton = new Button { Content = LocExtension.Get("Common.Browse"), MinWidth = 110, Margin = new Thickness(8, 0, 0, 0) };
        browseButton.Click += async (_, _) => await browse();
        Grid.SetColumn(browseButton, 2); row.Children.Add(browseButton);
        if (open is not null)
        {
            var openButton = new Button { Content = LocExtension.Get("Common.OpenFolder"), MinWidth = 120, Margin = new Thickness(8, 0, 0, 0) };
            openButton.Click += async (_, _) => await open();
            Grid.SetColumn(openButton, 3); row.Children.Add(openButton);
        }
        return row;
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

    private async Task BrowseStorageBaseFolderAsync()
    {
        var dialog = new OpenFolderDialog { Multiselect = false, Title = LocExtension.Get("Emulation.StorageBaseFolder") };
        if (dialog.ShowDialog(Window.GetWindow(this)) != true) return;
        _storageBaseFolder.Text = dialog.FolderName;
        await SaveGeneralSettingsAsync();
    }

    private Task BrowseCaptureFolderAsync() => BrowseGeneralFolderAsync(_captureFolder, "Emulation.CaptureFolder");
    private Task BrowseStateFolderAsync() => BrowseGeneralFolderAsync(_stateFolder, "Emulation.StateFolder");

    private async Task BrowseGeneralFolderAsync(TextBox target, string titleKey)
    {
        var dialog = new OpenFolderDialog { Multiselect = false, Title = LocExtension.Get(titleKey) };
        if (dialog.ShowDialog(Window.GetWindow(this)) != true) return;
        target.Text = dialog.FolderName;
        await SaveGeneralSettingsAsync();
    }

    private Task OpenStorageBaseFolderAsync()
    {
        var path = string.IsNullOrWhiteSpace(_storageBaseFolder.Text)
            ? StoragePaths.EmulationStorageDirectory : Path.GetFullPath(_storageBaseFolder.Text);
        Directory.CreateDirectory(path);
        Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        return Task.CompletedTask;
    }

    private async Task SaveGeneralSettingsAsync()
    {
        if (_appSettings is null) return;
        _appSettings.EmulationStorageFolder = Path.GetFullPath(_storageBaseFolder.Text.Trim());
        _appSettings.EmulationCaptureFolder = Path.GetFullPath(_captureFolder.Text.Trim());
        _appSettings.EmulationStateFolder = Path.GetFullPath(_stateFolder.Text.Trim());
        StoragePaths.ConfigureEmulationStorageDirectory(_appSettings.EmulationStorageFolder);
        EnsureStorageFolders();
        if (_persistAppSettings is not null) await _persistAppSettings();
    }

    private void EnsureStorageFolders()
    {
        if (_appSettings?.CreateEmulationFoldersAutomatically != true) return;
        foreach (var path in new[]
                 {
                     Path.Combine(_appSettings.EmulationStorageFolder, "HDD", "Amiga"),
                     Path.Combine(_appSettings.EmulationStorageFolder, "HDD", "Atari"),
                     Path.Combine(_appSettings.EmulationStorageFolder, "Saves", "Amiga"),
                     Path.Combine(_appSettings.EmulationStorageFolder, "Saves", "Atari"),
                     _appSettings.EmulationCaptureFolder, _appSettings.EmulationStateFolder
                 }) Directory.CreateDirectory(path);
    }

    private void RefreshStorageTree()
    {
        var root = string.IsNullOrWhiteSpace(_storageBaseFolder.Text)
            ? StoragePaths.EmulationStorageDirectory
            : _storageBaseFolder.Text;
        _storageTree.Text = $"📁  {root}\n    ├─ 📁  HDD\\Amiga\n    ├─ 📁  HDD\\Atari\n    ├─ 📁  Saves\\Amiga\n    └─ 📁  Saves\\Atari";
    }

    private Task DetectCommonDevicesAsync()
    {
        var devices = XInputControllerReader.GetConnectedDevices();
        _detectedDevices.Text = devices.Count == 0
            ? LocExtension.Get("Emulation.NoControllersDetected")
            : string.Join(" · ", devices.Select(device => device.Name));
        return Task.CompletedTask;
    }

    private Task TestInputsAsync()
    {
        var status = new TextBlock
        {
            Text = LocExtension.Get("Emulation.TestInputsPrompt"), TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(20), MinWidth = 420, MinHeight = 100,
            VerticalAlignment = VerticalAlignment.Center, TextAlignment = TextAlignment.Center
        };
        var window = new Window
        {
            Title = LocExtension.Get("Emulation.TestInputs"), Content = status, Owner = Window.GetWindow(this),
            SizeToContent = SizeToContent.WidthAndHeight, WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        window.PreviewKeyDown += (_, e) => status.Text = $"{LocExtension.Get("Emulation.KeyboardTab")} : {(e.Key == Key.System ? e.SystemKey : e.Key)}";
        window.PreviewMouseDown += (_, e) => status.Text = $"{LocExtension.Get("Emulation.MouseTab")} : {e.ChangedButton}";
        window.ShowDialog();
        return Task.CompletedTask;
    }

    private UIElement BuildAmigaEditor()
    {
        var root = new Grid { Margin = new Thickness(8) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition());
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var header = new Grid { Margin = new Thickness(4, 4, 4, 12) };
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(360) });
        header.ColumnDefinitions.Add(new ColumnDefinition());
        var modelLabel = new TextBlock
        {
            Text = LocExtension.Get("Emulation.Model"), FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 12, 0)
        };
        header.Children.Add(modelLabel);
        _model.Height = 44;
        _model.Margin = new Thickness(0, 4, 0, 4);
        _model.VerticalAlignment = VerticalAlignment.Center;
        Grid.SetColumn(_model, 1);
        header.Children.Add(_model);
        root.Children.Add(header);
        var tabs = new TabControl();
        AddMachineTab(tabs, "\uE713", LocExtension.Get("Emulation.GeneralTab"),
            new AmigaCoreManagementSection { Margin = new Thickness(12) });
        AddMachineTab(tabs, "\uE950", "CPU", BuildCpuTab());
        AddMachineTab(tabs, "\uE964", "RAM", BuildRamTab());
        AddMachineTab(tabs, "\uE8B7", "ROM", Wrap(BuildRomTab()));
        AddMachineTab(tabs, "\uE7F4", LocExtension.Get("Emulation.VideoTab"), BuildVideoTab());
        AddMachineTab(tabs, "\uE767", LocExtension.Get("Emulation.Audio"), BuildAudioTab());
        AddMachineTab(tabs, "\uEDA2", LocExtension.Get("Emulation.StorageTab"), Wrap(BuildStorageTab()));
        AddMachineTab(tabs, "\uE765", LocExtension.Get("Emulation.KeyboardTab"), Wrap(BuildKeyboardTab()));
        AddMachineTab(tabs, "\uE962", LocExtension.Get("Emulation.MouseTab"), BuildMouseTab());
        AddMachineTab(tabs, "\uE7FC", LocExtension.Get("Emulation.ControllersTab"), BuildControllersTab());
        Grid.SetRow(tabs, 1);
        root.Children.Add(tabs);
        var actions = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 10, 0, 0) };
        AddButton(actions, "Common.Save", SaveConfigurationAsync);
        Grid.SetRow(actions, 2);
        root.Children.Add(actions);
        return root;
    }

    private UIElement BuildCpuTab()
    {
        var processor = new StackPanel();
        processor.Children.Add(FieldGrid((LocExtension.Get("Emulation.CpuModel"), _cpuModel)));
        _cpuModelHint.Margin = new Thickness(12, 0, 12, 12);
        _cpuModelHint.SetResourceReference(ForegroundProperty, "MutedTextBrush");
        processor.Children.Add(_cpuModelHint);
        var root = TwoColumnPage(
            IconCard(processor, LocExtension.Get("Emulation.Processor"), "\uE950"),
            IconCard(FieldGrid((LocExtension.Get("Emulation.Precision"), _cpuCompatibility),
                (LocExtension.Get("Emulation.FpuModel"), _fpuModel)), LocExtension.Get("Emulation.CpuCompatibility"), "\uEA18"));
        root.Children.Add(FullWidthIconCard(FieldGrid(2,
            (LocExtension.Get("Emulation.CpuSpeedOriginal"), _cpuNominalFrequency),
            (LocExtension.Get("Emulation.CpuSpeed"), _cpuFrequency)),
            LocExtension.Get("Emulation.Acceleration"), "\uE945", 1));
        return ScrollPage(root);
    }

    private UIElement BuildRamTab()
    {
        return ScrollPage(TwoColumnPage(
            Card(FieldGrid((LocExtension.Get("Emulation.ChipMemory"), _chipMemory),
                (LocExtension.Get("Emulation.SlowMemory"), _slowMemory)),
                $"{LocExtension.Get("Emulation.ChipMemory")} / {LocExtension.Get("Emulation.SlowMemory")}"),
            Card(FieldGrid((LocExtension.Get("Emulation.FastMemory"), _fastMemory),
                (LocExtension.Get("Emulation.Z3Memory"), _z3Memory)),
                $"{LocExtension.Get("Emulation.FastMemory")} / {LocExtension.Get("Emulation.Z3Memory")}")));
    }

    private UIElement BuildVideoTab()
    {
        var root = TwoColumnPage(
            Card(FieldGrid((LocExtension.Get("Emulation.Chipset"), _chipset),
                (LocExtension.Get("Emulation.VideoStandard"), _videoStandard),
                (LocExtension.Get("Emulation.VideoResolution"), _videoResolution),
                (LocExtension.Get("Emulation.AspectRatio"), _videoAspect)), LocExtension.Get("Emulation.VideoTab")),
            Card(FieldGrid((LocExtension.Get("Emulation.VideoCrop"), _cropVideo),
                (LocExtension.Get("Emulation.FlickerFixer"), _flickerFixer),
                (LocExtension.Get("Emulation.VideoLineMode"), _videoLineMode),
                (LocExtension.Get("Emulation.VideoHzChange"), _videoHzChange)), LocExtension.Get("Emulation.VideoResolution")));
        root.Children.Add(FullWidthCard(FieldGrid(2,
            (LocExtension.Get("Emulation.VideoFrameskip"), _videoFrameskip),
            (LocExtension.Get("Emulation.VideoColors"), _videoColors),
            (LocExtension.Get("Emulation.VideoGamma"), _videoGamma),
            (LocExtension.Get("Emulation.ImmediateBlits"), _immediateBlits),
            (LocExtension.Get("Emulation.CollisionLevel"), _collisionLevel)), LocExtension.Get("Emulation.AdvancedTab"), 1));
        return ScrollPage(root);
    }

    private UIElement BuildRomTab()
    {
        var root = new Grid { Margin = new Thickness(4) };
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        var form = CreateForm(3);
        AddPathField(form, 0, "Kickstart", _kickstart, "ROM|*.rom;*.bin|All files|*.*");
        AddPathField(form, 1, LocExtension.Get("Emulation.ExtendedRom"), _extendedRom, "ROM|*.rom;*.bin|All files|*.*");
        AddPathField(form, 2, LocExtension.Get("Emulation.RomKey"), _romKey, "ROM key|*.key|All files|*.*");
        var pathsCard = Card(form, "ROM");
        pathsCard.Margin = new Thickness(0, 0, 6, 0);
        root.Children.Add(pathsCard);
        var firmware = new Grid { Margin = new Thickness(6, 0, 0, 0) };
        firmware.RowDefinitions.Add(new RowDefinition());
        firmware.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        firmware.Children.Add(_firmwareList);
        var actions = new WrapPanel { Margin = new Thickness(0, 8, 0, 0) };
        AddButton(actions, "Common.OpenFolder", OpenFirmwareFolder);
        Grid.SetRow(actions, 1);
        firmware.Children.Add(actions);
        var firmwareCard = Card(firmware, LocExtension.Get("Emulation.Firmware"));
        Grid.SetColumn(firmwareCard, 1);
        root.Children.Add(firmwareCard);
        return root;
    }

    private UIElement BuildAudioTab()
    {
        var root = ThreeColumnPage(
            Card(FieldGrid((LocExtension.Get("Emulation.AudioEnabled"), _audio),
                (LocExtension.Get("Emulation.AudioOutput"), _audioOutput),
                (LocExtension.Get("Emulation.AudioLatency"), _audioLatency),
                (LocExtension.Get("Emulation.AudioInput"), new TextBlock
        {
            Text = LocExtension.Get("Emulation.AudioInputUnavailable"),
            VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap
        })), LocExtension.Get("Emulation.AudioOutput")),
            Card(FieldGrid((LocExtension.Get("Emulation.AudioInterpolation"), _audioInterpolation),
                (LocExtension.Get("Emulation.AudioFilter"), _audioFilter),
                (LocExtension.Get("Emulation.AudioFilterType"), _audioFilterType),
                (LocExtension.Get("Emulation.StereoSeparation"), _stereoSeparation)), LocExtension.Get("Emulation.AudioFilter")),
            Card(FieldGrid(
            (LocExtension.Get("Emulation.FloppySound"), _floppySound),
            (LocExtension.Get("Emulation.FloppySoundType"), _floppySoundType),
            (LocExtension.Get("Emulation.MuteEmptyFloppy"), _muteEmptyFloppy),
            (LocExtension.Get("Emulation.CdAudioVolume"), _cdAudioVolume)), LocExtension.Get("Emulation.FloppySound")));
        return ScrollPage(root);
    }

    private UIElement BuildStorageTab()
    {
        var root = new Grid { Margin = new Thickness(12) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition());
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var storagePath = new Grid { Margin = new Thickness(10, 6, 10, 6) };
        storagePath.ColumnDefinitions.Add(new ColumnDefinition());
        storagePath.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        storagePath.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        storagePath.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        storagePath.Children.Add(new TextBlock
        {
            Text = $"{LocExtension.Get("Emulation.StorageBaseFolder")} : {StoragePaths.EmulationStorageDirectory}",
            FontWeight = FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap
        });
        var hddPath = new TextBlock { Text = StoragePaths.AmigaHardDisksDirectory, Margin = new Thickness(0, 7, 0, 0) };
        Grid.SetRow(hddPath, 1); storagePath.Children.Add(hddPath);
        var generalButton = new Button { Content = LocExtension.Get("Emulation.GeneralTab"), MinWidth = 150 };
        generalButton.Click += (_, _) => _familyTabs.SelectedIndex = 0;
        Grid.SetColumn(generalButton, 1); Grid.SetRowSpan(generalButton, 2); storagePath.Children.Add(generalButton);
        root.Children.Add(Card(storagePath));

        var hardwareGrid = new Grid { Margin = new Thickness(0, 10, 0, 10) };
        hardwareGrid.ColumnDefinitions.Add(new ColumnDefinition());
        hardwareGrid.ColumnDefinitions.Add(new ColumnDefinition());
        var floppy = FieldGrid(2,
            (LocExtension.Get("Emulation.FloppyDriveCount"), _floppyDriveCount),
            (LocExtension.Get("Emulation.FloppySpeed"), _floppySpeed),
            (LocExtension.Get("Emulation.FloppyWriteProtection"), _floppyWriteProtection),
            (LocExtension.Get("Emulation.FloppyWriteRedirect"), _floppyWriteRedirect));
        var floppyCard = Card(floppy, LocExtension.Get("Emulation.Floppies"));
        floppyCard.Margin = new Thickness(0, 0, 5, 0);
        hardwareGrid.Children.Add(floppyCard);
        var drives = new Grid { Margin = new Thickness(8) };
        drives.RowDefinitions.Add(new RowDefinition());
        drives.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _mediaRows.Margin = new Thickness(0, 0, 0, 8);
        drives.Children.Add(_mediaRows);
        var driveButtons = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(8) };
        AddButton(driveButtons, "Emulation.AddHardDisk", AddHardDiskAsync);
        AddButton(driveButtons, "Emulation.CreateHardDisk", CreateHardDiskAsync);
        Grid.SetRow(driveButtons, 1);
        drives.Children.Add(driveButtons);
        var drivesCard = Card(drives, LocExtension.Get("Emulation.HardDisks"));
        drivesCard.Margin = new Thickness(5, 0, 0, 0);
        Grid.SetColumn(drivesCard, 1);
        hardwareGrid.Children.Add(drivesCard);
        Grid.SetRow(hardwareGrid, 1);
        root.Children.Add(hardwareGrid);
        var cd = Card(FieldGrid(2,
            (LocExtension.Get("Emulation.CdDrive"), _cdDrive),
            (LocExtension.Get("Emulation.CdSpeed"), _cdSpeed)), "CD");
        Grid.SetRow(cd, 2); root.Children.Add(cd);
        return ScrollPage(root);
    }

    private UIElement BuildKeyboardTab()
    {
        _keyboardGrid.MinHeight = 280;
        var style = new Style(typeof(DataGridRow));
        style.Triggers.Add(new DataTrigger
        {
            Binding = new Binding(nameof(KeyMappingItem.HasConflict)),
            Value = true,
            Setters =
            {
                new Setter(BackgroundProperty, Brushes.MistyRose),
                new Setter(ForegroundProperty, Brushes.DarkRed)
            }
        });
        _keyboardGrid.RowStyle = style;
        _keyboardGrid.CurrentCellChanged += (_, _) => ValidateKeyboardMappings();
        var panel = new Grid { Margin = new Thickness(12) };
        panel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        panel.RowDefinitions.Add(new RowDefinition());
        var keyboardOptions = FieldGrid(2,
            (LocExtension.Get("Emulation.KeyboardPassThrough"), _keyboardPassThrough),
            (LocExtension.Get("Emulation.KeyboardPriorityHint"), new TextBlock
            {
                Text = LocExtension.Get("Emulation.KeyboardPriorityDescription"),
                TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center
            }));
        panel.Children.Add(Card(keyboardOptions, LocExtension.Get("Emulation.InputBehavior")));
        var mappings = new Grid { Margin = new Thickness(8) };
        mappings.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        mappings.RowDefinitions.Add(new RowDefinition());
        var mappingActions = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 0, 0, 8) };
        AddButton(mappingActions, "Common.Refresh", ResetKeyboardMappings);
        mappings.Children.Add(mappingActions);
        Grid.SetRow(_keyboardGrid, 1);
        _keyboardGrid.Margin = new Thickness(0);
        mappings.Children.Add(_keyboardGrid);
        var mappingCard = Card(mappings, LocExtension.Get("Emulation.InputActions"));
        mappingCard.Margin = new Thickness(0, 10, 0, 0);
        Grid.SetRow(mappingCard, 1);
        panel.Children.Add(mappingCard);
        return ScrollPage(panel);
    }

    private UIElement BuildMouseTab()
    {
        var root = TwoColumnPage(
            Card(FieldGrid((LocExtension.Get("Emulation.CaptureMouse"), _captureMouse),
                (LocExtension.Get("Emulation.ReleaseMouseKey"), _releaseMouseKey),
                (LocExtension.Get("Emulation.KeyboardPriorityHint"), new TextBlock
                {
                    Text = LocExtension.Get("Emulation.KeyboardPriorityDescription"), TextWrapping = TextWrapping.Wrap
                })), LocExtension.Get("Emulation.CaptureMouse")),
            Card(FieldGrid((LocExtension.Get("Emulation.PhysicalMouse"), _physicalMouse),
                (LocExtension.Get("Emulation.MouseSpeed"), _mouseSpeed)), LocExtension.Get("Emulation.PhysicalMouse")));
        root.Children.Add(FullWidthCard(BuildMouseMappings(), LocExtension.Get("Emulation.InputActions"), 1));
        return ScrollPage(root);
    }

    private UIElement BuildControllersTab()
    {
        var root = new Grid { Margin = new Thickness(12) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition());
        var detect = new WrapPanel { Margin = new Thickness(8) };
        AddButton(detect, "Emulation.DetectControllers", DetectControllersAsync);
        detect.Children.Add(_detectedControllers);
        root.Children.Add(Card(detect, LocExtension.Get("Emulation.SharedDevices")));

        var ports = new Grid { Margin = new Thickness(0, 10, 0, 10) };
        ports.ColumnDefinitions.Add(new ColumnDefinition());
        ports.ColumnDefinitions.Add(new ColumnDefinition());
        for (var port = 0; port < 2; port++)
        {
            var portLabel = $"{LocExtension.Get("Emulation.ControllersTab")} {port + 1}";
            var form = FieldGrid(
                (portLabel, _controllers[port]),
                (LocExtension.Get("Emulation.ControllerDevice", port + 1), _controllerDevices[port]));
            var card = Card(form, portLabel);
            card.Margin = new Thickness(port == 0 ? 0 : 5, 0, port == 0 ? 5 : 0, 0);
            Grid.SetColumn(card, port); ports.Children.Add(card);
        }
        Grid.SetRow(ports, 1); root.Children.Add(ports);

        var lower = new Grid();
        lower.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        lower.ColumnDefinitions.Add(new ColumnDefinition());
        lower.Children.Add(Card(BuildControllerMappingGrid(0), LocExtension.Get("Emulation.InputActions")));
        var behavior = FieldGrid(
            (LocExtension.Get("Emulation.AnalogMouseDeadzone"), _analogMouseDeadzone),
            (LocExtension.Get("Emulation.AnalogMouseSpeed"), _analogMouseSpeed),
            (LocExtension.Get("Emulation.TurboFire"), _turboFire),
            (LocExtension.Get("Emulation.TurboButton"), _turboButton),
            (LocExtension.Get("Emulation.TurboPulse"), _turboPulse));
        var behaviorCard = Card(behavior, LocExtension.Get("Emulation.InputBehavior"));
        behaviorCard.Margin = new Thickness(10, 0, 0, 0); Grid.SetColumn(behaviorCard, 1); lower.Children.Add(behaviorCard);
        Grid.SetRow(lower, 2); root.Children.Add(lower);
        return ScrollPage(root);
    }

    private DataGrid BuildControllerMappingGrid(int port)
    {
        var grid = new DataGrid
        {
            AutoGenerateColumns = false,
            CanUserAddRows = false,
            CanUserDeleteRows = false,
            ItemsSource = _controllerMappings[port],
            MinHeight = 190,
            Margin = new Thickness(12, 0, 12, 12)
        };
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = LocExtension.Get("Emulation.EmulatedAction"),
            Binding = new Binding(nameof(ControllerMappingItem.Action)),
            IsReadOnly = true,
            Width = new DataGridLength(1, DataGridLengthUnitType.Star)
        });
        grid.Columns.Add(new DataGridComboBoxColumn
        {
            Header = LocExtension.Get("Emulation.PhysicalControl"),
            ItemsSource = ControllerSources,
            SelectedItemBinding = new Binding(nameof(ControllerMappingItem.PhysicalButton))
            {
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            },
            Width = new DataGridLength(1, DataGridLengthUnitType.Star)
        });
        return grid;
    }

    private Task DetectControllersAsync()
    {
        var devices = XInputControllerReader.GetConnectedDevices();
        _detectedControllers.Text = devices.Count == 0
            ? LocExtension.Get("Emulation.NoControllersDetected")
            : string.Join(" · ", devices.Select(device => device.Name));
        for (var port = 0; port < _controllerDevices.Length; port++)
        {
            var selectedId = (_controllerDevices[port].SelectedItem as GameControllerDevice)?.Id
                ?? _controllerDevices[port].Tag as string;
            _controllerDevices[port].ItemsSource = devices;
            _controllerDevices[port].SelectedItem = devices.FirstOrDefault(device => device.Id == selectedId)
                ?? devices.ElementAtOrDefault(port);
            _controllerDevices[port].Tag = null;
        }
        return Task.CompletedTask;
    }

    private void ConfigureOptionChoices()
    {
        var frenchUnits = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "fr";
        var kib = frenchUnits ? "Kio" : "KiB";
        var mib = frenchUnits ? "Mio" : "MiB";
        var oneAndHalf = 1.5.ToString("0.0", System.Globalization.CultureInfo.CurrentCulture);
        var oneAndEight = 1.8.ToString("0.0", System.Globalization.CultureInfo.CurrentCulture);
        _chipMemory.ItemsSource = new[] { ("auto", LocExtension.Get("Visual.Automatic")), ("1", $"512 {kib}"), ("2", $"1 {mib}"), ("3", $"{oneAndHalf} {mib}"), ("4", $"2 {mib}") }.Select(item => new OptionChoice(item.Item1, item.Item2)).ToArray();
        _slowMemory.ItemsSource = new[] { ("auto", LocExtension.Get("Visual.Automatic")), ("0", LocExtension.Get("HostTools.None")), ("2", $"512 {kib}"), ("4", $"1 {mib}"), ("6", $"{oneAndHalf} {mib}"), ("7", $"{oneAndEight} {mib}") }.Select(item => new OptionChoice(item.Item1, item.Item2)).ToArray();
        _fastMemory.ItemsSource = MemoryChoices([0, 1, 2, 4, 8]);
        _z3Memory.ItemsSource = MemoryChoices([0, 1, 2, 4, 8, 16, 32, 64, 128, 256, 512]);
        _videoStandard.ItemsSource = new[] { new OptionChoice("PAL auto", $"PAL ({LocExtension.Get("Visual.Automatic")})"), new OptionChoice("NTSC auto", $"NTSC ({LocExtension.Get("Visual.Automatic")})"), new OptionChoice("PAL", "PAL"), new OptionChoice("NTSC", "NTSC") };
        _cpuCompatibility.ItemsSource = new[]
        {
            new OptionChoice("normal", $"{LocExtension.Get("Emulation.CompatibilityNormal")} (CPU)"),
            new OptionChoice("compatible", $"{LocExtension.Get("Emulation.CompatibilityCompatible")} (CPU)"),
            new OptionChoice("memory", $"{LocExtension.Get("Emulation.CompatibilityMemory")} (DMA / RAM)"),
            new OptionChoice("exact", $"{LocExtension.Get("Emulation.CompatibilityExact")} (CPU / DMA / RAM)")
        };
        _videoResolution.ItemsSource = Choices(("auto", "Visual.Automatic"), ("auto-lores", "Emulation.ResolutionAutoLow"), ("auto-superhires", "Emulation.ResolutionAutoSuperHigh"), ("lores", "Emulation.ResolutionLow"), ("hires", "Emulation.ResolutionHigh"), ("superhires", "Emulation.ResolutionSuperHigh"));
        _videoAspect.ItemsSource = Choices(("auto", "Visual.Automatic"), ("PAL", "PAL"), ("NTSC", "NTSC"), ("1:1", "1:1"));
        _cropVideo.ItemsSource = Choices(("disabled", "Emulation.Disabled"), ("minimum", "Emulation.Minimum"), ("smaller", "Emulation.VerySmall"), ("small", "Emulation.Small"), ("medium", "Emulation.Medium"), ("large", "Emulation.Large"), ("larger", "Emulation.VeryLarge"), ("maximum", "Emulation.Maximum"), ("auto", "Visual.Automatic"));
        _videoLineMode.ItemsSource = Choices(("auto", "Visual.Automatic"), ("single", "Emulation.LineModeSingle"), ("double", "Emulation.LineModeDouble"));
        _videoHzChange.ItemsSource = Choices(("disabled", "Emulation.Disabled"), ("enabled", "Emulation.Enabled"), ("locked", "Emulation.Locked"));
        _videoFrameskip.ItemsSource = Choices(("disabled", "Emulation.Disabled"), ("1", "1"), ("2", "2"));
        _videoColors.ItemsSource = new[] { new OptionChoice("16bit", "16 bits"), new OptionChoice("24bit", "24 bits") };
        _videoGamma.ItemsSource = Enumerable.Range(-5, 11).Select(value => new OptionChoice((value * 100).ToString(), value.ToString())).ToArray();
        _immediateBlits.ItemsSource = Choices(("false", "Emulation.Disabled"), ("immediate", "Emulation.Immediate"), ("waiting", "Emulation.Waiting"));
        _collisionLevel.ItemsSource = Choices(("none", "HostTools.None"), ("sprites", "Emulation.CollisionSprites"), ("playfields", "Emulation.CollisionPlayfields"), ("full", "Emulation.CollisionFull"));
        _audioInterpolation.ItemsSource = Choices(("none", "HostTools.None"), ("anti", "Emulation.InterpolationAnti"), ("sinc", "Sinc"), ("rh", "RH"), ("crux", "Crux"));
        _audioFilter.ItemsSource = Choices(("emulated", "Emulation.FilterEmulated"), ("off", "Emulation.Disabled"), ("on", "Emulation.Enabled"));
        _audioFilterType.ItemsSource = Choices(("auto", "Visual.Automatic"), ("standard", "Emulation.Standard"), ("enhanced", "Emulation.Enhanced"));
        _floppySound.ItemsSource = Enumerable.Range(0, 21).Select(index => 100 - index * 5).Select(value => new OptionChoice(value.ToString(), $"{value} %")).ToArray();
        _floppySoundType.ItemsSource = new[] { new OptionChoice("internal", LocExtension.Get("Emulation.Internal")), new OptionChoice("A500", "A500"), new OptionChoice("LOUD", LocExtension.Get("Emulation.Loud")) };
        _cdAudioVolume.ItemsSource = Enumerable.Range(0, 21).Select(index => index * 5).Select(value => new OptionChoice($"{value}%", $"{value} %")).ToArray();
        _floppySpeed.ItemsSource = new[] { 100, 200, 400, 800, 0 }.Select(value => new OptionChoice(value.ToString(), value == 0 ? LocExtension.Get("Emulation.Maximum") : $"{value} %")).ToArray();
        _cdSpeed.ItemsSource = new[] { new OptionChoice("100", "1×"), new OptionChoice("0", LocExtension.Get("Emulation.Maximum")) };
        _physicalMouse.ItemsSource = Choices(("disabled", "Emulation.Disabled"), ("enabled", "Emulation.Enabled"), ("double", "Emulation.PhysicalMouseDouble"));
        _mouseSpeed.ItemsSource = Enumerable.Range(1, 100).Select(value => value * 10).Select(value => new OptionChoice(value.ToString(), $"{value} %")).ToArray();
        _analogMouse.ItemsSource = Choices(("disabled", "Emulation.Disabled"), ("left", "Emulation.LeftStick"), ("right", "Emulation.RightStick"), ("both", "Emulation.BothSticks"));
        _analogMouseDeadzone.ItemsSource = Enumerable.Range(0, 11).Select(value => value * 5).Select(value => new OptionChoice(value.ToString(), $"{value} %")).ToArray();
        _analogMouseSpeed.ItemsSource = Enumerable.Range(1, 30).Select(value => value / 10d).Select(value => new OptionChoice(value.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture), $"{value:0.0}×")).ToArray();
        _turboButton.ItemsSource = new[] { "B", "A", "Y", "X", "L", "R", "L2", "R2" };
        _turboPulse.ItemsSource = new[] { "2", "4", "6", "8", "10", "12" };
        _joyPortOrder.ItemsSource = new[] { "1234", "2143", "3412", "4321" };
    }

    private static OptionChoice[] Choices(params (string Value, string TextOrKey)[] choices) => choices
        .Select(choice => new OptionChoice(choice.Value, choice.TextOrKey.Contains('.') ? LocExtension.Get(choice.TextOrKey) : choice.TextOrKey))
        .ToArray();

    private static OptionChoice[] MemoryChoices(IEnumerable<int> values)
    {
        var unit = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "fr" ? "Mio" : "MiB";
        return [new OptionChoice("auto", LocExtension.Get("Visual.Automatic")), .. values.Select(value => new OptionChoice(value.ToString(), value == 0 ? LocExtension.Get("HostTools.None") : $"{value} {unit}"))];
    }

    private Task AddMediaAsync()
    {
        var dialog = new OpenFileDialog
        {
            Multiselect = true,
            Filter = LocExtension.Get("Emulation.AmigaMediaFilter")
        };
        if (dialog.ShowDialog() != true) return Task.CompletedTask;
        foreach (var path in dialog.FileNames)
            _media.Add(new MediaItem
            {
                Path = path,
                Kind = InferMediaKind(path),
                Label = Path.GetFileNameWithoutExtension(path)
            });
        RefreshMediaRows();
        return Task.CompletedTask;
    }

    private Task AddHardDiskAsync()
    {
        if (_model.SelectedItem is not AmigaModel { MaximumHardDrives: > 0 })
            throw new InvalidOperationException(LocExtension.Get("Emulation.HardDiskNotSupported"));
        Directory.CreateDirectory(StoragePaths.AmigaHardDisksDirectory);
        var dialog = new OpenFileDialog
        {
            Multiselect = true,
            InitialDirectory = StoragePaths.AmigaHardDisksDirectory,
            Filter = LocExtension.Get("Emulation.HardDiskFilter")
        };
        if (dialog.ShowDialog() != true) return Task.CompletedTask;
        foreach (var path in dialog.FileNames.Where(path => _media.All(item =>
                     !string.Equals(item.Path, path, StringComparison.OrdinalIgnoreCase))))
            _media.Add(new MediaItem
            {
                Path = path, Kind = AmigaMediaKind.HardDrive,
                Label = Path.GetFileNameWithoutExtension(path)
            });
        _hardDriveCount.SelectedItem = Math.Min(_media.Count(item => item.Kind == AmigaMediaKind.HardDrive),
            ((AmigaModel)_model.SelectedItem).MaximumHardDrives);
        RefreshMediaRows();
        return Task.CompletedTask;
    }

    private Task CreateHardDiskAsync()
    {
        if (_model.SelectedItem is not AmigaModel { MaximumHardDrives: > 0 } model)
            throw new InvalidOperationException(LocExtension.Get("Emulation.HardDiskNotSupported"));
        var dialog = new SaveFileDialog
        {
            Filter = LocExtension.Get("Emulation.HardDiskFilter"),
            DefaultExt = ".hdf",
            AddExtension = true,
            InitialDirectory = Directory.CreateDirectory(StoragePaths.AmigaHardDisksDirectory).FullName
        };
        if (dialog.ShowDialog() != true) return Task.CompletedTask;
        var size = AskHardDiskSize();
        if (size is null) return Task.CompletedTask;
        using (var stream = new FileStream(dialog.FileName, FileMode.Create, FileAccess.Write, FileShare.None))
            stream.SetLength(size.Value * 1024L * 1024L);
        _media.Add(new MediaItem
        {
            Path = dialog.FileName,
            Kind = AmigaMediaKind.HardDrive,
            Label = Path.GetFileNameWithoutExtension(dialog.FileName)
        });
        _hardDriveCount.SelectedItem = Math.Min(_media.Count(item => item.Kind == AmigaMediaKind.HardDrive), model.MaximumHardDrives);
        RefreshMediaRows();
        return Task.CompletedTask;
    }

    private int? AskHardDiskSize()
    {
        var sizes = new[] { 20, 40, 80, 120, 250, 500, 1024, 2048, 4096 };
        var choice = new ComboBox { ItemsSource = sizes, SelectedItem = 500, MinWidth = 180 };
        var ok = new Button { Content = LocExtension.Get("Emulation.CreateHardDisk"), IsDefault = true, MinWidth = 100 };
        var cancel = new Button { Content = LocExtension.Get("Common.Cancel"), IsCancel = true, MinWidth = 100 };
        var buttons = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Right };
        buttons.Children.Add(ok); buttons.Children.Add(cancel);
        var panel = new StackPanel { Margin = new Thickness(16) };
        panel.Children.Add(new TextBlock { Text = LocExtension.Get("Emulation.HardDiskSize"), Margin = new Thickness(0, 0, 0, 8) });
        panel.Children.Add(choice); panel.Children.Add(buttons);
        var window = new Window
        {
            Title = LocExtension.Get("Emulation.CreateHardDisk"), Content = panel,
            Owner = Window.GetWindow(this), SizeToContent = SizeToContent.WidthAndHeight,
            WindowStartupLocation = WindowStartupLocation.CenterOwner, ResizeMode = ResizeMode.NoResize
        };
        ok.Click += (_, _) => window.DialogResult = true;
        return window.ShowDialog() == true ? (int?)choice.SelectedItem : null;
    }

    private void RefreshMediaRows()
    {
        _mediaRows.Children.Clear();
        var hardDisks = _media.Where(item => item.Kind == AmigaMediaKind.HardDrive).ToArray();
        if (hardDisks.Length == 0)
        {
            _mediaRows.Children.Add(new TextBlock
            {
                Text = LocExtension.Get("Emulation.NoHardDisks"),
                Margin = new Thickness(8),
                TextWrapping = TextWrapping.Wrap
            });
            return;
        }
        foreach (var item in hardDisks)
        {
            var row = new Grid { Margin = new Thickness(0, 0, 0, 8) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            row.ColumnDefinitions.Add(new ColumnDefinition());
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var label = new TextBox { Text = item.Label, Margin = new Thickness(4) };
            label.TextChanged += (_, _) => item.Label = label.Text;
            row.Children.Add(label);
            var path = new TextBox { Text = item.Path, Margin = new Thickness(4), IsReadOnly = true };
            path.TextChanged += (_, _) => item.Path = path.Text;
            Grid.SetColumn(path, 1); row.Children.Add(path);
            var remove = new Button { Content = LocExtension.Get("Common.Delete"), MinWidth = 90, Margin = new Thickness(4) };
            remove.Click += (_, _) => { _media.Remove(item); RefreshMediaRows(); };
            Grid.SetColumn(remove, 2); row.Children.Add(remove);
            var card = new Border { Child = row };
            card.SetResourceReference(StyleProperty, "Card");
            _mediaRows.Children.Add(card);
        }
    }

    private static AmigaMediaKind InferMediaKind(string path) => Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".hdf" or ".hdz" => AmigaMediaKind.HardDrive,
        ".cue" or ".ccd" or ".chd" or ".nrg" or ".mds" or ".iso" => AmigaMediaKind.CompactDisc,
        ".lha" or ".slave" or ".info" => AmigaMediaKind.WhdLoad,
        ".uae" => AmigaMediaKind.Configuration,
        _ => AmigaMediaKind.Floppy
    };

    private static int SelectedCount(ComboBox comboBox) => comboBox.SelectedItem is int value ? value : 0;

    private void ValidateKeyboardMappings()
    {
        var duplicates = _keyboardMappings.Where(item => !string.IsNullOrWhiteSpace(item.HostBinding))
            .GroupBy(item => item.HostBinding.Trim(), StringComparer.OrdinalIgnoreCase).Where(group => group.Count() > 1)
            .Select(group => group.Key).ToHashSet();
        foreach (var item in _keyboardMappings) item.HasConflict = duplicates.Contains(item.HostBinding.Trim());
        _keyboardGrid.Items.Refresh();
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

    private static Grid TwoColumnPage(Border left, Border right)
    {
        var grid = new Grid { Margin = new Thickness(12) };
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        left.Margin = new Thickness(0, 0, 5, 0);
        right.Margin = new Thickness(5, 0, 0, 0);
        grid.Children.Add(left);
        Grid.SetColumn(right, 1);
        grid.Children.Add(right);
        return grid;
    }

    private static void AddMachineTab(TabControl tabs, string icon, string title, UIElement content)
    {
        var tab = new TabItem
        {
            Header = new MainTabHeader { Icon = icon, Text = title },
            Content = content,
            Padding = new Thickness(14, 9, 14, 9)
        };
        tab.SetResourceReference(StyleProperty, "MainTabItemStyle");
        tabs.Items.Add(tab);
    }

    private static Grid ThreeColumnPage(Border left, Border center, Border right)
    {
        var grid = new Grid { Margin = new Thickness(12) };
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        left.Margin = new Thickness(0, 0, 5, 0);
        center.Margin = new Thickness(5, 0, 5, 0);
        right.Margin = new Thickness(5, 0, 0, 0);
        grid.Children.Add(left);
        Grid.SetColumn(center, 1); grid.Children.Add(center);
        Grid.SetColumn(right, 2); grid.Children.Add(right);
        return grid;
    }

    private static Border FullWidthCard(UIElement content, string title, int row)
    {
        var card = Card(content, title);
        card.Margin = new Thickness(0, 10, 0, 0);
        Grid.SetRow(card, row);
        Grid.SetColumnSpan(card, 2);
        return card;
    }

    private static Border FullWidthIconCard(UIElement content, string title, string icon, int row)
    {
        var card = IconCard(content, title, icon);
        card.Margin = new Thickness(0, 10, 0, 0);
        Grid.SetRow(card, row);
        Grid.SetColumnSpan(card, 2);
        return card;
    }

    private static Grid FieldGrid(params (string Label, FrameworkElement Control)[] fields) => FieldGrid(1, fields);

    private static Grid FieldGrid(int columns, params (string Label, FrameworkElement Control)[] fields)
    {
        var grid = new Grid { Margin = new Thickness(12, 6, 12, 10) };
        for (var column = 0; column < columns; column++)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(155) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
        }
        var rows = (int)Math.Ceiling(fields.Length / (double)columns);
        for (var row = 0; row < rows; row++) grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        for (var index = 0; index < fields.Length; index++)
        {
            var row = index / columns;
            var column = (index % columns) * 2;
            var label = new TextBlock
            {
                Text = fields[index].Label, VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(column == 0 ? 0 : 18, 8, 10, 8), TextWrapping = TextWrapping.Wrap
            };
            Grid.SetRow(label, row); Grid.SetColumn(label, column); grid.Children.Add(label);
            var control = fields[index].Control;
            control.MinWidth = control is CheckBox ? 0 : 145;
            control.Margin = new Thickness(0, 4, 0, 4);
            control.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetRow(control, row); Grid.SetColumn(control, column + 1); grid.Children.Add(control);
        }
        return grid;
    }

    private static ScrollViewer ScrollPage(UIElement child) => new()
    {
        Content = child,
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
    };

    private UIElement BuildMouseMappings()
    {
        var grid = new Grid { Margin = new Thickness(12, 6, 12, 12) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(210) });
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var labels = new[]
        {
            LocExtension.Get("Emulation.MouseLeftButton"),
            LocExtension.Get("Emulation.MouseRightButton"),
            LocExtension.Get("Emulation.MouseMiddleButton")
        };
        for (var index = 0; index < labels.Length; index++)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var label = new TextBlock { Text = labels[index], Margin = new Thickness(0, 8, 10, 8), VerticalAlignment = VerticalAlignment.Center };
            Grid.SetRow(label, index); grid.Children.Add(label);
            var source = _mouseActions[index];
            source.IsEditable = true;
            Grid.SetRow(source, index); Grid.SetColumn(source, 1); grid.Children.Add(source);
            var assign = new Button
            {
                Content = LocExtension.Get("Emulation.AssignInput"), MinWidth = 110,
                Margin = new Thickness(8, 4, 0, 4), Tag = source
            };
            assign.Click += BeginSourceAssignment;
            assign.PreviewKeyDown += CaptureSourceKey;
            assign.PreviewMouseDown += CaptureSourceMouse;
            Grid.SetRow(assign, index); Grid.SetColumn(assign, 2); grid.Children.Add(assign);
        }
        return grid;
    }

    private void BeginSourceAssignment(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        button.Content = LocExtension.Get("Emulation.PressInput");
        button.Focus();
    }

    private void CaptureSourceKey(object sender, KeyEventArgs e)
    {
        if (sender is not Button { Tag: ComboBox target } button) return;
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (key is Key.LeftCtrl or Key.RightCtrl or Key.LeftShift or Key.RightShift or Key.LeftAlt or Key.RightAlt) return;
        var parts = new List<string>();
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
        parts.Add(key.ToString());
        target.Text = $"Keyboard:{string.Join("+", parts)}";
        button.Content = LocExtension.Get("Emulation.AssignInput");
        e.Handled = true;
    }

    private void CaptureSourceMouse(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Button { Tag: ComboBox target } button || e.ChangedButton == MouseButton.Left) return;
        target.Text = $"Mouse:{e.ChangedButton}";
        button.Content = LocExtension.Get("Emulation.AssignInput");
        e.Handled = true;
    }

    private Task ResetKeyboardMappings()
    {
        foreach (var mapping in _keyboardMappings) mapping.HostBinding = mapping.AmigaKey;
        ValidateKeyboardMappings();
        return Task.CompletedTask;
    }

    private static Grid CreateCompactForm(int fieldColumns, params (string Label, FrameworkElement Control)[] fields)
    {
        var form = new Grid { Margin = new Thickness(10) };
        for (var column = 0; column < fieldColumns; column++)
        {
            form.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto, MinWidth = 125 });
            form.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 150 });
        }
        var rows = (int)Math.Ceiling(fields.Length / (double)fieldColumns);
        for (var row = 0; row < rows; row++) form.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        for (var index = 0; index < fields.Length; index++)
        {
            var row = index / fieldColumns;
            var column = (index % fieldColumns) * 2;
            var label = new TextBlock
            {
                Text = fields[index].Label, VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(column == 0 ? 0 : 18, 7, 10, 7), TextWrapping = TextWrapping.Wrap
            };
            Grid.SetRow(label, row); Grid.SetColumn(label, column);
            form.Children.Add(label);
            var control = fields[index].Control;
            control.Margin = new Thickness(0, 4, 0, 4);
            control.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetRow(control, row); Grid.SetColumn(control, column + 1);
            form.Children.Add(control);
        }
        return form;
    }

    private static Border Card(UIElement child, string? title = null)
    {
        UIElement content = child;
        if (!string.IsNullOrWhiteSpace(title))
        {
            var panel = new StackPanel();
            panel.Children.Add(new TextBlock
            {
                Text = title, FontWeight = FontWeights.SemiBold, FontSize = 16,
                Margin = new Thickness(10, 8, 10, 2)
            });
            panel.Children.Add(child);
            content = panel;
        }
        var card = new Border { Child = content, Padding = new Thickness(2) };
        card.SetResourceReference(StyleProperty, "Card");
        return card;
    }

    private static Border IconCard(UIElement child, string title, string icon)
    {
        var header = new Border
        {
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(16, 12, 16, 12)
        };
        header.SetResourceReference(BorderBrushProperty, "BorderBrush");
        var heading = new StackPanel { Orientation = Orientation.Horizontal };
        var glyph = new TextBlock
        {
            Text = icon,
            FontFamily = new FontFamily("Segoe MDL2 Assets"),
            FontSize = 19,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0)
        };
        glyph.SetResourceReference(ForegroundProperty, "AccentBrush");
        heading.Children.Add(glyph);
        heading.Children.Add(new TextBlock
        {
            Text = title,
            FontWeight = FontWeights.SemiBold,
            FontSize = 17,
            VerticalAlignment = VerticalAlignment.Center
        });
        header.Child = heading;

        var body = new Border { Child = child, Padding = new Thickness(6, 8, 6, 8) };
        var layout = new Grid();
        layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        layout.RowDefinitions.Add(new RowDefinition());
        layout.Children.Add(header);
        Grid.SetRow(body, 1);
        layout.Children.Add(body);

        var card = new Border
        {
            Child = layout,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(9),
            ClipToBounds = true
        };
        card.SetResourceReference(BackgroundProperty, "CardBrush");
        card.SetResourceReference(BorderBrushProperty, "BorderBrush");
        return card;
    }

    private static UIElement Wrap(UIElement child)
    {
        var card = new Border
        {
            Child = child,
            Margin = new Thickness(12),
            Padding = new Thickness(8),
            VerticalAlignment = VerticalAlignment.Top
        };
        card.SetResourceReference(StyleProperty, "Card");
        return new ScrollViewer { Content = card, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
    }

    private void ConfigureGrids()
    {
        _keyboardGrid.CanUserAddRows = false;
        _keyboardGrid.CanUserDeleteRows = false;
        _keyboardGrid.Columns.Add(new DataGridTextColumn
        {
            Header = LocExtension.Get("Emulation.SystemKey", "Amiga"),
            Binding = new Binding(nameof(KeyMappingItem.AmigaKey)),
            IsReadOnly = true,
            Width = new DataGridLength(1, DataGridLengthUnitType.Star)
        });
        _keyboardGrid.Columns.Add(new DataGridComboBoxColumn
        {
            Header = LocExtension.Get("Emulation.HostKey"),
            ItemsSource = Enum.GetValues<GWGUI.Emulation.EmulationKey>().Select(key => key.ToString()),
            SelectedItemBinding = new Binding(nameof(KeyMappingItem.HostBinding)) { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
            Width = new DataGridLength(1, DataGridLengthUnitType.Star)
        });
        var assignFactory = new FrameworkElementFactory(typeof(Button));
        assignFactory.SetValue(ContentControl.ContentProperty, LocExtension.Get("Emulation.AssignInput"));
        assignFactory.SetValue(FrameworkElement.MarginProperty, new Thickness(4));
        assignFactory.AddHandler(Button.ClickEvent, new RoutedEventHandler(BeginKeyboardAssignment));
        assignFactory.AddHandler(UIElement.PreviewKeyDownEvent, new KeyEventHandler(CaptureKeyboardAssignment));
        _keyboardGrid.Columns.Add(new DataGridTemplateColumn
        {
            Header = LocExtension.Get("Emulation.InputActions"),
            CellTemplate = new DataTemplate { VisualTree = assignFactory }, Width = DataGridLength.Auto
        });
    }

    private void BeginKeyboardAssignment(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            button.Content = LocExtension.Get("Emulation.PressInput");
            button.Focus();
        }
    }

    private void CaptureKeyboardAssignment(object sender, KeyEventArgs e)
    {
        if (sender is not Button { DataContext: KeyMappingItem mapping } button) return;
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (key is Key.LeftCtrl or Key.RightCtrl or Key.LeftShift or Key.RightShift or Key.LeftAlt or Key.RightAlt or Key.LWin or Key.RWin)
            return;
        var parts = new List<string>();
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Windows)) parts.Add("Win");
        parts.Add(key.ToString());
        mapping.HostBinding = string.Join("+", parts);
        button.Content = LocExtension.Get("Emulation.AssignInput");
        ValidateKeyboardMappings();
        e.Handled = true;
    }

    private void ApplyModelDefaults()
    {
        if (_model.SelectedItem is not AmigaModel model) return;
        ConfigureCpuModelChoices();
        SelectValue(_cpuModel, model.DefaultCpu);
        ConfigureFpuChoices();
        _chipset.Text = model.Chipset;
        SelectValue(_chipMemory, Math.Clamp(model.ChipMemoryKib / 512, 1, 4).ToString());
        SelectValue(_slowMemory, model.SlowMemoryKib == 0 ? "0" : Math.Clamp(model.SlowMemoryKib / 256, 2, 7).ToString());
        SelectValue(_fastMemory, model.FastMemoryMib.ToString());
        SelectValue(_z3Memory, "0");
        SelectValue(_videoStandard, "PAL auto");
        SelectValue(_cpuCompatibility, "exact");
        ConfigureCpuFrequencyChoices();
        SelectCpuFrequency("0.0", "0");
        SelectValue(_videoResolution, "auto");
        SelectValue(_videoAspect, "auto");
        SelectValue(_cropVideo, "disabled");
        _floppyDriveCount.ItemsSource = Enumerable.Range(0, model.MaximumFloppyDrives + 1).ToArray();
        _floppyDriveCount.SelectedItem = Math.Min(1, model.MaximumFloppyDrives);
        _hardDriveCount.ItemsSource = Enumerable.Range(0, model.MaximumHardDrives + 1).ToArray();
        _hardDriveCount.SelectedItem = 0;
        _cdDrive.IsChecked = model.HasCdDrive;
        RefreshMediaRows();
    }

    private void ConfigureFpuChoices()
    {
        var cpu = SelectedText(_cpuModel);
        if (string.IsNullOrEmpty(cpu)) cpu = "68000";
        var values = cpu switch
        {
            "68000" or "68010" => new[] { "0" },
            "68020" or "68030" => new[] { "0", "68881", "68882" },
            _ => new[] { "cpu", "0", "68881", "68882" }
        };
        var previous = SelectedText(_fpuModel);
        _fpuModel.ItemsSource = values.Select(value => new OptionChoice(value, value switch
        {
            "0" when values.Length == 1 => $"{LocExtension.Get("HostTools.None")} — {CpuDisplayName(cpu)}",
            "0" => LocExtension.Get("HostTools.None"),
            "cpu" => $"{LocExtension.Get("Emulation.IntegratedFpu")} — {CpuDisplayName(cpu)}",
            _ => $"Motorola {value}"
        })).ToArray();
        SelectValue(_fpuModel, values.Contains(previous) ? previous : DefaultFpu(cpu));
        _fpuModel.IsEnabled = values.Length > 1;
    }

    private void ConfigureCpuModelChoices()
    {
        if (_model.SelectedItem is not AmigaModel model) return;
        var previous = SelectedText(_cpuModel);
        var nominalFrequency = NominalCpuFrequencyMhz(model);
        _cpuModel.ItemsSource = model.CpuModels
            .Select(cpu => new OptionChoice(cpu, $"{CpuDisplayName(cpu)} — {FormatMhz(nominalFrequency)}"))
            .ToArray();
        SelectValue(_cpuModel, model.CpuModels.Contains(previous) ? previous : model.DefaultCpu);
        _cpuModel.IsEnabled = model.CpuModels.Count > 1;
        UpdateCpuModelSummary();
    }

    private void UpdateCpuModelSummary()
    {
        if (_model.SelectedItem is not AmigaModel model) return;
        var cpu = SelectedText(_cpuModel);
        if (string.IsNullOrWhiteSpace(cpu)) cpu = model.DefaultCpu;
        var frequency = FormatMhz(NominalCpuFrequencyMhz(model));
        _cpuNominalFrequency.Text = frequency;
        _cpuModelHint.Text = $"{model.DisplayName} · {CpuDisplayName(cpu)} · {model.Chipset} · {frequency}";
    }

    private void ConfigureCpuFrequencyChoices()
    {
        if (_model.SelectedItem is not AmigaModel model) return;
        var previousRatio = (_cpuFrequency.SelectedItem as CpuFrequencyChoice)?.Ratio ?? 1d;
        var nominalFrequency = NominalCpuFrequencyMhz(model);
        var choices = new List<CpuFrequencyChoice>();

        if (IsCycleExactCpu())
        {
            var halfA500Clock = IsNtsc() ? 3.579545d : 3.546895d;
            foreach (var multiplier in new[] { 1, 2, 4, 8, 16 })
            {
                var frequency = halfA500Clock * multiplier;
                var ratio = frequency / nominalFrequency;
                if (Approximately(ratio, 1d)) continue;
                choices.Add(new CpuFrequencyChoice(ratio, "0.0", multiplier.ToString(),
                    FrequencyChoiceText(ratio, frequency)));
            }
            choices.Add(new CpuFrequencyChoice(1d, "0.0", "0",
                FrequencyChoiceText(1d, nominalFrequency)));
            choices.Sort((left, right) => left.Ratio.CompareTo(right.Ratio));
        }
        else
        {
            var ratios = new[]
            {
                (Ratio: 0.5d, Throttle: "-500.0"),
                (Ratio: 1d, Throttle: "0.0"),
                (Ratio: 2d, Throttle: "1000.0"),
                (Ratio: 4d, Throttle: "3000.0"),
                (Ratio: 8d, Throttle: "7000.0")
            };
            choices.AddRange(ratios.Select(item => new CpuFrequencyChoice(item.Ratio, item.Throttle, "0",
                FrequencyChoiceText(item.Ratio, nominalFrequency * item.Ratio))));
        }

        _cpuFrequency.ItemsSource = choices;
        _cpuFrequency.SelectedItem = choices.OrderBy(choice => Math.Abs(choice.Ratio - previousRatio)).FirstOrDefault();
    }

    private void SelectCpuFrequency(string throttle, string multiplier)
    {
        var choices = _cpuFrequency.Items.OfType<CpuFrequencyChoice>().ToArray();
        var selected = IsCycleExactCpu()
            ? choices.FirstOrDefault(choice => choice.MultiplierValue == multiplier)
            : choices.FirstOrDefault(choice => choice.ThrottleValue == throttle);
        _cpuFrequency.SelectedItem = selected ?? choices.FirstOrDefault(choice => Approximately(choice.Ratio, 1d))
            ?? choices.FirstOrDefault();
    }

    private bool IsCycleExactCpu() => SelectedText(_cpuCompatibility) is "memory" or "exact";
    private bool IsNtsc() => SelectedText(_videoStandard).StartsWith("NTSC", StringComparison.OrdinalIgnoreCase);

    private double NominalCpuFrequencyMhz(AmigaModel model) => model.Id switch
    {
        "A1200" or "CD32" => IsNtsc() ? 14.31818d : 14.18758d,
        "A3000" or "A4000" => 25d,
        _ => IsNtsc() ? 7.15909d : 7.09379d
    };

    private static string FrequencyChoiceText(double ratio, double frequency)
    {
        var percentage = Math.Round(ratio * 100d);
        var prefix = Approximately(ratio, 1d)
            ? $"{LocExtension.Get("Emulation.CpuSpeedOriginal")} (100 %)"
            : $"{percentage:0} %";
        return $"{prefix} — {FormatMhz(frequency)}";
    }

    private static string FormatMhz(double frequency) =>
        $"{frequency.ToString("0.00", System.Globalization.CultureInfo.CurrentCulture)} MHz";

    private static string CpuDisplayName(string cpu) => cpu switch
    {
        "68020" => "Motorola 68EC020",
        _ => $"Motorola {cpu}"
    };

    private static bool Approximately(double left, double right) => Math.Abs(left - right) < 0.01d;

    private static string DefaultFpu(string cpu) => cpu is "68040" or "68060" ? "cpu" : "0";

    private static void SelectValue(ComboBox comboBox, string value)
    {
        comboBox.SelectedItem = comboBox.Items.Cast<object>().FirstOrDefault(item =>
            string.Equals(item is OptionChoice choice ? choice.Value : item.ToString(), value, StringComparison.OrdinalIgnoreCase));
        if (comboBox.SelectedItem is null && comboBox.Items.Count > 0) comboBox.SelectedIndex = 0;
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

    private static void AddField(Grid grid, int row, string label, FrameworkElement control)
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
        var selectedModel = AmigaModelCatalog.Get(configuration.Model);
        _model.SelectedItem = selectedModel;
        _kickstart.Text = configuration.KickstartPath;
        _extendedRom.Text = configuration.ExtendedRomPath ?? string.Empty;
        _romKey.Text = configuration.RomKeyPath ?? string.Empty;
        _audio.IsChecked = configuration.AudioEnabled;
        for (var port = 0; port < _controllers.Length; port++)
            SelectChoice(_controllers[port], configuration.Controllers?.ElementAtOrDefault(port) ?? AmigaControllerType.Automatic);
        _options.Clear();
        foreach (var option in configuration.Options ?? new Dictionary<string, string>())
            _options.Add(new OptionItem { Category = "Configuration", Key = option.Key, Name = option.Key, Value = option.Value });
        SetOption(_cpuModel, configuration, "puae_cpu_model", selectedModel.DefaultCpu);
        SetOption(_fpuModel, configuration, "puae_fpu_model", DefaultFpu(selectedModel.DefaultCpu));
        SetOption(_cpuCompatibility, configuration, "puae_cpu_compatibility", "exact");
        ConfigureCpuFrequencyChoices();
        SelectCpuFrequency(GetOption(configuration, "puae_cpu_throttle", "0.0"),
            GetOption(configuration, "puae_cpu_multiplier", "0"));
        SetOption(_chipMemory, configuration, "puae_chipmem_size", Math.Clamp(((_model.SelectedItem as AmigaModel)?.ChipMemoryKib ?? 512) / 512, 1, 4).ToString());
        SetOption(_slowMemory, configuration, "puae_bogomem_size", ((_model.SelectedItem as AmigaModel)?.SlowMemoryKib ?? 0) == 0 ? "0" : "2");
        SetOption(_fastMemory, configuration, "puae_fastmem_size", ((_model.SelectedItem as AmigaModel)?.FastMemoryMib ?? 0).ToString());
        SetOption(_z3Memory, configuration, "puae_z3mem_size", "0");
        SetOption(_videoStandard, configuration, "puae_video_standard", "PAL");
        SetOption(_videoResolution, configuration, "puae_video_resolution", "auto");
        SetOption(_videoAspect, configuration, "puae_video_aspect", "auto");
        SetOption(_cropVideo, configuration, "puae_crop", "disabled");
        SetOption(_videoLineMode, configuration, "puae_video_vresolution", "auto");
        SetOption(_videoHzChange, configuration, "puae_video_allow_hz_change", "locked");
        SetOption(_videoFrameskip, configuration, "puae_gfx_framerate", "disabled");
        SetOption(_videoColors, configuration, "puae_gfx_colors", "24bit");
        SetOption(_videoGamma, configuration, "puae_gfx_gamma", "0");
        SetOption(_immediateBlits, configuration, "puae_immediate_blits", "waiting");
        SetOption(_collisionLevel, configuration, "puae_collision_level", "playfields");
        _flickerFixer.IsChecked = GetOption(configuration, "puae_gfx_flickerfixer", "disabled") == "enabled";
        var audio = configuration.Audio ?? new AmigaAudioConfiguration();
        var outputs = WasapiAudioOutput.GetOutputDevices();
        _audioOutput.ItemsSource = new[] { new AudioOutputDevice(string.Empty, LocExtension.Get("Emulation.DefaultAudioOutput")) }.Concat(outputs).ToArray();
        _audioOutput.SelectedItem = _audioOutput.Items.OfType<AudioOutputDevice>().FirstOrDefault(device => device.Id == audio.OutputDeviceId)
            ?? _audioOutput.Items.OfType<AudioOutputDevice>().FirstOrDefault();
        _audioLatency.SelectedItem = audio.LatencyMilliseconds;
        SetOption(_audioInterpolation, configuration, "puae_sound_interpol", audio.Interpolation);
        SetOption(_audioFilter, configuration, "puae_sound_filter", audio.Filter);
        SetOption(_audioFilterType, configuration, "puae_sound_filter_type", "auto");
        SetOption(_floppySound, configuration, "puae_floppy_sound", "80");
        SetOption(_floppySoundType, configuration, "puae_floppy_sound_type", "internal");
        _muteEmptyFloppy.IsChecked = GetOption(configuration, "puae_floppy_sound_empty_mute", "enabled") == "enabled";
        SetOption(_cdAudioVolume, configuration, "puae_sound_volume_cd", "100%");
        _stereoSeparation.Value = int.TryParse(GetOption(configuration, "puae_sound_stereo_separation", $"{audio.StereoSeparation}%").TrimEnd('%'), out var separation) ? separation : 100;
        _media.Clear();
        var media = configuration.Media ?? configuration.Floppies?.Select(floppy => new AmigaMediaConfiguration(
            floppy.Path, AmigaMediaKind.Floppy, floppy.Label, floppy.IsReadOnly)).ToArray()
            ?? (configuration.InitialDiskPath is null ? [] : [new AmigaMediaConfiguration(configuration.InitialDiskPath, InferMediaKind(configuration.InitialDiskPath))]);
        foreach (var item in media.Where(item => item.Kind == AmigaMediaKind.HardDrive))
            _media.Add(new MediaItem { Path = item.Path, Kind = item.Kind, Label = item.Label ?? string.Empty });
        _floppyDriveCount.SelectedItem = Math.Clamp(
            int.TryParse(GetOption(configuration, "gwgui_floppy_drive_count", "1"), out var floppyCount) ? floppyCount : 1,
            0, selectedModel.MaximumFloppyDrives);
        _hardDriveCount.SelectedItem = Math.Clamp(
            int.TryParse(GetOption(configuration, "gwgui_hard_drive_count", media.Count(item => item.Kind == AmigaMediaKind.HardDrive).ToString()), out var hardCount) ? hardCount : 0,
            0, selectedModel.MaximumHardDrives);
        _cdDrive.IsChecked = GetOption(configuration, "gwgui_cd_drive_enabled", selectedModel.HasCdDrive ? "enabled" : "disabled") == "enabled";
        _multiDrive.IsChecked = configuration.MountFloppiesInSeparateDrives;
        SetOption(_floppySpeed, configuration, "puae_floppy_speed", "100");
        _floppyWriteProtection.IsChecked = GetOption(configuration, "puae_floppy_write_protection", "disabled") == "enabled";
        _floppyWriteRedirect.IsChecked = GetOption(configuration, "puae_floppy_write_redirect", "disabled") == "enabled";
        SetOption(_cdSpeed, configuration, "puae_cd_speed", "100");
        _keyboardMappings.Clear();
        foreach (var key in Enum.GetValues<GWGUI.Emulation.EmulationKey>().Where(key => key != GWGUI.Emulation.EmulationKey.Unknown))
            _keyboardMappings.Add(new KeyMappingItem
            {
                AmigaKey = key.ToString(),
                HostBinding = configuration.Input?.KeyboardBindings?.GetValueOrDefault(key.ToString())
                    ?? (configuration.Input?.KeyboardMappings?.GetValueOrDefault(key.ToString()) ?? key).ToString()
            });
        _mouseDevice.Text = configuration.Input?.MouseDeviceId ?? string.Empty;
        _captureMouse.IsChecked = configuration.Input?.CaptureMouse ?? true;
        _releaseMouseKey.SelectedItem = configuration.Input?.ReleaseMouseKey ?? GWGUI.Emulation.EmulationKey.Escape;
        SetOption(_physicalMouse, configuration, "puae_physicalmouse", "enabled");
        SetOption(_mouseSpeed, configuration, "puae_mouse_speed", "100");
        SetOption(_analogMouse, configuration, "puae_analogmouse", "both");
        SetOption(_analogMouseDeadzone, configuration, "puae_analogmouse_deadzone", "20");
        SetOption(_analogMouseSpeed, configuration, "puae_analogmouse_speed", "1.0");
        _keyboardPassThrough.IsChecked = GetOption(configuration, "puae_physical_keyboard_pass_through", "disabled") == "enabled";
        _turboFire.IsChecked = GetOption(configuration, "puae_turbo_fire", "disabled") == "enabled";
        SetOption(_turboButton, configuration, "puae_turbo_fire_button", "B");
        SetOption(_turboPulse, configuration, "puae_turbo_pulse", "6");
        SetOption(_joyPortOrder, configuration, "puae_joyport_order", "1234");
        var mouseMappings = configuration.Input?.MouseButtonMappings;
        SelectValue(_mouseActions[0], mouseMappings?.FirstOrDefault(item => item.Value == AmigaMouseAction.LeftButton).Key ?? "Mouse:Left");
        SelectValue(_mouseActions[1], mouseMappings?.FirstOrDefault(item => item.Value == AmigaMouseAction.RightButton).Key ?? "Mouse:Right");
        SelectValue(_mouseActions[2], mouseMappings?.FirstOrDefault(item => item.Value == AmigaMouseAction.MiddleButton).Key ?? "Mouse:Middle");
        for (var port = 0; port < _controllerDevices.Length; port++)
        {
            var binding = configuration.Input?.ControllerBindings?.FirstOrDefault(item => item.Port == port);
            _controllerMappings[port].Clear();
            foreach (var action in ControllerButtons)
                _controllerMappings[port].Add(new ControllerMappingItem
                {
                    Action = action,
                    PhysicalButton = binding?.ButtonMappings?.FirstOrDefault(item => item.Value == action).Key ?? action
                });
            _controllerDevices[port].Tag = binding?.DeviceId;
        }
        _ = DetectControllersAsync();
        RefreshMediaRows();
        ValidateKeyboardMappings();
    }

    private static string GetOption(AmigaMachineConfiguration configuration, string key, string fallback) =>
        configuration.Options?.GetValueOrDefault(key) ?? fallback;

    private static void SetOption(ComboBox comboBox, AmigaMachineConfiguration configuration, string key, string? fallback)
    {
        var value = GetOption(configuration, key, fallback ?? string.Empty);
        comboBox.SelectedItem = comboBox.Items.Cast<object>().FirstOrDefault(item =>
            string.Equals(item is OptionChoice choice ? choice.Value : item.ToString(), value, StringComparison.OrdinalIgnoreCase));
        if (comboBox.SelectedItem is null && comboBox.Items.Count > 0) comboBox.SelectedIndex = 0;
    }

    private async Task SaveConfigurationAsync()
    {
        if (_model.SelectedItem is not AmigaModel model) throw new InvalidOperationException(LocExtension.Get("Emulation.ModelRequired"));
        if (string.IsNullOrWhiteSpace(_kickstart.Text)) throw new InvalidOperationException(LocExtension.Get("Emulation.KickstartRequired"));
        ValidateOptionalFile(_kickstart.Text, required: true);
        ValidateOptionalFile(_extendedRom.Text);
        ValidateOptionalFile(_romKey.Text);
        ValidateKeyboardMappings();
        if (_keyboardMappings.Any(item => item.HasConflict))
            throw new InvalidOperationException(LocExtension.Get("Emulation.DuplicateKeyboardMapping"));
        var options = _options.Where(item => !string.IsNullOrWhiteSpace(item.Key))
            .ToDictionary(item => item.Key.Trim(), item => item.Value?.Trim() ?? string.Empty, StringComparer.Ordinal);
        options["puae_model"] = model.BackendModel;
        options["puae_cpu_model"] = SelectedText(_cpuModel);
        options["puae_fpu_model"] = SelectedText(_fpuModel);
        var cpuFrequency = _cpuFrequency.SelectedItem as CpuFrequencyChoice;
        options["puae_cpu_throttle"] = cpuFrequency?.ThrottleValue ?? "0.0";
        options["puae_cpu_multiplier"] = cpuFrequency?.MultiplierValue ?? "0";
        options["puae_cpu_compatibility"] = SelectedText(_cpuCompatibility);
        options["puae_chipmem_size"] = SelectedText(_chipMemory);
        options["puae_bogomem_size"] = SelectedText(_slowMemory);
        options["puae_fastmem_size"] = SelectedText(_fastMemory);
        options["puae_z3mem_size"] = SelectedText(_z3Memory);
        options["puae_video_standard"] = SelectedText(_videoStandard);
        options["puae_video_resolution"] = SelectedText(_videoResolution);
        options["puae_video_aspect"] = SelectedText(_videoAspect);
        options["puae_crop"] = SelectedText(_cropVideo);
        options["puae_video_vresolution"] = SelectedText(_videoLineMode);
        options["puae_video_allow_hz_change"] = SelectedText(_videoHzChange);
        options["puae_gfx_framerate"] = SelectedText(_videoFrameskip);
        options["puae_gfx_colors"] = SelectedText(_videoColors);
        options["puae_gfx_gamma"] = SelectedText(_videoGamma);
        options["puae_immediate_blits"] = SelectedText(_immediateBlits);
        options["puae_collision_level"] = SelectedText(_collisionLevel);
        options["puae_gfx_flickerfixer"] = _flickerFixer.IsChecked == true ? "enabled" : "disabled";
        options["puae_sound_interpol"] = SelectedText(_audioInterpolation);
        options["puae_sound_filter"] = SelectedText(_audioFilter);
        options["puae_sound_filter_type"] = SelectedText(_audioFilterType);
        options["puae_floppy_sound"] = SelectedText(_floppySound);
        options["puae_floppy_sound_type"] = SelectedText(_floppySoundType);
        options["puae_floppy_sound_empty_mute"] = _muteEmptyFloppy.IsChecked == true ? "enabled" : "disabled";
        options["puae_sound_volume_cd"] = SelectedText(_cdAudioVolume);
        options["puae_sound_stereo_separation"] = $"{(int)_stereoSeparation.Value}%";
        options["puae_floppy_speed"] = SelectedText(_floppySpeed);
        options["puae_floppy_write_protection"] = _floppyWriteProtection.IsChecked == true ? "enabled" : "disabled";
        options["puae_floppy_write_redirect"] = _floppyWriteRedirect.IsChecked == true ? "enabled" : "disabled";
        options["puae_cd_speed"] = SelectedText(_cdSpeed);
        options["gwgui_floppy_drive_count"] = SelectedCount(_floppyDriveCount).ToString();
        options["gwgui_hard_drive_count"] = SelectedCount(_hardDriveCount).ToString();
        options["gwgui_cd_drive_enabled"] = _cdDrive.IsChecked == true ? "enabled" : "disabled";
        options["puae_physical_keyboard_pass_through"] = _keyboardPassThrough.IsChecked == true ? "enabled" : "disabled";
        options["puae_physicalmouse"] = SelectedText(_physicalMouse);
        options["puae_mouse_speed"] = SelectedText(_mouseSpeed);
        options["puae_analogmouse"] = SelectedText(_analogMouse);
        options["puae_analogmouse_deadzone"] = SelectedText(_analogMouseDeadzone);
        options["puae_analogmouse_speed"] = SelectedText(_analogMouseSpeed);
        options["puae_turbo_fire"] = _turboFire.IsChecked == true ? "enabled" : "disabled";
        options["puae_turbo_fire_button"] = SelectedText(_turboButton);
        options["puae_turbo_pulse"] = SelectedText(_turboPulse);
        options["puae_joyport_order"] = SelectedText(_joyPortOrder);
        var media = _media.Where(item => !string.IsNullOrWhiteSpace(item.Path)).Select(item =>
        {
            ValidateOptionalFile(item.Path, required: true);
            return new AmigaMediaConfiguration(Path.GetFullPath(item.Path), item.Kind,
                string.IsNullOrWhiteSpace(item.Label) ? null : item.Label.Trim(), item.IsReadOnly);
        }).ToArray();
        var floppyDriveCount = SelectedCount(_floppyDriveCount);
        var hardDriveCount = SelectedCount(_hardDriveCount);
        if (media.Count(item => item.Kind == AmigaMediaKind.Floppy) > floppyDriveCount)
            throw new InvalidOperationException(LocExtension.Get("Emulation.TooManyFloppyDrives", floppyDriveCount));
        if (media.Count(item => item.Kind == AmigaMediaKind.HardDrive) > hardDriveCount)
            throw new InvalidOperationException(LocExtension.Get("Emulation.TooManyHardDrives", hardDriveCount));
        if (_cdDrive.IsChecked != true && media.Any(item => item.Kind == AmigaMediaKind.CompactDisc))
            throw new InvalidOperationException(LocExtension.Get("Emulation.CdNotSupported"));
        var initialPath = media.FirstOrDefault()?.Path;
        var floppies = media.Where(item => item.Kind == AmigaMediaKind.Floppy)
            .Select(item => new AmigaFloppyConfiguration(item.Path, item.Label, item.IsReadOnly)).ToArray();
        var keyboardBindings = _keyboardMappings.Where(item => !string.IsNullOrWhiteSpace(item.AmigaKey))
            .ToDictionary(item => item.AmigaKey.Trim(), item => item.HostBinding.Trim(), StringComparer.OrdinalIgnoreCase);
        var keyboard = keyboardBindings
            .Where(item => Enum.TryParse<GWGUI.Emulation.EmulationKey>(item.Value, true, out _))
            .ToDictionary(item => item.Key,
                item => Enum.Parse<GWGUI.Emulation.EmulationKey>(item.Value, true), StringComparer.OrdinalIgnoreCase);
        var controllerBindings = Enumerable.Range(0, 4).Select(port =>
        {
            var mappings = _controllerMappings[port].Where(item => !string.IsNullOrWhiteSpace(item.PhysicalButton))
                .ToDictionary(item => item.PhysicalButton, item => item.Action, StringComparer.OrdinalIgnoreCase);
            return new AmigaControllerBinding(port,
                SelectedChoice(_controllers[port], AmigaControllerType.Automatic),
                (_controllerDevices[port].SelectedItem as GameControllerDevice)?.Id,
                mappings);
        }).ToArray();
        var mouseMappings = new Dictionary<string, AmigaMouseAction>(StringComparer.OrdinalIgnoreCase)
        {
            [SelectedText(_mouseActions[0])] = AmigaMouseAction.LeftButton,
            [SelectedText(_mouseActions[1])] = AmigaMouseAction.RightButton,
            [SelectedText(_mouseActions[2])] = AmigaMouseAction.MiddleButton
        };
        var input = new AmigaInputConfiguration(keyboard,
            string.IsNullOrWhiteSpace(_mouseDevice.Text) ? null : _mouseDevice.Text.Trim(),
            _captureMouse.IsChecked == true, controllerBindings, mouseMappings,
            (GWGUI.Emulation.EmulationKey)(_releaseMouseKey.SelectedItem ?? GWGUI.Emulation.EmulationKey.Escape),
            keyboardBindings);
        var selectedOutput = _audioOutput.SelectedItem as AudioOutputDevice;
        var audio = new AmigaAudioConfiguration(string.IsNullOrWhiteSpace(selectedOutput?.Id) ? null : selectedOutput.Id,
            (int)(_audioLatency.SelectedItem ?? 50), SelectedText(_audioInterpolation), SelectedText(_audioFilter),
            (int)_stereoSeparation.Value);
        var configuration = new AmigaMachineConfiguration(model.Id, Path.GetFullPath(_kickstart.Text),
            initialPath, OptionalFullPath(_extendedRom.Text), OptionalFullPath(_romKey.Text),
            Options: options, Id: _currentId == Guid.Empty ? Guid.NewGuid() : _currentId,
            AudioEnabled: _audio.IsChecked == true,
            Controllers: _controllers.Select(combo => SelectedChoice(combo, AmigaControllerType.Automatic)).ToArray(),
            Input: input,
            Floppies: floppies.Length == 0 ? null : floppies,
            MountFloppiesInSeparateDrives: floppies.Length > 1 && _multiDrive.IsChecked == true,
            Media: media.Length == 0 ? null : media,
            Audio: audio);
        await _store.SaveAsync(configuration);
        _currentId = configuration.Id;
        await ReloadAsync();
    }

    private static string SelectedText(ComboBox comboBox) => comboBox.SelectedItem is OptionChoice choice
        ? choice.Value
        : comboBox.SelectedItem?.ToString() ?? string.Empty;

    private static T SelectedChoice<T>(ComboBox comboBox, T fallback) where T : struct, Enum =>
        comboBox.SelectedItem is LocalizedChoice<T> choice ? choice.Value : fallback;

    private static void SelectChoice<T>(ComboBox comboBox, T value) where T : struct, Enum =>
        comboBox.SelectedItem = comboBox.Items.OfType<LocalizedChoice<T>>().FirstOrDefault(choice =>
            EqualityComparer<T>.Default.Equals(choice.Value, value));

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

    public sealed class MediaItem
    {
        public string Path { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public bool IsReadOnly { get; set; }
        public AmigaMediaKind Kind { get; set; }
    }

    public sealed class KeyMappingItem
    {
        public string AmigaKey { get; set; } = string.Empty;
        public string HostBinding { get; set; } = string.Empty;
        public bool HasConflict { get; set; }
    }

    public sealed class ControllerMappingItem
    {
        public string Action { get; set; } = string.Empty;
        public string PhysicalButton { get; set; } = string.Empty;
    }

    private sealed record LocalizedChoice<T>(T Value, string Text) where T : struct, Enum
    {
        public override string ToString() => Text;
    }

    private sealed record OptionChoice(string Value, string Text)
    {
        public override string ToString() => Text;
    }

    private sealed record CpuFrequencyChoice(double Ratio, string ThrottleValue, string MultiplierValue, string Text)
    {
        public override string ToString() => Text;
    }
}
