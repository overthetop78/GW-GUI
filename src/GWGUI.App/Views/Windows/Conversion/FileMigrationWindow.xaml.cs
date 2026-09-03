using GWGUI.App.Enums.Services.Dialogs;
using GWGUI.App.Interfaces.Services.Dialogs;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Functions.Localization;
using GWGUI.App.Services.Conversion;
using GWGUI.App.Services.Dialogs;
using GWGUI.App.Services.Logging;
using GWGUI.App.ViewModels.Conversion;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using GWGUI.MediaEngine.Composition;
using GWGUI.MediaEngine.Exploration;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Migration;

namespace GWGUI.App.Views.Windows.Conversion;

public partial class FileMigrationWindow : Window
{
    private readonly IFileDialogService _fileDialogs;
    private readonly IMessageDialogService _dialogs;
    private readonly FileMigrationCoordinator _coordinator;
    private FileSystemVolume? _source;
    private MigrationValidationReport? _report;
    private bool _busy;

    public ObservableCollection<FileMigrationTargetOption> Targets { get; } = new(FileSystemMigrationTargetCatalog.All.Select(target => new FileMigrationTargetOption(target)));
    public ObservableCollection<FileMigrationLossRow> Losses { get; } = [];
    public FileMigrationTargetOption? SelectedTarget { get; set; }

    public FileMigrationWindow(string? initialSourcePath = null)
    {
        InitializeComponent();
        DataContext = this;
        _fileDialogs = new WpfFileDialogService(this);
        _dialogs = new WpfMessageDialogService(this);
        _coordinator = new(DiskImageExplorer.CreateDefault(), MediaEngineFactory.CreateFileSystemMigrationService());
        SelectedTarget = Targets.FirstOrDefault();
        TargetComboBox.SelectedItem = SelectedTarget;
        BrowseSourceButton.Click += BrowseSource_Click;
        BrowseOutputButton.Click += BrowseOutput_Click;
        TargetComboBox.SelectionChanged += Target_Changed;
        AcceptLossesCheckBox.Checked += Acceptance_Changed;
        AcceptLossesCheckBox.Unchecked += Acceptance_Changed;
        ExecuteButton.Click += Execute_Click;
        if (!string.IsNullOrWhiteSpace(initialSourcePath)) Loaded += async (_, _) => await LoadSourceAsync(initialSourcePath);
    }

    private async void BrowseSource_Click(object sender, RoutedEventArgs e)
    {
        var path = _fileDialogs.OpenFile(new(LocExtension.Get("Common.DiskImageFilter"), Path.GetDirectoryName(SourcePathTextBox.Text)));
        if (path is not null) await LoadSourceAsync(path);
    }

