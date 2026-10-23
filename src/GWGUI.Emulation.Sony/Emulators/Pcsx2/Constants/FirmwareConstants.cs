using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants;

internal static class FirmwareConstants
{
    internal const string SystemField = "firmware.system";
    internal const string SystemLabelResource = FirmwareCatalogConstants.ResourceBios;
    internal const string SystemFile = "pcsx2/bios/scph.bin";
    internal const string Rom1Field = "firmware.rom1";
    internal const string Rom1File = "pcsx2/bios/scph.rom1";
    internal const string Rom2Field = "firmware.rom2";
    internal const string Rom2File = "pcsx2/bios/scph.rom2";
    internal const string Rom1DumpName = "rom1.bin";
    internal const string Rom2DumpName = "rom2.bin";
    internal const string MechanicsField = "firmware.mechanics";
    internal const string MechanicsFile = "pcsx2/bios/scph.mec";
    internal const string NvramField = "firmware.nvram";
    internal const string NvramFile = "pcsx2/bios/scph.nvm";
    internal const int NvramSize = 1024;
    internal const int MechanicsSize = 4;
    internal const string PersistentDirectory = "bios";
    internal static CoreFirmwarePersistenceSettings Persistence { get; } = new(SystemField, PersistentDirectory,
        [new(NvramField, NvramFile, NvramSize), new(MechanicsField, MechanicsFile, MechanicsSize)]);
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new(SystemField, SystemFile, SystemLabelResource, true, null, SystemFile),
        new(Rom1Field, Rom1File, FirmwareCatalogConstants.ResourceRom1, false, null, Rom1File, FirmwareCatalogConstants.ResourceRom1Help, FirmwareCatalogConstants.ResourceRom1Details, [Rom1DumpName]),
        new(Rom2Field, Rom2File, FirmwareCatalogConstants.ResourceRom2, false, null, Rom2File, FirmwareCatalogConstants.ResourceRom2Help, FirmwareCatalogConstants.ResourceRom2Details, [Rom2DumpName]),
        new(MechanicsField, MechanicsFile, FirmwareCatalogConstants.ResourceMechanics, false, null, MechanicsFile, FirmwareCatalogConstants.ResourceMechanicsHelp, FirmwareCatalogConstants.ResourceMechanicsDetails),
        new(NvramField, NvramFile, FirmwareCatalogConstants.ResourceNvram, false, null, NvramFile, FirmwareCatalogConstants.ResourceNvramHelp, FirmwareCatalogConstants.ResourceNvramDetails),
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
