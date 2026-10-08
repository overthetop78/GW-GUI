using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Dolphin.Constants;

internal static class FirmwareConstants
{
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new("configuration.firmware.dolphin-emu/Sys/codehandler.bin", "dolphin-emu/Sys/codehandler.bin", "Emulation.Firmware.dolphin.Slot0", true, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameCube, ModelConstants.Wii }),
        new("configuration.firmware.dolphin-emu/Sys/GC/JAP/IPL.bin", "dolphin-emu/Sys/GC/JAP/IPL.bin", "Emulation.Firmware.dolphin.Slot1", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameCube, ModelConstants.Wii }),
        new("configuration.firmware.dolphin-emu/Sys/GC/USA/IPL.bin", "dolphin-emu/Sys/GC/USA/IPL.bin", "Emulation.Firmware.dolphin.Slot2", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameCube, ModelConstants.Wii }),
        new("configuration.firmware.dolphin-emu/Sys/GC/EUR/IPL.bin", "dolphin-emu/Sys/GC/EUR/IPL.bin", "Emulation.Firmware.dolphin.Slot3", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameCube, ModelConstants.Wii })
    ];
}
