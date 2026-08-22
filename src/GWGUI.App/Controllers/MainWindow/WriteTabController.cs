using GWGUI.Domain.Commands;
using GWGUI.Domain.Commands.Building;
using GWGUI.Domain.Commands.Execution;
using GWGUI.Domain.Conversion;
using GWGUI.Domain.Formats;
using GWGUI.Domain.Formats.Detection;
using GWGUI.Domain.Profiles;
using GWGUI.Domain.Settings;
using GWGUI.Domain.Settings.Engines;
using GWGUI.Domain.Write;
using GWGUI.App.Contracts.Services.Hardware;
using GWGUI.App.Contracts.Services.PhysicalDiskWriting;
using GWGUI.App.Enums.Services.Dialogs;
using GWGUI.App.Functions.Services.PhysicalDiskWriting;
using GWGUI.App.Interfaces.Services.Dialogs;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Services.DiskImages;
using GWGUI.App.Services.Operations;
using GWGUI.App.Services.PhysicalDiskWriting;
using GWGUI.App.Services.Profiles;
using GWGUI.App.ViewModels.Main;
using GWGUI.App.Views.Controls.Write;
using GWGUI.App.Views.Windows.Shell;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using GWGUI.Infrastructure.Processes;

namespace GWGUI.App.Controllers.MainWindow;

