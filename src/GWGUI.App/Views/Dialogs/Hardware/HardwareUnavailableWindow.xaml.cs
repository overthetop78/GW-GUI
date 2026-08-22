using GWGUI.Domain.Settings.Hardware;
using GWGUI.App.Enums.Services.Dialogs;
using System.Windows;
using System.Windows.Controls;

namespace GWGUI.App.Views.Dialogs.Hardware;

public partial class HardwareUnavailableWindow : Window
{
    public MissingHardwareChoice Choice { get; private set; } = MissingHardwareChoice.Continue;

    public HardwareUnavailableWindow(IReadOnlyList<ControllerSettings> controllers)
    {
        InitializeComponent();
        MissingControllers.ItemsSource = controllers.Select(controller =>
            $"{controller.Model} — {controller.UsbId} — {controller.LastPort}").ToArray();
    }

    private void Choice_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string value } && Enum.TryParse(value, out MissingHardwareChoice choice))
            Choice = choice;
        DialogResult = true;
    }
}
