using GWGUI.App.Views.Controls.Visualization;
using System.Windows;
using System.Windows.Input;

namespace GWGUI.App.Functions.Views.Visualization;

internal static class VisualizationPanFunctions
{
    internal static void Begin(FrameworkElement surface, IInputElement coordinateSpace, float zoom,
        ref Point? panDragLast, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Middle || zoom <= 1) return;
        panDragLast = e.GetPosition(coordinateSpace);
        surface.Cursor = VisualizationCursors.Grabbing;
        surface.CaptureMouse();
        e.Handled = true;
    }

    internal static void End(FrameworkElement surface, ref Point? panDragLast, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Middle || panDragLast is null) return;
        panDragLast = null;
        surface.ReleaseMouseCapture();
        surface.ClearValue(FrameworkElement.CursorProperty);
        e.Handled = true;
    }
}
