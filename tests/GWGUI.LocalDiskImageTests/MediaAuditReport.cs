namespace GWGUI.MediaAudit;

public sealed record MediaAuditReport(
    int SchemaVersion,
    DateTimeOffset AuditedAt,
    SourceAudit Source,
    RecognitionAudit Recognition,
    MediaAudit Media,
    IReadOnlyList<VolumeAudit> Volumes,
    VisualizationAudit Visualization,
    SupportAudit Support,
    object Representation,
    ValidationAudit Validation);

public sealed record SourceAudit(
    string Path,
    string FileName,
    string Extension,
    long Length,
    DateTimeOffset Created,
    DateTimeOffset Modified,
    string Sha256,
    string? ContentStartHex,
    string? ContentEndHex,
    string? ExpectedFormatHint,
    IReadOnlyList<AssociatedSourceFileAudit> AssociatedFiles);

public sealed record AssociatedSourceFileAudit(string Path, string Extension, long Length, string Sha256);

public sealed record RecognitionAudit(
    bool Readable,
    string Reader,
    string FormatId,
    string MediaKind,
    string RepresentationKind,
    IReadOnlyDictionary<string, string> Metadata,
    IReadOnlyList<string> Diagnostics,
    IReadOnlyDictionary<string, string> RejectedCandidates);

public sealed record MediaAudit(
    long? LogicalLength,
    bool SupportsRandomAccess,
    bool SupportsSequentialAccess,
    long? DeclaredCapacity,
    long? UsedBytes,
    long? FreeBytes);

public sealed record VolumeAudit(
    long Start,
    long Length,
    string Origin,
    string? PartitionScheme,
    int? PartitionNumber,
    int? SessionNumber,
    int? TrackNumber,
    string? PartitionType,
    string? PartitionId,
    string? Name,
    string? FileSystemReader,
    string? FileSystemId,
    long? Capacity,
    long? FreeBytes,
    bool? FreeSpaceKnown,
    bool? Bootable,
    DateTimeOffset? Created,
    DateTimeOffset? Modified,
    IReadOnlyList<string> Attributes,
    IReadOnlyList<string> Diagnostics,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<FileEntryAudit> Entries);

public sealed record FileEntryAudit(
    string Name,
    string DisplayName,
    string Kind,
    string Type,
    string Category,
    string ContentFormat,
    string TextEncoding,
    string ExecutionKind,
    string Preview,
    long Size,
    long? OccupiedSize,
    DateTimeOffset? Created,
    DateTimeOffset? Modified,
    DateTimeOffset? Accessed,
    string Comment,
    uint RawAttributes,
    int StorageReference,
    bool MetadataValid,
    bool? DataValid,
    bool SyntheticName,
    string? NativeTypeId,
    string? LinkTarget,
    IReadOnlyList<string> Attributes,
    IReadOnlyList<string> Diagnostics,
    IReadOnlyDictionary<string, string> Metadata,
    string? ContentSha256,
    string? ContentStartHex,
    string? ContentEndHex,
    string? ContentHex,
    bool ContentExtracted,
    IReadOnlyList<FileEntryAudit> Children);

public sealed record VisualizationAudit(
    bool Recognized,
    string RepresentationKind,
    string ProgressUnit,
    string Direction,
    IReadOnlyList<int> Surfaces,
    IReadOnlyList<int>? Layers,
    int ElementCount);

public sealed record SupportAudit(
    bool ExplorerRecognized,
    bool VisualizerRecognized,
    bool? GreaseweazlePhysicalRead,
    bool GreaseweazlePhysicalWritePlan,
    string? GreaseweazleWriteDiagnostic,
    string? FormatFamily,
    string? FormFactor,
    string? Density,
    IReadOnlyList<string> DeclaredExtensions,
    IReadOnlyList<ConversionAudit> Conversions);

public sealed record ConversionAudit(string FormatId, string Extension, string Writer, bool MultipleFiles);

public sealed record ValidationAudit(bool Passed, IReadOnlyList<string> Errors, IReadOnlyList<string> Warnings);
