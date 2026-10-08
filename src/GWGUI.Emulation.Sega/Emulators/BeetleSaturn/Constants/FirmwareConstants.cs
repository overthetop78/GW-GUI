namespace GWGUI.Emulation.Sega.Emulators.BeetleSaturn.Constants;

internal static class FirmwareConstants
{
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new("configuration.firmware.mednafen_saturn.0", "sega_101.bin", "Emulation.Firmware.mednafen_saturn.0", true, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Saturn }),
        new("configuration.firmware.mednafen_saturn.1", "mpr-17933.bin", "Emulation.Firmware.mednafen_saturn.1", true, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Saturn }),
    ];
}
