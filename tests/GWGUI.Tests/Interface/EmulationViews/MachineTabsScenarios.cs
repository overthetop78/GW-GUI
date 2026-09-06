using GWGUI.App.Contracts.Emulation.Configurations;
using GWGUI.App.Views.Controls.Emulation.Machine;
using GWGUI.Emulation.Contracts;
using System.Windows;
using System.Windows.Controls;
namespace GWGUI.Tests.Interface.EmulationViews;
internal static class MachineTabsScenarios
{
    public static async Task Tabs()
    {
        var module = new MachineConfigurationScenarios.Module(); var section = new EmulationSection();
        var tabs = Assert.Single(MachineConfigurationScenarios.Controls<TabControl>(section));
        var first = new MachineConfigurationScenarios.Configuration(module.Id,Guid.NewGuid(),"a");
        var second = first with { Id = Guid.NewGuid(),MachineId = "b" };
        EmulationMachineRuntime Runtime(MachineConfigurationScenarios.Configuration configuration) => new(configuration,_ => throw new InvalidOperationException("No core may start"),[],[],"Emulation.Model",false);
        var stopped = new List<Guid>(); var firstView = new Grid(); var secondView = new Grid();
        await section.AddMachineAsync(new(module.Service,first,"first"),Runtime(first),firstView,() => { stopped.Add(first.Id); return Task.CompletedTask; });
        var firstTab = Assert.IsType<TabItem>(tabs.SelectedItem); Assert.Same(firstView,tabs.SelectedContent);
        await section.AddMachineAsync(new(module.Service,second,"second"),Runtime(second),secondView,() => { stopped.Add(second.Id); return Task.CompletedTask; });
        var secondTab = Assert.IsType<TabItem>(tabs.SelectedItem); Assert.Same(secondView,tabs.SelectedContent); Assert.Equal(3,tabs.Items.Count);
        tabs.SelectedItem = firstTab; Assert.Same(firstView,tabs.SelectedContent);
        var close = Assert.Single(MachineConfigurationScenarios.Controls<Button>((DependencyObject)secondTab.Header));
        close.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.Equal(new[] {second.Id},stopped); Assert.Equal(2,tabs.Items.Count); Assert.Same(firstView,tabs.SelectedContent);
        close.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); Assert.Single(stopped);
        await section.CloseMachineAsync((module.Id,first.Id),firstTab,() => throw new IOException("synthetic stop"))
            .ContinueWith(task => Assert.IsType<IOException>(task.Exception!.InnerException));
        Assert.Contains(firstTab,tabs.Items.Cast<TabItem>());
        await section.CloseMachineAsync((module.Id,first.Id),firstTab,() => Task.CompletedTask);
        Assert.Single(tabs.Items); Assert.DoesNotContain(firstTab,tabs.Items.Cast<TabItem>());
    }
}
