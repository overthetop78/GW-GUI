using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Irogb.Constants;

internal static class FirmwareConstants
{
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new("configuration.firmware.dmg_boot.bin", "dmg_boot.bin", "Emulation.Firmware.irogb.Slot0", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoy, ModelConstants.GameBoyColor }),
        new("configuration.firmware.cgb_bios.bin", "cgb_bios.bin", "Emulation.Firmware.irogb.Slot1", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoy, ModelConstants.GameBoyColor })
    ];
}
