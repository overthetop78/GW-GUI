namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants;

internal static class FirmwareConstants
{
    internal const string SystemField = "firmware.system";
    internal const string SystemLabelResource = FirmwareCatalogConstants.ResourceBios;
    internal const string SystemFile = "pcsx2/bios/scph.bin";
    internal const string Rom1Field = "firmware.rom1";
    internal const string Rom1LabelResource = "Emulation.Sony.Firmware.Pcsx2.Rom1";
    internal const string Rom1File = "pcsx2/bios/rom1.bin";
    internal const string ExtendedField = "firmware.extended";
    internal const string ExtendedLabelResource = "Emulation.Sony.Firmware.Pcsx2.Extended";
    internal const string ExtendedFile = "pcsx2/bios/EROM.BIN";
    internal const string MechanicsField = "firmware.mechanics";
    internal const string MechanicsLabelResource = "Emulation.Sony.Firmware.Pcsx2.Mechanics";
    internal const string MechanicsFile = "pcsx2/bios/scph.mec";
    internal const string NvramField = "firmware.nvram";
    internal const string NvramLabelResource = "Emulation.Sony.Firmware.Pcsx2.Nvram";
    internal const string NvramFile = "pcsx2/bios/scph.nvm";
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new(SystemField, SystemFile, SystemLabelResource, true, SystemFile),
        new(Rom1Field, Rom1File, Rom1LabelResource, false, Rom1File),
        new(ExtendedField, ExtendedFile, ExtendedLabelResource, false, ExtendedFile),
        new(MechanicsField, MechanicsFile, MechanicsLabelResource, false, MechanicsFile),
        new(NvramField, NvramFile, NvramLabelResource, false, NvramFile),
    ];
    internal const string ElfExtension = ".elf";
    internal const string IsoExtension = ".iso";
    internal const string CisoExtension = ".ciso";
    internal const string CueExtension = ".cue";
    internal const string BinExtension = ".bin";
    internal const string GzExtension = ".gz";
    internal const string ChdExtension = ".chd";
    internal const string CsoExtension = ".cso";
    internal const string ZsoExtension = ".zso";
    internal const string MdfExtension = ".mdf";
    internal const string NrgExtension = ".nrg";
    internal const string DumpExtension = ".dump";
    internal const string ImgExtension = ".img";
    internal static IReadOnlyList<string> ContentExtensions { get; } = [ElfExtension, IsoExtension, CisoExtension, CueExtension, BinExtension, GzExtension, ChdExtension, CsoExtension, ZsoExtension, MdfExtension, NrgExtension, DumpExtension, ImgExtension];
}
