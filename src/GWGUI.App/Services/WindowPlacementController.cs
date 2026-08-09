using System.Windows;
using GWGUI.Domain.Settings;

namespace GWGUI.App.Services;

public sealed class WindowPlacementController
{
    public void Capture(Window window, AppSettings settings, bool consoleExpanded, double consoleHeight)
    {
        settings.Window.Width = window.RestoreBounds.Width;
        settings.Window.Height = window.RestoreBounds.Height;
        settings.Window.Left = window.RestoreBounds.Left;
        settings.Window.Top = window.RestoreBounds.Top;
        settings.Window.Maximized = window.WindowState == WindowState.Maximized;
        settings.ConsoleExpanded = consoleExpanded;
        if (consoleExpanded) settings.ConsoleHeight = consoleHeight;
    }

    public void Restore(Window window, WindowPlacementSettings settings)
    {
        var placement = WindowPlacementPolicy.Normalize(
            settings,
            window.MinWidth,
            window.MinHeight,
            SystemParameters.VirtualScreenLeft,
            SystemParameters.VirtualScreenTop,
            SystemParameters.VirtualScreenWidth,
            SystemParameters.VirtualScreenHeight);

        window.Width = placement.Width;
        window.Height = placement.Height;
        if (placement.Left is double left && placement.Top is double top)
        {
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Left = left;
            window.Top = top;
        }
        if (settings.Maximized) window.WindowState = WindowState.Maximized;
    }

    public void ConstrainToCurrentWorkArea(Window window)
    {
        if (window.WindowState == WindowState.Maximized) return;
        var area = MonitorWorkArea.Get(window);
        var placement = WindowPlacementPolicy.ConstrainToWorkArea(
            new(window.Width, window.Height, window.Left, window.Top),
            area.Left,
            area.Top,
            area.Width,
            area.Height);

        window.Width = placement.Width;
        window.Height = placement.Height;
        window.Left = placement.Left!.Value;
        window.Top = placement.Top!.Value;
    }
}
