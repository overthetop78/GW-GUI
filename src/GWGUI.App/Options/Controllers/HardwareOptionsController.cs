using GWGUI.Domain.Hardware;
using GWGUI.Domain.Settings;
using GWGUI.Domain.Settings.Hardware;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Options.States;
using GWGUI.App.ViewModels.Options;
using GWGUI.App.Views.Controls.Options;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

using GWGUI.Infrastructure.Hardware;


namespace GWGUI.App.Options.Controllers;

internal sealed class HardwareOptionsController(
    Window owner,
    OptionsHardwareSection section,
    HardwareOptionsState state,
    ObservableCollection<HardwareRow> rows,
    IHardwareRegistry registry,
    Func<string?> executablePath,
    Func<Task> persistSettings,
    Action<Exception, string, string, MessageBoxImage> showLoggedError)
{
    public void Initialize()
    {
        RefreshRows();
        section.Drives.ItemsSource = rows;
    }

    public async Task ScanAsync()
    {
        var path = executablePath();
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            MessageBox.Show(owner, LocExtension.Get("Hardware.GwRequired"), LocExtension.Get("Hardware.ScanTitle"));
            return;
        }

        section.ScanAction.IsEnabled = false;
        try
        {
            var scanned = await registry.ScanAsync(path, state.Controllers);
            state.Controllers.Clear();
            state.Controllers.AddRange(scanned.ConfiguredControllers);
            MergeUnconfigured(scanned.UnconfiguredControllers);
            RefreshRows();
            await persistSettings();
        }
        catch (Exception exception)
        {
            showLoggedError(exception, "Scanning hardware", "Hardware.ScanTitle", MessageBoxImage.Error);
        }
        finally
        {
            section.ScanAction.IsEnabled = true;
        }
    }

    public void AddDrive()
    {
        var selected = section.Drives.SelectedItem as HardwareRow;
        var controllerId = selected?.UsbId ?? (state.Controllers.Count == 1 ? state.Controllers[0].UsbId : null);
        if (controllerId is null)
        {
            MessageBox.Show(owner, LocExtension.Get("Hardware.SelectController"), LocExtension.Get("Hardware.DriveDialogTitle"));
            return;
        }
        if (state.HasMaximumDrives(controllerId))
        {
            MessageBox.Show(owner, LocExtension.Get("Hardware.MaximumDrives"), LocExtension.Get("Hardware.DriveDialogTitle"));
            return;
        }
        rows.Add(state.CreateDraftRow(controllerId));
        section.Drives.SelectedItem = rows[^1];
    }

    public async Task SaveAsync(HardwareRow row)
    {
        if (!state.Save(row))
        {
            MessageBox.Show(owner, LocExtension.Get("Hardware.MaximumDrives"), LocExtension.Get("Hardware.DriveDialogTitle"));
            return;
        }
        RefreshRows();
        await persistSettings();
    }

    public async Task ForgetAsync(HardwareRow row)
    {
        var lastDrive = row.DriveId is not null && state.Drives.Count(item => item.ControllerUsbId == row.UsbId) == 1;
        var message = lastDrive ? LocExtension.Get("Hardware.ForgetLastConfirm") : LocExtension.Get("Hardware.ForgetConfirm");
        if (MessageBox.Show(owner, message, LocExtension.Get("Hardware.Forget"), MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;
        Remove(row);
        await persistSettings();
    }

    public void Remove(HardwareRow row)
    {
        if (row.DriveId is null && row.Configured)
        {
            rows.Remove(row);
            return;
        }
        state.Remove(row);
        RefreshRows();
    }

    public void RefreshRows()
    {
        rows.Clear();
        foreach (var row in state.CreateRows()) rows.Add(row);
    }

    public void MergeUnconfigured(IReadOnlyList<ControllerSettings> detectedControllers) =>
        state.MergeUnconfigured(detectedControllers);

    public void ApplyTo(AppSettings settings)
    {
        settings.Controllers = state.Controllers;
        settings.UnconfiguredControllers = state.UnconfiguredControllers;
        settings.Drives = state.Drives;
    }
}
