using GWGUI.Emulation.Nintendo.Emulators.Mesen.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Mesen.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Mesen.Factories;
using GWGUI.Emulation.Nintendo.Emulators.Mesen.Functions;
using GWGUI.Emulation.Nintendo.Emulators.Mesen.Services;

namespace GWGUI.Emulation.Nintendo.Emulators.Mesen.Constants;

internal static class ProcessCoreConstants
{
    internal const string CoreHost = MesenConstants.CoreHostCommand;
    internal const string PipePrefix = "gwgui-amstrad-mesen-";
    internal const string VideoMapPrefix = "gwgui-amstrad-mesen-video-";
    internal const int PipeBufferSize = 8 * 1024 * 1024;
}


