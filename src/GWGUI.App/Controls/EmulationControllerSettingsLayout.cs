using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using GWGUI.App.Localization;
using GWGUI.App.Services;

namespace GWGUI.App.Controls;

internal static partial class EmulationSettingsLayout
{
    internal static ScrollViewer ControllerSettingsPage(
        IReadOnlyList<EmulationControllerPortSettings> ports,
        TextBlock detectedControllers,
        Func<Task> detectControllers,
        EmulationSettingsControlField? behavior = null,
        string? behaviorTitle = null,
        string? behaviorGlyph = null)
    {
        var root = new Grid { Margin = new Thickness(12) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var detection = new Grid { Margin = new Thickness(12, 8, 12, 10) };
        detection.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        detection.ColumnDefinitions.Add(new ColumnDefinition());
        var detectButton = new Button { Content = LocExtension.Get("Emulation.Controller.Detect") };
        AutomationProperties.SetName(detectButton, LocExtension.Get("Emulation.Controller.Detect"));
        detectButton.Click += async (_, _) => await detectControllers();
        detection.Children.Add(detectButton);
        detectedControllers.Margin = new Thickness(14, 0, 0, 0);
        detectedControllers.VerticalAlignment = VerticalAlignment.Center;
        detectedControllers.TextWrapping = TextWrapping.Wrap;
        Grid.SetColumn(detectedControllers, 1);
        detection.Children.Add(detectedControllers);
        root.Children.Add(IconCard(detection, LocExtension.Get("Emulation.Controller.Detected"),
            ControlVisualConstants.GameControllerGlyph));

        var portCards = new Grid { Margin = new Thickness(0, 10, 0, 10) };
        portCards.ColumnDefinitions.Add(new ColumnDefinition());
        portCards.ColumnDefinitions.Add(new ColumnDefinition());
        var mappingTabs = new TabControl
        {
            Margin = new Thickness(0, 0, behavior is null ? 0 : 8, 0),
            MinHeight = EmulationControllerSettingsConstants.MappingMinimumHeight
        };
        for (var index = 0; index < ports.Count; index++)
        {
            var port = ports[index];
            DetachForReuse(port.Type);
            DetachForReuse(port.Device);
            DetachForReuse(port.Bindings);
            var row = index / 2;
            while (portCards.RowDefinitions.Count <= row)
                portCards.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var portLabel = LocExtension.Get("Emulation.Controller.Port", port.Number);
            var form = SettingsFields(1,
                (LocExtension.Get("Emulation.Controller.Type"), port.Type),
                (LocExtension.Get("Emulation.Controller.Device", port.Number), port.Device));
            var card = IconCard(form, portLabel, ControlVisualConstants.GameControllerGlyph);
            card.Margin = new Thickness(index % 2 == 0 ? 0 : 5, row == 0 ? 0 : 10,
                index % 2 == 0 ? 5 : 0, 0);
            Grid.SetRow(card, row);
            Grid.SetColumn(card, index % 2);
            portCards.Children.Add(card);
            mappingTabs.Items.Add(new TabItem { Header = portLabel, Content = port.Bindings });
        }
        Grid.SetRow(portCards, 1);
        root.Children.Add(portCards);

        var lower = new Grid();
        lower.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        if (behavior is not null) lower.ColumnDefinitions.Add(new ColumnDefinition());
        lower.Children.Add(ActionCard(mappingTabs, LocExtension.Get("Emulation.Controller.Mappings")));
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
            behaviorCard.Margin = new Thickness(10, 0, 0, 0);
            behaviorCard.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetColumn(behaviorCard, 1);
            lower.Children.Add(behaviorCard);
        }
        Grid.SetRow(lower, 2);
        root.Children.Add(lower);
        return ScrollPage(root);
    }

    internal static ScrollViewer ControllerSettingsPage(
        IReadOnlyList<EmulationControllerPortSettings> ports, TextBlock detectedControllers,
        Func<Task> detectControllers, IReadOnlyList<EmulationSettingsControlField> behaviors,
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
        return ControllerSettingsPage(ports, detectedControllers, detectControllers,
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

    internal static Task DetectControllersAsync(IReadOnlyList<ComboBox> deviceSelectors,
        TextBlock detectedControllers)
    {
        var devices = XInputControllerReader.GetConnectedDevices();
        detectedControllers.Text = devices.Count == 0
            ? LocExtension.Get("Emulation.Controller.NoneDetected")
            : string.Join(ControlVisualConstants.DetailSeparator, devices.Select(device => device.Name));
        for (var index = 0; index < deviceSelectors.Count; index++)
        {
            var selector = deviceSelectors[index];
            var selectedId = (selector.SelectedItem as GameControllerDevice)?.Id ?? selector.Tag as string;
            selector.ItemsSource = devices;
            selector.SelectedItem = devices.FirstOrDefault(device => device.Id == selectedId)
                ?? devices.ElementAtOrDefault(index);
            selector.Tag = null;
        }
        return Task.CompletedTask;
    }
}
