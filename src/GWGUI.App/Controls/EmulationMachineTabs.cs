using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using GWGUI.App.Localization;

namespace GWGUI.App.Controls;

internal static class EmulationMachineTabs
{
    internal const double HorizontalPadding = 14;
    internal const double VerticalPadding = 9;
    internal const double OuterMargin = 8;

    internal static readonly IReadOnlyList<EmulationMachineTabDefinition> Definitions =
    [
        new(EmulationMachineTab.General, "\uE713", "Emulation.Tab.General"),
        new(EmulationMachineTab.Cpu, "\uE950", "Emulation.Tab.Cpu"),
        new(EmulationMachineTab.Ram, "\uE964", "Emulation.Tab.Ram"),
        new(EmulationMachineTab.Rom, "\uE8B7", "Emulation.Tab.Rom"),
        new(EmulationMachineTab.Video, "\uE7F4", "Emulation.Tab.Video"),
        new(EmulationMachineTab.Audio, "\uE767", "Emulation.Audio"),
        new(EmulationMachineTab.Storage, "\uEDA2", "Emulation.Tab.Storage"),
        new(EmulationMachineTab.Keyboard, "\uE765", "Emulation.Tab.Keyboard"),
        new(EmulationMachineTab.Mouse, "\uE962", "Emulation.Tab.Mouse"),
        new(EmulationMachineTab.Controllers, "\uE7FC", "Emulation.Controller.Tab")
    ];

    internal static TabControl Create(Func<EmulationMachineTab, UIElement?> contentProvider,
        string? automationName = null, Func<EmulationMachineTab, Task>? tabActivated = null)
    {
        var tabs = new TabControl
        {
            Margin = new Thickness(OuterMargin),
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch
        };
        if (!string.IsNullOrWhiteSpace(automationName)) AutomationProperties.SetName(tabs, automationName);
        foreach (var definition in Definitions)
        {
            var content = contentProvider(definition.Tab);
            if (content is null) continue;
            var title = LocExtension.Get(definition.ResourceKey);
            var tab = new TabItem
            {
                Tag = definition.Tab,
                Header = new MainTabHeader { Icon = definition.Icon, Text = title },
                Content = content,
                Padding = new Thickness(HorizontalPadding, VerticalPadding,
                    HorizontalPadding, VerticalPadding),
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
