using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace GWGUI.App.Controls;

internal static class AtariCoreManagementFunctions
{
    internal static UIElement CreateButtonContent(string glyph, string text)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal };
        panel.Children.Add(new TextBlock
        {
            Text = glyph,
            FontFamily = ControlVisualConstants.IconFont,
            FontSize = 15,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0)
        });
        panel.Children.Add(new TextBlock { Text = text, VerticalAlignment = VerticalAlignment.Center });
        return panel;
    }

    internal static IProgress<T> CreateDispatcherProgress<T>(Dispatcher dispatcher, Action<T> report) =>
        new DispatcherProgress<T>(dispatcher, report);

    private sealed class DispatcherProgress<T>(Dispatcher dispatcher, Action<T> report) : IProgress<T>
    {
        public void Report(T value)
        {
            if (dispatcher.CheckAccess()) report(value);
            else dispatcher.Invoke(() => report(value));
        }
    }
}
