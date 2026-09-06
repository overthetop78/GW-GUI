using GWGUI.App.ViewModels.Operations;
namespace GWGUI.Tests.Interface.ReadViews;
internal static class ReadDestinationScenarios
{
    public static async Task Preview(bool alphabetic)
    {
        using var context = new ReadOperationScenarios.Context();
        context.Model.Read.FileName = "  sample {name}  "; context.Model.Read.Folder = "virtual destination";
        context.Model.Read.SequenceWidthIndex = 2; context.Model.Read.SequenceKindIndex = alphabetic ? 1 : 0;
        context.Model.Read.SequenceValue = alphabetic ? "AB" : "7";
        await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.ContextIdle);
        var expected = "sample {name} " + (alphabetic ? "AAB" : "007") + ".img";
        context.Controller.UpdateCommand();
        Assert.Equal(expected, context.View.AdvancedBlock.NamePreviewTextBlock.Text);
        var running = context.Controller.ExecuteAsync();
        await Task.WhenAny(context.Started.Task, running); Assert.True(context.Started.Task.IsCompleted);
        Assert.Equal(Path.Combine("virtual destination", expected), context.Command!.Arguments.Last());
        context.Pending.SetResult(new(0, false, TimeSpan.Zero, [])); await running;
    }
    public static async Task MissingName()
    {
        using var context = new ReadOperationScenarios.Context();
        context.Model.Read.FileName = " ";
        await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.ContextIdle);
        await context.Controller.ExecuteAsync();
        Assert.Single(context.ValidationMessages); Assert.Equal(0, context.Calls);
        Assert.False(context.Operation.IsRunning); Assert.Equal("virtual-folder", context.Model.Read.Folder);
    }
    public static async Task Conflict(int choice)
    {
        using var context = new ReadOperationScenarios.Context();
        context.Model.Read.SequenceWidthIndex = 0;
        context.ConflictChoice = choice < 0 ? null : (GWGUI.App.Enums.Services.Dialogs.ReadConflictChoice)choice;
        var first = Path.Combine("virtual-folder", "disk 1.img");
        context.ExistingPaths.Add(first);
        context.ExistingPaths.Add(Path.Combine("virtual-folder", "disk 2.img"));
        await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.ContextIdle);
        var running = context.Controller.ExecuteAsync();
        if (choice is -1 or 2) {
            await running; Assert.Equal(0, context.Calls); Assert.Equal(1, context.NameSelections);
            Assert.Equal("1", context.Model.Read.SequenceValue);
        } else {
            await Task.WhenAny(context.Started.Task, running); Assert.True(context.Started.Task.IsCompleted);
            Assert.Equal(choice == 1 ? Path.Combine("virtual-folder", "disk 3.img") : first, context.Command!.Arguments.Last());
            context.Pending.SetResult(new(0, false, TimeSpan.Zero, [])); await running;
            Assert.Equal(choice == 1 ? "4" : "2", context.Model.Read.SequenceValue);
            Assert.Equal(0, context.NameSelections);
        }
        Assert.Equal(first, Assert.Single(context.Conflicts));
        Assert.False(context.Operation.IsRunning); Assert.Empty(context.Errors);
    }
    public static void Browse(string? response)
    {
        var context=ReadFormatScenarios.CreateSelectionContext(response);
        context.Model.Read.Folder="virtual-old";
        context.Controller.BrowseFolder();
        Assert.Equal(response??"virtual-old",context.Model.Read.Folder);
    }
    public static void Target()
    {
        var model=new ReadOperationViewModel { Folder="virtual",FileName=" disk ",AutoNumber=true,SequenceWidthIndex=2,SequenceValue="7" };
        Assert.Equal(Path.Combine("virtual","disk 007.scp"),model.BuildTarget(".scp","fallback"));
        Assert.True(model.TryAdvanceSequence());
        Assert.Equal("8",model.SequenceValue);
        model.SequenceKindIndex=1;
        model.SequenceWidthIndex=0;
        model.SequenceValue="Z";
        Assert.True(model.TryAdvanceSequence());
        Assert.Equal("AA",model.SequenceValue);
        model.FileName="";
        model.AutoNumber=false;
        Assert.Equal(Path.Combine("virtual","fallback.scp"),model.BuildTarget(".scp","fallback"));
        Assert.False(model.TryAdvanceSequence());
        model.AutoNumber=true;
        model.SequenceValue="!";
        Assert.False(model.TryAdvanceSequence());
    }
}