    private void BrowseOutput_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedTarget is null) return;
        var path = _fileDialogs.SaveFile(new($"{SelectedTarget.Label}|*{SelectedTarget.Target.Extension}", SuggestedOutputName(), SelectedTarget.Target.Extension));
        if (path is not null) OutputPathTextBox.Text = path;
        RefreshValidation();
    }

    private void Target_Changed(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        SelectedTarget = TargetComboBox.SelectedItem as FileMigrationTargetOption;
        AcceptLossesCheckBox.IsChecked = false;
        if (_source is not null) OutputPathTextBox.Text = SuggestedOutputPath();
        RefreshValidation();
    }

    private void Acceptance_Changed(object sender, RoutedEventArgs e) => RefreshValidation();

    private async void Execute_Click(object sender, RoutedEventArgs e)
    {
        if (_source is null || SelectedTarget is null || _report?.CanExecute != true || string.IsNullOrWhiteSpace(OutputPathTextBox.Text)) return;
        if (Path.GetFullPath(SourcePathTextBox.Text).Equals(Path.GetFullPath(OutputPathTextBox.Text), StringComparison.OrdinalIgnoreCase))
        {
            _dialogs.Show(LocExtension.Get("Migration.SourceDestinationSame"), LocExtension.Get("Migration.Title"), icon: UserDialogIcon.Warning);
            return;
        }
        if (File.Exists(OutputPathTextBox.Text) && _dialogs.Show(LocExtension.Get("Migration.OverwriteConfirm", OutputPathTextBox.Text), LocExtension.Get("Migration.Title"), UserDialogButtons.YesNo, UserDialogIcon.Warning) != UserDialogResult.Yes) return;
        try
        {
            SetBusy(true);
            await _coordinator.ExecuteAsync(_source, OutputPathTextBox.Text, SelectedTarget.Target.FormatId, AcceptLossesCheckBox.IsChecked == true);
            _dialogs.Show(LocExtension.Get("Migration.Completed", OutputPathTextBox.Text), LocExtension.Get("Migration.Title"), icon: UserDialogIcon.Information);
        }
        catch (Exception exception)
        {
            ShowLoggedError(exception, "Executing file migration", "Migration.Failed");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task LoadSourceAsync(string path)
    {
        try
        {
            SetBusy(true);
            _source = await _coordinator.ReadSourceAsync(path);
            SourcePathTextBox.Text = path;
            SourceSummaryText.Text = LocExtension.Get("Migration.SourceSummary", _source.Name, _source.FileSystemId, _source.Entries.Count);
            OutputPathTextBox.Text = SuggestedOutputPath();
            RefreshValidation();
        }
        catch (Exception exception)
        {
            _source = null;
            SourceSummaryText.Text = LoggedErrorText(exception, "Preparing file migration source", "Migration.SourceFailed");
            RefreshValidation();
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void RefreshValidation()
    {
        Losses.Clear();
        _report = null;
        if (_source is null || SelectedTarget is null)
        {
            CompatibilityText.Text = LocExtension.Get("Migration.Waiting");
            AcceptLossesCheckBox.Visibility = Visibility.Collapsed;
            UpdateExecuteState();
            return;
        }
        try
        {
            _report = _coordinator.Validate(_source, SelectedTarget.Target.FormatId, AcceptLossesCheckBox.IsChecked == true);
            foreach (var loss in _report.Losses) Losses.Add(new(loss));
            var hasBlockingLoss = _report.Losses.Any(loss => loss.IsBlocking);
            var hasMetadataLoss = _report.Losses.Any(loss => !loss.IsBlocking);
            CompatibilityText.Text = LocExtension.Get(hasBlockingLoss ? "Migration.Incompatible" : hasMetadataLoss ? "Migration.CompatibleWithLosses" : "Migration.Compatible");
            AcceptLossesCheckBox.Visibility = hasMetadataLoss && !hasBlockingLoss ? Visibility.Visible : Visibility.Collapsed;
        }
        catch (Exception exception)
        {
            CompatibilityText.Text = LoggedErrorText(exception, "Validating file migration", "Migration.SourceFailed");
        }
        UpdateExecuteState();
    }

    private string SuggestedOutputPath()
    {
        if (SelectedTarget is null || string.IsNullOrWhiteSpace(SourcePathTextBox.Text)) return string.Empty;
        return Path.Combine(Path.GetDirectoryName(SourcePathTextBox.Text) ?? string.Empty, SuggestedOutputName());
    }

    private string SuggestedOutputName()
    {
        if (SelectedTarget is null) return string.Empty;
        return $"{Path.GetFileNameWithoutExtension(SourcePathTextBox.Text)}-migrated{SelectedTarget.Target.Extension}";
    }

    private void SetBusy(bool busy)
    {
        _busy = busy;
        ProgressBar.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
        BrowseSourceButton.IsEnabled = !busy;
        BrowseOutputButton.IsEnabled = !busy;
        TargetComboBox.IsEnabled = !busy;
        UpdateExecuteState();
    }

    private void UpdateExecuteState() => ExecuteButton.IsEnabled = !_busy && _report?.CanExecute == true && !string.IsNullOrWhiteSpace(OutputPathTextBox.Text);

    private void ShowLoggedError(Exception exception, string context, string messageKey) => _dialogs.Show(LoggedErrorText(exception, context, messageKey), LocExtension.Get("Migration.Title"), icon: UserDialogIcon.Error);

    private static string LoggedErrorText(Exception exception, string context, string messageKey)
    {
        ErrorLog.Write(exception, context);
        var detail = ExceptionDescriptionFunctions.Describe(exception);
        return LocExtension.Get(messageKey, detail);
    }
}
