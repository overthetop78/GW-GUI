using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GWGUI.App.Controls;

public partial class OptionsLogsSection : UserControl
{
    public OptionsLogsSection() => InitializeComponent();
    public ItemsControl OptionsList => LogOptionsList;
    public TextBlock DirectoryText => LogsDirectoryText;
    public event RoutedEventHandler? LogRowChanged;
    public event KeyboardFocusChangedEventHandler? MaximumSizeEditingFinished;
    public event TextCompositionEventHandler? NumericTextEntered;
    public event RoutedEventHandler? OpenLogsFolderRequested;
    private void LogRow_Changed(object sender, RoutedEventArgs e) => LogRowChanged?.Invoke(sender, e);
    private void LogRowMaximumSize_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => MaximumSizeEditingFinished?.Invoke(sender, e);
    private void NumericText_PreviewTextInput(object sender, TextCompositionEventArgs e) => NumericTextEntered?.Invoke(sender, e);
    private void OpenLogsFolder_Click(object sender, RoutedEventArgs e) => OpenLogsFolderRequested?.Invoke(sender, e);
}
