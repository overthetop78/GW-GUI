namespace GWGUI.MediaFileSystems.Migration;

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
