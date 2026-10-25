using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.MelonDsDs.Constants;

internal static class FirmwareConstants
{
    internal const string DsFirmwareFile = "firmware.bin";
    internal const string DsiFirmwareFile = "dsi_firmware.bin";
    internal const string DsiNandFile = "dsi_nand.bin";
    internal const string DsFirmwareOption = "melonds_firmware_nds_path";
    internal const string DsiFirmwareOption = "melonds_firmware_dsi_path";
    internal const string DsiNandOption = "melonds_dsi_nand_path";
    internal static IReadOnlyDictionary<string, string> PathOptions { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        [DsFirmwareFile] = DsFirmwareOption,
        [DsiFirmwareFile] = DsiFirmwareOption,
        [DsiNandFile] = DsiNandOption
    };
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new("configuration.firmware.firmware.bin", "firmware.bin", "Emulation.Firmware.melondsds.Slot0", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.NintendoDs, ModelConstants.NintendoDsi }),
        new("configuration.firmware.bios7.bin", "bios7.bin", "Emulation.Firmware.melondsds.Slot1", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.NintendoDs, ModelConstants.NintendoDsi }),
        new("configuration.firmware.bios9.bin", "bios9.bin", "Emulation.Firmware.melondsds.Slot2", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.NintendoDs, ModelConstants.NintendoDsi }),
        new("configuration.firmware.dsi_firmware.bin", "dsi_firmware.bin", "Emulation.Firmware.melondsds.Slot3", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.NintendoDsi }),
        new("configuration.firmware.dsi_bios7.bin", "dsi_bios7.bin", "Emulation.Firmware.melondsds.Slot4", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.NintendoDsi }),
        new("configuration.firmware.dsi_bios9.bin", "dsi_bios9.bin", "Emulation.Firmware.melondsds.Slot5", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.NintendoDsi }),
        new("configuration.firmware.dsi_nand.bin", "dsi_nand.bin", "Emulation.Firmware.melondsds.Slot6", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.NintendoDsi })
    ];
}
