using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Localization;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;
using Microsoft.Win32;

namespace GWGUI.App.Controls;

internal sealed class AtariStorageSettingsSection
{
    private readonly EmulationStorageDeviceList _deviceList = new();
    private readonly ListBox _devices = new() { DisplayMemberPath = nameof(AtariStorageDeviceItem.DisplayName) };
    private readonly ComboBox _type = new() { DisplayMemberPath = nameof(AtariStorageTypeChoice.DisplayName) };
    private readonly ComboBox _slot = new() { DisplayMemberPath = nameof(AtariStorageSlotChoice.DisplayName) };
    private readonly ComboBox _bus = new() { DisplayMemberPath = nameof(AtariStorageBusChoice.DisplayName) };
    private readonly TextBox _path = new();
    private AtariMachineConfiguration? _configuration;
    private AtariStorageView? _view;
    internal UIElement Content => _deviceList;

    internal AtariStorageSettingsSection()
    {
        _type.SelectionChanged += (_, _) => LoadSlots();
        _devices.SelectionChanged += (_, _) => LoadSelected();
        _deviceList.AddRequested += (_, _) => EditDevice(null);
        _deviceList.ConfigureRequested += (_, args) => EditDevice(args.Device.Identifier);
        _deviceList.SetCanAdd(false);
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

    private void RefreshDevices()
    {
        _devices.ItemsSource = _view?.Devices;
        _devices.SelectedIndex = AtariStorageSettingsConstants.NoSelectionIndex;
        _deviceList.SetDevices((_view?.Devices ?? []).Select(ToCommonDevice));
    }

    private void EditDevice(string? identifier)
    {
        if (_view is null) return;
        _devices.SelectedItem = identifier is null ? null : _view.Devices.FirstOrDefault(item =>
            item.Configuration.Slot.ToString() == identifier);
        if (identifier is null)
        {
            _type.SelectedIndex = _view.Types.Count == 0
                ? AtariStorageSettingsConstants.NoSelectionIndex : AtariStorageSettingsConstants.FirstItemIndex;
            _path.Clear();
        }
        var panel = new StackPanel { Margin = new Thickness(18) };
        panel.Children.Add(AtariAccessibilityFunctions.LabeledRow(
            LocExtension.Get(AtariStorageSettingsConstants.TypeResource), _type));
        panel.Children.Add(AtariAccessibilityFunctions.LabeledRow(
            LocExtension.Get(AtariStorageSettingsConstants.IdentifierResource), _slot));
        panel.Children.Add(AtariAccessibilityFunctions.LabeledRow(
            LocExtension.Get(AtariStorageSettingsConstants.InterfaceResource), _bus));
        panel.Children.Add(AtariAccessibilityFunctions.LabeledRow(
            LocExtension.Get(AtariStorageSettingsConstants.PathResource), _path));
        _type.IsEnabled = false;
        _slot.IsEnabled = false;
        _bus.IsEnabled = false;
        var buttons = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Right };
        var browse = new Button { Content = LocExtension.Get(AtariStorageSettingsConstants.BrowseResource) };
        browse.Click += (_, _) => Browse();
        var save = new Button { Content = LocExtension.Get(AtariStorageSettingsConstants.ConfigureResource), IsDefault = true };
        var dialog = new Window
        {
            Title = LocExtension.Get(AtariStorageSettingsConstants.StorageTabResource),
            Owner = Window.GetWindow(_deviceList), SizeToContent = SizeToContent.WidthAndHeight,
            WindowStartupLocation = WindowStartupLocation.CenterOwner, Content = panel
        };
        save.Click += (_, _) => { Save(); dialog.DialogResult = true; };
        buttons.Children.Add(browse);
        buttons.Children.Add(save);
        panel.Children.Add(buttons);
        dialog.ShowDialog();
        dialog.Content = null;
    }

    private static EmulationStorageDeviceItem ToCommonDevice(AtariStorageDeviceItem item) => new(
        item.Configuration.Slot.ToString(), ToCommonType(item.Configuration.Kind),
        item.DisplayName, string.IsNullOrWhiteSpace(item.Configuration.Path) ? null : item.Configuration.Path,
        false);

    private static EmulationStorageDeviceType ToCommonType(AtariMediaKind kind) => kind switch
    {
        AtariMediaKind.Floppy => EmulationStorageDeviceType.Floppy,
        AtariMediaKind.HardDisk => EmulationStorageDeviceType.HardDisk,
        AtariMediaKind.CompactDisc => EmulationStorageDeviceType.CompactDisc,
        AtariMediaKind.Cassette => EmulationStorageDeviceType.Tape,
        AtariMediaKind.Cartridge => EmulationStorageDeviceType.Cartridge,
        AtariMediaKind.Directory => EmulationStorageDeviceType.Directory,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
    };

    private void Browse()
    {
        if (_type.SelectedItem is AtariStorageTypeChoice { Kind: AtariMediaKind.Directory })
        {
            var folder = new OpenFolderDialog();
            if (folder.ShowDialog() == true) _path.Text = folder.FolderName;
            return;
        }
        var dialog = new OpenFileDialog
        {
            Filter = LocExtension.Get(AtariStorageSettingsConstants.MediaFilterResource)
        };
        if (dialog.ShowDialog() == true) _path.Text = dialog.FileName;
    }

}
