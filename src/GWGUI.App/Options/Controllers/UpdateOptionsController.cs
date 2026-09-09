using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Services.Updates;
using GWGUI.App.Views.Controls.Options;
using GWGUI.Updates.Contracts;

namespace GWGUI.App.Options.Controllers;

internal sealed class UpdateOptionsController
{
    private readonly OptionsUpdatesSection _section;
    private readonly Window _owner;
    private readonly ApplicationUpdateService _service;
    private readonly Func<string, object[], string> _localize;
    private readonly Action<Exception> _reportError;
    private readonly Dictionary<string, string> _selections = new(StringComparer.OrdinalIgnoreCase);
    private UpdateSearchResult? _lastResult;
    private CancellationTokenSource? _preparationCancellation;

    internal ObservableCollection<UpdateComponentRow> Results { get; } = [];

    internal UpdateOptionsController(
        Window owner,
        OptionsUpdatesSection section,
        ApplicationUpdateService service,
        Func<string, object[], string> localize,
        Action<Exception> reportError)
    {
        _owner = owner;
        _section = section;
        _service = service;
        _localize = localize;
        _reportError = reportError;
        _section.Results.ItemsSource = Results;
        _section.SearchRequested += Search;
        _section.VersionSelectionChanged += VersionChanged;
        _section.InstallRequested += Install;
        _section.CancelRequested += Cancel;
        RefreshLocalizedContent();
        _section.Scope.SelectedIndex = 2;
    }

    internal void RefreshLocalizedContent()
    {
        var selected = _section.Scope.SelectedValue is UpdateSearchScope scope ? scope : UpdateSearchScope.All;
        _section.Scope.ItemsSource = new[]
        {
            new UpdateScopeOption(UpdateSearchScope.Application, _localize("Updates.ScopeApplication", [])),
            new UpdateScopeOption(UpdateSearchScope.Modules, _localize("Updates.ScopeModules", [])),
            new UpdateScopeOption(UpdateSearchScope.All, _localize("Updates.ScopeAll", []))
        };
        _section.Scope.SelectedValue = selected;
        if (_lastResult is null) _section.Status.Text = _localize("Updates.Ready", []);
        else Render(_lastResult);
    }

    private async void Search(object sender, RoutedEventArgs e)
    {
        if (_section.Scope.SelectedValue is not UpdateSearchScope scope) return;
        _section.SearchButton.IsEnabled = false;
        _section.Status.Text = _localize("Updates.Searching", []);
        try
        {
            _lastResult = await _service.SearchAsync(scope, _selections);
            Render(_lastResult);
        }
        catch (Exception exception)
        {
            _section.Status.Text = _localize("Updates.SearchFailed", []);
            _reportError(exception);
        }
        finally { _section.SearchButton.IsEnabled = true; }
    }

    private void VersionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox { DataContext: UpdateComponentRow row }) return;
        if (string.IsNullOrWhiteSpace(row.SelectedVersion)) _selections.Remove(row.Id);
        else _selections[row.Id] = row.SelectedVersion;
    }

    private void Render(UpdateSearchResult result)
    {
        Results.Clear();
        foreach (var update in result.Components)
        {
            var displayName = update.Kind == UpdateComponentKind.Application
                ? _localize("Updates.Application", []) : update.ComponentId;
            var status = update.Availability switch
            {
                UpdateAvailability.UpToDate => _localize("Updates.UpToDate", []),
                UpdateAvailability.Available => _localize("Updates.Available", []),
                UpdateAvailability.ApplicationUpdateRequired => _localize("Updates.ApplicationRequired", [update.RequiredHostApiVersion ?? ""]),
                _ => _localize("Updates.Incompatible", [])
            };
            Results.Add(new(update.ComponentId, displayName,
                _localize("Updates.InstalledVersion", [update.InstalledVersion]), status,
                update.Releases.Select(release => release.Version).ToArray(), update.SelectedVersion,
                update.Availability == UpdateAvailability.Available));
        }
        _section.Status.Text = result.Components.Any(update => update.Availability != UpdateAvailability.UpToDate)
            ? _localize("Updates.Results", []) : _localize("Updates.NoUpdates", []);
        _section.InstallButton.IsEnabled = result.Plan.IsCompatible && result.Plan.Items.Count > 0;
    }

    private async void Install(object sender, RoutedEventArgs e)
    {
        if (_section.Scope.SelectedValue is not UpdateSearchScope scope || _preparationCancellation is not null) return;
        try
        {
            _lastResult = await _service.SearchAsync(scope, _selections);
            Render(_lastResult);
            if (!_lastResult.Plan.IsCompatible || _lastResult.Plan.Items.Count == 0) return;
            if (MessageBox.Show(_owner, _localize("Updates.InstallConfirm", [_lastResult.Plan.Items.Count]),
                    _localize("Updates.Title", []), MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            _preparationCancellation = new CancellationTokenSource();
            SetPreparing(true);
            var progress = new Progress<double>(value => _section.Progress.Value = value * 100);
            var launch = await _service.PrepareAsync(_lastResult.Plan, progress, _preparationCancellation.Token);
            var start = new ProcessStartInfo(launch.UpdaterExecutable)
            {
                UseShellExecute = false,
                WorkingDirectory = launch.WorkingDirectory
            };
            start.ArgumentList.Add("--apply");
            start.ArgumentList.Add(launch.PlanPath);
            if (Process.Start(start) is null) throw new InvalidOperationException("The updater could not be started.");
            _owner.Close();
            _ = Application.Current.Dispatcher.BeginInvoke(() => Application.Current.MainWindow?.Close());
        }
        catch (OperationCanceledException) { _section.Status.Text = _localize("Updates.PreparationCancelled", []); }
        catch (Exception exception)
        {
            _section.Status.Text = _localize("Updates.PreparationFailed", []);
            _reportError(exception);
        }
        finally
        {
            _preparationCancellation?.Dispose();
            _preparationCancellation = null;
            if (_owner.IsVisible) SetPreparing(false);
        }
    }

    private void Cancel(object sender, RoutedEventArgs e) => _preparationCancellation?.Cancel();

    private void SetPreparing(bool value)
    {
        _section.SearchButton.IsEnabled = !value;
        _section.InstallButton.IsEnabled = !value && _lastResult is { Plan.IsCompatible: true }
            && _lastResult.Plan.Items.Count > 0;
        _section.CancelButton.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        _section.Progress.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        if (value)
        {
            _section.Progress.Value = 0;
            _section.Status.Text = _localize("Updates.Preparing", []);
        }
    }
}

internal sealed record UpdateScopeOption(UpdateSearchScope Scope, string Label);

internal sealed class UpdateComponentRow
{
    internal UpdateComponentRow(string id, string displayName, string installedLabel, string statusLabel,
        IReadOnlyList<string> versions, string? selectedVersion, bool canSelectVersion)
    {
        Id = id;
        DisplayName = displayName;
        InstalledLabel = installedLabel;
        StatusLabel = statusLabel;
        Versions = versions;
        SelectedVersion = selectedVersion;
        CanSelectVersion = canSelectVersion;
    }

    public string Id { get; }
    public string DisplayName { get; }
    public string InstalledLabel { get; }
    public string StatusLabel { get; }
    public IReadOnlyList<string> Versions { get; }
    public string? SelectedVersion { get; set; }
    public bool CanSelectVersion { get; }
}
