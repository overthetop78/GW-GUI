using GWGUI.App.Constants.Emulation;
using GWGUI.App.Contracts.Emulation.Controllers;
using GWGUI.App.Contracts.Emulation.Settings;
using GWGUI.App.Localization.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GWGUI.App.Functions.Views.Emulation.Settings;

internal static partial class EmulationSettingsLayout
{
    internal static ScrollViewer ControllerSettingsPage(
        IReadOnlyList<EmulationControllerPortSettings> ports,
        EmulationSettingsControlField? behavior = null,
        string? behaviorTitle = null,
        string? behaviorGlyph = null)
    {
        var root = new Grid { Margin = new Thickness(12) };
        root.RowDefinitions.Add(new RowDefinition());
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var mappingTabs = new TabControl
        {
            Height = EmulationControllerSettingsConstants.MappingHeight
        };
        foreach (var port in ports)
        {
            DetachForReuse(port.Type);
            DetachForReuse(port.Visual);
            DetachForReuse(port.Visualizer);
            DetachForReuse(port.Bindings);
            var portLabel = LocExtension.Get("Emulation.Controller.Port", port.Number);
            var content = new Grid();
            content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            content.RowDefinitions.Add(new RowDefinition());
            var selectors = SettingsFields(2,
                (LocExtension.Get("Emulation.Controller.Type"), port.Type),
                (LocExtension.Get("Emulation.Controller.Visual"), port.Visual));
            selectors.Margin = new Thickness(10, 10, 10, 4);
            content.Children.Add(selectors);
            var portContent = new Grid { ClipToBounds = true };
            portContent.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
            portContent.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            port.Bindings.HorizontalAlignment = HorizontalAlignment.Stretch;
            portContent.Children.Add(port.Bindings);
            port.Visualizer.MinWidth = 0;
            port.Visualizer.HorizontalAlignment = HorizontalAlignment.Stretch;
            port.Visualizer.VerticalAlignment = VerticalAlignment.Stretch;
            port.Visualizer.Margin = new Thickness(8, 4, 10, 8);
            port.Visualizer.ClipToBounds = true;
            Grid.SetColumn(port.Visualizer, 1);
            portContent.Children.Add(port.Visualizer);
            Grid.SetRow(portContent, 1);
            content.Children.Add(portContent);
            mappingTabs.Items.Add(new TabItem { Header = portLabel, Content = content });
        }
        var mappings = ActionCard(mappingTabs, LocExtension.Get("Emulation.Controller.Mappings"));
        root.Children.Add(mappings);

        if (behavior is not null)
        {
            DetachForReuse(behavior.Control);
            var behaviorGrid = new Grid { Margin = new Thickness(16, 12, 16, 14) };
            behaviorGrid.ColumnDefinitions.Add(new ColumnDefinition());
            behaviorGrid.ColumnDefinitions.Add(new ColumnDefinition
                { Width = new GridLength(EmulationControllerSettingsConstants.BehaviorWidth) });
            behaviorGrid.Children.Add(new TextBlock
            {
                Text = behavior.Label,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            });
            behavior.Control.HorizontalAlignment = HorizontalAlignment.Stretch;
            if (string.IsNullOrEmpty(behavior.Label)) Grid.SetColumnSpan(behavior.Control, 2);
            else Grid.SetColumn(behavior.Control, 1);
            behaviorGrid.Children.Add(behavior.Control);
            var behaviorCard = IconCard(behaviorGrid, behaviorTitle ?? string.Empty, behaviorGlyph ?? string.Empty);
            behaviorCard.Margin = new Thickness(0, 10, 0, 0);
            Grid.SetRow(behaviorCard, 1);
            root.Children.Add(behaviorCard);
        }
        return ScrollPage(root);
    }

    internal static ScrollViewer ControllerSettingsPage(
        IReadOnlyList<EmulationControllerPortSettings> ports,
        IReadOnlyList<EmulationSettingsControlField> behaviors,
        string? behaviorTitle = null, string? behaviorGlyph = null)
    {
        var panel = new StackPanel();
        foreach (var behavior in behaviors)
        {
            DetachForReuse(behavior.Control);
            var row = new Grid { Margin = new Thickness(0, panel.Children.Count == 0 ? 0 : 10, 0, 0) };
            row.ColumnDefinitions.Add(new ColumnDefinition());
            row.ColumnDefinitions.Add(new ColumnDefinition
                { Width = new GridLength(EmulationControllerSettingsConstants.BehaviorWidth) });
            var label = new TextBlock
            {
                Text = behavior.Label,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };
            label.SetBinding(UIElement.VisibilityProperty, new Binding(nameof(UIElement.Visibility))
            {
                Source = behavior.Control,
                Mode = BindingMode.OneWay
            });
            row.Children.Add(label);
            behavior.Control.HorizontalAlignment = HorizontalAlignment.Stretch;
            Grid.SetColumn(behavior.Control, 1);
            row.Children.Add(behavior.Control);
            panel.Children.Add(row);
        }
        return ControllerSettingsPage(ports,
            new EmulationSettingsControlField(string.Empty, panel), behaviorTitle, behaviorGlyph);
    }

    private static void DetachForReuse(UIElement element)
    {
        var parent = LogicalTreeHelper.GetParent(element);
        switch (parent)
        {
            case Panel panel:
                panel.Children.Remove(element);
                break;
            case ContentControl content when ReferenceEquals(content.Content, element):
                content.Content = null;
                break;
            case Decorator decorator when ReferenceEquals(decorator.Child, element):
                decorator.Child = null;
                break;
        }
    }

}
