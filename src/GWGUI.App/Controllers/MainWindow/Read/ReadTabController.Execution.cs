using GWGUI.Infrastructure.Commands;
using GWGUI.Infrastructure.Commands.Building;
using GWGUI.Infrastructure.Commands.Execution;
using GWGUI.MediaEngine.Formats;
using GWGUI.Infrastructure.Naming;
using GWGUI.App.Profiles;
using GWGUI.Infrastructure.Read;
using GWGUI.Infrastructure.Settings;
using GWGUI.Infrastructure.Settings.Engines;
using GWGUI.App.Constants.Services.PhysicalDiskReading;
using GWGUI.App.Constants.Views.Read;
using GWGUI.App.Contracts.Services.Hardware;
using GWGUI.App.Contracts.Services.PhysicalDiskReading;
using GWGUI.App.Enums.Services.Dialogs;
using GWGUI.App.Functions.Services.Conversion;
using GWGUI.App.Functions.Services.PhysicalDiskReading;
using GWGUI.App.Functions.Services.PhysicalDiskWriting;
using GWGUI.App.Interfaces.Services.Dialogs;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Functions.Localization;
using GWGUI.App.Services.DiskImages;
using GWGUI.App.Services.Logging;
using GWGUI.App.Services.Operations;
using GWGUI.App.Services.PhysicalDiskReading;
using GWGUI.App.Services.Profiles;
using GWGUI.App.ViewModels.Main;
using GWGUI.App.Views.Controls.Read;
using GWGUI.App.Views.Windows.Shell;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.IO;
using GWGUI.Infrastructure.Processes;


namespace GWGUI.App.Controllers.MainWindow;

