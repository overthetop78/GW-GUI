using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Localization;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;

namespace GWGUI.App.Controls;

internal sealed class AtariStorageSettingsSection
{
    private readonly EmulationStorageDeviceList _deviceList = new();
    private AtariMachineConfiguration? _configuration;
    private AtariStorageView? _view;
    private readonly StackPanel _emulatorOptions = new();
    private readonly CheckBox _showActivityOsd = new();
    private readonly CheckBox _sioAcceleration = new();
    private readonly CheckBox _cassetteBoot = new();
    private readonly CheckBox _showSpeedOsd = new();
    private readonly CheckBox _showSectorOsd = new();
    private readonly CheckBox _realTimeClock = new();
    private readonly CheckBox _printerDevice = new();
    private readonly CheckBox _serialDevice = new();
    internal EmulationStorageDeviceList DeviceList => _deviceList;
    internal UIElement EmulatorOptions => _emulatorOptions;

    internal AtariStorageSettingsSection()
    {
        _showActivityOsd.Content = LocExtension.Get("Emulation.Storage.ActivityOsd");
        _sioAcceleration.Content = LocExtension.Get("Emulation.Atari.Storage.SioAcceleration");
        _cassetteBoot.Content = LocExtension.Get("Emulation.Atari.Storage.CassetteBoot");
        _showSpeedOsd.Content = LocExtension.Get("Emulation.Atari.Storage.SpeedOsd");
        _showSectorOsd.Content = LocExtension.Get("Emulation.Atari.Storage.SectorOsd");
        _realTimeClock.Content = LocExtension.Get("Emulation.Atari.Storage.RealTimeClock");
        _printerDevice.Content = LocExtension.Get("Emulation.Atari.Storage.PrinterDevice");
        _serialDevice.Content = LocExtension.Get("Emulation.Atari.Storage.SerialDevice");
        foreach (var option in new[] { _showActivityOsd, _showSpeedOsd, _showSectorOsd, _sioAcceleration,
                     _cassetteBoot, _realTimeClock, _printerDevice, _serialDevice })
        {
            option.Margin = new Thickness(12, 8, 12, 0);
            option.Visibility = Visibility.Collapsed;
            _emulatorOptions.Children.Add(option);
        }
        _deviceList.AddRequested += (_, _) => EditDevice(null);
        _deviceList.ConfigureRequested += (_, args) => EditDevice(args.Device.Identifier);
        _deviceList.RemoveRequested += (_, args) => RemoveDevice(args.Device.Identifier);
    }

    internal void Load(AtariMachineConfiguration configuration)
    {
        _configuration = configuration;
        _view = AtariStorageSettingsFunctions.Create(configuration);
        var eightBitComputer = AtariEightBitSettingsCatalog.SupportsComputerOptions(configuration.Model);
        var activityKey = configuration.Core == AtariCoreKind.Hatari
            ? "hatari_led_status_display" : AtariEightBitSettingsConstants.ShowActivityOptionKey;
        _showActivityOsd.IsChecked = configuration.Options.TryGetValue(activityKey, out var value)
            ? IsEnabled(value)
            : configuration.Core == AtariCoreKind.Atari800;
        _showActivityOsd.Visibility = configuration.Core == AtariCoreKind.Hatari || eightBitComputer
            ? Visibility.Visible : Visibility.Collapsed;
        _sioAcceleration.IsChecked = !configuration.Options.TryGetValue(
            AtariEightBitSettingsConstants.SioAccelerationOptionKey, out var sio) || IsEnabled(sio);
        _sioAcceleration.Visibility = eightBitComputer ? Visibility.Visible : Visibility.Collapsed;
        var hasCassette = AtariCompatibilityCatalog.Get(configuration.Model).Media.Any(rule =>
            rule.Kind == AtariMediaKind.Cassette && rule.Availability == AtariMediaAvailability.Available);
        _cassetteBoot.IsChecked = configuration.Options.TryGetValue(
            AtariEightBitSettingsConstants.CassetteBootOptionKey, out var cassette) && IsEnabled(cassette);
        _cassetteBoot.Visibility = hasCassette ? Visibility.Visible : Visibility.Collapsed;
        LoadToggle(_showSpeedOsd, configuration, AtariEightBitSettingsConstants.ShowSpeedOptionKey,
            defaultEnabled: false, eightBitComputer);
        LoadToggle(_showSectorOsd, configuration, AtariEightBitSettingsConstants.ShowSectorOptionKey,
            defaultEnabled: false, eightBitComputer);
        LoadToggle(_realTimeClock, configuration, AtariEightBitSettingsConstants.RealTimeClockOptionKey,
            defaultEnabled: false, eightBitComputer);
        LoadToggle(_printerDevice, configuration, AtariEightBitSettingsConstants.PrinterDeviceOptionKey,
            defaultEnabled: false, eightBitComputer);
        LoadToggle(_serialDevice, configuration, AtariEightBitSettingsConstants.SerialDeviceOptionKey,
            defaultEnabled: false, eightBitComputer);
        RefreshDevices();
    }

