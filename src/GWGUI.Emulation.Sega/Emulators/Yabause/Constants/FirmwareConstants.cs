namespace GWGUI.Emulation.Sega.Emulators.Yabause.Constants;

internal static class FirmwareConstants
{
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new("configuration.firmware.yabause.0", "saturn_bios.bin", "Emulation.Firmware.yabause.0", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Saturn }),
    ];
}
