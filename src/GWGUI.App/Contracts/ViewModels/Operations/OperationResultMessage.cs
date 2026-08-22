namespace GWGUI.App.Contracts.ViewModels.Operations;

public sealed record OperationResultMessage(
    string ResourceKey,
    IReadOnlyList<object> Arguments,
    bool StartOnNewLine = false);
