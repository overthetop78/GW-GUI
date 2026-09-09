namespace GWGUI.Updates.Contracts;

public sealed record UpdateExecutionPlan(
    int SchemaVersion,
    string TransactionId,
    string InstallationDirectory,
    string ApplicationExecutable,
    IReadOnlyList<int> ProcessIds,
    int ShutdownTimeoutSeconds,
    int StartupTimeoutSeconds,
    string StartupSignalPath,
    string ResultPath,
    IReadOnlyList<PreparedUpdateComponent> Components);

public sealed record PreparedUpdateComponent(
    string ComponentId,
    UpdateComponentKind Kind,
    UpdateComponentOperation Operation,
    string Version,
    string PreparedDirectory);

public enum UpdateComponentOperation
{
    UpdateApplication,
    InstallModule,
    UpdateModule
}

public enum UpdateTransactionStatus
{
    Succeeded,
    FailedBeforeReplacement,
    Restored
}

public sealed record UpdateTransactionResult(
    int SchemaVersion,
    string TransactionId,
    UpdateTransactionStatus Status,
    string? Detail = null);
