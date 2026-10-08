using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.BsnesMercuryPerformance.Constants;

internal static class FirmwareConstants
{
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new("configuration.firmware.dsp1.data.rom", "dsp1.data.rom", "Emulation.Firmware.bsnes_mercury_performance.Slot0", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dsp1.program.rom", "dsp1.program.rom", "Emulation.Firmware.bsnes_mercury_performance.Slot1", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dsp1b.data.rom", "dsp1b.data.rom", "Emulation.Firmware.bsnes_mercury_performance.Slot2", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dsp1b.program.rom", "dsp1b.program.rom", "Emulation.Firmware.bsnes_mercury_performance.Slot3", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dsp2.data.rom", "dsp2.data.rom", "Emulation.Firmware.bsnes_mercury_performance.Slot4", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dsp2.program.rom", "dsp2.program.rom", "Emulation.Firmware.bsnes_mercury_performance.Slot5", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dsp3.data.rom", "dsp3.data.rom", "Emulation.Firmware.bsnes_mercury_performance.Slot6", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dsp3.program.rom", "dsp3.program.rom", "Emulation.Firmware.bsnes_mercury_performance.Slot7", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dsp4.data.rom", "dsp4.data.rom", "Emulation.Firmware.bsnes_mercury_performance.Slot8", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.dsp4.program.rom", "dsp4.program.rom", "Emulation.Firmware.bsnes_mercury_performance.Slot9", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.st010.data.rom", "st010.data.rom", "Emulation.Firmware.bsnes_mercury_performance.Slot10", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.st010.program.rom", "st010.program.rom", "Emulation.Firmware.bsnes_mercury_performance.Slot11", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.st011.data.rom", "st011.data.rom", "Emulation.Firmware.bsnes_mercury_performance.Slot12", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.st011.program.rom", "st011.program.rom", "Emulation.Firmware.bsnes_mercury_performance.Slot13", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.st018.data.rom", "st018.data.rom", "Emulation.Firmware.bsnes_mercury_performance.Slot14", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.st018.program.rom", "st018.program.rom", "Emulation.Firmware.bsnes_mercury_performance.Slot15", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.sgb.boot.rom", "sgb.boot.rom", "Emulation.Firmware.bsnes_mercury_performance.Slot16", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes })
    ];
}
