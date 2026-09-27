using GWGUI.App.Contracts.Emulation.Storage;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Views.Controls.Emulation.Storage;
using GWGUI.App.Views.Dialogs.Emulation.Storage;
using System.Windows;
using GWGUI.Emulation;
using GWGUI.App.Services.Emulation;
using GWGUI.App.Functions.Emulation.Storage;
using GWGUI.App.Constants.Localization;


namespace GWGUI.App.Controllers.Emulation.Storage;

internal sealed class EmulationStorageSettingsController
{
    private readonly IEmulationStorageSettingsManager _manager;
    private readonly Func<EmulationDefaultFolderCategory?, string> _defaultFolder;
    private readonly string _moduleId;
    private EmulationStorageDeviceList _view = null!;
    private IEmulationConfiguration? _configuration;
    private EmulationStorageSettings _settings = new([], [], []);

    internal EmulationStorageSettingsController(string moduleId, IEmulationStorageSettingsManager manager,
        Func<EmulationDefaultFolderCategory?, string> defaultFolder)
    {
        _moduleId = moduleId;
        _manager = manager;
        _defaultFolder = defaultFolder;
    }

    internal event EventHandler? SettingsChanged;

    internal UIElement CreateContent(IEmulationConfiguration configuration)
    {
        _view = new EmulationStorageDeviceList();
        _view.AddRequested += AddDevice;
        _view.RemoveRequested += RemoveDevice;
        _view.ConfigureRequested += ConfigureDevice;
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
        EmulationMediaType type;
        try
        {
            if (dialog.ShowDialog() != true) return;
            type = dialog.SelectedType;
        }
        finally
        {
            dialog.Close();
        }
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
        SettingsChanged?.Invoke(this, EventArgs.Empty);
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
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ConfigureDevice(object? sender, EmulationStorageDeviceEventArgs args)
    {
        if (!TryFind(args.Device, out var device)) return;
        switch (device.ConfigurationKind)
        {
            case EmulationStorageConfigurationKind.FloppyDrive:
                ConfigureFloppy(device);
                break;
            case EmulationStorageConfigurationKind.HardDiskDrive:
                ConfigureHardDisk(device);
                break;
        }
    }

    private void ConfigureHardDisk(EmulationMediaDevice device)
    {
        var current = _settings.MountedMedia.FirstOrDefault(item => item.Slot == device.Slot);
        var dialog = new HardDiskDriveConfigurationDialog(device.DisplayLabel ?? device.Slot.ToString(),
            _configuration?.MachineId ?? string.Empty, current?.Path,
            device.ImageDirectory ?? _defaultFolder(EmulationDefaultFolderCategory.HardDisk),
            device.HardDiskFormats ?? [], EmulationMediaDialogFunctions.ClientGuid(_moduleId,
                _configuration?.MachineId ?? string.Empty, device.Slot),
            path => HardDiskDeletionService.DeleteAsync(path, _configuration!));
        string? supportPath;
        string? interfaceId;
        try
        {
            if (dialog.ShowDialog() != true) return;
            supportPath = dialog.SupportPath;
            interfaceId = dialog.InterfaceId;
        }
        finally
        {
            dialog.Close();
        }
        var media = string.IsNullOrWhiteSpace(supportPath) ? null
            : new EmulationMedia(supportPath, device.Slot, device.MediaType,
                current?.IsReadOnly ?? false, true);
        _settings = _settings with
        {
            MountedMedia = _settings.MountedMedia.Where(item => item.Slot != device.Slot)
                .Concat(media is null ? [] : new[] { media }).ToArray(),
            DeviceSettings = (_settings.DeviceSettings ?? []).Where(item => item.Slot != device.Slot)
                .Append(new EmulationStorageDeviceSettings(device.Slot,
                    InterfaceId: media is null ? null : interfaceId?.ToLowerInvariant())).ToArray()
        };
        Rebuild();
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ConfigureFloppy(EmulationMediaDevice device)
    {
        var current = (_settings.DeviceSettings ?? []).FirstOrDefault(item => item.Slot == device.Slot)?.Floppy
            ?? new FloppyDriveSettings(device.FloppyOptions!.Models[0].Value, "100", false, false);
        var dialog = new FloppyDriveConfigurationDialog(device.DisplayLabel ?? device.Slot.ToString(),
            _configuration?.MachineId ?? string.Empty, current, device.FloppyOptions!,
            EmulationMediaDialogFunctions.ClientGuid(_moduleId,
                _configuration?.MachineId ?? string.Empty, device.Slot), _moduleId);
        FloppyDriveSettings selectedSettings;
        try
        {
            if (dialog.ShowDialog() != true) return;
            selectedSettings = dialog.Settings;
        }
        finally
        {
            dialog.Close();
        }
        var settings = (_settings.DeviceSettings ?? []).Where(item => item.Slot != device.Slot)
            .Append(new EmulationStorageDeviceSettings(device.Slot, selectedSettings)).ToArray();
        _settings = _settings with { DeviceSettings = settings };
        Rebuild();
        SettingsChanged?.Invoke(this, EventArgs.Empty);
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
                model, media?.Path, !device.IsPermanent,
                device.ConfigurationKind != EmulationStorageConfigurationKind.None);
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
            : LocExtension.GetForModule(_moduleId, choice.DisplayResourceKey);
    }

}
