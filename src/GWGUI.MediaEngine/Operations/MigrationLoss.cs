namespace GWGUI.MediaEngine.Operations;

public sealed record MigrationLoss(MigrationLossKind Kind, string Path, bool IsBlocking, string Detail);
