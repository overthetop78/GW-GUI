using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.MesenS.Constants;

internal static class FirmwareConstants
{
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new("configuration.firmware.dsp1.data.rom", "dsp1.data.rom", "Emulation.Firmware.mesen_s.Slot0", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dsp1.program.rom", "dsp1.program.rom", "Emulation.Firmware.mesen_s.Slot1", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dsp1b.data.rom", "dsp1b.data.rom", "Emulation.Firmware.mesen_s.Slot2", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dsp1b.program.rom", "dsp1b.program.rom", "Emulation.Firmware.mesen_s.Slot3", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dsp2.data.rom", "dsp2.data.rom", "Emulation.Firmware.mesen_s.Slot4", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dsp2.program.rom", "dsp2.program.rom", "Emulation.Firmware.mesen_s.Slot5", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dsp3.data.rom", "dsp3.data.rom", "Emulation.Firmware.mesen_s.Slot6", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dsp3.program.rom", "dsp3.program.rom", "Emulation.Firmware.mesen_s.Slot7", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dsp4.data.rom", "dsp4.data.rom", "Emulation.Firmware.mesen_s.Slot8", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dsp4.program.rom", "dsp4.program.rom", "Emulation.Firmware.mesen_s.Slot9", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.st010.data.rom", "st010.data.rom", "Emulation.Firmware.mesen_s.Slot10", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.st010.program.rom", "st010.program.rom", "Emulation.Firmware.mesen_s.Slot11", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.st011.data.rom", "st011.data.rom", "Emulation.Firmware.mesen_s.Slot12", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.st011.program.rom", "st011.program.rom", "Emulation.Firmware.mesen_s.Slot13", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dmg_boot.bin", "dmg_boot.bin", "Emulation.Firmware.mesen_s.Slot14", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoy }),
        new("configuration.firmware.cgb_boot.bin", "cgb_boot.bin", "Emulation.Firmware.mesen_s.Slot15", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoyColor }),
        new("configuration.firmware.sgb_boot.bin", "sgb_boot.bin", "Emulation.Firmware.mesen_s.Slot16", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoy, ModelConstants.GameBoyColor }),
        new("configuration.firmware.sgb2_boot.bin", "sgb2_boot.bin", "Emulation.Firmware.mesen_s.Slot17", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.SGB1.sfc", "SGB1.sfc", "Emulation.Firmware.mesen_s.Slot18", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.SGB2.sfc", "SGB2.sfc", "Emulation.Firmware.mesen_s.Slot19", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.BS-X.bin", "BS-X.bin", "Emulation.Firmware.mesen_s.Slot20", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes })
    ];
}
