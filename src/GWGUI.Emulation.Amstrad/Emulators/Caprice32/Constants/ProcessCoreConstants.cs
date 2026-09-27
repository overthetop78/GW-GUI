using GWGUI.Emulation.Amstrad.Emulators.Caprice32.Constants;
using GWGUI.Emulation.Amstrad.Emulators.Caprice32.Contracts;
using GWGUI.Emulation.Amstrad.Emulators.Caprice32.Factories;
using GWGUI.Emulation.Amstrad.Emulators.Caprice32.Functions;
using GWGUI.Emulation.Amstrad.Emulators.Caprice32.Services;

namespace GWGUI.Emulation.Amstrad.Emulators.Caprice32.Constants;

internal static class ProcessCoreConstants
{
    internal const string CoreHost = Caprice32Constants.CoreHostCommand;
    internal const string PipePrefix = "gwgui-amstrad-caprice32-";
    internal const string VideoMapPrefix = "gwgui-amstrad-caprice32-video-";
    internal const int PipeBufferSize = 8 * 1024 * 1024;
}
