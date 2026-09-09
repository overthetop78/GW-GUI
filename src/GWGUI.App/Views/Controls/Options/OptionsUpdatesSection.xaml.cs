using System.Windows;
using System.Windows.Controls;

namespace GWGUI.App.Views.Controls.Options;

public partial class OptionsUpdatesSection : UserControl
{
    public OptionsUpdatesSection() => InitializeComponent();

    public ComboBox Scope => ScopeSelector;
    public Button SearchButton => SearchAction;
    public Button InstallButton => InstallAction;
    public Button CancelButton => CancelAction;
    public ProgressBar Progress => PreparationProgress;
    public TextBlock Status => SearchStatus;
    public ItemsControl Results => ResultsList;

    public event RoutedEventHandler? SearchRequested;
    public event SelectionChangedEventHandler? VersionSelectionChanged;
    public event RoutedEventHandler? InstallRequested;
    public event RoutedEventHandler? CancelRequested;

    private void Search_Click(object sender, RoutedEventArgs e) => SearchRequested?.Invoke(sender, e);
    private void Version_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
        VersionSelectionChanged?.Invoke(sender, e);
    private void Install_Click(object sender, RoutedEventArgs e) => InstallRequested?.Invoke(sender, e);
    private void Cancel_Click(object sender, RoutedEventArgs e) => CancelRequested?.Invoke(sender, e);
}
