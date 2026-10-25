using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Skyemu.Constants;

internal static class FirmwareConstants
{
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new("configuration.firmware.cgb_boot.bin", "cgb_boot.bin", "Emulation.Firmware.skyemu.Slot0", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoyColor }),
        new("configuration.firmware.gbc_bios.bin", "gbc_bios.bin", "Emulation.Firmware.skyemu.Slot1", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoyColor }),
        new("configuration.firmware.cgb0_boot.bin", "cgb0_boot.bin", "Emulation.Firmware.skyemu.Slot2", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoyColor }),
        new("configuration.firmware.cgb_agb_boot.bin", "cgb_agb_boot.bin", "Emulation.Firmware.skyemu.Slot3", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoyColor }),
        new("configuration.firmware.dmg_rom.bin", "dmg_rom.bin", "Emulation.Firmware.skyemu.Slot4", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoy }),
        new("configuration.firmware.dmg0_rom.bin", "dmg0_rom.bin", "Emulation.Firmware.skyemu.Slot5", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoy }),
        new("configuration.firmware.gba_bios.bin", "gba_bios.bin", "Emulation.Firmware.skyemu.Slot6", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoyAdvance }),
        new("configuration.firmware.nds7.bin", "nds7.bin", "Emulation.Firmware.skyemu.Slot7", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.NintendoDs }),
        new("configuration.firmware.nds9.bin", "nds9.bin", "Emulation.Firmware.skyemu.Slot8", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.NintendoDs }),
        new("configuration.firmware.firmware.bin", "firmware.bin", "Emulation.Firmware.skyemu.Slot9", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.NintendoDs })
    ];
}
