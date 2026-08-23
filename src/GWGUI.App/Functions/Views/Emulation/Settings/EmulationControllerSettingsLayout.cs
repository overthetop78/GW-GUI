using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Constants.Emulation;
using GWGUI.App.Contracts.Emulation.Controllers;
using GWGUI.App.Contracts.Emulation.Settings;
using GWGUI.App.Contracts.Services.Input;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Services.Input.GameInput;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;

namespace GWGUI.App.Functions.Views.Emulation.Settings;

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
        root.RowDefinitions.Add(new RowDefinition());
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var mappingTabs = new TabControl
        {
            MinHeight = EmulationControllerSettingsConstants.MappingMinimumHeight
        };
        foreach (var port in ports)
        {
            DetachForReuse(port.Type);
            DetachForReuse(port.Device);
            DetachForReuse(port.Bindings);
            var portLabel = LocExtension.Get("Emulation.Controller.Port", port.Number);
            var content = new Grid();
            content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            content.RowDefinitions.Add(new RowDefinition());
            var selectors = SettingsFields(2,
                (LocExtension.Get("Emulation.Controller.Type"), port.Type),
                (LocExtension.Get("Emulation.Controller.Device", port.Number), port.Device));
            selectors.Margin = new Thickness(10, 10, 10, 4);
            content.Children.Add(selectors);
            Grid.SetRow(port.Bindings, 1);
            content.Children.Add(port.Bindings);
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
        var devices = GameInputControllerReader.GetConnectedControllerDetailsCached()
            .Select(device => new GameControllerDevice(device.Id, device.ProductName)).ToArray();
        detectedControllers.Text = devices.Length == 0
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
