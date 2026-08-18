using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using GWGUI.App.Localization;

namespace GWGUI.App.Controls;

internal sealed record EmulationDefaultFolderRow(string Label, TextBox Value, Func<Task> Browse);

internal static partial class EmulationSettingsLayout
{
    private const double DefaultFolderLabelWidth = 320;
    private const double DefaultFolderControlHeight = 32;
    private const double DefaultFolderBrowseButtonWidth = 110;

    internal static Border DefaultFoldersCard(string title, params EmulationDefaultFolderRow[] rows)
    {
        var content = new StackPanel { Margin = new Thickness(10, 6, 10, 2) };
        foreach (var rowDefinition in rows)
        {
            var row = new Grid();
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(DefaultFolderLabelWidth) });
            row.ColumnDefinitions.Add(new ColumnDefinition());
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.Children.Add(new TextBlock { Text = rowDefinition.Label, VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0) });

            rowDefinition.Value.MinWidth = 0;
            rowDefinition.Value.Height = DefaultFolderControlHeight;
            AutomationProperties.SetName(rowDefinition.Value, rowDefinition.Label);
            Grid.SetColumn(rowDefinition.Value, 1);
            row.Children.Add(rowDefinition.Value);

            var browse = ControlUiFactory.TextButton(LocExtension.Get("Common.Browse"), DefaultFolderBrowseButtonWidth,
                async (_, _) => await rowDefinition.Browse(), new Thickness(8, 0, 0, 0), DefaultFolderControlHeight);
            AutomationProperties.SetName(browse, $"{LocExtension.Get("Common.Browse")}: {rowDefinition.Label}");
            Grid.SetColumn(browse, 2);
            row.Children.Add(browse);
            content.Children.Add(row);
        }
        return DefaultValuesCard(content, title);
    }

    private static Border DefaultValuesCard(UIElement child, string title)
    {
        var panel = new StackPanel();
        panel.Children.Add(new TextBlock { Text = title, FontWeight = FontWeights.SemiBold, FontSize = 16,
            Margin = new Thickness(10, 8, 10, 2) });
        panel.Children.Add(child);
        var card = new Border { Child = panel, Padding = new Thickness(2) };
        card.SetResourceReference(FrameworkElement.StyleProperty, "Card");
        return card;
    }
}
