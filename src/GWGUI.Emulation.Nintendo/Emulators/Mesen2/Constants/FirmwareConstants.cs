using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Mesen2.Constants;

internal static class FirmwareConstants
{
    internal const string FamicomDiskBiosField = "configuration.firmware.disksys.rom";
    internal const string FamicomDiskBiosFile = "disksys.rom";
    internal const string FamicomDiskBiosLabel = "Emulation.Firmware.FamicomDiskSystem";
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new(FamicomDiskBiosField, FamicomDiskBiosFile, FamicomDiskBiosLabel, true, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.FamicomDisk }),
        new("configuration.firmware.dsp1.data.rom", "dsp1.data.rom", "Emulation.Firmware.mesen2.Slot0", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dsp1.program.rom", "dsp1.program.rom", "Emulation.Firmware.mesen2.Slot1", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dsp1b.data.rom", "dsp1b.data.rom", "Emulation.Firmware.mesen2.Slot2", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dsp1b.program.rom", "dsp1b.program.rom", "Emulation.Firmware.mesen2.Slot3", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dsp2.data.rom", "dsp2.data.rom", "Emulation.Firmware.mesen2.Slot4", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dsp2.program.rom", "dsp2.program.rom", "Emulation.Firmware.mesen2.Slot5", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dsp3.data.rom", "dsp3.data.rom", "Emulation.Firmware.mesen2.Slot6", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dsp3.program.rom", "dsp3.program.rom", "Emulation.Firmware.mesen2.Slot7", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dsp4.data.rom", "dsp4.data.rom", "Emulation.Firmware.mesen2.Slot8", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dsp4.program.rom", "dsp4.program.rom", "Emulation.Firmware.mesen2.Slot9", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.st010.data.rom", "st010.data.rom", "Emulation.Firmware.mesen2.Slot10", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.st010.program.rom", "st010.program.rom", "Emulation.Firmware.mesen2.Slot11", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.st011.data.rom", "st011.data.rom", "Emulation.Firmware.mesen2.Slot12", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.st011.program.rom", "st011.program.rom", "Emulation.Firmware.mesen2.Slot13", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dmg_boot.bin", "dmg_boot.bin", "Emulation.Firmware.mesen2.Slot14", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoy }),
        new("configuration.firmware.cgb_boot.bin", "cgb_boot.bin", "Emulation.Firmware.mesen2.Slot15", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoyColor }),
        new("configuration.firmware.gb_bios.bin", "gb_bios.bin", "Emulation.Firmware.mesen2.Slot23", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoy }),
        new("configuration.firmware.gbc_bios.bin", "gbc_bios.bin", "Emulation.Firmware.mesen2.Slot24", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoyColor }),
        new("configuration.firmware.sgb_bios.bin", "sgb_bios.bin", "Emulation.Firmware.mesen2.Slot25", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoy })
    ];
}
