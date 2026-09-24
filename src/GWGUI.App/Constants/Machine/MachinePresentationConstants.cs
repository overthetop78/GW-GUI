using GWGUI.App.Constants.Controls.Visual;

namespace GWGUI.App.Constants.Machine;

internal static class MachinePresentationConstants
{
    internal const string CaptureFileExtension = ".png";
    internal const string CaptureTimestampFormat = "yyyyMMdd-HHmmss";
    internal const string CaptureStem = "capture";
    internal const string FileNameSeparator = "-";
    internal const string Direct3D11Renderer = "Direct3D 11";
    internal const string WpfRenderer = "WPF";
    internal const string StatusFormat = "{0} × {1} · {2:0.0} Hz · {3:0.0} FPS";
    internal static string FloppyGlyph => IconGlyphs.FloppyDisk;
    internal static string HardDiskGlyph => IconGlyphs.HardDisk;
    internal static string CompactDiscGlyph => IconGlyphs.OpticalDisc;
    internal static string CartridgeGlyph => IconGlyphs.Controller;
    internal static string CassetteGlyph => IconGlyphs.Cassette;
    internal const double DefaultAspectRatio = 4d / 3d;
    internal const double WideToolbarMinimumWidth = 1450d;
    internal const double EmptyMeasurement = 0d;
    internal const int InactiveFramePending = 0;
    internal const int ActiveFramePending = 1;
    internal const int FrameRateWindowSeconds = 1;
    internal const int UiFrameNotificationMilliseconds = 100;
    internal const int FirstDuplicateSuffix = 2;
}
