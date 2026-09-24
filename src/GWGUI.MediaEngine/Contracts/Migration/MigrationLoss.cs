namespace GWGUI.MediaEngine.Contracts.Migration;

public sealed record MigrationLoss(MigrationLossKind Kind, string Path, bool IsBlocking, string Detail);