internal sealed partial class ReadTabController
{
    internal async Task ExecuteAsync()
    {
        if (operation.IsRunning) { confirmAndRequestStop(); return; }
        if (!ensureSelectedHardwareAvailable()) return;
        if (!diskDefinitionsController.Validate(view.AdvancedBlock.DiskDefinitionsEnabled, view.AdvancedBlock.DiskDefinitionsValue, LocExtension.Get("Read.Title"))) return;
        var fileName = view.FileNameBlock.FileNameTextBox;
        if (string.IsNullOrWhiteSpace(fileName.Text))
        {
            dialogs.Show(LocExtension.Get("Read.NameRequired"), LocExtension.Get("Read.Title"), icon: UserDialogIcon.Information);
            return;
        }
        if (!UsesInternalPhysicalRead && (string.IsNullOrWhiteSpace(settings().GwExecutablePath) || !exists(settings().GwExecutablePath)))
        {
            dialogs.Show(LocExtension.Get("App.GwNotConfigured"), LocExtension.Get("App.Title"), icon: UserDialogIcon.Information);
            return;
        }

        var extension = GetExtension();
        if (string.IsNullOrWhiteSpace(extension))
        {
            dialogs.Show(LocExtension.Get("Read.TypeRequired"), LocExtension.Get("Read.Title"), icon: UserDialogIcon.Information);
            return;
        }
        var target = GetTarget(extension);
        if (exists(target))
        {
            var choice = businessDialogs.ResolveReadConflict(target);
            if (choice is null or ReadConflictChoice.EditName)
            {
                if (selectOutputName is not null) selectOutputName();
                else { fileName.Focus(); fileName.SelectAll(); }
                return;
            }
            if (choice == ReadConflictChoice.UseNextNumber)
            {
                var advanced = view.AdvancedBlock;
                if (advanced.AutoNumberCheckBox.IsChecked != true) advanced.AutoNumberCheckBox.IsChecked = true;
                var sequenceKind = advanced.SequenceKindComboBox.SelectedIndex == 1 ? SequenceKind.Alphabetic : SequenceKind.Numeric;
                if (!SequenceFormatter.TryParse(advanced.SequenceValueTextBox.Text, sequenceKind, out var next)) next = sequenceKind == SequenceKind.Alphabetic ? 0 : 1;
                var available = OutputConflictResolver.FindNextAvailableWithValue(
                    view.FolderBlock.Input.Text,
                    fileName.Text.Trim(),
                    extension,
                    sequenceKind,
                    advanced.SequenceWidthComboBox.SelectedIndex + 1,
                    next, exists);
                target = available.Path;
                viewModel.Read.SequenceValue = sequenceKind == SequenceKind.Numeric
                    ? available.Value.ToString()
                    : SequenceFormatter.Format(available.Value, sequenceKind, 1);
            }
        }

        if (UsesInternalPhysicalRead)
        {
            var hardware = selectedHardware();
            if (hardware is null)
            {
                dialogs.Show(LocExtension.Get("Hardware.NotConfigured"), LocExtension.Get("Read.Title"), icon: UserDialogIcon.Warning);
                return;
            }
            if (RawScpRadio.IsChecked != true)
            {
                dialogs.Show(LocExtension.Get("Read.InternalRawScpOnly"), LocExtension.Get("Read.Title"), icon: UserDialogIcon.Warning);
                return;
            }
            if (HasUnsupportedInternalOptions())
            {
                dialogs.Show(LocExtension.Get("Read.InternalUnsupportedOptions"), LocExtension.Get("Read.Title"), icon: UserDialogIcon.Warning);
                return;
            }
            PhysicalDiskReadOptions options;
            try { options = CreateInternalOptions(hardware); }
            catch (Exception exception) when (exception is ArgumentException or OverflowException)
            {
                dialogs.Show(LocExtension.Get("Read.InternalInvalidOptions"), LocExtension.Get("Read.Title"), icon: UserDialogIcon.Warning);
                return;
            }
            await ExecuteInternalAsync(options, target);
            return;
        }

        GwCommand command;
        try { command = BuildCommand(target); }
        catch (ArgumentException) { diskDefinitionsController.ShowInvalid(LocExtension.Get("Read.Title")); return; }
        view.ExecuteActionButton.Content = LocExtension.Get("Common.Stop");
        operation.Begin();
        view.CompletionBlock.BeginCapture(target);
        await operation.RenderPendingAsync();
        logOutput.Clear();
        await consoleLog.BeginAsync(ReadTabConstants.ExternalReadLogId, command.ToDisplayString());
        var output = new Progress<GwOutputLine>(operation.Report);
        var outcome = await operation.RunAsync(token => runner.RunAsync(command, output, token));
        await operation.FlushPendingAsync();
        operation.Apply(operation.Present(outcome));
        if (outcome.Result is { } result)
        {
            if (result.WasCancelled)
                HandleCancelledOutput(target, true, ReadTabConstants.CancelledReadDeletionContext);
            if (result.IsSuccess)
            {
                diskImageWorkspace.LastCapturedPath = target;
                var summary = extension.Equals(ReadTabConstants.ScpExtension, StringComparison.OrdinalIgnoreCase)
                    ? await AppendScpCaptureSummaryAsync(target)
                    : string.Empty;
                view.CompletionBlock.CompleteCapture(target, summary);
            }
            if (result.IsSuccess) viewModel.Read.TryAdvanceSequence();
        }
        if (outcome.Result?.IsSuccess != true)
            view.CompletionBlock.DiscardCapture(target);
        operation.End();
        view.ExecuteActionButton.Content = LocExtension.Get("Common.Execute");
    }

