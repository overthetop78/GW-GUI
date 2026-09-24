using System.Globalization;

namespace Hst.Amiga.FileSystems.Pfs3;

internal static class Pfs3ErrorMessages
{
    internal const string CachedChainBitmapTemplate =
        "Allocation: Cached block nr {0} in chnode allocate returns null as bitmap block, type is '{1}'";
    internal const string CachedBitmapTemplate =
        "Allocation: Cached block nr {0} in allocate returns null as bitmap block, type is '{1}'";
    internal const string BitmapBlockMissing = "Bitmap block is null";
    internal const string FreeBlockLoopTemplate =
        "Loop detected in 'FreeBlocksAC', chnode didn't change from chnode nr = {0}";
    internal const string AnodeMissingTemplate = "GetAnode: ERR: anode = {0}";
    internal const string AnodeBlockAllocationFailed = "AFS_ERROR_DNV_ALLOC_BLOCK";
    internal const string DiskFull = "ERROR_DISK_FULL";
    internal const string NoFreeStore = "ERROR_NO_FREE_STORE";
    internal const string SeekError = "ERROR_SEEK_ERROR";
    internal const string AnodeOperationFailed = "AFS_ERROR_ANODE_ERROR";
    internal const string AnodeBitmapIndexBlockMissingTemplate =
        "MakeAnodeBitmap: ERR: GetIndexBlock returned NULL!. {0} {1}\n";
    internal const string NullBlockType = "null";

    internal static string CachedChainBitmap(uint blockNumber, object? block) =>
        string.Format(CultureInfo.InvariantCulture, CachedChainBitmapTemplate,
            blockNumber, block?.GetType().Name ?? NullBlockType);

    internal static string CachedBitmap(uint blockNumber, object? block) =>
        string.Format(CultureInfo.InvariantCulture, CachedBitmapTemplate,
            blockNumber, block?.GetType().Name ?? NullBlockType);

    internal static string FreeBlockLoop(uint anodeNumber) =>
        string.Format(CultureInfo.InvariantCulture, FreeBlockLoopTemplate, anodeNumber);

    internal static string AnodeMissing(uint anodeNumber) =>
        string.Format(CultureInfo.InvariantCulture, AnodeMissingTemplate, anodeNumber);

    internal static string AnodeBitmapIndexBlockMissing(int sequence, int index) =>
        string.Format(CultureInfo.InvariantCulture, AnodeBitmapIndexBlockMissingTemplate, sequence, index);
}
