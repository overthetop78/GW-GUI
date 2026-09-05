using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Constants.Emulation.Errors;
using GWGUI.App.Contracts.Emulation.Configurations;
using GWGUI.App.Functions.Views.Emulation.Machine;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Presenters.Common;
using GWGUI.App.Presenters.Emulation.Configurations;
using System.Windows;
using System.Windows.Controls;


namespace GWGUI.App.Views.Controls.Emulation.Machine;

public sealed partial class EmulationSection
{
    private void VideoConfigurationChanged(object? sender,
        EmulationConfigurationSavedEventArgs args) =>
        EmulationOpenMachineConfigurationFunctions.TryApply(_openMachines,
            args.Configuration.ModuleId, args.Configuration.Id, tab =>
            {
                if (tab.Content is MachineController view)
                {
                    var profile = GWGUI.App.Services.Emulation.EmulationVideoPresentationProfiles.Store.Get(
                        args.Configuration.ModuleId, args.Configuration.Id);
                    view.ApplyVideoConfiguration(profile.Renderer, profile.Processing!);
                }
            });

    private async void ConfigurationSaved(object? sender, EmulationConfigurationSavedEventArgs args)
    {
        await ReloadConfigurationsAsync();
        EmulationOpenMachineConfigurationFunctions.TryApply(_openMachines,
            args.Configuration.ModuleId, args.Configuration.Id, tab =>
            {
                if (tab.Content is MachineController view)
                {
                    var profile = GWGUI.App.Services.Emulation.EmulationVideoPresentationProfiles.Store.Get(
                        args.Configuration.ModuleId, args.Configuration.Id);
                    view.ApplyVideoConfiguration(profile.Renderer, profile.Processing!);
                }
            });
    }

    public async Task ReloadConfigurationsAsync()
    {
        var selected = _configuration.SelectedItem as EmulationConfigurationListItem;
        var selectedModuleId = (_module.SelectedItem as EmulationModuleListItem)?.Module.Id
            ?? selected?.Module.Id;
        var items = new List<EmulationConfigurationListItem>();
        foreach (var module in _modules)
        {
            foreach (var configuration in await module.LoadConfigurationsAsync())
            {
                items.Add(new EmulationConfigurationListItem(module, configuration,
                    EmulationConfigurationPresenter.DisplayName(module, configuration)));
            }
        }
        _configurations = items;
        var modules = _modules.Select(module => new EmulationModuleListItem(module,
            LocExtension.Get(module.DisplayResourceKey))).ToArray();
        _module.ItemsSource = modules;
        _module.SelectedItem = modules.FirstOrDefault(item => item.Module.Id == selectedModuleId)
            ?? modules.FirstOrDefault(item => items.Any(configuration =>
                configuration.Module.Id == item.Module.Id))
            ?? modules.FirstOrDefault();
        RefreshConfigurations(selected?.Configuration.Id);
    }

    private void ModuleSelectionChanged(object sender, SelectionChangedEventArgs args) =>
        RefreshConfigurations();

    private void RefreshConfigurations(Guid? selectedConfigurationId = null)
    {
        if (_module.SelectedItem is not EmulationModuleListItem selectedModule)
        {
            _configuration.ItemsSource = null;
            _open.IsEnabled = false;
            return;
        }

        var selected = _configuration.SelectedItem as EmulationConfigurationListItem;
        if (selected?.Module.Id == selectedModule.Module.Id)
            selectedConfigurationId ??= selected.Configuration.Id;
        var configurations = _configurations.Where(item =>
            item.Module.Id == selectedModule.Module.Id).ToArray();
        _configuration.ItemsSource = configurations;
        _configuration.SelectedItem = configurations.FirstOrDefault(item =>
                item.Configuration.Id == selectedConfigurationId)
            ?? configurations.FirstOrDefault();
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
