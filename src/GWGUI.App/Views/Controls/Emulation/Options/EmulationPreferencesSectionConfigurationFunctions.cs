using GWGUI.App.Constants.Emulation.Errors;
using GWGUI.App.Presenters.Common;
using GWGUI.App.Functions.Views.Emulation.Machine;
using GWGUI.App.Views.Controls.Emulation.Machine;
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
        _configurationTable.SetRows(selectedModule is null ? [] : _configurationRows.Where(row => ReferenceEquals(row.Module, selectedModule)).ToArray());
    }

    private void RebuildConfigurationBrands()
    {
        var configuredModules = _modules.Where(module => _configurationRows.Any(row => row.Module.Id == module.Id)).ToArray();
        _selectedConfigurationModule = configuredModules.FirstOrDefault(module => module.Id == _selectedConfigurationModule?.Id)
            ?? configuredModules.FirstOrDefault();
        _configurationBrand.Children.Clear();
        foreach (var module in configuredModules)
        {
            var image = EmulationAssetFunctions.Load(module, module.BrandImageResourceName);
            if (image is null) continue;
            var button = EmulationSelectorButtonFunctions.Create(
                LocExtension.GetForModule(module, module.DisplayResourceKey), image, module.Id == _selectedConfigurationModule?.Id);
            button.Click += (_, _) =>
            {
                _selectedConfigurationModule = module;
                RebuildConfigurationBrands();
                FilterConfigurationTable();
            };
            _configurationBrand.Children.Add(button);
        }
    }

}

