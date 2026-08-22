using GWGUI.Domain.Settings.Emulation;
using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Contracts.Input;
using GWGUI.App.Functions.Input.Bindings;
using GWGUI.App.Localization.Extensions;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;


namespace GWGUI.App.Functions.Emulation.Shortcuts;

internal static class EmulationShortcutViewFunctions
{
    internal static Border CreateGroup(IReadOnlyList<GlobalShortcutBinding> configuredShortcuts,
        params (string Action, string ResourceKey)[] shortcuts)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };
        foreach (var shortcut in shortcuts)
            panel.Children.Add(CreateHint(configuredShortcuts, shortcut));
        var border = new Border
        {
            Child = panel,
            Height = 32,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(2, 1, 2, 1),
            Margin = new Thickness(2, 1, 2, 1),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };
        border.SetResourceReference(Border.BackgroundProperty, ControlVisualConstants.CardBrushResource);
        border.SetResourceReference(Border.BorderBrushProperty, ControlVisualConstants.BorderBrushResource);
        return border;
    }

    private static UIElement CreateHint(IReadOnlyList<GlobalShortcutBinding> configuredShortcuts,
        (string Action, string ResourceKey) shortcut)
    {
        var shortcutText = ResolveShortcutText(configuredShortcuts, shortcut.Action);
        var label = LocExtension.Get(shortcut.ResourceKey);
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 0, 5, 0)
        };
        panel.Children.Add(new TextBlock
        {
            Text = label,
            FontSize = 11,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 5, 0)
        });
        var key = new Border
        {
            CornerRadius = new CornerRadius(3),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(5, 1, 5, 1),
            Child = new TextBlock { Text = shortcutText, FontSize = 11, FontWeight = FontWeights.SemiBold }
        };
        key.SetResourceReference(Border.BackgroundProperty, ControlVisualConstants.ControlBrushResource);
        key.SetResourceReference(Border.BorderBrushProperty, ControlVisualConstants.BorderBrushResource);
        panel.Children.Add(key);
        AutomationProperties.SetName(panel, $"{label} {shortcutText}");
        return panel;
    }

    private static string ResolveShortcutText(IReadOnlyList<GlobalShortcutBinding> configuredShortcuts,
        string action)
    {
        var binding = configuredShortcuts.FirstOrDefault(item => item.Action == action)?.Chord;
        if (binding is null && EmulationShortcutDefaults.Values.TryGetValue(action, out var fallback))
            KeyboardChordFunctions.TryParse(fallback, out binding);
        return binding is null ? string.Empty : KeyboardChordFunctions.Format(binding.Modifiers, binding.Keys);
    }
}
