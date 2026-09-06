using GWGUI.App.Views.Dialogs.Conversion;
using GWGUI.Domain.Commands.Execution;
using System.Windows.Threading;
namespace GWGUI.Tests.Interface.ConversionViews;
internal static class ConversionConflictScenarios
{
    public static async Task Decision(int decision)
    {
        ConversionConflictChoice? choice = decision < 0 ? null : (ConversionConflictChoice)decision;
        var context = new ConversionOperationScenarios.Context(true, choice);
        await Dispatcher.Yield(DispatcherPriority.ContextIdle);
        context.Pending.SetResult(new GwExecutionResult(0, false, TimeSpan.Zero, []));
        await context.Controller.ExecuteAsync();
        Assert.Equal(1, context.ConflictPrompts); Assert.False(context.Operation.IsRunning);
        Assert.Equal("disk", context.Model.Conversion.OutputName);
        if (choice is null or ConversionConflictChoice.Skip) Assert.Empty(context.Commands);
        else
        {
            Assert.Equal(2, context.Commands.Count);
            Assert.Equal(new[] { choice == ConversionConflictChoice.Number ? "disk (2).ima" : "disk.ima", choice == ConversionConflictChoice.Number ? "disk (2).img" : "disk.img" }, context.Commands.Select(command => Path.GetFileName(command.Arguments.Last())));
        }
    }
}
