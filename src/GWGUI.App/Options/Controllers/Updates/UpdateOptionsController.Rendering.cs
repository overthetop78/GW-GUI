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
    private void RenderApplication(UpdateSearchResult result)
    {
        Render(result, ApplicationResults);
        _section.Status.Text = ResultStatus(result);
        _section.InstallButton.IsEnabled = CanInstall(result) && _preparationCancellation is null;
    }

    private void RenderModules(UpdateSearchResult result)
    {
        Render(result, ModuleResults);
        _section.ModuleStatus.Text = ResultStatus(result);
        _section.InstallModulesButton.IsEnabled = CanInstall(result) && _preparationCancellation is null;
        _section.FilterModuleResults(_section.SelectedModuleId);
    }

    private void Render(UpdateSearchResult result, ObservableCollection<UpdateComponentRow> target)
    {
        target.Clear();
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
            target.Add(new(update.ComponentId, update.Kind, displayName,
                _localize("Updates.InstalledVersion", [update.InstalledVersion]), status,
                update.Releases.Select(release => release.Version).ToArray(), update.SelectedVersion,
                update.Availability == UpdateAvailability.Available, VisualStateFor(update.Availability)));
        }
    }

    private static UpdateVisualState StateFor(UpdateSearchResult result) =>
        result.Components.Any(update => update.Availability is UpdateAvailability.Incompatible
            or UpdateAvailability.ApplicationUpdateRequired)
            ? UpdateVisualState.Error
            : result.Components.Any(update => update.Availability == UpdateAvailability.Available)
                ? UpdateVisualState.Available
                : UpdateVisualState.Current;

    private static UpdateVisualState VisualStateFor(UpdateAvailability availability) => availability switch
    {
        UpdateAvailability.UpToDate => UpdateVisualState.Current,
        UpdateAvailability.Available => UpdateVisualState.Available,
        _ => UpdateVisualState.Error
    };

    private string ResultStatus(UpdateSearchResult result) =>
        result.Components.Any(update => update.Availability != UpdateAvailability.UpToDate)
            ? _localize("Updates.Results", []) : _localize("Updates.NoUpdates", []);

}
