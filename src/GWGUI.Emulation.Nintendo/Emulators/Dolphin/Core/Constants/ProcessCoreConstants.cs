using GWGUI.Emulation.Nintendo.Emulators.Dolphin.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Dolphin.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Dolphin.Factories;
using GWGUI.Emulation.Nintendo.Emulators.Dolphin.Functions;
using GWGUI.Emulation.Nintendo.Emulators.Dolphin.Services;

namespace GWGUI.Emulation.Nintendo.Emulators.Dolphin.Constants;

internal static class ProcessCoreConstants
{
    internal const string CoreHost = DolphinConstants.CoreHostCommand;
    internal const string PipePrefix = "gwgui-nintendo-dolphin-";
    internal const string VideoMapPrefix = "gwgui-nintendo-dolphin-video-";
    internal const int PipeBufferSize = 8 * 1024 * 1024;
}
