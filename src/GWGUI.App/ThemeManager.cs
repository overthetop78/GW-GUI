using System.Windows;
using System.Windows.Media;
using GWGUI.Domain.Settings;
using Microsoft.Win32;

namespace GWGUI.App;

public static class ThemeManager
{
    public static void Apply(AppTheme requested)
    {
        var dark = requested == AppTheme.Dark || requested == AppTheme.System && SystemUsesDarkTheme();
        if (Application.Current.Resources["AccentBrush"] is SolidColorBrush accent)
        {
            var systemAccent = SystemParameters.WindowGlassColor;
            accent.Color = systemAccent.A == 0 ? Color.FromRgb(77, 118, 232) : Color.FromRgb(systemAccent.R, systemAccent.G, systemAccent.B);
        }
        Set("WindowBrush", dark ? "#17191F" : "#F6F7FA");
        Set("CardBrush", dark ? "#23262E" : "#FFFFFF");
        Set("ControlBrush", dark ? "#2B2F38" : "#FFFFFF");
        Set("TextBrush", dark ? "#F2F3F5" : "#20242C");
        Set("BorderBrush", dark ? "#454B57" : "#E1E4EA");
    }

    private static void Set(string key, string color)
    {
        if (Application.Current.Resources[key] is SolidColorBrush brush) brush.Color = (Color)ColorConverter.ConvertFromString(color);
    }

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
