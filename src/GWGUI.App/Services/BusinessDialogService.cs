using System.Windows;
using GWGUI.Domain.Conversion;

namespace GWGUI.App.Services;

public sealed record ConversionConflictDecision(ConversionOutput Output, ConversionConflictChoice Choice);
public enum ReadConflictChoice { Overwrite, UseNextNumber, EditName }

public interface IBusinessDialogService
{
    string? PromptProfileName(string? initialName = null);
    ReadConflictChoice? ResolveReadConflict(string outputPath);
    IReadOnlyList<ConversionConflictDecision>? ResolveConversionConflicts(IReadOnlyList<ConversionOutput> outputs);
}

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
}
