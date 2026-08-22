using GWGUI.App.Contracts.Emulation.Firmware;
using GWGUI.App.Functions.Views.Emulation.Settings;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using GWGUI.Emulation;


namespace GWGUI.App.Controllers.Emulation.Firmware;

internal sealed class EmulationFirmwareManagementController
{
    private readonly IEmulationFirmwareManager _manager;
    private readonly Func<IEmulationConfiguration> _getConfiguration;
    private readonly Action<IEmulationConfiguration> _setConfiguration;
    private ListBox _firmwares = null!;
    private Button _use = null!;

    internal EmulationFirmwareManagementController(IEmulationFirmwareManager manager,
        Func<IEmulationConfiguration> getConfiguration, Action<IEmulationConfiguration> setConfiguration)
    {
        _manager = manager;
        _getConfiguration = getConfiguration;
        _setConfiguration = setConfiguration;
    }

    internal UIElement CreateView(UIElement configuredFirmware)
    {
        _firmwares = new ListBox();
        _use = new Button();
        _firmwares.SelectionChanged += (_, _) => UpdateUseButton();
        _use.Click += (_, _) => UseSelected();
        return EmulationSettingsLayout.FirmwareSettingsPage(new EmulationFirmwareSettingsContent(
            configuredFirmware, _firmwares, _ => RefreshAsync(), _use, _ => OpenFolderAsync()));
    }

    internal async Task RefreshAsync()
    {
        var configuration = _getConfiguration();
        var entries = await _manager.ScanFirmwareAsync(configuration.MachineId, configuration);
        _firmwares.Items.Clear();
        foreach (var firmware in entries
                     .OrderBy(item => EmulationSettingsLayout.FirmwareCompatibilityOrder(item.Compatibility))
                     .ThenBy(item => item.DisplayName, StringComparer.CurrentCultureIgnoreCase))
        {
            _firmwares.Items.Add(new ListBoxItem
            {
                Tag = firmware,
                Padding = new Thickness(0),
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Content = EmulationSettingsLayout.FirmwareRow(firmware.DisplayName, firmware.Version,
                    firmware.Compatibility, firmware.Path)
            });
        }
        UpdateUseButton();
    }

    internal string GetFirmwareDirectory() =>
        _manager.GetFirmwareDirectory(_getConfiguration().MachineId);

    private void UseSelected()
    {
        if ((_firmwares.SelectedItem as ListBoxItem)?.Tag is EmulationFirmwareCandidate firmware)
            Use(firmware);
    }

    private void Use(EmulationFirmwareCandidate firmware)
    {
        if (firmware.Compatibility == EmulationFirmwareCompatibility.Incompatible) return;
        _setConfiguration(_manager.UseFirmware(_getConfiguration(), firmware));
    }

    private void UpdateUseButton() => EmulationSettingsLayout.UpdateFirmwareUseButton(_use,
        (_firmwares.SelectedItem as ListBoxItem)?.Tag is EmulationFirmwareCandidate firmware
            ? firmware.Compatibility : null);

    private Task OpenFolderAsync()
    {
        var configuration = _getConfiguration();
        var directory = _manager.GetFirmwareDirectory(configuration.MachineId);
        Directory.CreateDirectory(directory);
        Process.Start(new ProcessStartInfo(directory) { UseShellExecute = true });
        return Task.CompletedTask;
    }
}
