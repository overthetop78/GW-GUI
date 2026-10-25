using MachineFirmware = GWGUI.Emulation.Sony.Common.Machines.PlayStation.Constants.FirmwareConstants;

namespace GWGUI.Emulation.Sony.Emulators.BeetlePsx.Constants;

internal static class FirmwareConstants
{
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        MachineFirmware.Japan,
        MachineFirmware.NorthAmerica,
        MachineFirmware.Europe,
        MachineFirmware.Psp,
        MachineFirmware.Ps3,
        MachineFirmware.OpenBios,
    ];
    internal const string ExeExtension = ".exe";
    internal const string CueExtension = ".cue";
    internal const string TocExtension = ".toc";
    internal const string CcdExtension = ".ccd";
    internal const string PbpExtension = ".pbp";
    internal const string ChdExtension = ".chd";
    internal const string BinExtension = ".bin";
    internal const string ImgExtension = ".img";
    internal const string IsoExtension = ".iso";
    internal static IReadOnlyList<string> ContentExtensions { get; } =
    [ExeExtension, CueExtension, TocExtension, CcdExtension, PbpExtension, ChdExtension, BinExtension, ImgExtension, IsoExtension];
}
