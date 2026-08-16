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
    private readonly TextBlock _firmwareError = new()
    {
        TextWrapping = TextWrapping.Wrap,
        Visibility = Visibility.Collapsed,
        Margin = new Thickness(0, 0, 0, 10)
    };
    private IReadOnlyList<AtariScannedFirmware> _scannedFirmware = [];
    private readonly TextBlock _totalMemory = new() { Margin = new Thickness(0, 8, 0, 0) };
    private readonly Border _totalMemoryCard;
    private readonly AtariVideoAudioSettingsSection _videoAudio = new();
    private readonly AtariStorageSettingsSection _storage = new();
    private readonly AtariInputSettingsSection _input = new();
    private AtariMachineConfiguration? _configuration;
    private AtariHardwareView? _view;
    private bool _loading;

    internal AtariHardwareSettingsSection(UIElement general)
    {
        _totalMemoryCard = EmulationSettingsLayout.IconCard(_totalMemory,
            LocExtension.Get(AtariHardwareSettingsConstants.TotalMemoryResource,
                AtariHardwareSettingsConstants.NoBytes, AtariHardwareSettingsConstants.ByteSuffix.Trim()), "\uE964");
        _totalMemoryCard.Margin = new Thickness(12, 0, 12, 12);
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
            _memory.Children.Add(_totalMemoryCard);
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
        var tabs = new TabControl();
        AtariAccessibilityFunctions.Configure(tabs,
            LocExtension.Get(AtariAccessibilityConstants.ConfigurationTabsResource));
        tabs.Items.Add(AtariAccessibilityFunctions.Tab("\uE713", LocExtension.Get(AtariConfigurationCatalogConstants.GeneralResource), general));
        tabs.Items.Add(AtariAccessibilityFunctions.Tab("\uE950", AtariHardwareSettingsConstants.CpuTab, EmulationSettingsLayout.ScrollPage(_cpu)));
        tabs.Items.Add(AtariAccessibilityFunctions.Tab("\uE964", AtariHardwareSettingsConstants.RamTab, EmulationSettingsLayout.ScrollPage(_memory)));
        tabs.Items.Add(AtariAccessibilityFunctions.Tab("\uE8B7", AtariHardwareSettingsConstants.RomTab, EmulationSettingsLayout.ScrollPage(_firmware)));
        tabs.Items.Add(AtariAccessibilityFunctions.Tab("\uE7F4", LocExtension.Get(AtariVideoAudioSettingsConstants.VideoTabResource), _videoAudio.Video));
        tabs.Items.Add(AtariAccessibilityFunctions.Tab("\uE767", LocExtension.Get(AtariVideoAudioSettingsConstants.AudioTabResource), _videoAudio.Audio));
        tabs.Items.Add(AtariAccessibilityFunctions.Tab("\uEDA2", LocExtension.Get(AtariStorageSettingsConstants.StorageTabResource), StoragePage()));
        tabs.Items.Add(AtariAccessibilityFunctions.Tab("\uE765", LocExtension.Get(AtariInputSettingsConstants.KeyboardTabResource), _input.Keyboard));
        tabs.Items.Add(AtariAccessibilityFunctions.Tab("\uE962", LocExtension.Get(AtariInputSettingsConstants.MouseTabResource), _input.Mouse));
        tabs.Items.Add(AtariAccessibilityFunctions.Tab("\uE7FC", LocExtension.Get(AtariInputSettingsConstants.ControllersTabResource), _input.Controllers));
        return tabs;
    }

    private UIElement StoragePage()
    {
        var content = new StackPanel();
        content.Children.Add(_storage.Content);
        var hint = EmulationSettingsLayout.InformationBanner(LocExtension.Get("Emulation.RemovableMediaRuntimeHint"));
        content.Children.Add(hint);
        var page = new Grid { Margin = new Thickness(12) };
        page.Children.Add(EmulationSettingsLayout.ActionCard(content,
            LocExtension.Get("Emulation.StorageDevices")));
        return EmulationSettingsLayout.ScrollPage(page);
    }

    private void BuildFields(Panel panel, IReadOnlyList<AtariHardwareField> fields)
    {
        panel.Children.Clear();
        var rows = new Dictionary<AtariSettingOption, UIElement>();
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
            rows[field.Option] = AtariAccessibilityFunctions.LabeledRow(LocExtension.Get(field.ResourceKey), editor);
        }
        if (ReferenceEquals(panel, _cpu)) BuildCpuLayout(rows);
        else if (ReferenceEquals(panel, _memory)) BuildMemoryLayout(rows);
        else foreach (var row in rows.Values) panel.Children.Add(row);
    }

    private void BuildCpuLayout(IReadOnlyDictionary<AtariSettingOption, UIElement> rows)
    {
        var processor = new StackPanel();
        processor.Children.Add(rows[AtariSettingOption.CpuModel]);
        var compatibility = new StackPanel();
        compatibility.Children.Add(rows[AtariSettingOption.CpuPrecision]);
        compatibility.Children.Add(rows[AtariSettingOption.Fpu]);
        var root = EmulationSettingsLayout.TwoColumnPage(
            EmulationSettingsLayout.IconCard(processor, LocExtension.Get("Emulation.Processor"), "\uE950"),
            EmulationSettingsLayout.IconCard(compatibility,
                LocExtension.Get("Emulation.CpuCompatibility"), "\uEA18"));
        var acceleration = EmulationSettingsLayout.IconCard(rows[AtariSettingOption.CpuSpeed],
            LocExtension.Get("Emulation.Acceleration"), "\uE945");
        acceleration.Margin = new Thickness(0, 10, 0, 0);
        Grid.SetRow(acceleration, 1);
        Grid.SetColumnSpan(acceleration, 2);
        root.Children.Add(acceleration);
        _cpu.Children.Add(root);
    }

    private void BuildMemoryLayout(IReadOnlyDictionary<AtariSettingOption, UIElement> rows)
    {
        var root = EmulationSettingsLayout.TwoColumnPage(
            EmulationSettingsLayout.IconCard(rows[AtariSettingOption.MainMemory],
                LocExtension.Get("Emulation.MainMemory"), "\uE964"),
            EmulationSettingsLayout.IconCard(rows[AtariSettingOption.AlternateMemory],
                LocExtension.Get("Emulation.MemoryExtensions"), "\uE950"));
        _memory.Children.Add(root);
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
                Text = LocExtension.Get("Emulation.NotUsed"),
                TextWrapping = TextWrapping.Wrap
            });
        _scannedFirmware = await new AtariFirmwareScanner(StoragePaths.AtariFirmwareDirectory).ScanAsync(model);
        _firmwareList.ItemsSource = _scannedFirmware;
    }

    private UIElement BuildFirmwarePage()
    {
        _firmwareList.ItemTemplate = AtariFirmwarePresentation.CreateTemplate();
        _firmwareList.HorizontalContentAlignment = HorizontalAlignment.Stretch;
        var refresh = new Button { Content = LocExtension.Get("Common.Refresh") };
        refresh.Click += async (_, _) => await RefreshFirmwareAsync();
        var use = new Button
        {
            Content = LocExtension.Get("Emulation.UseFirmware"),
            Margin = new Thickness(8, 0, 0, 0)
        };
        use.Click += (_, _) => UseSelectedFirmware();
        var actions = new StackPanel { Orientation = Orientation.Horizontal };
        actions.Children.Add(refresh);
        actions.Children.Add(use);
        var layout = EmulationSettingsLayout.TwoColumnPage(
            EmulationSettingsLayout.ActionCard(_configuredFirmware,
                LocExtension.Get(AtariHardwareSettingsConstants.SystemRomResource)),
            EmulationSettingsLayout.ActionCard(_firmwareList,
                LocExtension.Get(AtariHardwareSettingsConstants.DetectedRomsResource), actions));
        var openFolder = new Button
        {
            Content = LocExtension.Get("Emulation.OpenRomFolder"),
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 10, 0, 0)
        };
        openFolder.Click += (_, _) => OpenFirmwareFolder();
        var root = new StackPanel { Margin = new Thickness(12) };
        root.Children.Add(_firmwareError);
        root.Children.Add(layout);
        root.Children.Add(openFolder);
        return root;
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

}
