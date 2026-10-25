using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Sameboy.Constants;

internal static class FirmwareConstants
{
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new("configuration.firmware.dmg_boot.bin", "dmg_boot.bin", "Emulation.Firmware.sameboy.Slot0", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoy, ModelConstants.GameBoyColor }),
        new("configuration.firmware.cgb_boot.bin", "cgb_boot.bin", "Emulation.Firmware.sameboy.Slot1", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoy, ModelConstants.GameBoyColor })
    ];
}