internal sealed class WriteTabController(
    WriteTabSection view,
    MainWindowViewModel viewModel,
    OperationProfileController profileController,
    Func<IImageFormatCatalog> formatCatalog,
    Func<ImageFormatDetector> formatDetector,
    Func<AppSettings> settings,
    IGwCommandBuilder commandBuilder,
    IFileDialogService fileDialogs,
    IMessageDialogService dialogs,
    DiskDefinitionsController diskDefinitionsController,
    OperationRuntimeController operation,
    OperationProgressController operationProgress,
    ConsoleLogSession consoleLog,
    IGreaseweazleRunner runner,
    DiskImageWorkspaceController diskImageWorkspace,
    TextBox readFolder,
    TextBox commandPreview,
    TextBox logOutput,
    Func<int> selectedMainTab,
    Action<int> selectMainTab,
    Func<string?> selectedDeviceArgument,
    Func<string?> selectedDriveArgument,
    Func<bool> ensureSelectedHardwareAvailable,
    Func<HardwareChoice?> selectedHardware,
    Action confirmAndRequestStop,
    Func<string, Task> loadImage,
    Func<string, string?, Task> loadScp,
    Func<string, Task> loadExplorer,
    Action<Exception, string> appendAnalysisFailure,
    Action updateProfileStatus)
{
    private DetectedImageFormat? detectedFormat;
    private bool UsesInternal => settings().Engines.PhysicalWrite == OperationEngine.Internal;
    private ComboBox FormatCombo => view.FormatBlock.FormatCombo;
    private ComboBox ProfileCombo => view.ProfileBlock.ProfileCombo;

    internal void RefreshProfiles(string? selectedId = null) => profileController.Refresh(ProfileCombo, OperationKind.Write, selectedId);

    internal async Task BrowseSourceAsync()
    {
        var path = fileDialogs.OpenFile(new(LocExtension.Get("Common.DiskImageFilter"), readFolder.Text));
        if (path is null) return;
        viewModel.Write.SourcePath = path;
        detectedFormat = formatDetector().Detect(path, new FileInfo(path).Length);
        view.FormatBlock.DetectionText.Text = $"{detectedFormat.Format?.DisplayName ?? LocExtension.Get("Detection.Ambiguous")} — {LocExtension.Get(detectedFormat.ExplanationKey)}";
        FormatCombo.ItemsSource = detectedFormat.Candidates.Count > 0 ? detectedFormat.Candidates : formatCatalog().Formats;
        FormatCombo.SelectedItem = detectedFormat.Format;
        FormatCombo.Visibility = detectedFormat.RequiresUserChoice ? Visibility.Visible : Visibility.Collapsed;
        view.FormatBlock.VisualizeTracksButton.IsEnabled = true;
        try { await diskImageWorkspace.AnalyzeAsync(path); }
        catch (Exception exception) when (exception is InvalidDataException or NotSupportedException)
        { appendAnalysisFailure(exception, $"Analyzing write source: {path}"); }
        UpdateCommand();
    }

    internal async Task VisualizeSourceAsync()
    {
        var source = viewModel.Write.SourcePath;
        if (string.IsNullOrWhiteSpace(source) || !File.Exists(source)) return;
        if (Path.GetExtension(source).Equals(".scp", StringComparison.OrdinalIgnoreCase))
        { selectMainTab(3); await loadImage(source); return; }
        if (operation.IsRunning) return;
        if (string.IsNullOrWhiteSpace(settings().GwExecutablePath) || !File.Exists(settings().GwExecutablePath))
        { dialogs.Show(LocExtension.Get("App.GwNotConfigured"), LocExtension.Get("App.Title")); return; }
        var format = FormatCombo.SelectedItem as DiskFormat ?? detectedFormat?.Format;
        if (format is null)
        { dialogs.Show(LocExtension.Get("Write.VisualizeFormatRequired"), LocExtension.Get("Write.Title")); return; }
        var temporaryPath = Path.Combine(Path.GetTempPath(), $"gwgui-write-{Guid.NewGuid():N}.scp");
        try
        {
            var output = new ConversionOutput(format.Id, ".scp", temporaryPath, false);
            var command = commandBuilder.BuildConversion(settings().GwExecutablePath!, source, output);
            operation.Begin(); await operation.RenderPendingAsync(); logOutput.Clear();
            await consoleLog.BeginAsync("convert", command.ToDisplayString());
            var outcome = await operation.RunAsync(token => runner.RunAsync(command, new Progress<GwOutputLine>(operation.Report), token));
            await operation.FlushPendingAsync(); operation.Apply(operation.Present(outcome)); operation.End();
            if (outcome.Result?.IsSuccess != true || !File.Exists(temporaryPath)) return;
            selectMainTab(3);
            await Task.WhenAll(loadScp(temporaryPath, Path.GetFileName(source)), loadExplorer(source));
        }
        finally { try { if (File.Exists(temporaryPath)) File.Delete(temporaryPath); } catch { } }
    }

    internal void ToggleFormat()
    {
        if (FormatCombo.ItemsSource is null) FormatCombo.ItemsSource = formatCatalog().Formats;
        FormatCombo.Visibility = FormatCombo.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
    }

    internal void EnableFakeIndex() { viewModel.Write.EnableFakeIndex(); UpdateCommand(); }
    internal void EnableHardSectors() { viewModel.Write.EnableHardSectors(); UpdateCommand(); }
    internal void EnableDensel() { viewModel.Write.EnableDensel(); UpdateCommand(); }
    internal void EnableTg43() { viewModel.Write.EnableTg43(); UpdateCommand(); }

    internal GwCommand BuildCommand() => commandBuilder.BuildWrite(new WriteRequest(
        settings().GwExecutablePath ?? "gw.exe", viewModel.Write.SourcePath,
        (FormatCombo.SelectedItem as DiskFormat)?.Id ?? detectedFormat?.Format?.Id,
        viewModel.Write.BuildOptions(), viewModel.Write.DisableVerification,
        selectedDeviceArgument(), selectedDriveArgument(), viewModel.Write.ExpertArguments));

    internal void UpdateCommand()
    {
        if (selectedMainTab() != 1) return;
        if (UsesInternal) { commandPreview.Text = LocExtension.Get("Write.InternalPreview", view.SourceBlock.Input.Text); return; }
        try { commandPreview.Text = BuildCommand().ToDisplayString(); }
        catch (ArgumentException) { commandPreview.Text = $"⚠ {LocExtension.Get("Advanced.Invalid", LocExtension.Get("Common.Unknown"))}"; }
    }

    internal async Task ExecuteAsync()
    {
        if (operation.IsRunning) { confirmAndRequestStop(); return; }
        if (!ensureSelectedHardwareAvailable()) return;
        if (!diskDefinitionsController.Validate(view.AdvancedBlock.DiskDefinitionsEnabled, view.AdvancedBlock.DiskDefinitionsValue, LocExtension.Get("Write.Title"))) return;
        if (!File.Exists(view.SourceBlock.Input.Text)) { dialogs.Show(LocExtension.Get("Write.SelectSource"), LocExtension.Get("Write.Title"), icon: UserDialogIcon.Information); return; }
        var selected = FormatCombo.SelectedItem as DiskFormat ?? detectedFormat?.Format;
        if (selected is null || (detectedFormat?.RequiresUserChoice == true && FormatCombo.SelectedItem is null))
        { dialogs.Show(LocExtension.Get("Write.Ambiguous"), LocExtension.Get("Write.Title"), icon: UserDialogIcon.Warning); FormatCombo.Visibility = Visibility.Visible; return; }
        if (UsesInternal) { await ExecuteInternalAsync(selected); return; }
        if (string.IsNullOrWhiteSpace(settings().GwExecutablePath) || !File.Exists(settings().GwExecutablePath))
        { dialogs.Show(LocExtension.Get("App.GwNotConfigured"), LocExtension.Get("App.Title"), icon: UserDialogIcon.Information); return; }
        GwCommand command;
        try { command = BuildCommand(); }
        catch (ArgumentException) { diskDefinitionsController.ShowInvalid(LocExtension.Get("Write.Title")); return; }
        var warning = LocExtension.Get(viewModel.Write.DisableVerification ? "Write.VerifyOff" : "Write.VerifyOn");
        var confirmation = LocExtension.Get("Write.Confirm", Path.GetFileName(view.SourceBlock.Input.Text), selected.DisplayName, selectedHardware()?.Label ?? LocExtension.Get("Hardware.NotConfigured"), warning);
        if (dialogs.Show(confirmation, LocExtension.Get("Write.ConfirmTitle"), UserDialogButtons.OkCancel, UserDialogIcon.Warning) != UserDialogResult.Ok) return;
        view.ExecuteActionButton.Content = LocExtension.Get("Common.Stop"); operation.Begin(); await operation.RenderPendingAsync(); logOutput.Clear();
        await consoleLog.BeginAsync("write", command.ToDisplayString());
        var outcome = await operation.RunAsync(token => runner.RunAsync(command, new Progress<GwOutputLine>(operation.Report), token));
        await operation.FlushPendingAsync(); operation.Apply(operation.Present(outcome)); operation.End();
        view.ExecuteActionButton.Content = LocExtension.Get("Common.Execute");
    }

    private async Task ExecuteInternalAsync(DiskFormat selected)
    {
        var hardware = selectedHardware();
        if (hardware is null) { dialogs.Show(LocExtension.Get("Hardware.NotConfigured"), LocExtension.Get("Write.Title"), icon: UserDialogIcon.Warning); return; }
        if (!viewModel.Write.DisableVerification) { dialogs.Show(LocExtension.Get("Write.InternalVerificationUnavailable"), LocExtension.Get("Write.Title"), icon: UserDialogIcon.Warning); return; }
        if (HasUnsupportedInternalOptions()) { dialogs.Show(LocExtension.Get("Write.InternalUnsupportedOptions"), LocExtension.Get("Write.Title"), icon: UserDialogIcon.Warning); return; }
        var warning = LocExtension.Get(viewModel.Write.DisableVerification ? "Write.VerifyOff" : "Write.VerifyOn");
        var confirmation = LocExtension.Get("Write.Confirm", Path.GetFileName(view.SourceBlock.Input.Text), selected.DisplayName, hardware.Label, warning);
        if (dialogs.Show(confirmation, LocExtension.Get("Write.ConfirmTitle"), UserDialogButtons.OkCancel, UserDialogIcon.Warning) != UserDialogResult.Ok) return;
        view.ExecuteActionButton.Content = LocExtension.Get("Common.Stop"); operation.Begin(); await operation.RenderPendingAsync(); logOutput.Clear();
        await consoleLog.BeginAsync("write-internal", LocExtension.Get("Write.InternalPreview", view.SourceBlock.Input.Text));
        var stopwatch = Stopwatch.StartNew();
        var outcome = await operation.RunAsync(async token =>
        {
            var writer = InternalPhysicalDiskWriter.CreateDefault();
            var selection = GreaseweazleDriveSelectionFunctions.Resolve(hardware.Drive.Selection);
            var options = new PhysicalDiskWriteOptions(hardware.Port, selection.BusType, selection.Unit, Verify: false);
            var result = await writer.WriteAsync(new InternalPhysicalDiskWriteRequest(view.SourceBlock.Input.Text, selected.Id, options), new Progress<PhysicalTrackWriteProgress>(operationProgress.Accept), token);
            var lines = result.Failures.Select(failure => new GwOutputLine(DateTimeOffset.Now, GwOutputStream.Error,
                LocExtension.Get("Write.InternalFailure", failure.Cylinder?.ToString() ?? "-", failure.Head?.ToString() ?? "-", LocExtension.Get("Write.InternalFailureReason")))).ToArray();
            foreach (var line in lines) operation.Report(line);
            return new GwExecutionResult(result.IsSuccess ? 0 : 1, result.Cancelled, stopwatch.Elapsed, lines);
        });
        await operation.FlushPendingAsync(); operation.Apply(operation.Present(outcome)); operation.End();
        view.ExecuteActionButton.Content = LocExtension.Get("Common.Execute");
    }

    private bool HasUnsupportedInternalOptions() =>
        viewModel.Write.EraseEmpty.Enabled || viewModel.Write.Retries.Enabled || viewModel.Write.Tracks.Enabled ||
        viewModel.Write.PreErase.Enabled || viewModel.Write.FakeIndex.Enabled || viewModel.Write.HardSectors.Enabled ||
        viewModel.Write.Precomp.Enabled || viewModel.Write.Reverse.Enabled || viewModel.Write.Densel.Enabled ||
        viewModel.Write.Tg43.Enabled || viewModel.Write.DiskDefs.Enabled || !string.IsNullOrWhiteSpace(viewModel.Write.ExpertArguments);

    internal void ProfileChanged()
    {
        if (ProfileCombo.SelectedItem is not OperationProfile profile) return;
        viewModel.Write.ApplyOptions(profile.EnabledOptions, profile.Values); ApplyProfileFormat(profile); UpdateCommand(); updateProfileStatus();
    }

    private void ApplyProfileFormat(OperationProfile profile)
    {
        if (profile.Values.TryGetValue("format", out var formatId) && formatCatalog().Formats.FirstOrDefault(x => x.Id == formatId) is { } format)
        { FormatCombo.ItemsSource = formatCatalog().Formats.Where(x => x.Family != "Raw").ToArray(); FormatCombo.SelectedItem = format; FormatCombo.Visibility = Visibility.Visible; return; }
        if (detectedFormat is not null)
        { FormatCombo.ItemsSource = detectedFormat.Candidates.Count > 0 ? detectedFormat.Candidates : formatCatalog().Formats; FormatCombo.SelectedItem = detectedFormat.Format; FormatCombo.Visibility = detectedFormat.RequiresUserChoice ? Visibility.Visible : Visibility.Collapsed; }
        else { FormatCombo.SelectedItem = null; FormatCombo.Visibility = Visibility.Collapsed; }
    }

    internal void ResetProfile() { if (ProfileCombo.SelectedItem is OperationProfile profile) { ProfileCombo.SelectedItem = null; ProfileCombo.SelectedItem = profile; } }

    internal void SaveProfile()
    {
        var enabled = viewModel.Write.CaptureEnabledOptions(); var values = viewModel.Write.CaptureValues();
        if (FormatCombo.SelectedItem is DiskFormat format) values["format"] = format.Id;
        var profile = profileController.Save(OperationKind.Write, name => new OperationProfile(Guid.NewGuid().ToString("N"), OperationKind.Write, name, values, enabled));
        if (profile is not null) RefreshProfiles(profile.Id);
    }

    internal void RestoreSettings() => viewModel.Write.ApplyOptions(settings().Write.EnabledOptions, settings().Write.OptionValues);
    internal void CaptureSettings()
    { settings().Write.EnabledOptions = viewModel.Write.CaptureEnabledOptions(); settings().Write.OptionValues = viewModel.Write.CaptureValues(); }
}
