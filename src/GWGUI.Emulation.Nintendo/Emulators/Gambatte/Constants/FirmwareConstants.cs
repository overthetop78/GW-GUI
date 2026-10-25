using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Gambatte.Constants;

internal static class FirmwareConstants
{
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new("configuration.firmware.gb_bios.bin", "gb_bios.bin", "Emulation.Firmware.gambatte.Slot0", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoy, ModelConstants.GameBoyColor }),
        new("configuration.firmware.gbc_bios.bin", "gbc_bios.bin", "Emulation.Firmware.gambatte.Slot1", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoy, ModelConstants.GameBoyColor })
    ];
}
