using GWGUI.MediaEngine.Constants;

namespace GWGUI.Emulation.Sony.Common.Machines.Common.Constants;

internal static class StorageSettingsFunctionsConstants
{
    internal const string CompactDiscDriveEnabledOption = "storage.compactDiscDriveEnabled";
    internal const string CompactDiscDriveLabel = "Optical disc";

    internal static readonly IReadOnlyList<string> PlayStationExtensions =
        [DiskImageFileExtensions.Cue, DiskImageFileExtensions.Bin,
         DiskImageFileExtensions.Chd, DiskImageFileExtensions.Iso,
         DiskImageFileExtensions.Ccd, DiskImageFileExtensions.Mds];

    internal static readonly IReadOnlyList<string> PspExtensions =
        [DiskImageFileExtensions.Iso, DiskImageFileExtensions.Chd];
}
