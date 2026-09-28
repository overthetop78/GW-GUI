using GWGUI.Emulation.Sega.Emulators.GenesisPlusGX.Constants;
using GWGUI.Emulation.Sega.Emulators.GenesisPlusGX.Contracts;
using GWGUI.Emulation.Sega.Emulators.GenesisPlusGX.Factories;
using GWGUI.Emulation.Sega.Emulators.GenesisPlusGX.Functions;
using GWGUI.Emulation.Sega.Emulators.GenesisPlusGX.Services;

namespace GWGUI.Emulation.Sega.Emulators.GenesisPlusGX.Constants;

internal static class ProcessCoreConstants
{
    internal const string CoreHost = GenesisPlusGXConstants.CoreHostCommand;
    internal const string PipePrefix = "gwgui-sega-genesisplusgx-";
    internal const string VideoMapPrefix = "gwgui-sega-genesisplusgx-video-";
    internal const int PipeBufferSize = 8 * 1024 * 1024;
}
