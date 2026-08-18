using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Localization;

namespace GWGUI.App.Controls;

internal sealed record EmulationMemorySettingsContent(
    IReadOnlyList<EmulationSettingsField> MainMemory,
    TextBlock MainMemoryHint,
    IReadOnlyList<EmulationSettingsField> MemoryExtensions,
    TextBlock MemoryExtensionsHint,
    TextBlock TotalMemory);

internal static partial class EmulationSettingsLayout
{
    private const string MainMemoryIcon = "\uE964";
    private const string MemoryExtensionsIcon = "\uE950";

    internal static Grid MemorySettingsPage(EmulationMemorySettingsContent settings)
    {
        var mainMemory = new StackPanel();
        mainMemory.Children.Add(SettingsFieldGrid(settings.MainMemory.Select(field => (field.Label, field.Control)).ToArray()));
        mainMemory.Children.Add(InformationBanner(settings.MainMemoryHint));

        var extensions = new StackPanel();
        extensions.Children.Add(SettingsFieldGrid(settings.MemoryExtensions.Select(field => (field.Label, field.Control)).ToArray()));
        extensions.Children.Add(InformationBanner(settings.MemoryExtensionsHint));

        var root = TwoColumnPage(
            IconCard(mainMemory, LocExtension.Get("Emulation.Memory.Main"), MainMemoryIcon),
            IconCard(extensions, LocExtension.Get("Emulation.Memory.Extensions"), MemoryExtensionsIcon));
        root.Children.Add(MemorySummaryCard(settings.TotalMemory));
        return root;
    }

    private static Border MemorySummaryCard(TextBlock totalMemory)
    {
        totalMemory.VerticalAlignment = VerticalAlignment.Center;
        totalMemory.FontSize = 16;
        var content = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(14, 10, 14, 10) };
        var icon = new TextBlock { Text = MainMemoryIcon, FontFamily = ControlVisualConstants.IconFont, FontSize = 22,
            VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 12, 0) };
        icon.SetResourceReference(TextBlock.ForegroundProperty, "AccentBrush");
        content.Children.Add(icon);
        content.Children.Add(totalMemory);
        var card = new Border { Child = content, Padding = new Thickness(2), Margin = new Thickness(0, 10, 0, 0) };
        card.SetResourceReference(FrameworkElement.StyleProperty, "Card");
        Grid.SetRow(card, 1);
        Grid.SetColumnSpan(card, 2);
        return card;
    }
}
