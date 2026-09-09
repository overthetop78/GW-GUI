using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Contracts.Emulation.Machine;
using GWGUI.App.Controllers.Emulation.Options;
using GWGUI.App.Views.Controls.Emulation.Options;
using GWGUI.App.Views.Windows.EmulationModuleOptions;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Interfaces;
using GWGUI.Tests.Application.TestInfrastructure;
using GWGUI.Tests.Interface.EmulationViews;

namespace GWGUI.Tests.Interface.SettingsViews;

internal static class EmulationModuleSettingsNavigationScenarios
{
    internal static async Task VerticalMachinesPreserveConfigurationStateAndTabs()
    {
        var module = new MachineConfigurationScenarios.Module();
        module.Saved.Add(module.Service.CreateConfiguration("a"));
        var section = new EmulationModuleSettingsSection(module.Service);
        try
        {
            await section.ReloadWhenOpenedAsync();
            var machines = Assert.Single(MachineConfigurationScenarios.Controls<ListBox>(section));
            var choices = machines.Items.Cast<EmulationMachineChoice>().ToArray();
            Assert.Equal(2, choices.Length);
            Assert.True(choices.Single(choice => choice.Definition.Id == "a").HasSavedConfiguration);
            Assert.False(choices.Single(choice => choice.Definition.Id == "b").HasSavedConfiguration);
            Assert.Contains(machines.ItemContainerStyle.Triggers.OfType<DataTrigger>(), trigger =>
                Equals(trigger.Value, true));
            Assert.Equal(2, Assert.Single(MachineConfigurationScenarios.Controls<TabControl>(section)).Items.Count);
            machines.SelectedItem = choices.Single(choice => choice.Definition.Id == "b");
            await System.Windows.Threading.Dispatcher.Yield(
                System.Windows.Threading.DispatcherPriority.ContextIdle);
            Assert.Equal("b", section.CurrentConfiguration.MachineId);
        }
        finally { module.Cleanup(); }
    }

    internal static async Task CurrentEmulatorIsTheOnlyChoice()
    {
        var manager = ControlledDependencies.Simulate<IEmulationEmulatorManager>((method, _) => method.Name switch
        {
            "GetEmulatorInstallationAsync" => ValueTask.FromResult(
                new EmulationEmulatorInstallation("current-emulator", "1.0.0")),
            _ => throw new InvalidOperationException(method.Name)
        });
        var controller = new EmulationEmulatorManagementController(manager, () => "machine-a");
        var panel = Assert.IsType<EmulationCoreManagementPanel>(controller.CreateView());
        await controller.RefreshAsync();
        Assert.Equal("current-emulator", Assert.Single(panel.Emulators.Items.Cast<string>()));
        Assert.Equal(Visibility.Visible, panel.Installed.Visibility);
        Assert.Equal(Visibility.Collapsed, panel.Install.Visibility);
    }

    internal static void ModuleWindowUsesTheGenericSectionAndDynamicTitle()
    {
        var module = new MachineConfigurationScenarios.Module();
        var window = new EmulationModuleOptionsWindow(module.Service);
        try
        {
            Assert.IsType<EmulationModuleSettingsSection>(window.ModuleContent.Content);
            Assert.False(string.IsNullOrWhiteSpace(window.Title));
        }
        finally
        {
            window.Close();
            module.Cleanup();
        }
    }
}
