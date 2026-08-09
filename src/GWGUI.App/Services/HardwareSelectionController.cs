using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GWGUI.App.Controls;
using GWGUI.App.ViewModels;
using GWGUI.Domain.Hardware;
using GWGUI.Domain.Settings;

namespace GWGUI.App.Services;

public sealed record HardwareChoice(DriveSettings Drive, string Port, bool Available, string Label);

public sealed class HardwareSelectionController(
    ApplicationStatusBar statusBar,
    MainWindowViewModel viewModel,
    Func<AppSettings> settings,
    IMessageDialogService dialogs,
    Action<bool> setHardwareActionsEnabled,
    Action selectionChanged,
    Func<string, object[], string> localize)
{
    public void Refresh()
    {
        var selector = statusBar.HardwareChoices;
        var currentSettings = settings();
        var previousId = (selector.SelectedItem as HardwareChoice)?.Drive.Id;
        var choices = currentSettings.Drives.Select(drive =>
        {
            var controller = currentSettings.Controllers.FirstOrDefault(item => item.UsbId == drive.ControllerUsbId);
            if (controller is null) return null;
            var number = currentSettings.Drives.Where(item => item.ControllerUsbId == drive.ControllerUsbId).ToList().IndexOf(drive) + 1;
            var label = localize("Hardware.DriveChoice", [number, drive.Size, drive.Density, controller.LastPort]);
            return new HardwareChoice(drive, controller.LastPort, controller.IsAvailable,
                label + (controller.IsAvailable ? "" : $" ({localize("Hardware.Disconnected", [])})"));
        }).Where(choice => choice is not null).Cast<HardwareChoice>().ToArray();

        selector.ItemsSource = choices;
        selector.SelectedItem = choices.FirstOrDefault(choice => choice.Drive.Id == previousId) ?? choices.FirstOrDefault();
        var selectionRequired = choices.Length > 1;
        statusBar.HiddenHardwareSelectorItem.Visibility = Visibility.Collapsed;
        selector.Visibility = selectionRequired ? Visibility.Visible : Visibility.Collapsed;
        statusBar.HardwareText.Visibility = selectionRequired ? Visibility.Collapsed : Visibility.Visible;
        UpdateStatus();
    }

    public HardwareChoice? Selected => statusBar.HardwareChoices.SelectedItem as HardwareChoice;

    public string? DeviceArgument()
    {
        var currentSettings = settings();
        return HardwareRoutingPolicy.DeviceArgument(currentSettings.Controllers, currentSettings.Drives, Selected?.Drive);
    }

    public string? DriveArgument() => HardwareRoutingPolicy.DriveArgument(settings().Drives, Selected?.Drive);

    public void OnSelectionChanged()
    {
        UpdateStatus();
        selectionChanged();
    }

    public bool EnsureAvailable()
    {
        if (Selected is not { Available: false }) return true;
        dialogs.Show(localize("Hardware.SelectedDisconnected", []), localize("Menu.Hardware", []), icon: UserDialogIcon.Warning);
        return false;
    }

    private void UpdateStatus()
    {
        var selected = Selected;
        var enabled = selected is not { Available: false };
        viewModel.HardwareText = selected is null ? localize("Hardware.NotConfigured", []) : selected.Label;
        viewModel.HardwareBrush = new SolidColorBrush(selected?.Available == true
            ? Color.FromRgb(63, 171, 91)
            : Color.FromRgb(136, 136, 136));
        setHardwareActionsEnabled(enabled);
    }
}
