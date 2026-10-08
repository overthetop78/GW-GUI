using GWGUI.Emulation.Sega.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Services;

namespace GWGUI.Emulation.Sega.Emulators.Common.Interop.Constants;

internal static class ProcessCoreConstants
{
    internal const int GracefulExitTimeoutMilliseconds = 5_000;
    internal const string CoreHost = "--sega-core-host";
    internal const string PipePrefix = "gwgui-nintendo-";
    internal const string VideoMapPrefix = "gwgui-nintendo-video-";
    internal const int PipeBufferSize = 8 * 1024 * 1024;
}

