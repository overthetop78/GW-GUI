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
    private readonly Grid _firmware = new();
    private readonly StackPanel _configuredFirmware = new();
    private readonly Dictionary<string, ComboBox> _editors = new(StringComparer.Ordinal);
    private readonly Dictionary<AtariFirmwareKind, TextBox> _firmwarePaths = new();
    private readonly ListBox _firmwareList = new();
    private readonly Button _useSelectedFirmware = new() { MinWidth = 100, IsEnabled = false };
    private readonly CheckBox _fastBoot = new();
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
    private readonly TabControl _tabs;
    private AtariMachineConfiguration? _configuration;
    private AtariHardwareView? _view;
    private bool _loading;

    internal AtariHardwareSettingsSection(UIElement general)
    {
        _firmwareList.SelectionChanged += (_, _) =>
            EmulationSettingsLayout.UpdateFirmwareUseButton(_useSelectedFirmware,
                SelectedFirmware() is { } selected ? FirmwareCompatibilityFor(selected) : null);
        _useSelectedFirmware.Click += (_, _) => UseSelectedFirmware();
        _firmware.Children.Add(BuildFirmwarePage());
        _tabs = BuildTabs(general);
        Content = _tabs;
    }

    internal async Task LoadAsync(AtariMachineConfiguration configuration)
    {
        _loading = true;
        try
        {
            _configuration = configuration;
            _view = AtariHardwareSettingsFunctions.Create(configuration.Model, configuration.Options);
            _editors.Clear();
            UpdateVisibleTabs(configuration.Model);
            _videoAudio.Load(configuration);
            _storage.Load(configuration);
            _input.Load(configuration);
            _fastBoot.Content = LocExtension.Get("Emulation.Atari.FastBoot");
            _fastBoot.Visibility = configuration.Core == AtariCoreKind.Hatari
                ? Visibility.Visible : Visibility.Collapsed;
            _fastBoot.IsChecked = configuration.Options.TryGetValue("hatari_fastboot", out var fastBoot)
                && string.Equals(fastBoot, "true", StringComparison.Ordinal);
            BuildFields(_cpu, _view.Cpu);
            BuildFields(_memory, _view.Memory);
            UpdateTotalMemory();
            try { await BuildFirmwareAsync(configuration.Model, scan: false); }
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
        var values = _editors
            .Where(item => item.Value.SelectedItem is AtariHardwareChoice)
            .Select(item => KeyValuePair.Create(item.Key,
                ((AtariHardwareChoice)item.Value.SelectedItem).Value));
        var configured = AtariHardwareSettingsFunctions.ReplaceOptions(configuration, values);
        if (configured.Core == AtariCoreKind.Hatari)
            configured = AtariHardwareSettingsFunctions.ReplaceOptions(configured,
                [KeyValuePair.Create("hatari_fastboot", _fastBoot.IsChecked == true ? "true" : "false")]);
        configured = ReplaceFirmwares(configured);
        return _input.Apply(_storage.Apply(_videoAudio.Apply(configured)));
    }

    private TabControl BuildTabs(UIElement general)
    {
        return EmulationMachineTabs.Create(kind => kind switch
        {
            EmulationMachineTab.General => general,
            EmulationMachineTab.Cpu => EmulationSettingsLayout.ScrollPage(_cpu),
            EmulationMachineTab.Ram => EmulationSettingsLayout.ScrollPage(_memory),
            EmulationMachineTab.Rom => _firmware,
            EmulationMachineTab.Video => _videoAudio.Video,
            EmulationMachineTab.Audio => _videoAudio.Audio,
            EmulationMachineTab.Storage => EmulationSettingsLayout.StorageSettingsPage(
                _storage.DeviceList, _storage.EmulatorOptions),
            EmulationMachineTab.Keyboard => _input.Keyboard,
            EmulationMachineTab.Mouse => _input.Mouse,
            EmulationMachineTab.Controllers => _input.Controllers,
            _ => null
        }, LocExtension.Get(AtariAccessibilityConstants.ConfigurationTabsResource), kind =>
        {
            if (kind == EmulationMachineTab.Rom) LoadFirmwareCatalog();
            return Task.CompletedTask;
        });
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
            if (field.Availability == AtariOptionAvailability.Hidden) continue;
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
            : AtariConfigurationCatalogFunctions.ModelName(_configuration.Model);
        var cpuName = cpu.Choices.Single(choice => choice.Value == cpu.SelectedValue).DisplayName;
        var originalSpeed = speed.Choices[0].DisplayName;
        _cpu.Children.Add(EmulationSettingsLayout.CpuSettingsPage(new EmulationCpuSettingsContent(
            editors[AtariSettingOption.CpuModel],
            new TextBlock { Text = $"{modelName} · {cpuName} · {originalSpeed}" },
            editors.GetValueOrDefault(AtariSettingOption.CpuPrecision),
            editors.GetValueOrDefault(AtariSettingOption.Fpu),
            new TextBlock { Text = originalSpeed },
            speed.Availability == AtariOptionAvailability.Editable
                ? editors.GetValueOrDefault(AtariSettingOption.CpuSpeed) : null)));
    }

    private void BuildMemoryLayout(IReadOnlyDictionary<AtariSettingOption, FrameworkElement> editors,
        IReadOnlyList<AtariHardwareField> fields)
    {
        _totalMemory = new TextBlock();
        ConfigureMemoryExpansionDependencies(editors);
        var main = fields.Single(field => field.Option == AtariSettingOption.MainMemory);
        var extensions = fields.Where(field => field.Option != AtariSettingOption.MainMemory
                && field.Availability != AtariOptionAvailability.Hidden)
            .ToArray();
        var modelName = _configuration is null
            ? string.Empty
            : AtariConfigurationCatalogFunctions.ModelName(_configuration.Model);
        _memory.Children.Add(EmulationSettingsLayout.MemorySettingsPage(new EmulationMemorySettingsContent(
            [new(LocExtension.Get(main.ResourceKey), editors[main.Option])],
            new TextBlock { Text = LocExtension.Get("Emulation.Memory.CompatibleWithModel", modelName) },
            extensions.Select(field => new EmulationSettingsField(
                LocExtension.Get(field.ResourceKey), editors[field.Option])).ToArray(),
            new TextBlock
            {
                Text = LocExtension.Get("Emulation.Memory.ExtensionsCompatibleWithModel", modelName)
            },
            _totalMemory)));
    }

    private void ConfigureMemoryExpansionDependencies(
        IReadOnlyDictionary<AtariSettingOption, FrameworkElement> editors)
    {
        if (editors.GetValueOrDefault(AtariSettingOption.MosaicMemory) is not ComboBox mosaic
            || editors.GetValueOrDefault(AtariSettingOption.AxlonMemory) is not ComboBox axlon
            || editors.GetValueOrDefault(AtariSettingOption.AxlonShadow) is not ComboBox shadow)
            return;

        var changing = false;
        void Update(ComboBox? changed)
        {
            if (changing) return;
            changing = true;
            try
            {
                var mosaicEnabled = !Equals(mosaic.SelectedValue, AtariEightBitSettingsConstants.Disabled);
                var axlonEnabled = !Equals(axlon.SelectedValue, AtariEightBitSettingsConstants.Disabled);
                if (mosaicEnabled && axlonEnabled)
                {
                    if (ReferenceEquals(changed, mosaic)) axlon.SelectedValue = AtariEightBitSettingsConstants.Disabled;
                    else mosaic.SelectedValue = AtariEightBitSettingsConstants.Disabled;
                }
                shadow.Visibility = Equals(axlon.SelectedValue, AtariEightBitSettingsConstants.Disabled)
                    ? Visibility.Collapsed : Visibility.Visible;
            }
            finally { changing = false; }
            UpdateTotalMemory();
        }

        mosaic.SelectionChanged += (_, _) => Update(mosaic);
        axlon.SelectionChanged += (_, _) => Update(axlon);
        Update(null);
    }

    private void UpdateVisibleTabs(AtariMachineModel model)
    {
        var visible = AtariCompatibilityCatalog.Get(model).VisibleTabs;
        foreach (var item in _tabs.Items.OfType<TabItem>())
        {
            if (item.Tag is not EmulationMachineTab tab) continue;
            var atariTab = tab switch
            {
                EmulationMachineTab.General => AtariSettingsTab.General,
                EmulationMachineTab.Cpu => AtariSettingsTab.Cpu,
                EmulationMachineTab.Ram => AtariSettingsTab.Memory,
                EmulationMachineTab.Rom => AtariSettingsTab.Firmware,
                EmulationMachineTab.Video => AtariSettingsTab.Video,
                EmulationMachineTab.Audio => AtariSettingsTab.Audio,
                EmulationMachineTab.Storage => AtariSettingsTab.Storage,
                EmulationMachineTab.Keyboard => AtariSettingsTab.Keyboard,
                EmulationMachineTab.Mouse => AtariSettingsTab.Mouse,
                EmulationMachineTab.Controllers => AtariSettingsTab.Controllers,
                _ => throw new ArgumentOutOfRangeException(nameof(tab), tab, null)
            };
            item.Visibility = visible.Contains(atariTab) ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private async Task BuildFirmwareAsync(AtariMachineModel model, bool scan = true)
    {
        _firmwareError.Visibility = Visibility.Collapsed;
        _configuredFirmware.Children.Clear();
        _firmwarePaths.Clear();
        if (_view is null || _configuration is null) return;
        var fields = new List<(string Label, FrameworkElement Control)>();
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
            fields.Add((AtariHardwareSettingsFunctions.FirmwareKindName(kind), row));
        }
        if (model != AtariMachineModel.Atari400 &&
            AtariEightBitSettingsCatalog.SupportsOriginalComputerOptions(model))
            fields.Add((LocExtension.Get(AtariHardwareSettingsConstants.BasicEnabledResource),
                CreateFirmwareOption(AtariEightBitSettingsConstants.BasicEnabledOptionKey,
                    [AtariEightBitSettingsConstants.Disabled, AtariEightBitSettingsConstants.Enabled],
                    _configuration.Options)));
        if (fields.Count == 0)
            _configuredFirmware.Children.Add(new TextBlock
            {
                Text = LocExtension.Get("Emulation.Value.NotUsed"),
                TextWrapping = TextWrapping.Wrap
            });
        else
            _configuredFirmware.Children.Add(EmulationSettingsLayout.CompactForm(
                fields.Count > 1 ? 2 : 1, fields.ToArray()));
        if (scan)
        {
            _scannedFirmware = await new AtariFirmwareScanner(StoragePaths.AtariFirmwareDirectory).ScanAsync(model);
            FirmwareCatalogCache.Write("Atari", _scannedFirmware);
        }
        else
        {
            _scannedFirmware = FirmwareCatalogCache.Read<AtariScannedFirmware>("Atari");
        }
        _scannedFirmware = _scannedFirmware.Select(firmware => firmware with
        {
            Compatibility = AtariFirmwareScanFunctions.Classify(firmware.Definition, null, model, null)
        }).ToArray();
        RefreshFirmwareRows();
    }

    private void LoadFirmwareCatalog()
    {
        _scannedFirmware = FirmwareCatalogCache.Read<AtariScannedFirmware>("Atari");
        if (_configuration is not null)
            _scannedFirmware = _scannedFirmware.Select(firmware => firmware with
            {
                Compatibility = AtariFirmwareScanFunctions.Classify(
                    firmware.Definition, null, _configuration.Model, null)
            }).ToArray();
        RefreshFirmwareRows();
    }

    private ComboBox CreateFirmwareOption(string key, IReadOnlyList<string> values,
        IReadOnlyDictionary<string, string> options)
    {
        var choices = values.Select(value => new AtariHardwareChoice(value,
            value == AtariEightBitSettingsConstants.Disabled
                ? LocExtension.Get(AtariVideoAudioSettingsConstants.DisabledResource)
                : value == AtariEightBitSettingsConstants.Enabled
                    ? LocExtension.Get(AtariVideoAudioSettingsConstants.EnabledResource) : value)).ToArray();
        var editor = new ComboBox
        {
            ItemsSource = choices,
            DisplayMemberPath = nameof(AtariHardwareChoice.DisplayName),
            SelectedValuePath = nameof(AtariHardwareChoice.Value),
            SelectedValue = options.TryGetValue(key, out var selected) && values.Contains(selected)
                ? selected : values[0]
        };
        _editors[key] = editor;
        return editor;
    }

    private void RefreshFirmwareRows()
    {
        _firmwareList.ItemsSource = null;
        _firmwareList.Items.Clear();
        foreach (var firmware in _scannedFirmware
                     .OrderBy(firmware => EmulationSettingsLayout.FirmwareCompatibilityOrder(
                         FirmwareCompatibilityFor(firmware)))
                     .ThenBy(FirmwareDisplayName,
                         StringComparer.CurrentCultureIgnoreCase))
        {
            var row = new ListBoxItem
            {
                Tag = firmware,
                Content = EmulationSettingsLayout.FirmwareRow(
                    FirmwareDisplayName(firmware),
                    null,
                    FirmwareCompatibilityFor(firmware),
                    () => UseFirmware(firmware),
                    firmware.Path),
                Padding = new Thickness(0),
                HorizontalContentAlignment = HorizontalAlignment.Stretch
            };
            _firmwareList.Items.Add(row);
        }
        EmulationSettingsLayout.UpdateFirmwareUseButton(_useSelectedFirmware,
            SelectedFirmware() is { } selected ? FirmwareCompatibilityFor(selected) : null);
    }

    private static string FirmwareDisplayName(AtariScannedFirmware firmware)
    {
        var definition = firmware.Definition;
        if (definition?.Kind is null) return Path.GetFileName(firmware.Path);
        var role = AtariHardwareSettingsFunctions.FirmwareKindName(definition.Kind.Value);
        if (definition.Kind == AtariFirmwareKind.Tos)
        {
            if (string.IsNullOrWhiteSpace(definition.Version)) return "TOS";
            return definition.Version.StartsWith("EmuTOS", StringComparison.OrdinalIgnoreCase) ||
                   definition.Version.StartsWith("KAOS TOS", StringComparison.OrdinalIgnoreCase)
                ? definition.Version
                : $"TOS {definition.Version}";
        }
        return string.IsNullOrWhiteSpace(definition.Version) ? role : $"{role} {definition.Version}";
    }

    private EmulationFirmwareCompatibility FirmwareCompatibilityFor(AtariScannedFirmware firmware)
    {
        if (_configuration is not null
            && AtariEightBitSettingsCatalog.SupportsOriginalComputerOptions(_configuration.Model)
            && firmware.Definition is { } definition
            && !AtariEightBitSettingsCatalog.IsOriginalOsCompatible(
                definition, AtariHardwareSettingsFunctions.ClassicRegion(_configuration)))
            return EmulationFirmwareCompatibility.Incompatible;
        return firmware.Compatibility switch
        {
            AtariFirmwareCompatibility.Compatible => EmulationFirmwareCompatibility.Compatible,
            AtariFirmwareCompatibility.PartiallyCompatible => EmulationFirmwareCompatibility.PartiallyCompatible,
            AtariFirmwareCompatibility.Incompatible when firmware.Definition is not null =>
                EmulationFirmwareCompatibility.Incompatible,
            _ => EmulationFirmwareCompatibility.Unknown
        };
    }

    private UIElement BuildFirmwarePage()
    {
        var configuredFirmware = new StackPanel();
        configuredFirmware.Children.Add(_firmwareError);
        configuredFirmware.Children.Add(_configuredFirmware);
        _fastBoot.Margin = new Thickness(0, 14, 0, 0);
        configuredFirmware.Children.Add(_fastBoot);
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
        EmulationSettingsLayout.UseFirmware(FirmwareCompatibilityFor(selected), () =>
        {
            var firmware = AtariFirmwareScanFunctions.CreateSelection(selected);
            if (_firmwarePaths.TryGetValue(firmware.Kind, out var target)) target.Text = firmware.Path;
        });
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
        var selected = _editors
            .Where(item => item.Value.SelectedItem is AtariHardwareChoice)
            .ToDictionary(item => item.Key,
                item => ((AtariHardwareChoice)item.Value.SelectedItem).Value, StringComparer.Ordinal);
        var bytes = AtariHardwareSettingsFunctions.TotalMemoryBytes(selected, _view);
        var formatted = AtariHardwareSettingsFunctions.FormatMemoryTotal(bytes);
        _totalMemory.Text = LocExtension.Get(AtariHardwareSettingsConstants.TotalMemoryResource,
            formatted.Value, formatted.Unit);
    }

}
