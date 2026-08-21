using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using GWGUI.App.Contracts;
using GWGUI.App.Localization;

namespace GWGUI.App.Controls;

/// <summary>The single visual shell used by every emulated machine.</summary>
internal sealed class MachineView : UserControl
{
    private readonly Dictionary<string, Ellipse> _deviceLeds = new(StringComparer.Ordinal);

    internal MachineView()
    {
        Root = new Grid { Background = Brushes.Transparent };
        Root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        Root.RowDefinitions.Add(new RowDefinition());
        Root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        Toolbar = new DockPanel
        {
            Height = 34,
            LastChildFill = true,
            Margin = new Thickness(0, 0, 0, 2)
        };
        Root.Children.Add(Toolbar);

        VideoHost = new Grid { Background = Brushes.Black };
        Screen = new Border
        {
            Background = Brushes.Black,
            Child = VideoHost,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            SnapsToDevicePixels = true
        };
        DisplayHost = new Grid { Background = new SolidColorBrush(Color.FromRgb(43, 46, 50)) };
        DisplayHost.Children.Add(Screen);
        Grid.SetRow(DisplayHost, 1);
        Root.Children.Add(DisplayHost);

        DeviceStrip = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };
        BottomBar = new Border
        {
            Height = 24,
            BorderThickness = new Thickness(1, 1, 1, 0),
            Child = DeviceStrip,
            Padding = new Thickness(4, 1, 4, 1)
        };
        BottomBar.SetResourceReference(Border.BorderBrushProperty, "BorderBrush");
        Grid.SetRow(BottomBar, 2);
        Root.Children.Add(BottomBar);
        Content = Root;
    }

    internal Grid Root { get; }
    internal DockPanel Toolbar { get; }
    internal Grid VideoHost { get; }
    internal Border Screen { get; }
    internal Grid DisplayHost { get; }
    internal StackPanel DeviceStrip { get; }
    internal Border BottomBar { get; }
    internal IReadOnlyDictionary<string, Ellipse> DeviceLeds => _deviceLeds;

    internal static Button CreateCommandButton(string glyph, string tooltip)
    {
        var icon = new TextBlock
        {
            Text = glyph, FontFamily = ControlVisualConstants.IconFont, FontSize = 17,
            HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center
        };
        icon.SetBinding(TextBlock.ForegroundProperty, new Binding(nameof(Button.Foreground))
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Button), 1)
        });
        var button = new Button
        {
            Content = icon, ToolTip = tooltip, Width = 28, Height = 28, MinWidth = 0, MinHeight = 0,
            Padding = new Thickness(2), Margin = new Thickness(0, 0, 2, 0)
        };
        button.SetResourceReference(StyleProperty, "StatusIconButton");
        return button;
    }

    internal static Border CreateToolbarGroup(params UIElement[] children)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };
        foreach (var child in children) panel.Children.Add(child);
        var border = new Border
        {
            Child = panel, Height = 32, BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(6),
            Padding = new Thickness(2, 1, 2, 1), Margin = new Thickness(2, 1, 2, 1),
            VerticalAlignment = VerticalAlignment.Center
        };
        border.SetResourceReference(BackgroundProperty, "CardBrush");
        border.SetResourceReference(BorderBrushProperty, "BorderBrush");
        return border;
    }

    internal void SetVideoView(FrameworkElement view)
    {
        VideoHost.Children.Clear();
        VideoHost.Children.Add(view);
    }

    internal void SetDevices(IEnumerable<MachineViewDevice> devices, Action<Exception> showError)
    {
        DeviceStrip.Children.Clear();
        _deviceLeds.Clear();
        foreach (var device in devices) DeviceStrip.Children.Add(DeviceItem(device, showError));
    }

    private FrameworkElement DeviceItem(MachineViewDevice device, Action<Exception> showError)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };
        var led = new Ellipse
        {
            Width = 10,
            Height = 10,
            Fill = device.Present || !device.Removable ? Brushes.ForestGreen : Brushes.Gray,
            Margin = new Thickness(0, 0, 4, 0),
            Tag = device.Key
        };
        _deviceLeds[device.Key] = led;
        panel.Children.Add(led);

        var open = new Button
        {
            ToolTip = device.Removable ? LocExtension.Get("Common.Browse") : device.Label,
            Height = 20,
            MinHeight = 0,
            MinWidth = 0,
            Padding = new Thickness(2, 0, 2, 0),
            Margin = new Thickness(0),
            Background = Brushes.Transparent,
            BorderBrush = Brushes.Transparent,
            Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new TextBlock
                    {
                        Text = device.Glyph,
                        FontFamily = ControlVisualConstants.IconFont,
                        FontSize = 15,
                        Margin = new Thickness(0, 0, 4, 0)
                    },
                    new TextBlock
                    {
                        Text = device.Label,
                        FontWeight = FontWeights.SemiBold,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                }
            }
        };
        if (device.Removable && device.Insert is not null)
            open.Click += async (_, _) => await RunAsync(device.Insert, showError);
        panel.Children.Add(open);

        if (device.Removable && device.Eject is not null)
        {
            var eject = new Button
            {
                Content = new TextBlock
                {
                    Text = "\u23CF",
                    FontFamily = new FontFamily("Segoe UI Symbol"),
                    FontSize = 15
                },
                ToolTip = LocExtension.Get("Common.Eject"),
                Width = 22,
                Height = 20,
                MinWidth = 0,
                MinHeight = 0,
                Padding = new Thickness(0),
                Margin = new Thickness(3, 0, 0, 0),
                IsEnabled = device.Present
            };
            eject.SetResourceReference(StyleProperty, "StatusIconButton");
            eject.Click += async (_, _) => await RunAsync(device.Eject, showError);
            panel.Children.Add(eject);
        }

        return new Border
        {
            Child = panel,
            Padding = new Thickness(4, 0, 4, 0),
            Margin = new Thickness(0, 0, 3, 0),
            BorderThickness = new Thickness(0, 0, 1, 0),
            BorderBrush = new SolidColorBrush(Color.FromRgb(215, 222, 231))
        };
    }

    private static async Task RunAsync(Func<Task> action, Action<Exception> showError)
    {
        try { await action(); }
        catch (Exception error) { showError(error); }
    }
}
