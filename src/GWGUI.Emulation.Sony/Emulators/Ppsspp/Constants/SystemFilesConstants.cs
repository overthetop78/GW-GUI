namespace GWGUI.Emulation.Sony.Emulators.Ppsspp.Constants;

internal static class SystemFilesConstants
{
    internal const string PackageUrl = "https://buildbot.libretro.com/assets/system/PPSSPP.zip";
    internal const string DirectoryName = "PPSSPP";
    internal const string StagingPrefix = ".ppsspp-system-";
    internal const string PreviousSuffix = ".previous";
    internal static IReadOnlyList<string> RequiredFiles { get; } =
    ["compat.ini", "ppge_atlas.meta", "ppge_atlas.zim", "flash0/font/ltn0.pgf", "lang/en_US.ini"];
}
