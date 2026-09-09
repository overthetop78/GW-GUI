using System.Windows;
using System.Windows.Controls;

namespace GWGUI.App.Views.Controls.Options;

public partial class OptionsUpdatesSection : UserControl
{
    public OptionsUpdatesSection() => InitializeComponent();

    public Button SearchButton => SearchAction;
    public Button InstallButton => InstallAction;
    public Button CancelButton => CancelAction;
    public ProgressBar Progress => PreparationProgress;
    public TextBlock Status => SearchStatus;
    public ItemsControl Results => ResultsList;
    public Button SearchModulesButton => SearchModulesAction;
    public Button SearchAvailableModulesButton => SearchAvailableModulesAction;
    public Button InstallModulesButton => InstallModulesAction;
    public Button CancelModulesButton => CancelModulesAction;
    public ProgressBar ModulesProgress => ModulesPreparationProgress;
    public TextBlock ModuleStatus => ModulesStatus;
    public ItemsControl ModuleResults => ModulesResultsList;
    public TextBlock AvailableModuleStatus => AvailableModulesStatus;
    public ItemsControl AvailableModuleResults => AvailableModulesList;
    public Button InstallModuleFileButton => InstallModuleFileAction;
    public Button InstallModuleUrlButton => InstallModuleUrlAction;
    public TextBox ModuleCatalogUrl => ModuleCatalogUrlInput;

    public event RoutedEventHandler? SearchRequested;
    public event SelectionChangedEventHandler? VersionSelectionChanged;
    public event RoutedEventHandler? InstallRequested;
    public event RoutedEventHandler? CancelRequested;
    public event RoutedEventHandler? SearchModulesRequested;
    public event RoutedEventHandler? SearchAvailableModulesRequested;
    public event RoutedEventHandler? InstallAvailableModuleRequested;
    public event RoutedEventHandler? InstallModulesRequested;
    public event RoutedEventHandler? CancelModulesRequested;
    public event RoutedEventHandler? InstallModuleFileRequested;
    public event RoutedEventHandler? InstallModuleUrlRequested;

    private void Search_Click(object sender, RoutedEventArgs e) => SearchRequested?.Invoke(sender, e);
    private void Version_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
        VersionSelectionChanged?.Invoke(sender, e);
    private void Install_Click(object sender, RoutedEventArgs e) => InstallRequested?.Invoke(sender, e);
    private void Cancel_Click(object sender, RoutedEventArgs e) => CancelRequested?.Invoke(sender, e);
    private void SearchModules_Click(object sender, RoutedEventArgs e) => SearchModulesRequested?.Invoke(sender, e);
    private void SearchAvailableModules_Click(object sender, RoutedEventArgs e) =>
        SearchAvailableModulesRequested?.Invoke(sender, e);
    private void InstallAvailableModule_Click(object sender, RoutedEventArgs e) =>
        InstallAvailableModuleRequested?.Invoke(sender, e);
    private void InstallModules_Click(object sender, RoutedEventArgs e) => InstallModulesRequested?.Invoke(sender, e);
    private void CancelModules_Click(object sender, RoutedEventArgs e) => CancelModulesRequested?.Invoke(sender, e);
    private void InstallModuleFile_Click(object sender, RoutedEventArgs e) => InstallModuleFileRequested?.Invoke(sender, e);
    private void InstallModuleUrl_Click(object sender, RoutedEventArgs e) => InstallModuleUrlRequested?.Invoke(sender, e);
}
