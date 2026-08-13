namespace GWGUI.MediaEngine.Migration;

public sealed record MigrationTargetCapabilities(
    string FileSystemId,
    int MaximumNameLength,
    long MaximumFileSize,
    bool SupportsDirectories,
    bool SupportsLinks,
    bool SupportsModifiedDate,
    bool SupportsComments,
    bool SupportsRawAttributes,
    bool IsCaseSensitive,
    string ForbiddenNameCharacters,
    bool AllowsControlCharacters = false,
    IMigrationNamePolicy? NamePolicy = null,
    int MaximumVolumeNameLength = int.MaxValue,
    IMigrationNamePolicy? VolumeNamePolicy = null);
