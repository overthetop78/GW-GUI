using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Localization;
using GWGUI.Emulation.Atari;
using Microsoft.Win32;

namespace GWGUI.App.Controls;

internal sealed class AtariHardwareSettingsSection : UserControl
{
    private readonly StackPanel _cpu = new();
    private readonly StackPanel _memory = new();
    private readonly StackPanel _firmware = new();
    private readonly StackPanel _configuredFirmware = new();
    private readonly Dictionary<string, ComboBox> _editors = new(StringComparer.Ordinal);
    private readonly Dictionary<AtariFirmwareKind, TextBox> _firmwarePaths = new();
    private readonly ListBox _firmwareList = new();
    private readonly Button _useSelectedFirmware = new() { MinWidth = 100, IsEnabled = false };
    private readonly TextBlock _firmwareError = new()
    {
        TextWrapping = TextWrapping.Wrap,
        Visibility = Visibility.Collapsed,
        Margin = new Thickness(0, 0, 0, 10)
    };
    private IReadOnlyList<AtariScannedFirmware> _scannedFirmware = [];
    private TextBlock _totalMemory = new();
    private readonly AtariVideoAudioSettingsSection _videoAudio = new();
    private readonly AtariStorageSettingsSection _storage = new();
    private readonly AtariInputSettingsSection _input = new();
    private AtariMachineConfiguration? _configuration;
    private AtariHardwareView? _view;
    private bool _loading;

    internal AtariHardwareSettingsSection(UIElement general)
    {
        _firmwareList.SelectionChanged += (_, _) =>
            _useSelectedFirmware.IsEnabled = SelectedFirmware() is not null;
        _useSelectedFirmware.Click += (_, _) => UseSelectedFirmware();
        _firmware.Children.Add(BuildFirmwarePage());
        Content = BuildTabs(general);
    }

    internal async Task LoadAsync(AtariMachineConfiguration configuration)
    {
        _loading = true;
        try
        {
            _configuration = configuration;
            _view = AtariHardwareSettingsFunctions.Create(configuration.Model, configuration.Options);
            _videoAudio.Load(configuration);
            _storage.Load(configuration);
            _input.Load(configuration);
            BuildFields(_cpu, _view.Cpu);
            BuildFields(_memory, _view.Memory);
            UpdateTotalMemory();
            try { await BuildFirmwareAsync(configuration.Model); }
            catch (Exception error)
            {
                _firmwareError.Text = ControlErrorPresenter.Describe(error, ControlErrorContexts.AtariConfiguration);
                _firmwareError.Visibility = Visibility.Visible;
            }
        }
        finally { _loading = false; }
    }

    internal AtariMachineConfiguration Apply(AtariMachineConfiguration configuration)
    {
        var values = _editors.Where(item => item.Value.SelectedValue is string)
            .Select(item => KeyValuePair.Create(item.Key, (string)item.Value.SelectedValue));
        var configured = AtariHardwareSettingsFunctions.ReplaceOptions(configuration, values);
        configured = ReplaceFirmwares(configured);
        return _input.Apply(_storage.Apply(_videoAudio.Apply(configured)));
    }

    private UIElement BuildTabs(UIElement general)
    {
        return EmulationMachineTabs.Create(kind => kind switch
        {
            EmulationMachineTabKind.General => general,
            EmulationMachineTabKind.Cpu => EmulationSettingsLayout.ScrollPage(_cpu),
            EmulationMachineTabKind.Ram => EmulationSettingsLayout.ScrollPage(_memory),
            EmulationMachineTabKind.Rom => EmulationSettingsLayout.ScrollPage(_firmware),
            EmulationMachineTabKind.Video => _videoAudio.Video,
            EmulationMachineTabKind.Audio => _videoAudio.Audio,
            EmulationMachineTabKind.Storage => EmulationSettingsLayout.StorageSettingsPage(_storage.DeviceList),
            EmulationMachineTabKind.Keyboard => _input.Keyboard,
            EmulationMachineTabKind.Mouse => _input.Mouse,
            EmulationMachineTabKind.Controllers => _input.Controllers,
            _ => null
        }, LocExtension.Get(AtariAccessibilityConstants.ConfigurationTabsResource));
    }

