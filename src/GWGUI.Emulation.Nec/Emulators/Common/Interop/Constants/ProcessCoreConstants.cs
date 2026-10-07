using GWGUI.Emulation.Nec.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Services;

namespace GWGUI.Emulation.Nec.Emulators.Common.Interop.Constants;

internal static class ProcessCoreConstants
{
    internal const string CoreHost = "--nec-core-host";
    internal const string PipePrefix = "gwgui-nec-beetle-pce-";
    internal const string VideoMapPrefix = "gwgui-nec-beetle-pce-video-";
    internal const int PipeBufferSize = 8 * 1024 * 1024;
}
