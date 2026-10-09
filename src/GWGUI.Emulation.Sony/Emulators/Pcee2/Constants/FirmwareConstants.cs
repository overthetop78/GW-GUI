namespace GWGUI.Emulation.Sony.Emulators.Pcee2.Constants;

internal static class FirmwareConstants
{
    internal const string SystemField = "firmware.system";
    internal const string SystemFile = "pcsx2/bios/scph.bin";
    internal const string SystemLabelResource = FirmwareCatalogConstants.ResourceBios;
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [new(SystemField, SystemFile, SystemLabelResource, true, SystemFile)];
    internal const string IsoExtension = ".iso";
    internal const string ChdExtension = ".chd";
    internal const string CueExtension = ".cue";
    internal const string CsoExtension = ".cso";
    internal const string ZsoExtension = ".zso";
    internal const string GzExtension = ".gz";
    internal const string BinExtension = ".bin";
    internal const string MdfExtension = ".mdf";
    internal const string NrgExtension = ".nrg";
    internal const string ElfExtension = ".elf";
    internal const string IrxExtension = ".irx";
    internal static IReadOnlyList<string> ContentExtensions { get; } =
    [IsoExtension, ChdExtension, CueExtension, CsoExtension, ZsoExtension, GzExtension, BinExtension, MdfExtension, NrgExtension, ElfExtension, IrxExtension];
}
