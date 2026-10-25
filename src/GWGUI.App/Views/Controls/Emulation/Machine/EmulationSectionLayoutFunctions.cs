using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Contracts.Emulation.Configurations;
using GWGUI.App.Functions.Views.Common;
using GWGUI.App.Functions.Views.Emulation.Machine;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Views.Controls.Common;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using GWGUI.Emulation;


namespace GWGUI.App.Views.Controls.Emulation.Machine;

public sealed partial class EmulationSection
{
    private const double ExpandedSelectorFrameHeight = 124;

    private UIElement BuildContent()
    {
        var root = new Grid { Margin = new Thickness(16) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition());
        var selector = new DockPanel();
        _open.Margin = new Thickness(12, 4, 0, 4);
        _open.VerticalAlignment = VerticalAlignment.Stretch;
        _open.Visibility = Visibility.Collapsed;
        DockPanel.SetDock(_open, Dock.Right);
        ConfigureSelectorToggle();
        DockPanel.SetDock(_selectorToggle, Dock.Right);
        selector.Children.Add(_selectorToggle);
        selector.Children.Add(_open);
        ConfigureSelectorCollapsedText();
        DockPanel.SetDock(_selectorCollapsedText, Dock.Left);
        selector.Children.Add(_selectorCollapsedText);
        selector.Children.Add(_selectorRows);
        _selectorFrame.Child = selector;
        _selectorFrame.BorderThickness = new Thickness(1);
        _selectorFrame.CornerRadius = new CornerRadius(8);
        _selectorFrame.Padding = new Thickness(8, 4, 8, 4);
        _selectorFrame.Margin = new Thickness(0, 0, 0, 12);
        _selectorFrame.Height = ExpandedSelectorFrameHeight;
        _selectorFrame.SetResourceReference(BackgroundProperty, ControlVisualConstants.CardBrushResource);
        _selectorFrame.SetResourceReference(BorderBrushProperty, ControlVisualConstants.BorderBrushResource);
        root.Children.Add(_selectorFrame);
        var welcome = new TabItem
        {
            Header = _welcomeHeader,
            Content = _welcomeText,
            Padding = new Thickness(18, 9, 18, 9)
        };
        _welcomeHeader.Icon = ControlVisualConstants.HomeGlyph;
        _welcomeHeader.Text = LocExtension.Get(ControlVisualConstants.WelcomeTabResource);
        _welcomeText.Text = LocExtension.Get(ControlVisualConstants.WelcomeResource);
        _welcomeText.TextWrapping = TextWrapping.Wrap;
        _welcomeText.HorizontalAlignment = HorizontalAlignment.Center;
        _welcomeText.VerticalAlignment = VerticalAlignment.Center;
        _welcomeText.MaxWidth = 680;
        _welcomeText.TextAlignment = TextAlignment.Center;
        _welcomeText.FontSize = 18;
        _welcomeText.Margin = new Thickness(32);
        welcome.SetResourceReference(StyleProperty, ControlVisualConstants.MainTabItemStyleResource);
        _machines.Items.Add(welcome);
        Grid.SetRow(_machines, 1);
        root.Children.Add(_machines);
        return root;
    }

    private void ConfigureSelectorToggle()
    {
        _selectorToggle.Width = 28;
        _selectorToggle.Height = 28;
        _selectorToggle.MinWidth = 0;
        _selectorToggle.MinHeight = 0;
        _selectorToggle.Padding = new Thickness(0);
        _selectorToggle.Margin = new Thickness(4, 4, 0, 4);
        _selectorToggle.VerticalAlignment = VerticalAlignment.Top;
        _selectorToggle.SetResourceReference(StyleProperty, ControlVisualConstants.StatusIconButtonStyleResource);
        UpdateSelectorToggle();
    }

    private void ConfigureSelectorCollapsedText()
    {
        _selectorCollapsedText.Text = LocExtension.Get(ControlVisualConstants.ConfigurationSelectorResource);
        _selectorCollapsedText.FontWeight = FontWeights.SemiBold;
        _selectorCollapsedText.VerticalAlignment = VerticalAlignment.Center;
        _selectorCollapsedText.Margin = new Thickness(0, 4, 8, 4);
        _selectorCollapsedText.Visibility = Visibility.Collapsed;
    }

    private void ToggleSelector(object sender, RoutedEventArgs args)
    {
        args.Handled = true;
        SetSelectorExpanded(!_selectorExpanded);
    }

    private void SetSelectorExpanded(bool expanded)
    {
        _selectorExpanded = expanded;
        _selectorRows.Visibility = expanded ? Visibility.Visible : Visibility.Collapsed;
        _selectorCollapsedText.Visibility = expanded ? Visibility.Collapsed : Visibility.Visible;
        _selectorFrame.Height = expanded ? ExpandedSelectorFrameHeight : double.NaN;
        UpdateSelectorToggle();
        RefreshOpenButtonVisibility();
    }

    private void UpdateSelectorToggle()
    {
        _selectorToggle.Content = new TextBlock
        {
            Text = _selectorExpanded ? "▲" : "▼",
            FontSize = 12,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        var name = LocExtension.Get(ControlVisualConstants.ConfigurationResource);
        _selectorToggle.ToolTip = name;
        AutomationProperties.SetName(_selectorToggle, name);
    }

    private static FrameworkElement CreateMachineTabHeader(string title, string description, Func<Task> close)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            ToolTip = description
        };
        panel.Children.Add(new TextBlock
        {
            Text = ControlVisualConstants.GameControllerGlyph,
            FontFamily = ControlVisualConstants.IconFont,
            FontSize = 16,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 7, 0)
        });
        panel.Children.Add(new TextBlock
        {
            Text = title,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 7, 0)
        });
        var button = new Button
        {
            Content = new TextBlock
            {
                Text = ControlVisualConstants.CloseGlyph,
                FontFamily = ControlVisualConstants.IconFont,
                FontSize = 9
            },
            ToolTip = LocExtension.Get(ControlVisualConstants.CloseResource),
            Width = 18,
            Height = 18,
            MinWidth = 0,
            MinHeight = 0,
            Padding = new Thickness(0),
            Margin = new Thickness(0)
        };
        button.SetResourceReference(StyleProperty, ControlVisualConstants.StatusIconButtonStyleResource);
        button.Click += async (_, eventArgs) =>
        {
            eventArgs.Handled = true;
            await ButtonAsyncAction.RunAsync(button, close);
        };
        panel.Children.Add(button);
        return panel;
    }

    private static string MachineTitle(EmulationConfigurationListItem selected)
    {
        var machine = selected.Module.Machines.First(item => item.Id == selected.Configuration.MachineId);
        return LocExtension.GetForModule(selected.Module, machine.DisplayResourceKey);
    }

    private static string RuntimeDisplayName(EmulationMachineRuntime runtime) => LocExtension.GetForModule(runtime.Configuration.ModuleId, runtime.DisplayResourceKey);
}
