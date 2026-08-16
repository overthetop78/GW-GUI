using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Localization;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;
using Microsoft.Win32;

namespace GWGUI.App.Controls;

internal sealed class AtariStorageSettingsSection
{
    private readonly ListBox _devices = new() { DisplayMemberPath = nameof(AtariStorageDeviceItem.DisplayName) };
    private readonly ComboBox _type = new() { DisplayMemberPath = nameof(AtariStorageTypeChoice.DisplayName) };
    private readonly ComboBox _slot = new() { DisplayMemberPath = nameof(AtariStorageSlotChoice.DisplayName) };
    private readonly ComboBox _bus = new() { DisplayMemberPath = nameof(AtariStorageBusChoice.DisplayName) };
    private readonly TextBox _path = new();
    private readonly Button _save = new();
    private AtariMachineConfiguration? _configuration;
    private AtariStorageView? _view;
    internal StackPanel Content { get; } = new();

    internal AtariStorageSettingsSection()
    {
        _type.SelectionChanged += (_, _) => LoadSlots();
        _devices.SelectionChanged += (_, _) => LoadSelected();
        Build();
    }

    internal void Load(AtariMachineConfiguration configuration)
    {
        _configuration = configuration;
        _view = AtariStorageSettingsFunctions.Create(configuration);
        _type.ItemsSource = _view.Types;
        _type.SelectedIndex = _view.Types.Count == AtariStorageSettingsConstants.FirstItemIndex
            ? AtariStorageSettingsConstants.NoSelectionIndex : AtariStorageSettingsConstants.FirstItemIndex;
        RefreshDevices();
    }

    internal AtariMachineConfiguration Apply(AtariMachineConfiguration configuration) =>
        _configuration is null ? configuration : new AtariMachineConfiguration(configuration.Model,
            configuration.Firmwares, _configuration.Media, configuration.Options, configuration.Input,
            configuration.Id, configuration.SchemaVersion, configuration.AudioEnabled,
            configuration.VideoRenderer, configuration.Folders);

    private void Build()
    {
        Content.Children.Add(new TextBlock { Text = LocExtension.Get(AtariStorageSettingsConstants.HintResource), TextWrapping = TextWrapping.Wrap });
        Content.Children.Add(_devices);
        Content.Children.Add(Row(AtariStorageSettingsConstants.TypeResource, _type));
        Content.Children.Add(Row(AtariStorageSettingsConstants.IdentifierResource, _slot));
        Content.Children.Add(Row(AtariStorageSettingsConstants.InterfaceResource, _bus));
        var path = new Grid();
        path.ColumnDefinitions.Add(new ColumnDefinition());
        path.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        path.Children.Add(_path);
        var browse = new Button { Content = LocExtension.Get(AtariStorageSettingsConstants.BrowseResource) };
        browse.Click += (_, _) => Browse();
        Grid.SetColumn(browse, 1);
        path.Children.Add(browse);
        Content.Children.Add(Row(AtariStorageSettingsConstants.PathResource, path));
        var actions = new WrapPanel();
        _save.Content = LocExtension.Get(AtariStorageSettingsConstants.AddResource);
        _save.Click += (_, _) => Save();
        var remove = new Button { Content = LocExtension.Get(AtariStorageSettingsConstants.RemoveResource) };
        remove.Click += (_, _) => Remove();
        actions.Children.Add(_save);
        actions.Children.Add(remove);
        Content.Children.Add(actions);
        Content.Children.Add(new TextBlock { Text = LocExtension.Get(AtariStorageSettingsConstants.RuntimeHintResource), TextWrapping = TextWrapping.Wrap });
    }

    private void LoadSlots()
    {
        if (_view is null || _type.SelectedItem is not AtariStorageTypeChoice selected) return;
        _slot.ItemsSource = _view.Slots[selected.Kind];
        _slot.SelectedIndex = AtariStorageSettingsConstants.FirstItemIndex;
        _bus.ItemsSource = _view.Buses[selected.Kind];
        _bus.SelectedIndex = _view.Buses[selected.Kind].Count == AtariStorageSettingsConstants.FirstItemIndex
            ? AtariStorageSettingsConstants.NoSelectionIndex : AtariStorageSettingsConstants.FirstItemIndex;
        _bus.IsEnabled = _view.Buses[selected.Kind].Count > AtariStorageSettingsConstants.FirstItemIndex;
    }

    private void LoadSelected()
    {
        if (_devices.SelectedItem is not AtariStorageDeviceItem selected || _view is null) return;
        _save.Content = LocExtension.Get(AtariStorageSettingsConstants.ConfigureResource);
        _type.SelectedItem = _view.Types.First(value => value.Kind == selected.Configuration.Kind);
        _slot.SelectedItem = _view.Slots[selected.Configuration.Kind]
            .First(value => value.Slot == selected.Configuration.Slot);
        _bus.SelectedItem = _view.Buses[selected.Configuration.Kind]
            .FirstOrDefault(value => value.Bus == selected.Configuration.StorageBus);
        _path.Text = selected.Configuration.Path;
    }

    private void Save()
    {
        if (_configuration is null || _type.SelectedItem is not AtariStorageTypeChoice type
            || _slot.SelectedItem is not AtariStorageSlotChoice slot) return;
        var replaced = (_devices.SelectedItem as AtariStorageDeviceItem)?.Configuration.Slot;
        _configuration = AtariStorageSettingsFunctions.AddOrReplace(_configuration,
            new AtariMediaConfiguration(_path.Text, type.Kind, slot.Slot,
                StorageBus: (_bus.SelectedItem as AtariStorageBusChoice)?.Bus), replaced);
        _view = AtariStorageSettingsFunctions.Create(_configuration);
        RefreshDevices();
    }

    private void Remove()
    {
        if (_configuration is null || _devices.SelectedItem is not AtariStorageDeviceItem selected) return;
        _configuration = AtariStorageSettingsFunctions.Remove(_configuration, selected.Configuration.Slot);
        _view = AtariStorageSettingsFunctions.Create(_configuration);
        RefreshDevices();
    }

    private void RefreshDevices()
    {
        _devices.ItemsSource = _view?.Devices;
        _devices.SelectedIndex = AtariStorageSettingsConstants.NoSelectionIndex;
        _save.Content = LocExtension.Get(AtariStorageSettingsConstants.AddResource);
    }

    private void Browse()
    {
        if (_type.SelectedItem is AtariStorageTypeChoice { Kind: AtariMediaKind.Directory })
        {
            var folder = new OpenFolderDialog();
            if (folder.ShowDialog() == true) _path.Text = folder.FolderName;
            return;
        }
        var dialog = new OpenFileDialog { Filter = AtariStorageSettingsConstants.AllFilesFilter };
        if (dialog.ShowDialog() == true) _path.Text = dialog.FileName;
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
}
