namespace GWGUI.MediaFileSystems.Migration;

public sealed record MigrationLoss(MigrationLossKind Kind, string Path, bool IsBlocking, string Detail);
