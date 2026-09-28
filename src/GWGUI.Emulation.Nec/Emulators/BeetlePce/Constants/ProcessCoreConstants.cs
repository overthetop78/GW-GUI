using GWGUI.Emulation.Nec.Emulators.BeetlePce.Constants;
using GWGUI.Emulation.Nec.Emulators.BeetlePce.Contracts;
using GWGUI.Emulation.Nec.Emulators.BeetlePce.Factories;
using GWGUI.Emulation.Nec.Emulators.BeetlePce.Functions;
using GWGUI.Emulation.Nec.Emulators.BeetlePce.Services;

namespace GWGUI.Emulation.Nec.Emulators.BeetlePce.Constants;

internal static class ProcessCoreConstants
{
    internal const string CoreHost = BeetlePceConstants.CoreHostCommand;
    internal const string PipePrefix = "gwgui-nec-beetle-pce-";
    internal const string VideoMapPrefix = "gwgui-nec-beetle-pce-video-";
    internal const int PipeBufferSize = 8 * 1024 * 1024;
}

