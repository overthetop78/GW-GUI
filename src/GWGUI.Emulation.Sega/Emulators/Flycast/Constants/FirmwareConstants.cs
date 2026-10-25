namespace GWGUI.Emulation.Sega.Emulators.Flycast.Constants;

internal static class FirmwareConstants
{
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new("configuration.firmware.flycast.0", "dc/dc_boot.bin", "Emulation.Firmware.flycast.0", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Dreamcast }),
        new("configuration.firmware.flycast.1", "dc/naomi.zip", "Emulation.Firmware.flycast.1", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Naomi }),
        new("configuration.firmware.flycast.2", "dc/naomi2.zip", "Emulation.Firmware.flycast.2", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Naomi2 }),
        new("configuration.firmware.flycast.3", "dc/hod2bios.zip", "Emulation.Firmware.flycast.3", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Naomi }),
        new("configuration.firmware.flycast.4", "dc/f355dlx.zip", "Emulation.Firmware.flycast.4", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Naomi }),
        new("configuration.firmware.flycast.5", "dc/f355bios.zip", "Emulation.Firmware.flycast.5", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Naomi }),
        new("configuration.firmware.flycast.6", "dc/airlbios.zip", "Emulation.Firmware.flycast.6", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Naomi }),
        new("configuration.firmware.flycast.7", "dc/awbios.zip", "Emulation.Firmware.flycast.7", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Atomiswave }),
        new("configuration.firmware.flycast.8", "dc/dc_flash.bin", "Emulation.Firmware.flycast.8", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Dreamcast }),
        new("configuration.firmware.flycast.9", "dc/segasp.zip", "Emulation.Firmware.flycast.9", true, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.SystemSp }),
    ];
}
