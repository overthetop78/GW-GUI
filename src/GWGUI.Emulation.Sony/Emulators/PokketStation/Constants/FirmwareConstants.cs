namespace GWGUI.Emulation.Sony.Emulators.PokketStation.Constants;

internal static class FirmwareConstants
{
    internal const string BiosField = "firmware.bios";
    internal const string BiosFile = "pocketstation.bin";
    internal const string BiosLabelResource = FirmwareCatalogConstants.ResourceBios;
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new(BiosField, BiosFile, BiosLabelResource, IsRequired: true)
    ];
}
