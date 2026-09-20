namespace GWGUI.MediaEngine.Operations;

public enum MigrationLossKind
{
    MissingContent,
    UnsupportedEntryKind,
    InvalidName,
    NameTooLong,
    NameCollision,
    FileTooLarge,
    InvalidMetadata,
    ModifiedDate,
    Comment,
    Attributes
}
