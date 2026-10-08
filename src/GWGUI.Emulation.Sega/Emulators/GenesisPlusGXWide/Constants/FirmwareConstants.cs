namespace GWGUI.Emulation.Sega.Emulators.GenesisPlusGXWide.Constants;

internal static class FirmwareConstants
{
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new("configuration.firmware.genesis_plus_gx_wide.0", "bios_MD.bin", "Emulation.Firmware.genesis_plus_gx_wide.0", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.MegaDrive }),
        new("configuration.firmware.genesis_plus_gx_wide.1", "bios_CD_E.bin", "Emulation.Firmware.genesis_plus_gx_wide.1", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.MegaCd, ModelConstants.MegaDrive }),
        new("configuration.firmware.genesis_plus_gx_wide.2", "bios_CD_U.bin", "Emulation.Firmware.genesis_plus_gx_wide.2", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.MegaCd, ModelConstants.MegaDrive }),
        new("configuration.firmware.genesis_plus_gx_wide.3", "bios_CD_J.bin", "Emulation.Firmware.genesis_plus_gx_wide.3", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.MegaCd, ModelConstants.MegaDrive }),
        new("configuration.firmware.genesis_plus_gx_wide.4", "bios_E.sms", "Emulation.Firmware.genesis_plus_gx_wide.4", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.MasterSystem, ModelConstants.MarkIII }),
        new("configuration.firmware.genesis_plus_gx_wide.5", "bios_U.sms", "Emulation.Firmware.genesis_plus_gx_wide.5", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.MasterSystem, ModelConstants.MarkIII }),
        new("configuration.firmware.genesis_plus_gx_wide.6", "bios_J.sms", "Emulation.Firmware.genesis_plus_gx_wide.6", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.MasterSystem, ModelConstants.MarkIII }),
        new("configuration.firmware.genesis_plus_gx_wide.7", "bios.gg", "Emulation.Firmware.genesis_plus_gx_wide.7", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameGear }),
    ];
}
