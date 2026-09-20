using ConversionOutput = global::GWGUI.MediaEngine.Conversion.ConversionOutput;
using GWGUI.Infrastructure.Settings.Hardware;
using GWGUI.App.Contracts.Services.Dialogs;
using GWGUI.App.Enums.Services.Dialogs;

namespace GWGUI.App.Interfaces.Services.Dialogs;

public interface IBusinessDialogService
{
    string? PromptProfileName(string? initialName = null);
    ReadConflictChoice? ResolveReadConflict(string outputPath);
    IReadOnlyList<ConversionConflictDecision>? ResolveConversionConflicts(IReadOnlyList<ConversionOutput> outputs);
    MissingHardwareChoice ResolveMissingHardware(IReadOnlyList<ControllerSettings> controllers);
}
