namespace Hst.Amiga.FileSystems;

public enum FileSystemErrorCode
{
    ReadFailed,
    CountExceedsBuffer,
    IncorrectDataBlockSequence,
    NoFreeSectorAvailable,
    OnlyOffsetZeroSupported,
    SetLengthUnsupported,
    CachedChainBitmapMissing,
    CachedBitmapMissing,
    BitmapBlockMissing,
    FreeBlockLoop,
    AnodeMissing,
    AnodeBlockAllocationFailed,
    SeekError,
    AnodeOperationFailed,
    AnodeBitmapIndexBlockMissing
}
