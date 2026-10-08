namespace GWGUI.Emulation.Sony.Emulators.SwanStation.Constants;

internal static class FirmwareConstants
{
    internal const string JapanField = "firmware.japan";
    internal const string JapanLabelResource = "Emulation.Sony.Firmware.SwanStation.Japan";
    internal const string JapanFile = "scph5500.bin";
    internal const string NorthAmericaField = "firmware.northamerica";
    internal const string NorthAmericaLabelResource = "Emulation.Sony.Firmware.SwanStation.NorthAmerica";
    internal const string NorthAmericaFile = "scph5501.bin";
    internal const string EuropeField = "firmware.europe";
    internal const string EuropeLabelResource = "Emulation.Sony.Firmware.SwanStation.Europe";
    internal const string EuropeFile = "scph5502.bin";
    internal const string PspField = "firmware.psp";
    internal const string PspLabelResource = "Emulation.Sony.Firmware.SwanStation.Psp";
    internal const string PspFile = "psxonpsp660.bin";
    internal const string Ps3Field = "firmware.ps3";
    internal const string Ps3LabelResource = "Emulation.Sony.Firmware.SwanStation.Ps3";
    internal const string Ps3File = "ps1_rom.bin";
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new(JapanField, JapanFile, JapanLabelResource, false, JapanFile),
        new(NorthAmericaField, NorthAmericaFile, NorthAmericaLabelResource, false, NorthAmericaFile),
        new(EuropeField, EuropeFile, EuropeLabelResource, false, EuropeFile),
        new(PspField, PspFile, PspLabelResource, false, PspFile),
        new(Ps3Field, Ps3File, Ps3LabelResource, false, Ps3File),
    ];
    internal const string ExeExtension = ".exe";
    internal const string PsexeExtension = ".psexe";
    internal const string CueExtension = ".cue";
    internal const string BinExtension = ".bin";
    internal const string ImgExtension = ".img";
    internal const string IsoExtension = ".iso";
    internal const string ChdExtension = ".chd";
    internal const string PbpExtension = ".pbp";
    internal const string EcmExtension = ".ecm";
    internal const string MdsExtension = ".mds";
    internal const string PsfExtension = ".psf";
    internal static IReadOnlyList<string> ContentExtensions { get; } = [ExeExtension, PsexeExtension, CueExtension, BinExtension, ImgExtension, IsoExtension, ChdExtension, PbpExtension, EcmExtension, MdsExtension, PsfExtension];
}
