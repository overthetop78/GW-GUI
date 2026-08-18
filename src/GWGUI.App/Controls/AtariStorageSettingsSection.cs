using System.Windows;
using GWGUI.App.Localization;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;

namespace GWGUI.App.Controls;

internal sealed class AtariStorageSettingsSection
{
    private readonly EmulationStorageDeviceList _deviceList = new();
    private AtariMachineConfiguration? _configuration;
    private AtariStorageView? _view;
    internal EmulationStorageDeviceList DeviceList => _deviceList;

    internal AtariStorageSettingsSection()
    {
        _deviceList.AddRequested += (_, _) => EditDevice(null);
        _deviceList.ConfigureRequested += (_, args) => EditDevice(args.Device.Identifier);
        _deviceList.RemoveRequested += (_, args) => RemoveDevice(args.Device.Identifier);
    }

    internal void Load(AtariMachineConfiguration configuration)
    {
        _configuration = configuration;
        _view = AtariStorageSettingsFunctions.Create(configuration);
        RefreshDevices();
    }

    internal AtariMachineConfiguration Apply(AtariMachineConfiguration configuration) =>
        _configuration is null ? configuration : new AtariMachineConfiguration(configuration.Model,
            configuration.Firmwares, _configuration.Media, _configuration.Options, configuration.Input,
            configuration.Id, configuration.SchemaVersion, configuration.AudioEnabled,
            configuration.VideoRenderer, configuration.Folders);

    private void RefreshDevices()
    {
        _deviceList.SetDevices((_view?.Devices ?? []).Select(ToCommonDevice));
        _deviceList.SetCanAdd(_view is not null && _configuration is not null
            && AtariStorageSettingsFunctions.CanAdd(_configuration.Model, _view));
    }

    private void EditDevice(string? identifier)
    {
        if (_view is null || _configuration is null) return;
        if (identifier is null)
        {
            var available = _view.Types.Where(type => _view.Slots[type.Kind].Any(slot =>
                _view.Devices.All(device => device.Configuration.Slot != slot.Slot))).ToArray();
            var dialog = new AddStorageDeviceDialog(available.Select(type => ToCommonType(type.Kind)))
                { Owner = Window.GetWindow(_deviceList) };
            if (dialog.ShowDialog() != true) return;
            var kind = ToAtariType(dialog.SelectedType);
            var slot = _view.Slots[kind].First(choice => _view.Devices.All(device =>
                device.Configuration.Slot != choice.Slot)).Slot;
            _configuration = AtariStorageSettingsFunctions.AddDevice(_configuration, kind, slot);
            Reload();
            return;
        }

        var selected = _view.Devices.FirstOrDefault(item => item.Identifier == identifier);
        if (selected is null) return;
        switch (selected.Configuration.Kind)
        {
            case AtariMediaKind.Floppy:
                ConfigureFloppy(selected);
                break;
            case AtariMediaKind.HardDisk:
            case AtariMediaKind.Directory:
                ConfigureHardDisk(selected);
                break;
            case AtariMediaKind.CompactDisc:
                ConfigureCompactDisc(selected);
                break;
        }
    }

    private static EmulationStorageDeviceItem ToCommonDevice(AtariStorageDeviceItem item) => new(
        item.Identifier, ToCommonType(item.Configuration.Kind),
        item.Model, string.IsNullOrWhiteSpace(item.Configuration.Path) ? null : item.Configuration.Path,
        item.CanRemove);

    private void RemoveDevice(string identifier)
    {
        if (_configuration is null || _view is null) return;
        var selected = _view.Devices.FirstOrDefault(item => item.Identifier == identifier);
        if (selected is null || AtariStorageSettingsFunctions.IsPrimaryDevice(
                _configuration.Model, selected.Configuration.Slot)) return;
        var slot = selected.Configuration.Slot;
        _configuration = AtariStorageSettingsFunctions.Remove(_configuration, slot);
        Reload();
    }

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

    private static AtariMediaKind ToAtariType(EmulationStorageDeviceType type) => type switch
    {
        EmulationStorageDeviceType.Floppy => AtariMediaKind.Floppy,
        EmulationStorageDeviceType.HardDisk => AtariMediaKind.HardDisk,
        EmulationStorageDeviceType.CompactDisc => AtariMediaKind.CompactDisc,
        EmulationStorageDeviceType.Tape => AtariMediaKind.Cassette,
        EmulationStorageDeviceType.Cartridge => AtariMediaKind.Cartridge,
        EmulationStorageDeviceType.Directory => AtariMediaKind.Directory,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

    private void ConfigureFloppy(AtariStorageDeviceItem device)
    {
        if (_configuration is null) return;
        var family = AtariStorageSettingsFunctions.Family(_configuration.Model);
        var options = new FloppyDriveDialogOptions(
            AtariStorageSettingsFunctions.FloppyModels(_configuration.Model),
            _configuration.Folders.Floppies ?? _configuration.Folders.Shared ?? Environment.CurrentDirectory,
            LocExtension.Get(AtariStorageSettingsConstants.MediaFilterResource),
            family == AtariMachineFamily.St ? ".st" : ".atr");
        var dialog = new FloppyDriveConfigurationDialog(device.Identifier,
            AtariStorageSettingsFunctions.MachineName(_configuration.Model),
            AtariStorageSettingsFunctions.FloppySettings(_configuration, device.Configuration.Slot), options)
            { Owner = Window.GetWindow(_deviceList) };
        if (dialog.ShowDialog() != true) return;
        _configuration = AtariStorageSettingsFunctions.ConfigureFloppy(
            _configuration, device.Configuration.Slot, dialog.Settings);
        Reload();
    }

    private void ConfigureHardDisk(AtariStorageDeviceItem device)
    {
        if (_configuration is null) return;
        var dialog = new HardDiskDriveConfigurationDialog(device.Identifier,
            AtariStorageSettingsFunctions.MachineName(_configuration.Model), device.Configuration.Path)
            { Owner = Window.GetWindow(_deviceList) };
        if (dialog.ShowDialog() != true || string.IsNullOrWhiteSpace(dialog.SupportPath)) return;
        _configuration = AtariStorageSettingsFunctions.AddOrReplace(_configuration,
            device.Configuration with { Path = dialog.SupportPath }, device.Configuration.Slot);
        Reload();
    }

    private void ConfigureCompactDisc(AtariStorageDeviceItem device)
    {
        if (_configuration is null) return;
        var dialog = new CompactDiscDriveConfigurationDialog(device.Identifier,
            AtariStorageSettingsFunctions.MachineName(_configuration.Model),
            new CompactDiscDriveSettings("CD-ROM", "100"), supportsWriter: false)
            { Owner = Window.GetWindow(_deviceList) };
        dialog.ShowDialog();
    }

    private void Reload()
    {
        if (_configuration is null) return;
        _view = AtariStorageSettingsFunctions.Create(_configuration);
        RefreshDevices();
    }
}
