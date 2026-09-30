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
    private const double SelectorRowHeight = 56;
    private const double ExpandedSelectorFrameHeight = 124;

    private UIElement BuildContent()
    {
        var root = new Grid { Margin = new Thickness(16) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition());
        var selector = new DockPanel();
        ConfigureSelectorLabel(_brandLabel, ControlVisualConstants.ConfigurationBrandResource);
        ConfigureSelectorLabel(_machineLabel, ControlVisualConstants.ConfigurationMachineResource);
        ConfigureSelectorPanel(_brandPanel);
        ConfigureSelectorPanel(_machinePanel);
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
        _selectorRows.Orientation = Orientation.Vertical;
        _selectorRows.Height = SelectorRowHeight * 2;
        var brandRow = new DockPanel { Height = SelectorRowHeight };
        DockPanel.SetDock(_brandLabel, Dock.Left);
        brandRow.Children.Add(_brandLabel);
        brandRow.Children.Add(_brandPanel);
        _selectorRows.Children.Add(brandRow);
        var machineRow = new DockPanel { Height = SelectorRowHeight };
        DockPanel.SetDock(_machineLabel, Dock.Left);
        machineRow.Children.Add(_machineLabel);
        machineRow.Children.Add(_machinePanel);
        _selectorRows.Children.Add(machineRow);
        selector.Children.Add(_selectorRows);
        _selectorFrame.Child = selector;
        _selectorFrame.BorderThickness = new Thickness(1);
        _selectorFrame.CornerRadius = new CornerRadius(8);
        _selectorFrame.Padding = new Thickness(8, 4, 8, 4);
        _selectorFrame.Margin = new Thickness(0, 0, 0, 12);
        _selectorFrame.Height = ExpandedSelectorFrameHeight;
        _selectorFrame.SetResourceReference(BackgroundProperty,
            ControlVisualConstants.CardBrushResource);
        _selectorFrame.SetResourceReference(BorderBrushProperty,
            ControlVisualConstants.BorderBrushResource);
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
        _selectorToggle.SetResourceReference(StyleProperty,
            ControlVisualConstants.StatusIconButtonStyleResource);
        UpdateSelectorToggle();
    }

    private void ConfigureSelectorCollapsedText()
    {
        _selectorCollapsedText.Text = LocExtension.Get(
            ControlVisualConstants.ConfigurationSelectorResource);
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

    private Button? CreateBrandButton(IEmulationModule module, string displayName)
    {
        var image = EmulationAssetFunctions.Load(module, module.BrandImageResourceName);
        if (image is null) return null;
        var button = CreateSelectorButton(displayName, image,
            ReferenceEquals(_selectedModule, module));
        button.Click += (_, _) => SelectModule(module);
        return button;
    }

    private Button? CreateMachineButton(IEmulationModule module,
        EmulationMachineDefinition definition, string displayName)
    {
        var image = EmulationAssetFunctions.Load(module, definition.ImageResourceName);
        if (image is null) return null;
        var button = CreateSelectorButton(displayName, image,
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

    private static Button CreateSelectorButton(string displayName, ImageSource image,
        bool selected)
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
        button.Background = selected
            ? new SolidColorBrush(Color.FromArgb(64, 77, 118, 232))
            : Brushes.Transparent;
        if (selected)
            button.BorderBrush = new SolidColorBrush(Color.FromRgb(77, 118, 232));
        else
            button.BorderBrush = new SolidColorBrush(Color.FromRgb(155, 165, 180));
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
