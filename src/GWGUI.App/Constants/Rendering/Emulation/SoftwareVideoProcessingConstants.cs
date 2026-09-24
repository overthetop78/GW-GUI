namespace GWGUI.App.Constants.Rendering.Emulation;

internal static class SoftwareVideoProcessingConstants
{
    internal const float RedLuminance = 0.2126f;
    internal const float GreenLuminance = 0.7152f;
    internal const float BlueLuminance = 0.0722f;
    internal const double PalFrameDurationThresholdMilliseconds = 18.2;
    internal const float SrgbToLinearThreshold = 0.04045f;
    internal const float LinearToSrgbThreshold = 0.0031308f;
    internal const float SrgbLinearScale = 12.92f;
    internal const float SrgbOffset = 0.055f;
    internal const float SrgbScale = 1.055f;
    internal const float SrgbGamma = 2.4f;
    internal const float MaximumColorComponent = 255f;
    internal const int MinimumColorByte = 0;
    internal const int MaximumColorByte = 255;
}
