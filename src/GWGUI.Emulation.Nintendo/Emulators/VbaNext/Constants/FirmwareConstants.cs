using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.VbaNext.Constants;

internal static class FirmwareConstants
{
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new("configuration.firmware.gba_bios.bin", "gba_bios.bin", "Emulation.Firmware.vba_next.Slot0", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoyAdvance })
    ];
}
