using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Localization;

namespace GWGUI.App.Controls;

internal static partial class EmulationSettingsLayout
{
    private const string MouseIcon = "\uE962";
    private const string ControllerIcon = "\uE7FC";

    internal static Border InputBindings(InputBindingEditor editor, string title, string? hint = null)
    {
        var layout = new Grid();
        layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        layout.RowDefinitions.Add(new RowDefinition());
        var heading = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(10, 6, 10, 2) };
        heading.Children.Add(new TextBlock { Text = title, FontWeight = FontWeights.SemiBold, FontSize = 16,
            VerticalAlignment = VerticalAlignment.Center });
        if (!string.IsNullOrWhiteSpace(hint))
            heading.Children.Add(new TextBlock { Text = "\uE946", FontFamily = ControlVisualConstants.IconFont,
                FontSize = 15, Margin = new Thickness(9, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center,
                ToolTip = hint });
        layout.Children.Add(heading);
        Grid.SetRow(editor, 1);
        layout.Children.Add(editor);
        var card = new Border { Child = layout, Padding = new Thickness(2) };
        card.SetResourceReference(FrameworkElement.StyleProperty, "Card");
        return card;
    }

    internal static Grid KeyboardSettingsPage(InputBindingEditor editor, string? hint = null,
        Border? unavailable = null)
    {
        var page = new Grid { Margin = new Thickness(12) };
        page.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        page.RowDefinitions.Add(new RowDefinition());
        if (unavailable is not null)
        {
            unavailable.Margin = new Thickness(0, 0, 0, 10);
            page.Children.Add(unavailable);
        }
        var bindings = InputBindings(editor, LocExtension.Get("Emulation.Input.Actions"), hint);
        Grid.SetRow(bindings, 1);
        page.Children.Add(bindings);
        return page;
    }

    internal static ScrollViewer MouseSettingsPage(
        IReadOnlyList<EmulationSettingsField> mouseFields,
        IReadOnlyList<EmulationSettingsField>? analogFields,
        InputBindingEditor editor,
        Border? unavailable = null)
    {
        var settings = new Grid { Margin = new Thickness(12) };
        settings.ColumnDefinitions.Add(new ColumnDefinition());
        settings.ColumnDefinitions.Add(new ColumnDefinition());
        settings.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        settings.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var mouse = IconCard(SettingsFields(1,
                mouseFields.Select(field => (field.Label, field.Control)).ToArray()),
            LocExtension.Get("Emulation.Tab.Mouse"), MouseIcon);
        mouse.Margin = new Thickness(0, 0, 5, 0);
        if (analogFields is not null && analogFields.Count > 0)
        {
            var analog = IconCard(SettingsFields(1,
                    analogFields.Select(field => (field.Label, field.Control)).ToArray()),
                LocExtension.Get("Emulation.Mouse.Analog"), ControllerIcon);
            analog.Margin = new Thickness(5, 0, 0, 0);
            Grid.SetColumn(analog, 1);
            settings.Children.Add(analog);
        }
        settings.Children.Add(mouse);

        var bindings = InputBindings(editor, LocExtension.Get("Emulation.Mouse.Actions"),
            LocExtension.Get("Emulation.Input.Capture.Hint"));
        bindings.Margin = new Thickness(0, 10, 0, 0);
        Grid.SetRow(bindings, 1);
        Grid.SetColumnSpan(bindings, 2);
        settings.Children.Add(bindings);

        if (unavailable is null)
            return ScrollPage(settings);

        unavailable.Margin = new Thickness(12, 12, 12, 0);
        var root = new StackPanel();
        root.Children.Add(unavailable);
        root.Children.Add(settings);
        return ScrollPage(root);
    }
}
