using System.Windows;
using System.Windows.Controls;

namespace GWGUI.App.Views.Controls.Shell;

public partial class MainMenu : UserControl
{
    public MainMenu() => InitializeComponent();

    public event RoutedEventHandler? PreferencesRequested;
    public event RoutedEventHandler? LogHistoryRequested;
    public event RoutedEventHandler? DocumentationRequested;
    public event RoutedEventHandler? AboutRequested;
    public event Action<object, string>? ToolRequested;

    public MenuItem OptionsMenuItem => Options;
    public MenuItem HelpMenuItem => Help;
    public MenuItem AlignMenuItem => Align;

    private void Preferences_Click(object sender, RoutedEventArgs e) => PreferencesRequested?.Invoke(sender, e);
    private void LogHistory_Click(object sender, RoutedEventArgs e) => LogHistoryRequested?.Invoke(sender, e);
    private void Documentation_Click(object sender, RoutedEventArgs e) => DocumentationRequested?.Invoke(sender, e);
    private void About_Click(object sender, RoutedEventArgs e) => AboutRequested?.Invoke(sender, e);
    private void Tool_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem { Tag: string verb }) ToolRequested?.Invoke(sender, verb);
    }
}
