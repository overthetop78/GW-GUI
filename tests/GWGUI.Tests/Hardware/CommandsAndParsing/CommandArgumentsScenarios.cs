using GWGUI.Domain.Read;
using GWGUI.Domain.Write;
using GWGUI.Domain.Maintenance;
namespace GWGUI.Tests.Hardware.CommandsAndParsing;
internal static class CommandArgumentsScenarios
{
    public static void Options(string argument,string? value,bool valid)
    {
        GWGUI.Domain.Commands.Options.EnabledOption[] options=[new(argument,value)];
        GWGUI.Domain.Commands.GwCommand Build()=>ReadCommandBuilder.Build(new("virtual","virtual folder/a\"b.scp",ReadResultKind.RawScp,null,options));
        if(!valid) { Assert.ThrowsAny<ArgumentException>(Build); return; }
        var command=Build(); Assert.Equal("virtual folder/a\"b.scp",command.Arguments[^1]); Assert.Equal(argument,command.Arguments[0]);
        Assert.Equal(value is null?2:3,command.Arguments.Count); if(value is not null) Assert.Equal(value,command.Arguments[1]);
    }
    public static void ExclusiveAndExpert()
    {
        foreach(var pair in new[]{new[]{new GWGUI.Domain.Commands.Options.EnabledOption("--fake-index","300rpm"),new("--hard-sectors")},new[]{new GWGUI.Domain.Commands.Options.EnabledOption("--densel","H"),new("--gen-tg43")}})
            Assert.Throws<ArgumentException>(()=>ReadCommandBuilder.Build(new("virtual","out",ReadResultKind.RawScp,null,pair)));
        Assert.Throws<ArgumentException>(()=>ReadCommandBuilder.Build(new("virtual","out",ReadResultKind.RawScp,null,[],ExpertArguments:"--tracks \"unclosed")));
        var explicitDefinitions=ReadCommandBuilder.Build(new("virtual","out",ReadResultKind.KnownFormat,"amstrad.cpc",[new("--diskdefs","virtual config")]));
        Assert.Equal(new[]{"--format","amstrad.cpc","--diskdefs","virtual config","out"},explicitDefinitions.Arguments);
        Assert.Equal(new[]{"--one","two words","--path",@"virtual\path"},GWGUI.Domain.Commands.CommandLineTokenizer.Tokenize("  --one \"two words\"\t--path virtual\\path  "));
    }
    public static void Read()
    {
        var command=ReadCommandBuilder.Build(new("virtual-tool","virtual folder/disk.scp",ReadResultKind.RawScp,null,[],Device:"device-1",Drive:"B",ExpertArguments:"--tracks \"c=0-1:h=0\""));
        Assert.Equal("virtual-tool",command.ExecutablePath);
        Assert.Equal("read",command.Verb);
        Assert.Equal(new[]{"--device","device-1","--drive","B","--tracks","c=0-1:h=0","virtual folder/disk.scp"},command.Arguments);
    }
    public static void Write()
    {
        var command=WriteCommandBuilder.Build(new("virtual-tool","virtual folder/disk.img",null,[],true,"device-2","A"));
        Assert.Equal("write",command.Verb);
        Assert.Equal(new[]{"--device","device-2","--drive","A","--no-verify","virtual folder/disk.img"},command.Arguments);
    }
    public static void Invalid()
    {
        Assert.Throws<ArgumentException>(()=>ReadCommandBuilder.Build(new("virtual","",ReadResultKind.RawScp,null,[])));
        Assert.Throws<ArgumentException>(()=>ReadCommandBuilder.Build(new("virtual","out",ReadResultKind.KnownFormat,null,[])));
        Assert.Throws<ArgumentException>(()=>WriteCommandBuilder.Build(new("virtual","",null,[])));
        Assert.Throws<ArgumentException>(()=>ToolCommandBuilder.Build(new("virtual","unknown",new Dictionary<string,string>(),new HashSet<string>())));
    }
    public static void Tools()
    {
        var pin=ToolCommandBuilder.Build(new("virtual","pin",new Dictionary<string,string>{{"pin","8"}},new HashSet<string>{"set","high"},Device:"controller"));
        Assert.Equal(new[]{"set","8","H","--device","controller"},pin.Arguments);
        var seek=ToolCommandBuilder.Build(new("virtual","seek",new Dictionary<string,string>{{"cylinder","12"}},new HashSet<string>{"force"},"controller","B"));
        Assert.Equal(new[]{"12","--force","--device","controller","--drive","B"},seek.Arguments);
        Assert.Throws<ArgumentOutOfRangeException>(()=>ToolCommandBuilder.Build(new("virtual","pin",new Dictionary<string,string>{{"pin","9"}},new HashSet<string>())));
    }
}
