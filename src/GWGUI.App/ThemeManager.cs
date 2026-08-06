using System.Windows;
using System.Windows.Media;
using GWGUI.Domain.Settings;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace GWGUI.App;

public static class ThemeManager
{
    public static bool IsDark { get; private set; }

    public static void Apply(AppTheme requested)
    {
        var dark = requested == AppTheme.Dark || requested == AppTheme.System && SystemUsesDarkTheme();
        IsDark = dark;
        var systemAccent = SystemParameters.WindowGlassColor;
        Set("AccentBrush", systemAccent.A == 0 ? Color.FromRgb(77, 118, 232) : Color.FromRgb(systemAccent.R, systemAccent.G, systemAccent.B));
        Set("WindowBrush", dark ? "#17191F" : "#F6F7FA");
        Set("CardBrush", dark ? "#23262E" : "#FFFFFF");
        Set("ControlBrush", dark ? "#2B2F38" : "#FFFFFF");
        Set("TextBrush", dark ? "#F2F3F5" : "#20242C");
        Set("BorderBrush", dark ? "#454B57" : "#E1E4EA");
        Set("HoverBrush", dark ? "#343A46" : "#EEF2FF");
        Set("SelectedBrush", dark ? "#3D4963" : "#DDE6FF");
        Set("MutedTextBrush", dark ? "#AEB6C4" : "#69707D");
        Set("StatusBrush", dark ? "#20232A" : "#FFFFFF");
        foreach (Window window in Application.Current.Windows) ApplyWindowTheme(window);
    }

    public static void ApplyWindowTheme(Window window)
    {
        var handle = new WindowInteropHelper(window).Handle;
        if (handle == IntPtr.Zero) return;
        var enabled = IsDark ? 1 : 0;
        _ = DwmSetWindowAttribute(handle, 20, ref enabled, sizeof(int));
        _ = DwmSetWindowAttribute(handle, 19, ref enabled, sizeof(int));
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr window, int attribute, ref int value, int valueSize);

    private static void Set(string key, string color)
    {
        Set(key, (Color)ColorConverter.ConvertFromString(color));
    }

    private static void Set(string key, Color color) => Application.Current.Resources[key] = new SolidColorBrush(color);

    private static bool SystemUsesDarkTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            return key?.GetValue("AppsUseLightTheme") is int value && value == 0;
        }
        catch { return false; }
    }
}
