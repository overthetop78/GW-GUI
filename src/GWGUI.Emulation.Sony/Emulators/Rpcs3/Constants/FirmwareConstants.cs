namespace GWGUI.Emulation.Sony.Emulators.Rpcs3.Constants;

internal static class FirmwareConstants
{
    internal const string SystemField = "firmware.system";
    internal const string SystemFile = "rpcs3/PS3UPDAT.PUP";
    internal const string SystemLabelResource = "Emulation.Sony.Firmware.Rpcs3.System";
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [new(SystemField, SystemFile, SystemLabelResource, true, SystemFile)];
    internal const string BinExtension = ".bin";
    internal const string SelfExtension = ".self";
    internal const string ElfExtension = ".elf";
    internal const string PkgExtension = ".pkg";
    internal const string IsoExtension = ".iso";
    internal const string SfoExtension = ".sfo";
    internal const string SfbExtension = ".sfb";
    internal static IReadOnlyList<string> ContentExtensions { get; } =
    [BinExtension, SelfExtension, ElfExtension, PkgExtension, IsoExtension, SfoExtension, SfbExtension];
}
