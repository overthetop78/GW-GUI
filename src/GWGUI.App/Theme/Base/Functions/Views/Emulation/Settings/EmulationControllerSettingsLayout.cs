using GWGUI.App.Constants.Controls.Visual;
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
    private static FrameworkElement ControllerSelector(string resource, FrameworkElement control)
    {
        if (resource == "Emulation.Controller.Visual")
        {
            var column = new StackPanel { Margin = new Thickness(0, 0, 0, 8) };
            column.Children.Add(new TextBlock
            {
                Text = LocExtension.Get(resource), Margin = new Thickness(0, 0, 0, 6),
                TextWrapping = TextWrapping.Wrap
            });
            control.MinWidth = 0;
            control.HorizontalAlignment = HorizontalAlignment.Stretch;
            column.Children.Add(control);
            return column;
        }
        var row = new DockPanel { Margin = new Thickness(0, 0, 18, 8), LastChildFill = true };
        row.Children.Add(new TextBlock
        {
            Text = LocExtension.Get(resource), VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 12, 0), TextWrapping = TextWrapping.Wrap
        });
        control.MinWidth = 240;
        control.HorizontalAlignment = HorizontalAlignment.Stretch;
        row.Children.Add(control);
        return row;
    }

    internal static ScrollViewer ControllerSettingsPage(
        IReadOnlyList<EmulationControllerPortSettings> ports,
        EmulationSettingsControlField? behavior = null,
        string? behaviorTitle = null,
        string? behaviorGlyph = null,
        Func<EmulationControllerPortSettings, FrameworkElement>? deviceOptionsFactory = null,
        IReadOnlyList<EmulationSettingsControlField>? globalOptions = null)
    {
        var root = new Grid { Margin = new Thickness(12) };
        root.RowDefinitions.Add(new RowDefinition());
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var mappingTabs = new TabControl
        {
            Height = deviceOptionsFactory is null ? EmulationControllerSettingsConstants.MappingHeight : double.NaN
        };
        foreach (var port in ports)
        {
            DetachForReuse(port.Type);
            DetachForReuse(port.Visual);
            DetachForReuse(port.Visualizer);
            DetachForReuse(port.Bindings);
            if (port.Availability is not null) DetachForReuse(port.Availability);
            if (port.Outputs is not null) DetachForReuse(port.Outputs);
            var portLabel = port.DisplayName ?? LocExtension.Get("Emulation.Controller.Port", port.DisplayNumber ?? port.Number);
            if (deviceOptionsFactory is null)
            {
            var legacyContent = new Grid();
            legacyContent.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            legacyContent.RowDefinitions.Add(new RowDefinition());
            legacyContent.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
            legacyContent.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            var typeSelector = new StackPanel();
            typeSelector.Children.Add(ControllerSelector(port.IsUsb ? "Emulation.Controller.PeripheralType" : "Emulation.Controller.Type", port.Type));
            if (port.Availability is not null) typeSelector.Children.Add(port.Availability);
            typeSelector.Margin = new Thickness(10, 10, 6, 4);
            legacyContent.Children.Add(typeSelector);
            var portContent = new Grid { ClipToBounds = true };
            portContent.RowDefinitions.Add(new RowDefinition());
            portContent.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            port.Bindings.HorizontalAlignment = HorizontalAlignment.Stretch;
            portContent.Children.Add(port.Bindings);
            if (port.Outputs is not null)
            {
                Grid.SetRow(port.Outputs, 1);
                portContent.Children.Add(port.Outputs);
            }
            port.Visualizer.MinWidth = 0;
            port.Visualizer.HorizontalAlignment = HorizontalAlignment.Stretch;
            port.Visualizer.VerticalAlignment = VerticalAlignment.Stretch;
            port.Visualizer.Margin = new Thickness(8, 4, 10, 8);
            port.Visualizer.ClipToBounds = true;
            Grid.SetRow(portContent, 1);
            legacyContent.Children.Add(portContent);
            var visualColumn = new Grid { Margin = new Thickness(6, 10, 10, 4) };
            visualColumn.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            visualColumn.RowDefinitions.Add(new RowDefinition());
            visualColumn.Children.Add(ControllerSelector("Emulation.Controller.Visual", port.Visual));
            Grid.SetRow(port.Visualizer, 1);
            visualColumn.Children.Add(port.Visualizer);
            Grid.SetColumn(visualColumn, 1);
            Grid.SetRowSpan(visualColumn, 2);
            legacyContent.Children.Add(visualColumn);
            mappingTabs.Items.Add(new TabItem { Header = portLabel, Content = legacyContent });

                continue;
            }
            var content = new Grid();
            content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
            content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            var left = new StackPanel { Margin = new Thickness(10, 10, 6, 4) };
            left.Children.Add(ControllerSelector(port.IsUsb ? "Emulation.Controller.PeripheralType" : "Emulation.Controller.Type", port.Type));
            if (port.Availability is not null) left.Children.Add(port.Availability);
            if (deviceOptionsFactory is not null) left.Children.Add(deviceOptionsFactory(port));
            port.Bindings.Height = EmulationControllerSettingsConstants.MappingHeight;
            port.Bindings.HorizontalAlignment = HorizontalAlignment.Stretch;
            left.Children.Add(port.Bindings);
            if (port.Outputs is not null) left.Children.Add(port.Outputs);
            content.Children.Add(left);
            var right = new Grid { Margin = new Thickness(6, 10, 10, 4) };
            right.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            right.RowDefinitions.Add(new RowDefinition());
            right.Children.Add(ControllerSelector("Emulation.Controller.Visual", port.Visual));
            port.Visualizer.MinWidth = 0;
            port.Visualizer.HorizontalAlignment = HorizontalAlignment.Stretch;
            port.Visualizer.VerticalAlignment = VerticalAlignment.Top;
            port.Visualizer.Height = EmulationControllerSettingsConstants.MappingHeight;
            port.Visualizer.Margin = new Thickness(8, 4, 10, 8);
            port.Visualizer.ClipToBounds = true;
            Grid.SetRow(port.Visualizer, 1);
            right.Children.Add(port.Visualizer);
            Grid.SetColumn(right, 1); content.Children.Add(right);
            mappingTabs.Items.Add(new TabItem { Header = portLabel, Content = content });
        }
        var hasGlobalOptions = globalOptions is { Count: > 0 };
        if (hasGlobalOptions)
        {
            root.RowDefinitions.Insert(0, new RowDefinition { Height = GridLength.Auto });
            var globalOptionsCard = IconCard(BoundedForm(2, globalOptions!.ToArray()), behaviorTitle ?? LocExtension.Get(EmulationControllerSettingsConstants.BehaviorResourceKey), IconGlyphs.Controller);
            globalOptionsCard.Margin = new Thickness(0, 0, 0, 10);
            root.Children.Add(globalOptionsCard);
        }
        var mappings = ActionCard(mappingTabs, LocExtension.Get("Emulation.Controller.Mappings"));
        Grid.SetRow(mappings, hasGlobalOptions ? 1 : 0);
        root.Children.Add(mappings);
        if (behavior is not null)
        {
            DetachForReuse(behavior.Control);
            var behaviorGrid = new Grid { Margin = new Thickness(16, 12, 16, 14) };
            behaviorGrid.ColumnDefinitions.Add(new ColumnDefinition());
            behaviorGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(EmulationControllerSettingsConstants.BehaviorWidth) });
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
            Grid.SetRow(behaviorCard, hasGlobalOptions ? 2 : 1);
            root.Children.Add(behaviorCard);
        }
        return ScrollPage(root);
    }

    internal static ScrollViewer ControllerSettingsPage(IReadOnlyList<EmulationControllerPortSettings> ports, IReadOnlyList<EmulationSettingsControlField> behaviors, string? behaviorTitle = null, string? behaviorGlyph = null)
    {
        if (behaviors.Any(field => field.Definition?.AppliesToControllerIds is not null || field.Definition?.IsGlobalControllerOption == true))
            return ControllerSettingsPage(ports, behaviorTitle: behaviorTitle, deviceOptionsFactory: port => DeviceOptions(port, behaviors.Where(field => field.Definition?.IsGlobalControllerOption != true).ToArray()),
                globalOptions: behaviors.Where(field => field.Definition?.IsGlobalControllerOption == true).ToArray());
        var panel = new StackPanel();
        foreach (var behavior in behaviors)
        {
            DetachForReuse(behavior.Control);
            var row = new Grid { Margin = new Thickness(0, panel.Children.Count == 0 ? 0 : 10, 0, 0) };
            row.ColumnDefinitions.Add(new ColumnDefinition());
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(EmulationControllerSettingsConstants.BehaviorWidth) });
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
        return ControllerSettingsPage(ports, new EmulationSettingsControlField(string.Empty, panel), behaviorTitle, behaviorGlyph);
    }

    private static FrameworkElement DeviceOptions(EmulationControllerPortSettings port, IReadOnlyList<EmulationSettingsControlField> fields)
    {
        var panel = new StackPanel { Margin = new Thickness(10) };
        void Refresh()
        {
            if (!panel.IsLoaded) return;
            panel.Children.Clear();
            var selectedId = (port.Type.SelectedItem as EmulationControllerChoiceView)?.Choice.Id;
            var visible = fields.Where(field =>
                (field.Definition?.ControllerPortNumber is not { } number || number == port.Number)
                && (field.Definition?.AppliesToControllerIds is not { } types
                    || selectedId is not null && types.Contains(selectedId))).ToArray();
            foreach (var field in visible) DetachForReuse(field.Control);
            if (visible.Length != 0) panel.Children.Add(BoundedForm(1, visible));
        }
        panel.Loaded += (_, _) => Refresh();
        port.Type.SelectionChanged += (_, _) => Refresh();
        return panel;
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
