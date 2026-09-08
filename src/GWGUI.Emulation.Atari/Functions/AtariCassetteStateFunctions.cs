using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Functions;

internal static class AtariCassetteStateFunctions
{
    internal static EmulationCassetteState From(bool mounted, bool motorActive) =>
        !mounted ? EmulationCassetteState.Empty
        : motorActive ? EmulationCassetteState.Playing
        : EmulationCassetteState.Stopped;
}