    private async Task ExecuteInternalAsync(PhysicalDiskReadOptions options, string target)
    {
        view.ExecuteActionButton.Content = LocExtension.Get("Common.Stop");
        operation.Begin();
        view.CompletionBlock.BeginCapture(target);
        await operation.RenderPendingAsync();
        logOutput.Clear();
        await consoleLog.BeginAsync(ReadTabConstants.InternalReadLogId, LocExtension.Get("Read.InternalPreview", target));
        var stopwatch = Stopwatch.StartNew();
        PhysicalDiskReadResult? capture = null;
        lastInternalReadProgressLine = null;
        var outcome = await operation.RunAsync(async token =>
        {
            var reader = createInternalReader();
            var readProgress = new Progress<PhysicalDiskReadOperationProgress>(ReportInternalProgress);
            capture = await reader.ReadAsync(options, target, readProgress, token);
            return new GwExecutionResult(0, false, stopwatch.Elapsed, []);
        });
        await operation.FlushPendingAsync();
        operation.Apply(operation.Present(outcome));
        if (outcome.Result is { WasCancelled: true })
            HandleCancelledOutput(target, false, ReadTabConstants.CancelledInternalReadDeletionContext);
        if (outcome.Result?.IsSuccess == true && capture is not null)
        {
            diskImageWorkspace.RememberReadImage(capture.Document);
            diskImageWorkspace.LastCapturedPath = target;
            var summary = Path.GetExtension(target).Equals(ReadTabConstants.ScpExtension, StringComparison.OrdinalIgnoreCase)
                ? await AppendScpCaptureSummaryAsync(target)
                : string.Empty;
            view.CompletionBlock.CompleteCapture(target, summary);
            viewModel.Read.TryAdvanceSequence();
        }
        else
        {
            view.CompletionBlock.DiscardCapture(target);
        }
        operation.End();
        view.ExecuteActionButton.Content = LocExtension.Get("Common.Execute");
    }

    private void ReportInternalProgress(PhysicalDiskReadOperationProgress readProgress)
    {
        progress.Accept(readProgress);
        var line = readProgress.Cylinder is int cylinder && readProgress.Head is int head
            ? LocExtension.Get("Status.TrackProgress", cylinder, head, readProgress.CompletedTracks, readProgress.TotalTracks)
            : LocExtension.Get("Status.Running");
        if (string.Equals(line, lastInternalReadProgressLine, StringComparison.Ordinal)) return;
        lastInternalReadProgressLine = line;
        operation.AppendText(line + Environment.NewLine);
    }

    private void HandleCancelledOutput(string target, bool showDialog, string logContext)
    {
        var deletionError = deleteOutput(target);
        if (deletionError is null)
        {
            operation.AppendText(Environment.NewLine + LocExtension.Get("Read.CancelledFileDeleted", target) + Environment.NewLine);
            return;
        }
        ErrorLog.Write(deletionError, logContext);
        var detail = ExceptionDescriptionFunctions.Describe(deletionError);
        var message = LocExtension.Get("Read.CancelledFileDeleteFailed", target, detail);
        operation.AppendText(Environment.NewLine + message + Environment.NewLine);
        if (showDialog) dialogs.Show(message, LocExtension.Get("Read.Title"), icon: UserDialogIcon.Warning);
    }

    private async Task<string> AppendScpCaptureSummaryAsync(string path)
    {
        try
        {
            var info = await captureInfo(path);
            var checksum = LocExtension.Get(info.ChecksumValid ? "Visual.ChecksumValid" : "Visual.ChecksumInvalid");
            operation.AppendText(Environment.NewLine + LocExtension.Get("Read.ScpSummaryTitle") + Environment.NewLine);
            operation.AppendText(LocExtension.Get("Read.ScpTracksSummary", info.CapturedTracks, info.MissingTracks, info.Cylinders, info.Sides) + Environment.NewLine);
            operation.AppendText(LocExtension.Get("Read.ScpTechnicalSummary", info.Header.Revolutions, info.Header.ResolutionNanoseconds, info.FileSize, checksum) + Environment.NewLine);
            operation.AppendText(LocExtension.Get("Read.ScpOutputFile", path) + Environment.NewLine);
            var summary = LocExtension.Get("Read.ScpBannerSummary", info.CapturedTracks, info.MissingTracks, info.Cylinders, info.Sides, info.Header.Revolutions, info.FileSize, checksum);
            logOutput.ScrollToEnd();
            return summary;
        }
        catch (Exception exception)
        {
            reportError(exception, ReadTabConstants.ScpSummaryReadContext);
            var detail = ExceptionDescriptionFunctions.Describe(exception);
            var summary = LocExtension.Get("Read.ScpSummaryUnavailable", detail);
            operation.AppendText(Environment.NewLine + summary + Environment.NewLine);
            return summary;
        }
    }

}
