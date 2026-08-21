using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari;

internal static class EmulationMediaActivityFunctions
{
    internal static IReadOnlyDictionary<EmulationMediaSlot, bool> FromRuntimeStatus(
        AtariRuntimeStatus status) => status.MediaActivity;
}
