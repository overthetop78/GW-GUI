namespace GWGUI.App.Constants.Theming;

internal static class ThemeConstants
{
    internal const string PackPrefix = "/gwgui.app;component/Theme/";
    internal const string TokensUri = PackPrefix + "Base/Tokens.xaml";
    internal const string DarkPaletteSuffix = "/Dark.xaml";
    internal const string LightPaletteSuffix = "/Light.xaml";
    internal const string DarkPreviewSuffix = "/DarkPreview.png";
    internal const string LightPreviewSuffix = "/LightPreview.png";
    internal const string ResourcePrefix = "Options.Themes.";
    internal const string NameSuffix = ".Name";
    internal const string DescriptionSuffix = ".Description";
    internal const string AccentResource = "AccentBrush";
    internal const string BaseWindowStyleResource = "ApplicationWindowStyle";
    internal const string TextResource = "TextBrush";
    internal const string WindowResource = "WindowBrush";
    internal const int DefaultIndex = 0;
    internal const string DefaultAccent = "#4D76E8";
    internal const string LightWindow = "#F1F2F4";
    internal const string DarkWindow = "#17191F";
    internal const string LightMutedText = "#4D5562";
    internal const string DarkMutedText = "#CAD0DA";
    internal const double MinimumTextContrast = 4.5;
    internal const double ContrastAdjustmentStep = 0.05;
    internal const double SrgbThreshold = 0.04045;
    internal const double SrgbOffset = 0.055;
    internal const double SrgbDivisor = 1.055;
    internal const double SrgbExponent = 2.4;
    internal const double RedLuminance = 0.2126;
    internal const double GreenLuminance = 0.7152;
    internal const double BlueLuminance = 0.0722;
    internal const double ContrastOffset = 0.05;
}
