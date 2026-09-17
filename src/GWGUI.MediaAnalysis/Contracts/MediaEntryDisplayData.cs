using GWGUI.MediaAnalysis.Enums;
using System.Collections.ObjectModel;

namespace GWGUI.MediaAnalysis.Contracts;

public sealed record MediaEntryDisplayData
{
    public MediaEntryDisplayData(
        string name,
        MediaEntryKind kind,
        long size,
        DateTimeOffset? modified,
        string comment,
        uint rawAttributes,
        int storageReference,
        bool metadataValid,
        IEnumerable<MediaEntryDisplayData> children,
        MediaContentTypeDefinition type,
        IEnumerable<byte>? content = null,
        string? nativeTypeId = null,
        long? occupiedSize = null,
        DateTimeOffset? created = null,
        DateTimeOffset? accessed = null,
        IEnumerable<string>? attributes = null,
        bool? dataValid = null,
        bool syntheticName = false,
        string? linkTarget = null,
        IEnumerable<string>? diagnostics = null,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        Name = name;
        Kind = kind;
        Size = size;
        Modified = modified;
        Comment = comment;
        RawAttributes = rawAttributes;
        StorageReference = storageReference;
        MetadataValid = metadataValid;
        Children = Array.AsReadOnly(children.ToArray());
        Type = type;
        Content = content is null ? null : Array.AsReadOnly(content.ToArray());
        NativeTypeId = nativeTypeId;
        OccupiedSize = occupiedSize;
        Created = created;
        Accessed = accessed;
        Attributes = Array.AsReadOnly((attributes ?? []).ToArray());
        DataValid = dataValid;
        SyntheticName = syntheticName;
        LinkTarget = linkTarget;
        Diagnostics = Array.AsReadOnly((diagnostics ?? []).ToArray());
        Metadata = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(metadata ?? new Dictionary<string, string>(), StringComparer.Ordinal));
    }

    public string Name { get; }
    public MediaEntryKind Kind { get; }
    public long Size { get; }
    public DateTimeOffset? Modified { get; }
    public string Comment { get; }
    public uint RawAttributes { get; }
    public int StorageReference { get; }
    public bool MetadataValid { get; }
    public IReadOnlyList<MediaEntryDisplayData> Children { get; }
    public MediaContentTypeDefinition Type { get; }
    public IReadOnlyList<byte>? Content { get; }
    public string? NativeTypeId { get; }
    public long? OccupiedSize { get; }
    public DateTimeOffset? Created { get; }
    public DateTimeOffset? Accessed { get; }
    public IReadOnlyList<string> Attributes { get; }
    public bool? DataValid { get; }
    public bool SyntheticName { get; }
    public string? LinkTarget { get; }
    public IReadOnlyList<string> Diagnostics { get; }
    public IReadOnlyDictionary<string, string> Metadata { get; }
}
