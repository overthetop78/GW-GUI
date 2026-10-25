using GWGUI.App.Constants.Controls.Visual;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;

namespace GWGUI.App.Functions.Views.Emulation.Machine;

internal static class EmulationSelectorButtonFunctions
{
    internal static Button Create(string displayName, ImageSource image, bool selected)
    {
        var button = new Button
        {
            Content = new Image
            {
                Source = image,
                Width = 42,
                Height = 26,
                Stretch = Stretch.Uniform
            },
            Padding = new Thickness(4),
            Margin = new Thickness(0, 2, 6, 2),
            Width = 60,
            Height = 40,
            MinWidth = 60,
            MaxWidth = 60,
            MinHeight = 40,
            MaxHeight = 40,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            ToolTip = displayName,
            BorderThickness = new Thickness(1)
        };
        button.Background = selected ? new SolidColorBrush(Color.FromArgb(64, 77, 118, 232)) : Brushes.Transparent;
        if (selected)
            button.BorderBrush = new SolidColorBrush(Color.FromRgb(77, 118, 232));
        else
            button.BorderBrush = new SolidColorBrush(Color.FromRgb(155, 165, 180));
        button.SetResourceReference(Control.ForegroundProperty, ControlVisualConstants.TextBrushResource);
        AutomationProperties.SetName(button, displayName);
        return button;
    }

}
