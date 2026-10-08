namespace GWGUI.Emulation.Sony.Emulators.Ppsspp.Constants;

internal static class PpssppConstants
{
    internal const string Id = "ppsspp";
    internal const string DisplayName = "PPSSPP";
    internal const string LibraryName = "PPSSPP";
    internal const string DescriptionResourceKey = "Emulation.Emulator.ppsspp.Description";
    internal const string CoreHostCommand = "--sony-ppsspp-core-host";
    internal const string ElfExtension = ".elf";
    internal const string IsoExtension = ".iso";
    internal const string CsoExtension = ".cso";
    internal const string PrxExtension = ".prx";
    internal const string PbpExtension = ".pbp";
    internal const string ChdExtension = ".chd";
    internal static IReadOnlyList<string> ContentExtensions { get; } =
        [ElfExtension, IsoExtension, CsoExtension, PrxExtension, PbpExtension, ChdExtension];
}
