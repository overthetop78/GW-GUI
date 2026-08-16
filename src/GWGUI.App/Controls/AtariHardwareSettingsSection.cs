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
    private readonly Dictionary<string, ComboBox> _editors = new(StringComparer.Ordinal);
    private readonly Dictionary<AtariFirmwareKind, TextBox> _firmwarePaths = new();
    private readonly ListBox _firmwareList = new();
    private IReadOnlyList<AtariScannedFirmware> _scannedFirmware = [];
    private readonly TextBlock _totalMemory = new() { Margin = new Thickness(0, 8, 0, 0) };
    private readonly AtariVideoAudioSettingsSection _videoAudio = new();
    private readonly AtariStorageSettingsSection _storage = new();
    private readonly AtariInputSettingsSection _input = new();
    private AtariMachineConfiguration? _configuration;
    private AtariHardwareView? _view;
    private bool _loading;

    internal AtariHardwareSettingsSection(UIElement general)
    {
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
            _memory.Children.Add(_totalMemory);
            UpdateTotalMemory();
            try { await BuildFirmwareAsync(configuration.Model); }
            catch (Exception error)
            {
                _firmware.Children.Clear();
                _firmware.Children.Add(new TextBlock
                {
                    Text = ControlErrorPresenter.Describe(error, ControlErrorContexts.AtariConfiguration),
                    TextWrapping = TextWrapping.Wrap
                });
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
        var tabs = new TabControl();
        AtariAccessibilityFunctions.Configure(tabs,
            LocExtension.Get(AtariAccessibilityConstants.ConfigurationTabsResource));
        tabs.Items.Add(AtariAccessibilityFunctions.Tab("\uE713", LocExtension.Get(AtariConfigurationCatalogConstants.GeneralResource), general));
        tabs.Items.Add(AtariAccessibilityFunctions.Tab("\uE950", AtariHardwareSettingsConstants.CpuTab, SettingsPage(_cpu, AtariHardwareSettingsConstants.CpuTab)));
        tabs.Items.Add(AtariAccessibilityFunctions.Tab("\uE964", AtariHardwareSettingsConstants.RamTab, SettingsPage(_memory, AtariHardwareSettingsConstants.RamTab)));
        tabs.Items.Add(AtariAccessibilityFunctions.Tab("\uE8B7", AtariHardwareSettingsConstants.RomTab, SettingsPage(_firmware, LocExtension.Get(AtariHardwareSettingsConstants.SystemRomResource))));
        tabs.Items.Add(AtariAccessibilityFunctions.Tab("\uE7F4", LocExtension.Get(AtariVideoAudioSettingsConstants.VideoTabResource), _videoAudio.Video));
        tabs.Items.Add(AtariAccessibilityFunctions.Tab("\uE767", LocExtension.Get(AtariVideoAudioSettingsConstants.AudioTabResource), _videoAudio.Audio));
        tabs.Items.Add(AtariAccessibilityFunctions.Tab("\uEDA2", LocExtension.Get(AtariStorageSettingsConstants.StorageTabResource), SettingsPage(_storage.Content, LocExtension.Get("Emulation.StorageDevices"))));
        tabs.Items.Add(AtariAccessibilityFunctions.Tab("\uE765", LocExtension.Get(AtariInputSettingsConstants.KeyboardTabResource), _input.Keyboard));
        tabs.Items.Add(AtariAccessibilityFunctions.Tab("\uE962", LocExtension.Get(AtariInputSettingsConstants.MouseTabResource), _input.Mouse));
        tabs.Items.Add(AtariAccessibilityFunctions.Tab("\uE7FC", LocExtension.Get(AtariInputSettingsConstants.ControllersTabResource), _input.Controllers));
        return tabs;
    }

    private static UIElement SettingsPage(UIElement content, string title)
    {
        var page = new Grid { Margin = new Thickness(12) };
        page.Children.Add(EmulationSettingsLayout.ActionCard(content, title));
        return EmulationSettingsLayout.ScrollPage(page);
    }

    private void BuildFields(Panel panel, IReadOnlyList<AtariHardwareField> fields)
    {
        panel.Children.Clear();
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
            panel.Children.Add(AtariAccessibilityFunctions.LabeledRow(LocExtension.Get(field.ResourceKey), editor));
        }
    }

    private async Task BuildFirmwareAsync(AtariMachineModel model)
    {
        _firmware.Children.Clear();
        _firmwarePaths.Clear();
        if (_view is null || _configuration is null) return;
        var configured = new StackPanel();
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
            configured.Children.Add(AtariAccessibilityFunctions.LabeledRow(kind.ToString(), row));
        }
        _scannedFirmware = await new AtariFirmwareScanner(StoragePaths.AtariFirmwareDirectory).ScanAsync(model);
        _firmwareList.ItemsSource = _scannedFirmware;
        _firmwareList.DisplayMemberPath = nameof(AtariScannedFirmware.Path);
        var use = new Button { Content = LocExtension.Get("Emulation.UseFirmware") };
        use.Click += (_, _) => UseSelectedFirmware();
        var layout = EmulationSettingsLayout.TwoColumnPage(
            EmulationSettingsLayout.ActionCard(configured, LocExtension.Get(AtariHardwareSettingsConstants.SystemRomResource)),
            EmulationSettingsLayout.ActionCard(_firmwareList,
                LocExtension.Get(AtariHardwareSettingsConstants.DetectedRomsResource), use));
        _firmware.Children.Add(layout);
    }

    private void BrowseFirmware(TextBox target)
    {
        var dialog = new OpenFileDialog { Filter = LocExtension.Get(AtariStorageSettingsConstants.MediaFilterResource) };
        if (dialog.ShowDialog() == true) target.Text = dialog.FileName;
    }

    private void UseSelectedFirmware()
    {
        if (_firmwareList.SelectedItem is not AtariScannedFirmware selected) return;
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
        _totalMemory.Text = LocExtension.Get(AtariHardwareSettingsConstants.TotalMemoryResource,
            bytes, AtariHardwareSettingsConstants.ByteSuffix.Trim());
    }

    private static TextBlock Heading(string resource) => new()
    {
        Text = LocExtension.Get(resource),
        FontWeight = FontWeights.SemiBold,
        Margin = new Thickness(0, 8, 0, 5)
    };

}
