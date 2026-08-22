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
    internal const string FloppyGlyph = "\uE7C3";
    internal const string HardDiskGlyph = "\uEDA2";
    internal const string CompactDiscGlyph = "\uE958";
    internal const string CartridgeGlyph = "\uE7FC";
    internal const string CassetteGlyph = "\uE8D4";
    internal const double DefaultAspectRatio = 4d / 3d;
    internal const double WideToolbarMinimumWidth = 1450d;
    internal const double EmptyMeasurement = 0d;
    internal const int InactiveFramePending = 0;
    internal const int ActiveFramePending = 1;
    internal const int FrameRateWindowSeconds = 1;
    internal const int FirstDuplicateSuffix = 2;
}
