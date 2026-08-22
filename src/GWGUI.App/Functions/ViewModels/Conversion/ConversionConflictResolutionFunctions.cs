using GWGUI.Domain.Conversion;
using GWGUI.App.Contracts.Services.Dialogs;
using GWGUI.App.Views.Dialogs.Conversion;

namespace GWGUI.App.Functions.ViewModels.Conversion;

public static class ConversionConflictResolutionFunctions
{
    public static IReadOnlyList<ConversionOutput> Apply(
        IReadOnlyList<ConversionOutput> outputs,
        IReadOnlyList<ConversionOutput> conflicts,
        IReadOnlyList<ConversionConflictDecision> decisions,
        Func<string, string> numberedPath)
    {
        ArgumentNullException.ThrowIfNull(numberedPath);
        var result = outputs.Except(conflicts).ToList();
        foreach (var decision in decisions)
        {
            if (!conflicts.Contains(decision.Output))
                throw new ArgumentException("A conflict decision does not belong to the supplied conflicts.", nameof(decisions));
            if (decision.Choice == ConversionConflictChoice.Skip) continue;
            result.Add(decision.Choice == ConversionConflictChoice.Number
                ? decision.Output with { OutputPath = numberedPath(decision.Output.OutputPath) }
                : decision.Output);
        }
        return result;
    }
}
