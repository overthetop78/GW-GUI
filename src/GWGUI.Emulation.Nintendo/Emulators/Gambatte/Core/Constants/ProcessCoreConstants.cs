using GWGUI.Emulation.Nintendo.Emulators.Gambatte.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Gambatte.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Gambatte.Factories;
using GWGUI.Emulation.Nintendo.Emulators.Gambatte.Functions;
using GWGUI.Emulation.Nintendo.Emulators.Gambatte.Services;

namespace GWGUI.Emulation.Nintendo.Emulators.Gambatte.Constants;

internal static class ProcessCoreConstants
{
    internal const string CoreHost = GambatteConstants.CoreHostCommand;
    internal const string PipePrefix = "gwgui-nintendo-gambatte-";
    internal const string VideoMapPrefix = "gwgui-nintendo-gambatte-video-";
    internal const int PipeBufferSize = 8 * 1024 * 1024;
}

