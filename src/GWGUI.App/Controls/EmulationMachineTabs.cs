using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using GWGUI.App.Constants;
using GWGUI.App.Localization;
using GWGUI.Emulation;

namespace GWGUI.App.Controls;

internal static class EmulationMachineTabs
{
    internal static TabControl Create(Func<EmulationMachineTab, UIElement?> contentProvider,
        string? automationName = null, Func<EmulationMachineTab, Task>? tabActivated = null)
    {
        var tabs = new TabControl
        {
            Margin = new Thickness(EmulationMachineTabConstants.OuterMargin),
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch
        };
        if (!string.IsNullOrWhiteSpace(automationName)) AutomationProperties.SetName(tabs, automationName);
        foreach (var definition in EmulationMachineTabConstants.Definitions)
        {
            var content = contentProvider(definition.Tab);
            if (content is null) continue;
            var title = LocExtension.Get(definition.ResourceKey);
            var tab = new TabItem
            {
                Tag = definition.Tab,
                Header = new MainTabHeader { Icon = definition.Icon, Text = title },
                Content = content,
                Padding = new Thickness(EmulationMachineTabConstants.HorizontalPadding,
                    EmulationMachineTabConstants.VerticalPadding,
                    EmulationMachineTabConstants.HorizontalPadding,
                    EmulationMachineTabConstants.VerticalPadding),
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch
            };
            AutomationProperties.SetName(tab, title);
            tab.SetResourceReference(FrameworkElement.StyleProperty, "MainTabItemStyle");
            tabs.Items.Add(tab);
        }
        if (tabActivated is not null)
            tabs.SelectionChanged += async (_, args) =>
            {
                if (args.Source == tabs && tabs.SelectedItem is TabItem { Tag: EmulationMachineTab tab })
                    await tabActivated(tab);
            };
        return tabs;
    }
}
