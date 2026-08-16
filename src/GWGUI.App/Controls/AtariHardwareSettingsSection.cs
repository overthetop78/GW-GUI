using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Localization;
using GWGUI.Emulation.Atari;

namespace GWGUI.App.Controls;

internal sealed class AtariHardwareSettingsSection : UserControl
{
    private readonly StackPanel _cpu = new();
    private readonly StackPanel _memory = new();
    private readonly StackPanel _firmware = new();
    private readonly Dictionary<string, ComboBox> _editors = new(StringComparer.Ordinal);
    private readonly TextBlock _totalMemory = new() { Margin = new Thickness(0, 8, 0, 0) };
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
        return AtariHardwareSettingsFunctions.ReplaceOptions(configuration, values);
    }

    private UIElement BuildTabs(UIElement general)
    {
        var tabs = new TabControl();
        tabs.Items.Add(Tab(LocExtension.Get(AtariConfigurationCatalogConstants.GeneralResource), general));
        tabs.Items.Add(Tab(AtariHardwareSettingsConstants.CpuTab, _cpu));
        tabs.Items.Add(Tab(AtariHardwareSettingsConstants.RamTab, _memory));
        tabs.Items.Add(Tab(AtariHardwareSettingsConstants.RomTab, _firmware));
        return tabs;
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
            editor.SelectionChanged += (_, _) => { if (!_loading) UpdateTotalMemory(); };
            _editors[key] = editor;
            panel.Children.Add(Row(field.ResourceKey, editor));
        }
    }

    private async Task BuildFirmwareAsync(AtariMachineModel model)
    {
        _firmware.Children.Clear();
        if (_view is null) return;
        _firmware.Children.Add(Heading(AtariHardwareSettingsConstants.SystemRomResource));
        foreach (var definition in _view.Firmware)
        {
            var details = new[] { definition.ExpectedFileName, definition.Version }
                .Where(value => !string.IsNullOrWhiteSpace(value));
            _firmware.Children.Add(new TextBlock
            {
                Text = string.Join(AtariHardwareSettingsConstants.ValueSeparator, details),
                TextWrapping = TextWrapping.Wrap
            });
        }
        _firmware.Children.Add(Row(AtariHardwareSettingsConstants.RegionResource,
            RegionEditor(_view.Regions)));
        _firmware.Children.Add(Heading(AtariHardwareSettingsConstants.DetectedRomsResource));
        var scanned = await new AtariFirmwareScanner(StoragePaths.AtariFirmwareDirectory).ScanAsync(model);
        foreach (var item in scanned)
        {
            _firmware.Children.Add(new TextBlock
            {
                Text = item.Path + AtariGeneralSettingsConstants.FirmwareDetailSeparator
                    + item.Compatibility,
                TextWrapping = TextWrapping.Wrap
            });
        }
    }

    private ComboBox RegionEditor(IReadOnlyList<AtariHardwareChoice> choices)
    {
        var editor = new ComboBox
        {
            ItemsSource = choices,
            DisplayMemberPath = nameof(AtariHardwareChoice.DisplayName),
            SelectedValuePath = nameof(AtariHardwareChoice.Value),
            IsEnabled = choices.Count > AtariHardwareSettingsConstants.SingleChoiceCount
        };
        var configured = _configuration?.Options.TryGetValue(AtariHardwareSettingsConstants.RegionOptionKey,
            out var value) == true ? value : choices.FirstOrDefault()?.Value;
        editor.SelectedValue = configured;
        _editors[AtariHardwareSettingsConstants.RegionOptionKey] = editor;
        return editor;
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

    private static UIElement Row(string resource, UIElement editor)
    {
        var row = new Grid { Margin = new Thickness(0, 4, 0, 4) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
        row.ColumnDefinitions.Add(new ColumnDefinition());
        row.Children.Add(new TextBlock { Text = LocExtension.Get(resource), VerticalAlignment = VerticalAlignment.Center });
        Grid.SetColumn(editor, 1);
        row.Children.Add(editor);
        return row;
    }

    private static TextBlock Heading(string resource) => new()
    {
        Text = LocExtension.Get(resource),
        FontWeight = FontWeights.SemiBold,
        Margin = new Thickness(0, 8, 0, 5)
    };

    private static TabItem Tab(string header, UIElement content) => new()
    {
        Header = header,
        Content = new ScrollViewer
        {
            Content = new Border { Child = content, Padding = new Thickness(14) },
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        }
    };
}
