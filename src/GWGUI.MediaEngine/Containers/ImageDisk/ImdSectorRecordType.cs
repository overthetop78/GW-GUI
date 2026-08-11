namespace GWGUI.MediaEngine.Containers.ImageDisk;

public enum ImdSectorRecordType : byte
{
    Unavailable = 0,
    Normal = 1,
    Compressed = 2,
    Deleted = 3,
    CompressedDeleted = 4,
    NormalWithError = 5,
    CompressedWithError = 6,
    DeletedWithError = 7,
    CompressedDeletedWithError = 8
}

internal static class ImdSectorRecordTypeExtensions
{
    public static bool HasData(this ImdSectorRecordType type) => type != ImdSectorRecordType.Unavailable;
    public static bool IsCompressed(this ImdSectorRecordType type) => type is ImdSectorRecordType.Compressed or ImdSectorRecordType.CompressedDeleted or ImdSectorRecordType.CompressedWithError or ImdSectorRecordType.CompressedDeletedWithError;
    public static bool IsIntegrityValid(this ImdSectorRecordType type) => type is ImdSectorRecordType.Normal or ImdSectorRecordType.Compressed or ImdSectorRecordType.Deleted or ImdSectorRecordType.CompressedDeleted;
}
