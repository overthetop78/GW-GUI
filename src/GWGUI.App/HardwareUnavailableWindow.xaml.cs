using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Services;
using GWGUI.Domain.Settings;

namespace GWGUI.App;

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
