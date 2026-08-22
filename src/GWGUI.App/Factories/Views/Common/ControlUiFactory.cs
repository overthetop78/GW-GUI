using GWGUI.App.Constants.Controls.Visual;
using System.Windows;
using System.Windows.Controls;

namespace GWGUI.App.Factories.Views.Common;

internal static class ControlUiFactory
{
    internal static Button TextButton(string text, double minWidth, RoutedEventHandler click,
        Thickness? margin = null, double? height = null)
    {
        var button = new Button { Content = text, MinWidth = minWidth, Margin = margin ?? new Thickness(0) };
        if (height is not null) button.Height = height.Value;
        button.Click += click;
        return button;
    }

    internal static Button IconTextButton(string icon, string text, double minWidth = 110)
    {
        var content = new StackPanel { Orientation = Orientation.Horizontal };
        content.Children.Add(new TextBlock
        {
            Text = icon,
            FontFamily = ControlVisualConstants.IconFont,
            FontSize = 16,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0)
        });
        content.Children.Add(new TextBlock { Text = text, VerticalAlignment = VerticalAlignment.Center });
        return new Button { Content = content, MinWidth = minWidth, Margin = new Thickness(0) };
    }

    internal static void ApplyCardAppearance(Border card)
    {
        card.SetResourceReference(Border.BackgroundProperty, "CardBrush");
        card.SetResourceReference(Border.BorderBrushProperty, "BorderBrush");
    }
}
