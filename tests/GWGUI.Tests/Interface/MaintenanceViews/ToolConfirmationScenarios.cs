namespace GWGUI.Tests.Interface.MaintenanceViews;
internal static class ToolConfirmationScenarios
{
    public static async Task Stop(bool clean)
    {
        var context = new ToolOperationScenarios.Context(); var running = context.Execute(clean); await context.WaitUntilStarted(running);
        await context.Execute(clean); Assert.Equal(1, context.StopPrompts); Assert.False(context.Token.IsCancellationRequested); Assert.Single(context.Commands);
        context.AcceptStop = true; await context.Execute(clean); Assert.Equal(2, context.StopPrompts); Assert.True(context.Token.IsCancellationRequested); Assert.Single(context.Commands);
        context.Pending.SetResult(new(0, true, TimeSpan.Zero, [])); await running;
        Assert.Equal("Status.Cancelled", context.Model.OperationText); Assert.False(context.Operation.IsRunning); Assert.Equal("Common.Execute", context.Button(clean).Content);
    }
}
