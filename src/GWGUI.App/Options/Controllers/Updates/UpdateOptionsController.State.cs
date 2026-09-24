using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using GWGUI.App.Services.Updates;
using GWGUI.App.Services.Emulation;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Views.Controls.Options;
using GWGUI.Updates.Contracts;
using GWGUI.App.Contracts.Updates;
using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Options.Models.Updates;

namespace GWGUI.App.Options.Controllers;

internal sealed partial class UpdateOptionsController : IDisposable
{
    private void SetPreparing(UpdateSearchScope scope, bool value)
    {
        if (value)
        {
            if (scope == UpdateSearchScope.Application) _applicationState = UpdateVisualState.Busy;
            else SetModuleVisualState(UpdateVisualState.Busy);
        }
        else if (scope == UpdateSearchScope.Application && _applicationState == UpdateVisualState.Busy)
            _applicationState = _applicationResult is null ? UpdateVisualState.Neutral : StateFor(_applicationResult);
        else if (scope == UpdateSearchScope.Modules)
            RestoreModuleVisualStateIfBusy();
        RebuildNavigation();
        _section.SearchButton.IsEnabled = !value;
        _section.SearchModulesButton.IsEnabled = !value;
        _section.SearchAvailableModulesButton.IsEnabled = !value;
        _section.InstallButton.IsEnabled = !value && _applicationResult is not null && CanInstall(_applicationResult);
        _section.InstallModulesButton.IsEnabled = !value && _moduleResult is not null && CanInstall(_moduleResult);
        _section.InstallModuleFileButton.IsEnabled = !value;
        _section.InstallModuleUrlButton.IsEnabled = !value;
        _section.ModuleCatalogUrl.IsEnabled = !value;
        _section.CancelButton.Visibility = value && scope == UpdateSearchScope.Application
            ? Visibility.Visible : Visibility.Collapsed;
        _section.CancelModulesButton.Visibility = value && scope == UpdateSearchScope.Modules
            ? Visibility.Visible : Visibility.Collapsed;
        _section.Progress.Visibility = value && scope == UpdateSearchScope.Application
            ? Visibility.Visible : Visibility.Collapsed;
        _section.ModulesProgress.Visibility = value && scope == UpdateSearchScope.Modules
            && _moduleOperationPage == ModuleOperationPage.Module ? Visibility.Visible : Visibility.Collapsed;
        _section.AvailableModulesProgress.Visibility = value && scope == UpdateSearchScope.Modules
            && _moduleOperationPage == ModuleOperationPage.Directory ? Visibility.Visible : Visibility.Collapsed;
        _section.ManualInstallationProgress.Visibility = value && scope == UpdateSearchScope.Modules
            && _moduleOperationPage == ModuleOperationPage.Advanced ? Visibility.Visible : Visibility.Collapsed;
        if (!value) return;
        if (scope == UpdateSearchScope.Application) _section.Progress.Value = 0;
        else ActiveModuleProgress().Value = 0;
        SetStatus(scope, _localize("Updates.Preparing", []));
    }

    private void SetStatus(UpdateSearchScope scope, string value)
    {
        if (scope == UpdateSearchScope.Application) _section.Status.Text = value;
        else SetModuleOperationStatus(value);
    }

    private ProgressBar ActiveModuleProgress() => _moduleOperationPage switch
    {
        ModuleOperationPage.Directory => _section.AvailableModulesProgress,
        ModuleOperationPage.Advanced => _section.ManualInstallationProgress,
        _ => _section.ModulesProgress
    };

    private void SetModuleOperationStatus(string value)
    {
        switch (_moduleOperationPage)
        {
            case ModuleOperationPage.Directory: _section.AvailableModuleStatus.Text = value; break;
            case ModuleOperationPage.Advanced: _section.ManualInstallationStatus.Text = value; break;
            default: _section.ModuleStatus.Text = value; break;
        }
    }

    private void SetModuleVisualState(UpdateVisualState state)
    {
        switch (_moduleOperationPage)
        {
            case ModuleOperationPage.Directory: _directoryState = state; break;
            case ModuleOperationPage.Advanced: _advancedState = state; break;
            default: _modulesState = state; break;
        }
    }

    private void RestoreModuleVisualStateIfBusy()
    {
        switch (_moduleOperationPage)
        {
            case ModuleOperationPage.Directory when _directoryState == UpdateVisualState.Busy:
                _directoryState = AvailableModuleResults.Count == 0 ? UpdateVisualState.Neutral : UpdateVisualState.Current;
                break;
            case ModuleOperationPage.Advanced when _advancedState == UpdateVisualState.Busy:
                _advancedState = UpdateVisualState.Neutral;
                break;
            case ModuleOperationPage.Module when _modulesState == UpdateVisualState.Busy:
                _modulesState = _moduleResult is null ? UpdateVisualState.Current : StateFor(_moduleResult);
                break;
        }
    }

    private static bool CanInstall(UpdateSearchResult result) =>
        result.Plan.IsCompatible && result.Plan.Items.Count > 0;

    private static void DeletePreparedWork(string workDirectory)
    {
        try { if (Directory.Exists(workDirectory)) Directory.Delete(workDirectory, recursive: true); }
        catch { }
    }

    private void PendingInstallationsChanged(object? sender, EventArgs e)
    {
        if (!_owner.Dispatcher.CheckAccess())
        {
            _owner.Dispatcher.Invoke(() => PendingInstallationsChanged(sender, e));
            return;
        }
        RefreshAvailableModuleRows();
    }

    private void RefreshAvailableModuleRows()
    {
        foreach (var row in AvailableModuleResults.ToArray())
        {
            var index = AvailableModuleResults.IndexOf(row);
            var downloaded = _pendingInstallations.Contains(row.Id);
            if (downloaded == row.IsDownloaded) continue;
            AvailableModuleResults[index] = new(row.Id, row.DisplayName, row.CatalogUrl,
                downloaded ? _localize("Updates.ModuleDownloaded", []) : row.DetailsLabel,
                row.IsInstalled, downloaded, !row.IsInstalled && !downloaded,
                downloaded ? _localize("Updates.ModuleDownloaded", [])
                    : _localize("Updates.ModuleDirectoryInstalledBadge", []));
        }
    }
}
