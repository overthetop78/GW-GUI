using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Gpsp.Constants;

internal static class FirmwareConstants
{
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new("configuration.firmware.gba_bios.bin", "gba_bios.bin", "Emulation.Firmware.gpsp.Slot0", true, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoyAdvance })
    ];
}
