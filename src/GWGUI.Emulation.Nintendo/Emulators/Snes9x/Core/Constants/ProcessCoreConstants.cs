using GWGUI.Emulation.Nintendo.Emulators.Snes9x.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Snes9x.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Snes9x.Factories;
using GWGUI.Emulation.Nintendo.Emulators.Snes9x.Functions;
using GWGUI.Emulation.Nintendo.Emulators.Snes9x.Services;

namespace GWGUI.Emulation.Nintendo.Emulators.Snes9x.Constants;

internal static class ProcessCoreConstants
{
    internal const string CoreHost = Snes9xConstants.CoreHostCommand;
    internal const string PipePrefix = "gwgui-amstrad-snes9x-";
    internal const string VideoMapPrefix = "gwgui-amstrad-snes9x-video-";
    internal const int PipeBufferSize = 8 * 1024 * 1024;
}


