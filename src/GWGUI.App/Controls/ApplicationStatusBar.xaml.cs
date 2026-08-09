using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;

namespace GWGUI.App.Controls;

public partial class ApplicationStatusBar : UserControl
{
    public ApplicationStatusBar() => InitializeComponent();

    public event SelectionChangedEventHandler? HardwareSelectionChanged;
    public event RoutedEventHandler? HostToolsUpdateRequested;
    public event RoutedEventHandler? ToggleConsoleRequested;

    public Ellipse HardwareLight => HardwareStatusLight;
    public TextBlock HardwareText => HardwareStatusText;
    public ComboBox HardwareChoices => HardwareSelector;
    public StatusBarItem HiddenHardwareSelectorItem => HardwareSelectorItem;
    public StatusBarItem ProfileItem => ProfileStatusItem;
    public TextBlock ProfileText => ProfileStatusText;
    public StatusBarItem OperationItem => OperationStatusItem;
    public Ellipse OperationLight => OperationStatusLight;
    public TextBlock OperationText => OperationStatusText;
    public StatusBarItem ProgressItem => ProgressStatusItem;
    public Grid GlobalProgress => GlobalProgressPanel;
    public ProgressBar ProgressBar => OperationProgress;
    public TextBlock ProgressText => OperationProgressText;
    public TrackProgressStrip Face0Progress => Face0TrackProgress;
    public TrackProgressStrip Face1Progress => Face1TrackProgress;
    public StatusBarItem HostToolsItem => HostToolsUpdateItem;
    public Button HostToolsButton => HostToolsUpdateButton;

    private void HardwareSelector_Changed(object sender, SelectionChangedEventArgs e) => HardwareSelectionChanged?.Invoke(sender, e);
    private void HostToolsUpdateButton_Click(object sender, RoutedEventArgs e) => HostToolsUpdateRequested?.Invoke(sender, e);
    private void ToggleConsoleButton_Click(object sender, RoutedEventArgs e) => ToggleConsoleRequested?.Invoke(sender, e);
}
