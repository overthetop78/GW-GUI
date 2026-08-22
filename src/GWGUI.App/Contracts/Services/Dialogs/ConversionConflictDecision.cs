using GWGUI.Domain.Conversion;
using GWGUI.App.Views.Dialogs.Conversion;

namespace GWGUI.App.Contracts.Services.Dialogs;

public sealed record ConversionConflictDecision(ConversionOutput Output, ConversionConflictChoice Choice);
