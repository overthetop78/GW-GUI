using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;

internal static class ProcessCoreConstants
{
    internal const string CoreHost = "--sony-core-host";
    internal const string TransportName = "Sony";
    internal const string PipePrefix = "gwgui-sony-";
    internal const string VideoMapPrefix = "gwgui-sony-video-";
    internal const int GracefulShutdownTimeoutMilliseconds = 5_000;
    internal const int ConnectionTimeoutSeconds = 15;
    internal const int ResponseTimeoutSeconds = 30;
    internal const int PipeBufferSize = 8 * 1024 * 1024;
}

