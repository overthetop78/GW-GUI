using GWGUI.App.Enums.ViewModels.Operations;
namespace GWGUI.App.Contracts.ViewModels.Operations;

public sealed record OperationResultPresentation(
    OperationResultState State,
    IReadOnlyList<OperationResultMessage> Messages);
