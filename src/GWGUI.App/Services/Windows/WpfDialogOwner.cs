using System.Windows;

namespace GWGUI.App.Services.Windows;

internal static class WpfDialogOwner
{
    internal static Window? Resolve(DependencyObject? context = null) =>
        Application.Current?.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive)
        ?? (context as Window ?? (context is null ? null : Window.GetWindow(context)))
        ?? Application.Current?.MainWindow;
}
