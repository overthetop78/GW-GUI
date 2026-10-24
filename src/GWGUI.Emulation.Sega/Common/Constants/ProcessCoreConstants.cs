
namespace GWGUI.Emulation.Sega.Common.Constants;

internal static class ProcessCoreConstants
{
    internal const int GracefulExitTimeoutMilliseconds = 5_000;
    internal const string CoreHost = "--sega-core-host";
    internal const string PipePrefix = "gwgui-nintendo-";
    internal const string VideoMapPrefix = "gwgui-nintendo-video-";
    internal const int PipeBufferSize = 8 * 1024 * 1024;
    internal const string TransportName = "Sega";
}

