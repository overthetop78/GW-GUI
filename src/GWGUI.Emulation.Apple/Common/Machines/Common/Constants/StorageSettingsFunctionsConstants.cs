namespace GWGUI.Emulation.Apple.Common.Machines.Common.Constants;

internal static class StorageSettingsFunctionsConstants
{
    internal const string StorageDevice = "storage.device.";
    internal const string FirstDriveLabel = "D1:";
    internal const string SecondDriveLabel = "D2:";
    internal const string HardDiskLabel = "HD0:";
    internal const string ResourcePlaylist = "Emulation.Apple.Storage.Playlist";
    internal static readonly IReadOnlyList<string> AppleWinFloppyExtensions =
        [".bin", ".do", ".dsk", ".nib", ".po", ".gz", ".woz", ".zip", ".2mg", ".2img", ".iie", ".apl", ".m3u"];
    internal static readonly IReadOnlyList<string> AppleWinHardDiskExtensions = [".hdv", ".po", ".2mg", ".2img"];
    internal static readonly IReadOnlyList<string> MacintoshDiskExtensions = [".dsk", ".img", ".zip", ".hvf", ".cmd"];
}
