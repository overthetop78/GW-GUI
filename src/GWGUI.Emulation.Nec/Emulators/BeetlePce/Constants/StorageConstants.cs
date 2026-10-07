namespace GWGUI.Emulation.Nec.Emulators.BeetlePce.Constants;

internal static class StorageConstants
{
    internal static IReadOnlyList<string> CartridgeExtensions { get; } = [".pce", ".sgx"];
    internal static IReadOnlyList<string> CompactDiscExtensions { get; } = [".cue", ".ccd", ".chd", ".toc"];
    internal const string CartridgeLabel = "HuCARD";
    internal const string CompactDiscLabel = "CD-ROM";
}
