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
        var loaded = await Task.WhenAll(_modules.Select(async module =>
            (Module: module, Configurations: await Task.Run(async () =>
                await module.LoadConfigurationsAsync()))));
        _configurationRows = EmulationConfigurationTablePresenter.CreateRows(
            loaded.SelectMany(item => item.Configurations.Select(configuration =>
                (item.Module, Configuration: configuration))));
        RebuildConfigurationBrands();
        FilterConfigurationTable();
    }

    private async Task DeleteConfigurationAsync(EmulationConfigurationTableRow row)
    {
        var answer = MessageBox.Show(
            string.Format(
                LocExtension.Get("Emulation.Configuration.DeleteConfirm"),
                LocExtension.GetForModule(row.Module, row.Module.DisplayResourceKey),
                row.MachineName),
            LocExtension.Get("Common.Delete"),
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (answer != MessageBoxResult.Yes)
            return;

        try
        {
            await row.Module.DeleteConfigurationAsync(row.Configuration.Id);
            GWGUI.App.Services.Emulation.EmulationVideoPresentationProfiles.Store.Delete(
                row.Module.Id, row.Configuration.Id);
            await ReloadConfigurationsAsync();
        }
        catch (Exception error)
        {
            ControlErrorPresenter.ShowEmulation(
                this,
                error,
                ControlErrorContexts.EmulationConfigurationManagement,
                LocExtension.GetForModule(row.Module, row.Module.DisplayResourceKey));
        }
    }

    private async Task EditConfigurationAsync(EmulationConfigurationTableRow row)
    {
        if (EditConfigurationRequested is { } edit)
            await edit(row.Module, row.Configuration);
    }

    private void FilterConfigurationTable()
    {
        var selectedModule = (_configurationBrand.SelectedItem as EmulationModuleListItem)?.Module;
        _configurationTable.SetRows(selectedModule is null
            ? []
            : _configurationRows.Where(row => ReferenceEquals(row.Module, selectedModule)).ToArray());
    }

    private void RebuildConfigurationBrands()
    {
        var selectedModule = (_configurationBrand.SelectedItem as EmulationModuleListItem)?.Module;
        _configurationBrand.ItemsSource = _configurationBrands;
        _configurationBrand.DisplayMemberPath = nameof(EmulationModuleListItem.DisplayName);
        _configurationBrands.Clear();
        foreach (var module in _modules.Where(module =>
                     _configurationRows.Any(row => ReferenceEquals(row.Module, module))))
            _configurationBrands.Add(new EmulationModuleListItem(
                module, LocExtension.GetForModule(module, module.DisplayResourceKey)));
        _configurationBrand.SelectedItem = selectedModule is null
            ? null
            : _configurationBrands.FirstOrDefault(item =>
                ReferenceEquals(item.Module, selectedModule));
    }

}

