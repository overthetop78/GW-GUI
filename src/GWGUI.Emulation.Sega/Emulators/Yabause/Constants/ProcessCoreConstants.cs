using GWGUI.Emulation.Sega.Emulators.Yabause.Constants;
using GWGUI.Emulation.Sega.Emulators.Yabause.Contracts;
using GWGUI.Emulation.Sega.Emulators.Yabause.Factories;
using GWGUI.Emulation.Sega.Emulators.Yabause.Functions;
using GWGUI.Emulation.Sega.Emulators.Yabause.Services;

namespace GWGUI.Emulation.Sega.Emulators.Yabause.Constants;

internal static class ProcessCoreConstants
{
    internal const string CoreHost = YabauseConstants.CoreHostCommand;
    internal const string PipePrefix = "gwgui-sega-yabause-";
    internal const string VideoMapPrefix = "gwgui-sega-yabause-video-";
    internal const int PipeBufferSize = 8 * 1024 * 1024;
}
