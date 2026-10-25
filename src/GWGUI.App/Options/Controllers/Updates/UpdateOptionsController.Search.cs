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
    private async void SearchApplication(object sender, RoutedEventArgs e)
        => await SearchAsync(UpdateSearchScope.Application);

    private async void SearchModules(object sender, RoutedEventArgs e)
        => await SearchAsync(UpdateSearchScope.Modules);

    private async Task SearchAsync(UpdateSearchScope scope)
    {
        var isApplication = scope == UpdateSearchScope.Application;
        var moduleId = _section.SelectedModuleId;
        if (!isApplication && string.IsNullOrWhiteSpace(moduleId)) return;
        var button = isApplication ? _section.SearchButton : _section.SearchModulesButton;
        var status = isApplication ? _section.Status : _section.ModuleStatus;
        button.IsEnabled = false;
        SetVisualState(UpdateVisualState.Busy);
        RebuildNavigation();
        status.Text = _localize("Updates.Searching", []);
        try
        {
            if (isApplication)
            {
                _applicationResult = await _applicationService.SearchAsync(_applicationSelections);
                RenderApplication(_applicationResult);
                _applicationState = StateFor(_applicationResult);
            }
            else
            {
                var result = await _moduleService.SearchAsync(moduleId!, _moduleSelections);
                AcceptModuleResult(moduleId!, result);
            }
        }
        catch (Exception exception)
        {
            if (isApplication || string.Equals(_section.SelectedModuleId, moduleId, StringComparison.OrdinalIgnoreCase)) status.Text = _localize("Updates.SearchFailed", []);
            SetVisualState(UpdateVisualState.Error);
            _reportError(exception);
        }
        finally
        {
            RebuildNavigation();
            if (_preparationCancellation is null) button.IsEnabled = true;
        }

        void SetVisualState(UpdateVisualState state)
        {
            if (isApplication) _applicationState = state;
            else _moduleStatesById[moduleId!] = state;
        }
    }

    private async void SearchAvailableModules(object sender, RoutedEventArgs e)
    {
        _section.SearchAvailableModulesButton.IsEnabled = false;
        _directoryState = UpdateVisualState.Busy;
        _section.AvailableModulesProgress.Visibility = Visibility.Visible;
        _section.AvailableModulesProgress.IsIndeterminate = true;
        RebuildNavigation();
        _section.AvailableModuleStatus.Text = _localize("Updates.ModuleDirectorySearching", []);
        try
        {
            var modules = await _moduleDirectoryService.SearchAsync();
            AvailableModuleResults.Clear();
            foreach (var module in modules)
            {
                var installedVersion = module.InstalledVersion;
                var isInstalled = installedVersion is not null;
                var isDownloaded = _pendingInstallations.Contains(module.Id);
                var details = installedVersion is not null
                    ? _localize("Updates.InstalledVersion", [installedVersion])
                    : isDownloaded ? _localize("Updates.ModuleDownloaded", []) : module.AvailableVersion is null ? _localize("Updates.Incompatible", []) : _localize("Updates.ModuleAvailableVersion", [module.AvailableVersion]);
                AvailableModuleResults.Add(new(module.Id, module.DisplayName, module.CatalogUrl,
                    details, isInstalled, isDownloaded, module.CanInstall && !isDownloaded,
                    isDownloaded ? _localize("Updates.ModuleDownloaded", []) : _localize("Updates.ModuleDirectoryInstalledBadge", [])));
            }
            _section.AvailableModuleStatus.Text = AvailableModuleResults.Count == 0 ? _localize("Updates.ModuleDirectoryEmpty", []) : _localize("Updates.ModuleDirectoryResults", [AvailableModuleResults.Count]);
            _directoryState = UpdateVisualState.Current;
        }
        catch (Exception exception)
        {
            _section.AvailableModuleStatus.Text = _localize("Updates.ModuleDirectoryFailed", []);
            _directoryState = UpdateVisualState.Error;
            _reportError(exception);
        }
        finally
        {
            _section.AvailableModulesProgress.IsIndeterminate = false;
            _section.AvailableModulesProgress.Visibility = Visibility.Collapsed;
            RebuildNavigation();
            if (_preparationCancellation is null) _section.SearchAvailableModulesButton.IsEnabled = true;
        }
    }

    private void VersionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox { DataContext: UpdateComponentRow row }) return;
        var selections = row.Kind == UpdateComponentKind.Application ? _applicationSelections : _moduleSelections;
        if (string.IsNullOrWhiteSpace(row.SelectedVersion)) selections.Remove(row.Id);
        else selections[row.Id] = row.SelectedVersion;
    }

}
