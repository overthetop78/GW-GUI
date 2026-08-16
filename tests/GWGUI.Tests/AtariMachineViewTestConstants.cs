namespace GWGUI.Tests;

internal static class AtariMachineViewTestConstants
{
    internal const double WideWidth = 1600d;
    internal const double WideHeight = 900d;
    internal const double FourThreeWidth = 1200d;
    internal const double FourThreeHeight = 900d;
    internal const double WideAspectRatio = 16d / 9d;
    internal const double FourThreeAspectRatio = 4d / 3d;
    internal const int FrameWidth = 320;
    internal const int FrameHeight = 200;
    internal const int FramePitch = FrameWidth * BytesPerPixel;
    internal const int BytesPerPixel = 4;
    internal const int SampleRate = 44100;
    internal const double NativeFramesPerSecond = 59.94d;
    internal const double MeasuredFramesPerSecond = 60d;
    internal const int PixelDimension = 2;
    internal const int PixelDpi = 96;
    internal const int PixelStride = PixelDimension * BytesPerPixel;
    internal const int StaTimeoutMilliseconds = 10000;
    internal const int DimensionPrecision = 4;
    internal const int AspectRatioPrecision = 5;
    internal const int ExpectedPowerCycleMachineCount = 2;
    internal const int PointerDeltaX = 7;
    internal const int PointerDeltaY = -3;
    internal const int PointerWheel = 120;
    internal const float DefaultAudioVolume = 1f;
    internal const string FirstMediaPath = "disk.st";
    internal const string CaptureFolderName = "gwgui-atari-view-captures";
    internal const string StateFileName = "quick.gwats";
}
