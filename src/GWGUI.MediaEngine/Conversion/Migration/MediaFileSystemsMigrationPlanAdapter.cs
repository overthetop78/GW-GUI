using GWGUI.MediaEngine.FileSystems;
using FileSystemsEntryKind = GWGUI.MediaFileSystems.FileSystemEntryKind;
using FileSystemsMigrationEntry = GWGUI.MediaFileSystems.Migration.MigrationEntry;
using FileSystemsMigrationPlan = GWGUI.MediaFileSystems.Migration.MigrationPlan;

namespace GWGUI.MediaEngine.Conversion.Migration;

/// <summary>Transmet un plan de migration validé aux writers de MediaFileSystems.</summary>
internal static class MediaFileSystemsMigrationPlanAdapter
{
    public static FileSystemsMigrationPlan Convert(MigrationPlan plan) => new(
        plan.SourceFileSystemId,
        plan.TargetFileSystemId,
        plan.VolumeName,
        plan.Entries.Select(ConvertEntry));

    private static FileSystemsMigrationEntry ConvertEntry(MigrationEntry entry) => new(
        entry.SourcePath,
        entry.TargetName,
        entry.Kind switch
        {
            FileSystemEntryKind.Directory => FileSystemsEntryKind.Directory,
            FileSystemEntryKind.File => FileSystemsEntryKind.File,
            FileSystemEntryKind.Link => FileSystemsEntryKind.Link,
            _ => FileSystemsEntryKind.Unknown
        },
        entry.Content,
        entry.Modified,
        entry.Comment,
        entry.RawAttributes,
        entry.MetadataValid,
        entry.Children.Select(ConvertEntry));
}
