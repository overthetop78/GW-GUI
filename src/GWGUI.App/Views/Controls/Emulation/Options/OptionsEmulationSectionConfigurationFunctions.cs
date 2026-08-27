using GWGUI.App.Contracts.Emulation.Configurations;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Presenters.Emulation.Configurations;
using GWGUI.Emulation;

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
        _configurations.Clear();
        foreach (var (module, configurations) in loaded)
        {
            foreach (var configuration in configurations)
            {
                var display = EmulationConfigurationPresenter.DisplayName(module, configuration);
                _configurations.Add(new EmulationConfigurationListItem(module, configuration, display));
            }
        }
    }

    private async Task DeleteSelectedConfigurationAsync()
    {
        if (_configurationList.SelectedItem is not EmulationConfigurationListItem selected) return;
        await selected.Module.DeleteConfigurationAsync(selected.Configuration.Id);
        await ReloadConfigurationsAsync();
        var section = _moduleSections.FirstOrDefault(item =>
            ReferenceEquals(_moduleTabs[item.Key], selected.Module)).Value;
        if (section is not null) await section.ReloadWhenOpenedAsync();
    }
}
