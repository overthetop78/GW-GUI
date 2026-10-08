using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Mgba.Constants;

internal static class FirmwareConstants
{
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new("configuration.firmware.gba_bios.bin", "gba_bios.bin", "Emulation.Firmware.mgba.Slot0", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoyAdvance }),
        new("configuration.firmware.gb_bios.bin", "gb_bios.bin", "Emulation.Firmware.mgba.Slot1", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoyAdvance }),
        new("configuration.firmware.gbc_bios.bin", "gbc_bios.bin", "Emulation.Firmware.mgba.Slot2", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoyAdvance }),
        new("configuration.firmware.sgb_bios.bin", "sgb_bios.bin", "Emulation.Firmware.mgba.Slot3", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoyAdvance })
    ];
}
