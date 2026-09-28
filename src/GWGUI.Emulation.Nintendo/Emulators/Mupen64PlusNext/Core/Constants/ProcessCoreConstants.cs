using GWGUI.Emulation.Nintendo.Emulators.Mupen64PlusNext.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Mupen64PlusNext.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Mupen64PlusNext.Factories;
using GWGUI.Emulation.Nintendo.Emulators.Mupen64PlusNext.Functions;
using GWGUI.Emulation.Nintendo.Emulators.Mupen64PlusNext.Services;

namespace GWGUI.Emulation.Nintendo.Emulators.Mupen64PlusNext.Constants;

internal static class ProcessCoreConstants
{
    internal const string CoreHost = Mupen64PlusNextConstants.CoreHostCommand;
    internal const string PipePrefix = "gwgui-nintendo-mupen64plus-next-";
    internal const string VideoMapPrefix = "gwgui-nintendo-mupen64plus-next-video-";
    internal const int PipeBufferSize = 8 * 1024 * 1024;
}

