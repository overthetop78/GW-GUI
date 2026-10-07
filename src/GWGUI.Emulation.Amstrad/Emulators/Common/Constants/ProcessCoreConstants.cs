
namespace GWGUI.Emulation.Amstrad.Emulators.Common.Constants;

internal static class ProcessCoreConstants
{
    internal const string PipePrefix = "gwgui-amstrad-";
    internal const string VideoMapPrefix = "gwgui-amstrad-video-";
    internal const int PipeBufferSize = 8 * 1024 * 1024;
    internal const string ProcessExitDiagnosticPrefix = " It exited with code ";
    internal const string ProcessExitDiagnosticSuffix = ".";
    internal const int PipeInstanceCount = 1;
    internal const int HostConnectTimeoutSeconds = 15;
    internal const int HostResponseTimeoutSeconds = 30;
    internal const int HostExitTimeoutMilliseconds = 5000;
    internal const int NoSelectedDiskIndex = -1;
}
