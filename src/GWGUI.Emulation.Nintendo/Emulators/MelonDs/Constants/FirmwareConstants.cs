using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.MelonDs.Constants;

internal static class FirmwareConstants
{
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new("configuration.firmware.firmware.bin", "firmware.bin", "Emulation.Firmware.melonds.Slot0", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.NintendoDs }),
        new("configuration.firmware.bios7.bin", "bios7.bin", "Emulation.Firmware.melonds.Slot1", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.NintendoDs }),
        new("configuration.firmware.bios9.bin", "bios9.bin", "Emulation.Firmware.melonds.Slot2", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.NintendoDs }),
        new("configuration.firmware.dsi_firmware.bin", "dsi_firmware.bin", "Emulation.Firmware.melonds.Slot3", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.NintendoDs }),
        new("configuration.firmware.dsi_bios7.bin", "dsi_bios7.bin", "Emulation.Firmware.melonds.Slot4", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.NintendoDs }),
        new("configuration.firmware.dsi_bios9.bin", "dsi_bios9.bin", "Emulation.Firmware.melonds.Slot5", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.NintendoDs }),
        new("configuration.firmware.dsi_nand.bin", "dsi_nand.bin", "Emulation.Firmware.melonds.Slot6", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.NintendoDs }),
        new("configuration.firmware.dsi_sd_card.bin", "dsi_sd_card.bin", "Emulation.Firmware.melonds.Slot7", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.NintendoDs })
    ];
}
