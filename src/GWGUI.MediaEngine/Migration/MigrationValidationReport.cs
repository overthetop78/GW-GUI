namespace GWGUI.MediaEngine.Migration;

public sealed record MigrationValidationReport
{
    public MigrationValidationReport(IEnumerable<MigrationLoss> losses, bool metadataLossAccepted)
    {
        Losses = Array.AsReadOnly(losses.ToArray());
        MetadataLossAccepted = metadataLossAccepted;
    }

    public IReadOnlyList<MigrationLoss> Losses { get; }
    public bool MetadataLossAccepted { get; }
    public bool CanExecute => Losses.All(loss => !loss.IsBlocking) && (MetadataLossAccepted || Losses.Count == 0);
}
