using GWGUI.App.Constants.Emulation.Errors;
using GWGUI.App.Presenters.Common;
using GWGUI.App.Contracts.Emulation.Configurations;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Presenters.Emulation.Configurations;
using System.Windows;

namespace GWGUI.App.Views.Controls.Emulation.Options;

public sealed partial class OptionsEmulationSection
{
    private async void ModuleConfigurationSaved(object? sender, EmulationConfigurationSavedEventArgs args)
    {
        ConfigurationSaved?.Invoke(this, args);
        await ReloadConfigurationsAsync();
    }

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
                LocExtension.Get(row.Module.DisplayResourceKey),
                row.MachineName),
            LocExtension.Get("Common.Delete"),
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (answer != MessageBoxResult.Yes)
            return;

        try
        {
            await row.Module.DeleteConfigurationAsync(row.Configuration.Id);
            await ReloadConfigurationsAsync();
            var section = _moduleSections.FirstOrDefault(item =>
                ReferenceEquals(_moduleTabs[item.Key], row.Module)).Value;
            if (section is not null)
                await section.ReloadAfterConfigurationDeletedAsync(
                    row.Configuration.Id,
                    row.Configuration.MachineId);
        }
        catch (Exception error)
        {
            ControlErrorPresenter.ShowEmulation(
                this,
                error,
                ControlErrorContexts.EmulationConfigurationManagement,
                LocExtension.Get(row.Module.DisplayResourceKey));
        }
    }

    private async Task EditConfigurationAsync(EmulationConfigurationTableRow row)
    {
        var section = GetOrCreateModuleSection(row.Module);
        await section.EditConfigurationAsync(row.Configuration);
        _tabs.SelectedItem = _moduleTabs.First(entry =>
            ReferenceEquals(entry.Value, row.Module)).Key;
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
                module, LocExtension.Get(module.DisplayResourceKey)));
        _configurationBrand.SelectedItem = selectedModule is null
            ? null
            : _configurationBrands.FirstOrDefault(item =>
                ReferenceEquals(item.Module, selectedModule));
    }

}
