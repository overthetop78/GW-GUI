using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Contracts.Emulation.Configurations;
using GWGUI.App.Functions.Views.Common;
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
    private UIElement BuildContent()
    {
        var root = new Grid { Margin = new Thickness(16) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition());
        var selector = new DockPanel { Margin = new Thickness(0, 0, 0, 12) };
        ConfigureSelectorLabel(_brandLabel, ControlVisualConstants.ConfigurationBrandResource);
        ConfigureSelectorLabel(_machineLabel, ControlVisualConstants.ConfigurationMachineResource);
        ConfigureSelectorPanel(_brandPanel);
        ConfigureSelectorPanel(_machinePanel);
        _open.Margin = new Thickness(12, 4, 0, 4);
        _open.VerticalAlignment = VerticalAlignment.Stretch;
        DockPanel.SetDock(_open, Dock.Right);
        selector.Children.Add(_open);
        var rows = new StackPanel { Orientation = Orientation.Vertical };
        var brandRow = new DockPanel();
        DockPanel.SetDock(_brandLabel, Dock.Left);
        brandRow.Children.Add(_brandLabel);
        brandRow.Children.Add(_brandPanel);
        rows.Children.Add(brandRow);
        var machineRow = new DockPanel();
        DockPanel.SetDock(_machineLabel, Dock.Left);
        machineRow.Children.Add(_machineLabel);
        machineRow.Children.Add(_machinePanel);
        rows.Children.Add(machineRow);
        selector.Children.Add(rows);
        root.Children.Add(selector);
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

    private static void ConfigureSelectorLabel(TextBlock label, string resourceKey)
    {
        label.Text = LocExtension.Get(resourceKey);
        label.VerticalAlignment = VerticalAlignment.Top;
        label.Margin = new Thickness(0, 11, 12, 5);
        label.FontWeight = FontWeights.SemiBold;
    }

    private static void ConfigureSelectorPanel(WrapPanel panel)
    {
        panel.Orientation = Orientation.Horizontal;
        panel.Margin = new Thickness(0, 2, 0, 2);
    }

    private Button CreateBrandButton(IEmulationModule module, string displayName)
    {
        var button = CreateSelectorButton(displayName,
            EmulationAssetFunctions.Load(module, module.BrandImageResourceName),
            ReferenceEquals(_selectedModule, module));
        button.Click += (_, _) => SelectModule(module);
        return button;
    }

    private Button CreateMachineButton(IEmulationModule module,
        EmulationMachineDefinition definition, string displayName)
    {
        var button = CreateSelectorButton(displayName,
            EmulationAssetFunctions.Load(module, definition.ImageResourceName),
            ReferenceEquals(_selectedModule, module) && _selectedMachineId == definition.Id);
        button.Click += (_, _) => SelectMachine(module, definition);
        button.MouseDoubleClick += (_, args) =>
        {
            args.Handled = true;
            var selected = FindConfiguration(module.Id, definition.Id);
            if (selected is null) return;
            _selectedModule = module;
            _selectedMachineId = definition.Id;
            _selectedConfigurationId = selected.Configuration.Id;
            RefreshSelector();
            OpenSelectedMachine(button, new RoutedEventArgs());
        };
        return button;
    }

    private static Button CreateSelectorButton(string displayName, ImageSource? image,
        bool selected)
    {
        var content = new StackPanel { Orientation = Orientation.Horizontal };
        if (image is not null)
            content.Children.Add(new Image
            {
                Source = image,
                Width = 46,
                Height = 36,
                Stretch = Stretch.Uniform,
                Margin = new Thickness(0, 0, 7, 0)
            });
        content.Children.Add(new TextBlock
        {
            Text = displayName,
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.NoWrap
        });
        var button = new Button
        {
            Content = content,
            Padding = new Thickness(8, 5, 8, 5),
            Margin = new Thickness(0, 2, 8, 2),
            MinHeight = 48,
            ToolTip = displayName,
            BorderThickness = new Thickness(selected ? 1 : 0)
        };
        button.Background = selected
            ? new SolidColorBrush(Color.FromArgb(64, 77, 118, 232))
            : Brushes.Transparent;
        button.BorderBrush = selected
            ? new SolidColorBrush(Color.FromRgb(77, 118, 232))
            : Brushes.Transparent;
        button.SetResourceReference(ForegroundProperty, ControlVisualConstants.TextBrushResource);
        AutomationProperties.SetName(button, displayName);
        return button;
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
        return LocExtension.GetForModule(selected.Module, machine.DisplayResourceKey);
    }

    private static string RuntimeDisplayName(EmulationMachineRuntime runtime) =>
        LocExtension.GetForModule(runtime.Configuration.ModuleId, runtime.DisplayResourceKey);
}
