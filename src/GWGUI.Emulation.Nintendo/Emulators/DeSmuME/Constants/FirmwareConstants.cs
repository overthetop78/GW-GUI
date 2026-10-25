using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.DeSmuME.Constants;

internal static class FirmwareConstants
{
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new("configuration.firmware.firmware.bin", "firmware.bin", "Emulation.Firmware.desmume.Slot0", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.NintendoDs }),
        new("configuration.firmware.bios7.bin", "bios7.bin", "Emulation.Firmware.desmume.Slot1", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.NintendoDs }),
        new("configuration.firmware.bios9.bin", "bios9.bin", "Emulation.Firmware.desmume.Slot2", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.NintendoDs })
    ];
}
