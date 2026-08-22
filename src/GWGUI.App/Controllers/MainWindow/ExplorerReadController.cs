using GWGUI.Domain.Commands.Building;
using GWGUI.Domain.Commands.Execution;
using GWGUI.Domain.Read;
using GWGUI.Domain.Settings;
using GWGUI.Domain.Settings.Engines;
using GWGUI.App.Contracts.Services.PhysicalDiskReading;
using GWGUI.App.Contracts.ViewModels.Operations;
using GWGUI.App.Enums.Services.Dialogs;
using GWGUI.App.Functions.Services.PhysicalDiskReading;
using GWGUI.App.Functions.Services.PhysicalDiskWriting;
using GWGUI.App.Interfaces.Services.Dialogs;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Services.Hardware;
using GWGUI.App.Services.Operations;
using GWGUI.App.Services.PhysicalDiskReading;
using GWGUI.App.Views.Controls.Explorer;
using GWGUI.App.Views.Windows.Shell;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;
using GWGUI.Infrastructure.Processes;

namespace GWGUI.App.Controllers.MainWindow;

internal sealed class ExplorerReadController(
    ExplorerSection explorer,
    TextBox output,
    AppSettings settings,
    IMessageDialogService dialogs,
    HardwareSelectionController hardwareSelection,
    OperationRuntimeController operation,
    OperationProgressController progressController,
    IGwCommandBuilder commandBuilder,
    IGreaseweazleRunner runner,
    ConsoleLogSession consoleLog,
    Func<string, Task> loadImage,
    Action<Exception, string, string, string> showLoggedError)
{
    private string? _lastInternalProgressLine;

    internal async Task ExecuteAsync()
    {
        if (operation.IsRunning)
        {
            ConfirmAndRequestStop();
            return;
        }
        if (!hardwareSelection.EnsureAvailable()) return;

        var usesInternalRead = settings.Engines.ExplorerRead == OperationEngine.Internal;
        if (!usesInternalRead &&
            (string.IsNullOrWhiteSpace(settings.GwExecutablePath) || !File.Exists(settings.GwExecutablePath)))
        {
            dialogs.Show(LocExtension.Get("App.GwNotConfigured"), LocExtension.Get("App.Title"),
                icon: UserDialogIcon.Information);
            return;
        }

        var selectedDrive = hardwareSelection.Selected?.Label ?? LocExtension.Get("Hardware.NotConfigured");
        if (dialogs.Show(
                LocExtension.Get("Explorer.ReadDiskConfirm", selectedDrive),
                LocExtension.Get("Explorer.ReadDiskConfirmTitle"),
                UserDialogButtons.YesNo,
                UserDialogIcon.Question) != UserDialogResult.Yes)
            return;

        var temporaryDirectory = Path.Combine(Path.GetTempPath(), "GW GUI");
        Directory.CreateDirectory(temporaryDirectory);
        var temporaryPath = Path.Combine(temporaryDirectory, $"explorer-{Guid.NewGuid():N}.scp");
        var command = usesInternalRead
            ? null
            : commandBuilder.BuildRead(new ReadRequest(
                settings.GwExecutablePath!,
                temporaryPath,
                ReadResultKind.RawScp,
                null,
                [],
                hardwareSelection.DeviceArgument(),
                hardwareSelection.DriveArgument()));

        try
        {
            explorer.SetReadDiskRunning(true);
            operation.Begin();
            await operation.RenderPendingAsync();
            output.Clear();
            await consoleLog.BeginAsync(
                usesInternalRead ? "read-explorer-internal" : "read-explorer",
                usesInternalRead
                    ? LocExtension.Get("Read.InternalPreview", temporaryPath)
                    : command!.ToDisplayString());
            var outcome = usesInternalRead
                ? await ExecuteInternalAsync(temporaryPath)
                : await operation.RunAsync(token => runner.RunAsync(
                    command!, new Progress<GwOutputLine>(operation.Report), token));
            await operation.FlushPendingAsync();
            operation.Apply(operation.Present(outcome));
            if (outcome.Result?.IsSuccess == true && File.Exists(temporaryPath))
                await loadImage(temporaryPath);
        }
        catch (Exception exception)
        {
            showLoggedError(exception, "Reading disk into Explorer", "Tab.Explorer", "Explorer.LoadFailed");
        }
        finally
        {
            operation.End();
            explorer.SetReadDiskRunning(false);
            try
            {
                if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
            }
            catch
            {
            }
        }
    }

    private async Task<OperationOutcome<GwExecutionResult>> ExecuteInternalAsync(string temporaryPath)
    {
        var hardware = hardwareSelection.Selected;
        if (hardware is null)
            return new(false, null,
                new InvalidOperationException(LocExtension.Get("Hardware.NotConfigured")));

        var selection = GreaseweazleDriveSelectionFunctions.Resolve(hardware.Drive.Selection);
        var options = new PhysicalDiskReadOptions(
            hardware.Port,
            selection.BusType,
            selection.Unit,
            PhysicalDiskTrackSelectionParser.Parse("c=0-79:h=0-1"),
            ScpCaptureDiskTypeFunctions.Resolve(hardware.Drive.Density));
        var stopwatch = Stopwatch.StartNew();
        _lastInternalProgressLine = null;
        return await operation.RunAsync(async token =>
        {
            var progress = new Progress<PhysicalDiskReadOperationProgress>(ReportInternalProgress);
            await InternalPhysicalDiskReader.CreateDefault().ReadAsync(
                options, temporaryPath, progress, token);
            return new GwExecutionResult(0, false, stopwatch.Elapsed, []);
        });
    }

    private void ReportInternalProgress(PhysicalDiskReadOperationProgress progress)
    {
        progressController.Accept(progress);
        var line = progress.Cylinder is int cylinder && progress.Head is int head
            ? LocExtension.Get(
                "Status.TrackProgress",
                cylinder,
                head,
                progress.CompletedTracks,
                progress.TotalTracks)
            : LocExtension.Get("Status.Running");
        if (string.Equals(line, _lastInternalProgressLine, StringComparison.Ordinal)) return;

        _lastInternalProgressLine = line;
        operation.AppendText(line + Environment.NewLine);
    }

    private void ConfirmAndRequestStop()
    {
        if (dialogs.Show(
                LocExtension.Get("Operation.StopConfirm"),
                LocExtension.Get("Operation.StopTitle"),
                UserDialogButtons.YesNo,
                UserDialogIcon.Warning) == UserDialogResult.Yes)
            operation.RequestCancellation();
    }
}
