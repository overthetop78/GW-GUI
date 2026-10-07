namespace GWGUI.Emulation.Amstrad.Emulators.Common.Interop.Constants;

internal static class ExternalCoreInteropConstants
{
    internal const uint ApiVersion = 1;
    internal const int MessageInterfaceVersion = 1;
    internal const int PixelFormat0Rgb1555 = 0;
    internal const int PixelFormatXrgb8888 = 1;
    internal const int PixelFormatRgb565 = 2;
    internal const uint NoInputState = 0;
    internal const byte NativeBooleanFalse = 0;
    internal const byte NativeBooleanTrue = 1;
    internal const uint EmptyNativeCollectionCount = 0;
    internal const uint EmptyFrameDimension = 0;
    internal const nuint EmptyNativeSize = 0;
    internal const int InactiveState = 0;

    internal const int CoreOptionsInterfaceVersion = 2;
    internal const int DiskControlInterfaceVersion = 1;
    internal const int OptionsUpdatedState = 1;
    internal const int DiskMetadataBufferSize = 4096;
    internal const short PressedInputState = 1;
    internal const short ReleasedInputState = 0;
}

