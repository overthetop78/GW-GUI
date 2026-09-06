using GWGUI.App.ViewModels.Operations;
namespace GWGUI.Tests.Interface.WriteViews;
internal static class WriteConfirmationScenarios
{
    public static async Task Request(bool verify)
    {
        var context = new WriteOperationScenarios.Context();
        var model = context.Model.Write;
        model.Tracks.Enabled = true; model.Tracks.Value = "c=2-4:h=1";
        model.Retries.Enabled = true; model.Retries.Value = "4";
        model.PreErase.Enabled = true; model.NoVerify.Enabled = !verify;
        model.ExpertArguments = "--raw";
        await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.ContextIdle);
        var command = context.Controller.BuildCommand();
        var expected = new List<string> { "--device", "controller", "--drive", "B", "--format", "test.one" };
        if (!verify) expected.Add("--no-verify");
        expected.AddRange(["--retries", "4", "--tracks", "c=2-4:h=1", "--pre-erase", "--raw", "virtual-source.img"]);
        Assert.Equal(expected, command.Arguments);
        model.Tracks.Enabled = false;
        Assert.DoesNotContain("--tracks", context.Controller.BuildCommand().Arguments);
        Assert.Equal("c=2-4:h=1", model.Tracks.Value);
        model.Retries.Value = "invalid";
        Assert.ThrowsAny<ArgumentException>(() => context.Controller.BuildCommand());
        Assert.Equal(0, context.Calls);
    }
    public static async Task Refusal()
    {
        var context = WriteOperationScenarios.Context.Refusing();
        await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.ContextIdle);
        await context.Controller.ExecuteAsync();
        Assert.Equal(1, context.Confirmations); Assert.Equal(0, context.Calls);
        Assert.False(context.Operation.IsRunning); Assert.Equal("virtual-source.img", context.Model.Write.SourcePath);
    }
    public static void Options()
    {
        var model=new WriteOperationViewModel();
        model.ApplyOptions(new HashSet<string>{"no-verify","retries"},new Dictionary<string,string>{{"retries","4"},{"expert","--raw"}});
        Assert.True(model.DisableVerification);
        var option=Assert.Single(model.BuildOptions());
        Assert.Equal("--retries",option.Argument);Assert.Equal("4",option.Value);
        Assert.Contains("no-verify",model.CaptureEnabledOptions());
        Assert.Equal("--raw",model.CaptureValues()["expert"]);
        model.EnableHardSectors();model.EnableFakeIndex();
        Assert.False(model.HardSectors.Enabled);Assert.True(model.FakeIndex.Enabled);
        model.EnableTg43();model.EnableDensel();
        Assert.False(model.Tg43.Enabled);Assert.True(model.Densel.Enabled);
    }
}
