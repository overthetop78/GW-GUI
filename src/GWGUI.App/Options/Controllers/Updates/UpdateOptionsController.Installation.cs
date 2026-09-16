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
using GWGUI.App.Constants.Options.Updates;

namespace GWGUI.App.Options.Controllers;

internal sealed partial class UpdateOptionsController : IDisposable
{
    private async void InstallApplication(object sender, RoutedEventArgs e) =>
        await InstallAsync(UpdateSearchScope.Application);

    private async void InstallModules(object sender, RoutedEventArgs e) =>
        await InstallModuleUpdatesAsync();

    private async Task InstallModuleUpdatesAsync()
    {
        _moduleOperationPage = ModuleOperationPage.Module;
        await InstallAsync(UpdateSearchScope.Modules);
    }

    private async void InstallModuleFromFile(object sender, RoutedEventArgs e)
    {
        _moduleOperationPage = ModuleOperationPage.Advanced;
        var dialog = new OpenFileDialog
        {
            Title = _localize("Updates.ModuleInstallFromFile", []),
            Filter = _localize("Updates.ModuleArchiveFilter", [])
        };
        if (dialog.ShowDialog(_owner) != true) return;
        await PrepareNewModuleAsync(cancellation =>
            _moduleInstallation.PrepareFromFileAsync(dialog.FileName,
                Path.GetFileNameWithoutExtension(dialog.FileName), cancellation));
    }

    private async void InstallModuleFromUrl(object sender, RoutedEventArgs e)
    {
        _moduleOperationPage = ModuleOperationPage.Advanced;
        var catalogUrl = _section.ModuleCatalogUrl.Text.Trim();
        if (catalogUrl.Length == 0)
        {
            _section.ManualInstallationStatus.Text = _localize("Updates.ModuleCatalogUrlRequired", []);
            return;
        }
        await PrepareNewModuleAsync(cancellation =>
            _moduleInstallation.PrepareFromCatalogAsync(catalogUrl, catalogUrl,
                new Progress<double>(value => ActiveModuleProgress().Value = value * 100), cancellation));
    }

