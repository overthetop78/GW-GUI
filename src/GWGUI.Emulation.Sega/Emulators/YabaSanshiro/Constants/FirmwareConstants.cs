namespace GWGUI.Emulation.Sega.Emulators.YabaSanshiro.Constants;

internal static class FirmwareConstants
{
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new("configuration.firmware.yabasanshiro.0", "saturn_bios.bin", "Emulation.Firmware.yabasanshiro.0", true, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Saturn }),
    ];
}
