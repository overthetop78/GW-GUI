using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Constants.Emulation;
using GWGUI.App.Contracts.Emulation.Settings;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Views.Controls.Emulation.Input;
using GWGUI.App.Views.Controls.Options;
using System.Windows;
using System.Windows.Controls;


namespace GWGUI.App.Functions.Views.Emulation.Settings;

internal static partial class EmulationSettingsLayout
{
    internal static Border InputBindings(InputBindingEditor editor, string title, string? hint = null)
    {
        var layout = new Grid();
        layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        layout.RowDefinitions.Add(new RowDefinition());
        var heading = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(10, 6, 10, 2) };
        heading.Children.Add(new TextBlock { Text = title, FontWeight = FontWeights.SemiBold, FontSize = 16,
            VerticalAlignment = VerticalAlignment.Center });
        if (!string.IsNullOrWhiteSpace(hint))
            heading.Children.Add(new TextBlock { Text = EmulationInputSettingsConstants.InformationIcon,
                FontFamily = ControlVisualConstants.IconFont,
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
        IReadOnlyList<EmulationSettingsControlField> mouseFields,
        IReadOnlyList<EmulationSettingsControlField>? analogFields,
        InputBindingEditor editor,
        Border? unavailable = null,
        ControllerVisualizer? visualizer = null)
    {
        var settings = new Grid { Margin = new Thickness(12) };
        settings.ColumnDefinitions.Add(new ColumnDefinition());
        settings.ColumnDefinitions.Add(new ColumnDefinition());
        settings.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        settings.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var mouse = IconCard(SettingsFields(1, mouseFields.ToArray()),
            LocExtension.Get("Emulation.Tab.Mouse"), EmulationInputSettingsConstants.MouseIcon);
        mouse.Margin = new Thickness(0, 0, 5, 0);
        if (analogFields is not null && analogFields.Count > 0)
        {
            var analog = IconCard(SettingsFields(1, analogFields.ToArray()),
                LocExtension.Get("Emulation.Mouse.Analog"), EmulationInputSettingsConstants.ControllerIcon);
            analog.Margin = new Thickness(5, 0, 0, 0);
            Grid.SetColumn(analog, 1);
            settings.Children.Add(analog);
        }
        else
        {
            Grid.SetColumnSpan(mouse, 2);
        }
        settings.Children.Add(mouse);

        var bindings = InputBindings(editor, LocExtension.Get("Emulation.Mouse.Actions"),
            LocExtension.Get("Emulation.Input.Capture.Hint"));
        var mapping = new Grid { Margin = new Thickness(0, 10, 0, 0) };
        mapping.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
        mapping.Children.Add(bindings);
        if (visualizer is not null)
        {
            mapping.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            visualizer.MinWidth = 0;
            visualizer.HorizontalAlignment = HorizontalAlignment.Stretch;
            visualizer.VerticalAlignment = VerticalAlignment.Stretch;
            visualizer.Margin = new Thickness(8, 4, 10, 8);
            visualizer.ClipToBounds = true;
            Grid.SetColumn(visualizer, 1);
            mapping.Children.Add(visualizer);
        }
        Grid.SetRow(mapping, 1);
        Grid.SetColumnSpan(mapping, 2);
        settings.Children.Add(mapping);

        if (unavailable is null)
            return ScrollPage(settings);

        unavailable.Margin = new Thickness(12, 12, 12, 0);
        var root = new StackPanel();
        root.Children.Add(unavailable);
        root.Children.Add(settings);
        return ScrollPage(root);
    }
}
