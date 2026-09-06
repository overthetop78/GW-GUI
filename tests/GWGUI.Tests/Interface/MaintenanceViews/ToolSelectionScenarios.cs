using GWGUI.App.Services.Maintenance;
using GWGUI.App.Views.Controls.Tools;
using GWGUI.Domain.Commands.Building;
using GWGUI.Domain.Settings;
using System.Windows;
namespace GWGUI.Tests.Interface.MaintenanceViews;
internal static class ToolSelectionScenarios
{
    public static async Task Invalid(int field, string value)
    {
        var context = new ToolOperationScenarios.Context();
        var view = context.View;
        view.ToolsList.SelectedIndex = field == 0 ? 0 : 1;
        var enabled = field switch { 0 => view.EraseRevsEnabled, 1 => view.CleanCylindersEnabled, 2 => view.CleanPassesEnabled, _ => view.CleanLingerEnabled };
        var input = field switch { 0 => view.EraseRevsValue, 1 => view.CleanCylindersValue, 2 => view.CleanPassesValue, _ => view.CleanLingerValue };
        enabled.IsChecked = true; input.Text = value;
        context.Controller.UpdatePreview();
        Assert.Contains("Advanced.Invalid", Assert.Single(context.Previews));
        Assert.IsAssignableFrom<ArgumentException>(Assert.Single(context.Errors));
        await context.Execute(field != 0);
        Assert.Single(context.Messages); Assert.Empty(context.Commands); Assert.False(context.Operation.IsRunning);
        Assert.Equal(value, input.Text);
        input.Text = field == 3 ? "0" : "1";
        context.Controller.UpdatePreview(); Assert.DoesNotContain("Advanced.Invalid", context.Previews.Last());
        var running = context.Execute(field != 0); await context.WaitUntilStarted(running);
        context.Pending.SetResult(new(0, false, TimeSpan.Zero, [])); await running;
        Assert.Single(context.Commands); Assert.Equal("Status.Success", context.Model.OperationText);
        enabled.IsChecked = false; input.Text = value;
        context.Controller.UpdatePreview(); Assert.DoesNotContain("Advanced.Invalid", context.Previews.Last());
    }
    internal static MaintenanceToolsController Controller(ToolsTabSection section) => new(section, () => new AppSettings(), new GwCommandBuilder(), () => "virtual-device", () => "B", () => false, _ => throw new InvalidOperationException(), (key, _) => key);
    public static void Selection()
    {
        var section = new ToolsTabSection(); var controller = Controller(section);
        section.EraseTracksValue.Text = "c=0-3:h=1";
        section.CleanPassesValue.Text = "7";
        section.ToolsList.SelectedIndex = 0; controller.UpdateSelection();
        Assert.Equal(Visibility.Visible, section.ErasePanel.Visibility); Assert.Equal(Visibility.Collapsed, section.CleanPanel.Visibility);
        section.ToolsList.SelectedIndex = 1; controller.UpdateSelection();
        Assert.Equal(Visibility.Collapsed, section.ErasePanel.Visibility); Assert.Equal(Visibility.Visible, section.CleanPanel.Visibility);
        Assert.Equal("c=0-3:h=1", section.EraseTracksValue.Text); Assert.Equal("7", section.CleanPassesValue.Text);
    }
}
