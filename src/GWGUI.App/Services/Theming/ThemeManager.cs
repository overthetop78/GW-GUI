using GWGUI.Infrastructure.Settings;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using GWGUI.App.Constants.Theming;
using GWGUI.App.Dictionaries.Theming;

namespace GWGUI.App.Services.Theming;

public static class ThemeManager
{
    static ThemeManager()
    {
        EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.LoadedEvent,
            new RoutedEventHandler(WindowLoaded));
    }

    private static void WindowLoaded(object sender, RoutedEventArgs args)
    {
        if (sender is Window window && ReferenceEquals(args.OriginalSource, window)
            && Application.Current?.TryFindResource(ThemeConstants.BaseWindowStyleResource) is Style)
            ApplyWindowTheme(window);
    }

    public static bool IsDark { get; private set; }
    public static AppTheme Current { get; private set; } = AppTheme.System;
    public static AppThemeDesign CurrentDesign { get; private set; } = AppThemeDesign.Base;
    public static void Apply(AppTheme requested) => Apply(CurrentDesign, requested);

    public static void Apply(AppThemeDesign design, AppTheme variant) => Apply(design, variant,
        variant == AppTheme.System && SystemUsesDarkTheme(), SystemParameters.WindowGlassColor);

    internal static void Apply(AppTheme requested, bool systemUsesDarkTheme, Color systemAccent) =>
        Apply(AppThemeDesign.Base, requested, systemUsesDarkTheme, systemAccent);

    internal static void Apply(AppThemeDesign design, AppTheme variant, bool systemUsesDarkTheme, Color systemAccent)
    {
        var definition = ThemeCatalog.Get(design);
        CurrentDesign = definition.Id;
        Current = definition.Normalize(variant);
        var dark = Current == AppTheme.Dark || Current == AppTheme.System && systemUsesDarkTheme;
        IsDark = dark;
        var resources = Application.Current.Resources;
        CopyResources(ThemeConstants.TokensUri, resources);
        CopyResources(definition.PaletteUri(dark), resources);
        var windowColor = ((SolidColorBrush)resources[ThemeConstants.WindowResource]).Color;
        var accent = systemAccent.A == 0 ? ParseColor(ThemeConstants.DefaultAccent) : Color.FromRgb(systemAccent.R, systemAccent.G, systemAccent.B);
        Set(ThemeConstants.AccentResource, EnsureTextContrast(accent, windowColor, dark));
        foreach (Window window in Application.Current.Windows)
        {
            ApplyWindowTheme(window);
        }
    }

    private static void CopyResources(string uri, ResourceDictionary destination)
    {
        var source = new ResourceDictionary { Source = new Uri(uri, UriKind.Relative) };
        foreach (var key in source.Keys) destination[key] = source[key];
    }

    public static void ApplyWindowTheme(Window window)
    {
        var ownsChrome = window.ReadLocalValue(Window.WindowStyleProperty) is WindowStyle.None;
        if (!ownsChrome && window.ReadLocalValue(FrameworkElement.StyleProperty) == DependencyProperty.UnsetValue)
            window.SetResourceReference(FrameworkElement.StyleProperty, ThemeConstants.BaseWindowStyleResource);
        if (window.ReadLocalValue(Control.BackgroundProperty) == DependencyProperty.UnsetValue)
            window.SetResourceReference(Control.BackgroundProperty, ThemeConstants.WindowResource);
        if (window.ReadLocalValue(Control.ForegroundProperty) == DependencyProperty.UnsetValue)
            window.SetResourceReference(Control.ForegroundProperty, ThemeConstants.TextResource);
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
        return (Math.Max(firstLuminance, secondLuminance) + ThemeConstants.ContrastOffset) / (Math.Min(firstLuminance, secondLuminance) + ThemeConstants.ContrastOffset);
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

    private static Color Blend(Color source, Color target, double amount) => Color.FromRgb((byte)Math.Round(source.R + (target.R - source.R) * amount), (byte)Math.Round(source.G + (target.G - source.G) * amount), (byte)Math.Round(source.B + (target.B - source.B) * amount));

    private static double Luminance(Color color) => ThemeConstants.RedLuminance * Linear(color.R) + ThemeConstants.GreenLuminance * Linear(color.G) + ThemeConstants.BlueLuminance * Linear(color.B);

    private static double Linear(byte value)
    {
        var component = value / 255d;
        return component <= ThemeConstants.SrgbThreshold ? component / 12.92 : Math.Pow((component + ThemeConstants.SrgbOffset) / ThemeConstants.SrgbDivisor, ThemeConstants.SrgbExponent);
    }

    private static Color ParseColor(string value) => (Color)ColorConverter.ConvertFromString(value);

    internal static bool SystemUsesDarkTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            return key?.GetValue("AppsUseLightTheme") is int value && value == 0;
        }
        catch { return false; }
    }
}
