namespace GWGUI.Emulation.Sony.Emulators.Play.Constants;

internal static class FirmwareConstants
{
    internal static IReadOnlyList<FirmwareSlot> All { get; } = [];
    internal const string ElfExtension = ".elf";
    internal const string IsoExtension = ".iso";
    internal const string CsoExtension = ".cso";
    internal const string IszExtension = ".isz";
    internal const string CueExtension = ".cue";
    internal const string ChdExtension = ".chd";
    internal static IReadOnlyList<string> ContentExtensions { get; } =
    [ElfExtension, IsoExtension, CsoExtension, IszExtension, CueExtension, ChdExtension];
}
