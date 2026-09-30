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
    private async void VideoConfigurationChanged(object? sender,
        EmulationConfigurationSavedEventArgs args) =>
        await EmulationOpenMachineConfigurationFunctions.TryApplyAsync(_openMachines,
            args.Configuration.ModuleId, args.Configuration.Id, async tab =>
            {
                if (tab.Content is MachineController view)
                {
                    var profile = GWGUI.App.Services.Emulation.EmulationVideoPresentationProfiles.Store.Get(
                        args.Configuration.ModuleId, args.Configuration.Id);
                    view.ApplyVideoConfiguration(profile.Renderer, profile.Processing!);
                    var module = _modules.First(item => item.Id == args.Configuration.ModuleId);
                    await view.ApplyRuntimeOptionsAsync(module.RuntimeOptions(args.Configuration));
                }
            });

    private async void ConfigurationSaved(object? sender, EmulationConfigurationSavedEventArgs args)
    {
        await ReloadConfigurationsAsync();
        await EmulationOpenMachineConfigurationFunctions.TryApplyAsync(_openMachines,
            args.Configuration.ModuleId, args.Configuration.Id, async tab =>
            {
                if (tab.Content is MachineController view)
                {
                    var profile = GWGUI.App.Services.Emulation.EmulationVideoPresentationProfiles.Store.Get(
                        args.Configuration.ModuleId, args.Configuration.Id);
                    view.ApplyVideoConfiguration(profile.Renderer, profile.Processing!);
                    var module = _modules.First(item => item.Id == args.Configuration.ModuleId);
                    await view.ApplyConfigurationAsync(args.Configuration);
                    await view.ApplyRuntimeOptionsAsync(module.RuntimeOptions(args.Configuration));
                }
            });
    }

    public async Task ReloadConfigurationsAsync()
    {
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
        if (_selectedModule is not null
            && !items.Any(item => item.Module.Id == _selectedModule.Id))
        {
            _selectedModule = null;
            _selectedMachineId = null;
            _selectedConfigurationId = null;
        }
        if (_selectedModule is not null && _selectedMachineId is not null)
        {
            var matching = items.Where(item => item.Module.Id == _selectedModule.Id
                && item.Configuration.MachineId == _selectedMachineId).ToArray();
            if (matching.Length == 0)
            {
                _selectedMachineId = null;
                _selectedConfigurationId = null;
            }
            else if (!matching.Any(item => item.Configuration.Id == _selectedConfigurationId))
                _selectedConfigurationId = matching[0].Configuration.Id;
        }
        RefreshSelector();
    }

    private void SelectModule(IEmulationModule module)
    {
        if (ReferenceEquals(_selectedModule, module))
        {
            _selectedModule = null;
            _selectedMachineId = null;
            _selectedConfigurationId = null;
        }
        else
        {
            _selectedModule = module;
            _selectedMachineId = null;
            _selectedConfigurationId = null;
        }
        RefreshSelector();
    }

    private void SelectMachine(IEmulationModule module, EmulationMachineDefinition definition)
    {
        var selected = FindConfiguration(module.Id, definition.Id);
        if (selected is null) return;
        var sameSelection = ReferenceEquals(_selectedModule, module)
            && _selectedMachineId == definition.Id;
        if (sameSelection && _openMachines.TryGetValue((module.Id, selected.Configuration.Id),
            out var existing))
        {
            _machines.SelectedItem = existing;
            return;
        }
        if (sameSelection)
        {
            _selectedMachineId = null;
            _selectedConfigurationId = null;
        }
        else
        {
            _selectedModule = module;
            _selectedMachineId = definition.Id;
            _selectedConfigurationId = selected.Configuration.Id;
        }
        RefreshSelector();
    }

    private EmulationConfigurationListItem? FindConfiguration(string moduleId, string machineId)
    {
        var configurations = _configurations.Where(item => item.Module.Id == moduleId
            && item.Configuration.MachineId == machineId).ToArray();
        return configurations.FirstOrDefault(item => item.Configuration.Id == _selectedConfigurationId)
            ?? configurations.FirstOrDefault();
    }

    private EmulationConfigurationListItem? FindSelectedConfiguration() =>
        _selectedModule is not null && _selectedMachineId is not null
            ? FindConfiguration(_selectedModule.Id, _selectedMachineId)
            : null;

    private void RefreshSelector()
    {
        if (_selectorRendering) return;
        _selectorRendering = true;
        try
        {
            _brandPanel.Children.Clear();
            _machinePanel.Children.Clear();
            var configuredModules = _modules.Where(module => _configurations.Any(item =>
                item.Module.Id == module.Id)).ToArray();
            foreach (var module in configuredModules)
            {
                _brandPanel.Children.Add(CreateBrandButton(module,
                    LocExtension.GetForModule(module, module.DisplayResourceKey)));
            }
            _machinePanel.Visibility = _selectedModule is null
                ? Visibility.Collapsed : Visibility.Visible;
            if (_selectedModule is not null)
            {
                var machineIds = _configurations.Where(item =>
                    item.Module.Id == _selectedModule.Id)
                    .Select(item => item.Configuration.MachineId)
                    .Distinct(StringComparer.Ordinal)
                    .ToHashSet(StringComparer.Ordinal);
                foreach (var definition in _selectedModule.Machines.Where(item =>
                    machineIds.Contains(item.Id)))
                {
                    _machinePanel.Children.Add(CreateMachineButton(_selectedModule, definition,
                        LocExtension.GetForModule(_selectedModule, definition.DisplayResourceKey)));
                }
            }
            var selected = FindSelectedConfiguration();
            var isOpen = selected is not null && _openMachines.ContainsKey(
                (selected.Module.Id, selected.Configuration.Id));
            _open.Visibility = Visibility.Visible;
            _open.IsEnabled = selected is not null && !isOpen;
        }
        finally
        {
            _selectorRendering = false;
        }
    }

    private void ActiveMachineTabChanged(object? sender, SelectionChangedEventArgs args)
    {
        if (_selectorRendering
            || _machines.SelectedItem is not TabItem { Tag: EmulationMachineRuntime runtime }) return;
        var module = _modules.FirstOrDefault(item =>
            item.Id == runtime.Configuration.ModuleId);
        if (module is null) return;
        _selectedModule = module;
        _selectedMachineId = runtime.Configuration.MachineId;
        _selectedConfigurationId = runtime.Configuration.Id;
        RefreshSelector();
    }

    private async void OpenSelectedMachine(object sender, RoutedEventArgs args)
    {
        var selected = FindSelectedConfiguration();
        if (selected is null) return;
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
            RefreshSelector();
        }
    }
}
