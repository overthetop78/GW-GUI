namespace GWGUI.Emulation.Sega.Emulators.Ymir.Constants;

internal static class FirmwareConstants
{
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new("configuration.firmware.ymir.0", "sega_101.bin", "Emulation.Firmware.ymir.0", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Saturn }),
        new("configuration.firmware.ymir.1", "mpr-17933.bin", "Emulation.Firmware.ymir.1", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Saturn }),
        new("configuration.firmware.ymir.2", "saturn_bios.bin", "Emulation.Firmware.ymir.2", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Saturn }),
    ];
}
