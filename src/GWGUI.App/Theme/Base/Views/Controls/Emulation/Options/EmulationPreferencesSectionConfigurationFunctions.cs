using GWGUI.App.Constants.Emulation.Errors;
using GWGUI.App.Presenters.Common;
using GWGUI.App.Contracts.Emulation.Configurations;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Presenters.Emulation.Configurations;
using System.Windows;

namespace GWGUI.App.Views.Controls.Emulation.Options;

public sealed partial class EmulationPreferencesSection
{
    private async Task ReloadConfigurationsAsync()
    {
        var loaded = await Task.WhenAll(_modules.Select(async module => (Module: module, Configurations: await Task.Run(async () => await module.LoadConfigurationsAsync()))));
        foreach (var item in loaded) GWGUI.Emulation.EmulationSdk.Networks.SetConfigurations(item.Module, item.Configurations);
        _configurationRows = EmulationConfigurationTablePresenter.CreateRows(loaded.SelectMany(item => item.Configurations.Select(configuration => (item.Module, Configuration: configuration))));
        RebuildConfigurationBrands();
        FilterConfigurationTable();
    }

    private async Task DeleteConfigurationAsync(EmulationConfigurationTableRow row)
    {
        var answer = MessageBox.Show(
            string.Format(LocExtension.Get("Emulation.Configuration.DeleteConfirm"), LocExtension.GetForModule(row.Module, row.Module.DisplayResourceKey), row.MachineName),
            LocExtension.Get("Common.Delete"),
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (answer != MessageBoxResult.Yes)
            return;

        try
        {
            await row.Module.DeleteConfigurationAsync(row.Configuration.Id);
            GWGUI.App.Services.Emulation.EmulationVideoPresentationProfiles.Store.Delete(row.Module.Id, row.Configuration.Id);
            await ReloadConfigurationsAsync();
            foreach (Func<Task> refresh in ConfigurationDeleted?.GetInvocationList() ?? [])
                await refresh();
        }
        catch (Exception error)
        {
            ControlErrorPresenter.ShowEmulation(this, error, ControlErrorContexts.EmulationConfigurationManagement, LocExtension.GetForModule(row.Module, row.Module.DisplayResourceKey));
        }
    }

    private async Task EditConfigurationAsync(EmulationConfigurationTableRow row)
    {
        if (EditConfigurationRequested is { } edit)
            await edit(row.Module, row.Configuration);
    }

    private void FilterConfigurationTable()
    {
        var selectedModule = _selectedConfigurationModule;
        _configurationTable.SetRows(selectedModule is null ? [] : _configurationRows.Where(row => row.Module.Id == selectedModule.Id
            && (_selectedConfigurationMachineId is null || row.Configuration.MachineId == _selectedConfigurationMachineId)).ToArray());
    }

    private void RebuildConfigurationBrands()
    {
        var configuredModules = _modules.Where(module => _configurationRows.Any(row => row.Module.Id == module.Id)).ToArray();
        _selectedConfigurationModule = configuredModules.FirstOrDefault(module => module.Id == _selectedConfigurationModule?.Id)
            ?? configuredModules.FirstOrDefault();
        var machineIds = _configurationRows.Where(row => row.Module.Id == _selectedConfigurationModule?.Id)
            .Select(row => row.Configuration.MachineId).ToHashSet(StringComparer.Ordinal);
        if (_selectedConfigurationMachineId is not null && !machineIds.Contains(_selectedConfigurationMachineId))
            _selectedConfigurationMachineId = null;
        _configurationSelector.SetItems(configuredModules, _selectedConfigurationModule,
            _selectedConfigurationModule?.Machines.Where(machine => machineIds.Contains(machine.Id)).ToArray() ?? [],
            _selectedConfigurationMachineId);
    }

}
