using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Contracts.Emulation.Configurations;
using GWGUI.App.Functions.Views.Common;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Views.Controls.Common;
using System.Windows;
using System.Windows.Controls;
using GWGUI.Emulation;


namespace GWGUI.App.Views.Controls.Emulation.Machine;

public sealed partial class EmulationSection
{
    private UIElement BuildContent()
    {
        var root = new Grid { Margin = new Thickness(16) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition());
        var selector = new Grid { Margin = new Thickness(0, 0, 0, 12) };
        selector.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
        selector.ColumnDefinitions.Add(new ColumnDefinition());
        selector.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        _configurationLabel.Text = LocExtension.Get(ControlVisualConstants.ConfigurationResource);
        _configurationLabel.VerticalAlignment = VerticalAlignment.Center;
        _configurationLabel.Margin = new Thickness(0, 5, 12, 5);
        _configurationLabel.FontWeight = FontWeights.SemiBold;
        selector.Children.Add(_configurationLabel);
        _configuration.Margin = new Thickness(0, 4, 8, 4);
        Grid.SetColumn(_configuration, 1);
        selector.Children.Add(_configuration);
        _open.Margin = new Thickness(0, 4, 0, 4);
        Grid.SetColumn(_open, 2);
        selector.Children.Add(_open);
        var selectorCard = new Border { Child = selector };
        selectorCard.SetResourceReference(StyleProperty, ControlVisualConstants.CardStyleResource);
        root.Children.Add(selectorCard);
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

    private static FrameworkElement CreateMachineTabHeader(
        string title, string description, Func<Task> close)
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
        button.SetResourceReference(StyleProperty,
            ControlVisualConstants.StatusIconButtonStyleResource);
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
        var machine = selected.Module.Machines.First(item =>
            item.Id == selected.Configuration.MachineId);
        return LocExtension.Get(machine.DisplayResourceKey);
    }

    private static string RuntimeDisplayName(EmulationMachineRuntime runtime) =>
        LocExtension.Get(runtime.DisplayResourceKey);
}