    private void BuildFields(Panel panel, IReadOnlyList<AtariHardwareField> fields)
    {
        panel.Children.Clear();
        var isCpuPanel = ReferenceEquals(panel, _cpu);
        var isMemoryPanel = ReferenceEquals(panel, _memory);
        var rows = new Dictionary<AtariSettingOption, UIElement>();
        var editors = new Dictionary<AtariSettingOption, FrameworkElement>();
        foreach (var field in fields)
        {
            var editor = new ComboBox
            {
                ItemsSource = field.Choices,
                DisplayMemberPath = nameof(AtariHardwareChoice.DisplayName),
                SelectedValuePath = nameof(AtariHardwareChoice.Value),
                SelectedValue = field.SelectedValue,
                IsEnabled = field.Availability == AtariOptionAvailability.Editable,
                ToolTip = AtariHardwareSettingsFunctions.Explanation(field)
            };
            var key = AtariHardwareSettingsFunctions.OptionKey(field.Option);
            AtariAccessibilityFunctions.Configure(editor, LocExtension.Get(field.ResourceKey),
                AtariHardwareSettingsFunctions.Explanation(field));
            editor.SelectionChanged += (_, _) => { if (!_loading) UpdateTotalMemory(); };
            _editors[key] = editor;
            editors[field.Option] = editor;
            if (!isCpuPanel && !isMemoryPanel)
                rows[field.Option] = AtariAccessibilityFunctions.LabeledRow(
                    LocExtension.Get(field.ResourceKey), editor);
        }
        if (isCpuPanel) BuildCpuLayout(editors, fields);
        else if (isMemoryPanel) BuildMemoryLayout(editors, fields);
        else foreach (var row in rows.Values) panel.Children.Add(row);
    }

    private void BuildCpuLayout(IReadOnlyDictionary<AtariSettingOption, FrameworkElement> editors,
        IReadOnlyList<AtariHardwareField> fields)
    {
        var cpu = fields.Single(field => field.Option == AtariSettingOption.CpuModel);
        var speed = fields.Single(field => field.Option == AtariSettingOption.CpuSpeed);
        var modelName = _configuration is null
            ? string.Empty
            : AtariConfigurationCatalogFunctions.Models()
                .Single(model => model.Model == _configuration.Model).DisplayName;
        var cpuName = cpu.Choices.Single(choice => choice.Value == cpu.SelectedValue).DisplayName;
        var originalSpeed = speed.Choices[0].DisplayName;
        _cpu.Children.Add(EmulationSettingsLayout.CpuSettingsPage(new EmulationCpuSettingsContent(
            editors[AtariSettingOption.CpuModel],
            new TextBlock { Text = $"{modelName} · {cpuName} · {originalSpeed}" },
            editors[AtariSettingOption.CpuPrecision],
            editors[AtariSettingOption.Fpu],
            new TextBlock { Text = originalSpeed },
            editors[AtariSettingOption.CpuSpeed])));
    }

    private void BuildMemoryLayout(IReadOnlyDictionary<AtariSettingOption, FrameworkElement> editors,
        IReadOnlyList<AtariHardwareField> fields)
    {
        _totalMemory = new TextBlock();
        var main = fields.Single(field => field.Option == AtariSettingOption.MainMemory);
        var extensions = fields.Single(field => field.Option == AtariSettingOption.AlternateMemory);
        var modelName = _configuration is null
            ? string.Empty
            : AtariConfigurationCatalogFunctions.Models()
                .Single(model => model.Model == _configuration.Model).DisplayName;
        _memory.Children.Add(EmulationSettingsLayout.MemorySettingsPage(new EmulationMemorySettingsContent(
            [new(LocExtension.Get(main.ResourceKey), editors[main.Option])],
            new TextBlock { Text = LocExtension.Get("Emulation.Memory.CompatibleWithModel", modelName) },
            [new(LocExtension.Get(extensions.ResourceKey), editors[extensions.Option])],
            new TextBlock
            {
                Text = extensions.Availability == AtariOptionAvailability.Unavailable
                    ? AtariHardwareSettingsFunctions.Explanation(extensions)
                    : LocExtension.Get("Emulation.Memory.ExtensionsCompatibleWithModel", modelName)
            },
            _totalMemory)));
    }

    private async Task BuildFirmwareAsync(AtariMachineModel model)
    {
        _firmwareError.Visibility = Visibility.Collapsed;
        _configuredFirmware.Children.Clear();
        _firmwarePaths.Clear();
        if (_view is null || _configuration is null) return;
        foreach (var group in _view.Firmware.GroupBy(definition => definition.Kind))
        {
            if (group.Key is not { } kind) continue;
            var selected = _configuration.Firmwares.FirstOrDefault(value => value.Kind == kind);
            var path = new TextBox { Text = selected?.Path ?? string.Empty };
            _firmwarePaths[kind] = path;
            var row = new Grid();
            row.ColumnDefinitions.Add(new ColumnDefinition());
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.Children.Add(path);
            var browse = new Button { Content = LocExtension.Get(AtariStorageSettingsConstants.BrowseResource) };
            browse.Margin = new Thickness(8, 0, 0, 0);
            browse.Click += (_, _) => BrowseFirmware(path);
            Grid.SetColumn(browse, 1); row.Children.Add(browse);
            _configuredFirmware.Children.Add(AtariAccessibilityFunctions.LabeledRow(
                AtariHardwareSettingsFunctions.FirmwareKindName(kind), row));
        }
        if (_configuredFirmware.Children.Count == 0)
            _configuredFirmware.Children.Add(new TextBlock
            {
                Text = LocExtension.Get("Emulation.Value.NotUsed"),
                TextWrapping = TextWrapping.Wrap
            });
        _scannedFirmware = await new AtariFirmwareScanner(StoragePaths.AtariFirmwareDirectory).ScanAsync(model);
        RefreshFirmwareRows();
    }

