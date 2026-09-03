using GWGUI.Domain.Commands;
using GWGUI.Domain.Commands.Building;
using GWGUI.Domain.Commands.Execution;
using GWGUI.Domain.Formats;
using GWGUI.Domain.Naming;
using GWGUI.Domain.Profiles;
using GWGUI.Domain.Read;
using GWGUI.Domain.Settings;
using GWGUI.Domain.Settings.Engines;
using GWGUI.App.Constants.Services.PhysicalDiskReading;
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

internal sealed class ReadTabController(
    ReadTabSection view,
    MainWindowViewModel viewModel,
    OperationProfileController profileController,
    Func<IImageFormatCatalog> formatCatalog,
    Func<AppSettings> settings,
    IGwCommandBuilder commandBuilder,
    IFileDialogService fileDialogs,
    IBusinessDialogService businessDialogs,
    IMessageDialogService dialogs,
    DiskDefinitionsController diskDefinitionsController,
    OperationRuntimeController operation,
    OperationProgressController progress,
    ConsoleLogSession consoleLog,
    IGreaseweazleRunner runner,
    DiskImageWorkspaceController diskImageWorkspace,
    TextBox commandPreview,
    TextBox logOutput,
    Func<string?> selectedDeviceArgument,
    Func<string?> selectedDriveArgument,
    Func<bool> ensureSelectedHardwareAvailable,
    Func<HardwareChoice?> selectedHardware,
    Action confirmAndRequestStop,
    Action updateProfileStatus,
    Action updateReadCommand)
{
    private ComboBox ProfileCombo => view.ProfileBlock.ProfileCombo;
    private RadioButton RawScpRadio => view.ImageBlock.RawScpRadio;
    private RadioButton KnownFormatRadio => view.ImageBlock.KnownFormatRadio;
    private ComboBox FormatCombo => view.ImageBlock.FormatCombo;
    private ComboBox FamilyCombo => view.ImageBlock.FamilyCombo;
    private ComboBox ExtensionCombo => view.ImageBlock.ExtensionCombo;
    private Grid KnownFormatPanel => view.ImageBlock.KnownFormatPanel;
    private TextBox ExtensionText => view.FileNameBlock.ExtensionTextBox;
    private TextBlock NamePreview => view.AdvancedBlock.NamePreviewTextBlock;

    private bool UsesInternalPhysicalRead => settings().Engines.PhysicalRead == OperationEngine.Internal;
    private string? lastInternalReadProgressLine;

    internal void RefreshProfiles(string? selectedId = null)
        => profileController.Refresh(ProfileCombo, OperationKind.Read, selectedId);

    internal void ProfileChanged()
    {
        if (ProfileCombo.SelectedItem is not OperationProfile profile || view.AdvancedBlock.RevsEnabledCheckBox is null)
            return;

        ApplyProfile(profile);
        updateProfileStatus();
    }

    internal void ResetProfile()
    {
        if (ProfileCombo.SelectedItem is OperationProfile profile)
            ApplyProfile(profile);
    }

    internal void SaveProfile()
    {
        var enabled = viewModel.Read.CaptureEnabledOptions();
        var values = viewModel.Read.CaptureValues();
        values["result"] = RawScpRadio.IsChecked == true ? "raw" : "known";
        if (FormatCombo.SelectedItem is DiskFormat format) values["format"] = format.Id;
        if (ExtensionCombo.SelectedItem is ImageExtension extension) values["extension"] = extension.Extension;
        if (!string.IsNullOrWhiteSpace(viewModel.Read.Folder)) values["folder"] = viewModel.Read.Folder;

        var profile = profileController.Save(OperationKind.Read, name =>
            new OperationProfile(Guid.NewGuid().ToString("N"), OperationKind.Read, name, values, enabled));
        if (profile is not null)
            RefreshProfiles(profile.Id);
    }

    internal void InputChanged() => UpdateCommand();

    internal void ModeChanged()
    {
        KnownFormatPanel.Visibility = KnownFormatRadio.IsChecked == true
            ? Visibility.Visible
            : Visibility.Collapsed;
        UpdateExtension();
        UpdateCommand();
    }

    internal void FamilyChanged()
    {
        if (FormatCombo is null || FamilyCombo.SelectedItem is not string family) return;
        FormatCombo.ItemsSource = formatCatalog().Formats.Where(x => x.Family == family).ToArray();
        FormatCombo.SelectedIndex = 0;
    }

    internal void FormatChanged()
    {
        if (ExtensionCombo is null) return;
        ExtensionCombo.ItemsSource = (FormatCombo.SelectedItem as DiskFormat)?.Extensions;
        var extensions = ExtensionCombo.ItemsSource as IReadOnlyList<ImageExtension>;
        ExtensionCombo.SelectedIndex = extensions is null
            ? -1
            : Math.Max(0, extensions.ToList().FindIndex(x => x.IsDefault));
        UpdateExtension();
        UpdateCommand();
    }

    internal void UpdateCommand()
    {
        var extension = GetExtension();
        var target = GetTarget(extension);
        NamePreview.Text = Path.GetFileName(target);
        if (UsesInternalPhysicalRead)
        {
            commandPreview.Text = LocExtension.Get("Read.InternalPreview", target);
            return;
        }

        try
        {
            commandPreview.Text = BuildCommand(target).ToDisplayString();
        }
        catch (ArgumentException)
        {
            commandPreview.Text = $"⚠ {LocExtension.Get("Advanced.Invalid", LocExtension.Get("Common.Unknown"))}";
        }
    }

    internal string GetTarget(string extension)
        => viewModel.Read.BuildTarget(extension, "Exemple");

    internal string GetExtension()
        => RawScpRadio.IsChecked == true
            ? ".scp"
            : (ExtensionCombo.SelectedItem as ImageExtension)?.Extension ?? "";

    internal GwCommand BuildCommand(string target)
    {
        return commandBuilder.BuildRead(new ReadRequest(
            settings().GwExecutablePath ?? "gw.exe",
            target,
            RawScpRadio.IsChecked == true ? ReadResultKind.RawScp : ReadResultKind.KnownFormat,
            (FormatCombo.SelectedItem as DiskFormat)?.Id,
            viewModel.Read.BuildOptions(),
            selectedDeviceArgument(),
            selectedDriveArgument(),
            viewModel.Read.ExpertArguments));
    }

    internal void CopyName()
    {
        var fileName = view.FileNameBlock.FileNameTextBox.Text;
        if (!string.IsNullOrEmpty(fileName))
            Clipboard.SetText(fileName);
    }

    internal void BrowseFolder()
    {
        var folder = view.FolderBlock.Input;
        var path = fileDialogs.SelectFolder(new(LocExtension.Get("Read.DestinationFolder"), folder.Text));
        if (path is null) return;

        viewModel.Read.Folder = path;
        UpdateCommand();
    }

    internal void EnableFakeIndex()
    {
        viewModel.Read.EnableFakeIndex();
        UpdateCommand();
    }

    internal void ChangeSequenceKind()
    {
        var sequenceKind = view.AdvancedBlock.SequenceKindComboBox;
        var sequenceValue = view.AdvancedBlock.SequenceValueTextBox;
        var targetKind = sequenceKind.SelectedIndex == 1 ? SequenceKind.Alphabetic : SequenceKind.Numeric;
        var sourceKind = targetKind == SequenceKind.Alphabetic ? SequenceKind.Numeric : SequenceKind.Alphabetic;
        if (SequenceFormatter.TryParse(sequenceValue.Text, sourceKind, out var value))
            viewModel.Read.SequenceValue = targetKind == SequenceKind.Numeric
                ? (value + 1).ToString()
                : SequenceFormatter.Format(Math.Max(0, value - 1), targetKind, 1);
        UpdateCommand();
    }

    internal void EnableHardSectors()
    {
        viewModel.Read.EnableHardSectors();
        UpdateCommand();
    }

    internal void EnableDensel()
    {
        viewModel.Read.EnableDensel();
        UpdateCommand();
    }

    internal void EnableTg43()
    {
        viewModel.Read.EnableTg43();
        UpdateCommand();
    }

    internal void RestoreSettings()
    {
        var readSettings = settings().Read;
        KnownFormatRadio.IsChecked = readSettings.UseKnownFormat;
        RawScpRadio.IsChecked = !readSettings.UseKnownFormat;
        SelectFormat(readSettings.FormatId, readSettings.ImageExtension);
        viewModel.Read.AutoNumber = readSettings.AutoNumber;
        viewModel.Read.SequenceKindIndex = readSettings.SequenceKind == "Alphabetic" ? 1 : 0;
        viewModel.Read.SequenceWidthIndex = Math.Clamp(readSettings.SequenceWidth - 1, 0, 2);
        viewModel.Read.SequenceValue = readSettings.SequenceKind == "Alphabetic"
            ? SequenceFormatter.Format(readSettings.NextSequence, SequenceKind.Alphabetic, 1)
            : readSettings.NextSequence.ToString();
        viewModel.Read.ApplyOptions(readSettings.EnabledOptions, readSettings.OptionValues);
    }

    internal void CaptureSettings()
    {
        var readSettings = settings().Read;
        readSettings.UseKnownFormat = KnownFormatRadio.IsChecked == true;
        readSettings.FormatId = (FormatCombo.SelectedItem as DiskFormat)?.Id;
        readSettings.ImageExtension = (ExtensionCombo.SelectedItem as ImageExtension)?.Extension;
        readSettings.AutoNumber = viewModel.Read.AutoNumber;
        readSettings.SequenceKind = viewModel.Read.SequenceKind == SequenceKind.Alphabetic ? "Alphabetic" : "Numeric";
        readSettings.SequenceWidth = viewModel.Read.SequenceWidthIndex + 1;
        if (SequenceFormatter.TryParse(viewModel.Read.SequenceValue, viewModel.Read.SequenceKind, out var sequence))
            readSettings.NextSequence = sequence;
        readSettings.EnabledOptions = viewModel.Read.CaptureEnabledOptions();
        readSettings.OptionValues = viewModel.Read.CaptureValues();
    }

    internal PhysicalDiskReadOptions CreateInternalOptions(HardwareChoice hardware)
    {
        var selection = GreaseweazleDriveSelectionFunctions.Resolve(hardware.Drive.Selection);
        var tracks = PhysicalDiskTrackSelectionParser.Parse(
            viewModel.Read.Tracks.Enabled ? viewModel.Read.Tracks.Value : "c=0-79:h=0-1");
        var revolutions = viewModel.Read.Revs.Enabled
            ? int.Parse(viewModel.Read.Revs.Value)
            : PhysicalDiskReadDefaults.Revolutions;
        var retries = viewModel.Read.Retries.Enabled
            ? int.Parse(viewModel.Read.Retries.Value)
            : PhysicalDiskReadDefaults.FluxOverflowRetries;
        var seekRetries = viewModel.Read.SeekRetries.Enabled
            ? int.Parse(viewModel.Read.SeekRetries.Value)
            : PhysicalDiskReadDefaults.SeekRetries;
        TimeSpan? fakeIndex = viewModel.Read.FakeIndex.Enabled
            ? PhysicalDiskIndexPeriodParser.Parse(viewModel.Read.FakeIndex.Value)
            : null;
        return new PhysicalDiskReadOptions(
            hardware.Port,
            selection.BusType,
            selection.Unit,
            tracks,
            ScpCaptureDiskTypeFunctions.Resolve(hardware.Drive.Density),
            revolutions,
            retries,
            seekRetries,
            fakeIndex,
            viewModel.Read.HardSectors.Enabled);
    }

    internal bool HasUnsupportedInternalOptions() =>
        viewModel.Read.AdjustSpeed.Enabled ||
        viewModel.Read.Pll.Enabled ||
        viewModel.Read.Reverse.Enabled ||
        viewModel.Read.Densel.Enabled ||
        viewModel.Read.Tg43.Enabled ||
        viewModel.Read.DiskDefs.Enabled ||
        !string.IsNullOrWhiteSpace(viewModel.Read.ExpertArguments);

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
        if (!UsesInternalPhysicalRead && (string.IsNullOrWhiteSpace(settings().GwExecutablePath) || !File.Exists(settings().GwExecutablePath)))
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
        if (File.Exists(target))
        {
            var choice = businessDialogs.ResolveReadConflict(target);
            if (choice is null or ReadConflictChoice.EditName) { fileName.Focus(); fileName.SelectAll(); return; }
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
                    next);
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
        await operation.RenderPendingAsync();
        logOutput.Clear();
        await consoleLog.BeginAsync("read", command.ToDisplayString());
        var output = new Progress<GwOutputLine>(operation.Report);
        var outcome = await operation.RunAsync(token => runner.RunAsync(command, output, token));
        await operation.FlushPendingAsync();
        operation.Apply(operation.Present(outcome));
        if (outcome.Result is { } result)
        {
            if (result.WasCancelled)
                HandleCancelledOutput(target, true, "Deleting cancelled read output");
            if (result.IsSuccess && extension.Equals(".scp", StringComparison.OrdinalIgnoreCase))
            {
                diskImageWorkspace.LastCapturedPath = target;
                view.CompletionBlock.Visibility = Visibility.Visible;
                await AppendScpCaptureSummaryAsync(target);
            }
            if (result.IsSuccess && File.Exists(target))
                await AnalyzeCompletedAsync(target);
            if (result.IsSuccess) viewModel.Read.TryAdvanceSequence();
        }
        operation.End();
        view.ExecuteActionButton.Content = LocExtension.Get("Common.Execute");
    }

    private async Task ExecuteInternalAsync(PhysicalDiskReadOptions options, string target)
    {
        view.ExecuteActionButton.Content = LocExtension.Get("Common.Stop");
        operation.Begin();
        await operation.RenderPendingAsync();
        logOutput.Clear();
        await consoleLog.BeginAsync("read-internal", LocExtension.Get("Read.InternalPreview", target));
        var stopwatch = Stopwatch.StartNew();
        PhysicalDiskReadResult? capture = null;
        lastInternalReadProgressLine = null;
        var outcome = await operation.RunAsync(async token =>
        {
            var reader = InternalPhysicalDiskReader.CreateDefault();
            var readProgress = new Progress<PhysicalDiskReadOperationProgress>(ReportInternalProgress);
            capture = await reader.ReadAsync(options, target, readProgress, token);
            return new GwExecutionResult(0, false, stopwatch.Elapsed, []);
        });
        await operation.FlushPendingAsync();
        operation.Apply(operation.Present(outcome));
        if (outcome.Result is { WasCancelled: true })
            HandleCancelledOutput(target, false, "Deleting cancelled internal read output");
        if (outcome.Result?.IsSuccess == true && capture is not null)
        {
            diskImageWorkspace.RememberReadImage(capture.Document);
            diskImageWorkspace.LastCapturedPath = target;
            view.CompletionBlock.Visibility = Visibility.Visible;
            await AppendScpCaptureSummaryAsync(target);
            viewModel.Read.TryAdvanceSequence();
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
        var deletionError = CancelledOutputCleaner.TryDelete(target);
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

    private async Task AnalyzeCompletedAsync(string path)
    {
        try { await diskImageWorkspace.AnalyzeAsync(path); }
        catch (Exception exception) when (exception is InvalidDataException or NotSupportedException)
        {
            ErrorLog.Write(exception, $"Analyzing completed disk read: {path}");
            var detail = ExceptionDescriptionFunctions.Describe(exception);
            operation.AppendText(Environment.NewLine);
            operation.AppendText(LocExtension.Get("Error.Unexpected", detail));
            operation.AppendText(Environment.NewLine);
        }
    }

    private async Task AppendScpCaptureSummaryAsync(string path)
    {
        try
        {
            var info = await GWGUI.MediaEngine.Exploration.ScpCaptureInfoReader.ReadAsync(path);
            var checksum = LocExtension.Get(info.ChecksumValid ? "Visual.ChecksumValid" : "Visual.ChecksumInvalid");
            operation.AppendText(Environment.NewLine + LocExtension.Get("Read.ScpSummaryTitle") + Environment.NewLine);
            operation.AppendText(LocExtension.Get("Read.ScpTracksSummary", info.CapturedTracks, info.MissingTracks, info.Cylinders, info.Sides) + Environment.NewLine);
            operation.AppendText(LocExtension.Get("Read.ScpTechnicalSummary", info.Header.Revolutions, info.Header.ResolutionNanoseconds, info.FileSize, checksum) + Environment.NewLine);
            operation.AppendText(LocExtension.Get("Read.ScpOutputFile", path) + Environment.NewLine);
            view.CompletionBlock.SummaryTextBlock.Text = LocExtension.Get("Read.ScpBannerSummary", info.CapturedTracks, info.MissingTracks, info.Cylinders, info.Sides, info.Header.Revolutions, info.FileSize, checksum);
            logOutput.ScrollToEnd();
        }
        catch (Exception exception)
        {
            ErrorLog.Write(exception, "Reading SCP summary");
            var detail = ExceptionDescriptionFunctions.Describe(exception);
            view.CompletionBlock.SummaryTextBlock.Text = LocExtension.Get("Read.ScpSummaryUnavailable", detail);
            operation.AppendText(Environment.NewLine + LocExtension.Get("Read.ScpSummaryUnavailable", detail) + Environment.NewLine);
        }
    }

    internal void UpdateExtension()
    {
        ExtensionText.Text = RawScpRadio.IsChecked == true
            ? LocExtension.Get("Read.RawScp")
            : (ExtensionCombo.SelectedItem as ImageExtension)?.DisplayName ?? LocExtension.Get("Read.ChooseType");
    }

    private void ApplyProfile(OperationProfile profile)
    {
        viewModel.Read.ApplyOptions(profile.EnabledOptions, profile.Values);
        if (profile.IsSystem)
        {
            RawScpRadio.IsChecked = true;
        }
        else
        {
            if (profile.Values.GetValueOrDefault("result") == "raw") RawScpRadio.IsChecked = true;
            else if (profile.Values.GetValueOrDefault("result") == "known") KnownFormatRadio.IsChecked = true;
            SelectFormat(profile.Values.GetValueOrDefault("format"), profile.Values.GetValueOrDefault("extension"));
            if (profile.Values.TryGetValue("folder", out var folder) && !string.IsNullOrWhiteSpace(folder))
                viewModel.Read.Folder = folder;
        }

        updateReadCommand();
    }

    private void SelectFormat(string? formatId, string? imageExtension)
    {
        var format = formatCatalog().Formats.FirstOrDefault(item => item.Id == formatId);
        if (format is null) return;

        FamilyCombo.SelectedItem = format.Family;
        FormatCombo.SelectedItem = format;
        var extension = format.Extensions.FirstOrDefault(item =>
            item.Extension.Equals(imageExtension, StringComparison.OrdinalIgnoreCase));
        if (extension is not null)
            ExtensionCombo.SelectedItem = extension;
    }
}
