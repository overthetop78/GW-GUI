using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.FceUmm.Constants;

internal static class FirmwareConstants
{
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new("configuration.firmware.disksys.rom", "disksys.rom", "Emulation.Firmware.fceumm.Slot0", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.FamicomDisk }),
        new("configuration.firmware.nes.pal", "nes.pal", "Emulation.Firmware.fceumm.Slot1", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Nes, ModelConstants.FamicomDisk, ModelConstants.FamicomDisk }),
        new("configuration.firmware.gamegenie.nes", "gamegenie.nes", "Emulation.Firmware.fceumm.Slot2", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Nes })
    ];
}
