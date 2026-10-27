using GWGUI.App.Contracts.Emulation.Firmware;
using GWGUI.App.Functions.Views.Emulation.Settings;
using GWGUI.App.Localization.Extensions;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using GWGUI.Emulation;


namespace GWGUI.App.Controllers.Emulation.Firmware;

internal sealed class EmulationFirmwareManagementController
{
    private readonly IEmulationModule _module;
    private readonly IEmulationFirmwareManager _manager;
    private readonly Func<IEmulationConfiguration> _getConfiguration;
    private readonly Action<IEmulationConfiguration> _setConfiguration;
    private readonly Dictionary<EmulationMachineTab, FirmwareView> _views = [];

    /// <summary>Chaque onglet possède sa liste et son action, sans déplacer les contrôles d'un autre onglet.</summary>
    private sealed record FirmwareView(EmulationMachineTab Tab, ListBox Firmwares, Button Use);

    internal EmulationFirmwareManagementController(IEmulationModule module, IEmulationFirmwareManager manager, Func<IEmulationConfiguration> getConfiguration, Action<IEmulationConfiguration> setConfiguration)
    {
        _module = module;
        _manager = manager;
        _getConfiguration = getConfiguration;
        _setConfiguration = setConfiguration;
    }

    internal event EventHandler? ConfigurationChanged;

    /// <summary>Relâche les anciennes vues avant reconstruction de l'éditeur de machine.</summary>
    internal void ClearViews() => _views.Clear();

    internal UIElement CreateView(UIElement configuredFirmware, EmulationMachineTab tab = EmulationMachineTab.Rom)
    {
        var view = new FirmwareView(tab, new ListBox(), new Button());
        _views[tab] = view;
        view.Firmwares.SelectionChanged += (_, _) => UpdateUseButton(view);
        view.Use.Click += (_, _) => UseSelected(view);
        return EmulationSettingsLayout.FirmwareSettingsPage(new EmulationFirmwareSettingsContent(configuredFirmware, view.Firmwares, _ => RefreshAsync(), view.Use, _ => OpenFolderAsync()));
    }

    internal async Task RefreshAsync()
    {
        var configuration = _getConfiguration();
        var views = _views.Values.ToArray();
        if (views.Length == 0) return;
        var fields = _module.Describe(configuration.MachineId, configuration).Blocks.SelectMany(block => block.Fields).ToArray();
        var entries = await _manager.ScanFirmwareAsync(configuration.MachineId, configuration);
        if (_getConfiguration().Id != configuration.Id || _getConfiguration().MachineId != configuration.MachineId) return;
        foreach (var view in views)
        {
            if (!_views.TryGetValue(view.Tab, out var current) || !ReferenceEquals(current, view)) continue;
            view.Firmwares.Items.Clear();
            foreach (var firmware in entries.OrderBy(item => EmulationSettingsLayout.FirmwareCompatibilityOrder(item.Compatibility)).ThenBy(item => item.DisplayName, StringComparer.CurrentCultureIgnoreCase))
            {
                var destinationField = fields.FirstOrDefault(field => string.Equals(field.Id, firmware.DestinationFieldId, StringComparison.Ordinal));
                // Les fichiers non identifiés restent consultables. Une destination connue appartient à son onglet.
                if (destinationField is not null && destinationField.Tab != view.Tab) continue;
                var destination = destinationField is null ? string.Empty : LocExtension.GetForModule(_module, destinationField.LabelResourceKey);
                view.Firmwares.Items.Add(new ListBoxItem
                {
                    Tag = firmware,
                    Padding = new Thickness(0),
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Content = EmulationSettingsLayout.FirmwareRow(firmware.DisplayName, firmware.Version, firmware.Compatibility, firmware.Path, destination)
                });
            }
            UpdateUseButton(view);
        }
    }

    internal string GetFirmwareDirectory() => _manager.GetFirmwareDirectory(_getConfiguration().MachineId);

    private void UseSelected(FirmwareView view)
    {
        if ((view.Firmwares.SelectedItem as ListBoxItem)?.Tag is not EmulationFirmwareCandidate firmware
            || !CanUse(view, firmware)) return;
        _setConfiguration(_manager.UseFirmware(_getConfiguration(), firmware));
        ConfigurationChanged?.Invoke(this, EventArgs.Empty);
    }

    private bool CanUse(FirmwareView view, EmulationFirmwareCandidate firmware)
    {
        if (firmware.DestinationFieldId is null || firmware.Compatibility == EmulationFirmwareCompatibility.Incompatible) return false;
        var configuration = _getConfiguration();
        return _module.Describe(configuration.MachineId, configuration).Blocks
            .Where(block => block.Tab == view.Tab && block.IsVisible)
            .SelectMany(block => block.Fields)
            .Any(field => field.Id == firmware.DestinationFieldId && field.IsEnabled && field.IsVisible);
    }

    private void UpdateUseButton(FirmwareView view)
    {
        var firmware = (view.Firmwares.SelectedItem as ListBoxItem)?.Tag as EmulationFirmwareCandidate;
        EmulationSettingsLayout.UpdateFirmwareUseButton(view.Use, firmware is not null && CanUse(view, firmware) ? firmware.Compatibility : null);
    }

    private Task OpenFolderAsync()
    {
        var configuration = _getConfiguration();
        var directory = _manager.GetFirmwareDirectory(configuration.MachineId);
        Directory.CreateDirectory(directory);
        using var request = Process.Start(new ProcessStartInfo(directory) { UseShellExecute = true });
        return Task.CompletedTask;
    }
}
