namespace GWGUI.MediaEngine.Migration;

public sealed record MigrationPlan
{
    public MigrationPlan(string sourceFileSystemId, string targetFileSystemId, string volumeName, IEnumerable<MigrationEntry> entries)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceFileSystemId);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetFileSystemId);
        SourceFileSystemId = sourceFileSystemId;
        TargetFileSystemId = targetFileSystemId;
        VolumeName = volumeName;
        Entries = Array.AsReadOnly(entries.ToArray());
    }

    public string SourceFileSystemId { get; }
    public string TargetFileSystemId { get; }
    public string VolumeName { get; }
    public IReadOnlyList<MigrationEntry> Entries { get; }
}
