using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Noods.Constants;

internal static class FirmwareConstants
{
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new("configuration.firmware.firmware.bin", "firmware.bin", "Emulation.Firmware.noods.Slot0", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.NintendoDs }),
        new("configuration.firmware.bios7.bin", "bios7.bin", "Emulation.Firmware.noods.Slot1", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.NintendoDs }),
        new("configuration.firmware.bios9.bin", "bios9.bin", "Emulation.Firmware.noods.Slot2", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.NintendoDs }),
        new("configuration.firmware.gba_bios.bin", "gba_bios.bin", "Emulation.Firmware.noods.Slot3", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.NintendoDs }),
        new("configuration.firmware.nds_sd_card.bin", "nds_sd_card.bin", "Emulation.Firmware.noods.Slot4", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.NintendoDs })
    ];
}
