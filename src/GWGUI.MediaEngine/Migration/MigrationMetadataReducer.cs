namespace GWGUI.MediaEngine.Migration;

/// <summary>Retire uniquement les métadonnées dont la perte a déjà été validée.</summary>
internal static class MigrationMetadataReducer
{
    /// <summary>Crée un plan écrivable sans réinterpréter les attributs propres au système source.</summary>
    public static MigrationPlan Reduce(MigrationPlan plan, MigrationTargetCapabilities target) => new(plan.SourceFileSystemId, plan.TargetFileSystemId, plan.VolumeName, plan.Entries.Select(entry => Reduce(entry, target)));

    private static MigrationEntry Reduce(MigrationEntry entry, MigrationTargetCapabilities target) => new(entry.SourcePath, entry.TargetName, entry.Kind, entry.Content, target.SupportsModifiedDate ? entry.Modified : null, target.SupportsComments ? entry.Comment : string.Empty, target.SupportsRawAttributes ? entry.RawAttributes : 0, entry.MetadataValid, entry.Children.Select(child => Reduce(child, target)));
}
