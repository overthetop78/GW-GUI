using GWGUI.App.Options.Controllers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace GWGUI.App.Views.Controls.Options;

public partial class OptionsUpdatesSection : UserControl
{
    private IReadOnlyList<UpdateNavigationItem> _navigationItems = [];

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
    public ProgressBar AvailableModulesProgress => DirectoryProgress;
    public ProgressBar ManualInstallationProgress => AdvancedProgress;
    public TextBlock ModuleStatus => ModulesStatus;
    public TextBlock AvailableModuleStatus => AvailableModulesStatus;
    public TextBlock ManualInstallationStatus => AdvancedStatus;
    public ItemsControl ModuleResults => ModulesResultsList;
    public ItemsControl AvailableModuleResults => AvailableModulesList;
    public Button InstallModuleFileButton => InstallModuleFileAction;
    public Button InstallModuleUrlButton => InstallModuleUrlAction;
    public TextBox ModuleCatalogUrl => ModuleCatalogUrlInput;
    internal ListBox NavigationControl => UpdateNavigation;
    public string? SelectedModuleId => (UpdateNavigation.SelectedItem as UpdateNavigationItem)?.ModuleId;

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
    public event Action<string?>? SelectedModuleChanged;

    internal void SetNavigationItems(IReadOnlyList<UpdateNavigationItem> items)
    {
        var selectedKey = (UpdateNavigation.SelectedItem as UpdateNavigationItem)?.Key;
        _navigationItems = items;
        UpdateNavigation.ItemsSource = items;
        UpdateNavigation.SelectedItem = items.FirstOrDefault(item => item.Key == selectedKey)
            ?? items.FirstOrDefault();
    }

    internal void RefreshLocalizedContent() => ShowSelectedPage();

    private void Navigation_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ShowSelectedPage();
        SelectedModuleChanged?.Invoke(SelectedModuleId);
    }

    private void ShowSelectedPage()
    {
        var selected = UpdateNavigation.SelectedItem as UpdateNavigationItem
            ?? _navigationItems.FirstOrDefault();
        ApplicationPage.Visibility = selected?.Page == UpdateNavigationPage.Application
            ? Visibility.Visible : Visibility.Collapsed;
        DirectoryPage.Visibility = selected?.Page == UpdateNavigationPage.Directory
            ? Visibility.Visible : Visibility.Collapsed;
        InstalledModulePage.Visibility = selected?.Page == UpdateNavigationPage.Module
            ? Visibility.Visible : Visibility.Collapsed;
        AdvancedPage.Visibility = selected?.Page == UpdateNavigationPage.Advanced
            ? Visibility.Visible : Visibility.Collapsed;

        if (selected?.Page != UpdateNavigationPage.Module) return;
        SelectedModuleTitle.Text = selected.Title;
        SelectedModuleVersion.Text = selected.Subtitle;
        FilterModuleResults(selected.ModuleId);
    }

    internal void FilterModuleResults(string? moduleId)
    {
        if (ModulesResultsList.ItemsSource is null) return;
        var view = CollectionViewSource.GetDefaultView(ModulesResultsList.ItemsSource);
        view.Filter = item => item is UpdateComponentRow row
            && string.Equals(row.Id, moduleId, StringComparison.OrdinalIgnoreCase);
        view.Refresh();
    }

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

internal enum UpdateNavigationPage
{
    Application,
    Directory,
    Module,
    Advanced
}

internal enum UpdateVisualState
{
    Neutral,
    Current,
    Available,
    Busy,
    Error
}

internal sealed class UpdateNavigationItem
{
    internal UpdateNavigationItem(string key, UpdateNavigationPage page, string title, string subtitle,
        string icon, UpdateVisualState state = UpdateVisualState.Neutral, string? moduleId = null)
    {
        Key = key;
        Page = page;
        Title = title;
        Subtitle = subtitle;
        Icon = icon;
        ModuleId = moduleId;
        (Foreground, Background) = UpdateStateBrushes.For(state);
    }

    public string Key { get; }
    public UpdateNavigationPage Page { get; }
    public string Title { get; }
    public string Subtitle { get; }
    public string Icon { get; }
    public string? ModuleId { get; }
    public Brush Foreground { get; }
    public Brush Background { get; }
}

internal static class UpdateStateBrushes
{
    private static readonly Brush NeutralForeground = Frozen(Color.FromRgb(0x4F, 0x6B, 0x86));
    private static readonly Brush NeutralBackground = Frozen(Color.FromArgb(0x28, 0x4F, 0x6B, 0x86));
    private static readonly Brush CurrentForeground = Frozen(Color.FromRgb(0x1E, 0x8E, 0x5A));
    private static readonly Brush CurrentBackground = Frozen(Color.FromArgb(0x2E, 0x1E, 0x8E, 0x5A));
    private static readonly Brush AvailableForeground = Frozen(Color.FromRgb(0xD1, 0x78, 0x00));
    private static readonly Brush AvailableBackground = Frozen(Color.FromArgb(0x32, 0xD1, 0x78, 0x00));
    private static readonly Brush BusyForeground = Frozen(Color.FromRgb(0x00, 0x78, 0xD4));
    private static readonly Brush BusyBackground = Frozen(Color.FromArgb(0x30, 0x00, 0x78, 0xD4));
    private static readonly Brush ErrorForeground = Frozen(Color.FromRgb(0xC4, 0x2B, 0x1C));
    private static readonly Brush ErrorBackground = Frozen(Color.FromArgb(0x30, 0xC4, 0x2B, 0x1C));

    internal static (Brush Foreground, Brush Background) For(UpdateVisualState state) => state switch
    {
        UpdateVisualState.Current => (CurrentForeground, CurrentBackground),
        UpdateVisualState.Available => (AvailableForeground, AvailableBackground),
        UpdateVisualState.Busy => (BusyForeground, BusyBackground),
        UpdateVisualState.Error => (ErrorForeground, ErrorBackground),
        _ => (NeutralForeground, NeutralBackground)
    };

    private static Brush Frozen(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}
