namespace GWGUI.Emulation.Sega.Emulators.Gearsystem.Constants;

internal static class FirmwareConstants
{
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new("configuration.firmware.gearsystem.0", "bios.sms", "Emulation.Firmware.gearsystem.0", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.MasterSystem, ModelConstants.MarkIII }),
        new("configuration.firmware.gearsystem.1", "bios.gg", "Emulation.Firmware.gearsystem.1", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameGear }),
    ];
}