    private async void InstallAvailableModule(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: AvailableModuleRow module } || !module.CanInstall
            || _pendingInstallations.Contains(module.Id)) return;
        _moduleOperationPage = ModuleOperationPage.Directory;
        await PrepareNewModuleAsync(cancellation =>
            _moduleInstallation.PrepareFromCatalogAsync(module.CatalogUrl, module.DisplayName,
                new Progress<double>(value => ActiveModuleProgress().Value = value * 100), cancellation));
    }

    private async Task PrepareNewModuleAsync(
        Func<CancellationToken, Task<PendingModuleInstallation>> prepare)
    {
        if (_preparationCancellation is not null) return;
        PendingModuleInstallation? prepared = null;
        try
        {
            _preparationCancellation = new CancellationTokenSource();
            SetPreparing(UpdateSearchScope.Modules, true);
            prepared = await prepare(_preparationCancellation.Token);
            if (!_pendingInstallations.TryAdd(prepared))
            {
                DeletePreparedWork(prepared.WorkingDirectory);
                return;
            }
            SetModuleOperationStatus(_localize("Updates.ModuleDownloaded", []));
            SetModuleVisualState(UpdateVisualState.Current);
            RefreshAvailableModuleRows();
        }
        catch (OperationCanceledException)
        {
            SetModuleOperationStatus(_localize("Updates.PreparationCancelled", []));
            SetModuleVisualState(UpdateVisualState.Neutral);
        }
        catch (Exception exception)
        {
            if (prepared is not null && !_pendingInstallations.Contains(prepared.ModuleId))
                DeletePreparedWork(prepared.WorkingDirectory);
            SetModuleOperationStatus(_localize("Updates.ModuleInstallFailed", []));
            SetModuleVisualState(UpdateVisualState.Error);
            _reportError(exception);
        }
        finally
        {
            _preparationCancellation?.Dispose();
            _preparationCancellation = null;
            if (_owner.IsVisible) SetPreparing(UpdateSearchScope.Modules, false);
        }
    }

    internal async Task FinishModuleInstallationsAsync(bool confirm = true)
    {
        var installations = _pendingInstallations.Items;
        if (installations.Count == 0 || _preparationCancellation is not null) return;
        if (confirm && MessageBox.Show(_owner, _localize("Updates.FinishModuleInstallationsConfirm", []),
                _localize("Updates.Title", []), MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        try
        {
            _preparationCancellation = new CancellationTokenSource();
            SetPreparing(UpdateSearchScope.Modules, true);
            var launch = await _packagePreparation.CreateModuleLaunchAsync(
                installations, _preparationCancellation.Token);
            LaunchUpdater(launch, _pendingInstallations.Clear);
        }
        catch (OperationCanceledException)
        {
            SetModuleOperationStatus(_localize("Updates.PreparationCancelled", []));
        }
        catch (Exception exception)
        {
            SetModuleOperationStatus(_localize("Updates.PreparationFailed", []));
            _reportError(exception);
        }
        finally
        {
            _preparationCancellation?.Dispose();
            _preparationCancellation = null;
            if (_owner.IsVisible) SetPreparing(UpdateSearchScope.Modules, false);
        }
    }

    private async Task InstallAsync(UpdateSearchScope scope)
    {
        if (_preparationCancellation is not null) return;
        var isApplication = scope == UpdateSearchScope.Application;
        try
        {
            var result = isApplication
                ? await _applicationService.SearchAsync(_applicationSelections)
                : await _moduleService.SearchAsync(_moduleSelections);
            if (isApplication)
            {
                _applicationResult = result;
                RenderApplication(result);
            }
            else
            {
                _moduleResult = result;
                RenderModules(result);
            }
            if (!CanInstall(result)) return;
            if (MessageBox.Show(_owner, _localize("Updates.InstallConfirm", [result.Plan.Items.Count]),
                    _localize("Updates.Title", []), MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            _preparationCancellation = new CancellationTokenSource();
            SetPreparing(scope, true);
            var progressBar = isApplication ? _section.Progress : _section.ModulesProgress;
            var progress = new Progress<double>(value => progressBar.Value = value * 100);
            var launch = isApplication
                ? await _applicationService.PrepareAsync(result.Plan, progress, _preparationCancellation.Token)
                : await _packagePreparation.PrepareAsync(result.Plan, progress, _preparationCancellation.Token);
            LaunchUpdater(launch);
        }
        catch (OperationCanceledException)
        {
            SetStatus(scope, _localize("Updates.PreparationCancelled", []));
            if (scope == UpdateSearchScope.Application) _applicationState = UpdateVisualState.Neutral;
            else SetModuleVisualState(UpdateVisualState.Neutral);
        }
        catch (Exception exception)
        {
            SetStatus(scope, _localize("Updates.PreparationFailed", []));
            if (scope == UpdateSearchScope.Application) _applicationState = UpdateVisualState.Error;
            else SetModuleVisualState(UpdateVisualState.Error);
            _reportError(exception);
        }
        finally
        {
            _preparationCancellation?.Dispose();
            _preparationCancellation = null;
            if (_owner.IsVisible) SetPreparing(scope, false);
        }
    }

    private void LaunchUpdater(PreparedUpdateLaunch launch, Action? afterStart = null)
    {
        var start = new ProcessStartInfo(launch.UpdaterExecutable)
        {
            UseShellExecute = false,
            WorkingDirectory = launch.WorkingDirectory
        };
        start.ArgumentList.Add(UpdateOptionsConstants.ApplyUpdaterArgument);
        start.ArgumentList.Add(launch.PlanPath);
        if (Process.Start(start) is null)
            throw new InvalidOperationException(_localize("Updates.PreparationFailed", []));
        afterStart?.Invoke();
        _owner.Close();
        _ = Application.Current.Dispatcher.BeginInvoke(() => Application.Current.MainWindow?.Close());
    }

    private void Cancel(object sender, RoutedEventArgs e) => _preparationCancellation?.Cancel();

}
