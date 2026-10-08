using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.BsnesJg.Constants;

internal static class FirmwareConstants
{
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new("configuration.firmware.dsp1.data.rom", "dsp1.data.rom", "Emulation.Firmware.bsnes_jg.Slot0", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dsp1.program.rom", "dsp1.program.rom", "Emulation.Firmware.bsnes_jg.Slot1", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dsp1b.data.rom", "dsp1b.data.rom", "Emulation.Firmware.bsnes_jg.Slot2", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dsp1b.program.rom", "dsp1b.program.rom", "Emulation.Firmware.bsnes_jg.Slot3", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dsp2.data.rom", "dsp2.data.rom", "Emulation.Firmware.bsnes_jg.Slot4", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dsp2.program.rom", "dsp2.program.rom", "Emulation.Firmware.bsnes_jg.Slot5", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dsp3.data.rom", "dsp3.data.rom", "Emulation.Firmware.bsnes_jg.Slot6", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dsp3.program.rom", "dsp3.program.rom", "Emulation.Firmware.bsnes_jg.Slot7", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dsp4.data.rom", "dsp4.data.rom", "Emulation.Firmware.bsnes_jg.Slot8", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dsp4.program.rom", "dsp4.program.rom", "Emulation.Firmware.bsnes_jg.Slot9", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.st010.data.rom", "st010.data.rom", "Emulation.Firmware.bsnes_jg.Slot10", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.st010.program.rom", "st010.program.rom", "Emulation.Firmware.bsnes_jg.Slot11", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.st011.data.rom", "st011.data.rom", "Emulation.Firmware.bsnes_jg.Slot12", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.st011.program.rom", "st011.program.rom", "Emulation.Firmware.bsnes_jg.Slot13", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.st018.data.rom", "st018.data.rom", "Emulation.Firmware.bsnes_jg.Slot14", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.st018.program.rom", "st018.program.rom", "Emulation.Firmware.bsnes_jg.Slot15", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes })
    ];
}
