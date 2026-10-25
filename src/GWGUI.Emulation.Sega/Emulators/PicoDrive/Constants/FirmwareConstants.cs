namespace GWGUI.Emulation.Sega.Emulators.PicoDrive.Constants;

internal static class FirmwareConstants
{
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new("configuration.firmware.picodrive.0", "bios_CD_E.bin", "Emulation.Firmware.picodrive.0", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.MegaDrive, ModelConstants.MegaCd }),
        new("configuration.firmware.picodrive.1", "bios_CD_U.bin", "Emulation.Firmware.picodrive.1", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.MegaDrive, ModelConstants.MegaCd }),
        new("configuration.firmware.picodrive.2", "bios_CD_J.bin", "Emulation.Firmware.picodrive.2", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.MegaDrive, ModelConstants.MegaCd }),
    ];
}

