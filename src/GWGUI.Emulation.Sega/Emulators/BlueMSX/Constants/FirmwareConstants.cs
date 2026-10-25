namespace GWGUI.Emulation.Sega.Emulators.BlueMSX.Constants;

internal static class FirmwareConstants
{
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new("configuration.firmware.bluemsx.0", "Machines/SEGA - SF-7000/sf7000.rom", "Emulation.Firmware.bluemsx.0", true, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Sf7000 }),
    ];
}
