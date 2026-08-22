using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Constants.Emulation.Errors;
using GWGUI.App.Contracts.Emulation.Configurations;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Presenters.Common;
using GWGUI.App.Presenters.Emulation.Configurations;
using System.Windows;
using System.Windows.Controls;


namespace GWGUI.App.Views.Controls.Emulation.Machine;

public sealed partial class EmulationSection
{
    private async void ConfigurationSaved(object? sender, EmulationConfigurationSavedEventArgs args)
    {
        await ReloadConfigurationsAsync();
        if (_openMachines.TryGetValue(
                (args.Configuration.ModuleId, args.Configuration.Id), out var tab)
            && tab.Content is MachineController view)
            view.ApplyVideoRenderer(args.Configuration.VideoRenderer);
    }

    public async Task ReloadConfigurationsAsync()
    {
        var selected = _configuration.SelectedItem as EmulationConfigurationListItem;
        var items = new List<EmulationConfigurationListItem>();
        foreach (var module in _modules)
        {
            foreach (var configuration in await module.LoadConfigurationsAsync())
            {
                items.Add(new EmulationConfigurationListItem(module, configuration,
                    EmulationConfigurationPresenter.DisplayName(module, configuration)));
            }
        }
        _configuration.ItemsSource = items;
        _configuration.SelectedItem = _configuration.Items.OfType<EmulationConfigurationListItem>()
            .FirstOrDefault(item => item.Module.Id == selected?.Module.Id
                && item.Configuration.Id == selected.Configuration.Id)
            ?? _configuration.Items.OfType<EmulationConfigurationListItem>().FirstOrDefault();
        _open.IsEnabled = _configuration.SelectedItem is not null;
    }

    private async void OpenSelectedMachine(object sender, RoutedEventArgs args)
    {
        if (_configuration.SelectedItem is not EmulationConfigurationListItem selected) return;
        var key = (selected.Module.Id, selected.Configuration.Id);
        if (_openMachines.TryGetValue(key, out var existing))
        {
            _machines.SelectedItem = existing;
            return;
        }
        try
        {
            _open.IsEnabled = false;
            await OpenMachineAsync(selected);
        }
        catch (Exception error)
        {
            ControlErrorPresenter.ShowEmulation(this, error,
                ControlErrorContexts.EmulationConfigurationOpening, selected.DisplayName);
        }
        finally
        {
            _open.IsEnabled = _configuration.SelectedItem is not null;
        }
    }
}
