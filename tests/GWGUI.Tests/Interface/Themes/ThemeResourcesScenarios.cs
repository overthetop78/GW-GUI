using GWGUI.App.Services.Theming;
using GWGUI.Infrastructure.Settings;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GWGUI.App.Constants.Theming;
namespace GWGUI.Tests.Interface.Themes;
internal static class ThemeResourcesScenarios
{
    private static readonly Color Accent=Color.FromRgb(12,34,56);
    public static void Apply(AppTheme theme,bool systemDark,bool expected)
    {
        var old=ThemeManager.IsDark;
        var oldAccent=((SolidColorBrush)System.Windows.Application.Current.Resources["AccentBrush"]).Color;
        try
        {
            ThemeManager.Apply(theme,systemDark,Accent);
            Assert.Equal(expected,ThemeManager.IsDark);
            var appliedAccent = ((SolidColorBrush)System.Windows.Application.Current.Resources["AccentBrush"]).Color;
            Assert.Equal((Color)ColorConverter.ConvertFromString(expected?"#17191F":"#F1F2F4"),
                ((SolidColorBrush)System.Windows.Application.Current.Resources["WindowBrush"]).Color);
            var window = ((SolidColorBrush)System.Windows.Application.Current.Resources["WindowBrush"]).Color;
            Assert.True(ThemeManager.Contrast(appliedAccent, window) >= ThemeConstants.MinimumTextContrast);
            foreach(var key in new[]{"TextBrush","CardBrush","ControlBrush","BorderBrush","HoverBrush","SelectedBrush","StatusBrush"})
                Assert.IsType<SolidColorBrush>(System.Windows.Application.Current.Resources[key]);
        }
        finally{ThemeManager.Apply(old?AppTheme.Dark:AppTheme.Light,false,oldAccent);}
    }
    public static void Refresh()
    {
        var old=ThemeManager.IsDark;
        var oldAccent=((SolidColorBrush)System.Windows.Application.Current.Resources["AccentBrush"]).Color;
        try
        {
            var control=new Border();
            control.Resources.MergedDictionaries.Add(System.Windows.Application.Current.Resources);
            control.SetResourceReference(Border.BackgroundProperty,"WindowBrush");
            ThemeManager.Apply(AppTheme.Light,false,Accent);
            var light=((SolidColorBrush)control.Background).Color;
            ThemeManager.Apply(AppTheme.Dark,false,Accent);
            var dark=((SolidColorBrush)control.Background).Color;
            Assert.NotEqual(light,dark);
            Assert.False(control.IsLoaded);
        }
        finally{ThemeManager.Apply(old?AppTheme.Dark:AppTheme.Light,false,oldAccent);}
    }

    public static void Contrast()
    {
        var old=ThemeManager.IsDark;
        var oldAccent=((SolidColorBrush)System.Windows.Application.Current.Resources["AccentBrush"]).Color;
        try
        {
            ThemeManager.Apply(AppTheme.Light,false,Colors.White);
            AssertReadable("AccentBrush", "WindowBrush");
            AssertReadable("MutedTextBrush", "WindowBrush");
            ThemeManager.Apply(AppTheme.Dark,false,Colors.Black);
            AssertReadable("AccentBrush", "WindowBrush");
            AssertReadable("MutedTextBrush", "WindowBrush");
        }
        finally{ThemeManager.Apply(old?AppTheme.Dark:AppTheme.Light,false,oldAccent);}
    }

    private static void AssertReadable(string foregroundKey,string backgroundKey)
    {
        var foreground=((SolidColorBrush)System.Windows.Application.Current.Resources[foregroundKey]).Color;
        var background=((SolidColorBrush)System.Windows.Application.Current.Resources[backgroundKey]).Color;
        Assert.True(ThemeManager.Contrast(foreground,background)>=ThemeConstants.MinimumTextContrast);
    }
}
