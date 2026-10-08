namespace GWGUI.Emulation.Sega.Emulators.Kronos.Constants;

internal static class FirmwareConstants
{
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new("configuration.firmware.kronos.0", "kronos/saturn_bios.bin", "Emulation.Firmware.kronos.0", true, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Saturn }),
        new("configuration.firmware.kronos.1", "kronos/stvbios.zip", "Emulation.Firmware.kronos.1", true, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.StV }),
    ];
}
