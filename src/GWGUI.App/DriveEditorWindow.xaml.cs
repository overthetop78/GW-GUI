using System.Windows;
using System.Windows.Controls;
using GWGUI.Domain.Settings;

namespace GWGUI.App;

public partial class DriveEditorWindow : Window
{
    private readonly IReadOnlyList<ControllerSettings> _controllers;
    public DriveSettings? Drive { get; private set; }

    public DriveEditorWindow(IReadOnlyList<ControllerSettings> controllers)
    {
        InitializeComponent();
        _controllers = controllers;
        ControllerCombo.ItemsSource = controllers.Select(x => new ControllerChoice(x, $"{x.Model} — {x.LastPort}")).ToArray();
        ControllerCombo.SelectedIndex = controllers.Count > 0 ? 0 : -1;
        SelectionCombo.SelectedIndex = 0;
        SizeCombo.SelectedIndex = 1;
        DensityCombo.SelectedIndex = 0;
        RpmCombo.SelectedIndex = 0;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (ControllerCombo.SelectedIndex < 0) { MessageBox.Show(this, "Scannez ou ajoutez d’abord un contrôleur Greaseweazle.", "Lecteur"); return; }
        var controller = _controllers[ControllerCombo.SelectedIndex];
        Drive = new DriveSettings
        {
            ControllerUsbId = controller.UsbId,
            Selection = ((ComboBoxItem)SelectionCombo.SelectedItem).Content.ToString()!,
            Size = ((ComboBoxItem)SizeCombo.SelectedItem).Content.ToString()!.Replace(',', '.'),
            Density = ((ComboBoxItem)DensityCombo.SelectedItem).Tag.ToString()!,
            NominalRpm = RpmCombo.SelectedIndex switch { 1 => 300, 2 => 360, _ => null }
        };
        DialogResult = true;
    }

    private sealed record ControllerChoice(ControllerSettings Controller, string Label);
}
