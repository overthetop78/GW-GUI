using ConversionOutput = global::GWGUI.MediaEngine.Conversion.ConversionOutput;
using GWGUI.App.Views.Dialogs.Conversion;

namespace GWGUI.App.Contracts.Services.Dialogs;

public sealed record ConversionConflictDecision(ConversionOutput Output, ConversionConflictChoice Choice);
