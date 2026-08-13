using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.MediaEngine.Migration;

public static class MigrationValidator
{
    public static MigrationValidationReport Validate(MigrationPlan plan, MigrationTargetCapabilities target, bool acceptMetadataLoss = false)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(target);
        if (!plan.TargetFileSystemId.Equals(target.FileSystemId, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("The target capabilities do not describe the migration target.", nameof(target));
        if (target.MaximumNameLength <= 0 || target.MaximumFileSize < 0) throw new ArgumentOutOfRangeException(nameof(target), "Target name and file-size limits must be valid.");
        var losses = new List<MigrationLoss>();
        ValidateEntries(plan.Entries, target, losses);
        return new(losses, acceptMetadataLoss);
    }

    public static void EnsureExecutable(MigrationValidationReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        if (!report.CanExecute) throw new InvalidOperationException("The migration contains unhandled incompatibilities or unaccepted metadata losses.");
    }

    private static void ValidateEntries(IReadOnlyList<MigrationEntry> entries, MigrationTargetCapabilities target, ICollection<MigrationLoss> losses)
    {
        var comparer = target.IsCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
        foreach (var collision in entries.GroupBy(entry => entry.TargetName, comparer).Where(group => group.Skip(1).Any()))
        {
            foreach (var entry in collision) losses.Add(new(MigrationLossKind.NameCollision, entry.SourcePath, true, entry.TargetName));
        }
        foreach (var entry in entries)
        {
            ValidateEntry(entry, target, losses);
            ValidateEntries(entry.Children, target, losses);
        }
    }

    private static void ValidateEntry(MigrationEntry entry, MigrationTargetCapabilities target, ICollection<MigrationLoss> losses)
    {
        if (entry.TargetName.Length == 0 || entry.TargetName.Any(character => target.ForbiddenNameCharacters.Contains(character) || (!target.AllowsControlCharacters && char.IsControl(character)))) losses.Add(new(MigrationLossKind.InvalidName, entry.SourcePath, true, entry.TargetName));
        if (entry.TargetName.Length > target.MaximumNameLength) losses.Add(new(MigrationLossKind.NameTooLong, entry.SourcePath, true, entry.TargetName.Length.ToString()));
        if (!entry.MetadataValid) losses.Add(new(MigrationLossKind.InvalidMetadata, entry.SourcePath, true, string.Empty));
        if (entry.Kind == FileSystemEntryKind.Directory && !target.SupportsDirectories) losses.Add(new(MigrationLossKind.UnsupportedEntryKind, entry.SourcePath, true, entry.Kind.ToString()));
        if (entry.Kind == FileSystemEntryKind.Link && !target.SupportsLinks) losses.Add(new(MigrationLossKind.UnsupportedEntryKind, entry.SourcePath, true, entry.Kind.ToString()));
        if (entry.Kind == FileSystemEntryKind.Unknown) losses.Add(new(MigrationLossKind.UnsupportedEntryKind, entry.SourcePath, true, entry.Kind.ToString()));
        if (entry.Kind == FileSystemEntryKind.File && entry.Content is null) losses.Add(new(MigrationLossKind.MissingContent, entry.SourcePath, true, string.Empty));
        if (entry.Kind == FileSystemEntryKind.File && entry.Size > target.MaximumFileSize) losses.Add(new(MigrationLossKind.FileTooLarge, entry.SourcePath, true, entry.Size.ToString()));
        if (entry.Modified is not null && !target.SupportsModifiedDate) losses.Add(new(MigrationLossKind.ModifiedDate, entry.SourcePath, false, entry.Modified.Value.ToString("O")));
        if (!string.IsNullOrEmpty(entry.Comment) && !target.SupportsComments) losses.Add(new(MigrationLossKind.Comment, entry.SourcePath, false, entry.Comment));
        if (entry.RawAttributes != 0 && !target.SupportsRawAttributes) losses.Add(new(MigrationLossKind.Attributes, entry.SourcePath, false, entry.RawAttributes.ToString()));
    }
}
