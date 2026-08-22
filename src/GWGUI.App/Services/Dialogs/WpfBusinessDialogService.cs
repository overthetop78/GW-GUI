using GWGUI.Domain.Conversion;
using GWGUI.Domain.Settings.Hardware;
using GWGUI.App.Contracts.Services.Dialogs;
using GWGUI.App.Enums.Services.Dialogs;
using GWGUI.App.Interfaces.Services.Dialogs;
using GWGUI.App.Views.Dialogs.Conversion;
using GWGUI.App.Views.Dialogs.Hardware;
using GWGUI.App.Views.Dialogs.Profiles;
using GWGUI.App.Views.Dialogs.Read;
using System.Windows;

namespace GWGUI.App.Services.Dialogs;

public sealed class WpfBusinessDialogService(Window owner) : IBusinessDialogService
{
    public string? PromptProfileName(string? initialName = null)
    {
        var dialog = new ProfileNameWindow(initialName) { Owner = owner };
        return dialog.ShowDialog() == true ? dialog.ProfileName : null;
    }

    public ReadConflictChoice? ResolveReadConflict(string outputPath)
    {
        var dialog = new ReadConflictWindow(outputPath) { Owner = owner };
        return dialog.ShowDialog() == true ? dialog.Choice : null;
    }

    public IReadOnlyList<ConversionConflictDecision>? ResolveConversionConflicts(IReadOnlyList<ConversionOutput> outputs)
    {
        var dialog = new ConversionConflictWindow(outputs) { Owner = owner };
        return dialog.ShowDialog() == true
            ? dialog.Rows.Select(row => new ConversionConflictDecision(row.Output, row.Choice)).ToArray()
            : null;
    }

    public MissingHardwareChoice ResolveMissingHardware(IReadOnlyList<ControllerSettings> controllers)
    {
        var dialog = new HardwareUnavailableWindow(controllers) { Owner = owner };
        return dialog.ShowDialog() == true ? dialog.Choice : MissingHardwareChoice.Continue;
    }
}
