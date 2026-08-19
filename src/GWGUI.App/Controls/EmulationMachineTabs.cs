using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using GWGUI.App.Localization;

namespace GWGUI.App.Controls;

internal enum EmulationMachineTabKind
{
    General,
    Cpu,
    Ram,
    Rom,
    Video,
    Audio,
    Storage,
    Keyboard,
    Mouse,
    Controllers
}

internal readonly record struct EmulationMachineTabDefinition(
    EmulationMachineTabKind Kind, string Icon, string ResourceKey);

internal static class EmulationMachineTabs
{
    internal const double HorizontalPadding = 14;
    internal const double VerticalPadding = 9;
    internal const double OuterMargin = 8;

    internal static readonly IReadOnlyList<EmulationMachineTabDefinition> Definitions =
    [
        new(EmulationMachineTabKind.General, "\uE713", "Emulation.Tab.General"),
        new(EmulationMachineTabKind.Cpu, "\uE950", "Emulation.Tab.Cpu"),
        new(EmulationMachineTabKind.Ram, "\uE964", "Emulation.Tab.Ram"),
        new(EmulationMachineTabKind.Rom, "\uE8B7", "Emulation.Tab.Rom"),
        new(EmulationMachineTabKind.Video, "\uE7F4", "Emulation.Tab.Video"),
        new(EmulationMachineTabKind.Audio, "\uE767", "Emulation.Audio"),
        new(EmulationMachineTabKind.Storage, "\uEDA2", "Emulation.Tab.Storage"),
        new(EmulationMachineTabKind.Keyboard, "\uE765", "Emulation.Tab.Keyboard"),
        new(EmulationMachineTabKind.Mouse, "\uE962", "Emulation.Tab.Mouse"),
        new(EmulationMachineTabKind.Controllers, "\uE7FC", "Emulation.Controller.Tab")
    ];

    internal static TabControl Create(Func<EmulationMachineTabKind, UIElement?> contentProvider,
        string? automationName = null, Func<EmulationMachineTabKind, Task>? tabActivated = null)
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
            var content = contentProvider(definition.Kind);
            if (content is null) continue;
            var title = LocExtension.Get(definition.ResourceKey);
            var tab = new TabItem
            {
                Tag = definition.Kind,
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
                if (args.Source == tabs && tabs.SelectedItem is TabItem { Tag: EmulationMachineTabKind kind })
                    await tabActivated(kind);
            };
        return tabs;
    }
}
