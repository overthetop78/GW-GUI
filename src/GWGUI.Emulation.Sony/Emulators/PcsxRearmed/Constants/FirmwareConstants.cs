using MachineFirmware = GWGUI.Emulation.Sony.Common.Machines.PlayStation.Constants.FirmwareConstants;

namespace GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Constants;

internal static class FirmwareConstants
{
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        MachineFirmware.Japan,
        MachineFirmware.NorthAmerica,
        MachineFirmware.Europe,
        MachineFirmware.Psp,
    ];
    internal const string BinExtension = ".bin";
    internal const string CueExtension = ".cue";
    internal const string ImgExtension = ".img";
    internal const string MdfExtension = ".mdf";
    internal const string PbpExtension = ".pbp";
    internal const string TocExtension = ".toc";
    internal const string CbnExtension = ".cbn";
    internal const string ChdExtension = ".chd";
    internal const string IsoExtension = ".iso";
    internal const string ExeExtension = ".exe";
    internal static IReadOnlyList<string> ContentExtensions { get; } =
    [BinExtension, CueExtension, ImgExtension, MdfExtension, PbpExtension, TocExtension, CbnExtension, ChdExtension, IsoExtension, ExeExtension];
}
