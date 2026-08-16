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
    public static event EventHandler<AmigaMachineConfiguration>? ConfigurationSaved;
    private readonly AmigaConfigurationDocuments _configurationDocuments = new(StoragePaths.AmigaConfigurationsDirectory, StoragePaths.DataDirectory);
    private readonly ObservableCollection<ConfigurationItem> _configurations = [];
    private readonly ObservableCollection<FirmwareItem> _firmware = [];
    private readonly ObservableCollection<OptionItem> _options = [];
    private readonly ObservableCollection<MediaItem> _media = [];
    private readonly EmulationStorageDeviceList _storageDevices = new();
    private readonly string[] _floppyDriveModels = ["35dd", "35dd", "35dd", "35dd"];
    private string _cdDriveModel = "CD-ROM";
    private readonly ListBox _list = new() { MinWidth = 260 };
    private readonly ListBox _firmwareList = new() { MinWidth = 360, BorderThickness = new Thickness(0) };
    private readonly Button _useSelectedFirmware = new() { MinWidth = 100, IsEnabled = false };
    private readonly ComboBox _model = new() { ItemsSource = AmigaModelCatalog.All, DisplayMemberPath = nameof(AmigaModel.DisplayName) };
    private readonly TextBox _kickstart = new();
    private readonly TextBox _extendedRom = new();
    private readonly TextBox _romKey = new();
    private PathFieldControls? _extendedRomField;
    private PathFieldControls? _romKeyField;
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
    private readonly TextBlock _mainMemoryHint = new() { TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _extensionMemoryHint = new() { TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _totalMemory = new() { VerticalAlignment = VerticalAlignment.Center, FontSize = 16 };
    private readonly ComboBox _videoStandard = new() { ItemsSource = new[] { "PAL", "NTSC" } };
    private readonly ComboBox _chipset = new() { IsEnabled = false };
    private readonly ComboBox _videoResolution = new() { ItemsSource = new[] { "auto", "auto-lores", "auto-superhires", "lores", "hires", "superhires" } };
    private readonly ComboBox _videoAspect = new() { ItemsSource = new[] { "auto", "PAL", "NTSC", "1:1" } };
    private readonly ComboBox _cropVideo = new() { ItemsSource = new[] { "disabled", "minimum", "smaller", "small", "medium", "large", "larger", "maximum", "auto" } };
    private readonly ComboBox _videoLineMode = new();
    private readonly ComboBox _videoHzChange = new();
    private readonly ComboBox _videoFrameskip = new();
    private readonly ComboBox _videoColors = new();
    private readonly ComboBox _videoGamma = new();
    private readonly ComboBox _videoRenderer = new();
    private readonly ComboBox _immediateBlits = new();
    private readonly ComboBox _collisionLevel = new();
    private readonly CheckBox _flickerFixer = new();
    private readonly ComboBox _audioOutput = new() { DisplayMemberPath = nameof(AudioOutputDevice.Name) };
    private readonly ComboBox _audioLatency = new();
    private readonly ComboBox _audioInterpolation = new() { ItemsSource = new[] { "none", "anti", "sinc", "rh", "crux" } };
    private readonly ComboBox _audioFilter = new() { ItemsSource = new[] { "emulated", "off", "on" } };
    private readonly ComboBox _audioFilterType = new();
    private readonly Slider _floppySound = new() { Minimum = 0, Maximum = 100, Value = 80, TickFrequency = 5, IsSnapToTickEnabled = true };
    private readonly ComboBox _floppySoundType = new();
    private readonly CheckBox _muteEmptyFloppy = new();
    private readonly Slider _cdAudioVolume = new() { Minimum = 0, Maximum = 100, Value = 100, TickFrequency = 5, IsSnapToTickEnabled = true };
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
    private readonly TextBox _mouseSpeedRatio = new()
    {
        Text = "1.00",
        HorizontalContentAlignment = HorizontalAlignment.Right,
        MaxLength = 5
    };
    private readonly ComboBox _analogMouse = new();
    private readonly ComboBox _analogMouseDeadzone = new();
    private readonly ComboBox _analogMouseSpeed = new();
    private readonly ComboBox _analogMouseSpeedRight = new();
    private readonly ComboBox _releaseMouseKey = new() { ItemsSource = Enum.GetValues<GWGUI.Emulation.EmulationKey>().Where(key => key != GWGUI.Emulation.EmulationKey.Unknown) };
    private readonly InputBindingEditor _amigaMouseEditor = new();
    private readonly ComboBox[] _controllerDevices = Enumerable.Range(0, 4).Select(_ => new ComboBox { DisplayMemberPath = nameof(GameControllerDevice.Name) }).ToArray();
    private readonly InputBindingEditor[] _controllerEditors = Enumerable.Range(0, 4)
        .Select(_ => new InputBindingEditor()).ToArray();
    private readonly CheckBox _keyboardPassThrough = new();
    private readonly InputBindingEditor _globalShortcutEditor = new();
    private readonly InputBindingEditor _amigaKeyboardEditor = new();
    private readonly ComboBox _turboPulse = new();
    private readonly ComboBox _joyPortOrder = new();
    private readonly TextBox _storageBaseFolder = new();
    private readonly TextBox _captureFolder = new();
    private readonly TextBox _stateFolder = new();
    private readonly TextBox _amigaHardDisksFolder = new();
    private readonly TextBlock _detectedDevices = new() { TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _detectedControllers = new() { TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center };
    private readonly TextBlock _storageTree = new() { LineHeight = 24 };
    private readonly TabControl _familyTabs = new()
    {
        Margin = new Thickness(8),
        HorizontalContentAlignment = HorizontalAlignment.Stretch,
        VerticalContentAlignment = VerticalAlignment.Stretch
    };
    private AppSettings? _appSettings;
    private Func<Task>? _persistAppSettings;
    private Guid _currentId;
    private bool _loading;

    public OptionsEmulationSection()
    {
        _list.ItemsSource = _configurations;
        _list.DisplayMemberPath = nameof(ConfigurationItem.DisplayName);
        _list.SelectionChanged += ConfigurationSelected;
        _firmwareList.SelectionChanged += (_, _) =>
            _useSelectedFirmware.IsEnabled = SelectedFirmware() is not null;
        _model.SelectionChanged += (_, _) => ApplyModelDefaults();
        _kickstart.TextChanged += (_, _) => UpdateRomFieldAvailability();
        _chipMemory.SelectionChanged += (_, _) => UpdateMemorySummary();
        _slowMemory.SelectionChanged += (_, _) => UpdateMemorySummary();
        _fastMemory.SelectionChanged += (_, _) => UpdateMemorySummary();
        _z3Memory.SelectionChanged += (_, _) => UpdateMemorySummary();
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
        _storageDevices.AddRequested += (_, _) => AddStorageDevice();
        _storageDevices.ConfigureRequested += (_, args) => ConfigureStorageDevice(args.Device);
        _storageDevices.RemoveRequested += (_, args) => RemoveStorageDevice(args.Device);
        _globalShortcutEditor.ConfigurePresentation(LocExtension.Get("Emulation.InputActions"),
            LocExtension.Get("Emulation.SearchBinding"));
        _amigaKeyboardEditor.ConfigurePresentation(LocExtension.Get("Emulation.SystemKey", "Amiga"),
            LocExtension.Get("Emulation.SearchBinding"));
        _amigaMouseEditor.ConfigurePresentation(LocExtension.Get("Emulation.EmulatedAction"),
            LocExtension.Get("Emulation.SearchInputBinding"));
        _amigaMouseEditor.ConfigureCaptureSources(
            InputCaptureSources.Keyboard | InputCaptureSources.Mouse | InputCaptureSources.Controller,
            prefixKeyboardSource: true);
        foreach (var editor in _controllerEditors)
        {
            editor.ConfigurePresentation(LocExtension.Get("Emulation.EmulatedAction"),
                LocExtension.Get("Emulation.SearchInputBinding"));
            editor.ConfigureCaptureSources(
                InputCaptureSources.Keyboard | InputCaptureSources.Mouse | InputCaptureSources.Controller,
                prefixKeyboardSource: true);
        }
        for (var port = 0; port < _controllerEditors.Length; port++)
        {
            var capturedPort = port;
            _controllerEditors[port].ControllerCaptured += (_, args) =>
            {
                var device = _controllerDevices[capturedPort].Items.Cast<GameControllerDevice>()
                    .FirstOrDefault(item => item.Id == $"xinput:{args.Port}");
                if (device is not null) _controllerDevices[capturedPort].SelectedItem = device;
            };
        }
        _globalShortcutEditor.BindingsChanged += async (_, _) => await SaveGlobalShortcutsAsync();
        _multiDrive.Content = LocExtension.Get("Emulation.MultiDrive");
        ConfigureOptionChoices();
        for (var port = 0; port < _controllers.Length; port++)
        {
            var controller = _controllers[port];
            var capturedPort = port;
            controller.SelectionChanged += (_, _) =>
            {
                if (!_loading) RefreshControllerMappings(capturedPort, preserveBindings: true);
            };
        }

        AddFamilyTab("\uE713", LocExtension.Get("Emulation.GeneralTab"), BuildGeneralEmulationSettings());
        AddFamilyTab("\uE765", LocExtension.Get("Emulation.ShortcutsTab"), BuildGlobalInputAssignments());
        AddFamilyTab("\uE8A5", LocExtension.Get("Emulation.Configurations"), BuildConfigurationCatalog());
        AddFamilyTab("\uE7FC", "Amiga", BuildAmigaEditor());
        Content = _familyTabs;
        Loaded += async (_, _) => await ReloadAsync();
    }

    public void Configure(AppSettings settings, Func<Task> persistSettings)
    {
        _appSettings = settings;
        _persistAppSettings = persistSettings;
        StoragePaths.ConfigureEmulationStorageDirectory(settings.EmulationStorageFolder);
        StoragePaths.ConfigureEmulationStateDirectory(settings.EmulationStateFolder);
        StoragePaths.ConfigureEmulationCaptureDirectory(settings.EmulationCaptureFolder);
        StoragePaths.ConfigureAmigaHardDisksDirectory(settings.AmigaHardDisksFolder);
        _storageBaseFolder.Text = settings.EmulationStorageFolder;
        _captureFolder.Text = settings.EmulationCaptureFolder;
        _stateFolder.Text = settings.EmulationStateFolder;
        _amigaHardDisksFolder.Text = StoragePaths.AmigaHardDisksDirectory;
        _globalShortcutEditor.SetRows(GlobalShortcutDefinitions(), settings.EmulationShortcuts);
        _amigaKeyboardEditor.SetReservedBindings(settings.EmulationShortcuts.Values);
        EnsureStorageFolders();
    }

    private void AddFamilyTab(string icon, string title, UIElement content)
    {
        var tab = new TabItem
        {
            Header = new MainTabHeader { Icon = icon, Text = title },
            Content = content,
            Padding = new Thickness(14, 8, 14, 8)
        };
        tab.SetResourceReference(StyleProperty, "MainTabItemStyle");
        _familyTabs.Items.Add(tab);
    }

    private UIElement BuildGeneralEmulationSettings()
    {
        var root = new Grid { Margin = new Thickness(14) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var defaults = new StackPanel { Margin = new Thickness(8, 4, 8, 6) };
        defaults.Children.Add(BuildCompactPathRow(LocExtension.Get("Emulation.StorageBaseFolder"), _storageBaseFolder,
            BrowseStorageBaseFolderAsync, OpenStorageBaseFolderAsync));
        defaults.Children.Add(BuildCompactPathRow(LocExtension.Get("Emulation.CaptureFolder"), _captureFolder, BrowseCaptureFolderAsync));
        defaults.Children.Add(BuildCompactPathRow(LocExtension.Get("Emulation.StateFolder"), _stateFolder, BrowseStateFolderAsync));
        var folders = Card(defaults, LocExtension.Get("Emulation.DefaultFolders"));
        root.Children.Add(folders);
        return root;
    }

    private UIElement BuildGlobalInputAssignments()
    {
        var root = new Grid { Margin = new Thickness(14) };
        root.Children.Add(InputBindingCard(_globalShortcutEditor, LocExtension.Get("Emulation.GlobalShortcuts")));
        return root;
    }

    private static IReadOnlyList<InputBindingDefinition> GlobalShortcutDefinitions() =>
    [
        new(EmulationShortcutDefaults.ReleaseMouse, LocExtension.Get("Emulation.Shortcut.ReleaseMouse"), EmulationShortcutDefaults.Values[EmulationShortcutDefaults.ReleaseMouse]),
        new(EmulationShortcutDefaults.PauseResume, LocExtension.Get("Emulation.Shortcut.PauseResume"), EmulationShortcutDefaults.Values[EmulationShortcutDefaults.PauseResume]),
        new(EmulationShortcutDefaults.ToggleFullscreen, LocExtension.Get("Emulation.Shortcut.Fullscreen"), EmulationShortcutDefaults.Values[EmulationShortcutDefaults.ToggleFullscreen]),
        new(EmulationShortcutDefaults.Power, LocExtension.Get("Emulation.Shortcut.Power"), EmulationShortcutDefaults.Values[EmulationShortcutDefaults.Power]),
        new(EmulationShortcutDefaults.SoftReset, LocExtension.Get("Emulation.Shortcut.SoftReset"), EmulationShortcutDefaults.Values[EmulationShortcutDefaults.SoftReset]),
        new(EmulationShortcutDefaults.HardReset, LocExtension.Get("Emulation.Shortcut.HardReset"), EmulationShortcutDefaults.Values[EmulationShortcutDefaults.HardReset]),
        new(EmulationShortcutDefaults.QuickSave, LocExtension.Get("Emulation.Shortcut.QuickSave"), EmulationShortcutDefaults.Values[EmulationShortcutDefaults.QuickSave]),
        new(EmulationShortcutDefaults.QuickLoad, LocExtension.Get("Emulation.Shortcut.QuickLoad"), EmulationShortcutDefaults.Values[EmulationShortcutDefaults.QuickLoad]),
        new(EmulationShortcutDefaults.Screenshot, LocExtension.Get("Emulation.Shortcut.Screenshot"), EmulationShortcutDefaults.Values[EmulationShortcutDefaults.Screenshot]),
        new(EmulationShortcutDefaults.ToggleMute, LocExtension.Get("Emulation.Shortcut.Mute"), EmulationShortcutDefaults.Values[EmulationShortcutDefaults.ToggleMute]),
        new(EmulationShortcutDefaults.FastForward, LocExtension.Get("Emulation.Shortcut.FastForward"), EmulationShortcutDefaults.Values[EmulationShortcutDefaults.FastForward]),
        new(EmulationShortcutDefaults.InsertMedia, LocExtension.Get(EmulationResourceKeys.InsertMedia), EmulationShortcutDefaults.Values[EmulationShortcutDefaults.InsertMedia]),
        new(EmulationShortcutDefaults.EjectMedia, LocExtension.Get(EmulationResourceKeys.EjectMedia), EmulationShortcutDefaults.Values[EmulationShortcutDefaults.EjectMedia]),
        new(EmulationShortcutDefaults.NextMedia, LocExtension.Get(EmulationResourceKeys.NextMedia), EmulationShortcutDefaults.Values[EmulationShortcutDefaults.NextMedia])
    ];

    private static IReadOnlyList<InputBindingDefinition> AmigaSpecialKeyDefinitions()
    {
        var keys = new[]
        {
            GWGUI.Emulation.EmulationKey.F1, GWGUI.Emulation.EmulationKey.F2,
            GWGUI.Emulation.EmulationKey.F3, GWGUI.Emulation.EmulationKey.F4,
            GWGUI.Emulation.EmulationKey.F5, GWGUI.Emulation.EmulationKey.F6,
            GWGUI.Emulation.EmulationKey.F7, GWGUI.Emulation.EmulationKey.F8,
            GWGUI.Emulation.EmulationKey.F9, GWGUI.Emulation.EmulationKey.F10
        };
        var definitions = keys.Select(key => new InputBindingDefinition(key.ToString(), key.ToString(), key.ToString())).ToList();
        definitions.Add(new InputBindingDefinition(nameof(GWGUI.Emulation.EmulationKey.Help),
            LocExtension.Get("Emulation.Key.Help"), "Insert"));
        definitions.Add(new InputBindingDefinition(nameof(GWGUI.Emulation.EmulationKey.LeftAmiga),
            LocExtension.Get("Emulation.Key.LeftAmiga"), "PageUp"));
        definitions.Add(new InputBindingDefinition(nameof(GWGUI.Emulation.EmulationKey.RightAmiga),
            LocExtension.Get("Emulation.Key.RightAmiga"), "PageDown"));
        return definitions;
    }

    private static IReadOnlyList<InputBindingDefinition> AmigaMouseDefinitions(AmigaModel model)
    {
        var definitions = new List<InputBindingDefinition>
        {
            new(nameof(AmigaMouseAction.LeftButton), LocExtension.Get("Emulation.MouseLeftButton"), "Mouse:Left"),
            new(nameof(AmigaMouseAction.RightButton), LocExtension.Get("Emulation.MouseRightButton"), "Mouse:Right")
        };
        if (model.MouseButtonCount >= 3)
            definitions.Add(new InputBindingDefinition(nameof(AmigaMouseAction.MiddleButton),
                LocExtension.Get("Emulation.MouseMiddleButton"), "Mouse:Middle"));
        return definitions;
    }

    private void RefreshMouseMappings(bool preserveBindings)
    {
        if (_model.SelectedItem is not AmigaModel model) return;
        var values = preserveBindings
            ? _amigaMouseEditor.Rows.ToDictionary(row => row.Id, row => row.Binding, StringComparer.OrdinalIgnoreCase)
            : null;
        _amigaMouseEditor.SetRows(AmigaMouseDefinitions(model), values);
    }

    private static IReadOnlyList<InputBindingDefinition> AmigaControllerDefinitions(AmigaControllerType type)
    {
        if (type is AmigaControllerType.None or AmigaControllerType.Keyboard) return [];
        var directions = new List<InputBindingDefinition>
        {
            new("Up", LocExtension.Get("Emulation.DirectionUp"), string.Empty),
            new("Down", LocExtension.Get("Emulation.DirectionDown"), string.Empty),
            new("Left", LocExtension.Get("Emulation.DirectionLeft"), string.Empty),
            new("Right", LocExtension.Get("Emulation.DirectionRight"), string.Empty)
        };
        if (type == AmigaControllerType.Cd32Pad)
        {
            directions.AddRange([
                new("B", LocExtension.Get("Emulation.Cd32Red"), string.Empty),
                new("A", LocExtension.Get("Emulation.Cd32Blue"), string.Empty),
                new("Y", LocExtension.Get("Emulation.Cd32Green"), string.Empty),
                new("X", LocExtension.Get("Emulation.Cd32Yellow"), string.Empty),
                new("L", LocExtension.Get("Emulation.Cd32Rewind"), string.Empty),
                new("R", LocExtension.Get("Emulation.Cd32FastForward"), string.Empty),
                new("Start", LocExtension.Get("Emulation.Cd32PlayPause"), string.Empty)
            ]);
            directions.Add(new InputBindingDefinition("L2", LocExtension.Get("Emulation.TurboFire"), string.Empty));
            return directions;
        }
        directions.Add(new InputBindingDefinition("B", LocExtension.Get("Emulation.FireButton1"), string.Empty));
        directions.Add(new InputBindingDefinition("A", LocExtension.Get("Emulation.FireButton2"), string.Empty));
        directions.Add(new InputBindingDefinition("L2", LocExtension.Get("Emulation.TurboFire"), string.Empty));
        return directions;
    }

    private void RefreshControllerMappings(int port, bool preserveBindings,
        IReadOnlyDictionary<string, string>? configuredMappings = null)
    {
        var type = SelectedChoice(_controllers[port], AmigaControllerType.Automatic);
        var definitions = AmigaControllerDefinitions(type);
        var values = configuredMappings is not null
            ? definitions.ToDictionary(definition => definition.Id,
                definition => configuredMappings.FirstOrDefault(item => item.Value == definition.Id).Key
                    ?? definition.DefaultBinding, StringComparer.OrdinalIgnoreCase)
            : preserveBindings
                ? _controllerEditors[port].Rows.ToDictionary(row => row.Id, row => row.Binding,
                    StringComparer.OrdinalIgnoreCase)
                : null;
        _controllerEditors[port].SetRows(definitions, values);
    }

    private async Task SaveGlobalShortcutsAsync()
    {
        if (_appSettings is null || _globalShortcutEditor.HasErrors) return;
        _appSettings.EmulationShortcuts = _globalShortcutEditor.Rows
            .ToDictionary(row => row.Id, row => row.Binding, StringComparer.Ordinal);
        _amigaKeyboardEditor.SetReservedBindings(_appSettings.EmulationShortcuts.Values);
        if (_persistAppSettings is not null) await _persistAppSettings();
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
        var browseButton = ControlUiFactory.TextButton(LocExtension.Get("Common.Browse"), 110,
            async (_, _) => await browse(), new Thickness(8, 0, 0, 0));
        Grid.SetColumn(browseButton, 2); row.Children.Add(browseButton);
        if (open is not null)
        {
            var openButton = ControlUiFactory.TextButton(LocExtension.Get("Common.OpenFolder"), 120,
                async (_, _) => await open(), new Thickness(8, 0, 0, 0));
            Grid.SetColumn(openButton, 3); row.Children.Add(openButton);
        }
        return row;
    }

    private static FrameworkElement BuildCompactPathRow(string label, TextBox textBox, Func<Task> browse, Func<Task>? open = null)
    {
        var row = new Grid { Margin = new Thickness(2) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(285) });
        row.ColumnDefinitions.Add(new ColumnDefinition());
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        if (open is not null) row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        row.Children.Add(new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) });
        textBox.MinWidth = 0;
        textBox.Height = 32;
        Grid.SetColumn(textBox, 1);
        row.Children.Add(textBox);
        var browseButton = ControlUiFactory.TextButton(LocExtension.Get("Common.Browse"), 96,
            async (_, _) => await browse(), new Thickness(6, 0, 0, 0), 32);
        Grid.SetColumn(browseButton, 2);
        row.Children.Add(browseButton);
        if (open is not null)
        {
            var openButton = ControlUiFactory.TextButton(LocExtension.Get("Common.OpenFolder"), 112,
                async (_, _) => await open(), new Thickness(6, 0, 0, 0), 32);
            Grid.SetColumn(openButton, 3);
            row.Children.Add(openButton);
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
        _appSettings.AmigaHardDisksFolder = Path.GetFullPath(_amigaHardDisksFolder.Text.Trim());
        StoragePaths.ConfigureEmulationStorageDirectory(_appSettings.EmulationStorageFolder);
        StoragePaths.ConfigureEmulationStateDirectory(_appSettings.EmulationStateFolder);
        StoragePaths.ConfigureEmulationCaptureDirectory(_appSettings.EmulationCaptureFolder);
        StoragePaths.ConfigureAmigaHardDisksDirectory(_appSettings.AmigaHardDisksFolder);
        EnsureStorageFolders();
        if (_persistAppSettings is not null) await _persistAppSettings();
    }

    private void EnsureStorageFolders()
    {
        if (_appSettings?.CreateEmulationFoldersAutomatically != true) return;
        foreach (var path in new[]
                 {
                     _appSettings.AmigaHardDisksFolder,
                     Path.Combine(_appSettings.EmulationStorageFolder, "HDD", "Atari"),
                     StoragePaths.AmigaFloppyImagesDirectory,
                     StoragePaths.AmigaCompactDiscsDirectory,
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
        root.RowDefinitions.Add(new RowDefinition());
        var tabs = new TabControl
        {
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch
        };
        AddMachineTab(tabs, "\uE713", LocExtension.Get("Emulation.GeneralTab"), BuildAmigaGeneralTab());
        AddMachineTab(tabs, "\uE950", "CPU", BuildCpuTab());
        AddMachineTab(tabs, "\uE964", "RAM", BuildRamTab());
        AddMachineTab(tabs, "\uE8B7", "ROM", BuildRomTab());
        AddMachineTab(tabs, "\uE7F4", LocExtension.Get("Emulation.VideoTab"), BuildVideoTab());
        AddMachineTab(tabs, "\uE767", LocExtension.Get("Emulation.Audio"), BuildAudioTab());
        AddMachineTab(tabs, "\uEDA2", LocExtension.Get("Emulation.StorageTab"), BuildStorageTab());
        AddMachineTab(tabs, "\uE765", LocExtension.Get("Emulation.KeyboardTab"), BuildKeyboardTab());
        AddMachineTab(tabs, "\uE962", LocExtension.Get("Emulation.MouseTab"), BuildMouseTab());
        AddMachineTab(tabs, "\uE7FC", LocExtension.Get("Emulation.ControllersTab"), BuildControllersTab());
        root.Children.Add(tabs);
        return root;
    }

    private UIElement BuildAmigaGeneralTab()
    {
        var panel = new StackPanel { Margin = new Thickness(12) };
        var configuration = new Grid { Margin = new Thickness(14, 10, 14, 10) };
        configuration.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        configuration.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(360) });
        configuration.ColumnDefinitions.Add(new ColumnDefinition());
        configuration.Children.Add(new TextBlock
        {
            Text = LocExtension.Get("Emulation.Model"),
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 12, 0)
        });
        _model.Height = 36;
        _model.Margin = new Thickness(0);
        _model.VerticalAlignment = VerticalAlignment.Center;
        Grid.SetColumn(_model, 1);
        configuration.Children.Add(_model);
        var actions = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center };
        AddButton(actions, "Common.Save", SaveConfigurationAsync);
        Grid.SetColumn(actions, 2);
        configuration.Children.Add(actions);
        var configurationCard = new Border
        {
            Child = configuration,
            CornerRadius = new CornerRadius(9),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 0, 0, 12)
        };
        ControlUiFactory.ApplyCardAppearance(configurationCard);
        panel.Children.Add(configurationCard);
        panel.Children.Add(new AmigaCoreManagementSection { Margin = new Thickness(0, 0, 0, 12) });
        var hardDisks = new StackPanel { Margin = new Thickness(8, 6, 8, 8) };
        hardDisks.Children.Add(BuildPathRow(LocExtension.Get("Emulation.HardDisks"), _amigaHardDisksFolder,
            () => BrowseGeneralFolderAsync(_amigaHardDisksFolder, "Emulation.HardDisks")));
        panel.Children.Add(Card(hardDisks, LocExtension.Get("Emulation.DefaultFolders")));
        return ScrollPage(panel);
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
        var mainMemory = new StackPanel();
        mainMemory.Children.Add(FieldGrid((LocExtension.Get("Emulation.ChipMemory"), _chipMemory),
            (LocExtension.Get("Emulation.SlowMemory"), _slowMemory)));
        mainMemory.Children.Add(InformationBanner(_mainMemoryHint));

        var extensions = new StackPanel();
        extensions.Children.Add(FieldGrid((LocExtension.Get("Emulation.FastMemory"), _fastMemory),
            (LocExtension.Get("Emulation.Z3Memory"), _z3Memory)));
        extensions.Children.Add(InformationBanner(_extensionMemoryHint));

        var root = TwoColumnPage(
            IconCard(mainMemory, LocExtension.Get("Emulation.MainMemory"), "\uE964"),
            IconCard(extensions, LocExtension.Get("Emulation.MemoryExtensions"), "\uE950"));
        root.Children.Add(FullWidthMemorySummary(1));
        return ScrollPage(root);
    }

    private UIElement BuildVideoTab()
    {
        var display = TileGrid(2,
            LabeledTile(LocExtension.Get("Emulation.VideoStandard"), _videoStandard),
            LabeledTile(LocExtension.Get("Emulation.AspectRatio"), _videoAspect),
            LabeledTile(LocExtension.Get("Emulation.VideoResolution"), _videoResolution),
            LabeledTile(LocExtension.Get("Emulation.VideoLineMode"), _videoLineMode),
            LabeledTile(LocExtension.Get("Emulation.VideoCrop"), _cropVideo, columnSpan: 2));
        var rendering = TileGrid(2,
            LabeledTile(LocExtension.Get("Emulation.RenderingSettings"), _videoRenderer),
            LabeledTile(LocExtension.Get("Emulation.VideoColors"), _videoColors),
            LabeledTile(LocExtension.Get("Emulation.VideoFrameskip"), _videoFrameskip),
            LabeledTile(LocExtension.Get("Emulation.VideoGamma"), _videoGamma),
            TrailingCheckBoxTile(LocExtension.Get("Emulation.FlickerFixer"), _flickerFixer));
        var root = TwoColumnPage(
            ActionCard(display, LocExtension.Get("Emulation.DisplaySettings")),
            ActionCard(rendering, LocExtension.Get("Emulation.RenderingSettings")));
        root.Children.Add(FullWidthActionCard(TileGrid(4,
            LabeledTile(LocExtension.Get("Emulation.Chipset"), _chipset),
            LabeledTile(LocExtension.Get("Emulation.ImmediateBlits"), _immediateBlits),
            LabeledTile(LocExtension.Get("Emulation.CollisionLevel"), _collisionLevel),
            LabeledTile(LocExtension.Get("Emulation.VideoHzChange"), _videoHzChange)),
            LocExtension.Get("Emulation.ChipsetCompatibility"), 1));
        return ScrollPage(root);
    }

    private UIElement BuildRomTab()
    {
        var page = new Grid { Margin = new Thickness(12) };
        page.RowDefinitions.Add(new RowDefinition());
        page.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var root = new Grid();
        root.ColumnDefinitions.Add(new ColumnDefinition());
        root.ColumnDefinitions.Add(new ColumnDefinition());
        var form = CreateForm(3);
        AddPathField(form, 0, "Kickstart", _kickstart, "ROM|*.rom;*.bin|All files|*.*");
        _extendedRomField = AddPathField(form, 1, LocExtension.Get("Emulation.ExtendedRom"), _extendedRom,
            "ROM|*.rom;*.bin|All files|*.*", LocExtension.Get("Emulation.NotUsed"));
        _romKeyField = AddPathField(form, 2, LocExtension.Get("Emulation.RomKey"), _romKey,
            "ROM key|*.key|All files|*.*", LocExtension.Get("Emulation.NotUsed"));
        UpdateRomFieldAvailability();
        var pathsCard = ActionCard(form, LocExtension.Get("Emulation.SystemRom"));
        pathsCard.Margin = new Thickness(0, 0, 5, 0);
        root.Children.Add(pathsCard);

        var headerActions = new StackPanel { Orientation = Orientation.Horizontal };
        var refresh = CreateActionButton("\uE72C", LocExtension.Get("Common.Refresh"));
        refresh.Click += async (_, _) => await RunUiActionAsync(refresh, RefreshFirmwareAsync);
        headerActions.Children.Add(refresh);
        _useSelectedFirmware.Content = LocExtension.Get("Emulation.UseFirmware");
        _useSelectedFirmware.Margin = new Thickness(8, 0, 0, 0);
        _useSelectedFirmware.Click += (_, _) => UseFirmware(SelectedFirmware());
        headerActions.Children.Add(_useSelectedFirmware);
        var firmwareCard = ActionCard(_firmwareList, LocExtension.Get("Emulation.DetectedRoms"), headerActions);
        firmwareCard.Margin = new Thickness(5, 0, 0, 0);
        Grid.SetColumn(firmwareCard, 1);
        root.Children.Add(firmwareCard);
        page.Children.Add(root);

        var openFolder = CreateActionButton("\uE838", LocExtension.Get("Emulation.OpenRomFolder"));
        openFolder.HorizontalAlignment = HorizontalAlignment.Left;
        openFolder.Margin = new Thickness(0, 12, 0, 0);
        openFolder.Click += async (_, _) => await RunUiActionAsync(openFolder, OpenFirmwareFolder);
        Grid.SetRow(openFolder, 1);
        page.Children.Add(openFolder);
        return ScrollPage(page);
    }

    private UIElement BuildAudioTab()
    {
        _audio.Content = LocExtension.Get("Emulation.AudioEnabled");
        _muteEmptyFloppy.Content = LocExtension.Get("Emulation.MuteEmptyFloppy");
        var root = ThreeColumnPage(
            ActionCard(TileGrid(1,
                CheckBoxTile(_audio),
                LabeledTile(LocExtension.Get("Emulation.AudioDevice"), _audioOutput),
                LabeledTile(LocExtension.Get("Emulation.Latency"), _audioLatency)),
                LocExtension.Get("Emulation.AudioOutput")),
            ActionCard(TileGrid(1,
                LabeledTile(LocExtension.Get("Emulation.AudioInterpolation"), _audioInterpolation),
                LabeledTile(LocExtension.Get("Emulation.AudioFilter"), _audioFilter),
                LabeledTile(LocExtension.Get("Emulation.AudioFilterType"), _audioFilterType),
                PercentageSliderTile(LocExtension.Get("Emulation.StereoSeparation"), _stereoSeparation)),
                LocExtension.Get("Emulation.AudioQuality")),
            ActionCard(TileGrid(1,
                PercentageSliderTile(LocExtension.Get("Emulation.FloppySound"), _floppySound),
                LabeledTile(LocExtension.Get("Emulation.Type"), _floppySoundType),
                CheckBoxTile(_muteEmptyFloppy),
                PercentageSliderTile(LocExtension.Get("Emulation.CdAudioVolume"), _cdAudioVolume)),
                LocExtension.Get("Emulation.AudioDrives")));
        var audioInput = InformationBanner(new TextBlock
        {
            Text = LocExtension.Get("Emulation.AudioInputUnavailable"),
            TextWrapping = TextWrapping.Wrap
        });
        audioInput.Margin = new Thickness(0, 12, 0, 0);
        Grid.SetRow(audioInput, 1);
        Grid.SetColumnSpan(audioInput, 3);
        root.Children.Add(audioInput);
        return ScrollPage(root);
    }

    private UIElement BuildStorageTab()
    {
        var root = new Grid { Margin = new Thickness(12) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var devicePanel = new StackPanel();
        devicePanel.Children.Add(_storageDevices);
        devicePanel.Children.Add(InformationBanner(new TextBlock
        {
            Text = LocExtension.Get("Emulation.RemovableMediaRuntimeHint"),
            TextWrapping = TextWrapping.Wrap
        }));
        var devicesCard = ActionCard(devicePanel, LocExtension.Get("Emulation.StorageDevices"));
        devicesCard.Margin = new Thickness(0, 0, 0, 8);
        root.Children.Add(devicesCard);
        return ScrollPage(root);
    }

    private UIElement BuildKeyboardTab()
    {
        var panel = new Grid { Margin = new Thickness(12) };
        panel.RowDefinitions.Add(new RowDefinition());
        panel.Children.Add(InputBindingCard(_amigaKeyboardEditor, LocExtension.Get("Emulation.InputActions"),
            LocExtension.Get("Emulation.SpecialKeysOnlyHint")));
        return panel;
    }

    private UIElement BuildMouseTab()
    {
        var root = TwoColumnPage(
            IconCard(CreateCompactForm(1,
                    (LocExtension.Get("Emulation.MouseSpeed"), _mouseSpeedRatio)),
                LocExtension.Get("Emulation.MouseTab"), "\uE962"),
            IconCard(CreateCompactForm(1,
                    (LocExtension.Get("Emulation.AnalogMouse"), _analogMouse),
                    (LocExtension.Get("Emulation.AnalogMouseDeadzone"), _analogMouseDeadzone),
                    ($"{LocExtension.Get("Emulation.AnalogMouseSpeed")} ({LocExtension.Get("Emulation.LeftStick")})", _analogMouseSpeed),
                    ($"{LocExtension.Get("Emulation.AnalogMouseSpeed")} ({LocExtension.Get("Emulation.RightStick")})", _analogMouseSpeedRight)),
                LocExtension.Get("Emulation.AnalogMouse"), "\uE7FC"));
        root.Children.Add(FullWidthCard(
            InputBindingCard(_amigaMouseEditor, LocExtension.Get("Emulation.MouseActions"),
                LocExtension.Get("Emulation.InputCaptureHint")), string.Empty, 1));
        return ScrollPage(root);
    }

    private UIElement BuildControllersTab()
    {
        var root = new Grid { Margin = new Thickness(12) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var detect = new Grid { Margin = new Thickness(12, 8, 12, 10) };
        detect.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        detect.ColumnDefinitions.Add(new ColumnDefinition());
        AddButton(detect, "Emulation.DetectControllers", DetectControllersAsync);
        Grid.SetColumn(_detectedControllers, 1);
        _detectedControllers.Margin = new Thickness(14, 0, 0, 0);
        detect.Children.Add(_detectedControllers);
        root.Children.Add(IconCard(detect, LocExtension.Get("Emulation.DetectedControllers"), "\uE7FC"));

        var ports = new Grid { Margin = new Thickness(0, 10, 0, 10) };
        ports.ColumnDefinitions.Add(new ColumnDefinition());
        ports.ColumnDefinitions.Add(new ColumnDefinition());
        for (var port = 0; port < 2; port++)
        {
            var portLabel = LocExtension.Get("Emulation.ControllerPort", port + 1);
            var form = FieldGrid(
                (LocExtension.Get("Emulation.ControllerType"), _controllers[port]),
                (LocExtension.Get("Emulation.ControllerDevice", port + 1), _controllerDevices[port]));
            var card = IconCard(form, portLabel, "\uE7FC");
            card.Margin = new Thickness(port == 0 ? 0 : 5, 0, port == 0 ? 5 : 0, 0);
            Grid.SetColumn(card, port); ports.Children.Add(card);
        }
        Grid.SetRow(ports, 1); root.Children.Add(ports);

        var mappingTabs = new TabControl { Margin = new Thickness(0, 0, 8, 0), MinHeight = 300 };
        for (var port = 0; port < 2; port++)
            mappingTabs.Items.Add(new TabItem
            {
                Header = LocExtension.Get("Emulation.ControllerPort", port + 1),
                Content = _controllerEditors[port]
            });
        var behavior = new Grid { Margin = new Thickness(16, 12, 16, 14) };
        behavior.ColumnDefinitions.Add(new ColumnDefinition());
        behavior.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
        behavior.Children.Add(new TextBlock
        {
            Text = LocExtension.Get("Emulation.TurboPulse"),
            VerticalAlignment = VerticalAlignment.Center
        });
        _turboPulse.HorizontalAlignment = HorizontalAlignment.Stretch;
        Grid.SetColumn(_turboPulse, 1);
        behavior.Children.Add(_turboPulse);
        var lower = new Grid();
        lower.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        lower.ColumnDefinitions.Add(new ColumnDefinition());
        lower.Children.Add(ActionCard(mappingTabs, LocExtension.Get("Emulation.ControllerMappings")));
        var behaviorCard = IconCard(behavior, LocExtension.Get("Emulation.TurboFire"), "\uE945");
        behaviorCard.Margin = new Thickness(10, 0, 0, 0);
        behaviorCard.VerticalAlignment = VerticalAlignment.Top;
        Grid.SetColumn(behaviorCard, 1);
        lower.Children.Add(behaviorCard);
        Grid.SetRow(lower, 2); root.Children.Add(lower);
        return ScrollPage(root);
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
        _chipMemory.ItemsSource = EmulationOptionCatalog.InitialChipMemory();
        _slowMemory.ItemsSource = EmulationOptionCatalog.InitialSlowMemory();
        _fastMemory.ItemsSource = EmulationOptionCatalog.MemoryChoices([0, 1, 2, 4, 8]);
        _z3Memory.ItemsSource = EmulationOptionCatalog.MemoryChoices([0, 1, 2, 4, 8, 16, 32, 64, 128, 256, 512]);
        _videoStandard.ItemsSource = EmulationOptionCatalog.VideoStandards();
        _cpuCompatibility.ItemsSource = EmulationOptionCatalog.CpuCompatibility();
        _videoResolution.ItemsSource = EmulationOptionCatalog.Choices(("auto", "Visual.Automatic"), ("auto-lores", "Emulation.ResolutionAutoLow"), ("auto-superhires", "Emulation.ResolutionAutoSuperHigh"), ("lores", "Emulation.ResolutionLow"), ("hires", "Emulation.ResolutionHigh"), ("superhires", "Emulation.ResolutionSuperHigh"));
        _videoAspect.ItemsSource = EmulationOptionCatalog.Choices(("auto", "Visual.Automatic"), ("PAL", "PAL"), ("NTSC", "NTSC"), ("1:1", "1:1"));
        _cropVideo.ItemsSource = EmulationOptionCatalog.Choices(("disabled", "Emulation.Disabled"), ("minimum", "Emulation.Minimum"), ("smaller", "Emulation.VerySmall"), ("small", "Emulation.Small"), ("medium", "Emulation.Medium"), ("large", "Emulation.Large"), ("larger", "Emulation.VeryLarge"), ("maximum", "Emulation.Maximum"), ("auto", "Visual.Automatic"));
        _videoLineMode.ItemsSource = EmulationOptionCatalog.Choices(("auto", "Visual.Automatic"), ("single", "Emulation.LineModeSingle"), ("double", "Emulation.LineModeDouble"));
        _videoHzChange.ItemsSource = EmulationOptionCatalog.Choices(("disabled", "Emulation.Disabled"), ("enabled", "Emulation.Enabled"), ("locked", "Emulation.Locked"));
        _videoFrameskip.ItemsSource = EmulationOptionCatalog.Choices(("disabled", "Emulation.Disabled"), ("1", "1"), ("2", "2"));
        _videoColors.ItemsSource = new[] { new OptionChoice("16bit", "16 bits"), new OptionChoice("24bit", "24 bits") };
        _videoGamma.ItemsSource = Enumerable.Range(-5, 11).Select(value => new OptionChoice((value * 100).ToString(), value.ToString())).ToArray();
        _videoRenderer.ItemsSource = EmulationOptionCatalog.VideoRenderers();
        _videoRenderer.DisplayMemberPath = nameof(RendererChoice.Label);
        _videoRenderer.SelectedIndex = 0;
        _videoRenderer.MaxDropDownHeight = 132;
        _immediateBlits.ItemsSource = EmulationOptionCatalog.Choices(("false", "Emulation.Disabled"), ("immediate", "Emulation.Immediate"), ("waiting", "Emulation.Waiting"));
        _collisionLevel.ItemsSource = EmulationOptionCatalog.Choices(("none", "HostTools.None"), ("sprites", "Emulation.CollisionSprites"), ("playfields", "Emulation.CollisionPlayfields"), ("full", "Emulation.CollisionFull"));
        _audioInterpolation.ItemsSource = EmulationOptionCatalog.Choices(("none", "HostTools.None"), ("anti", "Emulation.InterpolationAnti"), ("sinc", "Sinc"), ("rh", "RH"), ("crux", "Crux"));
        _audioFilter.ItemsSource = EmulationOptionCatalog.Choices(("emulated", "Emulation.FilterEmulated"), ("off", "Emulation.Disabled"), ("on", "Emulation.Enabled"));
        _audioFilterType.ItemsSource = EmulationOptionCatalog.Choices(("auto", "Visual.Automatic"), ("standard", "Emulation.Standard"), ("enhanced", "Emulation.Enhanced"));
        _audioLatency.ItemsSource = new[] { 20, 35, 50, 75, 100, 150, 250 }
            .Select(value => new OptionChoice(value.ToString(), $"{value} ms")).ToArray();
        _floppySoundType.ItemsSource = new[] { new OptionChoice("internal", LocExtension.Get("Emulation.Internal")), new OptionChoice("A500", "A500"), new OptionChoice("LOUD", LocExtension.Get("Emulation.Loud")) };
        _floppySpeed.ItemsSource = new[] { 100, 200, 400, 800, 0 }.Select(value => new OptionChoice(value.ToString(), value == 0 ? LocExtension.Get("Emulation.Maximum") : $"{value} %")).ToArray();
        _cdSpeed.ItemsSource = new[] { new OptionChoice("100", "1×"), new OptionChoice("0", LocExtension.Get("Emulation.Maximum")) };
        _analogMouse.ItemsSource = EmulationOptionCatalog.Choices(("disabled", "Emulation.Disabled"), ("left", "Emulation.LeftStick"), ("right", "Emulation.RightStick"), ("both", "Emulation.BothSticks"));
        _analogMouseDeadzone.ItemsSource = Enumerable.Range(0, 11).Select(value => value * 5).Select(value => new OptionChoice(value.ToString(), $"{value} %")).ToArray();
        _analogMouseSpeed.ItemsSource = Enumerable.Range(1, 30).Select(value => value / 10d).Select(value => new OptionChoice(value.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture), $"{value:0.0}×")).ToArray();
        _analogMouseSpeedRight.ItemsSource = _analogMouseSpeed.ItemsSource;
        _turboPulse.ItemsSource = new[] { "2", "4", "6", "8", "10", "12" };
        _joyPortOrder.ItemsSource = new[] { "1234", "2143", "3412", "4321" };
    }

    private Task AddMediaAsync()
    {
        var dialog = new OpenFileDialog
        {
            Multiselect = true,
            Filter = LocExtension.Get("Emulation.AmigaMediaFilter")
                .Replace("|*.adf;", "|*.scp;*.adf;", StringComparison.OrdinalIgnoreCase)
        };
        if (dialog.ShowDialog() != true) return Task.CompletedTask;
        foreach (var path in dialog.FileNames)
            _media.Add(new MediaItem
            {
                Path = path,
                Kind = EmulationOptionValueConverter.InferMediaKind(path),
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
        if (_model.SelectedItem is not AmigaModel model) return;
        var devices = new List<EmulationStorageDeviceItem>();
        var floppyCount = Math.Clamp(SelectedCount(_floppyDriveCount), 0, model.MaximumFloppyDrives);
        for (var index = 0; index < floppyCount; index++)
            devices.Add(new EmulationStorageDeviceItem($"DF{index}:", EmulationStorageDeviceType.Floppy,
                EmulationOptionValueConverter.FloppyModelName(_floppyDriveModels[index]), null, CanRemove: index > 0));

        var hardDisks = _media.Where(item => item.Kind == AmigaMediaKind.HardDrive).ToArray();
        var hardDriveCount = Math.Clamp(SelectedCount(_hardDriveCount), 0, model.MaximumHardDrives);
        EnsureHardDiskSlots(hardDriveCount);
        hardDisks = _media.Where(item => item.Kind == AmigaMediaKind.HardDrive).Take(hardDriveCount).ToArray();
        for (var index = 0; index < hardDriveCount; index++)
            devices.Add(new EmulationStorageDeviceItem($"DH{index}:", EmulationStorageDeviceType.HardDisk,
                "HDF", hardDisks[index].Path));

        if (_cdDrive.IsChecked == true)
            devices.Add(new EmulationStorageDeviceItem("CD0:", EmulationStorageDeviceType.CompactDisc,
                _cdDriveModel, null));
        _storageDevices.SetDevices(devices);
    }

    private void AddStorageDevice()
    {
        if (_model.SelectedItem is not AmigaModel model) return;
        var available = new List<EmulationStorageDeviceType>();
        if (SelectedCount(_floppyDriveCount) < model.MaximumFloppyDrives)
            available.Add(EmulationStorageDeviceType.Floppy);
        if (SelectedCount(_hardDriveCount) < model.MaximumHardDrives)
            available.Add(EmulationStorageDeviceType.HardDisk);
        if (model.HasCdDrive && _cdDrive.IsChecked != true)
            available.Add(EmulationStorageDeviceType.CompactDisc);
        if (available.Count == 0)
        {
            MessageBox.Show(Window.GetWindow(this), LocExtension.Get("Emulation.NoStorageDeviceAvailable"),
                LocExtension.Get("Emulation.StorageDevices"), MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var dialog = new AddStorageDeviceDialog(available) { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() != true) return;
        switch (dialog.SelectedType)
        {
            case EmulationStorageDeviceType.Floppy:
                _floppyDriveCount.SelectedItem = SelectedCount(_floppyDriveCount) + 1;
                break;
            case EmulationStorageDeviceType.HardDisk:
                _hardDriveCount.SelectedItem = SelectedCount(_hardDriveCount) + 1;
                EnsureHardDiskSlots(SelectedCount(_hardDriveCount));
                break;
            case EmulationStorageDeviceType.CompactDisc:
                _cdDrive.IsChecked = true;
                break;
        }
        RefreshMediaRows();
    }

    private void ConfigureStorageDevice(EmulationStorageDeviceItem device)
    {
        if (_model.SelectedItem is not AmigaModel model) return;
        var index = EmulationOptionValueConverter.DeviceIndex(device.Identifier);
        switch (device.Type)
        {
            case EmulationStorageDeviceType.Floppy:
            {
                var dialog = new FloppyDriveConfigurationDialog(device.Identifier, model.DisplayName,
                    new FloppyDriveSettings(_floppyDriveModels[index], SelectedText(_floppySpeed),
                        _floppyWriteProtection.IsChecked == true, _floppyWriteRedirect.IsChecked == true))
                    { Owner = Window.GetWindow(this) };
                if (dialog.ShowDialog() != true) return;
                _floppyDriveModels[index] = dialog.Settings.Model;
                SelectValue(_floppySpeed, dialog.Settings.Speed);
                _floppyWriteProtection.IsChecked = dialog.Settings.WriteProtected;
                _floppyWriteRedirect.IsChecked = dialog.Settings.RedirectWrites;
                break;
            }
            case EmulationStorageDeviceType.HardDisk:
            {
                EnsureHardDiskSlots(SelectedCount(_hardDriveCount));
                var hardDisks = _media.Where(item => item.Kind == AmigaMediaKind.HardDrive).ToArray();
                var item = hardDisks[index];
                var dialog = new HardDiskDriveConfigurationDialog(device.Identifier, model.DisplayName, item.Path)
                    { Owner = Window.GetWindow(this) };
                if (dialog.ShowDialog() != true) return;
                item.Path = dialog.SupportPath ?? string.Empty;
                item.Label = string.IsNullOrWhiteSpace(item.Path) ? string.Empty : Path.GetFileNameWithoutExtension(item.Path);
                break;
            }
            case EmulationStorageDeviceType.CompactDisc:
            {
                var dialog = new CompactDiscDriveConfigurationDialog(device.Identifier, model.DisplayName,
                    new CompactDiscDriveSettings(_cdDriveModel, SelectedText(_cdSpeed)), supportsWriter: false)
                    { Owner = Window.GetWindow(this) };
                if (dialog.ShowDialog() != true) return;
                _cdDriveModel = dialog.Settings.Model;
                SelectValue(_cdSpeed, dialog.Settings.Speed);
                break;
            }
        }
        RefreshMediaRows();
    }

    private void RemoveStorageDevice(EmulationStorageDeviceItem device)
    {
        var index = EmulationOptionValueConverter.DeviceIndex(device.Identifier);
        switch (device.Type)
        {
            case EmulationStorageDeviceType.Floppy when index > 0:
                _floppyDriveCount.SelectedItem = Math.Max(1, SelectedCount(_floppyDriveCount) - 1);
                break;
            case EmulationStorageDeviceType.HardDisk:
            {
                var item = _media.Where(media => media.Kind == AmigaMediaKind.HardDrive).ElementAtOrDefault(index);
                if (item is not null) _media.Remove(item);
                _hardDriveCount.SelectedItem = Math.Max(0, SelectedCount(_hardDriveCount) - 1);
                break;
            }
            case EmulationStorageDeviceType.CompactDisc:
                _cdDrive.IsChecked = false;
                break;
        }
        RefreshMediaRows();
    }

    private void EnsureHardDiskSlots(int count)
    {
        var hardDisks = _media.Where(item => item.Kind == AmigaMediaKind.HardDrive).ToList();
        while (hardDisks.Count < count)
        {
            var item = new MediaItem { Kind = AmigaMediaKind.HardDrive };
            _media.Add(item);
            hardDisks.Add(item);
        }
        while (hardDisks.Count > count)
        {
            _media.Remove(hardDisks[^1]);
            hardDisks.RemoveAt(hardDisks.Count - 1);
        }
    }

    private static int SelectedCount(ComboBox comboBox) => comboBox.SelectedItem is int value ? value : 0;

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
            Padding = new Thickness(14, 9, 14, 9),
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch
        };
        tab.SetResourceReference(StyleProperty, "MainTabItemStyle");
        tabs.Items.Add(tab);
    }

    private static Grid ThreeColumnPage(Border left, Border center, Border right)
    {
        var grid = new Grid { Margin = new Thickness(12) };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
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

    private static Border FullWidthActionCard(UIElement content, string title, int row)
    {
        var card = ActionCard(content, title);
        card.Margin = new Thickness(0, 10, 0, 0);
        Grid.SetRow(card, row);
        Grid.SetColumnSpan(card, 2);
        return card;
    }

    private static Grid TileGrid(int columns, params FrameworkElement[] fields)
    {
        var grid = new Grid { Margin = new Thickness(14, 10, 14, 14) };
        for (var column = 0; column < columns; column++)
            grid.ColumnDefinitions.Add(new ColumnDefinition());
        var rows = (int)Math.Ceiling(fields.Length / (double)columns);
        for (var row = 0; row < rows; row++)
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        for (var index = 0; index < fields.Length; index++)
        {
            var field = fields[index];
            field.Margin = new Thickness(index % columns == 0 ? 0 : 12, 4,
                index % columns == columns - 1 ? 0 : 12, 10);
            Grid.SetRow(field, index / columns);
            Grid.SetColumn(field, index % columns);
            grid.Children.Add(field);
        }
        return grid;
    }

    private static FrameworkElement LabeledTile(string label, FrameworkElement control, int columnSpan = 1)
    {
        var panel = new StackPanel();
        panel.Children.Add(new TextBlock
        {
            Text = label,
            Margin = new Thickness(0, 0, 0, 7),
            TextWrapping = TextWrapping.NoWrap
        });
        control.HorizontalAlignment = HorizontalAlignment.Stretch;
        control.Margin = new Thickness(0);
        panel.Children.Add(control);
        Grid.SetColumnSpan(panel, columnSpan);
        return panel;
    }

    private static FrameworkElement TrailingCheckBoxTile(string label, CheckBox checkBox)
    {
        var row = new Grid { VerticalAlignment = VerticalAlignment.Bottom, MinHeight = 65 };
        row.ColumnDefinitions.Add(new ColumnDefinition());
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        row.Children.Add(new TextBlock
        {
            Text = label,
            TextWrapping = TextWrapping.NoWrap,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 22, 12, 0)
        });
        checkBox.Content = null;
        checkBox.Margin = new Thickness(8, 22, 0, 0);
        checkBox.VerticalAlignment = VerticalAlignment.Center;
        Grid.SetColumn(checkBox, 1);
        row.Children.Add(checkBox);
        return row;
    }

    private static FrameworkElement CheckBoxTile(CheckBox checkBox)
    {
        checkBox.HorizontalAlignment = HorizontalAlignment.Left;
        checkBox.VerticalAlignment = VerticalAlignment.Center;
        checkBox.Margin = new Thickness(0, 6, 0, 6);
        return checkBox;
    }

    private static FrameworkElement PercentageSliderTile(string label, Slider slider)
    {
        var panel = new StackPanel();
        panel.Children.Add(new TextBlock
        {
            Text = label,
            Margin = new Thickness(0, 0, 0, 7),
            TextWrapping = TextWrapping.NoWrap
        });
        var row = new Grid();
        row.ColumnDefinitions.Add(new ColumnDefinition());
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        slider.Margin = new Thickness(0, 0, 12, 0);
        slider.VerticalAlignment = VerticalAlignment.Center;
        row.Children.Add(slider);
        var value = new TextBlock
        {
            MinWidth = 48,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        void RefreshValue() => value.Text = $"{slider.Value:0} %";
        slider.ValueChanged += (_, _) => RefreshValue();
        RefreshValue();
        Grid.SetColumn(value, 1);
        row.Children.Add(value);
        panel.Children.Add(row);
        return panel;
    }

    private Border FullWidthMemorySummary(int row)
    {
        var content = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(14, 10, 14, 10)
        };
        var icon = new TextBlock
        {
            Text = "\uE964",
            FontFamily = ControlVisualConstants.IconFont,
            FontSize = 22,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 12, 0)
        };
        icon.SetResourceReference(ForegroundProperty, "AccentBrush");
        content.Children.Add(icon);
        content.Children.Add(_totalMemory);
        var card = Card(content);
        card.Margin = new Thickness(0, 10, 0, 0);
        Grid.SetRow(card, row);
        Grid.SetColumnSpan(card, 2);
        return card;
    }

    private static Border InformationBanner(TextBlock text)
    {
        var content = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(12, 9, 12, 9)
        };
        var icon = new TextBlock
        {
            Text = "\uE946",
            FontFamily = ControlVisualConstants.IconFont,
            FontSize = 18,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0)
        };
        icon.SetResourceReference(ForegroundProperty, "AccentBrush");
        content.Children.Add(icon);
        text.VerticalAlignment = VerticalAlignment.Center;
        text.SetResourceReference(ForegroundProperty, "MutedTextBrush");
        content.Children.Add(text);
        var banner = new Border
        {
            Child = content,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(7),
            Margin = new Thickness(12, 0, 12, 12)
        };
        banner.SetResourceReference(BackgroundProperty, "WindowBrush");
        banner.SetResourceReference(BorderBrushProperty, "BorderBrush");
        return banner;
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

    private static ScrollViewer ScrollPage(UIElement child)
    {
        var viewer = new ScrollViewer
        {
            Content = child,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            PanningMode = PanningMode.VerticalOnly
        };
        viewer.PreviewMouseWheel += (_, e) =>
        {
            if (FindNestedScrollViewer(e.OriginalSource as DependencyObject, viewer) is { ScrollableHeight: > 0 }) return;
            if (viewer.ScrollableHeight <= 0) return;
            var offset = Math.Clamp(viewer.VerticalOffset - e.Delta, 0, viewer.ScrollableHeight);
            if (Math.Abs(offset - viewer.VerticalOffset) < 0.5) return;
            viewer.ScrollToVerticalOffset(offset);
            e.Handled = true;
        };
        return viewer;
    }

    private static ScrollViewer? FindNestedScrollViewer(DependencyObject? source, ScrollViewer page)
    {
        while (source is not null && !ReferenceEquals(source, page))
        {
            if (source is ScrollViewer nested) return nested;
            source = VisualTreeHelper.GetParent(source);
        }
        return null;
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

    private static Border InputBindingCard(InputBindingEditor editor, string title, string? hint = null)
    {
        var layout = new Grid();
        layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        layout.RowDefinitions.Add(new RowDefinition());
        var heading = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(10, 6, 10, 2) };
        heading.Children.Add(new TextBlock
        {
            Text = title,
            FontWeight = FontWeights.SemiBold,
            FontSize = 16,
            VerticalAlignment = VerticalAlignment.Center
        });
        if (!string.IsNullOrWhiteSpace(hint))
        {
            heading.Children.Add(new TextBlock
            {
                Text = "\uE946",
                FontFamily = ControlVisualConstants.IconFont,
                FontSize = 15,
                Margin = new Thickness(9, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                ToolTip = hint
            });
        }
        layout.Children.Add(heading);
        Grid.SetRow(editor, 1);
        layout.Children.Add(editor);
        var card = new Border { Child = layout, Padding = new Thickness(2) };
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
            FontFamily = ControlVisualConstants.IconFont,
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
        layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
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
        ControlUiFactory.ApplyCardAppearance(card);
        return card;
    }

    private static Border ActionCard(UIElement child, string title, FrameworkElement? actions = null)
    {
        var header = new Grid
        {
            Margin = new Thickness(0),
            Height = 54
        };
        header.ColumnDefinitions.Add(new ColumnDefinition());
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        header.Children.Add(new TextBlock
        {
            Text = title,
            FontWeight = FontWeights.SemiBold,
            FontSize = 17,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(16, 0, 12, 0)
        });
        if (actions is not null)
        {
            Grid.SetColumn(actions, 1);
            actions.Margin = new Thickness(8, 8, 12, 8);
            header.Children.Add(actions);
        }

        var headerBorder = new Border
        {
            Child = header,
            BorderThickness = new Thickness(0, 0, 0, 1)
        };
        headerBorder.SetResourceReference(BorderBrushProperty, "BorderBrush");
        var body = new Border { Child = child, Padding = new Thickness(8) };
        var layout = new Grid();
        layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        layout.RowDefinitions.Add(new RowDefinition());
        layout.Children.Add(headerBorder);
        Grid.SetRow(body, 1);
        layout.Children.Add(body);

        var card = new Border
        {
            Child = layout,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(9),
            ClipToBounds = true
        };
        ControlUiFactory.ApplyCardAppearance(card);
        return card;
    }

    private void ApplyModelDefaults()
    {
        if (_model.SelectedItem is not AmigaModel model) return;
        ConfigureCpuModelChoices();
        SelectValue(_cpuModel, model.DefaultCpu);
        ConfigureFpuChoices();
        _chipset.ItemsSource = new[] { model.Chipset };
        _chipset.SelectedIndex = 0;
        ConfigureMemoryChoices(model);
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
        _floppyDriveCount.IsEnabled = model.MaximumFloppyDrives > 0;
        _hardDriveCount.IsEnabled = model.SupportsHardDrives && model.MaximumHardDrives > 0;
        _cdDrive.IsEnabled = model.HasCdDrive;
        var hasFloppyDrive = model.MaximumFloppyDrives > 0;
        _floppySpeed.IsEnabled = hasFloppyDrive;
        _floppyWriteProtection.IsEnabled = hasFloppyDrive;
        _floppyWriteRedirect.IsEnabled = hasFloppyDrive;
        _floppySound.IsEnabled = hasFloppyDrive;
        _floppySoundType.IsEnabled = hasFloppyDrive;
        _muteEmptyFloppy.IsEnabled = hasFloppyDrive;
        _cdSpeed.IsEnabled = model.HasCdDrive;
        _cdAudioVolume.IsEnabled = model.HasCdDrive;
        ConfigureControllerChoices(model);
        RefreshMouseMappings(preserveBindings: true);
        RefreshMediaRows();
        RefreshFirmwareRows();
        UpdateRomFieldAvailability();
    }

    private void ConfigureControllerChoices(AmigaModel model)
    {
        var choices = new List<LocalizedChoice<AmigaControllerType>>
        {
            new(AmigaControllerType.Automatic, LocExtension.Get("Visual.Automatic")),
            new(AmigaControllerType.Joystick, LocExtension.Get("Emulation.AmigaJoystick")),
            new(AmigaControllerType.AnalogJoystick, LocExtension.Get("Emulation.AnalogJoystick"))
        };
        if (model.SupportsCd32Controller)
            choices.Add(new(AmigaControllerType.Cd32Pad, LocExtension.Get("Emulation.Cd32Controller")));
        choices.Add(new(AmigaControllerType.None, LocExtension.Get("HostTools.None")));

        for (var port = 0; port < _controllers.Length; port++)
        {
            var modelDefault = model.SupportsCd32Controller ? AmigaControllerType.Cd32Pad : AmigaControllerType.Joystick;
            var current = _loading ? SelectedChoice(_controllers[port], modelDefault) : modelDefault;
            _controllers[port].ItemsSource = choices;
            var wanted = choices.Any(choice => choice.Value == current)
                ? current
                : modelDefault;
            SelectChoice(_controllers[port], wanted);
            RefreshControllerMappings(port, preserveBindings: true);
        }
    }

    private void ConfigureMemoryChoices(AmigaModel model)
    {
        var chipValues = EmulationOptionCatalog.ChipMemoryValues(model);
        var slowValues = EmulationOptionCatalog.SlowMemoryValues(model);
        var fastValues = EmulationOptionCatalog.FastMemoryValues();
        var z3Values = EmulationOptionCatalog.Z3MemoryValues(model);

        _chipMemory.ItemsSource = chipValues.Select(EmulationOptionCatalog.ChipMemoryChoice).ToArray();
        _slowMemory.ItemsSource = slowValues.Select(EmulationOptionCatalog.SlowMemoryChoice).ToArray();
        _fastMemory.ItemsSource = EmulationOptionCatalog.MemoryChoices(fastValues, includeAutomatic: false);
        _z3Memory.ItemsSource = EmulationOptionCatalog.MemoryChoices(z3Values, includeAutomatic: false);

        SelectValue(_chipMemory, EmulationOptionCatalog.ChipMemoryValue(model.ChipMemoryKib));
        SelectValue(_slowMemory, EmulationOptionCatalog.SlowMemoryValue(model.SlowMemoryKib));
        SelectValue(_fastMemory, fastValues.Contains(model.FastMemoryMib) ? model.FastMemoryMib.ToString() : "0");
        SelectValue(_z3Memory, "0");

        _chipMemory.IsEnabled = chipValues.Count > 1;
        _slowMemory.IsEnabled = slowValues.Count > 1;
        _fastMemory.IsEnabled = fastValues.Count > 1;
        _z3Memory.IsEnabled = z3Values.Count > 1;

        _mainMemoryHint.Text = LocExtension.Get("Emulation.MemoryCompatibleWithModel", model.DisplayName);
        _extensionMemoryHint.Text = _z3Memory.IsEnabled
            ? LocExtension.Get("Emulation.MemoryExtensionsCompatibleWithModel", model.DisplayName)
            : LocExtension.Get("Emulation.Z3MemoryUnavailableForModel", model.DisplayName);
        UpdateMemorySummary();
    }

    private void UpdateMemorySummary()
    {
        var totalKib = EmulationOptionCatalog.ChipMemoryKib(SelectedText(_chipMemory))
            + EmulationOptionCatalog.SlowMemoryKib(SelectedText(_slowMemory))
            + EmulationOptionCatalog.MemoryMib(SelectedText(_fastMemory)) * 1024
            + EmulationOptionCatalog.MemoryMib(SelectedText(_z3Memory)) * 1024;
        var totalMib = totalKib / 1024d;
        _totalMemory.Text = LocExtension.Get("Emulation.TotalMemoryConfigured",
            totalMib.ToString(totalMib % 1 == 0 ? "0" : "0.##", System.Globalization.CultureInfo.CurrentCulture),
            StorageSizeFormatter.MebibyteUnit);
    }

    private void ConfigureFpuChoices()
    {
        var cpu = SelectedText(_cpuModel);
        if (string.IsNullOrEmpty(cpu)) cpu = "68000";
        var values = EmulationOptionCatalog.FpuValues(cpu);
        var previous = SelectedText(_fpuModel);
        _fpuModel.ItemsSource = values.Select(value => new OptionChoice(value, value switch
        {
            "0" when values.Count == 1 => $"{LocExtension.Get("HostTools.None")} — {EmulationOptionCatalog.CpuDisplayName(cpu)}",
            "0" => LocExtension.Get("HostTools.None"),
            "cpu" => $"{LocExtension.Get("Emulation.IntegratedFpu")} — {EmulationOptionCatalog.CpuDisplayName(cpu)}",
            _ => $"Motorola {value}"
        })).ToArray();
        SelectValue(_fpuModel, values.Contains(previous) ? previous : EmulationOptionCatalog.DefaultFpu(cpu));
        _fpuModel.IsEnabled = values.Count > 1;
    }

    private void ConfigureCpuModelChoices()
    {
        if (_model.SelectedItem is not AmigaModel model) return;
        var previous = SelectedText(_cpuModel);
        var nominalFrequency = NominalCpuFrequencyMhz(model);
        _cpuModel.ItemsSource = model.CpuModels
            .Select(cpu => new OptionChoice(cpu, $"{EmulationOptionCatalog.CpuDisplayName(cpu)} — {FormatMhz(nominalFrequency)}"))
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
        _cpuModelHint.Text = $"{model.DisplayName} · {EmulationOptionCatalog.CpuDisplayName(cpu)} · {model.Chipset} · {frequency}";
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

    private double NominalCpuFrequencyMhz(AmigaModel model) =>
        EmulationOptionCatalog.NominalCpuFrequencyMhz(model, IsNtsc());

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

    private static bool Approximately(double left, double right) =>
        Math.Abs(left - right) < ControlTechnicalConstants.FrequencyComparisonTolerance;

    private static void SelectValue(ComboBox comboBox, string value)
        => ComboBoxSelection.SelectByValue<object>(comboBox, value,
            item => item is OptionChoice choice ? choice.Value : item.ToString());

    private static void AddButton(Panel panel, string resourceKey, Func<Task> action)
    {
        var button = new Button { Content = LocExtension.Get(resourceKey), MinWidth = 100 };
        button.Click += async (_, _) => await ButtonAsyncAction.RunAsync(button, action, error => ShowError(button, error));
        panel.Children.Add(button);
    }

    private static Task RunUiActionAsync(Button button, Func<Task> action) =>
        ButtonAsyncAction.RunAsync(button, action, error => ShowError(button, error));

    private static Button CreateActionButton(string icon, string text)
    {
        return ControlUiFactory.IconTextButton(icon, text);
    }

    private static void AddField(Grid grid, int row, string label, FrameworkElement control)
    {
        var text = new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 5, 12, 5) };
        Grid.SetRow(text, row); grid.Children.Add(text);
        control.Margin = new Thickness(0, 4, 0, 4);
        Grid.SetRow(control, row); Grid.SetColumn(control, 1); Grid.SetColumnSpan(control, 2); grid.Children.Add(control);
    }

    private static PathFieldControls AddPathField(Grid grid, int row, string label, TextBox textBox, string filter,
        string? emptyText = null)
    {
        var fieldLabel = new TextBlock
        {
            Text = label,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 5, 12, 5)
        };
        Grid.SetRow(fieldLabel, row);
        grid.Children.Add(fieldLabel);

        var editor = new Grid { Margin = new Thickness(0, 4, 0, 4) };
        textBox.Margin = new Thickness(0);
        editor.Children.Add(textBox);
        if (!string.IsNullOrWhiteSpace(emptyText))
        {
            var placeholder = new TextBlock
            {
                Text = emptyText,
                IsHitTestVisible = false,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(11, 0, 8, 0)
            };
            placeholder.SetResourceReference(ForegroundProperty, "MutedTextBrush");
            void UpdatePlaceholder() => placeholder.Visibility = string.IsNullOrEmpty(textBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
            textBox.TextChanged += (_, _) => UpdatePlaceholder();
            UpdatePlaceholder();
            editor.Children.Add(placeholder);
        }
        Grid.SetRow(editor, row);
        Grid.SetColumn(editor, 1);
        grid.Children.Add(editor);
        var browse = new Button { Content = LocExtension.Get("Common.Browse"), MinWidth = 100 };
        browse.Click += (_, _) =>
        {
            var dialog = new OpenFileDialog { Filter = filter, CheckFileExists = true };
            if (dialog.ShowDialog() == true) textBox.Text = dialog.FileName;
        };
        Grid.SetRow(browse, row); Grid.SetColumn(browse, 2); grid.Children.Add(browse);
        return new PathFieldControls(fieldLabel, editor, browse);
    }

    public async Task ReloadAsync()
    {
        if (_loading) return;
        _loading = true;
        try
        {
            var selectedId = _currentId;
            var loaded = await _configurationDocuments.LoadAllAsync();
            _configurations.Clear();
            foreach (var configuration in loaded) _configurations.Add(new ConfigurationItem(configuration));
            var selected = _configurations.FirstOrDefault(item => item.Configuration.Id == selectedId)
                ?? _configurations.FirstOrDefault();
            _list.SelectedItem = selected;
            if (selected is null) _ = NewConfiguration();
            else LoadEditor(selected.Configuration);
            await RefreshFirmwareAsync();
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

    private async Task RefreshFirmwareAsync()
    {
        var entries = await Task.Run(() => new AmigaFirmwareCatalog(StoragePaths.AmigaFirmwareDirectory).Scan());
        _firmware.Clear();
        foreach (var entry in entries) _firmware.Add(new FirmwareItem(entry));
        RefreshFirmwareRows();
    }

    private void RefreshFirmwareRows()
    {
        if (_firmwareList is null) return;
        var selectedPath = SelectedFirmware()?.Firmware.Path;
        _firmwareList.Items.Clear();
        foreach (var item in _firmware)
        {
            var row = new ListBoxItem
            {
                Tag = item,
                Content = BuildFirmwareRow(item),
                Padding = new Thickness(0),
                HorizontalContentAlignment = HorizontalAlignment.Stretch
            };
            _firmwareList.Items.Add(row);
            if (string.Equals(item.Firmware.Path, selectedPath, StringComparison.OrdinalIgnoreCase))
                _firmwareList.SelectedItem = row;
        }
        _useSelectedFirmware.IsEnabled = SelectedFirmware() is not null;
    }

    private UIElement BuildFirmwareRow(FirmwareItem item)
    {
        var grid = new Grid { MinHeight = 66, Margin = new Thickness(8, 2, 8, 2) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(44) });
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(145) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(185) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var icon = new TextBlock
        {
            Text = "\uE950",
            FontFamily = ControlVisualConstants.IconFont,
            FontSize = 22,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        grid.Children.Add(icon);
        var name = new TextBlock
        {
            Text = Path.GetFileName(item.Firmware.Path),
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(8, 0, 8, 0)
        };
        Grid.SetColumn(name, 1);
        grid.Children.Add(name);
        var version = new TextBlock
        {
            Text = item.Firmware.Version ?? LocExtension.Get("Common.Unknown"),
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(8, 0, 8, 0)
        };
        Grid.SetColumn(version, 2);
        grid.Children.Add(version);

        var compatibility = FirmwareCompatibilityFor(item.Firmware);
        var badge = new Border
        {
            Child = new TextBlock
            {
                Text = compatibility.Text,
                Foreground = compatibility.Foreground,
                VerticalAlignment = VerticalAlignment.Center
            },
            Background = compatibility.Background,
            BorderBrush = compatibility.Border,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10, 5, 10, 5),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 8, 0)
        };
        Grid.SetColumn(badge, 3);
        grid.Children.Add(badge);

        var use = new Button
        {
            Content = LocExtension.Get("Emulation.UseFirmware"),
            Tag = item,
            MinWidth = 90,
            Margin = new Thickness(8, 8, 0, 8)
        };
        use.Click += (_, _) => UseFirmware(item);
        Grid.SetColumn(use, 4);
        grid.Children.Add(use);
        return grid;
    }

    private FirmwareBadge FirmwareCompatibilityFor(AmigaFirmware firmware)
    {
        var model = _model.SelectedItem as AmigaModel;
        if (model is not null && firmware.CompatibleModels.Contains(model.Id, StringComparer.OrdinalIgnoreCase))
            return new FirmwareBadge(LocExtension.Get("Emulation.CompatibilityCompatible"),
                new SolidColorBrush(Color.FromRgb(31, 111, 58)), new SolidColorBrush(Color.FromRgb(231, 247, 235)),
                new SolidColorBrush(Color.FromRgb(146, 211, 159)));
        if (firmware.CompatibleModels.Count > 0)
            return new FirmwareBadge(LocExtension.Get("Emulation.FirmwarePartiallyCompatible"),
                new SolidColorBrush(Color.FromRgb(133, 85, 8)), new SolidColorBrush(Color.FromRgb(255, 246, 218)),
                new SolidColorBrush(Color.FromRgb(234, 187, 91)));
        return new FirmwareBadge(LocExtension.Get("Common.Unknown"),
            new SolidColorBrush(Color.FromRgb(78, 85, 96)), new SolidColorBrush(Color.FromRgb(239, 241, 244)),
            new SolidColorBrush(Color.FromRgb(190, 195, 204)));
    }

    private FirmwareItem? SelectedFirmware() =>
        (_firmwareList.SelectedItem as ListBoxItem)?.Tag as FirmwareItem;

    private void UseFirmware(FirmwareItem? item)
    {
        if (item is null) return;
        switch (item.Firmware.Type)
        {
            case AmigaFirmwareType.ExtendedRom: _extendedRom.Text = item.Firmware.Path; break;
            case AmigaFirmwareType.RomKey: _romKey.Text = item.Firmware.Path; break;
            default: _kickstart.Text = item.Firmware.Path; break;
        }
        UpdateRomFieldAvailability();
    }

    private void UpdateRomFieldAvailability()
    {
        var model = _model.SelectedItem as AmigaModel;
        SetPathFieldEnabled(_extendedRomField, model?.Id is "CDTV" or "CD32");
        SetPathFieldEnabled(_romKeyField,
            !string.IsNullOrWhiteSpace(_romKey.Text) || AmigaConfigurationDocuments.IsEncryptedKickstart(_kickstart.Text));
    }

    private static void SetPathFieldEnabled(PathFieldControls? field, bool enabled)
    {
        if (field is null) return;
        field.Label.IsEnabled = enabled;
        field.Editor.IsEnabled = enabled;
        field.Browse.IsEnabled = enabled;
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
        AmigaConfigurationDocuments.SelectOption(_cpuModel, configuration, "puae_cpu_model", selectedModel.DefaultCpu);
        AmigaConfigurationDocuments.SelectOption(_fpuModel, configuration, "puae_fpu_model", EmulationOptionCatalog.DefaultFpu(selectedModel.DefaultCpu));
        AmigaConfigurationDocuments.SelectOption(_cpuCompatibility, configuration, "puae_cpu_compatibility", "exact");
        ConfigureCpuFrequencyChoices();
        SelectCpuFrequency(AmigaConfigurationDocuments.GetOption(configuration, "puae_cpu_throttle", "0.0"),
            AmigaConfigurationDocuments.GetOption(configuration, "puae_cpu_multiplier", "0"));
        AmigaConfigurationDocuments.SelectOption(_chipMemory, configuration, "puae_chipmem_size", Math.Clamp(((_model.SelectedItem as AmigaModel)?.ChipMemoryKib ?? 512) / 512, 1, 4).ToString());
        AmigaConfigurationDocuments.SelectOption(_slowMemory, configuration, "puae_bogomem_size", ((_model.SelectedItem as AmigaModel)?.SlowMemoryKib ?? 0) == 0 ? "0" : "2");
        AmigaConfigurationDocuments.SelectOption(_fastMemory, configuration, "puae_fastmem_size", ((_model.SelectedItem as AmigaModel)?.FastMemoryMib ?? 0).ToString());
        AmigaConfigurationDocuments.SelectOption(_z3Memory, configuration, "puae_z3mem_size", "0");
        AmigaConfigurationDocuments.SelectOption(_videoStandard, configuration, "puae_video_standard", "PAL");
        AmigaConfigurationDocuments.SelectOption(_videoResolution, configuration, "puae_video_resolution", "auto");
        AmigaConfigurationDocuments.SelectOption(_videoAspect, configuration, "puae_video_aspect", "auto");
        AmigaConfigurationDocuments.SelectOption(_cropVideo, configuration, "puae_crop", "disabled");
        AmigaConfigurationDocuments.SelectOption(_videoLineMode, configuration, "puae_video_vresolution", "auto");
        AmigaConfigurationDocuments.SelectOption(_videoHzChange, configuration, "puae_video_allow_hz_change", "locked");
        AmigaConfigurationDocuments.SelectOption(_videoFrameskip, configuration, "puae_gfx_framerate", "disabled");
        AmigaConfigurationDocuments.SelectOption(_videoColors, configuration, "puae_gfx_colors", "24bit");
        AmigaConfigurationDocuments.SelectOption(_videoGamma, configuration, "puae_gfx_gamma", "0");
        _videoRenderer.SelectedItem = _videoRenderer.Items.OfType<RendererChoice>()
            .FirstOrDefault(item => item.Renderer == configuration.VideoRenderer)
            ?? _videoRenderer.Items.OfType<RendererChoice>().First();
        AmigaConfigurationDocuments.SelectOption(_immediateBlits, configuration, "puae_immediate_blits", "waiting");
        AmigaConfigurationDocuments.SelectOption(_collisionLevel, configuration, "puae_collision_level", "playfields");
        _flickerFixer.IsChecked = AmigaConfigurationDocuments.GetOption(configuration, "puae_gfx_flickerfixer", "disabled") == "enabled";
        var audio = configuration.Audio ?? new AmigaAudioConfiguration();
        var outputs = WasapiAudioOutput.GetOutputDevices();
        _audioOutput.ItemsSource = new[] { new AudioOutputDevice(string.Empty, LocExtension.Get("Emulation.DefaultAudioOutput")) }.Concat(outputs).ToArray();
        _audioOutput.SelectedItem = _audioOutput.Items.OfType<AudioOutputDevice>().FirstOrDefault(device => device.Id == audio.OutputDeviceId)
            ?? _audioOutput.Items.OfType<AudioOutputDevice>().FirstOrDefault();
        SelectValue(_audioLatency, audio.LatencyMilliseconds.ToString());
        AmigaConfigurationDocuments.SelectOption(_audioInterpolation, configuration, "puae_sound_interpol", audio.Interpolation);
        AmigaConfigurationDocuments.SelectOption(_audioFilter, configuration, "puae_sound_filter", audio.Filter);
        AmigaConfigurationDocuments.SelectOption(_audioFilterType, configuration, "puae_sound_filter_type", "auto");
        _floppySound.Value = EmulationOptionValueConverter.ParsePercentage(AmigaConfigurationDocuments.GetOption(configuration, "puae_floppy_sound", "80"), 80);
        AmigaConfigurationDocuments.SelectOption(_floppySoundType, configuration, "puae_floppy_sound_type", "internal");
        _muteEmptyFloppy.IsChecked = AmigaConfigurationDocuments.GetOption(configuration, "puae_floppy_sound_empty_mute", "enabled") == "enabled";
        _cdAudioVolume.Value = EmulationOptionValueConverter.ParsePercentage(AmigaConfigurationDocuments.GetOption(configuration, "puae_sound_volume_cd", "100%"), 100);
        _stereoSeparation.Value = int.TryParse(AmigaConfigurationDocuments.GetOption(configuration, "puae_sound_stereo_separation", $"{audio.StereoSeparation}%").TrimEnd('%'), out var separation) ? separation : 100;
        _media.Clear();
        var media = configuration.Media ?? configuration.Floppies?.Select(floppy => new AmigaMediaConfiguration(
            floppy.Path, AmigaMediaKind.Floppy, floppy.Label, floppy.IsReadOnly)).ToArray()
            ?? (configuration.InitialDiskPath is null ? [] : [new AmigaMediaConfiguration(configuration.InitialDiskPath, EmulationOptionValueConverter.InferMediaKind(configuration.InitialDiskPath))]);
        foreach (var item in media.Where(item => item.Kind == AmigaMediaKind.HardDrive))
            _media.Add(new MediaItem { Path = item.Path, Kind = item.Kind, Label = item.Label ?? string.Empty });
        _floppyDriveCount.SelectedItem = Math.Clamp(
            int.TryParse(AmigaConfigurationDocuments.GetOption(configuration, "gwgui_floppy_drive_count", "1"), out var floppyCount) ? floppyCount : 1,
            0, selectedModel.MaximumFloppyDrives);
        _hardDriveCount.SelectedItem = Math.Clamp(
            int.TryParse(AmigaConfigurationDocuments.GetOption(configuration, "gwgui_hard_drive_count", media.Count(item => item.Kind == AmigaMediaKind.HardDrive).ToString()), out var hardCount) ? hardCount : 0,
            0, selectedModel.MaximumHardDrives);
        _cdDrive.IsChecked = AmigaConfigurationDocuments.GetOption(configuration, "gwgui_cd_drive_enabled", selectedModel.HasCdDrive ? "enabled" : "disabled") == "enabled";
        _multiDrive.IsChecked = configuration.MountFloppiesInSeparateDrives;
        AmigaConfigurationDocuments.SelectOption(_floppySpeed, configuration, "puae_floppy_speed", "100");
        _floppyWriteProtection.IsChecked = AmigaConfigurationDocuments.GetOption(configuration, "puae_floppy_write_protection", "disabled") == "enabled";
        _floppyWriteRedirect.IsChecked = AmigaConfigurationDocuments.GetOption(configuration, "puae_floppy_write_redirect", "disabled") == "enabled";
        for (var index = 0; index < _floppyDriveModels.Length; index++)
            _floppyDriveModels[index] = AmigaConfigurationDocuments.GetOption(configuration, $"gwgui_floppy_drive_model_{index}", "35dd");
        AmigaConfigurationDocuments.SelectOption(_cdSpeed, configuration, "puae_cd_speed", "100");
        _cdDriveModel = AmigaConfigurationDocuments.GetOption(configuration, "gwgui_cd_drive_model", "CD-ROM");
        var keyboardBindings = configuration.Input?.KeyboardBindings;
        _amigaKeyboardEditor.SetRows(AmigaSpecialKeyDefinitions(), keyboardBindings);
        _amigaKeyboardEditor.SetReservedBindings(_appSettings is null
            ? Array.Empty<string>()
            : _appSettings.EmulationShortcuts.Values);
        _mouseDevice.Text = configuration.Input?.MouseDeviceId ?? string.Empty;
        _releaseMouseKey.SelectedItem = configuration.Input?.ReleaseMouseKey ?? GWGUI.Emulation.EmulationKey.Escape;
        _mouseSpeedRatio.Text = EmulationOptionValueConverter.MouseSpeedRatioText(AmigaConfigurationDocuments.GetOption(configuration, "puae_mouse_speed", "100"));
        AmigaConfigurationDocuments.SelectOption(_analogMouse, configuration, "puae_analogmouse", "both");
        AmigaConfigurationDocuments.SelectOption(_analogMouseDeadzone, configuration, "puae_analogmouse_deadzone", "20");
        AmigaConfigurationDocuments.SelectOption(_analogMouseSpeed, configuration, "puae_analogmouse_speed", "1.0");
        AmigaConfigurationDocuments.SelectOption(_analogMouseSpeedRight, configuration, "puae_analogmouse_speed_right", "1.0");
        _keyboardPassThrough.IsChecked = true;
        AmigaConfigurationDocuments.SelectOption(_turboPulse, configuration, "puae_turbo_pulse", "6");
        AmigaConfigurationDocuments.SelectOption(_joyPortOrder, configuration, "puae_joyport_order", "1234");
        var mouseMappings = configuration.Input?.MouseButtonMappings;
        var mouseValues = Enum.GetValues<AmigaMouseAction>().ToDictionary(
            action => action.ToString(),
            action => mouseMappings?.FirstOrDefault(item => item.Value == action).Key ?? action switch
            {
                AmigaMouseAction.LeftButton => "Mouse:Left",
                AmigaMouseAction.RightButton => "Mouse:Right",
                _ => "Mouse:Middle"
            }, StringComparer.OrdinalIgnoreCase);
        _amigaMouseEditor.SetRows(AmigaMouseDefinitions(selectedModel), mouseValues);
        _amigaMouseEditor.SetReservedBindings(_appSettings is null
            ? Array.Empty<string>()
            : _appSettings.EmulationShortcuts.Values);
        for (var port = 0; port < _controllerDevices.Length; port++)
        {
            var binding = configuration.Input?.ControllerBindings?.FirstOrDefault(item => item.Port == port);
            RefreshControllerMappings(port, preserveBindings: false, binding?.ButtonMappings);
            _controllerEditors[port].SetReservedBindings(_appSettings is null
                ? Array.Empty<string>()
                : _appSettings.EmulationShortcuts.Values);
            _controllerDevices[port].Tag = binding?.DeviceId;
        }
        _ = DetectControllersAsync();
        RefreshMediaRows();
        UpdateRomFieldAvailability();
    }

    private async Task SaveConfigurationAsync()
    {
        if (_model.SelectedItem is not AmigaModel model) throw new InvalidOperationException(LocExtension.Get("Emulation.ModelRequired"));
        if (string.IsNullOrWhiteSpace(_kickstart.Text)) throw new InvalidOperationException(LocExtension.Get("Emulation.KickstartRequired"));
        var supportsExtendedRom = model.Id is "CDTV" or "CD32";
        var requiresRomKey = AmigaConfigurationDocuments.IsEncryptedKickstart(_kickstart.Text);
        var extendedRomPath = supportsExtendedRom ? AmigaConfigurationDocuments.OptionalFullPath(_extendedRom.Text) : null;
        var romKeyPath = requiresRomKey ? AmigaConfigurationDocuments.OptionalFullPath(_romKey.Text) : null;
        AmigaConfigurationDocuments.ValidateOptionalFile(_kickstart.Text, required: true);
        if (supportsExtendedRom) AmigaConfigurationDocuments.ValidateOptionalFile(_extendedRom.Text);
        if (requiresRomKey) AmigaConfigurationDocuments.ValidateOptionalFile(_romKey.Text);
        if (_amigaKeyboardEditor.HasErrors || _amigaMouseEditor.HasErrors || _controllerEditors.Any(editor => editor.HasErrors))
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
        options["puae_floppy_sound"] = $"{(int)_floppySound.Value}";
        options["puae_floppy_sound_type"] = SelectedText(_floppySoundType);
        options["puae_floppy_sound_empty_mute"] = _muteEmptyFloppy.IsChecked == true ? "enabled" : "disabled";
        options["puae_sound_volume_cd"] = $"{(int)_cdAudioVolume.Value}%";
        options["puae_sound_stereo_separation"] = $"{(int)_stereoSeparation.Value}%";
        options["puae_floppy_speed"] = SelectedText(_floppySpeed);
        options["puae_floppy_write_protection"] = _floppyWriteProtection.IsChecked == true ? "enabled" : "disabled";
        options["puae_floppy_write_redirect"] = _floppyWriteRedirect.IsChecked == true ? "enabled" : "disabled";
        options["puae_cd_speed"] = SelectedText(_cdSpeed);
        options["gwgui_floppy_drive_count"] = SelectedCount(_floppyDriveCount).ToString();
        options["gwgui_hard_drive_count"] = SelectedCount(_hardDriveCount).ToString();
        options["gwgui_cd_drive_enabled"] = _cdDrive.IsChecked == true ? "enabled" : "disabled";
        for (var index = 0; index < _floppyDriveModels.Length; index++)
            options[$"gwgui_floppy_drive_model_{index}"] = _floppyDriveModels[index];
        options["gwgui_cd_drive_model"] = _cdDriveModel;
        options["puae_physical_keyboard_pass_through"] = "enabled";
        options["puae_physicalmouse"] = "enabled";
        options["puae_mouse_speed"] = EmulationOptionValueConverter.MouseSpeedPercentage(_mouseSpeedRatio.Text).ToString(System.Globalization.CultureInfo.InvariantCulture);
        options["puae_analogmouse"] = SelectedText(_analogMouse);
        options["puae_analogmouse_deadzone"] = SelectedText(_analogMouseDeadzone);
        options["puae_analogmouse_speed"] = SelectedText(_analogMouseSpeed);
        options["puae_analogmouse_speed_right"] = SelectedText(_analogMouseSpeedRight);
        options["puae_turbo_fire"] = _controllerEditors.Any(editor =>
            editor.Rows.Any(row => row.Id == "L2" && !string.IsNullOrWhiteSpace(row.Binding))) ? "enabled" : "disabled";
        options["puae_turbo_fire_button"] = "L2";
        options["puae_turbo_pulse"] = SelectedText(_turboPulse);
        options["puae_joyport_order"] = SelectedText(_joyPortOrder);
        var media = _media.Where(item => !string.IsNullOrWhiteSpace(item.Path)).Select(item =>
        {
            AmigaConfigurationDocuments.ValidateOptionalFile(item.Path, required: true);
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
        var keyboardBindings = _amigaKeyboardEditor.Rows.Where(item => !string.IsNullOrWhiteSpace(item.Binding))
            .ToDictionary(item => item.Id, item => item.Binding.Trim(), StringComparer.OrdinalIgnoreCase);
        var keyboard = keyboardBindings
            .Where(item => Enum.TryParse<GWGUI.Emulation.EmulationKey>(item.Value, true, out _))
            .ToDictionary(item => item.Key,
                item => Enum.Parse<GWGUI.Emulation.EmulationKey>(item.Value, true), StringComparer.OrdinalIgnoreCase);
        var controllerBindings = Enumerable.Range(0, 4).Select(port =>
        {
            var mappings = _controllerEditors[port].Rows.Where(item => !string.IsNullOrWhiteSpace(item.Binding))
                .ToDictionary(item => item.Binding.Trim(), item => item.Id, StringComparer.OrdinalIgnoreCase);
            return new AmigaControllerBinding(port,
                SelectedChoice(_controllers[port], AmigaControllerType.Automatic),
                (_controllerDevices[port].SelectedItem as GameControllerDevice)?.Id,
                mappings);
        }).ToArray();
        var mouseMappings = _amigaMouseEditor.Rows.Where(item => !string.IsNullOrWhiteSpace(item.Binding))
            .ToDictionary(item => item.Binding.Trim(), item => Enum.Parse<AmigaMouseAction>(item.Id),
                StringComparer.OrdinalIgnoreCase);
        var input = new AmigaInputConfiguration(keyboard,
            string.IsNullOrWhiteSpace(_mouseDevice.Text) ? null : _mouseDevice.Text.Trim(),
            true, controllerBindings, mouseMappings,
            (GWGUI.Emulation.EmulationKey)(_releaseMouseKey.SelectedItem ?? GWGUI.Emulation.EmulationKey.Escape),
            keyboardBindings);
        var selectedOutput = _audioOutput.SelectedItem as AudioOutputDevice;
        var audio = new AmigaAudioConfiguration(string.IsNullOrWhiteSpace(selectedOutput?.Id) ? null : selectedOutput.Id,
            int.TryParse(SelectedText(_audioLatency), out var latency) ? latency : 50,
            SelectedText(_audioInterpolation), SelectedText(_audioFilter),
            (int)_stereoSeparation.Value);
        var configuration = new AmigaMachineConfiguration(model.Id, Path.GetFullPath(_kickstart.Text),
            initialPath, extendedRomPath, romKeyPath,
            Options: options, Id: _currentId == Guid.Empty ? Guid.NewGuid() : _currentId,
            AudioEnabled: _audio.IsChecked == true,
            Controllers: _controllers.Select(combo => SelectedChoice(combo, AmigaControllerType.Automatic)).ToArray(),
            Input: input,
            Floppies: floppies.Length == 0 ? null : floppies,
            MountFloppiesInSeparateDrives: floppies.Length > 1 && _multiDrive.IsChecked == true,
            Media: media.Length == 0 ? null : media,
            Audio: audio,
            VideoRenderer: _videoRenderer.SelectedItem is RendererChoice renderer
                ? renderer.Renderer : GWGUI.Emulation.EmulationVideoRenderer.Direct3D11);
        await _configurationDocuments.SaveAsync(configuration);
        _currentId = configuration.Id;
        ConfigurationSaved?.Invoke(this, configuration);
        await ReloadAsync();
    }

    private static string SelectedText(ComboBox comboBox) =>
        ComboBoxSelection.SelectedValue<OptionChoice>(comboBox, choice => choice.Value);

    private static T SelectedChoice<T>(ComboBox comboBox, T fallback) where T : struct, Enum =>
        comboBox.SelectedItem is LocalizedChoice<T> choice ? choice.Value : fallback;

    private static void SelectChoice<T>(ComboBox comboBox, T value) where T : struct, Enum =>
        comboBox.SelectedItem = comboBox.Items.OfType<LocalizedChoice<T>>().FirstOrDefault(choice =>
            EqualityComparer<T>.Default.Equals(choice.Value, value));

    private async Task DeleteConfigurationAsync()
    {
        if (_currentId == Guid.Empty || _list.SelectedItem is null) return;
        if (MessageBox.Show(Window.GetWindow(this), LocExtension.Get("Emulation.DeleteConfirm"), ControlVisualConstants.AmigaTitle,
            MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        _configurationDocuments.Delete(_currentId);
        _currentId = Guid.Empty;
        await ReloadAsync();
    }

    private Task OpenFirmwareFolder()
    {
        Directory.CreateDirectory(StoragePaths.AmigaFirmwareDirectory);
        Process.Start(new ProcessStartInfo(StoragePaths.AmigaFirmwareDirectory) { UseShellExecute = true });
        return Task.CompletedTask;
    }

    private static void ShowError(FrameworkElement owner, Exception error)
        => ControlErrorPresenter.ShowUnexpected(owner, error,
            ControlErrorContexts.AmigaConfiguration, ControlVisualConstants.AmigaTitle);

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

    private sealed record FirmwareBadge(string Text, Brush Foreground, Brush Background, Brush Border);
    private sealed record PathFieldControls(TextBlock Label, Grid Editor, Button Browse);

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

    private sealed record LocalizedChoice<T>(T Value, string Text) where T : struct, Enum
    {
        public override string ToString() => Text;
    }

}