    internal AtariMachineConfiguration Apply(AtariMachineConfiguration configuration)
    {
        if (_configuration is null) return configuration;
        var storageOptions = _configuration.Options
            .Where(option => option.Key.StartsWith("storage.", StringComparison.Ordinal)).ToList();
        if (configuration.Core == AtariCoreKind.Hatari)
            storageOptions.Add(KeyValuePair.Create("hatari_led_status_display",
                _showActivityOsd.IsChecked == true ? "true" : "false"));
        else if (AtariEightBitSettingsCatalog.SupportsComputerOptions(configuration.Model))
        {
            storageOptions.Add(KeyValuePair.Create(AtariEightBitSettingsConstants.ShowActivityOptionKey,
                ToggleValue(_showActivityOsd)));
            storageOptions.Add(KeyValuePair.Create(AtariEightBitSettingsConstants.SioAccelerationOptionKey,
                ToggleValue(_sioAcceleration)));
            storageOptions.Add(KeyValuePair.Create(AtariEightBitSettingsConstants.CassetteBootOptionKey,
                ToggleValue(_cassetteBoot)));
            storageOptions.Add(KeyValuePair.Create(AtariEightBitSettingsConstants.ShowSpeedOptionKey,
                ToggleValue(_showSpeedOsd)));
            storageOptions.Add(KeyValuePair.Create(AtariEightBitSettingsConstants.ShowSectorOptionKey,
                ToggleValue(_showSectorOsd)));
            storageOptions.Add(KeyValuePair.Create(AtariEightBitSettingsConstants.RealTimeClockOptionKey,
                ToggleValue(_realTimeClock)));
            storageOptions.Add(KeyValuePair.Create(AtariEightBitSettingsConstants.PrinterDeviceOptionKey,
                ToggleValue(_printerDevice)));
            storageOptions.Add(KeyValuePair.Create(AtariEightBitSettingsConstants.SerialDeviceOptionKey,
                ToggleValue(_serialDevice)));
        }
        var options = AtariGeneralSettingsFunctions.MergeOptions(configuration.Options, storageOptions);
        return new AtariMachineConfiguration(configuration.Model, configuration.Firmwares, _configuration.Media,
            options, configuration.Input, configuration.Id, configuration.SchemaVersion,
            configuration.AudioEnabled, configuration.VideoRenderer, configuration.Folders);
    }

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

    private static bool IsEnabled(string value) =>
        string.Equals(value, AtariEightBitSettingsConstants.Enabled, StringComparison.OrdinalIgnoreCase)
        || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);

    private static void LoadToggle(CheckBox editor, AtariMachineConfiguration configuration, string key,
        bool defaultEnabled, bool visible)
    {
        editor.IsChecked = configuration.Options.TryGetValue(key, out var value)
            ? IsEnabled(value) : defaultEnabled;
        editor.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
    }

    private static string ToggleValue(CheckBox option) => option.IsChecked == true
        ? AtariEightBitSettingsConstants.Enabled : AtariEightBitSettingsConstants.Disabled;
}