    private void RefreshFirmwareRows()
    {
        _firmwareList.ItemsSource = null;
        _firmwareList.Items.Clear();
        foreach (var firmware in _scannedFirmware)
        {
            var row = new ListBoxItem
            {
                Tag = firmware,
                Content = EmulationSettingsLayout.FirmwareRow(
                    Path.GetFileName(firmware.Path),
                    firmware.Definition?.Version,
                    firmware.Compatibility switch
                    {
                        AtariFirmwareCompatibility.Compatible => EmulationFirmwareCompatibility.Compatible,
                        AtariFirmwareCompatibility.PartiallyCompatible =>
                            EmulationFirmwareCompatibility.PartiallyCompatible,
                        _ => EmulationFirmwareCompatibility.Unknown
                    },
                    () => UseFirmware(firmware)),
                Padding = new Thickness(0),
                HorizontalContentAlignment = HorizontalAlignment.Stretch
            };
            _firmwareList.Items.Add(row);
        }
        _useSelectedFirmware.IsEnabled = SelectedFirmware() is not null;
    }

    private UIElement BuildFirmwarePage()
    {
        var configuredFirmware = new StackPanel();
        configuredFirmware.Children.Add(_firmwareError);
        configuredFirmware.Children.Add(_configuredFirmware);
        return EmulationSettingsLayout.FirmwareSettingsPage(new EmulationFirmwareSettingsContent(
            configuredFirmware,
            _firmwareList,
            _ => RefreshFirmwareAsync(),
            _useSelectedFirmware,
            _ => OpenFirmwareFolderAsync()));
    }

    private async Task RefreshFirmwareAsync()
    {
        if (_configuration is null) return;
        try { await BuildFirmwareAsync(_configuration.Model); }
        catch (Exception error)
        {
            _firmwareError.Text = ControlErrorPresenter.Describe(error, ControlErrorContexts.AtariConfiguration);
            _firmwareError.Visibility = Visibility.Visible;
        }
    }

    private void OpenFirmwareFolder()
    {
        try
        {
            Directory.CreateDirectory(StoragePaths.AtariFirmwareDirectory);
            Process.Start(new ProcessStartInfo(StoragePaths.AtariFirmwareDirectory) { UseShellExecute = true });
        }
        catch (Exception error)
        {
            _firmwareError.Text = ControlErrorPresenter.Describe(error, ControlErrorContexts.AtariConfiguration);
            _firmwareError.Visibility = Visibility.Visible;
        }
    }

    private Task OpenFirmwareFolderAsync()
    {
        OpenFirmwareFolder();
        return Task.CompletedTask;
    }

    private void BrowseFirmware(TextBox target)
    {
        var dialog = new OpenFileDialog { Filter = LocExtension.Get(AtariStorageSettingsConstants.MediaFilterResource) };
        if (dialog.ShowDialog() == true) target.Text = dialog.FileName;
    }

    private void UseSelectedFirmware()
    {
        if (SelectedFirmware() is not { } selected) return;
        UseFirmware(selected);
    }

    private AtariScannedFirmware? SelectedFirmware() =>
        (_firmwareList.SelectedItem as ListBoxItem)?.Tag as AtariScannedFirmware;

    private void UseFirmware(AtariScannedFirmware selected)
    {
        var firmware = AtariFirmwareScanFunctions.CreateSelection(selected);
        if (_firmwarePaths.TryGetValue(firmware.Kind, out var target)) target.Text = firmware.Path;
    }

    private AtariMachineConfiguration ReplaceFirmwares(AtariMachineConfiguration source)
    {
        var definitions = _view?.Firmware ?? [];
        var firmwares = _firmwarePaths.Where(item => !string.IsNullOrWhiteSpace(item.Value.Text))
            .Select(item => new AtariFirmwareConfiguration(item.Key, item.Value.Text,
                definitions.Any(definition => definition.Kind == item.Key && definition.RequiresExternalFile)))
            .ToArray();
        return new AtariMachineConfiguration(source.Model, firmwares, source.Media, source.Options, source.Input,
            source.Id, source.SchemaVersion, source.AudioEnabled, source.VideoRenderer, source.Folders);
    }

    private void UpdateTotalMemory()
    {
        if (_view is null) return;
        var selected = _editors.Where(item => item.Value.SelectedValue is string)
            .ToDictionary(item => item.Key, item => (string)item.Value.SelectedValue, StringComparer.Ordinal);
        var bytes = AtariHardwareSettingsFunctions.TotalMemoryBytes(selected, _view);
        var formatted = AtariHardwareSettingsFunctions.FormatMemoryTotal(bytes);
        _totalMemory.Text = LocExtension.Get(AtariHardwareSettingsConstants.TotalMemoryResource,
            formatted.Value, formatted.Unit);
    }

}
