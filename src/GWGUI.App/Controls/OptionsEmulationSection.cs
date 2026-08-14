using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using GWGUI.App.Localization;
using GWGUI.App.Services;
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
    private readonly ComboBox _cpuMultiplier = new();
    private readonly ComboBox _cpuThrottle = new();
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
        _cpuModel.SelectionChanged += (_, _) => ConfigureFpuChoices();
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
        var mouseChoices = new[]
        {
            new LocalizedChoice<AmigaMouseAction>(AmigaMouseAction.None, LocExtension.Get("HostTools.None")),
            new(AmigaMouseAction.LeftButton, LocExtension.Get("Emulation.MouseLeftButton")),
            new(AmigaMouseAction.RightButton, LocExtension.Get("Emulation.MouseRightButton")),
            new(AmigaMouseAction.MiddleButton, LocExtension.Get("Emulation.MouseMiddleButton"))
        };
        foreach (var action in _mouseActions) action.ItemsSource = mouseChoices;

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
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition());
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var header = new Grid { Margin = new Thickness(4, 4, 4, 10) };
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
        header.ColumnDefinitions.Add(new ColumnDefinition());
        header.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        header.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        AddField(header, 0, LocExtension.Get("Emulation.Model"), _model);
        var core = new AmigaCoreManagementSection { Margin = new Thickness(0, 8, 0, 0) };
        Grid.SetRow(core, 1);
        Grid.SetColumnSpan(core, 2);
        header.Children.Add(core);
        root.Children.Add(header);
        var tabs = new TabControl();
        tabs.Items.Add(new TabItem { Header = "CPU", Content = BuildCpuTab() });
        tabs.Items.Add(new TabItem { Header = "RAM", Content = BuildRamTab() });
        tabs.Items.Add(new TabItem { Header = "ROM", Content = Wrap(BuildRomTab()) });
        tabs.Items.Add(new TabItem { Header = LocExtension.Get("Emulation.VideoTab"), Content = BuildVideoTab() });
        tabs.Items.Add(new TabItem { Header = LocExtension.Get("Emulation.Audio"), Content = BuildAudioTab() });
        tabs.Items.Add(new TabItem { Header = LocExtension.Get("Emulation.StorageTab"), Content = Wrap(BuildStorageTab()) });
        tabs.Items.Add(new TabItem { Header = LocExtension.Get("Emulation.KeyboardTab"), Content = Wrap(BuildKeyboardTab()) });
        tabs.Items.Add(new TabItem { Header = LocExtension.Get("Emulation.MouseTab"), Content = BuildMouseTab() });
        tabs.Items.Add(new TabItem { Header = LocExtension.Get("Emulation.ControllersTab"), Content = BuildControllersTab() });
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
        var form = CreateForm(5);
        AddField(form, 0, LocExtension.Get("Emulation.CpuModel"), _cpuModel);
        AddField(form, 1, LocExtension.Get("Emulation.FpuModel"), _fpuModel);
        AddField(form, 2, LocExtension.Get("Emulation.CpuSpeed"), _cpuThrottle);
        AddField(form, 3, LocExtension.Get("Emulation.CpuMultiplier"), _cpuMultiplier);
        AddField(form, 4, LocExtension.Get("Emulation.CpuCompatibility"), _cpuCompatibility);
        return Wrap(form);
    }

    private UIElement BuildRamTab()
    {
        var form = CreateForm(4);
        AddField(form, 0, LocExtension.Get("Emulation.ChipMemory"), _chipMemory);
        AddField(form, 1, LocExtension.Get("Emulation.SlowMemory"), _slowMemory);
        AddField(form, 2, LocExtension.Get("Emulation.FastMemory"), _fastMemory);
        AddField(form, 3, LocExtension.Get("Emulation.Z3Memory"), _z3Memory);
        return Wrap(form);
    }

    private UIElement BuildVideoTab()
    {
        var form = CreateForm(13);
        AddField(form, 0, LocExtension.Get("Emulation.Chipset"), _chipset);
        AddField(form, 1, LocExtension.Get("Emulation.VideoStandard"), _videoStandard);
        AddField(form, 2, LocExtension.Get("Emulation.VideoResolution"), _videoResolution);
        AddField(form, 3, LocExtension.Get("Emulation.AspectRatio"), _videoAspect);
        AddField(form, 4, LocExtension.Get("Emulation.VideoCrop"), _cropVideo);
        AddField(form, 5, LocExtension.Get("Emulation.FlickerFixer"), _flickerFixer);
        AddField(form, 6, LocExtension.Get("Emulation.VideoLineMode"), _videoLineMode);
        AddField(form, 7, LocExtension.Get("Emulation.VideoHzChange"), _videoHzChange);
        AddField(form, 8, LocExtension.Get("Emulation.VideoFrameskip"), _videoFrameskip);
        AddField(form, 9, LocExtension.Get("Emulation.VideoColors"), _videoColors);
        AddField(form, 10, LocExtension.Get("Emulation.VideoGamma"), _videoGamma);
        AddField(form, 11, LocExtension.Get("Emulation.ImmediateBlits"), _immediateBlits);
        AddField(form, 12, LocExtension.Get("Emulation.CollisionLevel"), _collisionLevel);
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
        var form = CreateForm(12);
        AddField(form, 0, LocExtension.Get("Emulation.AudioEnabled"), _audio);
        AddField(form, 1, LocExtension.Get("Emulation.AudioOutput"), _audioOutput);
        AddField(form, 2, LocExtension.Get("Emulation.AudioLatency"), _audioLatency);
        AddField(form, 3, LocExtension.Get("Emulation.AudioInterpolation"), _audioInterpolation);
        AddField(form, 4, LocExtension.Get("Emulation.AudioFilter"), _audioFilter);
        AddField(form, 5, LocExtension.Get("Emulation.StereoSeparation"), _stereoSeparation);
        AddField(form, 6, LocExtension.Get("Emulation.AudioInput"), new TextBlock
        {
            Text = LocExtension.Get("Emulation.AudioInputUnavailable"),
            VerticalAlignment = VerticalAlignment.Center
        });
        AddField(form, 7, LocExtension.Get("Emulation.AudioFilterType"), _audioFilterType);
        AddField(form, 8, LocExtension.Get("Emulation.FloppySound"), _floppySound);
        AddField(form, 9, LocExtension.Get("Emulation.FloppySoundType"), _floppySoundType);
        AddField(form, 10, LocExtension.Get("Emulation.MuteEmptyFloppy"), _muteEmptyFloppy);
        AddField(form, 11, LocExtension.Get("Emulation.CdAudioVolume"), _cdAudioVolume);
        return Wrap(form);
    }

    private UIElement BuildStorageTab()
    {
        var root = new Grid { Margin = new Thickness(12) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition());
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var hardware = CreateForm(7);
        AddField(hardware, 0, LocExtension.Get("Emulation.FloppyDriveCount"), _floppyDriveCount);
        AddField(hardware, 1, LocExtension.Get("Emulation.HardDriveCount"), _hardDriveCount);
        AddField(hardware, 2, LocExtension.Get("Emulation.CdDrive"), _cdDrive);
        AddField(hardware, 3, LocExtension.Get("Emulation.FloppySpeed"), _floppySpeed);
        AddField(hardware, 4, LocExtension.Get("Emulation.FloppyWriteProtection"), _floppyWriteProtection);
        AddField(hardware, 5, LocExtension.Get("Emulation.FloppyWriteRedirect"), _floppyWriteRedirect);
        AddField(hardware, 6, LocExtension.Get("Emulation.CdSpeed"), _cdSpeed);
        root.Children.Add(hardware);
        var media = new ScrollViewer
        {
            Content = _mediaRows,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };
        Grid.SetRow(media, 1);
        root.Children.Add(media);
        var panel = new WrapPanel { Margin = new Thickness(0, 10, 0, 0) };
        AddButton(panel, "Emulation.AddMedia", AddMediaAsync);
        AddButton(panel, "Emulation.CreateHardDisk", CreateHardDiskAsync);
        panel.Children.Add(_multiDrive);
        Grid.SetRow(panel, 2);
        root.Children.Add(panel);
        return root;
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
        var keyboardOptions = CreateForm(1);
        AddField(keyboardOptions, 0, LocExtension.Get("Emulation.KeyboardPassThrough"), _keyboardPassThrough);
        panel.Children.Add(keyboardOptions);
        Grid.SetRow(_keyboardGrid, 1);
        panel.Children.Add(_keyboardGrid);
        return panel;
    }

    private UIElement BuildMouseTab()
    {
        var form = CreateForm(11);
        AddField(form, 0, LocExtension.Get("Emulation.DeviceId"), _mouseDevice);
        AddField(form, 1, LocExtension.Get("Emulation.CaptureMouse"), _captureMouse);
        AddField(form, 2, LocExtension.Get("Emulation.ReleaseMouseKey"), _releaseMouseKey);
        AddField(form, 3, LocExtension.Get("Emulation.MouseLeftButton"), _mouseActions[0]);
        AddField(form, 4, LocExtension.Get("Emulation.MouseRightButton"), _mouseActions[1]);
        AddField(form, 5, LocExtension.Get("Emulation.MouseMiddleButton"), _mouseActions[2]);
        AddField(form, 6, LocExtension.Get("Emulation.PhysicalMouse"), _physicalMouse);
        AddField(form, 7, LocExtension.Get("Emulation.MouseSpeed"), _mouseSpeed);
        AddField(form, 8, LocExtension.Get("Emulation.AnalogMouse"), _analogMouse);
        AddField(form, 9, LocExtension.Get("Emulation.AnalogMouseDeadzone"), _analogMouseDeadzone);
        AddField(form, 10, LocExtension.Get("Emulation.AnalogMouseSpeed"), _analogMouseSpeed);
        return Wrap(form);
    }

    private UIElement BuildControllersTab()
    {
        var root = new StackPanel { Margin = new Thickness(12) };
        var detect = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
        AddButton(detect, "Emulation.DetectControllers", DetectControllersAsync);
        root.Children.Add(detect);
        var behavior = CreateForm(4);
        AddField(behavior, 0, LocExtension.Get("Emulation.TurboFire"), _turboFire);
        AddField(behavior, 1, LocExtension.Get("Emulation.TurboButton"), _turboButton);
        AddField(behavior, 2, LocExtension.Get("Emulation.TurboPulse"), _turboPulse);
        AddField(behavior, 3, LocExtension.Get("Emulation.JoyPortOrder"), _joyPortOrder);
        root.Children.Add(behavior);
        for (var port = 0; port < 4; port++)
        {
            var form = CreateForm(2);
            AddField(form, 0, LocExtension.Get("Emulation.Controller", port + 1), _controllers[port]);
            AddField(form, 1, LocExtension.Get("Emulation.ControllerDevice", port + 1), _controllerDevices[port]);
            var content = new StackPanel();
            content.Children.Add(form);
            content.Children.Add(BuildControllerMappingGrid(port));
            root.Children.Add(new Expander
            {
                Header = LocExtension.Get("Emulation.Controller", port + 1),
                IsExpanded = port == 0,
                Content = content
            });
        }
        return Wrap(root);
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
        _chipMemory.ItemsSource = new[] { ("auto", LocExtension.Get("Visual.Automatic")), ("1", "512 KiB"), ("2", "1 MiB"), ("3", "1.5 MiB"), ("4", "2 MiB") }.Select(item => new OptionChoice(item.Item1, item.Item2)).ToArray();
        _slowMemory.ItemsSource = new[] { ("auto", LocExtension.Get("Visual.Automatic")), ("0", LocExtension.Get("HostTools.None")), ("2", "512 KiB"), ("4", "1 MiB"), ("6", "1.5 MiB"), ("7", "1.8 MiB") }.Select(item => new OptionChoice(item.Item1, item.Item2)).ToArray();
        _fastMemory.ItemsSource = MemoryChoices([0, 1, 2, 4, 8]);
        _z3Memory.ItemsSource = MemoryChoices([0, 1, 2, 4, 8, 16, 32, 64, 128, 256, 512]);
        _videoStandard.ItemsSource = new[] { new OptionChoice("PAL auto", $"PAL ({LocExtension.Get("Visual.Automatic")})"), new OptionChoice("NTSC auto", $"NTSC ({LocExtension.Get("Visual.Automatic")})"), new OptionChoice("PAL", "PAL"), new OptionChoice("NTSC", "NTSC") };
        _cpuMultiplier.ItemsSource = Choices(("0", "Emulation.CpuSpeedOriginal"), ("1", "1×"), ("2", "2×"), ("4", "4×"), ("8", "8×"), ("10", "10×"), ("12", "12×"), ("16", "16×"));
        _cpuThrottle.ItemsSource = new[] { "-900.0", "-500.0", "-200.0", "-100.0", "0.0", "1000.0", "2000.0", "4000.0", "8000.0", "10000.0" }
            .Select(value => new OptionChoice(value, value == "0.0" ? LocExtension.Get("Emulation.CpuSpeedOriginal") : $"{value.TrimEnd('0').TrimEnd('.')} %")).ToArray();
        _cpuCompatibility.ItemsSource = Choices(("normal", "Emulation.CompatibilityNormal"), ("compatible", "Emulation.CompatibilityCompatible"), ("memory", "Emulation.CompatibilityMemory"), ("exact", "Emulation.CompatibilityExact"));
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

    private static OptionChoice[] MemoryChoices(IEnumerable<int> values) =>
        [new OptionChoice("auto", LocExtension.Get("Visual.Automatic")), .. values.Select(value => new OptionChoice(value.ToString(), value == 0 ? LocExtension.Get("HostTools.None") : $"{value} MiB"))];

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

    private Task CreateHardDiskAsync()
    {
        if (SelectedCount(_hardDriveCount) == 0)
            throw new InvalidOperationException(LocExtension.Get("Emulation.HardDiskNotSupported"));
        var dialog = new SaveFileDialog
        {
            Filter = LocExtension.Get("Emulation.HardDiskFilter"),
            DefaultExt = ".hdf",
            AddExtension = true
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
        if (_media.Count == 0)
        {
            _mediaRows.Children.Add(new TextBlock
            {
                Text = LocExtension.Get("Emulation.NoMedia"),
                Margin = new Thickness(8),
                TextWrapping = TextWrapping.Wrap
            });
            return;
        }
        var allowedKinds = Enum.GetValues<AmigaMediaKind>().Where(kind => kind switch
        {
            AmigaMediaKind.Floppy => SelectedCount(_floppyDriveCount) > 0,
            AmigaMediaKind.HardDrive => SelectedCount(_hardDriveCount) > 0,
            AmigaMediaKind.CompactDisc => _cdDrive.IsChecked == true,
            AmigaMediaKind.WhdLoad or AmigaMediaKind.Configuration => true,
            _ => false
        }).ToArray();
        foreach (var item in _media.ToArray())
        {
            var row = new Grid { Margin = new Thickness(0, 0, 0, 8) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            row.ColumnDefinitions.Add(new ColumnDefinition());
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var kind = new ComboBox { ItemsSource = allowedKinds, SelectedItem = item.Kind, Margin = new Thickness(4) };
            kind.SelectionChanged += (_, _) => item.Kind = (AmigaMediaKind)(kind.SelectedItem ?? item.Kind);
            row.Children.Add(kind);
            var path = new TextBox { Text = item.Path, Margin = new Thickness(4) };
            path.TextChanged += (_, _) => item.Path = path.Text;
            Grid.SetColumn(path, 1); row.Children.Add(path);
            var label = new TextBox { Text = item.Label, Margin = new Thickness(4) };
            label.TextChanged += (_, _) => item.Label = label.Text;
            Grid.SetColumn(label, 2); row.Children.Add(label);
            var readOnly = new CheckBox { Content = LocExtension.Get("Emulation.ReadOnly"), IsChecked = item.IsReadOnly, Margin = new Thickness(8, 4, 8, 4), VerticalAlignment = VerticalAlignment.Center };
            readOnly.Checked += (_, _) => item.IsReadOnly = true;
            readOnly.Unchecked += (_, _) => item.IsReadOnly = false;
            Grid.SetColumn(readOnly, 3); row.Children.Add(readOnly);
            var remove = new Button { Content = LocExtension.Get("Common.Delete"), MinWidth = 90, Margin = new Thickness(4) };
            remove.Click += (_, _) => { _media.Remove(item); RefreshMediaRows(); };
            Grid.SetColumn(remove, 4); row.Children.Add(remove);
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
        var duplicates = _keyboardMappings.Where(item => item.HostKey != GWGUI.Emulation.EmulationKey.Unknown)
            .GroupBy(item => item.HostKey).Where(group => group.Count() > 1)
            .Select(group => group.Key).ToHashSet();
        foreach (var item in _keyboardMappings) item.HasConflict = duplicates.Contains(item.HostKey);
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
            ItemsSource = Enum.GetValues<GWGUI.Emulation.EmulationKey>(),
            SelectedItemBinding = new Binding(nameof(KeyMappingItem.HostKey)) { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
            Width = new DataGridLength(1, DataGridLengthUnitType.Star)
        });
    }

    private void ApplyModelDefaults()
    {
        if (_model.SelectedItem is not AmigaModel model) return;
        _cpuModel.ItemsSource = model.CpuModels;
        _cpuModel.SelectedItem = model.DefaultCpu;
        _cpuModel.IsEnabled = model.CpuModels.Count > 1;
        ConfigureFpuChoices();
        _chipset.Text = model.Chipset;
        SelectValue(_chipMemory, Math.Clamp(model.ChipMemoryKib / 512, 1, 4).ToString());
        SelectValue(_slowMemory, model.SlowMemoryKib == 0 ? "0" : Math.Clamp(model.SlowMemoryKib / 256, 2, 7).ToString());
        SelectValue(_fastMemory, model.FastMemoryMib.ToString());
        SelectValue(_z3Memory, "0");
        SelectValue(_videoStandard, "PAL auto");
        SelectValue(_cpuThrottle, "0.0");
        SelectValue(_cpuMultiplier, "0");
        SelectValue(_cpuCompatibility, "exact");
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
        var cpu = _cpuModel.SelectedItem?.ToString() ?? "68000";
        var values = cpu switch
        {
            "68000" or "68010" => new[] { "0" },
            "68020" or "68030" => new[] { "0", "68881", "68882" },
            _ => new[] { "cpu", "0", "68881", "68882" }
        };
        var previous = SelectedText(_fpuModel);
        _fpuModel.ItemsSource = values.Select(value => new OptionChoice(value,
            value == "0" ? LocExtension.Get("HostTools.None") : value == "cpu" ? LocExtension.Get("Emulation.IntegratedFpu") : value)).ToArray();
        SelectValue(_fpuModel, values.Contains(previous) ? previous : DefaultFpu(cpu));
        _fpuModel.IsEnabled = values.Length > 1;
    }

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
        SetOption(_cpuThrottle, configuration, "puae_cpu_throttle", "0.0");
        SetOption(_cpuMultiplier, configuration, "puae_cpu_multiplier", "0");
        SetOption(_cpuCompatibility, configuration, "puae_cpu_compatibility", "exact");
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
        foreach (var item in media)
            _media.Add(new MediaItem { Path = item.Path, Kind = item.Kind, Label = item.Label ?? string.Empty, IsReadOnly = item.IsReadOnly });
        _floppyDriveCount.SelectedItem = Math.Min(selectedModel.MaximumFloppyDrives,
            Math.Max(selectedModel.MaximumFloppyDrives == 0 ? 0 : 1, media.Count(item => item.Kind == AmigaMediaKind.Floppy)));
        _hardDriveCount.SelectedItem = Math.Min(selectedModel.MaximumHardDrives,
            media.Count(item => item.Kind == AmigaMediaKind.HardDrive));
        _cdDrive.IsChecked = selectedModel.HasCdDrive || media.Any(item => item.Kind == AmigaMediaKind.CompactDisc);
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
                HostKey = configuration.Input?.KeyboardMappings?.GetValueOrDefault(key.ToString()) ?? key
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
        SelectChoice(_mouseActions[0], mouseMappings?.GetValueOrDefault("Left") ?? AmigaMouseAction.LeftButton);
        SelectChoice(_mouseActions[1], mouseMappings?.GetValueOrDefault("Right") ?? AmigaMouseAction.RightButton);
        SelectChoice(_mouseActions[2], mouseMappings?.GetValueOrDefault("Middle") ?? AmigaMouseAction.MiddleButton);
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
        options["puae_cpu_throttle"] = SelectedText(_cpuThrottle);
        options["puae_cpu_multiplier"] = SelectedText(_cpuMultiplier);
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
        var keyboard = _keyboardMappings.Where(item => !string.IsNullOrWhiteSpace(item.AmigaKey))
            .ToDictionary(item => item.AmigaKey.Trim(), item => item.HostKey, StringComparer.OrdinalIgnoreCase);
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
            ["Left"] = SelectedChoice(_mouseActions[0], AmigaMouseAction.LeftButton),
            ["Right"] = SelectedChoice(_mouseActions[1], AmigaMouseAction.RightButton),
            ["Middle"] = SelectedChoice(_mouseActions[2], AmigaMouseAction.MiddleButton)
        };
        var input = new AmigaInputConfiguration(keyboard,
            string.IsNullOrWhiteSpace(_mouseDevice.Text) ? null : _mouseDevice.Text.Trim(),
            _captureMouse.IsChecked == true, controllerBindings, mouseMappings,
            (GWGUI.Emulation.EmulationKey)(_releaseMouseKey.SelectedItem ?? GWGUI.Emulation.EmulationKey.Escape));
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
        public GWGUI.Emulation.EmulationKey HostKey { get; set; }
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
}
