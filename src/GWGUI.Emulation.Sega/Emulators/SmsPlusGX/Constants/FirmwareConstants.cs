namespace GWGUI.Emulation.Sega.Emulators.SmsPlusGX.Constants;

internal static class FirmwareConstants
{
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new("configuration.firmware.smsplus.0", "bios.sms", "Emulation.Firmware.smsplus.0", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.MasterSystem }),
    ];
}

