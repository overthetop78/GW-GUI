using GWGUI.App.Localization;
using GWGUI.Emulation;

namespace GWGUI.App.Controls;

public sealed partial class OptionsEmulationSection
{
    private async void ModuleConfigurationSaved(object? sender, EmulationConfigurationSavedEventArgs args)
    {
        ConfigurationSaved?.Invoke(this, args);
        await ReloadConfigurationsAsync();
    }

    private async Task ReloadConfigurationsAsync()
    {
        _configurations.Clear();
        foreach (var module in _modules)
        {
            var configurations = await module.LoadConfigurationsAsync();
            foreach (var configuration in configurations)
            {
                var machine = module.Machines.First(item => item.Id == configuration.MachineId);
                var display = $"{LocExtension.Get(machine.DisplayResourceKey)} · {configuration.Id.ToString("N")[..8]}";
                _configurations.Add(new EmulationConfigurationListItem(module, configuration, display));
            }
        }
    }

    private async Task DeleteSelectedConfigurationAsync()
    {
        if (_configurationList.SelectedItem is not EmulationConfigurationListItem selected) return;
        await selected.Module.DeleteConfigurationAsync(selected.Configuration.Id);
        await ReloadConfigurationsAsync();
    }
}
