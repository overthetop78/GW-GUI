namespace GWGUI.Emulation.Mattel.Emulators.Common.Interop.Constants;

internal static class MediaRuntimeConstants
{
    internal const string ExecutiveFileName = "exec.bin";
    internal const string GraphicsFileName = "grom.bin";
    internal const int MaximumCartridgeBytes = 131072;
    internal const byte IntellicartMagic = 0xA8;
    internal const int IntellicartHeaderBytes = 3;
    internal const int SegmentHeaderBytes = 2;
    internal const int SegmentChecksumBytes = 2;
    internal const int WordsPerPage = 256;
    internal const int BytesPerWord = 2;
}
