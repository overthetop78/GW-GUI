using GWGUI.App.Constants.Localization;
using GWGUI.App.Contracts.Machine;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Views.Controls.Emulation.Machine;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
namespace GWGUI.Tests.Interface.EmulationViews;
internal static class EmulationInteractionScenarios
{
    public static void Commands(bool failure)
    {
        var host = new DockPanel(); var calls = new List<int>(); var errors = new List<Exception>(); var restored = 0;
        Func<Task> Command(int index) => () => { calls.Add(index); return failure ? Task.FromException(new IOException("synthetic command")) : Task.CompletedTask; };
        var bar = new MachineCommandBar(host,new(Command(0),Command(1),Command(2),Command(3),Command(4),Command(5),Command(6),Command(7),Command(8),Command(9)),[],errors.Add,() => restored++);
        var keys = new[] {EmulationResourceKeys.Power,EmulationResourceKeys.PauseResume,EmulationResourceKeys.SoftReset,EmulationResourceKeys.HardReset,EmulationResourceKeys.QuickSave,EmulationResourceKeys.QuickLoad,EmulationResourceKeys.CaptureScreen,EmulationResourceKeys.Fullscreen,EmulationResourceKeys.Audio,EmulationResourceKeys.SwitchControllerPointer};
        Button Find(string key) => MachineConfigurationScenarios.Controls<Button>(host).Single(x => Equals(x.ToolTip,LocExtension.Get(key)));
        bar.SetPowered(false);
        foreach(var key in keys) Assert.Equal(key == EmulationResourceKeys.Power || key == EmulationResourceKeys.Fullscreen,Find(key).IsEnabled);
        bar.SetPowered(true); bar.SetSavedStateAvailability(true,false);
        Assert.True(Find(EmulationResourceKeys.QuickSave).IsEnabled); Assert.False(Find(EmulationResourceKeys.QuickLoad).IsEnabled);
        bar.SetSavedStateAvailability(true,true); Assert.True(Find(EmulationResourceKeys.QuickLoad).IsEnabled);
        foreach(var key in keys) Find(key).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.Equal(Enumerable.Range(0,10),calls); Assert.Equal(10,restored); Assert.Equal(failure?10:0,errors.Count);
        var pause = Assert.IsType<TextBlock>(Find(EmulationResourceKeys.PauseResume).Content); var previous = pause.Text;
        bar.SetPaused(true); Assert.NotEqual(previous,pause.Text); bar.SetPaused(false); Assert.Equal(previous,pause.Text);
        bar.SetInputStatus(true,false);
        Assert.Equal(LocExtension.Get("Emulation.Value.Enabled"),AutomationProperties.GetItemStatus(bar.PointerStatus));
        Assert.Equal(LocExtension.Get("Emulation.Value.Disabled"),AutomationProperties.GetItemStatus(bar.ControllerStatus));
        bar.SetInputStatus(false,true);
        Assert.Equal(LocExtension.Get("Emulation.Value.Disabled"),AutomationProperties.GetItemStatus(bar.PointerStatus));
        Assert.Equal(LocExtension.Get("Emulation.Value.Enabled"),AutomationProperties.GetItemStatus(bar.ControllerStatus));
        bar.SetPowered(false); Assert.False(Find(EmulationResourceKeys.QuickSave).IsEnabled); Assert.False(Find(EmulationResourceKeys.QuickLoad).IsEnabled);
    }
}
