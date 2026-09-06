using GWGUI.Domain.Commands;
using GWGUI.Domain.Commands.Building;
using GWGUI.Domain.Commands.Execution;
using GWGUI.Domain.Commands.Options;
using GWGUI.Domain.Maintenance;
using GWGUI.Domain.Settings;
using GWGUI.App.Interfaces.Services.Dialogs;
using GWGUI.App.Services.Logging;
using GWGUI.App.Services.Operations;
using GWGUI.App.Views.Controls.Tools;
using System.IO;
using System.Windows;

using GWGUI.Infrastructure.Processes;
using System.Windows.Controls;


namespace GWGUI.App.Services.Maintenance;

public sealed class MaintenanceToolsController(
    ToolsTabSection tools,
    Func<AppSettings> settings,
    IGwCommandBuilder commandBuilder,
    Func<string?> deviceArgument,
    Func<string?> driveArgument,
    Func<bool> isToolsTabSelected,
    Action<string> showCommand,
    Func<string, object[], string> localize,
    OperationRuntimeController? operation = null,
    IMessageDialogService? dialogs = null,
    Func<bool>? ensureHardware = null,
    Action? confirmStop = null,
    ConsoleLogSession? consoleLog = null,
    IGreaseweazleRunner? runner = null,
    TextBox? logOutput = null,
    Func<string?, bool>? fileExists = null,
    Action<Exception, string>? logError = null)
{
    private readonly Func<string?, bool> exists = fileExists ?? File.Exists;
    private readonly Action<Exception, string> writeError = logError ?? ((error, context) => ErrorLog.Write(error, context));
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
        GwOptionValidator.Validate(options);
        return commandBuilder.BuildErase(new EraseRequest(
            settings().GwExecutablePath ?? "gw.exe",
            options,
            deviceArgument(),
            driveArgument(),
            tools.EraseExpertArguments.Text));
    }

    public GwCommand BuildClean() => commandBuilder.BuildClean(new CleanRequest(
        settings().GwExecutablePath ?? "gw.exe",
        ParseOptional(tools.CleanCylindersEnabled, tools.CleanCylindersValue, 1),
        ParseOptional(tools.CleanPassesEnabled, tools.CleanPassesValue, 1),
        ParseOptional(tools.CleanLingerEnabled, tools.CleanLingerValue, 0),
        deviceArgument(),
        driveArgument(),
        tools.CleanExpertArguments.Text));

    private static int? ParseOptional(CheckBox enabled, TextBox input, int minimum)
    {
        if (enabled.IsChecked != true) return null;
        if (!int.TryParse(input.Text, out var value) || value < minimum)
            throw new ArgumentException("Invalid maintenance count or duration.");
        return value;
    }

    public void UpdatePreview()
    {
        if (!isToolsTabSelected()) return;
        try
        {
            showCommand((tools.ToolsList.SelectedIndex == 0 ? BuildErase() : BuildClean()).ToDisplayString());
        }
        catch (Exception exception)
        {
            writeError(exception, "Building maintenance preview");
            showCommand($"⚠ {localize("Advanced.Invalid", [localize("Common.Unknown", [])])}");
        }
    }

    public Task ExecuteEraseAsync() => ExecuteAsync(BuildErase, tools.EraseExecuteButton);
    public Task ExecuteCleanAsync() => ExecuteAsync(BuildClean, tools.CleanExecuteButton);

    private async Task ExecuteAsync(Func<GwCommand> buildCommand, Button button)
    {
        if (operation is null || dialogs is null || ensureHardware is null || confirmStop is null || consoleLog is null || runner is null || logOutput is null) return;
        if (operation.IsRunning) { confirmStop(); return; }
        if (!ensureHardware()) return;
        if (string.IsNullOrWhiteSpace(settings().GwExecutablePath) || !exists(settings().GwExecutablePath))
        { dialogs.Show(localize("App.GwNotConfigured", []), localize("App.Title", [])); return; }
        GwCommand command;
        try { command = buildCommand(); }
        catch (ArgumentException)
        {
            dialogs.Show(localize("Advanced.Invalid", [localize("Common.Unknown", [])]), localize("App.Title", []));
            return;
        }
        button.Content = localize("Common.Stop", []); operation.Begin(); await operation.RenderPendingAsync(); logOutput.Clear();
        await consoleLog.BeginAsync(command.Verb, command.ToDisplayString());
        var outcome = await operation.RunAsync(token => runner.RunAsync(command, new Progress<GwOutputLine>(operation.Report), token));
        await operation.FlushPendingAsync(); operation.Apply(operation.Present(outcome)); operation.End();
        button.Content = localize("Common.Execute", []);
    }
}
