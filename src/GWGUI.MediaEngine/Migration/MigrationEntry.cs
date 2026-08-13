using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.MediaEngine.Migration;

public sealed record MigrationEntry
{
    public MigrationEntry(string sourcePath, string targetName, FileSystemEntryKind kind, IReadOnlyList<byte>? content, DateTimeOffset? modified, string comment, uint rawAttributes, bool metadataValid, IEnumerable<MigrationEntry> children)
    {
        SourcePath = sourcePath;
        TargetName = targetName;
        Kind = kind;
        Content = content is null ? null : Array.AsReadOnly(content.ToArray());
        Modified = modified;
        Comment = comment;
        RawAttributes = rawAttributes;
        MetadataValid = metadataValid;
        Children = Array.AsReadOnly(children.ToArray());
    }

    public string SourcePath { get; }
    public string TargetName { get; }
    public FileSystemEntryKind Kind { get; }
    public IReadOnlyList<byte>? Content { get; }
    public DateTimeOffset? Modified { get; }
    public string Comment { get; }
    public uint RawAttributes { get; }
    public bool MetadataValid { get; }
    public IReadOnlyList<MigrationEntry> Children { get; }
    public long Size => Content?.Count ?? 0;
}
