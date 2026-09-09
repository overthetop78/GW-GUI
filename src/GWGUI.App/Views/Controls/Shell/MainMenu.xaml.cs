using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Localization.Extensions;
using GWGUI.Emulation;

namespace GWGUI.App.Views.Controls.Shell;

public partial class MainMenu : UserControl
{
    public MainMenu() => InitializeComponent();

    public event RoutedEventHandler? PreferencesRequested;
    public event RoutedEventHandler? UpdatesRequested;
    public event RoutedEventHandler? EmulationPreferencesRequested;
    public event Action<string>? EmulationModuleRequested;
    public event RoutedEventHandler? LogHistoryRequested;
    public event RoutedEventHandler? DocumentationRequested;
    public event RoutedEventHandler? AboutRequested;
    public event Action<object, string>? ToolRequested;

    public MenuItem OptionsMenuItem => Options;
    public MenuItem EmulationMenuItem => Emulation;
    public MenuItem HelpMenuItem => Help;
    public MenuItem AlignMenuItem => Align;

    private void Preferences_Click(object sender, RoutedEventArgs e) => PreferencesRequested?.Invoke(sender, e);
    private void Updates_Click(object sender, RoutedEventArgs e) => UpdatesRequested?.Invoke(sender, e);
    private void EmulationPreferences_Click(object sender, RoutedEventArgs e) =>
        EmulationPreferencesRequested?.Invoke(sender, e);

    public void SetEmulationModules(IEnumerable<IEmulationModule> modules)
    {
        while (Emulation.Items.Count > 2) Emulation.Items.RemoveAt(Emulation.Items.Count - 1);

        var entries = modules.OrderBy(module => LocExtension.GetForModule(module, module.DisplayResourceKey),
            StringComparer.CurrentCultureIgnoreCase).ToArray();
        EmulationModulesSeparator.Visibility = entries.Length == 0 ? Visibility.Collapsed : Visibility.Visible;
        foreach (var module in entries)
        {
            var item = new MenuItem
            {
                Header = LocExtension.GetForModule(module, module.DisplayResourceKey),
                Tag = module.Id,
                Icon = new TextBlock { FontFamily = new("Segoe MDL2 Assets"), Text = "\uE7FC" }
            };
            item.Click += EmulationModule_Click;
            Emulation.Items.Add(item);
        }
    }

    private void EmulationModule_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem { Tag: string moduleId }) EmulationModuleRequested?.Invoke(moduleId);
    }
    private void LogHistory_Click(object sender, RoutedEventArgs e) => LogHistoryRequested?.Invoke(sender, e);
    private void Documentation_Click(object sender, RoutedEventArgs e) => DocumentationRequested?.Invoke(sender, e);
    private void About_Click(object sender, RoutedEventArgs e) => AboutRequested?.Invoke(sender, e);
    private void Tool_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem { Tag: string verb }) ToolRequested?.Invoke(sender, verb);
    }
}
