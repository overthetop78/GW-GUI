using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Mupen64PlusNext.Constants;

internal static class FirmwareConstants
{
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new("configuration.firmware.Mupen64plus/IPL.n64", "Mupen64plus/IPL.n64", "Emulation.Firmware.mupen64plus-next.Slot0", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Nintendo64 })
    ];
}
