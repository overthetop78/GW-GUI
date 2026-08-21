using System.IO;
using System.Windows;
using GWGUI.App.Localization;
using GWGUI.Emulation;
using Microsoft.Win32;

namespace GWGUI.App.Controls;

internal sealed class EmulationStorageSettingsController
{
    private readonly IEmulationStorageSettingsManager _manager;
    private readonly EmulationStorageDeviceList _view = new();
    private IEmulationConfiguration? _configuration;
    private EmulationStorageSettings _settings = new([], [], []);

    internal EmulationStorageSettingsController(IEmulationStorageSettingsManager manager)
    {
        _manager = manager;
        _view.AddRequested += AddDevice;
        _view.RemoveRequested += RemoveDevice;
        _view.ConfigureRequested += ConfigureDevice;
    }

    internal UIElement CreateContent(IEmulationConfiguration configuration)
    {
        _configuration = configuration;
        _settings = _manager.DescribeStorageSettings(configuration);
        Rebuild();
        return _view;
    }

    internal IEmulationConfiguration Apply(IEmulationConfiguration configuration) =>
        _manager.ApplyStorageSettings(configuration, _settings);

    private void AddDevice(object? sender, EventArgs args)
    {
        var candidates = _settings.AvailableDevices
            .Where(device => !_settings.ConfiguredSlots.Contains(device.Slot)).ToArray();
        if (candidates.Length == 0) return;
        var dialog = new AddStorageDeviceDialog(candidates.Select(device => device.MediaType).Distinct());
        if (dialog.ShowDialog() != true) return;
        var type = dialog.SelectedType;
        var selected = candidates.First(device => device.MediaType == type);
        var deviceSettings = (_settings.DeviceSettings ?? []).ToList();
        if (selected.FloppyOptions is { Models.Count: > 0 } floppy)
            deviceSettings.Add(new EmulationStorageDeviceSettings(selected.Slot,
                new FloppyDriveSettings(floppy.Models[0].Value, "100", false, false)));
        _settings = _settings with
        {
            ConfiguredSlots = _settings.ConfiguredSlots.Append(selected.Slot).ToArray(),
            DeviceSettings = deviceSettings
        };
        Rebuild();
    }

    private void RemoveDevice(object? sender, EmulationStorageDeviceEventArgs args)
    {
        if (!TryFind(args.Device, out var device)) return;
        _settings = _settings with
        {
            ConfiguredSlots = _settings.ConfiguredSlots.Where(slot => slot != device.Slot).ToArray(),
            MountedMedia = _settings.MountedMedia.Where(media => media.Slot != device.Slot).ToArray(),
            DeviceSettings = (_settings.DeviceSettings ?? []).Where(item => item.Slot != device.Slot).ToArray()
        };
        Rebuild();
    }

    private void ConfigureDevice(object? sender, EmulationStorageDeviceEventArgs args)
    {
        if (!TryFind(args.Device, out var device)) return;
        if (device.FloppyOptions is not null)
        {
            ConfigureFloppy(device);
            return;
        }
        if (device.MediaType == EmulationMediaType.HardDisk)
        {
            ConfigureHardDisk(device);
            return;
        }
        var filter = string.Join(';', device.AcceptedExtensions.Select(extension => $"*{extension}"));
        var dialog = new OpenFileDialog
        {
            Filter = string.IsNullOrWhiteSpace(filter)
                ? LocExtension.Get("Emulation.Storage.Media.Associated") + "|*.*"
                : LocExtension.Get("Emulation.Storage.Media.Associated") + $"|{filter}|" +
                  LocExtension.Get("Emulation.Storage.Media.Associated") + "|*.*"
        };
        if (dialog.ShowDialog() != true) return;
        var media = new EmulationMedia(dialog.FileName, device.Slot, device.MediaType, false, true);
        _settings = _settings with
        {
            MountedMedia = _settings.MountedMedia.Where(item => item.Slot != device.Slot).Append(media).ToArray()
        };
        Rebuild();
    }

    private void ConfigureHardDisk(EmulationMediaDevice device)
    {
        var current = _settings.MountedMedia.FirstOrDefault(item => item.Slot == device.Slot);
        var dialog = new HardDiskDriveConfigurationDialog(device.DisplayLabel ?? device.Slot.ToString(),
            _configuration?.MachineId ?? string.Empty, current?.Path,
            device.ImageDirectory ?? Environment.CurrentDirectory);
        if (dialog.ShowDialog() != true || string.IsNullOrWhiteSpace(dialog.SupportPath)) return;
        var media = new EmulationMedia(dialog.SupportPath, device.Slot, device.MediaType,
            current?.IsReadOnly ?? false, true);
        _settings = _settings with
        {
            MountedMedia = _settings.MountedMedia.Where(item => item.Slot != device.Slot).Append(media).ToArray()
        };
        Rebuild();
    }

    private void ConfigureFloppy(EmulationMediaDevice device)
    {
        var current = (_settings.DeviceSettings ?? []).FirstOrDefault(item => item.Slot == device.Slot)?.Floppy
            ?? new FloppyDriveSettings(device.FloppyOptions!.Models[0].Value, "100", false, false);
        var dialog = new FloppyDriveConfigurationDialog(device.DisplayLabel ?? device.Slot.ToString(),
            _configuration?.MachineId ?? string.Empty, current, device.FloppyOptions!);
        if (dialog.ShowDialog() != true) return;
        var settings = (_settings.DeviceSettings ?? []).Where(item => item.Slot != device.Slot)
            .Append(new EmulationStorageDeviceSettings(device.Slot, dialog.Settings)).ToArray();
        _settings = _settings with { DeviceSettings = settings };
        Rebuild();
    }

    private bool TryFind(EmulationStorageDeviceItem item, out EmulationMediaDevice device)
    {
        device = default!;
        device = _settings.AvailableDevices.FirstOrDefault(candidate => candidate.Slot == item.Slot)!;
        return device is not null;
    }

    private void Rebuild()
    {
        var rows = _settings.ConfiguredSlots.Select(slot =>
        {
            var device = _settings.AvailableDevices.First(candidate => candidate.Slot == slot);
            var media = _settings.MountedMedia.FirstOrDefault(item => item.Slot == slot);
            var model = DeviceModel(device, slot);
            return new EmulationStorageDeviceItem(slot, device.DisplayLabel ?? slot.ToString(), device.MediaType,
                model, media?.Path, device.IsRemovable);
        }).ToArray();
        _view.SetDevices(rows);
        _view.SetCanAdd(_settings.AvailableDevices.Any(device => !_settings.ConfiguredSlots.Contains(device.Slot)));
    }

    private string DeviceModel(EmulationMediaDevice device, EmulationMediaSlot slot)
    {
        if (device.FloppyOptions is null) return device.MediaType.ToString();
        var selected = (_settings.DeviceSettings ?? []).FirstOrDefault(item => item.Slot == slot)?.Floppy?.Model;
        var choice = device.FloppyOptions.Models.FirstOrDefault(item => item.Value == selected)
            ?? device.FloppyOptions.Models[0];
        return string.IsNullOrWhiteSpace(choice.DisplayResourceKey)
            ? choice.InvariantDisplayValue ?? choice.Value
            : LocExtension.Get(choice.DisplayResourceKey);
    }
}
