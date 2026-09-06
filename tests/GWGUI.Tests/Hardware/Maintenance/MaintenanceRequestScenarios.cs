using GWGUI.App.Views.Windows.Tools;
using GWGUI.App.Localization.Extensions;
using GWGUI.Domain.Commands;
using GWGUI.Domain.Commands.Execution;
using GWGUI.Tests.Application.TestInfrastructure;
using System.Windows.Controls;
namespace GWGUI.Tests.Hardware.Maintenance;
internal static class MaintenanceRequestScenarios
{
    public static void Delay(string key)
    {
        var values=new Dictionary<string,string>{{key,"0"}};
        var command=GWGUI.Domain.Maintenance.ToolCommandBuilder.Build(new("virtual","delays",values,new HashSet<string>{key},"controller","B"));
        Assert.Equal(new[]{"--"+key,"0","--device","controller"},command.Arguments);
        values[key]="-1";
        Assert.ThrowsAny<ArgumentException>(()=>GWGUI.Domain.Maintenance.ToolCommandBuilder.Build(new("virtual","delays",values,new HashSet<string>{key})));
        Assert.Empty(GWGUI.Domain.Maintenance.ToolCommandBuilder.Build(new("virtual","delays",values,new HashSet<string>())).Arguments);
    }
    public static void Alignment()
    {
        var values=new Dictionary<string,string>{{"tracks","c=2-4:h=1"},{"revs","1"},{"reads","2"},{"format","ibm.720"},{"pll","period=5:phase=60"}};
        var enabled=new HashSet<string>{"format","pll","raw","reverse"};
        var command=GWGUI.Domain.Maintenance.ToolCommandBuilder.Build(new("virtual","align",values,enabled,"controller","B"));
        Assert.Equal(new[]{"--tracks","c=2-4:h=1","--revs","1","--reads","2","--format","ibm.720","--pll","period=5:phase=60","--raw","--reverse","--device","controller","--drive","B"},command.Arguments);
        values["reads"]="0"; Assert.ThrowsAny<ArgumentException>(()=>GWGUI.Domain.Maintenance.ToolCommandBuilder.Build(new("virtual","align",values,enabled)));
    }
    internal static GwToolWindow Window(string verb, Func<GwCommand, GwExecutionResult> run, List<Exception>? errors = null) => new("virtual-tool", verb, "controller", "B",
        ControlledDependencies.Simulate<IGreaseweazleRunner>((method, args) => method.Name switch {
            "get_IsRunning" => false,
            "RunAsync" => Task.FromResult(run(Assert.IsType<GwCommand>(args[0]))),
            _ => throw new InvalidOperationException(method.Name)
        }), confirm: (_, _) => throw new InvalidOperationException("Unexpected confirmation"), logError: (error, _) => { if (errors is null) throw new InvalidOperationException("Unexpected diagnostic error", error); errors.Add(error); });
    public static void Defaults(string verb, string[] expected)
    {
        var calls = 0; var window = Window(verb, command => { calls++; Assert.Equal("virtual-tool", command.ExecutablePath); Assert.Equal(verb, command.Verb); Assert.Equal(expected, command.Arguments); return new(0, false, TimeSpan.Zero, []); });
        window.ExecuteAsync().GetAwaiter().GetResult(); Assert.Equal(1, calls);
    }
    public static void Invalid(string verb)
    {
        var calls = 0; var window = Window(verb, _ => { calls++; throw new InvalidOperationException(); });
        var panel = Assert.IsType<WrapPanel>(window.FindName("ParametersPanel"));
        var field = panel.Children.OfType<StackPanel>().SelectMany(x => x.Children.OfType<TextBox>()).First();
        field.Text = verb == "pin" ? "7" : "-1";
        Assert.False(Assert.IsType<Button>(window.FindName("ExecuteButton")).IsEnabled);
        Assert.Equal(LocExtension.Get("Tool.InvalidParameters"), Assert.IsType<TextBox>(window.FindName("CommandText")).Text); Assert.Equal(0, calls);
        field.Text = verb == "pin" ? "8" : "1"; Assert.True(Assert.IsType<Button>(window.FindName("ExecuteButton")).IsEnabled);
    }
}
