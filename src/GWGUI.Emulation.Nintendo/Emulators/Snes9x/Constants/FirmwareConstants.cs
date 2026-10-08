using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Snes9x.Constants;

internal static class FirmwareConstants
{
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new("configuration.firmware.BS-X.bin", "BS-X.bin", "Emulation.Firmware.snes9x.Slot0", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }),
        new("configuration.firmware.STBIOS.bin", "STBIOS.bin", "Emulation.Firmware.snes9x.Slot1", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes })
    ];
}
