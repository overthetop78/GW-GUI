using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Fixnes.Constants;

internal static class FirmwareConstants
{
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new("configuration.firmware.disksys.rom", "disksys.rom", "Emulation.Firmware.fixnes.Slot0", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.FamicomDisk })
    ];
}
