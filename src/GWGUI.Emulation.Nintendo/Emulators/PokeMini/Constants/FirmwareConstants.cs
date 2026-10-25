using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.PokeMini.Constants;

internal static class FirmwareConstants
{
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new("configuration.firmware.bios.min", "bios.min", "Emulation.Firmware.pokemini.Slot0", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.PokemonMini })
    ];
}
