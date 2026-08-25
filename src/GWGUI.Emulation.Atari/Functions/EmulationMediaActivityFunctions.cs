using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Functions;

internal static class EmulationMediaActivityFunctions
{
    internal static IReadOnlyDictionary<EmulationMediaSlot, bool> FromRuntimeStatus(
        AtariRuntimeStatus status) => status.MediaActivity;
}
