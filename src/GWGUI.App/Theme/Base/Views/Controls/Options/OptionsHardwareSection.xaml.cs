using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GWGUI.App.Views.Controls.Options;

public partial class OptionsHardwareSection : UserControl
{
    public OptionsHardwareSection() => InitializeComponent();
    public Button ScanAction => ScanButton;
    public Button AddDriveAction => AddDriveButton;
    public ListBox Drives => DrivesGrid;
    public TextBox GwPath => GwPathText;
    public Button DownloadAction => DownloadHostToolsButton;
    public ProgressBar DownloadProgress => HostToolsProgress;
    public TextBlock HostToolsState => HostToolsStatus;
    public event RoutedEventHandler? ScanRequested;
    public event RoutedEventHandler? AddDriveRequested;
    public event RoutedEventHandler? SaveDriveRequested;
    public event RoutedEventHandler? ForgetDriveRequested;
    public event KeyboardFocusChangedEventHandler? AutoSaveTextEditingFinished;
    public event RoutedEventHandler? BrowseGwRequested;
    public event RoutedEventHandler? DetectHostToolsRequested;
    public event RoutedEventHandler? CheckHostToolsRequested;
    public event RoutedEventHandler? DownloadHostToolsRequested;
    public event RoutedEventHandler? RollbackHostToolsRequested;
    private void ScanHardware_Click(object sender, RoutedEventArgs e) => ScanRequested?.Invoke(sender, e);
    private void AddDrive_Click(object sender, RoutedEventArgs e) => AddDriveRequested?.Invoke(sender, e);
    private void SaveHardwareRow_Click(object sender, RoutedEventArgs e) => SaveDriveRequested?.Invoke(sender, e);
    private void ForgetHardwareRow_Click(object sender, RoutedEventArgs e) => ForgetDriveRequested?.Invoke(sender, e);
    private void AutoSaveText_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => AutoSaveTextEditingFinished?.Invoke(sender, e);
    private void BrowseGw_Click(object sender, RoutedEventArgs e) => BrowseGwRequested?.Invoke(sender, e);
    private void DetectHostTools_Click(object sender, RoutedEventArgs e) => DetectHostToolsRequested?.Invoke(sender, e);
    private void CheckHostTools_Click(object sender, RoutedEventArgs e) => CheckHostToolsRequested?.Invoke(sender, e);
    private void DownloadHostTools_Click(object sender, RoutedEventArgs e) => DownloadHostToolsRequested?.Invoke(sender, e);
    private void RollbackHostTools_Click(object sender, RoutedEventArgs e) => RollbackHostToolsRequested?.Invoke(sender, e);
}
