using GWGUI.Domain.Commands;
using GWGUI.Domain.Commands.Building;
using GWGUI.Domain.Commands.Execution;
using GWGUI.Domain.Commands.Options;
using GWGUI.Domain.Conversion;
using GWGUI.Domain.Formats;
using GWGUI.Domain.Formats.Detection;
using GWGUI.Domain.Profiles;
using GWGUI.Domain.Settings;
using GWGUI.Domain.Settings.Engines;
using GWGUI.App.Functions.ViewModels.Conversion;
using GWGUI.App.Interfaces.Services.Dialogs;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Presenters.Conversion;
using GWGUI.App.Services.Conversion;
using GWGUI.App.Services.DiskImages;
using GWGUI.App.Services.Logging;
using GWGUI.App.Services.Operations;
using GWGUI.App.Services.Profiles;
using GWGUI.App.ViewModels.Main;
using GWGUI.App.Views.Controls.Conversion;
using GWGUI.App.Views.Windows.Conversion;
using GWGUI.App.Views.Windows.Shell;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using GWGUI.Infrastructure.Processes;

namespace GWGUI.App.Controllers.MainWindow;

internal sealed class ConversionTabController(
    Window owner,
    ConversionTabSection view,
    MainWindowViewModel viewModel,
    OperationProfileController profileController,
    ConversionFormatPresenter formatPresenter,
    Func<IImageFormatCatalog> formatCatalog,
    Func<ImageFormatDetector> formatDetector,
    Func<AppSettings> settings,
    IGwCommandBuilder commandBuilder,
    IGreaseweazleRunner runner,
    IFileDialogService fileDialogs,
    IBusinessDialogService businessDialogs,
    IMessageDialogService dialogs,
    DiskDefinitionsController diskDefinitionsController,
    OperationRuntimeController operation,
    ConsoleLogSession consoleLog,
    DiskImageWorkspaceController diskImageWorkspace,
    TextBox readFolder,
    TextBox commandPreview,
    TextBox logOutput,
    Func<int> selectedMainTab,
    Action<int> selectMainTab,
    Func<string, Task> loadImage,
    Action confirmAndRequestStop,
    Action<Exception, string> appendAnalysisFailure,
    Action updateProfileStatus,
    Dispatcher dispatcher)
{
    private string? sourceExtension;
    private DetectedImageFormat? sourceDetection;
    private bool UsesInternal => settings().Engines.Conversion == OperationEngine.Internal;
    private ComboBox ProfileCombo => view.ProfileBlock.ProfileCombo;

    internal void RefreshProfiles(string? selectedId = null) => profileController.Refresh(ProfileCombo, OperationKind.Convert, selectedId);

    internal void BuildFormats(string? extension, DetectedImageFormat? detection = null)
    {
        sourceExtension = extension; sourceDetection = detection;
        var items = formatPresenter.Build(formatCatalog(), extension, detection, viewModel.Conversion.SelectedFormats, viewModel.Conversion.ExplicitExtensions);
        foreach (var item in items)
            if (!item.IsCompatible && viewModel.Conversion.SelectedFormats.Contains(item.Format.Id))
                viewModel.Conversion.SetFormat(item.Format.Id, false, item.ExplicitExtensions);
        view.FormatsBlock.SetItems(items, extension);
    }

    internal void ProfileChanged()
    {
        if (ProfileCombo.SelectedItem is not OperationProfile profile) return;
        ApplyProfile(profile); updateProfileStatus();
    }

    private void ApplyProfile(OperationProfile profile)
    { viewModel.Conversion.ApplyProfile(profile.EnabledOptions, profile.Values); BuildFormats(sourceExtension, sourceDetection); UpdateCommand(); }

    internal void ResetProfile() { if (ProfileCombo.SelectedItem is OperationProfile profile) ApplyProfile(profile); }

    internal void SaveProfile()
    {
        var enabled = viewModel.Conversion.CaptureProfileEnabled(); var values = viewModel.Conversion.CaptureProfileValues();
        var profile = profileController.Save(OperationKind.Convert, name => new OperationProfile(Guid.NewGuid().ToString("N"), OperationKind.Convert, name, values, enabled));
        if (profile is not null) RefreshProfiles(profile.Id);
    }

    internal void SelectionChanged(object? sender)
    {
        if (sender is not ConversionFormatControl control) return;
        viewModel.Conversion.SetFormat(control.Format.Id, control.IsSelected, control.ExplicitExtensions);
        BuildFormats(sourceExtension, sourceDetection); UpdateCommand();
    }

    internal async Task BrowseSourceAsync()
    {
        var path = fileDialogs.OpenFile(new(LocExtension.Get("Common.DiskImageFilter"), readFolder.Text));
        if (path is null) return;
        viewModel.Conversion.SourcePath = path; viewModel.Conversion.OutputName = Path.GetFileNameWithoutExtension(path);
        var detection = formatDetector().Detect(path, new FileInfo(path).Length);
        view.OutputBlock.SourceInformation.Text = detection.Format?.DisplayName ?? LocExtension.Get("Conversion.SourceAmbiguous");
        view.SourceBlock.ActionButton.Visibility = Path.GetExtension(path).Equals(".scp", StringComparison.OrdinalIgnoreCase) ? Visibility.Visible : Visibility.Collapsed;
        try { await diskImageWorkspace.AnalyzeAsync(path); }
        catch (Exception exception) when (exception is InvalidDataException or NotSupportedException)
        { appendAnalysisFailure(exception, $"Analyzing conversion source: {path}"); }
        BuildFormats(Path.GetExtension(path), detection); UpdateCommand();
    }

    internal async Task VisualizeSourceAsync()
    {
        var source = viewModel.Conversion.SourcePath;
        if (string.IsNullOrWhiteSpace(source) || !File.Exists(source) || !Path.GetExtension(source).Equals(".scp", StringComparison.OrdinalIgnoreCase)) return;
        selectMainTab(3); await loadImage(source);
    }

    internal void OpenMigration()
    {
        var sourcePath = File.Exists(view.SourceBlock.Input.Text) ? view.SourceBlock.Input.Text : null;
        new FileMigrationWindow(sourcePath) { Owner = owner }.ShowDialog();
    }

    internal IReadOnlyList<ConversionOutput> Plan()
    {
        if (string.IsNullOrWhiteSpace(viewModel.Conversion.SourcePath)) return [];
        return new ConversionPlanner(formatCatalog()).Plan(viewModel.Conversion.SourcePath, readFolder.Text,
            viewModel.Conversion.OutputName.Trim(), viewModel.Conversion.BuildSelections(formatCatalog().Formats),
            viewModel.Conversion.AddTags, settings().Conversion.TagPattern);
    }

    private EnabledOption[] Options() => viewModel.Conversion.BuildOptions().ToArray();

    internal void UpdateCommand()
    {
        if (selectedMainTab() != 2) return;
        try
        {
            var outputs = Plan();
            if (outputs.Count == 0) { commandPreview.Text = LocExtension.Get("Conversion.SelectOutput"); return; }
            if (UsesInternal && !ConversionBatchExecutor.IsInternal(viewModel.Conversion.SourcePath, outputs[0]))
            { commandPreview.Text = LocExtension.Get("Conversion.EngineInternalUnavailable", outputs[0].OutputPath); return; }
            var first = UsesInternal
                ? new GwCommand("GW GUI", "encode", ["--codec", outputs[0].FormatId, viewModel.Conversion.SourcePath, outputs[0].OutputPath])
                : commandBuilder.BuildConversion(settings().GwExecutablePath ?? "gw.exe", viewModel.Conversion.SourcePath, outputs[0], Options(), viewModel.Conversion.ExpertArguments);
            commandPreview.Text = first.ToDisplayString() + (outputs.Count > 1 ? LocExtension.Get("Conversion.More", outputs.Count - 1) : "");
        }
        catch (Exception exception) { ErrorLog.Write(exception, "Building conversion preview"); commandPreview.Text = $"⚠ {LocExtension.Get("Advanced.Invalid", LocExtension.Get("Common.Unknown"))}"; }
    }

    internal async Task ExecuteAsync()
    {
        if (operation.IsRunning) { confirmAndRequestStop(); return; }
        if (!diskDefinitionsController.Validate(view.AdvancedBlock.DiskDefinitionsEnabled, view.AdvancedBlock.DiskDefinitionsValue, LocExtension.Get("Conversion.Title"))) return;
        if (!File.Exists(view.SourceBlock.Input.Text)) { dialogs.Show(LocExtension.Get("Conversion.SourceRequired"), LocExtension.Get("Conversion.Title")); return; }
        if (string.IsNullOrWhiteSpace(view.OutputBlock.OutputNameTextBox.Text)) { dialogs.Show(LocExtension.Get("Conversion.NameRequired"), LocExtension.Get("Conversion.Title")); return; }
        IReadOnlyList<ConversionOutput> outputs;
        try { outputs = Plan(); GwOptionValidator.Validate(Options()); }
        catch { diskDefinitionsController.ShowInvalid(LocExtension.Get("Conversion.Title")); return; }
        if (outputs.Count == 0) { dialogs.Show(LocExtension.Get("Conversion.CheckOutput"), LocExtension.Get("Conversion.Title")); return; }
        if (UsesInternal && outputs.Any(x => !ConversionBatchExecutor.IsInternal(viewModel.Conversion.SourcePath, x)))
        { dialogs.Show(LocExtension.Get("Conversion.EngineInternalUnavailable", outputs.First(x => !ConversionBatchExecutor.IsInternal(viewModel.Conversion.SourcePath, x)).OutputPath), LocExtension.Get("Conversion.Title")); return; }
        if (!UsesInternal && (string.IsNullOrWhiteSpace(settings().GwExecutablePath) || !File.Exists(settings().GwExecutablePath)))
        { dialogs.Show(LocExtension.Get("App.GwNotConfigured"), LocExtension.Get("App.Title")); return; }
        var existing = outputs.Where(x => File.Exists(x.OutputPath)).ToArray();
        if (existing.Length > 0)
        {
            var decisions = businessDialogs.ResolveConversionConflicts(existing); if (decisions is null) return;
            outputs = ConversionConflictResolutionFunctions.Apply(outputs, existing, decisions, NumberedPath);
        }
        view.ExecuteActionButton.Content = LocExtension.Get("Common.Stop"); operation.Begin(); await operation.RenderPendingAsync(); logOutput.Clear();
        await consoleLog.BeginAsync("convert", commandPreview.Text);
        var progress = new Progress<GwOutputLine>(operation.Report);
        var outcome = await operation.RunAsync(token =>
        {
            var items = outputs.Select(x => (Output: x, Command: commandBuilder.BuildConversion(settings().GwExecutablePath ?? "gw.exe", viewModel.Conversion.SourcePath, x, Options(), viewModel.Conversion.ExpertArguments))).ToArray();
            return new ConversionBatchExecutor(runner).RunAsync(viewModel.Conversion.SourcePath, items, progress, item => dispatcher.Invoke(() =>
            { operation.Begin(); operation.AppendText($"{Environment.NewLine}→ {item.Label}{Environment.NewLine}"); }, DispatcherPriority.ContextIdle), token, settings().Engines.Conversion);
        });
        await operation.FlushPendingAsync(); operation.Apply(operation.Present(outcome)); operation.End();
        view.ExecuteActionButton.Content = LocExtension.Get("Common.Execute");
    }

    private static string NumberedPath(string path)
    {
        var folder = Path.GetDirectoryName(path)!; var name = Path.GetFileNameWithoutExtension(path); var extension = Path.GetExtension(path);
        for (var number = 1; number < int.MaxValue; number++) { var candidate = Path.Combine(folder, $"{name} ({number}){extension}"); if (!File.Exists(candidate)) return candidate; }
        throw new IOException(LocExtension.Get("Conversion.NoAvailableOutputName"));
    }

    internal void CaptureSettings()
    {
        settings().Conversion.AddTags = viewModel.Conversion.AddTags;
        settings().Conversion.SelectedFormats = viewModel.Conversion.SelectedFormats.ToHashSet();
        settings().Conversion.ExplicitExtensions = viewModel.Conversion.ExplicitExtensions.ToDictionary(x => x.Key, x => x.Value.ToHashSet());
        settings().Conversion.EnabledOptions = viewModel.Conversion.CaptureEnabledOptions();
        settings().Conversion.OptionValues = viewModel.Conversion.CaptureValues();
    }

    internal void RestoreSettings() => viewModel.Conversion.ApplySettings(settings().Conversion.AddTags,
        settings().Conversion.SelectedFormats, settings().Conversion.ExplicitExtensions,
        settings().Conversion.EnabledOptions, settings().Conversion.OptionValues);
}
