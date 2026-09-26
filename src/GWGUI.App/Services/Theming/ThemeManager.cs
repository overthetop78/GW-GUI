using GWGUI.Infrastructure.Settings;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using GWGUI.App.Constants.Theming;

namespace GWGUI.App.Services.Theming;

public static class ThemeManager
{
    public static bool IsDark { get; private set; }

    public static void Apply(AppTheme requested)
    {
        Apply(requested, requested == AppTheme.System && SystemUsesDarkTheme(), SystemParameters.WindowGlassColor);
    }

    internal static void Apply(AppTheme requested, bool systemUsesDarkTheme, Color systemAccent)
    {
        var dark = requested == AppTheme.Dark || requested == AppTheme.System && systemUsesDarkTheme;
        IsDark = dark;
        var windowColor = ParseColor(dark ? ThemeConstants.DarkWindow : ThemeConstants.LightWindow);
        var accent = systemAccent.A == 0
            ? ParseColor(ThemeConstants.DefaultAccent)
            : Color.FromRgb(systemAccent.R, systemAccent.G, systemAccent.B);
        Set("AccentBrush", EnsureTextContrast(accent, windowColor, dark));
        Set("WindowBrush", windowColor);
        Set("CardBrush", dark ? "#23262E" : "#FFFFFF");
        Set("ControlBrush", dark ? "#2B2F38" : "#FFFFFF");
        Set("TextBrush", dark ? "#F2F3F5" : "#20242C");
        Set("BorderBrush", dark ? "#454B57" : "#E1E4EA");
        Set("HoverBrush", dark ? "#343A46" : "#EEF2FF");
        Set("SelectedBrush", dark ? "#3D4963" : "#DDE6FF");
        Set("ExplorerSelectionBrush", dark ? "#34373D" : "#E7E9EC");
        Set("MutedTextBrush", dark ? ThemeConstants.DarkMutedText : ThemeConstants.LightMutedText);
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
        Set(key, ParseColor(color));
    }

    private static void Set(string key, Color color) => Application.Current.Resources[key] = new SolidColorBrush(color);

    internal static double Contrast(Color first, Color second)
    {
        var firstLuminance = Luminance(first);
        var secondLuminance = Luminance(second);
        return (Math.Max(firstLuminance, secondLuminance) + ThemeConstants.ContrastOffset)
            / (Math.Min(firstLuminance, secondLuminance) + ThemeConstants.ContrastOffset);
    }

    private static Color EnsureTextContrast(Color color, Color background, bool dark)
    {
        var target = dark ? Colors.White : Colors.Black;
        for (var amount = 0d; amount <= 1d; amount += ThemeConstants.ContrastAdjustmentStep)
        {
            var candidate = Blend(color, target, amount);
            if (Contrast(candidate, background) >= ThemeConstants.MinimumTextContrast)
                return candidate;
        }
        return target;
    }

    private static Color Blend(Color source, Color target, double amount) => Color.FromRgb(
        (byte)Math.Round(source.R + (target.R - source.R) * amount),
        (byte)Math.Round(source.G + (target.G - source.G) * amount),
        (byte)Math.Round(source.B + (target.B - source.B) * amount));

    private static double Luminance(Color color) =>
        ThemeConstants.RedLuminance * Linear(color.R)
        + ThemeConstants.GreenLuminance * Linear(color.G)
        + ThemeConstants.BlueLuminance * Linear(color.B);

    private static double Linear(byte value)
    {
        var component = value / 255d;
        return component <= ThemeConstants.SrgbThreshold
            ? component / 12.92
            : Math.Pow((component + ThemeConstants.SrgbOffset) / ThemeConstants.SrgbDivisor,
                ThemeConstants.SrgbExponent);
    }

    private static Color ParseColor(string value) => (Color)ColorConverter.ConvertFromString(value);

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
