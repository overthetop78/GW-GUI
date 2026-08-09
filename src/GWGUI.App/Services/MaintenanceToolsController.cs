using System.Windows;
using GWGUI.App.Controls;
using GWGUI.App.Localization;
using GWGUI.Domain.Commands;
using GWGUI.Domain.Maintenance;
using GWGUI.Domain.Read;
using GWGUI.Domain.Settings;

namespace GWGUI.App.Services;

public sealed class MaintenanceToolsController(
    ToolsTabSection tools,
    Func<AppSettings> settings,
    IGwCommandBuilder commandBuilder,
    Func<string?> deviceArgument,
    Func<string?> driveArgument,
    Func<bool> isToolsTabSelected,
    Action<string> showCommand,
    Func<string, object[], string> localize)
{
    public void UpdateSelection()
    {
        tools.ErasePanel.Visibility = tools.ToolsList.SelectedIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
        tools.CleanPanel.Visibility = tools.ToolsList.SelectedIndex == 1 ? Visibility.Visible : Visibility.Collapsed;
        UpdatePreview();
    }

    public GwCommand BuildErase()
    {
        var options = new List<EnabledOption>();
        if (tools.EraseTracksEnabled.IsChecked == true) options.Add(new("--tracks", tools.EraseTracksValue.Text.Trim()));
        if (tools.EraseRevsEnabled.IsChecked == true) options.Add(new("--revs", tools.EraseRevsValue.Text.Trim()));
        return commandBuilder.BuildErase(new EraseRequest(
            settings().GwExecutablePath ?? "gw.exe",
            options,
            deviceArgument(),
            driveArgument(),
            tools.EraseExpertArguments.Text));
    }

    public GwCommand BuildClean() => commandBuilder.BuildClean(new CleanRequest(
        settings().GwExecutablePath ?? "gw.exe",
        tools.CleanCylindersEnabled.IsChecked == true && int.TryParse(tools.CleanCylindersValue.Text, out var cylinders) ? cylinders : null,
        tools.CleanPassesEnabled.IsChecked == true && int.TryParse(tools.CleanPassesValue.Text, out var passes) ? passes : null,
        tools.CleanLingerEnabled.IsChecked == true && int.TryParse(tools.CleanLingerValue.Text, out var linger) ? linger : null,
        deviceArgument(),
        driveArgument(),
        tools.CleanExpertArguments.Text));

    public void UpdatePreview()
    {
        if (!isToolsTabSelected()) return;
        try
        {
            showCommand((tools.ToolsList.SelectedIndex == 0 ? BuildErase() : BuildClean()).ToDisplayString());
        }
        catch (Exception exception)
        {
            ErrorLog.Write(exception, "Building maintenance preview");
            showCommand($"⚠ {localize("Advanced.Invalid", [localize("Common.Unknown", [])])}");
        }
    }
}
